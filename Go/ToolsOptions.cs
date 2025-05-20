using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Windows.UI.Text;

namespace GoPlanner
{
  public partial class ToolsOptions : Form
  {
    private string[] chineseNumbers = { "一", "二", "三", "四", "五", "六 ", "七", "八", "九", "十",
    "十一", "十二", "十三", "十四", "十五", "十六 ", "十七", "十八", "十九"};
    private GoPlanner gp;
    public string[] topLabels;
    public string[] leftLabels;

    // control variables for labels, get saved between sessions
    public byte topScript = 2;  // 0 = none, 1 = numbers 1-19, 2 = letters, 3 = chinese, 4 = numbers 0-18
    private bool topFromLeft = true;
    private string omitLetters = "I";
    public byte leftScript = 1;
    private bool leftFromTop = false;

    private byte allRefs = 0;

    public bool showSequence;
    public bool enableSave;

    public ToolsOptions(GoPlanner parent)
    {
      InitializeComponent();
      Owner = parent;   // for benefit of FormStartPosition.CenterParent
      gp = parent;      // for many other uses. Has correct type
    }
    public void InitLabels()
    {
      topLabels = new string[gp.bSide];
      leftLabels = new string[gp.bSide];
      SetLabels(topLabels, topScript, topFromLeft);
      SetLabels(leftLabels, leftScript, leftFromTop);
    }
    public void ShowTools()
    {
      StartPosition = FormStartPosition.CenterParent;
      SetCombos();
      ShowDialog();
    }
    private void SetCombos()
    {
      TopScriptCombo.SelectedIndex = topScript;
      OmitLetters.Text = omitLetters;
      if (topFromLeft)
      {
        TopDirectionCombo.SelectedIndex = 0;
      }
      else
      {
        TopDirectionCombo.SelectedIndex = 1;
      }
      LeftScriptCombo.SelectedIndex = leftScript;
      if (leftFromTop)
      {
        LeftDirectionCombo.SelectedIndex = 0;
      }
      else
      {
        LeftDirectionCombo.SelectedIndex = 1;
      }
      EnableOmitLetters();
      DetectSetAllrefs();
      ShowSeqCheckBox.Checked = showSequence;
      StartBlack.Checked = Properties.Settings.Default.startBlack;
      StartAlternating.Checked = Properties.Settings.Default.startAlternating;
    }
    public void LoadOptions()
    {
      topScript = Properties.Settings.Default.topScript;        // = 0  if never set
      topFromLeft = Properties.Settings.Default.topFromLeft;    // = true if never set
      omitLetters = Properties.Settings.Default.omitLetters;    // = "" if never set
      leftScript = Properties.Settings.Default.leftScript;
      leftFromTop = Properties.Settings.Default.leftFromTop;

      showSequence = Properties.Settings.Default.showSequence;
      enableSave = Properties.Settings.Default.enableSave;
      if (!enableSave)
      {
        gp.saveToolStripMenuItem.Text = "Save As";
        gp.saveToolStripButton.ToolTipText = "Save As";
      }
      if (Properties.Settings.Default.startBlack)
      {
        gp.TSBblack.Checked = true;
        gp.TSBblack_Click(null, null);
      }
      else
      {
        gp.TSBwhite.Checked = true;
        gp.TSBwhite_Click(null,null);
      }
      if (Properties.Settings.Default.startAlternating)
      {
        gp.TSBblackWhite.Checked = true;
        gp.TSBblackWhite_Click(null, null);
      }
    }
    public void SaveOptions()
    {
      Properties.Settings.Default.topScript = topScript;
      Properties.Settings.Default.topFromLeft = topFromLeft;
      Properties.Settings.Default.omitLetters = omitLetters;
      Properties.Settings.Default.leftScript = leftScript;
      Properties.Settings.Default.leftFromTop = leftFromTop;

      Properties.Settings.Default.showSequence = showSequence;
      Properties.Settings.Default.enableSave = enableSave;

      Properties.Settings.Default.startBlack = StartBlack.Checked;
      Properties.Settings.Default.startAlternating = StartAlternating.Checked;
    }
    private void SetLabels(string[] labels, int whichScript, bool direction)
    {
      if (direction)
      {
        if (whichScript == 0)
        {
          for (int i = 0; i < gp.bSide; i++)
          {
            labels[i] = "";
          }
        }
        else if (whichScript == 1)
        {
          for (int i = 0; i < gp.bSide; i++)
          {
            labels[i] = (i + 1).ToString();
          }
        }
        else if (whichScript == 2)
        {
          int j = 0;
          for (int i = 0; i < gp.bSide; i++)
          {
            char letter = (char)('A' + j);
            while (omitLetters.Contains(letter.ToString()))
            {
              j++;
              letter = (char)('A' + j);
            }
            labels[i] = letter.ToString();
            j++;
          }
        }
        else if (whichScript == 3)
        {
          for (int i = 0; i < gp.bSide; i++)
          {
            labels[i] = chineseNumbers[i];
          }
        }
        else
        {
          for (int i = 0; i < gp.bSide; i++)
          {
            labels[i] = i.ToString();
          }
        }
      }
      else
      {
        if (whichScript == 0)
        {
          for (int i = 0; i < gp.bSide; i++)
          {
            labels[i] = "";
          }
        }
        else if (whichScript == 1)
        {
          for (int i = 0; i < gp.bSide; i++)
          {
            labels[gp.bSide - i - 1] = (i + 1).ToString();
          }
        }
        else if (whichScript == 2)
        {
          int j = 0;
          for (int i = 0; i < gp.bSide; i++)
          {
            char letter = (char)('A' + j);
            while (omitLetters.Contains(letter.ToString()))
            {
              j++;
              letter = (char)('A' + j);
            }
            labels[gp.bSide - i - 1] = letter.ToString();
            j++;
          }
        }
        else if (whichScript == 3)
        {
          for (int i = 0; i < gp.bSide; i++)
          {
            labels[gp.bSide - i - 1] = chineseNumbers[i];
          }
        }
        else
        {
          for (int i = 0; i < gp.bSide; i++)
          {
            labels[gp.bSide - i - 1] = i.ToString();
          }
        }
      }
    }
    private void EnableOmitLetters()
    {
      if (TopScriptCombo.SelectedIndex == 2 || LeftScriptCombo.SelectedIndex == 2)
      {
        OmitLetters.Enabled = true;
      }
      else
      {
        OmitLetters.Enabled = false;
      }
    }
    private void TopScriptCombo_SelectedIndexChanged(object sender, EventArgs e)
    {
      if (TopScriptCombo.SelectedIndex == topScript)
      {
        return;   // Duh, not changed
      }
      EnableOmitLetters();
      topScript = (byte)TopScriptCombo.SelectedIndex;
      DetectSetAllrefs();
      SetLabels(topLabels, topScript, topFromLeft);
      InvalidateTopBottom();
    }

    private void OmitLetters_TextChanged(object sender, EventArgs e)
    {
      if (OmitLetters.Text == omitLetters) { return; }
      OmitLetters.Text = OmitLetters.Text.ToUpper();
      omitLetters = OmitLetters.Text;
      DetectSetAllrefs();
      SetLabels(topLabels, topScript, topFromLeft);
      SetLabels(leftLabels, leftScript, leftFromTop);
      InvalidateTopBottom();
      InvalidateLeftRight();
    }
    private void TopDirectionCombo_SelectedIndexChanged(object sender, EventArgs e)
    {
      if ((TopDirectionCombo.SelectedIndex == 0) == topFromLeft) { return; }
      if (TopDirectionCombo.SelectedIndex == 0)
      {
        topFromLeft = true;
      }
      else
      {
        topFromLeft = false;
      }
      DetectSetAllrefs();
      SetLabels(topLabels, topScript, topFromLeft);
      InvalidateTopBottom();
    }
    private void LeftScriptCombo_SelectedIndexChanged(object sender, EventArgs e)
    {
      if (LeftScriptCombo.SelectedIndex == leftScript) { return; }
      EnableOmitLetters();
      leftScript = (byte)LeftScriptCombo.SelectedIndex;
      DetectSetAllrefs();
      SetLabels(leftLabels, leftScript, leftFromTop);
      InvalidateLeftRight();
    }
    private void LeftDirectionCombo_SelectedIndexChanged(object sender, EventArgs e)
    {
      if ((LeftDirectionCombo.SelectedIndex == 0) == leftFromTop) { return; }
      if (LeftDirectionCombo.SelectedIndex == 0)
      {
        leftFromTop = true;
      }
      else
      {
        leftFromTop = false;
      }
      DetectSetAllrefs();
      SetLabels(leftLabels, leftScript, leftFromTop);
      InvalidateLeftRight();
    }
    private void InvalidateLeftRight()
    {
      int cellSide = gp.PixelXYfromBoardXY(0);
      int squareSide = cellSide * (gp.bSide + 1);
      int stoneRadius = (int)(cellSide / 2.2);        // copied from PanelMain_Paint
      Rectangle rect = new Rectangle(0, 0, cellSide - stoneRadius, squareSide);
      gp.panelMain.Invalidate(rect);
      rect.X = squareSide - cellSide + stoneRadius;
      rect.Width += 3;  // avoids slight mess when showing chinese on right and 
                        // go into Options, amd have large right margin and then change to Arabic ...
      gp.panelMain.Invalidate(rect);
    }
    private void InvalidateTopBottom()
    {
      int cellSide = gp.PixelXYfromBoardXY(0);
      int squareSide = cellSide * (gp.bSide + 1);
      int stoneRadius = (int)(cellSide / 2.2);        // copied from PanelMain_Paint
      Rectangle rect = new Rectangle(0, 0, squareSide, cellSide - stoneRadius);
      gp.panelMain.Invalidate(rect);
      rect.Y = squareSide - cellSide + stoneRadius;
      gp.panelMain.Invalidate(rect);
    }

    private void AllRefsCombo_SelectedIndexChanged(object sender, EventArgs e)
    {
      if (AllRefsCombo.SelectedIndex == allRefs) { return; }
      byte allRefsWas = allRefs;
      allRefs = (byte)AllRefsCombo.SelectedIndex;
      switch (allRefs)
      {
        case 0:
          // user can not change to custom
          allRefs = allRefsWas;
          AllRefsCombo.SelectedIndex = allRefsWas;
          return;
        case 1:
          topScript = 0; topFromLeft = false; omitLetters = "";
          leftScript = 0; leftFromTop = false;
          break;
        case 2:
          topScript = 2; topFromLeft = true; omitLetters = "I";
          leftScript = 1; leftFromTop = false;
          break;
        case 3:
          topScript = 1; topFromLeft = true; omitLetters = "";
          leftScript = 3; leftFromTop = false;
          break;
        case 4:
          topScript = 2; topFromLeft = true; omitLetters = "";
          leftScript = 2; leftFromTop = true;
          break;
        case 5:
          topScript = 1; topFromLeft = true; omitLetters = "";
          leftScript = 1; leftFromTop = true;
          break;
        case 6:
          topScript = 4; topFromLeft = true; omitLetters = "";
          leftScript = 4; leftFromTop = true;
          break;
        case 7:
          topScript = 4; topFromLeft = true; omitLetters = "";
          leftScript = 4; leftFromTop = false;
          break;
      }
      SetCombos();
      SetLabels(topLabels, topScript, topFromLeft);
      SetLabels(leftLabels, leftScript, leftFromTop);
      InvalidateTopBottom();
      InvalidateLeftRight();
    }
    private void DetectAllRefs()
    {
      // AllRefsCombo
      // 0 Custom
      // 1 None
      // 2 Computer Standard
      // 3 IRL Standard(½ Chinese)
      // 4 Programmer A-Z
      // 5 Programmer 1-19
      // 6 Programmer 0-18
      // 7 Cartesian

      // scripts (top and left)
      // 0 None
      // 1 Arabic Numerals 1 - 19
      // 2 Roman letters
      // 3 Chinese Numerals
      // 4 Arabic Numerals 0 - 18
      if (topScript == 0 && leftScript == 0)
      {
        allRefs = 1; return;
      }
      if (topScript == 2 && topFromLeft && omitLetters == "I" && leftScript == 1 && !leftFromTop)
      {
        allRefs = 2; return;
      }
      if (topScript == 1 && topFromLeft && leftScript == 3 && !leftFromTop)
      {
        allRefs = 3; return;
      }
      if (topScript == 2 && topFromLeft && omitLetters == "" && leftScript == 2 && leftFromTop)
      {
        allRefs = 4; return;
      }
      if (topScript == 1 && topFromLeft && leftScript == 1 && leftFromTop)
      {
        allRefs = 5; return;
      }
      if (topScript == 4 && topFromLeft && leftScript == 4 && leftFromTop)
      {
        allRefs = 6; return;
      }
      if (topScript == 4 && topFromLeft && leftScript == 4 && !leftFromTop)
      {
        allRefs = 7; return;
      }
      allRefs = 0;
    }
    private void DetectSetAllrefs()
    {
      DetectAllRefs();
      AllRefsCombo.SelectedIndex = allRefs;
    }
    private void ShowSeqCheckBox_CheckedChanged(object sender, EventArgs e)
    {
      if (ShowSeqCheckBox.Checked == showSequence) { return; }
      showSequence = ShowSeqCheckBox.Checked;
      for (int x = 0; x < gp.bSide; x++)
      {
        for (int y = 0; y < gp.bSide; y++)
        {
          // invalidate cell if it is stoned
          if (gp.thePoints[x, y].color != 0 && gp.thePoints[x,y].moveAdded > 0)
          {
            gp.panelMain.Invalidate(gp.PixRectFromBoardXY(x, y));
          }
        }
      }
    }

    private void EnableSave_CheckedChanged(object sender, EventArgs e)
    {
      if (EnableSave.Checked == enableSave) { return; }
      enableSave = EnableSave.Checked;
      if (!enableSave)
      {
        gp.saveToolStripMenuItem.Text = "Save As";
        gp.saveToolStripButton.ToolTipText = "Save As";
      }
      else
      {
        gp.saveToolStripMenuItem.Text = "Save";
        gp.saveToolStripButton.ToolTipText = "Save";
      }

    }

    private void ShowNone_CheckedChanged(object sender, EventArgs e)
    {
      if (ShowNone.Checked) 
      { 
        if (ShowRemovals.Checked) { ShowRemovals.Checked = false; }
        if (ShowTerritory.Checked) { ShowTerritory.Checked = false; }
        if (ShowConnections.Checked) { ShowConnections.Checked = false; }
      }
    }
    private void ShowRemovals_CheckedChanged(object sender, EventArgs e)
    {
      if (ShowRemovals.Checked) ShowNone.Checked = false;
      gp.TSBremovals.Checked = ShowRemovals.Checked;
      for (int x = 0; x < gp.bSide; x++)
      {
        for (int y = 0; y < gp.bSide; y++)
        {
          // invalidate cell if it is stoned
          if (ScGui.Removals[x, y] != Color.Empty)
          {
            gp.panelMain.Invalidate(gp.PixRectFromBoardXY(x, y));
          }
        }
      }
    }
    private void ShowTerritory_CheckedChanged(object sender, EventArgs e)
    {
      if (ShowTerritory.Checked) ShowNone.Checked = false;
      gp.TSBterritories.Checked = ShowTerritory.Checked;
      for (int x = 0; x < gp.bSide; x++)
      {
        for (int y = 0; y < gp.bSide; y++)
        {
          // invalidate cell if it is stoned
          if (ScGui.Territories[x, y] != Color.Empty)
          {
            gp.panelMain.Invalidate(gp.PixRectFromBoardXY(x, y));
          }
        }
      }
    }
    private void ShowConnections_CheckedChanged(object sender, EventArgs e)
    {
      if (ShowConnections.Checked) ShowNone.Checked = false;
      gp.TSBconnections.Checked = ShowConnections.Checked;
      for (int x = 0; x < gp.bSide; x++)
      {
        for (int y = 0; y < gp.bSide; y++)
        {
          // invalidate cell if it is stoned
          if (ScGui.Connections[x, y] != Color.Empty)
          {
            gp.panelMain.Invalidate(gp.PixRectFromBoardXY(x, y));
          }
        }
      }

    }
  }
}
