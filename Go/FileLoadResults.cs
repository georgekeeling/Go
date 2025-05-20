using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GoPlanner
{
  public partial class FileLoadResults : Form
  {
    int width0 = 0;
    int height0 = 0;
    int width1 = 0;
    int height1 = 0;
    public FileLoadResults(GoPlanner parent)
    {
      InitializeComponent();
      Owner = parent;
      StartPosition = FormStartPosition.CenterParent;
      ResizeBegin += FileLoadResults_ResizeBegin;
      Resize += FileLoadResults_Resize;
      Show();
      Focus();
      Left = parent.Left + parent.Width / 2 - Width / 2;
      Top = parent.Top + parent.Height / 2 - Height / 2;
    }

    private void FileLoadResults_Resize(object sender, EventArgs e)
    {
      width1 = Width;
      height1 = Height;
      int heightDiff = height0 - height1;
      int widthDiff = width0 - width1;
      ButtonOK.Top -= heightDiff;
      ButtonOK.Left -= widthDiff;
      Results.Width -= widthDiff;
      Results.Height -= heightDiff;
      width0 = Width;
      height0 = Height;
    }

    private void FileLoadResults_ResizeBegin(object sender, EventArgs e)
    {
      width0 = Width;
      height0 = Height;
    }


    private void ButtonOK_Click(object sender, EventArgs e)
    {
      Visible = false;
    }

  }
}
