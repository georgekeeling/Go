using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Printing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;

namespace GoPlanner
{
  public partial class GameSetUp : Form
  {
    public HubConnection connection;
    GoPlanner gp;
    public string playerColor = "B";   // B or W
    // gamester who accepted challenge cannot alter values 
    private int setupState = 0;  // 0 = starting, 1 = issued challenge that was accepted, 2 = accepted challenge

    public GameSetUp(GoPlanner parent)
    {
      InitializeComponent();
      gp = parent;
      Owner = parent;
      StartPosition = FormStartPosition.CenterParent;
      PlayerName.Text = Properties.Settings.Default.playerName;
      OpponentName.Text = Properties.Settings.Default.opponentName;
      ButtonTellServerOpponent.Enabled = false;
      PlayHours.Text = Properties.Settings.Default.playHours;
      PlayMinutes.Text = Properties.Settings.Default.playMinutes;
      if (PlayMinutes.Text == "" && PlayHours.Text == "") { PlayMinutes.Text = "20"; }
      AllowUndos.Checked = Properties.Settings.Default.allowUndos;
      playerColor = Properties.Settings.Default.playerColor;
      if (playerColor == "B") 
      { 
        // usually player should change colors every game (assuming they always same opponent)
        playerColor = "W";
        SetStoneImage(PlayerStone, "W");
        SetStoneImage(OpponentStone, "B");
      } 
      else 
      {
        playerColor = "B";
        SetStoneImage(PlayerStone, "B");
        SetStoneImage(OpponentStone, "W");
      }
      ButtonStart.Enabled = false;
      ServerStatus.Text = "";
      NameError.Text = "";
      OpponentError.Text = "";
      TimeError.Text = "";
      Load += GameSetUp_Load;
      FormClosing += GameSetUp_FormClosing;
    }
    private async void GameSetUp_Load(object sender, System.EventArgs e)
    {
      string server;
#if DEBUG
      server = "https://localhost:7122/ChatHub";
#else
      server = "https://go.racingdemon.net/ChatHub";
#endif
      ServerStatus.Text = "Connecting to " + server;
      connection = new HubConnectionBuilder()
          .WithUrl(server)
          .Build();
      Console.WriteLine("GameSetUp_Load " + connection.ConnectionId);
      // see racingDemon main.ts & Program.cs for setting these
      const int kAIsecs = 100;
      connection.KeepAliveInterval = TimeSpan.FromSeconds(kAIsecs);
      connection.ServerTimeout = TimeSpan.FromSeconds(2 * kAIsecs);
      try
      {
        await connection.StartAsync();
        ServerStatus.Text = "Connected to " + server;
        ServerError.Text = "✓";
        ButtonTellServerName.Enabled = true;
        connection.On("NameOK", NameOK);
        connection.On("NameHadError", NameHadError);
        connection.On("OpponentUnavailable", OpponentUnavailable);
        connection.On("OpponentThinking", OpponentThinking);
        connection.On<string, string, string, bool, string>("ChallengeIn", ChallengeIn);
        connection.On("OpponentDeparted", OpponentDepartedInSetup);
        connection.On("ChallengeAccepted", ChallengeAccepted);
        connection.On("ChallengeDeclined", ChallengeDeclined);
        connection.On("GameStarted", GameStarted);
        connection.On<string>("ColorChanged", ColorChanged);
        connection.On<string>("HoursChanged", HoursChanged);
        connection.On<string>("MinutesChanged", MinutesChanged);
        connection.On<bool>("UndoChanged", UndoChanged);
      }
      catch (Exception ex)
      {
        ServerStatus.Text = "Could not connect to " + server;
        ServerError.Text = ex.Message;
      }

      gp.closedHandler = async (error) =>
      {
        // Default: if no action, connection is closed after 30s
        // If KeepAliveInterval & ServerTimeout set as above, connection does not
        // get closed.
        int delay = new Random().Next(0, 5) * 1000;
        DateTime now = DateTime.Now;
        Console.WriteLine("connection.Closed " + now.Minute + ":" + now.Second + " error " + error);
        await Task.Delay(delay);
        now = DateTime.Now;
        Console.WriteLine("connection.Closed " + now.Minute + ":" + now.Second + " restarted after " + delay);
        await connection.StartAsync();
      };
      connection.Closed += gp.closedHandler;

    }
    private async void ButtonTellServerName_Click(object sender, EventArgs e)
    {
      if (PlayerName.Text == "")
      {
        NameError.Text = "Please enter a name";
        return;
      }
      try
      {
        await connection.InvokeAsync("CheckName" , PlayerName.Text);
      }
      catch (Exception ex)
      {
        NameError.Text = ex.Message;
      }

    }
    private void NameOK()
    {
      if (InvokeRequired)
      {
        // NameOK is called from the server thread, so we need to use Invoke to call from UI thread
        Invoke((Action)NameOK);
        return;
      }
      NameError.Text = "✓";
      ButtonTellServerName.Enabled = false;
      PlayerName.Enabled = false;
      ButtonTellServerOpponent.Enabled = true;
    }
    private void NameHadError()
    {
      if (InvokeRequired) { Invoke((Action)NameHadError); return; }
      NameError.Text = "Name in use";
      ButtonTellServerName.Enabled = true;
      PlayerName.Enabled = true;
    }
    private async void ButtonTellServerOpponent_Click(object sender, EventArgs e)
    {
      if (OpponentName.Text == "")
      {
        OpponentError.Text = "Please enter a name";
        return;
      }
      if (OpponentName.Text == PlayerName.Text)
      {
        OpponentError.Text = "Please enter a different name. Duh!";
        return;
      }
      try
      {
        await connection.InvokeAsync("Challenge", PlayerName.Text, OpponentName.Text,
          PlayHours.Text, PlayMinutes.Text, AllowUndos.Checked, playerColor);
      }
      catch (Exception ex)
      {
        OpponentError.Text = ex.Message;
      }
    }
    private void OpponentUnavailable()
    {
      if (InvokeRequired) { Invoke((Action)OpponentUnavailable); return; }
      OpponentError.Text = "Opponent unavailable";
      ButtonTellServerOpponent.Enabled = true;
      OpponentName.Enabled = true;
    }
    private void OpponentThinking()
    {
      if (InvokeRequired) { Invoke((Action)OpponentThinking); return; }
      OpponentError.Text = "Opponent is thinking";
      ButtonTellServerOpponent.Enabled = false;
      OpponentName.Enabled = false;
    }
    private async void ChallengeIn(string opponentName,
      string hours, string minutes, bool undosAllowed, string opponentColor)
    {
      if (InvokeRequired)
      {
        Invoke((Action)(() => ChallengeIn(opponentName, hours, minutes, undosAllowed, opponentColor)));
        return;
      }
      int reply = 0;
      string hourString = "";
      if (hours != "0" && hours != "")
      {
        hourString = hours + "h";
      }
      string minutesString = "";
      if (minutes != "0" && minutes != "")
      {
        minutesString = minutes + "m";
      }
      new MyMessageBox(opponentName +" (" + opponentColor + ", " + hourString + minutesString + "," +
        (undosAllowed ? " Undos allowed" : " No undos") + ")" + 
        " challenges you to a game.", "Challenge", ref reply, 
        "Accept", "Decline", "", this);
      if (reply == 3)
      {
        // Accepted
        ButtonTellServerOpponent.Enabled = false;
        OpponentError.Text = "✓";
        OpponentName.Text = opponentName;
        playerColor = opponentColor;
        PlayerStone_Click(null, null);    // reverses colors
        PlayHours.Text = hours;
        PlayMinutes.Text = minutes;
        AllowUndos.Checked = undosAllowed;
        ParamsEnable(false);
        ButtonStart.Enabled = false;
        ButtonCancel.Enabled = true;
        setupState = 2;  // accepted challenge
        await connection.InvokeAsync("AcceptChallenge", PlayerName.Text, OpponentName.Text);
      }
      else
      {
        // Declined
        ButtonTellServerOpponent.Enabled = true;
        OpponentName.Enabled = true;
        await connection.InvokeAsync("DeclineChallenge", PlayerName.Text, opponentName);
      }
    }
    private void ParamsEnable(bool enable)
    {
      OpponentName.Enabled = enable;
      PlayHours.Enabled = enable;
      PlayMinutes.Enabled = enable;
      AllowUndos.Enabled = enable;
      PlayerStone.Enabled = enable;
      OpponentStone.Enabled = enable;
    }
    private void ChallengeAccepted()
    {
      if (InvokeRequired) { Invoke((Action)ChallengeAccepted); return; }
      ButtonTellServerOpponent.Enabled = false;
      OpponentError.Text = "✓";
      ButtonStart.Enabled = true;
      ButtonCancel.Enabled = true;
      OpponentName.Enabled = false;
      setupState = 1;  // issued challenge that was accepted
    }
    private void ChallengeDeclined()
    {
      if (InvokeRequired) { Invoke((Action)ChallengeDeclined); return; }
      Console.WriteLine("ChallengeDeclined");
      ButtonTellServerOpponent.Enabled = true;
      OpponentError.Text = OpponentName.Text + " declined challenge";
      OpponentName.Enabled = true;
      OpponentName.Text = "";
    }
    private void OpponentDepartedInSetup()
    {
      // this only game set up
      if (InvokeRequired) { Invoke((Action)OpponentDepartedInSetup); return; }
      int reply = 0;
      new MyMessageBox(OpponentName.Text + " has departed.", "Opponent Departed", ref reply, 
        "OK", "", "", this);
      ButtonTellServerOpponent.Enabled = true;
      OpponentError.Text = "Opponent departed suddenly";
      OpponentName.Text = "";
      ButtonStart.Enabled = false;
      ButtonCancel.Enabled = true;
      setupState = 0;
      ParamsEnable(true);
    }

    private async void GameSetUp_FormClosing(object sender, FormClosingEventArgs e)
    {
      Console.WriteLine("GameSetUp_FormClosing " + DialogResult);
      if (DialogResult == DialogResult.OK) 
      {
        // game start clicked
        SaveSettings();
        await connection.InvokeAsync("GameStart", PlayerName.Text, OpponentName.Text);
        return; 
      }
      else
      {
        //cancel or x clicked
        connection.Closed -= gp.closedHandler;
        await connection.StopAsync();
      }
    }
    private void SaveSettings()
    {
      Properties.Settings.Default.playerName = PlayerName.Text;
      Properties.Settings.Default.opponentName = OpponentName.Text;
      Properties.Settings.Default.playHours = PlayHours.Text;
      Properties.Settings.Default.playMinutes = PlayMinutes.Text;
      Properties.Settings.Default.allowUndos = AllowUndos.Checked;
      Properties.Settings.Default.playerColor = playerColor;
      Properties.Settings.Default.Save();
    }
    private void GameStarted()
    {
      // game started by other player
      if (InvokeRequired) { Invoke((Action)GameStarted); return; }
      FormClosing -= GameSetUp_FormClosing;
      SaveSettings();
      Close();
      DialogResult = DialogResult.OK;
    }
    private void SetStoneImage(Button stoneButton, string color)
    {
      // Scale the image to fit the button
      Image img = Properties.Resources.StoneWhite;
      if (color == "B")
      {
        img = Properties.Resources.StoneBlack;
      }
      var scaledImg = new Bitmap(img, stoneButton.Width / 2, stoneButton.Height / 2);
      stoneButton.Image = scaledImg;
      stoneButton.ImageAlign = ContentAlignment.MiddleCenter;
    }

    private async void PlayerStone_Click(object sender, EventArgs e)
    {
      if (playerColor == "B")
      {
        playerColor = "W";
        SetStoneImage(PlayerStone, "W");
        SetStoneImage(OpponentStone, "B");
      }
      else
      {
        playerColor = "B";
        SetStoneImage(PlayerStone, "B");
        SetStoneImage(OpponentStone, "W");
      }
      if (setupState == 1)
      {
        // change other player's color
        await connection.InvokeAsync("ChangeColor", OpponentName.Text, playerColor);
      }
    }
    private void OpponentStone_Click(object sender, EventArgs e)
    {
      PlayerStone_Click(sender, e);
    }
    private void ColorChanged(string newColor)
    {
      // color changed by other player
      if (InvokeRequired) { Invoke((Action)(() => ColorChanged(newColor))); return; }
      if (setupState != 2) { return; }
      PlayerStone_Click(null, null);
    }

    private async void PlayHours_TextChanged(object sender, EventArgs e)
    {
      if  (PlayHours.Text == " " || PlayHours.Text == "  ") PlayHours.Text = "";
      if (PlayHours.Text != "") 
      {
        if (!int.TryParse(PlayHours.Text, out int hours) || hours < 0)
        {
          PlayHours.Text = "";
          TimeError.Text = "Hours must be positive 2 digit number";
          return;
        }
      }
      TimeError.Text = "";
      if (setupState == 1)
      {
        // change other player's hours
        await connection.InvokeAsync("ChangeHours", OpponentName.Text, PlayHours.Text);
      }
    }
    private void HoursChanged(string newHours)
    {
      // color changed by other player
      if (InvokeRequired) { Invoke((Action)(() => HoursChanged(newHours))); return; }
      if (setupState != 2) { return; }
      PlayHours.Text = newHours;
    }
    private async void PlayMinutes_TextChanged(object sender, EventArgs e)
    {
      if (!int.TryParse(PlayMinutes.Text, out int minutes) || minutes < 0 || minutes >= 60)
      {
        PlayMinutes.Text = "";
        TimeError.Text = "Minutes must be positive number < 60";
        return;
      }
      TimeError.Text = "";
      if (setupState == 1)
      {
        // change other player's minutes
        await connection.InvokeAsync("ChangeMinutes", OpponentName.Text, PlayMinutes.Text);
      }
    }
    private void MinutesChanged(string newMinutes)
    {
      // color changed by other player
      if (InvokeRequired) { Invoke((Action)(() => MinutesChanged(newMinutes))); return; }
      if (setupState != 2) { return; }
      PlayMinutes.Text = newMinutes;
    }

    private async void AllowUndos_CheckedChanged(object sender, EventArgs e)
    {
      if (setupState == 1)
      {
        await connection.InvokeAsync("ChangeUndo", OpponentName.Text, AllowUndos.Checked);
      }
    }
    private void UndoChanged(bool newState)
    {
      if (InvokeRequired) { Invoke((Action)(() => UndoChanged(newState))); return; }
      if (setupState != 2) { return; }
      AllowUndos.Checked = newState;
    }
  }
}
