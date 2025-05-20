using System.Windows.Forms;

namespace GoPlanner
{
  // MyMessageBox. Just like MessageBox except centers the box on the parent window / Form
  // multi line message should use \r\n for line separater
  public partial class MyMessageBox : Form
  {
    public MyMessageBox(string message, string title, Form parent)
    {
      InitializeComponent();
      Text = title;
      messageBox.Text = message;
      Owner = parent;
      StartPosition = FormStartPosition.CenterParent;
      button3.Visible = false;
      button2.Visible = false;
      buttonOK.Select();
      ShowDialog();
    }
    public MyMessageBox(string message, Form parent)
    {
      InitializeComponent();
      Text = "Error";
      messageBox.Text = message;
      Owner = parent;
      StartPosition = FormStartPosition.CenterParent;
      button3.Visible = false;
      button2.Visible = false;
      buttonOK.Select();
      ShowDialog();
    }
    public MyMessageBox(string message, string title, ref int reply, 
      string button3, string button2, string buttonOK, Form parent)
    {
      InitializeComponent();
      Text = title;
      messageBox.Text = message;
      Owner = parent;
      StartPosition = FormStartPosition.CenterParent;
      SetButton(button3, this.button3);
      SetButton(button2, this.button2);
      SetButton(buttonOK, this.buttonOK);
      this.buttonOK.Select();
      reply = (int)ShowDialog();
      // should be 3 for button3, 2 for button2, 1 for buttonOK, 
      // Must be set up in button properties also see
      // https://learn.microsoft.com/en-us/dotnet/api/system.windows.forms.dialogresult?view=windowsdesktop-8.0
    }
    private void SetButton (string buttonText, Button button) 
    {
      if (buttonText == "")
      {
        button.Visible = false;
      }
      else
      {
        button.Text = buttonText;
        button.Visible = true;
      }
    }
  }
}
