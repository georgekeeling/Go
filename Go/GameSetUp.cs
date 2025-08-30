using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;

namespace GoPlanner
{
  public partial class GameSetUp : Form
  {
    HubConnection connection;
    GoPlanner gp;
    public GameSetUp(GoPlanner parent)
    {
      InitializeComponent();
      gp = parent;
      Owner = parent;
      StartPosition = FormStartPosition.CenterParent;
      ButtonTellServerName.Enabled = false;
      ButtonTellServerOpponent.Enabled = false;
      ButtonStart.Enabled = false;
      ServerStatus.Text = "";
      NameError.Text = "";
      OpponentError.Text = "";
      Load += GameSetUp_Load;
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
        connection.On<string>("ChallengeIn", ChallengeIn);
        connection.On("OpponentDeparted", OpponentDeparted);
        connection.On("ChallengeAccepted", ChallengeAccepted);
        connection.On("ChallengeDeclined", ChallengeDeclined);
        // connection.On<string, int>("XXX", XXX);   // example with 2 arguments
      }
      catch (Exception ex)
      {
        ServerStatus.Text = "Could not connect to " + server;
        ServerError.Text = ex.Message;
      }

      connection.Closed += async (error) =>
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
      try
      {
        await connection.InvokeAsync("Challenge", PlayerName.Text, OpponentName.Text);
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
    private async void ChallengeIn(string opponentName)
    {
      if (InvokeRequired)
      {
        Invoke((Action)(() => ChallengeIn(opponentName)));
        return;
      }
      int reply = 0;
      new MyMessageBox(opponentName + " challenges you to a game.", "Challenge", ref reply, 
        "Accept", "Decline", "", this);
      if (reply == 3)
      {
        // Accepted
        ButtonTellServerOpponent.Enabled = false;
        OpponentError.Text = "✓";
        OpponentName.Enabled = false;
        OpponentName.Text = opponentName;
        ButtonStart.Enabled = true;
        ButtonCancel.Enabled = false;
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
    private void ChallengeAccepted()
    {
      if (InvokeRequired) { Invoke((Action)ChallengeAccepted); return; }
      ButtonTellServerOpponent.Enabled = false;
      OpponentError.Text = "✓";
      ButtonStart.Enabled = true;
      ButtonCancel.Enabled = false;
      OpponentName.Enabled = false;
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
    private void OpponentDeparted()
    {
      // this can happen during game or game set up
      if (InvokeRequired) { Invoke((Action)OpponentDeparted); return; }
      int reply = 0;
      new MyMessageBox(OpponentName.Text + " has departed.", "Opponent Departed", ref reply, 
        "OK", "", "", this);
      ButtonTellServerOpponent.Enabled = true;
      OpponentError.Text = "Opponent departed suddenly";
      OpponentName.Enabled = true;
      OpponentName.Text = "";
      ButtonStart.Enabled = false;
      ButtonCancel.Enabled = true;
    }

  }
}
