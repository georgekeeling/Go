using System.Windows.Forms;
using System.Drawing;
using System;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Tab;

// The Play Replay slider can be moved so that is shows plays in sequence
// At the left hand end no stones are shown at the RH end all uncaptured stones are shown

// question at
// https://stackoverflow.com/questions/79343033/add-panel-to-toolstrip-in-c-sharp-visual-studio
namespace GoPlanner
{
  public partial class GoPlanner : Form
  {
    private Panel prSLider;
    private bool sliderMouseDown = false;
    private long sliderMDtime;
    private int TS1fixedWidth = 0;

    private void InitPlayReplay()
    {
      // The Play Replay slider Panel
      prSLider = new Panel
      {
        Width = 50,
        Height = toolStrip1.Height,
        BackColor = Color.Beige
      };
      ToolStripControlHost host = new ToolStripControlHost(prSLider);
      toolStrip1.Items.Insert(toolStrip1.Items.Count - 2, host);
      host.AutoSize = false;
      TS1fixedWidth = toolStrip1.Items[toolStrip1.Items.Count - 1].Bounds.Right;
      CalcPrSliderlWidth();
      host.Width = prSLider.Width;

      prSLider.Paint += PrSlider_Paint;
      prSLider.MouseUp += PrSlider_MouseUp;
      prSLider.MouseDown += PrSLider_MouseDown;
      prSLider.MouseMove += PrSLider_MouseMove;
    }
    private void CalcPrSliderlWidth()
    {
      prSLider.Width = Width - TS1fixedWidth  - 100;
    }
    private void PrSlider_Paint(object sender, PaintEventArgs e)
    {
      // draw square with centre indicating undos / redos ratio
      // total width = undos + redos
      // distance from left = undos
      int side = toolStrip1.Height;
      int centreX = CalcXFromUndos();
      int x = centreX - side / 2;
      if (x < 0) { x = 0; }
      if (x > prSLider.Width - side) { x = prSLider.Width - side; }
      centreX = x + side / 2;
      Rectangle square = new Rectangle(x, 0, side, side);
      if (e.ClipRectangle.IntersectsWith(square))
      {
        Font myFont = new Font("Arial", 6);
        e.Graphics.FillRectangle(brushDarkWhite, square);
        //CalcVisibleMovesFromPos();
        string s = undos.ToString();
        SizeF size = e.Graphics.MeasureString(s, myFont);
        e.Graphics.DrawString(s, myFont, brushBlack, centreX - size.Width / 2, side / 2 - size.Height / 2);
      }

    }
    private void PrSlider_MouseUp(object sender, MouseEventArgs e)
    {
      long mouseWasDownMs = (DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond) - sliderMDtime;
      if (mouseWasDownMs > 400 && !sliderMouseDown) { return; }   // too slow to be click
      BigMove(e.X);
      sliderMouseDown = false;
    }
    private void BigMove(int x)
    {
      // if moving left need some undos, if moving right need some redos
      int newUndos = CalcUndosFromX(x);
      if (newUndos == undos) { return; }
      if (newUndos < undos)
      {
        UndoMultiple(undos - newUndos);
      }
      else
      {
        RedoMultiple(newUndos - undos);
      }
    }
    private void PrSLider_MouseMove(object sender, MouseEventArgs e)
    {
      if (sliderMouseDown) { BigMove(e.X); }
    }
    private void PrSLider_MouseDown(object sender, MouseEventArgs e)
    {
      // only interested if mouse goes down over slider
      sliderMouseDown = false;
      sliderMDtime = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
      int halfSide = toolStrip1.Height / 2;
      int newX = CalcXFromUndos();
      if (newX - halfSide < e.X && e.X < newX + halfSide) { sliderMouseDown = true; }
    }
    int CalcUndosFromX(int x)
    {
      // x = position of slider / mouse click
      // prSLider.Width ⇒ undos + redos
      float temp = ((float)((x * (undos + redos)))) / prSLider.Width;
      return (int)(0.5 + temp);
    }
    int CalcXFromUndos()
    {
      if (undos + redos == 0) { return prSLider.Width / 2; }
      float temp = ((float)(undos * prSLider.Width)) / (undos + redos);
      return (int)(0.5 + temp);
    }

  }
}