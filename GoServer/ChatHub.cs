using GoServer;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.ConstrainedExecution;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace SignalRChat.Hubs
{
  public class ChatHub : Hub
  {
    public override Task OnConnectedAsync()
    {
      Debug.WriteLine($">>>Client connected: {Context.ConnectionId}");
      return base.OnConnectedAsync();
    }
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
      Debug.Write($">>>Client disconnected: {Context.ConnectionId} ");
      // remove user from list
      // if they are opponent to anybdy. tell that anybody and clean up
      var Users = Gls.users;
      string opponent = "";
      User? testUser = null;   // ? tells compiler that testUser might be null.
      lock (Gls.usersLock)
      {
        // need lock here in case multiple instances adding same name ...
        testUser = Users.Find(x => x.ConnectionId == Context.ConnectionId);
        if (testUser == null)
        {
          Debug.WriteLine("Player not found" );
        }
        else
        {
          Debug.WriteLine("removing " + testUser.Name);
          opponent = testUser.Opponent;
          Users.Remove(testUser);   // remove user from list
          if (opponent != "")
          {
            // opponent is busy, so we need to tell them (after lock ended) that their opponent has departed
            // and clean up their opponent too
            User? testOpponent = Users.Find(x => x.Name.ToLower() == opponent.ToLower());
            if (testOpponent != null)
            {
              testOpponent.Opponent = "";   // signals that testOpponent is no longer busy
            }
            else
            {
              Debug.WriteLine(">>> Opponent not found: " + opponent);
            }
          }
        }
      }
      if (opponent != "")
      {
        await SendPlayer(opponent, "OpponentDeparted");
      }
      await base.OnDisconnectedAsync(exception);
    }
    public async Task CheckName(string playerName)
    {
      // check playerName not in use. If not, add to users
      var Users = Gls.users;
      User? testUser = null;   // ? tells compiler that testUser might be null.
      lock (Gls.usersLock)
      {
        // need lock here in case multiple instances adding same name ...
        Debug.WriteLine(">>>" + playerName + " CheckName locked");
        string lowerName = playerName.ToLower();
        testUser = Users.Find(x => x.Name.ToLower() == lowerName);
        if (testUser == null)
        {
          Users.Add(new User(playerName, Context.ConnectionId));
          Debug.WriteLine(">>>" + playerName + " Num users = " + Users.Count + ". Unlocking");
        }
        else
        {
          Debug.WriteLine(">>>" + playerName + " Duplicate name. Unlocking");
        }
      }
      if (testUser == null)
      {
        await Clients.Caller.SendAsync("NameOK");
      }
      else
      {
        await Clients.Caller.SendAsync("NameHadError");
      }
    }
    public async Task Challenge(string playerName, string opponentName, 
      string hours, string minutes, bool undosAllowed, string playerColour, string pauses)
    {
      // player is issuing challenge to opponent
      // 1. check opponent exists and is not already playing
      // 2. if opponent is not playing or being challenged, send them a message to accept or decline
      // 3. Communicate result back to player
      var Users = Gls.users;
      User? testOpponent = null;   // ? tells compiler that testUser might be null.
      lock (Gls.usersLock)
      {
        string lowerName = opponentName.ToLower();
        testOpponent = Users.Find(x => (x.Name.ToLower() == lowerName) && (x.Opponent == ""));
        if (testOpponent != null)
        {
          testOpponent.Opponent = playerName;   // signals that testOpponent being challenged
        }
      }
      if (testOpponent == null)
      {
        await Clients.Caller.SendAsync("OpponentUnavailable");
        Debug.WriteLine(">>>" + opponentName + " No such user / busy");
      }
      else
      {
        await Clients.Caller.SendAsync("OpponentThinking");
        await Clients.Client(testOpponent.ConnectionId).SendAsync("ChallengeIn", 
          playerName, hours, minutes, undosAllowed, playerColour, pauses);
      }
    }
    public async Task AcceptChallenge(string opponentName, string playerName)
    {
      // opponent has accepted player's challenge
      // playerName, opponentName are same values as in Challenge()
      // set playerName's opponent = opponentName
      var Users = Gls.users;
      User? testPlayer = null; 
      lock (Gls.usersLock)
      {
        string lowerName = playerName.ToLower();
        testPlayer = Users.Find(x => (x.Name.ToLower() == lowerName));
        if (testPlayer == null)
        {
          User? testOpponent = null;
          string lowerNameOpp = opponentName.ToLower();
          testOpponent = Users.Find(x => (x.Name.ToLower() == lowerNameOpp));
          if (testOpponent != null)
          {
            // catastrophe
            testOpponent.Opponent = "";  // opponent is no longer busy
          }   
        }
        else
        {
          testPlayer.Opponent = opponentName;   // signals that game is started
        }
      }
      if (testPlayer == null)
      {
        await Clients.Caller.SendAsync("OpponentDeparted");   // catastrophe
      }
      else
      {
        await Clients.Client(testPlayer.ConnectionId).SendAsync("ChallengeAccepted");
      }
    }
    public async Task DeclineChallenge(string opponentName, string playerName)
    {
      // opponent has declined player's challenge
      // playerName, opponentName are same values as in Challenge()
      // set playerName's opponent = "" (not busy)
      var Users = Gls.users;
      User? testOpponent = null; 
      lock (Gls.usersLock)
      {
        string lowerName = opponentName.ToLower();
        testOpponent = Users.Find(x => (x.Name.ToLower() == lowerName));
        if (testOpponent != null)
        {
          testOpponent.Opponent = "";   // signals that testOpponent is free
        }
      }
      await SendPlayer(playerName, "ChallengeDeclined");
    }
    private async Task SendPlayer (string playerName, string message)
    {
      // send message to player
      var Users = Gls.users;
      string lowerName2 = playerName.ToLower();
      User? testPlayer = Users.Find(x => (x.Name.ToLower() == lowerName2));
      if (testPlayer != null)
      {
        Debug.WriteLine(">>> " + message + " sent to " + testPlayer.Name);
        await Clients.Client(testPlayer.ConnectionId).SendAsync(message);
      }
    }
    public async Task GameStart(string player1, string player2)
    {
      // send message to player2 that game is starting
      var Users = Gls.users;
      string lowerName2 = player2.ToLower();
      User? testPlayer2 = Users.Find(x => (x.Name.ToLower() == lowerName2));
      if (testPlayer2 != null)
      {
        Debug.WriteLine(">>> GameStart: sent to " + testPlayer2.Name);
        LogUsers();
        await Clients.Client(testPlayer2.ConnectionId).SendAsync("GameStarted");
        await LogSomething("Game started between " + player1 + " and " + player2);
      }

    }
    public async Task ChangeColor(string player2, string newColor)
    {
      // send message to player2 that opponent has changed color
      var Users = Gls.users;
      string lowerName2 = player2.ToLower();
      User? testPlayer2 = Users.Find(x => (x.Name.ToLower() == lowerName2));
      if (testPlayer2 != null)
      {
        await Clients.Client(testPlayer2.ConnectionId).SendAsync("ColorChanged", newColor);
      }
    }
    public async Task ChangeHours(string player2, string newHours)
    {
      // send message to player2 that opponent has changed hours
      var Users = Gls.users;
      string lowerName2 = player2.ToLower();
      User? testPlayer2 = Users.Find(x => (x.Name.ToLower() == lowerName2));
      if (testPlayer2 != null)
      {
        await Clients.Client(testPlayer2.ConnectionId).SendAsync("HoursChanged", newHours);
      }
    }
    public async Task ChangeMinutes(string player2, string newMinutes)
    {
      // send message to player2 that opponent has changed minutes
      var Users = Gls.users;
      string lowerName2 = player2.ToLower();
      User? testPlayer2 = Users.Find(x => (x.Name.ToLower() == lowerName2));
      if (testPlayer2 != null)
      {
        await Clients.Client(testPlayer2.ConnectionId).SendAsync("MinutesChanged", newMinutes);
      }
    }
    public async Task ChangeUndo(string player2, bool undosAllowed)
    {
      // send message to player2 that opponent has changed undosAllowed
      var Users = Gls.users;
      string lowerName2 = player2.ToLower();
      User? testPlayer2 = Users.Find(x => (x.Name.ToLower() == lowerName2));
      if (testPlayer2 != null)
      {
        await Clients.Client(testPlayer2.ConnectionId).SendAsync("UndoChanged", undosAllowed);
      }
    }
    public async Task PausesChanged(string player2, string newPauses)
    {
      // send message to player2 that opponent has changed minutes
      var Users = Gls.users;
      string lowerName2 = player2.ToLower();
      User? testPlayer2 = Users.Find(x => (x.Name.ToLower() == lowerName2));
      if (testPlayer2 != null)
      {
        await Clients.Client(testPlayer2.ConnectionId).SendAsync("PausesChanged", newPauses);
      }
    }
    public async Task MakeMove(int boardX, int boardY)
    {
      // send message to player2 that game is over
      string OpponentID = GetOtherConnectionID(Context.ConnectionId);
      if (OpponentID != "")
      {
        await Clients.Client(OpponentID).SendAsync("MakeMove" , boardX, boardY);
      }
    }
    public async Task RequestUndo()
    {
      string OpponentID = GetOtherConnectionID(Context.ConnectionId);
      if (OpponentID != "")
      {
        await Clients.Client(OpponentID).SendAsync("RequestUndo");
      }
    }
    public async Task UndoGranted()
    {
      string OpponentID = GetOtherConnectionID(Context.ConnectionId);
      if (OpponentID != "")
      {
        await Clients.Client(OpponentID).SendAsync("UndoGranted");
      }
    }
    public async Task UndoDenied()
    {
      string OpponentID = GetOtherConnectionID(Context.ConnectionId);
      if (OpponentID != "")
      {
        await Clients.Client(OpponentID).SendAsync("UndoDenied");
      }
    }
    public async Task Pass()
    {
      string OpponentID = GetOtherConnectionID(Context.ConnectionId);
      if (OpponentID != "")
      {
        await Clients.Client(OpponentID).SendAsync("Pass");
      }
    }
    public async Task EndGameKillUsers(string result)
    {
      // now clean up both players. Should not get called twice in rapid succession
      // at end of game. 
      var Users = Gls.users;
      User? testPlayer = null; 
      User? testOpponent = null; 
      Debug.WriteLine(">>> EndGameKillUsers: " + result + " Users " + Users.Count);
      lock (Gls.usersLock)
      {
        string OpponentID = GetOtherConnectionID(Context.ConnectionId);
        testPlayer = Users.Find(x => x.ConnectionId == Context.ConnectionId);
        if (testPlayer != null)
        {
          Users.Remove(testPlayer);   // remove user from list
        }
        if (OpponentID != "")
        {
          testOpponent = Users.Find(x => x.ConnectionId == OpponentID);
          if (testOpponent != null)
          {
            Users.Remove(testOpponent);   // remove user from list
          }
        }
      }
      Debug.WriteLine(">>> EndGameKillUsers2: " + result + " Users " + Users.Count);
      string logMessage = "Game ended between ";
      if (testPlayer != null)
      {
        logMessage += testPlayer.Name;
      }
      else
      {
        logMessage += "unknown";
      }
      logMessage += " and ";
      if (testOpponent != null)
      {
        logMessage += testOpponent.Name;
      }
      else
      {
        logMessage += "unknown";
      }
      await LogSomething(logMessage + ". " + result);
    }
    public async Task TickTock(string timeString)
    {
      string OpponentID = GetOtherConnectionID(Context.ConnectionId);
      if (OpponentID != "")
      {
        await Clients.Client(OpponentID).SendAsync("TickTock", timeString);
      }
    }
    public async Task OutOfTime()
    {
      // send message to both players that game is over
      string OpponentID = GetOtherConnectionID(Context.ConnectionId);
      if (OpponentID != "")
      {
        await Clients.Client(OpponentID).SendAsync("OpponentOutOfTime");
      }
      await Clients.Caller.SendAsync("YouOutOfTime");
    }
    public async Task Resign()
    {
      // send message to both players that game is over
      string OpponentID = GetOtherConnectionID(Context.ConnectionId);
      if (OpponentID != "")
      {
        await Clients.Client(OpponentID).SendAsync("OpponentResigned");
      }
      await Clients.Caller.SendAsync("YouResigned");
    }
    public async Task Resume()
    {
      // send message to other players that game resumed
      string OpponentID = GetOtherConnectionID(Context.ConnectionId);
      if (OpponentID != "")
      {
        await Clients.Client(OpponentID).SendAsync("Resume");
      }
    }
    public async Task RequestPause()
    {
      // send message to other players that game resumed
      string OpponentID = GetOtherConnectionID(Context.ConnectionId);
      if (OpponentID != "")
      {
        await Clients.Client(OpponentID).SendAsync("RequestPause");
      }
    }
    public async Task PauseDenied()
    {
      // send message to other players that game resumed
      string OpponentID = GetOtherConnectionID(Context.ConnectionId);
      if (OpponentID != "")
      {
        await Clients.Client(OpponentID).SendAsync("PauseDenied");
      }
    }
    public async Task PauseGranted()
    {
      // send message to other players that game resumed
      string OpponentID = GetOtherConnectionID(Context.ConnectionId);
      if (OpponentID != "")
      {
        await Clients.Client(OpponentID).SendAsync("PauseGranted");
      }
    }

    /////////////////////////////////////////////////////////////////////
    // Utility and test functions
    /////////////////////////////////////////////////////////////////////
    private string GetOtherConnectionID(string connectionId1)
    {
      var Users = Gls.users;
      User? fromPlayer = Users.Find(x => x.ConnectionId == connectionId1);
      if (fromPlayer == null)
      {
        Debug.WriteLine(">>> GetOtherConnectionID: player not found");
        LogUsers();
        return "";
      }
      string player2 = fromPlayer.Opponent;
      string lowerName2 = player2.ToLower();
      User? testPlayer2 = Users.Find(x => (x.Name.ToLower() == lowerName2));
      if (testPlayer2 != null)
      {
        return testPlayer2.ConnectionId;
      }
      return "";
    }
    private void LogUsers()
    {
      // log all users to console 
      var Users = Gls.users;
      Debug.WriteLine(">>> Users: " + Users.Count);
      foreach (var user in Users)
      {
        Debug.WriteLine("    " + user.Name +":" + user.ConnectionId + " Opponent: " + user.Opponent);
      }
    }
    public async Task LogSomething(string message)
    {
      // log message to repos\Go\logs\go.log in debug or
      // /logs/go.log on server
      try
      {
        using StreamWriter outputFile = new StreamWriter("../logs/go.log", true);
        {
          DateTime localDate = DateTime.Now;
          string showTime = localDate.ToString("yy-MM-dd HH:mm:ss");
          if (message != "")
          {
            await outputFile.WriteAsync(Environment.NewLine + showTime + " " + message);
          }
          else
          {
            await outputFile.WriteAsync(Environment.NewLine);
          }
        }
      }
      catch (Exception e)
      {
        Debug.WriteLine(">>> Error in LogMessage: " + e.ToString());
      }
    }
    public async Task Ping(string message)
    {
      var cID = Context.ConnectionId;
      Debug.WriteLine(">>> ping " + message);
      await Clients.Caller.SendAsync("pingBack", cID);           // that works
      // await Clients.Client(cID).SendAsync("pingBack", cID);   // that also works
    }

    // Test task: to be deleted
    public async Task TellMeGroups()
    {
      var cID = Context.ConnectionId;
      Debug.WriteLine(">>> TellMeGroups");
      string[] fakeGroups = ["There are no", "games", "on this server. Ha!"];
      await Clients.Caller.SendAsync("GroupList", fakeGroups); 
    }
  }
}
