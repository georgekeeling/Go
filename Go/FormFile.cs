using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using Windows.Devices.Bluetooth.Advertisement;
using Windows.Foundation;

namespace GoPlanner
{
  public partial class GoPlanner : Form
  {
    public string gameName = "";
    public string warnings = "";
    public string author = "";
    public string fileName = "";
    public string fileDirectory = "";
    public double komi = 0.0;
    public int handicap = 0;
    public string gameResult = "";
    public string rules = "";
    public string playerBlack = "";
    public string playerWhite = "";

    private AStone[,] saveInitialStones;
    private string savepatternName = "";
    private string savepatternDescription = "";
    private string saveauthor = "";

    FileLoadResults resultWindow = null;

    private void InitFiles()
    {
      saveInitialStones = CreateStones(bSide, bSide);
    }
    private void newToolStripMenuItem_Click(object sender, EventArgs e)
    {
      newToolStripButton_Click(sender, e);
    }
    private void newToolStripButton_Click(object sender, EventArgs e)
    {
      // For thread stuff see
      // https://stackoverflow.com/questions/79265469/how-to-make-a-c-sharp-winform-app-with-independent-windows/
      // more elaborate answers at
      // https://stackoverflow.com/questions/10769193/how-do-i-prevent-the-app-from-terminating-when-i-close-the-startup-form
      
      // ensure same settings exept location
      SaveSettings();
      Properties.Settings.Default.WindowLocation = 
        new System.Drawing.Point(Location.X + 100, Location.Y + 100);
      Properties.Settings.Default.Save();

      //Thread independent_thread = new Thread(NewFormStart);
      //independent_thread.SetApartmentState(ApartmentState.STA);
      //independent_thread.Start();

      // This way does not share static data. Also you can't debug it.
      System.Diagnostics.Process.Start(Application.ExecutablePath);
    }
    private void NewFormStart ()
    {
      Application.Run(new GoPlanner());
    }
    private bool DataChanged()
    {
      for (int X = 0; X < bSide; X++)
      {
        for (int Y = 0; Y < bSide; Y++)
        {
          if (thePoints[X, Y].EqualTo(saveInitialStones[X, Y]))
          {
            return true;
          }
        }
      } 
      if (gameName != savepatternName) return true;
      if (warnings != savepatternDescription) return true;
      if (author != saveauthor) return true;
      return false;
    }
    private void saveToolStripButton_Click(object sender, EventArgs e)
    {
      saveToolStripMenuItem_Click(sender, e);
    }
    private void saveToolStripMenuItem_Click(object sender, EventArgs e)
    {
      if (fileName == "" || !toolsOptions.enableSave)
      {
        saveAsToolStripMenuItem_Click(sender, e);
        return;
      }
      string fullFileName = fileDirectory + "\\" + fileName;
      if (!DataChanged())
      {
        return; // nothing changed so nothing to save.
      }
      SaveSGFfile(fullFileName);
    }
    private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
    {
      SaveFileDialog saveFileDialog1 = new SaveFileDialog();

      saveFileDialog1.Filter = "SGF files|*.sgf|All files|*.*";
      //saveFileDialog1.FilterIndex = 2;
      saveFileDialog1.RestoreDirectory = true;

      saveFileDialog1.FileName = fileName;

      if (saveFileDialog1.ShowDialog() == DialogResult.OK)
      {
        fileName = Path.GetFileName(saveFileDialog1.FileName);
        fileDirectory = Path.GetDirectoryName(saveFileDialog1.FileName);
        Text = programTitle + ": " + fileName;
        SaveSGFfile(saveFileDialog1.FileName);
      }
    }
    private void SaveSGFfile (string fullFileName)
    {
      SaveState("Save file start");
      try
      {
        StreamWriter theFile = new StreamWriter(fullFileName);

        theFile.Write("(;FF[4]GM[1]SZ[19]AP[GAKgo]");   // Standard preamble
        WritePreambleField("KM", komi.ToString());
        WritePreambleField("HA", handicap.ToString());
        WritePreambleField("RE", gameResult);
        WritePreambleField("RU", rules);
        WritePreambleField("PB", playerBlack);
        WritePreambleField("PW", playerWhite);
        WritePreambleField("GN", gameName);
        WritePreambleField("US", author);

        // Want to write stones in sequence they were put on board, including ones that were captured
        // and even possible large groups added or removed with Delete cut or paste
        // Start at undoObjects[1], compare with undoObjects[0], difference is move 1
        // next compare undoObjects[2] with undoObjects[1], difference is move 2
        // ...
        // finally compare thePoints with undoObjects[undos], difference is final move

        // first stick thePoints into undoObjects, done above (to be reversed at end)

        string simpleMove = "";     // use this if only one stone added
        string ABs = "";    // black stones added
        string AWs = "";    // white stones added
        string AEs = "";    // stones removed
        for (int iUndo = 0; iUndo < undos - 1; iUndo++)
        {
          simpleMove = GetDelta (undoObjects[iUndo], undoObjects[iUndo + 1], ref ABs, ref AWs, ref AEs);
          if (simpleMove != "None")
          {
            // simpleMove == "None" ⇒ no stones added or removed
            // ⇒ SaveState called twice somewhere back in the day
            theFile.Write(";");
            if (simpleMove != "")
            {
              theFile.Write(simpleMove);
            }
            else
            {
              theFile.Write(ABs + AWs + AEs);
            }
          }
        }

        theFile.WriteLine(")");
        theFile.Close();

        void WritePreambleField(string field, string value)
        {
          if (value != "")
          {
            value = value.Replace("[", "{");
            value = value.Replace("]", "}");
            theFile.Write(field + "[" + value + "]");
          }
        }
      }
      catch (Exception ex) 
      {
        new MyMessageBox(ex.Message, "Error", this);
      }
      UnSaveState("Save file end");
    }
    private string GetDelta(UndoRedoBuffer buf1, UndoRedoBuffer buf2, ref string ABs, ref string AWs, ref string AEs)
    {
      ABs = ""; AWs = ""; AEs = "";
      int captures = buf2.capturedBlacks + buf2.capturedWhites - buf1.capturedWhites - buf1.capturedBlacks;
      for (short x = 0; x < bSide; x++)
      {
        for (short y = 0; y < bSide; y++)
        {
          if (buf1.urPoints[x, y].color != buf2.urPoints[x, y].color)
          {
            if (buf2.urPoints[x, y].color == 1)
            {
              AWs += "[" + LetterPairfromXY(x, y) + "]";
            }
            else if (buf2.urPoints[x, y].color == 2)
            {
              ABs += "[" + LetterPairfromXY(x, y) + "]";
            }
            else
            {
              AEs += "[" + LetterPairfromXY(x, y) + "]";
            }
          }
        }
      }
      // Before finishing check for captures
      if (captures == AEs.Length / 4 && (AWs.Length + ABs.Length == 4))
      {
        // 1 added and captutes removed, therefore ordinary move with captures
        AEs = "";
      }

      if (AWs== "" && ABs == "" && AEs == "") { return "None"; }
      if (AWs.Length == 4 && ABs == "" && AEs == "") { return "W" + AWs; }
      if (ABs.Length == 4 && AWs == "" && AEs == "") { return "B" + ABs; }
      // Anything else is complicated, so need lists
      if (AWs != "") { AWs = "AW" + AWs; }
      if (ABs != "") { ABs = "AB" + ABs; }
      if (AEs != "") { AEs = "AE" + AEs; } 
      return "";      
    }
    private void openToolStripButton_Click(object sender, EventArgs e)
    {
      openToolStripMenuItem_Click(sender, e);
    }
    private bool CheckSafety (string action)
    {
      if (undos + redos > 0)
      {
        int reply = -1;
        string theMessage = "All previous undos / redos.";
        theMessage += "\r\nThis is not undoable!";
        theMessage += "\r\nFor safety use File / New, then " + action + ".";
        new MyMessageBox(theMessage, action + ": Warning", ref reply, "Continue", "Cancel", "", this);
        if (reply == 2) { return false; }
      }
      return true;
    }
    private void ClearBoard(string titleEnd)
    {
      InitUndoRedo();
      statusM.Clear();
      ScGui.ResetImage();
      thePoints = CreateStones(bSide, bSide);
      ToolSCapturedBlacks.Text = "0";
      ToolSCapturedWhites.Text = "0";
      capturedBlacks = 0;
      capturedWhites = 0;
      warnings = "";
      gameName = "";
      author = "";
      komi = 0.0;
      handicap = 0;
      gameResult = "";
      rules = "";
      playerWhite = "";
      playerBlack = "";
      Text = programTitle + ": " + titleEnd;
      selection.Off();
      panelMain.Invalidate();
      prSLider.Invalidate();
      // ********  save the initial state
      savepatternName = gameName;
      savepatternDescription = warnings;
      saveauthor = author;
    }
    private void openToolStripMenuItem_Click(object sender, EventArgs e)
    {
      // Show the Open File dialog. If the user clicks OK, load the
      // pattern 
      if (!CheckSafety("File / Open")) { return; }

      openFileDialog1.FileName = "";
      if (openFileDialog1.ShowDialog() == DialogResult.OK)
      {
        ReadFile1(openFileDialog1.FileName, false);
      }
      // panelMain.Focus();
    }
    public bool ReadFile1(string fullFileName, bool batch)
    {
      bool result = false;

      StreamReader theFile = new StreamReader(fullFileName);
      fileName = Path.GetFileName(fullFileName);
      fileDirectory = Path.GetDirectoryName(fullFileName);

      ClearBoard(fileName);

      result = ReadFile(theFile, batch);

      theFile.Close();

      return result;
    }
    private bool ReadFile(StreamReader theFile, bool batch)
    {
      // After flattening the file inot one longLine
      // we read the data and add it to the undo buffer as we go along.
      // So undo works as it would have worked in the saved file
      string line = theFile.ReadLine();
      string longLine = line;
      short moveNr = 0;

      if (resultWindow != null) { resultWindow.Results.Text = ""; }
      while (line != null)
      {
        line = theFile.ReadLine();
        longLine += line;
      }
      longLine += "   ";  // to simplify parsing

      SaveState("Read file 0");       // Save empty board to undo buffer
      bool moveProcessed = false;
      int ix = 0;
      string property = "";     // one or two characters
      string value = "";        // something in []
      while (ix < longLine.Length)
      {
        property = "";
        string theChar = longLine.Substring(ix, 1);
        if ("() ".Contains(theChar)) goto NextChar;
        if (theChar == ";")
        {
          // some stuff between semi-colons is not a move, most is
          if (moveProcessed) { SaveState("Read file n"); }
          goto NextChar;
        }
        if (theChar == "[")
        {
          // ignore to next "]" *** DONT THINK WE EVER GET HERE ***
          string ignored = "";
          int firstIx = ix;
          while (longLine.Substring(ix, 1) != "]")
          {
            ignored += longLine.Substring(ix, 1);
            ix++;
          }
          ignored += "]";
          FileWarning(ix, "Ignored " + ignored);
          goto NextChar;
        }
        if (longLine.Substring(ix + 1, 1) == "[")
        {
          // single character property
          property = theChar;
        }
        else
        {
          // must be double character property
          property = theChar + longLine.Substring(ix + 1, 1);
          if (longLine.Substring(ix + 2, 1) != "[")
          {
            // but there are exceptions
            if (longLine.Substring(ix, 4) == "Mark")
            {
              // In some Dyer files
              property = "Mark";
              ix += 2;
            }
            else if (longLine.Substring(ix, 6) == "Letter")
            {
              property = "Letter";
              ix += 4;
            }
            else
            {
              FileError(ix, "Property '" + longLine.Substring(ix, 10) + "' longer than 2 characters", batch);
              return false;
            }
          }
          ix++;
        }
        // Must have a value in brackets [...]
        ix += 2;      // on first character of value
        value = "";
        while (longLine.Substring(ix, 1) != "]")
        {
          value += longLine.Substring(ix, 1);
          ix++;
          if (longLine.Substring(ix, 1) == "\\")
          {
            // escape charachter, totally ignore it and ignore next (which might be "]")  .\]
            ix++;
            value += longLine.Substring(ix, 1);
            ix++;
          }
        }
        if (property == "" || value == "" && property != "VW")
        {
          // exceptionally VW[] is allowed
          FileError(ix, "Property or value missing", batch);
          return false;
        }

        // ix now on "]"
        switch (property)
        {
          case "GM":
            if (value != "1")
            {
              FileError(ix, "Only Game of Go supported", batch);
              return false;
            }
            break;
          case "FF":
            if (value != "4")
            {
              FileWarning(ix, "Only format 4 supported - may be inaccurate");
            }
            break;
          case "SZ":
            if (value != "19")
            {
              FileWarning (ix, "Only 19x19 supported");
            }
            break;
          case "AP":
            // name version of app, ignore
            break;
          case "GN":
            gameName = value;
            break;
          case "US":
            author = value;
            break;
          case "KM":
            try
            {
              komi = double.Parse(value);
            }
            catch (Exception ex)
            {
              FileWarning(ix, "Komi value not a number: " + ex.Message);
              komi = 0.0;
            }
            break;
          case "RE":
            gameResult = value;
            break;
          case "RU":
            rules = value;
            break;
          case "PB":
            playerBlack = value;
            break;
          case "PW":
            playerWhite = value;
            break;
          case "HA":
            try
            {
              handicap = int.Parse(value);
            }
            catch (Exception ex)
            {
              FileWarning(ix, "Handicap value not an integer: " + ex.Message);
              handicap = 0;
            }
            break;
          case "W":
          case "B":
            {
              int x = 0; int y = 0;
              if (XYfromLetterPair(ref x, ref y, value, ix))
              {
                moveProcessed = true;
                thePoints[x, y].color = PcolorFromAcolor(property[0]);
                thePoints[x, y].moveAdded = ++moveNr;
                ImprisonCaptures(x, y);
              }
            }
            break;
          case "AW":
          case "AB":
          case "AE":
            {
              // expect series of points like AB[ac][bc][cc][dc][ec][fc][gc][hc][ic]
              moveProcessed = true;
              do
              {
                int x = 0; int y = 0;
                if (XYfromLetterPair(ref x, ref y, value, ix))
                {
                  thePoints[x, y].color = PcolorFromAcolor(property[1]);
                  thePoints[x, y].moveAdded = ++moveNr;
                }
                if (longLine.Substring(ix + 1, 1) == "[")
                {
                  value = longLine.Substring(ix + 2, 2);
                  ix += 4;
                }
                else
                {
                  goto NextChar;
                }
              } while (true);
            }
          default:
            FileWarning(ix, "Ignored " + property + "[" + value + "]");
            break;
        }
      NextChar:
        ix++;
      }
      if (resultWindow != null)
      {
        warnings = resultWindow.Results.Text;
      }
      return true;
    }
    private string LetterPairfromXY (short x, short y)
    {
      char xLetter = (char)(x + 'a');
      char yLetter = (char)(y + 'a');
      return (xLetter.ToString() + yLetter.ToString());
    }
    private bool XYfromLetterPair (ref int x, ref int y, string letterPair, int ix)
    {
      // ab=0,1, ba=1,0 ...

      letterPair = letterPair.ToUpper();
      x = letterPair[0] - 65;
      y = letterPair[1] - 65;
      if (x < 0 || x >= bSide || y < 0 || y >= bSide) 
      {
        FileWarning(ix, "Stone coordinates [" + letterPair + "] out of range");
        return false; 
      }
      return true;
    }
    private byte PcolorFromAcolor (char aColor)
    {
      if (aColor == 'W') return 1;
      if (aColor == 'B') return 2;
      // Should be E for empty
      return 0;
    }
    private void FileError(int ix, string message, bool batch)
    {
      if (batch)
      {
        FileWarning(ix, "Error: " + message);
        return;
      }
      MyMessageBox myMessageBox = 
        new MyMessageBox("Error in file at position " + ix + "\r\n" + message, "File Error", this);
    }
    private void FileWarning(int ix, string message)
    {
      if (resultWindow == null || resultWindow.IsDisposed)
      {
        resultWindow = new FileLoadResults(this);
        resultWindow.Results.Text = "";
      }
      if (!resultWindow.Visible || resultWindow.Results.Text == "") 
      {
        resultWindow.Results.Text = "Warnings from file load of " + fileName;
        resultWindow.Show(); 
      }
      resultWindow.Results.Text += "\r\nPosition " + ix + ": " + message;
    }
    private void propertiesToolStripMenuItem_Click(object sender, EventArgs e)
    {
      new FileProperties(this);
    }
  }
}
