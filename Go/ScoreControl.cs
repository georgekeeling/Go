using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;

namespace GoPlanner
{
  public partial class GoPlanner : Form
  {
    private void InitScoreControl() 
    {
      TSBconnections.Paint += TSBconnections_Paint;
      TSBremovals.Paint += TSBremovals_Paint;
      TSBterritories.Paint += TSBterritories_Paint;
    }
    private void TSBterritories_Paint(object sender, PaintEventArgs e)
    {
      PaintButton(e, brushBlack);
    }
    private void TSBremovals_Paint(object sender, PaintEventArgs e)
    {
      PaintButton(e, brushRed);
    }
    private void TSBconnections_Paint(object sender, PaintEventArgs e)
    {
      PaintButton(e, brushLightBlack);
    }
    private void TSBconnections_Click(object sender, EventArgs e)
    {
      TSBconnections.Checked = !TSBconnections.Checked;
      toolsOptions.ShowConnections.Checked = TSBconnections.Checked;
    }
    private void TSBremovals_Click(object sender, EventArgs e)
    {
      TSBremovals.Checked = !TSBremovals.Checked;
      toolsOptions.ShowRemovals.Checked = TSBremovals.Checked;
    }
    private void TSBterritories_Click(object sender, EventArgs e)
    {
      TSBterritories.Checked = !TSBterritories.Checked;
      toolsOptions.ShowTerritory.Checked = TSBterritories.Checked;
    }
    private void PaintButton(PaintEventArgs e, Brush brush)
    {
      // very similar to DrawStar in PanelMain_Paint
      int xP = e.ClipRectangle.Width / 2;
      int yP = e.ClipRectangle.Height / 2;
      int starPoints = 10;        // double number of points on star
      int starRadius2 = Math.Min(xP, yP);
      int starRadius1 = starRadius2 / 2;
      Point[] star = new Point[starPoints];
      double angle = 0;
      for (int i = 0; i < starPoints; i++)
      {
        double r = (i % 2 == 0) ? starRadius1 : starRadius2;
        star[i].X = (int)(xP + r * Math.Cos(angle));
        star[i].Y = (int)(yP + r * Math.Sin(angle));
        angle += 2 * Math.PI / starPoints;
      }
      e.Graphics.FillPolygon(brush, star);
    }

    private void TSBscore_Click(object sender, EventArgs e)
    {
      ScoreToolStripMenuItem_Click(sender, e);
    }

    private void ScoreToolStripMenuItem_Click(object sender, EventArgs e)
    {
      double score = 0;
      int timeMs = 0;
      Color[,] saveT = new Color[bSide, bSide];
      Color[,] saveC = new Color[bSide, bSide];
      Color[,] saveR = new Color[bSide, bSide];
      if (!toolsOptions.ShowNone.Checked)
      {
        for (int x = 0; x < bSide; x++)
        {
          for (int y = 0; y < bSide; y++)
          {
            saveT[x, y] = ScGui.Territories[x, y];
            saveC[x, y] = ScGui.Connections[x, y];
            saveR[x, y] = ScGui.Removals[x, y];
          }
        }
      }
      ModScore.Shows = true;
      ModScore.ScoreCompute1(ref score, ref timeMs, this);
      double timeSec = timeMs / 1000.0;
      string timeString = " (" + timeSec.ToString("N1") + "s)";
      string scoreString = Math.Abs(score).ToString("N1");
      if (scoreString.EndsWith(".0"))
      {
        scoreString = scoreString.Substring(0, scoreString.Length - 2);
      }
      if (Math.Abs(score) == 1000)
      {
        scoreString = "lots";
      }
      if (score == double.MaxValue)
      {
        toolsOptions.ScoreResult.Text = ("Compute failed" + timeString);
      }
      else if (score > 0)
      {
        toolsOptions.ScoreResult.Text = ("Black wins by " + scoreString + timeString);
      }
      else if (score < 0)
      {
        toolsOptions.ScoreResult.Text = ("White wins by " + scoreString + timeString);
      }
      else
      {
        // score = 0
        toolsOptions.ScoreResult.Text = ("Draw" + timeString);
      }
      statusM.Set(toolsOptions.ScoreResult.Text);
      if (!toolsOptions.ShowNone.Checked)
      {
        for (int x = 0; x < bSide; x++)
        {
          for (int y = 0; y < bSide; y++)
          {
            if (toolsOptions.ShowTerritory.Checked && (saveT[x, y] != ScGui.Territories[x, y]))
            {
              panelMain.Invalidate(PixRectFromBoardXY(x, y));
            }
            if (toolsOptions.ShowConnections.Checked && (saveC[x, y] != ScGui.Connections[x, y]))
            {
              panelMain.Invalidate(PixRectFromBoardXY(x, y));
            }
            if (toolsOptions.ShowRemovals.Checked && (saveR[x, y] != ScGui.Removals[x, y]))
            {
              panelMain.Invalidate(PixRectFromBoardXY(x, y));
            }
          }
        }
      }
    }
  }
}
