using System.Windows.Forms;

namespace GoPlanner
{
  public partial class FileProperties : Form
  {
    public FileProperties(GoPlanner parent)
    {
      int whites = 0; int blacks = 0;
      InitializeComponent();
      Owner = parent;
      StartPosition = FormStartPosition.CenterParent;
      textFileName.Text = parent.fileName;
      textDirectory.Text = parent.fileDirectory;
      textDimensions.Text = parent.bSide + " × " + parent.bSide;
      parent.StonesOnBoard(ref whites, ref blacks);
      textStones.Text = "Whites: " + whites + ". Blacks: " + blacks;
      textKomi.Text = "" + parent.komi;
      textHandicap.Text = "" + parent.handicap;
      textResult.Text = parent.gameResult;
      textRules.Text = parent.rules;
      textGameName.Text = parent.gameName;
      textAuthor.Text = parent.author;
      textWarnings.Text = parent.warnings;
      textWarnings.LinkClicked += GotoURL;
      DialogResult result = ShowDialog();
      if (result == DialogResult.OK)
      {
        parent.gameName = textGameName.Text;
        parent.author = textAuthor.Text;
      }
    }

    private void GotoURL(object sender, LinkClickedEventArgs e)
    {
      System.Diagnostics.Process.Start(e.LinkText);
    }
    private void ButtonOK_Click(object sender, System.EventArgs e)
    {

    }

  }
}
