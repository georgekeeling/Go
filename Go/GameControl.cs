using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GoPlanner
{
  public partial class GoPlanner : Form
  {
    // game variables
    private bool gameInProgress = false;
    private string playerName;
    private string playerColor;
    private int playerTimeLeft;
    private int opponentTimeLeft;
    private bool allowUndos = true;
    HubConnection connection;

    private void StartGameToolStripMenuItem_Click(object sender, EventArgs e)
    {
      if (!CheckSafety("Game / Start")) { return; }
      GameSetUp gameSetUp = new GameSetUp(this);
      if (gameSetUp.ShowDialog() == DialogResult.Cancel) { return; }
      // game has started, black is playing
      statusM.Set("Game started. Black to play.");
      connection = gameSetUp.connection;
      playerName = gameSetUp.PlayerName.Text;
      if (gameSetUp.playerColor == "W")
      {
        YouColor.Image = Properties.Resources.StoneWhite;
        OpponentColor.Image = Properties.Resources.StoneBlack;
      }
      else
      {
        YouColor.Image = Properties.Resources.StoneBlack;
        OpponentColor.Image = Properties.Resources.StoneWhite;
      }
      playerColor = gameSetUp.playerColor;
      playerTimeLeft = int.Parse(gameSetUp.PlayHours.Text) * 3600 + 
        int.Parse(gameSetUp.PlayMinutes.Text) * 60;
      OpponentName.Text = gameSetUp.OpponentName.Text + ":";
      opponentTimeLeft = playerTimeLeft;
      allowUndos = gameSetUp.AllowUndos.Checked;
      YouMainTime.Text = TimeSpan.FromSeconds(playerTimeLeft).ToString(@"hh\:mm\:ss");
      OpponentMainTime.Text = TimeSpan.FromSeconds(opponentTimeLeft).ToString(@"hh\:mm\:ss");
      gameInProgress = true;
      CalcTSTextBox2Width();
    }

    private void PassToolStripMenuItem_Click(object sender, EventArgs e)
    {
    }
  }
}
