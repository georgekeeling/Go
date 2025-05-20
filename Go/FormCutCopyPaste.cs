using System;
using System.Windows.Forms;
using System.Drawing;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Windows;
using System.Windows.Input;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using System.Threading;
using Windows.ApplicationModel.DataTransfer;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

// cut / copy / paste / move / delete
// ------------------
// Summary INCOMPLETE !!!!
// clipboard data is in two formats: Bitmap and goClipType. See CopyCut
// which puts the selected data in the clipboard. The data remains there, 
// available for pasting until there is another CopyCut or another app puts data
// in the clipboard. The clipboard is monitored by CheckClipboardGo



// after paste, cut cells are gone and no more pasting possible.
// after paste, copy cells remain and more pasting is possible.
// after move, selection rectangle is gone and no more pasting possible.

// Details
// a selection rectangle is created by a combination of mouse down, move, up
// or just by mouse down on single stone
// cut and copy become enabled when a selection rectangle exists

// on mouse click, file open, escape pressed
// the selection rectangle disappears, pasting is aborted
// if a cut was in operation, the cut data is reinstated 
// move is aborted by escape

// On cut / copy, contents of selection retangle copied to clipboard
// Addtionally on cut or move, occupied points in selection rectangle are dimmed,
// top bit set on source points

// On paste, cursor changes to cross and dashed paste rectangle hangs off it
// paste rectangle is same size as selection rectangle
// selection rectangle remains in place. 

// on mouse up, when cursor is cross (pasting), 
// cursor becomes default
// clipboard data is pasted in at mouse position
// if cutting, cut data is removed, selection rectangle disappears, 
//   paste, cut and copy (and others) disabled
// if copying, selection rectangle remains

// Selection / paste / Move rectangles are always drawn half way between points, can be off board

// selection rectangle coordinates are stored in selection.TLselX, TLselY, BRselX, BRselY
// paste / move rectangle coordinates are stored in TLpasteCx, TLpasteCy, BRpasteCx, BRpasteCy

// MouseDown on a stone selects the stone, cursor changes to hand
// MouseDown not NearPoint(), can be off board, starts a selection rectangle
// MouseUp changes state of point if mouse is NearPoint() and !mouseDragging


// States
// mouseDown                         ⇒ mouse is down
// mouseDragged                      ⇒ mouse is down, moving and forming selection rectangle
// selection.Exists()                ⇒ have selection rectangle
// cutting                           ⇒ if (selection.Exists()) indicates cutting or pasting
// panelMain.Cursor == Cursors.Cross ⇒ pasting, final stage

// clipboard data is stored in the system clipboard as a bitmap and
// as "GoGK" in byte [,] array, see CopyCut
// move data is not in clipboard, stored in sele ction.dragPoints
// Like PowerPoint, cannot move (drag) from one PP window to other

namespace GoPlanner
{
  public partial class GoPlanner : Form
  {
    private int TLpasteCx;      // paste coordinates invalid when cursor == cursors.default
    private int TLpasteCy;
    private int BRpasteCx;
    private int BRpasteCy;
    public bool cutting;
    private readonly string goClipType = "GoGK";
    private void InitCutCopyPaste() 
    {
      EnableCutCopyEtc(false);
      DeleteToolStripMenuItem.Enabled = true;
      // detecting clipboard change is very complicated but possible after exhaustive research
      // https://stackoverflow.com/questions/621577/how-do-i-monitor-clipboard-changes-in-c
      // or here
      // https://learn.microsoft.com/en-us/uwp/api/windows.applicationmodel.datatransfer.clipboard.getcontent?view=winrt-26100
      // but I cannot get at Windows.ApplicationModel.DataTransfer until I found
      // https://stackoverflow.com/questions/60581121/how-to-add-a-reference-to-the-windows-applicationmodel-datatransfer-namespace-in
      // I have added to both the stackoverflow posts
      CheckClipboardGo();
      Windows.ApplicationModel.DataTransfer.Clipboard.ContentChanged += Clipboard_ContentChanged;
    }
    private void Clipboard_ContentChanged(object sender, object e)
    {
      CheckClipboardGo();
    }
    private bool CheckClipboardGo()
    {
      bool result = false;
      if (System.Windows.Clipboard.ContainsData(goClipType))
      {
        AStone[,] clipData;
        try
        {
          clipData = (AStone[,])System.Windows.Clipboard.GetData(goClipType);
          result = true;
        }
        catch
        {
          Console.WriteLine("System.Windows.Clipboard.GetData failed");
        }
      }
      pasteToolStripButton.Enabled = result;
      pasteToolStripMenuItem.Enabled = result;
      return result;
    }
    private bool GetClipboardData(ref AStone[,] clipData)
    {
      if (!CheckClipboardGo())
      {
        // difficult to see how this could happen and v hard to test
        new MyMessageBox("Oops, clipboard data vanished!", this);
        return false;
      }
      try
      {
        clipData = (AStone[,])System.Windows.Clipboard.GetData(goClipType);
      }
      catch
      {
        new MyMessageBox("Oops, clipboard data vanished!", this);
        return false;
      }
      return true;
    }
    public void EnableCutCopyEtc (bool enable)
    {
      copyToolStripButton.Enabled = enable;
      copyToolStripMenuItem.Enabled = enable;
      cutToolStripButton.Enabled = enable;
      cutToolStripMenuItem.Enabled = enable;
    }
    private void DeleteToolStripMenuItem_Click(object sender, EventArgs e)
    {
      // delete all cells or just selected cells
      if (!selection.Exists())
      {
        TSBnone_Click(null, null);
        return;
      }
      SaveState("Delete menu");
      prSLider.Invalidate();
      for (int Cx = selection.TLx(); Cx <= selection.BRx(); Cx++)
      {
        for (int Cy = selection.TLy(); Cy <= selection.BRy(); Cy++)
        {
          if (thePoints[Cx, Cy].color != 0)
          {
            thePoints[Cx, Cy].color = 0;
            panelMain.Invalidate(PixRectFromBoardXY(Cx, Cy));
          }
        }
      }
      RedCirclesRemoveOld();
    }
    private void copyToolStripButton_Click(object sender, EventArgs e)
    {
      copyToolStripMenuItem_Click(sender, e);
    }
    private void copyToolStripMenuItem_Click(object sender, EventArgs e)
    {
      CopyCut(false);
    }
    private void cutToolStripMenuItem_Click(object sender, EventArgs e)
    {
      cutToolStripButton_Click(sender,e);
    }
    private void cutToolStripButton_Click(object sender, EventArgs e)
    {
      CopyCut(true);
      panelMain.Invalidate(PixRectFromBoardRect(selection.TLx(), selection.TLy(),
        selection.BRx() + 1, selection.BRy() + 1));
    }
    private void CopyCut(bool cutting)
    {
      // use of DataObject from
      // https://learn.microsoft.com/en-us/dotnet/desktop/winforms/advanced/how-to-add-data-to-the-clipboard?view=netframeworkdesktop-4.8
      System.Windows.DataObject data = new System.Windows.DataObject();

      // 1) Use panelMain_Paint to draw clip data into bitmap, reduce to bitmap2
      // then copy bitmap2 to system clipboard
      // 
      // from Copilot, with a few changes, especially use of bitmap2
      int TLx = PixelXYfromBoardXY(selection.TLx());
      int TLy = PixelXYfromBoardXY(selection.TLy());
      int width = PixelXYfromBoardXY(selection.BRx() - selection.TLx() + 1);
      int height = PixelXYfromBoardXY(selection.BRy() - selection.TLy() + 1);
      int cellSide = PixelXYfromBoardXY(0);
      int squareSide = cellSide * (bSide + 1);
      int bitmap2width = width - cellSide;
      int bitmap2height = height - cellSide;
      this.cutting = cutting;

      if (selection.TLx() == 0) 
      {
        TLx -= cellSide / 2;
        bitmap2width += cellSide / 2; 
      }
      if (selection.TLy() == 0) 
      {
        TLy -= cellSide / 2;
        bitmap2height += cellSide / 2; 
      }
      if (selection.BRx() == bSide - 1) {  bitmap2width += cellSide / 2;}
      if (selection.BRy() == bSide - 1) { bitmap2height += cellSide / 2; }

      using (Bitmap bitmap = new Bitmap(squareSide, squareSide),
        bitmap2 = new Bitmap(bitmap2width, bitmap2height))
      {
        using (Graphics g = Graphics.FromImage(bitmap))
        {
          g.Clear(panelMain.BackColor); // factor of 100 speed up over looping through pixels and bitmap.SetPixel(x, y, panelMain.BackColor);
          PaintEventArgs paintEventArgs = new PaintEventArgs(g, new Rectangle(TLx, TLy, width, height));
          PanelMain_Paint("CopyCut", paintEventArgs);   // sender as string suppresses selection rectangle etc
        }

        TLx -= cellSide / 2 - 1;   
        TLy -= cellSide / 2 - 1;

        using (Graphics g2 = Graphics.FromImage(bitmap2))
        {
          g2.DrawImage(bitmap, new Rectangle(0, 0, bitmap2.Width, bitmap2.Height), new Rectangle(TLx, TLy, bitmap2.Width, bitmap2.Height), GraphicsUnit.Pixel);
          // also factor of 100 speed up over looping through pixels
        }

        System.Windows.Forms.Clipboard.Clear();
        data.SetData(System.Windows.Forms.DataFormats.Bitmap, bitmap2);


        // 2) Now do internal clipBoard in goClipType
        AStone[,] clipData = CreateStones(selection.BRx() - selection.TLx() + 1,
          selection.BRy() - selection.TLy() + 1);
        for (int Cy = selection.TLy(); Cy <= selection.BRy(); Cy++)
        {
          for (int Cx = selection.TLx(); Cx <= selection.BRx(); Cx++)
          {
            clipData[Cx - selection.TLx(), Cy - selection.TLy()].color = thePoints[Cx, Cy].color;
            clipData[Cx - selection.TLx(), Cy - selection.TLy()].moveAdded = thePoints[Cx, Cy].moveAdded;
            if (cutting)
            {
              thePoints[Cx, Cy].color |= 0x80;
              panelMain.Invalidate(PixRectFromBoardXY(Cx, Cy));
            }
          }
        }

        data.SetData(goClipType, clipData);
        System.Windows.Clipboard.SetDataObject(data, true);
      }
    }
    private void pasteToolStripMenuItem_Click(object sender, EventArgs e)
    {
      pasteToolStripButton_Click(sender, e);
    }
    private void pasteToolStripButton_Click(object sender, EventArgs e)
    {
      // Move clipboard data  to selection.MovePoints
      selection.movePoints = null;
      if (!GetClipboardData(ref selection.movePoints)) { return; }
      int width = selection.movePoints.GetLength(0);
      int height = selection.movePoints.GetLength(1);
      panelMain.Cursor = System.Windows.Forms.Cursors.Cross;
      System.Drawing.Point xy = System.Windows.Forms.Cursor.Position;          // xy relative to screen
      xy = panelMain.PointToClient(xy);     // xy relative to panelMain
      TLpasteCx = BoardXYfromPixelXY(xy.X);
      TLpasteCy = BoardXYfromPixelXY(xy.Y);

      BRpasteCx = TLpasteCx + width - 1;
      BRpasteCy = TLpasteCy + height - 1;
      panelMain.Invalidate(PixRectFromBoardRect(TLpasteCx - 1, TLpasteCy - 1,
        BRpasteCx + 1, BRpasteCy + 1));
    }
    private void EndPaste (System.Windows.Forms.MouseEventArgs e)
    {
      EndMovePaste(BoardXYfromPixelXY(e.X), BoardXYfromPixelXY(e.Y), cutting);
    }
    private void EndMovePaste (int baseX, int baseY, bool cutting)
    {
      int width = selection.movePoints.GetLength(0);
      int height = selection.movePoints.GetLength(1);
      if (cutting)
      {
        // remove shady bit before SaveState
        for (int Cy = 0; Cy < height; Cy++)
        {
          for (int Cx = 0; Cx < width; Cx++)
          {
            int x = selection.TLx() + Cx; int y = selection.TLy() + Cy;
            if (thePoints[x, y].color != 0)
            {
              thePoints[x, y].color &= 0x7F;
            }
          }
        }
      }
      SaveState("EndMovePaste");
      // now remove selection contents if cutting/moving (destination may be over selection)
      if (cutting)
      {
        for (int Cy = 0; Cy < height; Cy++)
        {
          for (int Cx = 0; Cx < width; Cx++)
          {
            int x = selection.TLx() + Cx; int y = selection.TLy() + Cy;
            if (thePoints[x, y].color != 0)
            {
              thePoints[x, y].color = 0;
              thePoints[x, y].moveAdded = 0;
              panelMain.Invalidate(PixRectFromBoardXY(x, y));
            }
          }
        }
      }
      // then paste in clipboard
      for (int Cy = 0; Cy < height; Cy++)
      {
        for (int Cx = 0; Cx < width; Cx++)
        {
          int x = Cx + baseX; int y = Cy + baseY;
          if ( x >= bSide || y >= bSide) { continue; }
          thePoints[x, y].color = selection.movePoints[Cx, Cy].color;
          if (cutting)
          {
            thePoints[x, y].moveAdded = selection.movePoints[Cx, Cy].moveAdded;
          }
          else
          {
            if (thePoints[x, y].color != 0)
            {
              thePoints[x, y].moveAdded = GetNextMoveNr();
            }
          }
          panelMain.Invalidate(PixRectFromBoardXY(x, y));
        }
      }
      RedCirclesRemoveOld();
      prSLider.Invalidate();
      CancelPaste();
      if (cutting) selection.Cancel();
      this.cutting = false;   // can paste again, but it's not cutting
    }
    private void EndMove(System.Windows.Forms.MouseEventArgs e)
    {
      // Like PowerPoint, cannot move (drag) from one PP window to other
      int baseX = BoardXYfromPixelXY(e.X);
      int baseY = BoardXYfromPixelXY(e.Y);
      if (!LegalMove(baseX, baseY))
      {
        baseX = TLpasteCx;
        baseY = TLpasteCy;
      }
      EndMovePaste(baseX, baseY, true);
    }
    private void CancelPaste()
    {
      if (TSBblack.Checked) { panelMain.Cursor = blackCursor; }
      else { panelMain.Cursor = whiteCursor; }

      panelMain.Invalidate(PixRectFromBoardRect(TLpasteCx, TLpasteCy, BRpasteCx,
        BRpasteCy));
      for (int x = 0; x < bSide; x++)
      {
        for (int y = 0; y < bSide; y++)
        {
          if ((thePoints[x, y].color & 0x80) != 0)
          {
            thePoints[x, y].color &= 0x7F;
            panelMain.Invalidate(PixRectFromBoardXY(x, y));
          }
        }
      }
    }
    private void selectAllToolStripMenuItem_Click(object sender, EventArgs e)
    {
      panelMain.Focus();
      selection.Cancel();
      CancelPaste();
      selection.SelectAll();
    }
  }
}
