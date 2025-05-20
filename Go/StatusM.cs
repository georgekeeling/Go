using System;
using System.Windows.Forms;

namespace GoPlanner
{
  public class StatusM
  {
    private ToolStripTextBox StatusMessage;
    public StatusM(ToolStripTextBox StatusMessage)
    {
      this.StatusMessage = StatusMessage;
    }
    public void Set(string message)
    {
      StatusMessage.Text = message;
    }
    public void Clear ()
    {
      StatusMessage.Text = "";
    }
  }
}
