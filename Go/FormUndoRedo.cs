using GoPlanner.Properties;
using System;
using System.Drawing;
using System.Windows.Forms;
using Windows.Networking.NetworkOperators;
using Windows.UI.Xaml.Automation;

// store up to 500 copies of boards for undo
namespace GoPlanner
{
  public partial class GoPlanner : Form
  {
    const int maxDos = 500;
    UndoRedoBuffer[] undoObjects = new UndoRedoBuffer[maxDos];

    // undos indexes  into undoObjects. It is position where next state will be saved (SaveState)
    // undos - 1 indexes undoObjects which gets restored to board on Undo
    // undos + 1 indexes undoObjects which gets restored to board on Redo
    // undos + redos gives valid length of undoObjects
    int undos;
    int redos;
    int redosWas;
    private void InitUndoRedo()
    {
      undos = 0;
      redos = 0;
      redosWas = 0;
      
      EnableControls();
      for (int board = 0; board < maxDos; board++)
      {
        undoObjects[board] = new UndoRedoBuffer(this);
      }
    }
    private void UndoStripButton_Click(object sender, EventArgs e)
    {
      UndoToolStripMenuItem_Click(sender, e);
    }
    private void UndoToolStripMenuItem_Click(object sender, EventArgs e)
    {
      if (undos <= 0) { EnableControls(); return; } // something wrongly enabled
      if (redos == 0) 
      {
        // current stae not saved yet, so
        SaveState2();
      }
      undos--;
      redos++;
      EnableControls();
      // copy out of undos and show
      RestoreBoard(undoObjects[undos]);
      RedCirclesAddOld();
    }
    private void RedoStripButton_Click(object sender, EventArgs e)
    {
      RedoToolStripMenuItem_Click(sender, e);
    }
    private void RedoToolStripMenuItem_Click(object sender, EventArgs e)
    {
      if (redos <= 0) { EnableControls(); return; } // something wrongly enabled
      redos--;
      undos++;
      RedCirclesRemoveOld();
      EnableControls();
      RestoreBoard(undoObjects[undos]);
    }
    private void RestoreBoard(UndoRedoBuffer UndoRedoObject)
    {
      // restore from UndoRedoBuffer to board, showing all changes
      for (int x = 0; x < bSide; x++)
      {
        for (int y = 0; y < bSide; y++)
        {
          bool updateNeeded = !LooksEqualTo(thePoints[x, y], UndoRedoObject.urPoints[x, y]);
          thePoints[x, y].color = UndoRedoObject.urPoints[x, y].color;
          thePoints[x, y].moveAdded = UndoRedoObject.urPoints[x, y].moveAdded;
          if (updateNeeded) 
          {
            panelMain.Invalidate(PixRectFromBoardXY(x, y)); 
          }
        }
      }
      prSLider.Invalidate();
      TSTextBox2.Text = UndoRedoObject.statusMessage;
      TSBblackWhite.Checked = UndoRedoObject.TSBblackWhiteChecked;
      TSBwhite.Checked = UndoRedoObject.TSBwhiteChecked;
      TSBblack.Checked = UndoRedoObject.TSBblackChecked;
      capturedBlacks = UndoRedoObject.capturedBlacks;
      capturedWhites = UndoRedoObject.capturedWhites;
      ToolSCapturedBlacks.Text = capturedBlacks.ToString();
      ToolSCapturedWhites.Text = capturedWhites.ToString();
      if (TSBwhite.Checked)
      {
        panelMain.Cursor = whiteCursor;
      }
      else
      {
        panelMain.Cursor = blackCursor;
      }
    }
    private void SaveState(string caller)
    {
      // copy in current board to undoObjects [undos]
      if (undos >= maxDos)
      {
        new MyMessageBox("Undo buffer overflow", "Error", this);
        return;
      }
      SaveState2();
      undos++;
      redosWas = redos;
      redos = 0;
      EnableControls();
      // Console.WriteLine("SaveState by " + caller + ", redos were " + redosWas);
    }
    private void SaveState2()
    {
      for (int x = 0; x < bSide; x++)
      {
        for (int y = 0; y < bSide; y++)
        {
          undoObjects[undos].urPoints[x, y].color = thePoints[x, y].color;
          undoObjects[undos].urPoints[x, y].moveAdded = thePoints[x, y].moveAdded;
        }
      }
      undoObjects[undos].statusMessage = TSTextBox2.Text;
      undoObjects[undos].TSBblackWhiteChecked = TSBblackWhite.Checked;
      undoObjects[undos].TSBwhiteChecked = TSBwhite.Checked;
      undoObjects[undos].TSBblackChecked = TSBblack.Checked;
      undoObjects[undos].capturedBlacks = capturedBlacks;
      undoObjects[undos].capturedWhites = capturedWhites;
    }
    private void UnSaveState(string caller)
    {
      // reverse SaveState which was just called (the operation was aborted)
      // state of board is left to caller
      undos--;
      redos = redosWas;
      EnableControls();
      // Console.WriteLine("UnSaveState by " + caller + ", redos were " + redosWas);
    }
    private void RedCirclesAddOld ()
    {
      // after undo must add some red circles. Very similar to RedCirclesRemoveOld
      if (undos == 0) { return; }
      for (int x = 0; x < bSide; x++)
      {
        for (int y = 0; y < bSide; y++)
        {
          if (undos == 1)
          {
            if (thePoints[x,y].color > 0)
            {
              panelMain.Invalidate(PixRectFromBoardXY(x, y));
              continue;
            }
          }
          if (undoObjects[undos].urPoints[x, y].color != undoObjects[undos - 1].urPoints[x, y].color)
          {
            panelMain.Invalidate(PixRectFromBoardXY(x, y));
          }
        }
      }
    }
    private bool CheckForKo()
    {
      // check for ko, return true if ko
      // ko is when the last move was a capture and the board is the same as the board before that
      
      if (undos < 2) { return false; }
      for (int x = 0; x < bSide; x++)
      {
        for (int y = 0; y < bSide; y++)
        {
          if (thePoints[x, y].color != undoObjects[undos - 2].urPoints[x, y].color) { return false; }
        }
      }
      return true;
    }
    private void EnableControls()
    {
      if (redos <= 0)
      {
        RedoToolStripMenuItem.Enabled = false;
        RedoStripButton.Enabled = false;
        Redo10StripButton.Enabled = false;
        redos = 0;
      }
      else
      {
        RedoToolStripMenuItem.Enabled = true;
        RedoStripButton.Enabled = true;
        Redo10StripButton.Enabled = true;
      }
      if (undos <= 0)
      {
        UndoToolStripMenuItem.Enabled = false;
        UndoStripButton.Enabled = false;
        Undo10StripButton.Enabled = false;
        undos = 0;
      }
      else
      {
        UndoToolStripMenuItem.Enabled = true;
        UndoStripButton.Enabled = true;
        Undo10StripButton.Enabled = true;
      }
    }
    private void Undo10StripButton_Click(object sender, EventArgs e)
    {
      UndoMultiple(10);
    }
    private void Redo10StripButton_Click(object sender, EventArgs e)
    {
      RedoMultiple(10);
    }
    private void UndoMultiple(int howMany)
    {
      if (undos <= 0) { EnableControls(); return; }
      if (howMany > undos) { howMany = undos; }
      for (int i = 0; i < howMany; i++)
      {
        UndoToolStripMenuItem_Click(null, null);
      }
    }
    private void RedoMultiple(int howMany)
    {
      if (redos <= 0) { EnableControls(); return; }
      if (howMany > redos) { howMany = redos; }
      for (int i = 0; i < howMany; i++)
      {
        RedoToolStripMenuItem_Click(null, null);
      }
    }
    public class UndoRedoBuffer
    {
      public AStone[,] urPoints;
      public string statusMessage = "";
      public bool TSBblackWhiteChecked;
      public bool TSBwhiteChecked;
      public bool TSBblackChecked;
      public short capturedBlacks = 0;
      public short capturedWhites = 0;

      public UndoRedoBuffer(GoPlanner gp)
      {
        urPoints = gp.CreateStones(gp.bSide, gp.bSide);
      }
    }
  }
}
