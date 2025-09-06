using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
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
    private Timer playerTimer = new Timer();
    private bool allowUndos = true;
    HubConnection connection;

    private void StartGameToolStripMenuItem_Click(object sender, EventArgs e)
    {
      if (!CheckSafety("Game / Start")) { return; }
      GameSetUp gameSetUp = new GameSetUp(this);
      if (gameSetUp.ShowDialog() == DialogResult.Cancel) { return; }
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
      playerTimeLeft = int.Parse(gameSetUp.PlayHours.Text) * 3600 +
        int.Parse(gameSetUp.PlayMinutes.Text) * 60;
      playerTimer.Interval = 1000; // 1 second
      playerTimer.Tick += new EventHandler(ProcessTick);
      YouMainTime.Text = TimeSpan.FromSeconds(playerTimeLeft).ToString(@"hh\:mm\:ss");
      OpponentMainTime.Text = YouMainTime.Text;

      OpponentName.Text = gameSetUp.OpponentName.Text + ":";
      allowUndos = gameSetUp.AllowUndos.Checked;
      gameInProgress = true;
      CalcTSTextBox2Width();
      if (playerColor == 2)
      {
        statusM.Set("Game started. You to play.");
        playersTurn = true;
        panelMain.Cursor = blackCursor;
        playerTimer.Start();
        playerTimer.Enabled = true;
      }
      else
      {
        statusM.Set("Game started. " + OpponentName.Text.Remove(OpponentName.Text.Length - 1) + " playing.");
        playersTurn = false;
        panelMain.Cursor = whiteCursor;
        playerTimer.Start();
        playerTimer.Enabled = false;
      }
      EnableDisableBoard();
      connection.On<int, int>("MakeMove", MakeMove);
      connection.On("UndoGranted", UndoGranted);
      connection.On("UndoDenied", UndoDenied);
      connection.On("RequestUndo", RequestUndo);
      connection.On<string>("TickTock", TickTock);
    }
    private void PassToolStripMenuItem_Click(object sender, EventArgs e)
    {
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
      TSBwhite.Enabled = !gameInProgress;
      TSBblack.Enabled = !gameInProgress;
      TSBblackWhite.Enabled = !gameInProgress;
      TSBnone.Enabled = !gameInProgress;
    }
    private async void MouseUpInGame(int boardX, int boardY)
    {
      // similar to parts of PanelMain_MouseUp
      SaveState("Mouse Up");
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
      thePoints[boardX, boardY].color = playerColor;
      if (!ProcessMove(boardX, boardY, playerColor)) return;
      EndMove(boardX, boardY, OpponentName.Text.Remove(OpponentName.Text.Length - 1) + " to move");
      SetPlayer(false);
      await connection.InvokeAsync("MakeMove", boardX, boardY);
    }
    private void SetPlayer (bool x)
    {
      playersTurn = x;
      playerTimer.Enabled = x;
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
      SetPlayer(true);
    }
    private async void GameUndo()
    {
      // if undos allowed, then ask opponent if ok
      // if not allowed should never get here as menu items disabled
      statusM.Set("Undo requested. Timers suspended.");
      await connection.InvokeAsync("RequestUndo");
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
        await connection.InvokeAsync("UndoGranted");
      }
      else
      {
        statusM.Set("Your turn continues");
        playerTimer.Enabled = true;
        await connection.InvokeAsync("UndoDenied");
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
      if (playerTimeLeft > 0) playerTimeLeft--;
      if (playerTimeLeft > 0)
      {
        YouMainTime.Text = TimeSpan.FromSeconds(playerTimeLeft).ToString(@"hh\:mm\:ss");
        await connection.InvokeAsync("TickTock", YouMainTime.Text);
      }
      else
      {
        playerTimer.Stop();
        gameInProgress = false;
        EnableDisableBoard();
        statusM.Set("Time up! You lose.");
        MessageBox.Show("Your time has run out. You lose!", "Time up",
          MessageBoxButtons.OK, MessageBoxIcon.Information);
      }
    }
    private void TickTock(string time)
    {
      if (InvokeRequired) { Invoke((Action)(() => TickTock(time))); return; }
      OpponentMainTime.Text = time;
    }
  }
}
