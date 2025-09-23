using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Interop;
using Windows.Media.Playback;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Tab;

namespace GoPlanner
{
  public partial class GoPlanner : Form
  {
    // game variables
    private bool gameInProgress = false;
    private bool playersTurn;
    private string playerName;
    private byte playerColor;
    private int playerTimeLeft;
    private int minuteReminder;     // remind player if they have been thinking for a while 
    private Timer playerTimer;
    private bool allowUndos = true;
    private int passCount = 0;
    HubConnection connection;
    public Func<Exception, Task> closedHandler;

    private void TSBsetupStart_Click(object sender, EventArgs e)
    {
      StartGameToolStripMenuItem_Click(sender, e);
    }
    private void StartGameToolStripMenuItem_Click(object sender, EventArgs e)
    {
      if (!CheckSafety("Game / Start")) { return; }
      GameSetUp gameSetUp = new GameSetUp(this);
      if (gameSetUp.ShowDialog() == DialogResult.Cancel) { 
        gameSetUp.Dispose();
        return; 
      }
      // game has started, black is playing
      ClearBoard(gameSetUp.PlayerName.Text + " plays " + gameSetUp.OpponentName.Text);

      statusM.Set("Game started. Black to play.");
      connection = gameSetUp.connection;
      playerName = gameSetUp.PlayerName.Text;
      if (gameSetUp.playerColor == "W")
      {
        playerColor = 1;    // same as Astone.color
        YouColor.Image = Properties.Resources.StoneWhite;
        OpponentColor.Image = Properties.Resources.StoneBlack;
      }
      else
      {
        playerColor = 2;
        YouColor.Image = Properties.Resources.StoneBlack;
        OpponentColor.Image = Properties.Resources.StoneWhite;
      }
      int hours = gameSetUp.PlayHours.Text == "" ? 0 : int.Parse(gameSetUp.PlayHours.Text);
      playerTimeLeft = hours * 3600 + int.Parse(gameSetUp.PlayMinutes.Text) * 60;
      minuteReminder = playerTimeLeft;
      playerTimer = new Timer();
      playerTimer.Interval = 1000; // 1 second
      playerTimer.Tick += new EventHandler(ProcessTick);
      YouMainTime.Text = TimeSpan.FromSeconds(playerTimeLeft).ToString(@"hh\:mm\:ss");
      OpponentMainTime.Text = YouMainTime.Text;

      OpponentName.Text = gameSetUp.OpponentName.Text + ":";
      allowUndos = gameSetUp.AllowUndos.Checked;
      gameInProgress = true;
      CalcTSTextBox2Width();
      TSBblackWhite.Checked = false;
      if (playerColor == 2)
      {
        statusM.Set("Game started. You to play.");
        toolsOptions.OptionalBeep();
        playersTurn = true;
        panelMain.Cursor = blackCursor;
        playerTimer.Start();
        playerTimer.Enabled = true;
        TSBblack.Checked = true;    // set these so htat undo does not change cursor color
        TSBwhite.Checked = false;
        playerBlack = playerName;
        playerWhite = GetOpponentName();
      }
      else
      {
        statusM.Set("Game started. " + GetOpponentName() + " playing.");
        playersTurn = false;
        panelMain.Cursor = whiteCursor;
        playerTimer.Start();
        playerTimer.Enabled = false;
        TSBblack.Checked = false;    // set these so that undo does not change cursor color
        TSBwhite.Checked = true;
        playerBlack = GetOpponentName();
        playerWhite = playerName;
      }
      EnableDisableBoard();
      passCount = 0;
      // every handler added is removed in EndGame 
      connection.On("OpponentDeparted", OpponentDepartedInGame);
      connection.On<int, int>("MakeMove", MakeMove);
      connection.On("UndoGranted", UndoGranted);
      connection.On("UndoDenied", UndoDenied);
      connection.On("RequestUndo", RequestUndo);
      connection.On<string>("TickTock", TickTock);
      connection.On("Pass", Pass);
      connection.On("OpponentResigned", OpponentResigned);
      connection.On("YouResigned", YouResigned);
      connection.On("OpponentOutOfTime", OpponentOutOfTime);
      connection.On("YouOutOfTime", YouOutOfTime);
      gameSetUp.Dispose();
    }
    private string GetOpponentName()
    {
      return OpponentName.Text.Remove(OpponentName.Text.Length - 1);
    }
    private void EnableDisableBoard()
    {
      // during game, most menu items etc are disabled
      openToolStripMenuItem.Enabled = !gameInProgress;
      openToolStripButton.Enabled = !gameInProgress;
      propertiesToolStripMenuItem.Enabled = !gameInProgress;
      EnableDoControls();
      if (gameInProgress)
      {
        EnableCutCopy(false);   // too bad, if selection rectangle going
      }
      CheckClipboardGo();   // looks after paste
      DeleteToolStripMenuItem.Enabled = !gameInProgress;
      StartGameToolStripMenuItem.Enabled = !gameInProgress;
      TSBsetupStart.Enabled = !gameInProgress;
      TSBwhite.Enabled = !gameInProgress;
      TSBblack.Enabled = !gameInProgress;
      TSBblackWhite.Enabled = !gameInProgress;
      TSBnone.Enabled = !gameInProgress;

      TSBpass.Enabled = gameInProgress && playersTurn;
      PassToolStripMenuItem.Enabled = TSBpass.Enabled;
      ResignToolStripMenuItem.Enabled = TSBpass.Enabled;
      TSBresign.Enabled = TSBpass.Enabled;
    }
    private async void MouseUpInGame(int boardX, int boardY)
    {
      // similar to parts of PanelMain_MouseUp
      if (!playersTurn)
      {
        statusM.Set("Not your turn");
        return;
      }
      if (thePoints[boardX, boardY].color != 0)
      {
        statusM.Set("Illegal move - point occupied");
        return;
      }
      SaveState("Mouse Up");
      thePoints[boardX, boardY].color = playerColor;
      if (!ProcessMove(boardX, boardY, playerColor)) return;  // ProcessMove does UnSaveState if false
      EndMove(boardX, boardY, GetOpponentName() + " to move");
      SetPlayer(false);
      passCount = 0;
      await SafeInvokeAsync("MakeMove", boardX, boardY);
    }
    private async Task SafeInvokeAsync(string methodName, params object[] args)
    {
      // in spite of everything this can still get unhandled exception
      // at end of game so added try catch
      try
      {
        if (connection != null && connection.State == HubConnectionState.Connected)
        {
          switch (args.Length)
          {
            case 0:
              await connection.InvokeAsync(methodName);
              break;
            case 1:
              await connection.InvokeAsync(methodName, args[0]);
              break;
            case 2:
              await connection.InvokeAsync(methodName, args[0], args[1]);
              break;
            case 3:
              await connection.InvokeAsync(methodName, args[0], args[1], args[2]);
              break;
            // Add more cases as needed
            default:
              Console.WriteLine("Too many arguments for SafeInvokeAsync");
              break;
          }
        }
        else
        {
          Console.WriteLine($"Cannot invoke '{methodName}': connection not active.");
        }
      }
      catch (Exception ex)
      {
        Console.WriteLine("Exception in SafeInvokeAsync: " + ex.Message);
        return;
      }
    }
    private void SetPlayer (bool playing)
    {
      if (playersTurn == false && playing == true)
      {
        toolsOptions.OptionalBeep();
      }
      playersTurn = playing;
      playerTimer.Enabled = playing;
      minuteReminder = playerTimeLeft;
      TSBpass.Enabled = playing;
      PassToolStripMenuItem.Enabled = playing;
      ResignToolStripMenuItem.Enabled = playing;
      TSBresign.Enabled = playing;
      EnableDoControls();
    }
    private void MakeMove(int boardX, int boardY)
    {
      // opponent has moved
      if (InvokeRequired)
      {
        Invoke((Action)(() => MakeMove(boardX, boardY)));
        return;
      }
      SaveState("Mouse Up");
      byte oppColor = (byte)(3 - playerColor);
      thePoints[boardX, boardY].color = oppColor;
      ProcessMove(boardX, boardY, oppColor);
      EndMove(boardX, boardY, "Your move");
      passCount = 0;
      SetPlayer(true);
    }
    private async void GameUndo()
    {
      // if undos allowed, then ask opponent if ok
      // if not allowed should never get here as menu items disabled
      if (InvokeRequired) { Invoke((Action)(() => GameUndo())); return; }
      if (passCount > 0)
      {
        statusM.Set("Undo not allowed after Pass.");
        return;
      }
      statusM.Set("Undo requested. Timers suspended.");
      await SafeInvokeAsync("RequestUndo");
    }
    private async void RequestUndo()
    {
      if (InvokeRequired) { Invoke((Action)(() => RequestUndo())); return; }
      int reply = 0;
      playerTimer.Enabled = false;
      new MyMessageBox(
        "Your opponent requests an undo. Do you agree?\r\nTimer is suspended while you think.",
        "Undo request",
        ref reply, "Yes", "No", "", this);
      if (reply == 3)
      {
        // it's a yes
        UndoForReal();
        statusM.Set("Undo granted. Opponent playing again.");
        SetPlayer(false);
        await SafeInvokeAsync("UndoGranted");
      }
      else
      {
        statusM.Set("Your turn continues");
        playerTimer.Enabled = true;
        await SafeInvokeAsync("UndoDenied");
      }
    }
    private void UndoGranted()
    {
      if (InvokeRequired) { Invoke((Action)(() => UndoGranted())); return; }
      UndoForReal();
      statusM.Set("Undo granted. You may try again.");
      SetPlayer(true);
    }
    private void UndoDenied()
    {
      if (InvokeRequired) { Invoke((Action)(() => UndoDenied())); return; }
      statusM.Set("Bad luck! Undo denied. Your move stands.");
    }
    private async void ProcessTick(object sender, EventArgs e)
    {
      // called by timer event every second
      if (playerTimeLeft > 0) playerTimeLeft--;
      YouMainTime.Text = TimeSpan.FromSeconds(playerTimeLeft).ToString(@"hh\:mm\:ss");
      await SafeInvokeAsync("TickTock", YouMainTime.Text);
      if (playerTimeLeft <= 0)    // <0 should be impossible
      {
        playerTimer.Stop();
        await SafeInvokeAsync("OutOfTime");
        return;
      }
      if (playerTimeLeft == 60)
      {
        statusM.Set("Less than 1 minute left");
        toolsOptions.OptionalBeep();
        return;
      }
      if (playerTimeLeft == 10)
      {
        statusM.Set("Less than 10 seconds left");
        toolsOptions.OptionalBeep();
        return;
      }
      if (minuteReminder - playerTimeLeft > 60)
      {
        minuteReminder = playerTimeLeft;
        statusM.Set("Still your move");
        toolsOptions.OptionalBeep();
        return;
      }
    }
    private void TickTock(string time)
    {
      if (InvokeRequired) { Invoke((Action)(() => TickTock(time))); return; }
      OpponentMainTime.Text = time;
    }
    private async void OpponentOutOfTime()
    {
      if (InvokeRequired) { Invoke((Action)(() => OpponentOutOfTime())); return; }
      string result = GetOpponentName() + " out of time, " + playerName + " wins";
      await SafeInvokeAsync("EndGameKillUsers", result);
      EndGame(GetOpponentName() + " out of time. You win!", result);
    }
    private void YouOutOfTime()
    {
      if (InvokeRequired) { Invoke((Action)(() => YouOutOfTime())); return; }
      string msg = "You are out of time. " + GetOpponentName() + " wins.";
      EndGame(msg, playerName + " out of time, " + GetOpponentName() + " wins");
    }
    private void PassToolStripMenuItem_Click(object sender, EventArgs e)
    {
      TSBpass_Click(sender, e);
    }
    private async void TSBpass_Click(object sender, EventArgs e)
    {
      if (!gameInProgress || !playersTurn) return;
      passCount++;
      SetPlayer(false);
      if (passCount == 1)
      {
        await SafeInvokeAsync("Pass");
        statusM.Set("You passed. Opponent's turn.");
      }
      else
      {
        // passCount must be 2
        await SafeInvokeAsync("Pass");
        EndGame(playerName + " and " + GetOpponentName() + " passed. Game over.", "Both players passed");
      }
    }
    private async void Pass()
    {
      // opponent has passed
      if (InvokeRequired) { Invoke((Action)(() => Pass())); return; }
      passCount++;
      if (passCount == 1)
      {
        statusM.Set("Opponent passed. Your turn. Game ends if you pass.");
        SetPlayer(true);
      }
      else
      {
        // passCount must be 2
        string result = "Both players passed";
        await SafeInvokeAsync("EndGameKillUsers", result);
        EndGame(playerName + " and " + GetOpponentName() + " passed. Game over.", result);
      }
    }
    private async void EndGame(string displayMessage, string result) 
    {       
      gameInProgress = false;
      playerTimer.Stop();
      playerTimer.Enabled = false;
      playerTimer.Dispose();
      statusM.Set(displayMessage);
      EnableDisableBoard();
      gameResult = result;
      new MyMessageBox(displayMessage, "Game over", this);
      try
      {
        connection.Remove("OpponentDeparted");
        connection.Remove("MakeMove");
        connection.Remove("UndoGranted");
        connection.Remove("UndoDenied");
        connection.Remove("RequestUndo");
        connection.Remove("TickTock");
        connection.Remove("Pass");
        connection.Remove("OpponentResigned");
        connection.Remove("YouResigned");
        connection.Remove("OpponentOutOfTime");
        connection.Remove("YouOutOfTime");

        connection.Closed -= closedHandler;
        await connection.StopAsync();     // this produces ton of exceptions in output debug window
                                          // CoPilot says not to worry!
        await connection.DisposeAsync();
      }
      catch (Exception ex)
      {
        Console.WriteLine("Exception in EndGame: " + ex.Message);
      }
    }
    private async void OpponentDepartedInGame()
    {
      if (!gameInProgress) 
        return;      // happens after game ended normally. So nothing to do
      if (InvokeRequired) { Invoke((Action)OpponentDepartedInGame); return; }
      string result = GetOpponentName() + " departed, " + playerName + " wins";
      await SafeInvokeAsync("EndGameKillUsers", result);
      EndGame(GetOpponentName() + " has departed. You win.", result);
    }
    private void ResignToolStripMenuItem_Click(object sender, EventArgs e)
    {
      TSBresign_Click(sender, e);
    }

    private async void TSBresign_Click(object sender, EventArgs e)
    {
      await SafeInvokeAsync("Resign");
    }
    private async void OpponentResigned()
    {
      if (InvokeRequired) { Invoke((Action)(() => OpponentResigned())); return; }
      string result = GetOpponentName() + " resigned, " + playerName + " wins";
      await SafeInvokeAsync("EndGameKillUsers", result);
      EndGame(GetOpponentName() + " resigned. You win!", result);
    }
    private void YouResigned()
    {
      if (InvokeRequired) { Invoke((Action)(() => YouResigned())); return; }
      string msg = "You resigned. " + GetOpponentName() + " wins.";
      EndGame(msg, playerName + " resigned, " + GetOpponentName() + " wins");
    }
  }
}
