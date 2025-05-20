using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Text;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GoPlanner
{
  public partial class GoPlanner // : Form is unecessary here
  {
    private Point firstPrisoner; // first prisoner added to prison
    private int CountCaptures(int x, int y)
    {
      // new stone added at x,y. Are any other stones captured as a result?
      // if so, add to prison
      // we must check each stone touching x,y
      byte suspectCol = (byte)(3 - thePoints[x, y].color);
      int captures = DetectCapture(x - 1, y, suspectCol, false);
      captures += DetectCapture(x + 1, y, suspectCol, false);
      captures += DetectCapture(x, y - 1, suspectCol, false);
      captures += DetectCapture(x, y + 1, suspectCol, false);
      return captures;
    }
    private int ImprisonCaptures(int x, int y)
    {
      // new stone added at x,y. Are any other stones captured as a result?
      // if so, add to prison
      // we must check each stone touching x,y
      byte suspectCol = (byte)(3 - thePoints[x,y].color);
      int captures = DetectCapture(x - 1, y, suspectCol, true);
      captures += DetectCapture(x + 1, y, suspectCol, true);
      captures += DetectCapture(x, y - 1, suspectCol, true);
      captures += DetectCapture(x, y + 1, suspectCol, true);
      return captures;
    }
    private int DetectCapture(int topX, int topY, byte suspectCol, bool reallyCapture)
    {
      // check if stone at x,y is in group that can be captured.
      int[] groupXYs = Enumerable.Repeat(-1, 800).ToArray(); // Each pair = x,y of stone in group
      int[] libsXY = Enumerable.Repeat(-1, 800).ToArray();   // Each pair = x,y of a liberty, to prevent double counting
      int liberties = 0;        // number of empty points touching group
      int captures = 0;
      if (topX < 0 || topY < 0 || topX >= bSide || topY >= bSide) { return 0; }
      CountLiberties(topX, topY, suspectCol, ref liberties, groupXYs, libsXY);
      if (liberties == 0)
      {
        // send all group to prison (it can be an empty group, which has 0 liberties inevitably)
        int i = 0;
        while (i < groupXYs.Length - 1 && groupXYs[i] != -1)
        {
          captures++;
          if (captures == 1)
          {
            firstPrisoner = new Point(groupXYs[i], groupXYs[i + 1]);
          }
          if (reallyCapture)
          {
            if (thePoints[groupXYs[i], groupXYs[i + 1]].color == 1)
            {
              capturedWhites++;
              ToolSCapturedWhites.Text = capturedWhites.ToString();
            }
            else
            {
              capturedBlacks++;
              ToolSCapturedBlacks.Text = capturedBlacks.ToString();
            }
            thePoints[groupXYs[i], groupXYs[i + 1]].color = 0;
            thePoints[groupXYs[i], groupXYs[i + 1]].moveAdded = 0;
            panelMain.Invalidate(PixRectFromBoardXY(groupXYs[i], groupXYs[i + 1]));
          }
          i += 2;
        }
      }
      return captures;
    }
    private int CountLiberties(int x, int y)
    {
      int liberties = 0;
      int[] groupXYs = Enumerable.Repeat(-1, 800).ToArray(); // Each pair = x,y of stone in group
      int[] libsXY = Enumerable.Repeat(-1, 800).ToArray();   // Each pair = x,y of a liberty, to prevent double counting
      CountLiberties(x, y, thePoints[x, y].color, ref liberties, groupXYs, libsXY);
      return liberties;
    }
    private void CountLiberties(int x, int y, byte stoneCol , ref int liberties, int[] groupXYs, int[] libsXY)
    {
      if (x < 0 || y < 0 || x >= bSide || y >= bSide) { return; }
      if (thePoints[x, y].color == 0)
      {
        int i = 0;
        while (i < libsXY.Length - 1)
        {
          if (libsXY[i] == x && libsXY[i + 1] == y)
          {
            // liberty already counted
            return;
          }
          if (libsXY[i] == -1)
          {
            // add to liberties
            libsXY[i] = x;
            libsXY[i + 1] = y;
            liberties++;
            return;
          }
          i += 2;
        }
        // "Should never get here, cause an exception";
        thePoints[21, 21].color = 9;
      }
      if (thePoints[x, y].color != stoneCol) return;
      if (thePoints[x, y].color == stoneCol)
      {
        int i = 0;
        while (i < groupXYs.Length - 1)
        {
          if (groupXYs[i] == x && groupXYs[i + 1] == y)
          {
            // stone already in group
            return;
          }
          if (groupXYs[i] == -1)
          {
            // add to group and check surrounding stones
            groupXYs[i] = x;
            groupXYs[i + 1] = y;
            CountLiberties(x - 1, y, stoneCol, ref liberties, groupXYs, libsXY);
            CountLiberties(x + 1, y, stoneCol, ref liberties, groupXYs, libsXY);
            CountLiberties(x, y - 1, stoneCol, ref liberties, groupXYs, libsXY);
            CountLiberties(x, y + 1, stoneCol, ref liberties, groupXYs, libsXY);
            return;
          }
          i += 2;
        }
      }

    }
  }
}
