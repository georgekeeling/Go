using System;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

namespace GoPlanner
{
  public partial class HelpAbout : Form
  {
    public HelpAbout(GoPlanner gp)
    {
      InitializeComponent();
      Owner = gp;
      StartPosition = FormStartPosition.CenterParent;
      Speech.LinkClicked += Speech_LinkClicked;
      CartaPaper.LinkClicked += Paper_Clicked;
      CartaCode.LinkClicked += Code_Clicked;
      Version.Text = "Version " + gp.programVersion + ", " + gp.programBuildDate;
      ShowDialog();
    }

    private void Speech_LinkClicked(object sender, LinkClickedEventArgs e)
    {
      System.Diagnostics.Process.Start(e.LinkText);
    }
    private void Paper_Clicked(object sender, LinkLabelLinkClickedEventArgs e)
    {
      System.Diagnostics.Process.Start("https://www.uni-trier.de/fileadmin/fb4/prof/BWL/FIN/Veranstaltungen/A_static_method_for_computing_the_score_of_a_Go_game__Carta_.pdf");
    }
    private void Code_Clicked(object sender, LinkLabelLinkClickedEventArgs e)
    {
      System.Diagnostics.Process.Start("https://github.com/Fantasio1960/Computing-Go-Scoring");
    }
  }
}
