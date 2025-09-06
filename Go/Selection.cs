using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Serialization;

namespace GoPlanner
{
  internal class Selection
  {
    readonly GoPlanner form1;
    // TL, BR of points selected. Selection rectangle is cellWidth/2 bigger on each side
    // note that values range 0-18, not 1-19 as shown on board
    private int TLselX = int.MaxValue;      // indicates selection coordinates invalid
    private int TLselY;
    private int BRselX = int.MaxValue;      // ditto
    private int BRselY;

    public AStone[,] movePoints;

    readonly private Pen selPen;

    public Selection(GoPlanner form1)
    {
      this.form1 = form1;
      selPen = new Pen(Brushes.White, 1);
    }
    public bool Exists()
    {
      return !(TLselX == int.MaxValue);
    }
    public int TLx() { return TLselX; }
    public int TLy() { return TLselY; }
    public int BRx() { return BRselX; }
    public int BRy() { return BRselY; }
    public void UpdateRect(int TLx, int TLy, int BRx, int BRy)
    {
      // update selection rectangle with new TLBR coordinates
      int newTLx = form1.BoardXYfromPixelXY(TLx);
      int newTLy = form1.BoardXYfromPixelXY(TLy);
      int newBRx = form1.BoardXYfromPixelXY(BRx);
      int newBRy = form1.BoardXYfromPixelXY(BRy);
      if ((newBRx == BRselX) && (newBRy == BRselY) && 
        (newTLx == TLselX) && (newTLy == TLselY)) return;
      if (BRselX != int.MaxValue)
      {
        Invalidate();
      }
      TLselX = newTLx;
      TLselY = newTLy;
      BRselX = newBRx;
      BRselY = newBRy;
      Invalidate();
      if ((BRselX >= TLselX) && (BRselY >= TLselY))
      {
        form1.EnableCutCopy(true);
      }
      else
      {
        form1.EnableCutCopy(false);
      }
    }
    public void StartDrag()
    {
      movePoints = form1.CreateStones(BRselX - TLselX + 1, BRselY - TLselY + 1);
      for (int Cx = TLselX; Cx <= BRselX; Cx++)
      {
        for (int Cy = TLselY; Cy <= BRselY; Cy++)
        {
          movePoints[Cx - TLselX, Cy - TLselY].color = form1.thePoints[Cx, Cy].color;
          movePoints[Cx - TLselX, Cy - TLselY].moveAdded = form1.thePoints[Cx, Cy].moveAdded;
          if (form1.thePoints[Cx, Cy].color > 0)
          {
            form1.thePoints[Cx, Cy].color |= 0x80;
          }
        }
      }
    }
    public bool Cancel()
    {
      if (TLselX != int.MaxValue)
      {
        // remove selection rectangle
        if ((BRselX >= TLselX) && (BRselY >= TLselY))
        {
          Invalidate();
          Off();
          return true;  // there was a selection to cancel
        }
      }
      Off();
      return false;   // there wasn't a selection to cancel
    }
    public void Off()
    {
      if ((TLselX != int.MaxValue) && form1.cutting)
      {
        // reinstate cut data
        for (int Cx = TLselX; Cx <= BRselX; Cx++)
        {
          for (int Cy = TLselY; Cy <= BRselY; Cy++)
          {
            byte age = form1.thePoints[Cx, Cy].color;
            if (age > 2)
            {
              age &= 0x7F;
              form1.thePoints[Cx, Cy].color = age;
              form1.panelMain.Invalidate(form1.PixRectFromBoardXY (Cx, Cy));
            }
          }
        }
      }
      TLselX = int.MaxValue;
      BRselX = int.MaxValue;
      form1.EnableCutCopy(false);
    }
    public void SelectAll()
    {
      TLselX = 0;
      TLselY = 0;
      BRselX = form1.bSide - 1;
      BRselY = form1.bSide - 1;
      Invalidate();
      form1.EnableCutCopy(true);
    }
    public void DrawSelRectagle (PaintEventArgs e)
    {
      if (TLselX != int.MaxValue)
      {
        selPen.Color = Color.Red;
        selPen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
        int cellSize = form1.PixelXYfromBoardXY(0);
        int width = form1.PixelXYfromBoardXY(BRselX - TLselX);
        int height = form1.PixelXYfromBoardXY(BRselY - TLselY);
        e.Graphics.DrawRectangle(selPen, form1.PixelXYfromBoardXY(TLselX) - cellSize / 2,
          form1.PixelXYfromBoardXY(TLselY) - cellSize / 2, width, height);
      }
    }
    public void Invalidate()
    {

      // Invalidate 4 sides separately 
      // results in one call to PanelMain_Paint taking 5 to 15 ms, no flickering 
      // left side
      Rectangle selRect = GetSelRect();
      Rectangle rc = new Rectangle(selRect.X, selRect.Y, 1, selRect.Height);
      form1.panelMain.Invalidate(rc);
      // top side 
      rc.Width = selRect.Width;
      rc.Height = 1;
      form1.panelMain.Invalidate(rc);
      // bottom
      rc.Y += selRect.Height;
      form1.panelMain.Invalidate(rc);
      // right
      rc.X += selRect.Width;
      rc.Y -= selRect.Height;
      rc.Width = 1;
      rc.Height = selRect.Height;
      form1.panelMain.Invalidate(rc);

      // Invalidate all at once
      // results in one call to PanelMain_Paint taking 2 to 12 ms, flickering appearance
      //Rectangle bigRC = new Rectangle(form1.cells.CellToPixelX(TLdragCx),
      //  form1.cells.CellToPixelY(TLdragCy), width, height);
      //form1.panelMain.Invalidate(bigRC);
    }
    public void InvalidateWholeArea()
    {
      // this is unselective and causes flicker. Do not use!
      if ((BRselX >= TLselX) && (BRselY >= TLselY))
      {
        form1.panelMain.Invalidate(GetSelRect());
      }
    }
    private Rectangle GetSelRect()
    {
      int wh = form1.PixelXYfromBoardXY(0);
      int width = form1.PixelXYfromBoardXY(BRselX) - form1.PixelXYfromBoardXY(TLselX) + wh;
      int height = form1.PixelXYfromBoardXY(BRselY) - form1.PixelXYfromBoardXY(TLselY) + wh;
      Rectangle rc = new Rectangle(form1.PixelXYfromBoardXY(TLselX) - wh / 2,
        form1.PixelXYfromBoardXY(TLselY) - wh / 2, width, height);
      return rc;
    }
  }
}
