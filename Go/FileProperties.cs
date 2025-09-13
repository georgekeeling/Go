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
      TextKomi.Text = "" + parent.komi;
      TextHandicap.Text = "" + parent.handicap;
      textResult.Text = parent.gameResult;
      textRules.Text = parent.rules;
      TextPlayerBlack.Text = parent.playerBlack;
      TextPlayerWhite.Text = parent.playerWhite;
      textGameName.Text = parent.gameName;
      textAuthor.Text = parent.author;
      textWarnings.Text = parent.warnings;
      textWarnings.LinkClicked += GotoURL;
      DialogResult result = ShowDialog();
      if (result == DialogResult.OK)
      {
        parent.komi = double.Parse(TextKomi.Text);
        parent.handicap = int.Parse(TextHandicap.Text);
        parent.gameResult = textResult.Text;
        parent.rules = textRules.Text;
        parent.playerBlack = TextPlayerBlack.Text;
        parent.playerWhite = TextPlayerWhite.Text;
        parent.gameName = textGameName.Text;
        parent.author = textAuthor.Text;
      }
    }

    private void GotoURL(object sender, LinkClickedEventArgs e)
    {
      System.Diagnostics.Process.Start(e.LinkText);
    }
    private void TextKomi_TextChanged(object sender, System.EventArgs e)
    {
      double result = 0.0;
      if (!double.TryParse(TextKomi.Text, out result))
      {
        TextKomi.Text = "0.0";
      }
    }
    private void TextHandicap_TextChanged(object sender, System.EventArgs e)
    {
      int result = 0;
      if (!int.TryParse(TextHandicap.Text, out result))
      {
        TextHandicap.Text = "0";
      }
    }
  }
}
