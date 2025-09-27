// using GoCartaDLL;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;

namespace GoPlanner
{
  public partial class GoPlanner : Form
  {
    private void TestToolStripMenuItem_Click(object sender, EventArgs e)
    {
      TestNumbers();
    }
    private void TestNumbers ()
    {
      double score = -1000;
      string scoreString = Math.Abs(score).ToString("N1");
      if (scoreString.EndsWith(".0"))
      {
        scoreString = scoreString.Substring(0, scoreString.Length - 2);
      }
      if (Math.Abs(score) == 1000)
      {
        scoreString = "lots";
      }
      statusM.Set("score " + scoreString);
    }
    private void TestBouzy()
    {
      int timeMs = 0;
      ModScore.Bouzy1(ref timeMs, this);
      statusM.Set("Bouzy in " + timeMs + " ms");
    }
    private void TestScoreVB()
    {
      double score = 0;
      int timeMs = 0;
      TestVBscore(ref score, ref timeMs);
      if (score > 0)
      {
        statusM.Set("Black wins by " + score + " (" + timeMs + " ms)");
      }
      else if (score < 0)
      {
        statusM.Set("White wins by " + -score + " (" + timeMs + " ms)");
      }
      else
      {
        statusM.Set("Draw (" + timeMs + " ms)");
      }
    }
    public void TestVBscore(ref double score, ref int timeMs)
    {
      Stopwatch stopWatch = new Stopwatch();
      stopWatch.Start();
      // japanese = true when territory scoring, = false when area scoring
      // komi = komi's value
      // hc = number of handicap stones (0 = no handicap)
      // translate for ScoreCompute
      // Board: 0 = empty intersection; 1 = black stone; -1 = white stone
      // thePoints.color: 0 blank, 1 white, 2 black, top bit set = pasting
      int[,] Board = new int[bSide, bSide];
      int whitemoves = capturedWhites, blackmoves = capturedBlacks;
      for (int i = 0; i < bSide; i++)
      {
        for (int j = 0; j < bSide; j++)
        {
          int code = thePoints[i, j].color & 0x7F;
          switch (code)
          {
            case 0:
              Board[i, j] = 0;
              break;
            case 1:
              Board[i, j] = -1;
              whitemoves++;
              break;
            case 2:
              Board[i, j] = 1;
              blackmoves++;
              break;
            default:
              // Error: code should be 0, 1, or 2
              Console.WriteLine("Error in testVBscore: invalid color code");
              break;
          }
        }
      }
      // score = VBdll.ScoreCompute(18, true, komi, handicap , whitemoves, blackmoves, Board);
      stopWatch.Stop();
      TimeSpan ts = stopWatch.Elapsed;
      timeMs = ts.Seconds * 1000 + ts.Milliseconds;


      //Console.WriteLine("ScoreCompute " + timeMs + " ms");
      //int reply = 9;
      //new MyMessageBox("Score = " + score.ToString("0.##"), "Carta score", ref reply, "", "", "OK", this);
    }
    public void BatchCompareCvsVB()
    {
      ScoreResults sr = new ScoreResults();
      sr.Show();
      sr.TheScores.Clear();
      sr.TheScores.Text = "File\tC# score\tC# ms\tVB score\tVB ms\tdiff\r\n";
      string[]files = loadFiles();
      string folder = "C:\\Users\\georg\\My Drive\\Personal\\Godel Escher Bach\\Games\\Go\\dyer x Carta\\";
      int filesToDo = files.Length;
      // filesToDo = 10;
      for (int i = 0; i < filesToDo; i++)
      {
        string fullFileName = folder + files[i];
        try
        {
          if ( ReadFile1 (fullFileName, true))
          {
            double scoreC = 0;
            int timeMsC = 0;
            // testVBscore(ref score, ref timeMs);
            ModScore.ScoreCompute1(ref scoreC, ref timeMsC, this);
            double scoreVB = 0;
            int timeMsVB = 0;
            TestVBscore(ref scoreVB, ref timeMsVB);
            sr.TheScores.AppendText(files[i] + "\t" + scoreC + "\t" + timeMsC + "\t" + 
              scoreVB + "\t" + timeMsVB + "\t" + (scoreC - scoreVB) + "\r\n");
          }
          else
          {
            sr.TheScores.AppendText(files[i] + "\t" + "Error reading file" + "\r\n");
          }
        }
        catch (Exception ex)
        {
          sr.TheScores.AppendText(files[i] + "\t" + ex.Message + "\r\n");
        }
      }
    }
    public void BatchTest()
    {
      ScoreResults sr = new ScoreResults();
      sr.Show();
      sr.TheScores.Clear();
      sr.TheScores.Text = "File\tf result\tvb score\tvb ms\trules\r\n";
      string[] files = loadFiles();
      string folder = "C:\\Users\\georg\\My Drive\\Personal\\Godel Escher Bach\\Games\\Go\\dyer x Carta\\";
      int filesToDo = files.Length;
      // filesToDo = 10;
      for (int i = 0; i < filesToDo; i++)
      {
        string fullFileName = folder + files[i];
        try
        {
          if (ReadFile1(fullFileName, true))
          {
            double score = 0;
            int timeMs = 0;
            TestVBscore(ref score, ref timeMs);
            sr.TheScores.AppendText(files[i] + "\t" + gameResult + "\t" + score +
              "\t" + timeMs + "\t" + rules + "\r\n");
          }
          else
          {
            sr.TheScores.AppendText(files[i] + "\t" + "Error reading file" + "\r\n");
          }
        }
        catch (Exception ex)
        {
          sr.TheScores.AppendText(files[i] + "\t" + ex.Message + "\r\n");
        }
      }
    }
    private string[] loadFiles()
    {
      // files_carta and files_dwyer are the same file names, the latter have been removed
      string[] files_carta =
{
"001_001.sgf",
"003_006.sgf",
"003_010.sgf",
"004_015.sgf",
"005_016.sgf",
"005_018.sgf",
"006_022.sgf",
"007_025.sgf",
"007_026.sgf",
"009_030.sgf",
"010_041.sgf",
"012_050.sgf",
"014_062.sgf",
"017_078.sgf",
"019_076.sgf",
"021_096.sgf",
"022_101.sgf",
"024_097.sgf",
"027_115.sgf",
"027_117.sgf",
"031_138.sgf",
"032_143.sgf",
"036_165.sgf",
"037_170.sgf",
"038_175.sgf",
"040_185.sgf",
"041_189.sgf",
"043_199.sgf",
"048_225.sgf",
"050_235.sgf",
"10TH-NEC-CUP-SEMI-FINAL-1.sgf",
"10TH-NEC-CUP-SEMI-FINAL-2.sgf",
"13TH-KAKU-SEI-TOUR-SEMI-FINAL-2.sgf",
"14TH-GOSEI-CHALLENGER-FINAL.sgf",
"14TH-GOSEI-CHALLNGER-SEMI-FINAL-2.sgf",
"14TH-GOSEI-TITLE-MATCH-GAME-3.sgf",
"14TH-GOSEI-TOURMENT-RD-1-#10.sgf",
"14TH-GOSEI-TOURMENT-RD-1-#12.sgf",
"14TH-GOSEI-TOURMENT-RD-1-#15.sgf",
"14TH-GOSEI-TOURMENT-RD-1-#8.sgf",
"14TH-GOSEI-TOURMENT-RD-1-#9.sgf",
"14TH-GOSEI-TOURMENT-RD-2-#3.sgf",
"14TH-GOSEI-TOURMENT-RD-2-#5.sgf",
"14TH-GOSEI-TOURMENT-RD-3-#1.sgf",
"14TH-GOSEI-TOURMENT-RD-3-#4.sgf",
"14TH-KISEI-(ALL-DAN)-GAME-6.sgf",
"14TH-KISEI-(TOP)-1ST-RD-GAME-2.sgf",
"14TH-KISEI-(TOP)-QTR-FINAL-2.sgf",
"14TH-KISEI-(TOP)-QTR-FINAL-3.sgf",
"14TH-KISEI-(TOP)-SEMI-FINAL-2.sgf",
"14TH-KISEI-2-DAN-FINAL.sgf",
"14TH-KISEI-7-DAN-FINAL.sgf",
"14TH-MEIJIN-LEAGUE-GAME-14.sgf",
"14TH-MEIJIN-LEAGUE-GAME-15.sgf",
"14TH-MEIJIN-LEAGUE-GAME-2.sgf",
"14TH-MEIJIN-LEAGUE-GAME-23.sgf",
"14TH-MEIJIN-LEAGUE-GAME-24.sgf",
"14TH-MEIJIN-LEAGUE-GAME-28.sgf",
"14TH-MEIJIN-LEAGUE-GAME-30.sgf",
"14TH-MEIJIN-LEAGUE-GAME-32.sgf",
"14TH-MEIJIN-LEAGUE-GAME-33.sgf",
"14TH-MEIJIN-LEAGUE-GAME-36.sgf",
"14TH-MEIJIN-LEAGUE-GAME-6.sgf",
"14TH-MEIJIN-LEAGUE-GAME-7.sgf",
"15TH-GOSEI-CHALLNGER-SEMI-FINAL-1.sgf",
"15TH-GOSEI-TOURMENT-RD-1-#1.sgf",
"15TH-GOSEI-TOURMENT-RD-1-#3.sgf",
"15TH-GOSEI-TOURMENT-RD-1-#6.sgf",
"15TH-GOSEI-TOURMENT-RD-1-#7.sgf",
"15TH-GOSEI-TOURMENT-RD-2-#2.sgf",
"15TH-GOSEI-TOURMENT-RD-2-#6.sgf",
"15TH-GOSEI-TOURMENT-RD-2-#7.sgf",
"15TH-GOSEI-TOURMENT-RD-3-#3.sgf",
"15TH-GOSEI-TOURMENT-RD-3-#4.sgf",
"15TH-KISEI-(ALL-DAN)-GAME-6.sgf",
"15TH-KISEI-(TOP)-1ST-RD-GAME-4.sgf",
"15TH-KISEI-(TOP)-1ST-RD-GAME-6.sgf",
"15TH-KISEI-(TOP)-SEMI-FINAL-1.sgf",
"15TH-KISEI-(TOP)-SEMI-FINAL-2.sgf",
"15TH-KISEI-CHALLENGER-GAME-3.sgf",
"15TH-KISEI-TITLE-MATCH-GAME-1.sgf",
"15TH-KISEI-TITLE-MATCH-GAME-2.sgf",
"15TH-KISEI-TITLE-MATCH-GAME-5.sgf",
"15TH-KISEI-TITLE-MATCH-GAME-6.sgf",
"15TH-KISEI-TITLE-MATCH-GAME-7.sgf",
"15TH-MEIJIN-LEAGUE-ENTRANT-2.sgf",
"15TH-MEIJIN-LEAGUE-GAME-10.sgf",
"15TH-MEIJIN-LEAGUE-GAME-11.sgf",
"15TH-MEIJIN-LEAGUE-GAME-12.sgf",
"15TH-MEIJIN-LEAGUE-GAME-14.sgf",
"15TH-MEIJIN-LEAGUE-GAME-16.sgf",
"15TH-MEIJIN-LEAGUE-GAME-17.sgf",
"15TH-MEIJIN-LEAGUE-GAME-19.sgf",
"15TH-MEIJIN-LEAGUE-GAME-20.sgf",
"15TH-MEIJIN-LEAGUE-GAME-21.sgf",
"15TH-MEIJIN-LEAGUE-GAME-23.sgf",
"15TH-MEIJIN-LEAGUE-GAME-28.sgf",
"15TH-MEIJIN-LEAGUE-GAME-3.sgf",
"15TH-MEIJIN-LEAGUE-GAME-31.sgf",
"15TH-MEIJIN-LEAGUE-GAME-35.sgf",
"15TH-MEIJIN-LEAGUE-GAME-36.sgf",
"15TH-MEIJIN-LEAGUE-GAME-8.sgf",
"15TH-TENGEN-CHALLENGER-FINAL.sgf",
"15TH-TENGEN-TOURMNT-RD-1-#12.sgf",
"15TH-TENGEN-TOURMNT-RD-1-#2.sgf",
"15TH-TENGEN-TOURMNT-RD-1-#7.sgf",
"15TH-TENGEN-TOURMNT-RD-2-#4.sgf",
"15TH-TENGEN-TOURMNT-RD-2-#5.sgf",
"16TH OZA #1.sgf",
"16TH OZA #2.sgf",
"16TH OZA P-119.sgf",
"16TH-GOSEI-CHALLENGER-FINAL.sgf",
"16TH-GOSEI-TITLE-MATCH-GAME-1.sgf",
"16TH-GOSEI-TITLE-MATCH-GAME-4.sgf",
"16TH-GOSEI-TOURMENT-RD-1-#12.sgf",
"16TH-GOSEI-TOURMENT-RD-1-#15.sgf",
"16TH-GOSEI-TOURMENT-RD-1-#3.sgf",
"16TH-GOSEI-TOURMENT-RD-1-#4.sgf",
"16TH-GOSEI-TOURMENT-RD-1-#5.sgf",
"16TH-GOSEI-TOURMENT-RD-2-#2.sgf",
"16TH-GOSEI-TOURMENT-RD-3-#4.sgf",
"16TH-KISEI-(ALL-DAN)-GAME-4.sgf",
"16TH-KISEI-(TOP)-1ST-RD-GAME-6.sgf",
"16TH-KISEI-(TOP)-1ST-RD-GAME-7.sgf",
"16TH-KISEI-(TOP)-QTR-FINAL-2.sgf",
"16TH-KISEI-(TOP)-QTR-FINAL-3.sgf",
"16TH-KISEI-(TOP)-QTR-FINAL-4.sgf",
"16TH-KISEI-1-DAN-FINAL.sgf",
"16TH-KISEI-3-DAN-FINAL.sgf",
"16TH-KISEI-CHALLENGER-GAME-1.sgf",
"16TH-KISEI-CHALLENGER-GAME-2.sgf",
"16TH-KISEI-CHALLENGER-GAME-3.sgf",
"16TH-KISEI-TITLE-MATCH-GAME-2.sgf",
"16TH-KISEI-TITLE-MATCH-GAME-3.sgf",
"16TH-KISEI-TITLE-MATCH-GAME-4.sgf",
"16TH-KISEI-TITLE-MATCH-GAME-5.sgf",
"16TH-KISEI-TITLE-MATCH-GAME-6.sgf",
"16TH-KIWANG-LEAGUE-ENTRANT-1.sgf",
"16TH-KIWANG-LEAGUE-ENTRANT-2.sgf",
"16TH-KIWANG-LEAGUE-GAME-1.sgf",
"16TH-KIWANG-LEAGUE-GAME-19.sgf",
"16TH-KIWANG-LEAGUE-GAME-24.sgf",
"16TH-KIWANG-LEAGUE-GAME-26.sgf",
"16TH-KIWANG-LEAGUE-GAME-3.sgf",
"16TH-KIWANG-LEAGUE-GAME-4.sgf",
"16TH-KIWANG-TITLE-MATCH-GAME-1.sgf",
"16TH-KIWANG-TITLE-MATCH-GAME-3.sgf",
"16TH-MEIJIN-LEAGUE-ENTRANT-1.sgf",
"16TH-MEIJIN-LEAGUE-ENTRANT-2.sgf",
"16TH-MEIJIN-LEAGUE-GAME-10.sgf",
"16TH-MEIJIN-LEAGUE-GAME-17.sgf",
"16TH-MEIJIN-LEAGUE-GAME-20.sgf",
"16TH-MEIJIN-LEAGUE-GAME-24.sgf",
"16TH-MEIJIN-LEAGUE-GAME-25.sgf",
"16TH-MEIJIN-LEAGUE-GAME-27.sgf",
"16TH-MEIJIN-LEAGUE-GAME-4.sgf",
"16TH-MEIJIN-LEAGUE-GAME-5.sgf",
"16TH-MEIJIN-LEAGUE-GAME-6.sgf",
"16TH-MEIJIN-LEAGUE-GAME-7.sgf",
"16TH-MEIJIN-LEAGUE-GAME-8.sgf",
"16TH-MEIJIN-LEAGUE-GAME-9.sgf",
"16TH-MEIJIN-TITLE-MATCH-GAME-1.sgf",
"16TH-MEIJIN-TITLE-MATCH-GAME-2.sgf",
"16TH-MEIJIN-TITLE-MATCH-GAME-5.sgf",
"16TH-OZA-P-120.sgf",
"16TH-TENGEN-CH'NGER-SEMI-FINAL-2.sgf",
"16TH-TENGEN-TITLE-MATCH-GAME-2.sgf",
"16TH-TENGEN-TITLE-MATCH-GAME-4.sgf",
"16TH-TENGEN-TOURMNT-RD-1-#15.sgf",
"16TH-TENGEN-TOURMNT-RD-1-#2.sgf",
"16TH-TENGEN-TOURMNT-RD-1-#3.sgf",
"16TH-TENGEN-TOURMNT-RD-1-#9.sgf",
"16TH-TENGEN-TOURMNT-RD-2-#1.sgf",
"16TH-TENGEN-TOURMNT-RD-2-#2.sgf",
"16TH-TENGEN-TOURMNT-RD-2-#5.sgf",
"16TH-TENGEN-TOURMNT-RD-3-#3.sgf",
"17TH-MEIJIN-LEAGUE-ENTRANT-2.sgf",
"17TH-TENGEN-TITLE-MATCH-GAME-2.sgf",
"17TH-TENGEN-TITLE-MATCH-GAME-4.sgf",
"17TH-TENGEN-TOURMNT-RD-1-#1.sgf",
"17TH-TENGEN-TOURMNT-RD-1-#14.sgf",
"17TH-TENGEN-TOURMNT-RD-1-#7.sgf",
"17TH-TENGEN-TOURMNT-RD-1-#8.sgf",
"17TH-TENGEN-TOURMNT-RD-2-#6.sgf",
"17TH-TENGEN-TOURMNT-RD-2-#8.sgf",
"17TH-TENGEN-TOURMNT-RD-3-#2.sgf",
"17TH-TENGEN-TOURMNT-RD-3-#3.sgf",
"1971-HONINBO-GAME-2.sgf",
"1971-HONINBO-GAME-6.sgf",
"1971-HONINBO-LEAGUE-HOSAI-ISHIDA.sgf",
"1971-HONINBO-LEAGUE-KANO-ISHIDA.sgf",
"1971-HONINBO-LEAGUE-KATO-ISHIDA.sgf",
"1971-HONINBO-LEAGUE-SAKATA-ISHIDA.sgf",
"1974-MEIJIN-6.sgf",
"1977-KISEI-GAME-3.sgf",
"1977-KISEI-GAME-4.sgf",
"1977-OTEAI.sgf",
"1980-KISEI-PRELIM-OTAKE-RIN.sgf",
"1980-MEIJIN-LEAGUE-1980-1.sgf",
"1980-MEIJIN-LEAGUE-1980-2.sgf",
"1980-MEIJIN-LEAGUE-1980-4.sgf",
"1980-MEIJIN-LEAGUE-1980-7.sgf",
"1980-MEIJIN-LEAGUE-1980-8.sgf",
"1981-GOSEI-GAME-1.sgf",
"1981-GOSEI-GAME-4.sgf",
"1981-HONINBO-GAME-6.sgf",
"1981-MEIJIN-GAME-1.sgf",
"1981-MEIJIN-GAME-2.sgf",
"1981-MEIJIN-GAME-3.sgf",
"1981-MEIJIN-GAME-4.sgf",
"1984-GOSEI-GAME-3.sgf",
"1984-GOSEI-GAME-4.sgf",
"1984-HONINBO-GAME-3.sgf",
"1984-KATO-NEIH-1.sgf",
"1984-MEIJIN-GAME-1.sgf",
"1984-MEIJIN-GAME-4.sgf",
"1984-MEIJIN-GAME-6.sgf",
"1984-OZA-GAME-1.sgf",
"1984-TENGEN-GAME-1.sgf",
"1984-TENGEN-GAME-3.sgf",
"1984-TENGEN-GAME-4.sgf",
"1985-HONINBO-GAME-3.sgf",
"1985-HONINBO-GAME-5.sgf",
"1985-JANG-AWAJI-1.sgf",
"1985-KISEI-GAME-1.sgf",
"1985-KISEI-GAME-4.sgf",
"1985-KISEI-GAME-6.sgf",
"1985-KISEI-GAME-7.sgf",
"1986-KISEI-GAME-1.sgf",
"1986-KISEI-GAME-2.sgf",
"1986-MEIJIN-GAME-2.sgf",
"1986-MEIJIN-GAME-3.sgf",
"1986-MEIJIN-GAME-4.sgf",
"1986-OZA-GAME-1.sgf",
"1986-OZA-GAME-3.sgf",
"1986-OZA-GAME-4.sgf",
"1986-TENGEN-GAME-2.sgf",
"1986-WOMENS-HONINBO-GAME-2.sgf",
"1987-HONINBO-GAME-1.sgf",
"1987-HONINBO-GAME-4.sgf",
"1987-JUDAN-GAME-2.sgf",
"1987-JUDAN-GAME-4.sgf",
"1987-KISEI-GAME-1.sgf",
"1987-KISEI-GAME-4.sgf",
"1987-KISEI-GAME-5.sgf",
"1987-MEIJIN-GAME-1.sgf",
"1987-OTAKE-NEI-1.sgf",
"1988-FINAL.sgf",
"1988-INTENTWO.sgf",
"1988-KISEI-GAME-1.sgf",
"1988-KISEI-GAME-2.sgf",
"1988-MEIJIN-GAME-3.sgf",
"1988-REDMOND-PRELIM-1.sgf",
"1988-SUPMA887.sgf",
"1988-SUPMA888.sgf",
"1988-SUPMA889.sgf",
"1988-SUPYOLIU.sgf",
"1988-TOOTAIMA.sgf",
"1988-TOSAKTAK.sgf",
"1988-WOHON881.sgf",
"1989-CHA-CHO-QUARTER-FINAL.sgf",
"1989-FINAL-1.sgf",
"1989-ING-CUP-GAME-3.sgf",
"1989-ING-CUP-GAME-4.sgf",
"1989-JUDAN-GAME-2.sgf",
"1989-KAKHASIY.sgf",
"1989-KAKORYAM.sgf",
"1989-KISEI-GAME-4.sgf",
"1989-KISUNG-GAME-1.sgf",
"1989-MEIJIN-GAME-1.sgf",
"1989-MEIJIN-GAME-3.sgf",
"1989-MEIJIN-GAME-4.sgf",
"1989-NEC_89_1.sgf",
"1990-FUJITSU-CHA-NEI.sgf",
"1990-FUJITSU-SEMIFINAL-1.sgf",
"1990-GAME-3.sgf",
"1990-HONINBO-GAME-5.sgf",
"1990-HONINBO-GAME-7.sgf",
"1990-KISEI-GAME-1.sgf",
"1990-LEE-TAKEMIYA-1.sgf",
"1990-MEIJIN-GAME-2.sgf",
"1990-MEIJIN-GAME-3.sgf",
"1990-PRELIM.sgf",
"1990-ROUND-3-NEI-RIN.sgf",
"1992-HONINBO-1992-GAME-7.sgf",
"1993-CHO-CHIKUN-LEE-CHANGHO-1.sgf",
"1993-MEIJIN-GAME-3.sgf",
"1993-TV-ASIA-FINAL.sgf",
"1993-ZHONG-JIALIN-HANE-YASUMASA-1.sgf",
"1994-KISEI-GAME-1.sgf",
"1994-KISEI-GAME-2.sgf",
"1994-KISEI-GAME-3.sgf",
"1995-World-Women's-Championship-1.sgf",
"1ST-KISEONG-LEAGUE-GAME-2.sgf",
"1ST-KISEONG-LEAGUE-GAME-4.sgf",
"1ST-KISEONG-TITLE-MATCH-GAME-4.sgf",
"21ST-SHIN-EI-TOURMT-SEMI-FINAL-2.sgf",
"23RD HONINBO #2.sgf",
"23RD HONINBO #3.sgf",
"23RD HONINBO #4.sgf",
"23RD HONINBO #6.sgf",
"23RD HONINBO #7.sgf",
"23RD HONINBO P-58.sgf",
"23RD HONINBO P-60.sgf",
"23RD-HAYA-GO-CHAMP-SEMI-FINAL-2.sgf",
"23RD-HAYA-GO-CHAMPIONSHIP-FINAL.sgf",
"25TH-WANGWI-LEAGUE-ENTRANT-4.sgf",
"25TH-WANGWI-LEAGUE-GAME-19.sgf",
"25TH-WANGWI-LEAGUE-GAME-2.sgf",
"25TH-WANGWI-LEAGUE-GAME-20.sgf",
"25TH-WANGWI-LEAGUE-GAME-22.sgf",
"25TH-WANGWI-LEAGUE-GAME-27.sgf",
"25TH-WANGWI-LEAGUE-GAME-4.sgf",
"25TH-WANGWI-TITLE-MATCH-GAME-6.sgf",
"26TH-WANGWI-LEAGUE-GAME-22.sgf",
"26TH-WANGWI-LEAGUE-GAME-23.sgf",
"26TH-WANGWI-LEAGUE-GAME-4.sgf",
"28TH-10-DAN-CHALLENGER-FINAL.sgf",
"28TH-10-DAN-LOSERS-FINAL.sgf",
"28TH-10-DAN-LOSERS-RD-1-#3.sgf",
"28TH-10-DAN-LOSERS-RD-1-#4.sgf",
"28TH-10-DAN-LOSERS-RD-2-#1.sgf",
"28TH-10-DAN-LOSERS-RD-2-#2.sgf",
"28TH-10-DAN-LOSERS-RD-2-#4.sgf",
"28TH-10-DAN-LOSERS-RD-3-#3.sgf",
"28TH-10-DAN-LOSERS-SEMI-FINAL-2.sgf",
"28TH-10-DAN-TITLE-MATCH-GAME-3.sgf",
"28TH-10-DAN-TITLE-MATCH-GAME-5.sgf",
"28TH-10-DAN-WINNERS-FINAL.sgf",
"28TH-10-DAN-WINNERS-RD-1-#3.sgf",
"28TH-10-DAN-WINNERS-RD-1-#4.sgf",
"28TH-10-DAN-WINNERS-RD-1-#5.sgf",
"28TH-10-DAN-WINNERS-RD-1-#6.sgf",
"28TH-10-DAN-WINNERS-RD-1-#8.sgf",
"28TH-10-DAN-WINNERS-RD-2-#4.sgf",
"28TH-10-DAN-WINNERS-SEMI-FINAL-2.sgf",
"29TH-10-DAN-LOSERS-RD-1-#2.sgf",
"29TH-10-DAN-LOSERS-RD-2-#3.sgf",
"29TH-10-DAN-LOSERS-RD-2-#4.sgf",
"29TH-10-DAN-LOSERS-RD-3-#1.sgf",
"29TH-10-DAN-LOSERS-RD-3-#2.sgf",
"29TH-10-DAN-LOSERS-RD-3-#3.sgf",
"29TH-10-DAN-TITLE-MATCH-GAME-1.sgf",
"29TH-10-DAN-TITLE-MATCH-GAME-3.sgf",
"29TH-10-DAN-WINNERS-FINAL.sgf",
"29TH-10-DAN-WINNERS-RD-1-#5.sgf",
"29TH-10-DAN-WINNERS-RD-1-#6.sgf",
"29TH-10-DAN-WINNERS-RD-2-#4.sgf",
"29TH-10-DAN-WINNERS-SEMI-FINAL-1.sgf",
"30TH-10-DAN-LOSERS-RD-1-#1.sgf",
"30TH-10-DAN-LOSERS-RD-1-#2.sgf",
"30TH-10-DAN-LOSERS-RD-2-#4.sgf",
"30TH-10-DAN-LOSERS-SEMI-FINAL-1.sgf",
"30TH-10-DAN-WINNERS-SEMI-FINAL-1.sgf",
"30TH-10-DAN-WINNERS-SEMI-FINAL-2.sgf",
"35TH-KUKSU-CHALLENGER-FINAL-#2.sgf",
"35TH-KUKSU-LOSERS-QTR-FINAL-2.sgf",
"35TH-KUKSU-LOSERS-SEMI-FINAL.sgf",
"35TH-KUKSU-PRELIMINARY-RD-#4.sgf",
"35TH-KUKSU-PRELIMINARY-RD-#5.sgf",
"35TH-KUKSU-WINNERS-ROUND-#2.sgf",
"37TH-OZA-PRELIMINARY-RD-3-#5.sgf",
"37TH-OZA-TITLE-MATCH-GAME-1.sgf",
"37TH-OZA-TITLE-MATCH-GAME-2.sgf",
"37TH-OZA-TOURNAMENT-RD-1-#2.sgf",
"37TH-OZA-TOURNAMENT-RD-1-#3.sgf",
"37TH-OZA-TOURNAMENT-RD-1-#8.sgf",
"37TH-OZA-TOURNAMENT-RD-2-#2.sgf",
"37TH-OZA-TOURNAMENT-RD-2-#4.sgf",
"38TH-NHK-CUP-SEMI-FINAL-1.sgf",
"38TH-NHK-CUP-SEMI-FINAL-2.sgf",
"38TH-NHK-CUP-TOURMT-QTR-FINAL-1.sgf",
"38TH-OZA-CHALLENGER-SEMI-FINAL-2.sgf",
"38TH-OZA-PRELIMINARY-RD-3-#1.sgf",
"38TH-OZA-PRELIMINARY-RD-3-#2.sgf",
"38TH-OZA-PRELIMINARY-RD-3-#4.sgf",
"38TH-OZA-PRELIMINARY-RD-3-#6.sgf",
"38TH-OZA-TITLE-MATCH-GAME-1.sgf",
"38TH-OZA-TITLE-MATCH-GAME-4.sgf",
"38TH-OZA-TOURNAMENT-RD-1-#2.sgf",
"38TH-OZA-TOURNAMENT-RD-1-#4.sgf",
"38TH-OZA-TOURNAMENT-RD-1-#7.sgf",
"38TH-OZA-TOURNAMENT-RD-2-#3.sgf",
"39TH-OZA-CHALLENGER-FINAL.sgf",
"39TH-OZA-CHALLENGER-SEMI-FINAL-1.sgf",
"39TH-OZA-PRELIMINARY-RD-3-#1.sgf",
"39TH-OZA-PRELIMINARY-RD-3-#2.sgf",
"39TH-OZA-PRELIMINARY-RD-3-#4.sgf",
"39TH-OZA-PRELIMINARY-RD-3-#5.sgf",
"39TH-OZA-PRELIMINARY-RD-3-#6.sgf",
"39TH-OZA-PRELIMINARY-RD-3-#8.sgf",
"39TH-OZA-TOURNAMENT-RD-1-#2.sgf",
"39TH-OZA-TOURNAMENT-RD-1-#4.sgf",
"39TH-OZA-TOURNAMENT-RD-1-#5.sgf",
"39TH-OZA-TOURNAMENT-RD-1-#6.sgf",
"39TH-OZA-TOURNAMENT-RD-1-#7.sgf",
"39TH-OZA-TOURNAMENT-RD-2-#3.sgf",
"39TH-OZA-TOURNAMENT-RD-2-#4.sgf",
"3RD-FUJITSU-CUP-RD-1-GAME-1.sgf",
"3RD-FUJITSU-CUP-RD-2-GAME-2.sgf",
"3RD-FUJITSU-CUP-RD-2-GAME-4.sgf",
"3RD-IBM-HAYA-GO-QUARTER-FINAL-1.sgf",
"3RD-IBM-HAYA-GO-QUARTER-FINAL-4.sgf",
"3RD-IBM-HAYA-GO-THIRD-PLACE.sgf",
"3RD-KISEONG-LEAGUE-ENTRANT-4.sgf",
"3RD-KISEONG-LEAGUE-GAME-18.sgf",
"3RD-KISEONG-LEAGUE-GAME-22.sgf",
"3RD-KISEONG-TITLE-MATCH-GAME-1.sgf",
"3RD-KISEONG-TITLE-MATCH-GAME-2.sgf",
"3RD-KISEONG-TITLE-MATCH-GAME-4.sgf",
"3RD-WOMEN'S-MEIJIN-CHALLNGR-FINAL.sgf",
"3RD-WOMEN'S-MEIJIN-TITLE-GAME-2.sgf",
"3RD-WOMEN'S-MEIJIN-TITLE-GAME-3.sgf",
"3RD-WOMEN'S-MEIJIN-WINNER-FINAL.sgf",
"44TH-HONINBO-LEAGUE-GAME-13.sgf",
"44TH-HONINBO-LEAGUE-GAME-14.sgf",
"44TH-HONINBO-LEAGUE-GAME-16.sgf",
"44TH-HONINBO-LEAGUE-GAME-19.sgf",
"44TH-HONINBO-LEAGUE-GAME-26.sgf",
"44TH-HONINBO-LEAGUE-GAME-3.sgf",
"44TH-HONINBO-LEAGUE-GAME-4.sgf",
"44TH-HONINBO-LEAGUE-GAME-5.sgf",
"44TH-HONINBO-LEAGUE-GAME-9.sgf",
"45TH-HONINBO-LEAGUE-ENTRANT-3.sgf",
"45TH-HONINBO-LEAGUE-GAME-12.sgf",
"45TH-HONINBO-LEAGUE-GAME-13.sgf",
"45TH-HONINBO-LEAGUE-GAME-15.sgf",
"45TH-HONINBO-LEAGUE-GAME-16.sgf",
"45TH-HONINBO-LEAGUE-GAME-17.sgf",
"45TH-HONINBO-LEAGUE-GAME-18.sgf",
"45TH-HONINBO-LEAGUE-GAME-19.sgf",
"45TH-HONINBO-LEAGUE-GAME-21.sgf",
"45TH-HONINBO-LEAGUE-GAME-25.sgf",
"45TH-HONINBO-LEAGUE-GAME-27.sgf",
"45TH-HONINBO-LEAGUE-GAME-5.sgf",
"45TH-HONINBO-LEAGUE-GAME-9.sgf",
"46TH-HONINBO-LEAGUE-ENTRANT-1.sgf",
"46TH-HONINBO-LEAGUE-ENTRANT-4.sgf",
"46TH-HONINBO-LEAGUE-GAME-10.sgf",
"46TH-HONINBO-LEAGUE-GAME-11.sgf",
"46TH-HONINBO-LEAGUE-GAME-14.sgf",
"46TH-HONINBO-LEAGUE-GAME-15.sgf",
"46TH-HONINBO-LEAGUE-GAME-16.sgf",
"46TH-HONINBO-LEAGUE-GAME-17.sgf",
"46TH-HONINBO-LEAGUE-GAME-18.sgf",
"46TH-HONINBO-LEAGUE-GAME-24.sgf",
"46TH-HONINBO-LEAGUE-GAME-25.sgf",
"46TH-HONINBO-LEAGUE-GAME-27.sgf",
"46TH-HONINBO-LEAGUE-GAME-5.sgf",
"46TH-HONINBO-LEAGUE-GAME-6.sgf",
"46TH-HONINBO-LEAGUE-GAME-9.sgf",
"47TH-HONINBO-LEAGUE-ENTRANT-1.sgf",
"47TH-HONINBO-LEAGUE-ENTRANT-2.sgf",
"47TH-HONINBO-LEAGUE-ENTRANT-3.sgf",
"6-STONE-KITANI-ISHIDA-1958.sgf",
"7TH JUDAN P-75.sgf",
"8TH MEIJIN #3.sgf",
"8TH MEIJIN P-32.sgf",
"8th-Japan-China-Super-Go-G3.sgf",
"8th-Japan-China-Super-Go-G9.sgf",
"8th-North-American-Fujitsu-Qualifiers.sgf",
"9-GOSEIGEN-9-033_148.sgf",
"9th-Japan-China-Super-Go.sgf",
"9TH-WOMEN'S-HONINBO-CHALLENGER.sgf",
"9TH-WOMEN'S-HONINBO-SEMI-FINAL-2.sgf",
"9TH-WOMEN'S-HONINBO-TITLE-GAME3.sgf",
"CHO-V-SUH.sgf",
"CJGX78_3.sgf",
"EURO22.sgf",
"EURO77_2.sgf",
"EURO78_1.sgf",
"GO186.sgf",
"go201a.sgf",
"go271.sgf",
"GO334.sgf",
"GO376.sgf",
"GO419.sgf",
"GO423H.sgf",
"GO427.sgf",
"go439.sgf",
"go453x.sgf",
"go461.sgf",
"go464.sgf",
"go467.sgf",
"go468.sgf",
"go469a.sgf",
"go471a.sgf",
"go475ax.sgf",
"GO490X.sgf",
"go604x.sgf",
"go616.sgf",
"GOSE03_2.sgf",
"GOSE03_4.sgf",
"gy93-K933.sgf",
"gy93-K934.sgf",
"gy93-K936.sgf",
"HBL01_01.sgf",
"HBL01_08.sgf",
"HBL01_10.sgf",
"HBL01_11.sgf",
"HBL01_13.sgf",
"HBL01_14.sgf",
"HBL01_16.sgf",
"HBL01_18.sgf",
"HBL01_19.sgf",
"HBL01_23.sgf",
"HBL01_25.sgf",
"HBL01_26.sgf",
"HBL01_27.sgf",
"HBL01_28.sgf",
"HBL01_30.sgf",
"HBL01_32.sgf",
"HBL01_35.sgf",
"HBL01_42.sgf",
"HBL01_46.sgf",
"HBL32_03.sgf",
"HNBO33_6.sgf",
"HONB01_2.sgf",
"HONB01_3.sgf",
"honinbo-10-2.sgf",
"honinbo-11-1.sgf",
"honinbo-25-2.sgf",
"honinbo-37-4.sgf",
"honinbo-37-5.sgf",
"honinbo-4-2.sgf",
"honinbo-4-4.sgf",
"honinbo-41-1.sgf",
"honinbo-5-3.sgf",
"honinbo-6-7.sgf",
"honinbo-8-2.sgf",
"honinbo-8-5.sgf",
"IF-YOU-LOSE-FOUR-CORNERS-RESIGN-2.sgf",
"IF-YOU-LOSE-FOUR-CORNERS-RESIGN-3.sgf",
"ING-VS-FUJITSU-1990-2.sgf",
"ITO02.sgf",
"Japan-China-1-13.sgf",
"Japan-China-2-11.sgf",
"Japan-China-3-6.sgf",
"Japan-China-3-9.sgf",
"Japan-China-5-4.sgf",
"Japan-China-6-12.sgf",
"Japan-China-6-8.sgf",
"Japan-China-9-5.sgf",
"JUDN15_2.sgf",
"JUDN16_4.sgf",
"k943.sgf",
"k945.sgf",
"k946.sgf",
"KAMAKURA-JUBANGO.sgf",
"KERWIN_3.sgf",
"KISE02_4.sgf",
"KISE02_7.sgf",
"kisei-79-1.sgf",
"kisei-80-2.sgf",
"kisei-83-7.sgf",
"kisei-84-4.sgf",
"kisei-86-3.sgf",
"kisei-86-4.sgf",
"KITANI-MANE.sgf",
"KITANI-SAKATA-1957.sgf",
"KITANI-SDAN.sgf",
"KO-PHIPPS.sgf",
"KOBAYAHI-V-ISHII-1970.sgf",
"KOBAYASHI-CHO-ROUND-1.sgf",
"KSL02_02.sgf",
"KSL02_03.sgf",
"KSL02_04.sgf",
"KSL02_06.sgf",
"LONGEST-PRO-GAME.sgf",
"meijin-64-2.sgf",
"meijin-70-x-sgf.sgf",
"meijin-79-1.sgf",
"meijin-79-2.sgf",
"MEIJIN-P-30.sgf",
"MEIJIN-P-31.sgf",
"MEJN02_1.sgf",
"MEJN03_2.sgf",
"MJL02_02.sgf",
"MJL03_01.sgf",
"MJL03_02.sgf",
"NHK-ETC-NHAWAMIN.sgf",
"NHK-ETC-NHHASKOM.sgf",
"NHK-ETC-NHOGAIM.sgf",
"NIHON KIIN CHAMP- #2.sgf",
"NIHON KIIN NO- 1 #3.sgf",
"NIHON-KIIN-1-P-130.sgf",
"NKCH P-82.sgf",
"NKCH P-83.sgf",
"NKCH P-85.sgf",
"OTAKE-KITANI-1961.sgf",
"OZA25_1.sgf",
"OZA25_2.sgf",
"OZA26_F.sgf",
"PAGES11A.sgf",
"PRO BEST TEN P-106.sgf",
"PUBLISHED-SUJI_04.sgf",
"PUBLISHED-SUJI_05.sgf",
"PUBLISHED-SUJI_07.sgf",
"REDMOND-CHO.sgf",
"REDMOND-JIMMY-CHA.sgf",
"REDMOND-KOJIMA-1.sgf",
"SAKATA-ARTICLE-GAME-1.sgf",
"SHUSAI-160.sgf",
"SHUSAKU-4-040_228.sgf",
"SHUSAKU-4-042_235.sgf",
"SHUSAKU-4-047_253.sgf",
"SHUSAKU-4-049_260.sgf",
"SHUSAKU-5-054_276.sgf",
"SHUSAKU-7-074_347.sgf",
"SHUSAKU-7-075_351.sgf",
"SHUSAKU-7-077_365.sgf",
"SHUSAKU-7-079_373.sgf",
"takemiya-article-game-2.sgf",
"takemiya-article-game-3.sgf",
"takemiya-article-game-6.sgf",
"TNGN03_1.sgf",
"TNGN03_2.sgf",
"TNGN03_3.sgf",
"TNGN03_4.sgf",
"TNGN04_1.sgf",
"TNGN04_4.sgf",
"WOM24_01.sgf",
"WOM24_02.sgf"};
      string[] files_special = {        
        "001_001.sgf",
        "003_006.sgf",
        "006_022.sgf",
        "007_025.sgf",
        "007_026.sgf",
        "1ST-KISEONG-TITLE-MATCH-GAME-4.sgf"
        };
      string[] egregiousCvsVB = {
        "14TH-GOSEI-TOURMENT-RD-1-#15.sgf",
        "17TH-TENGEN-TOURMNT-RD-1-#8.sgf",
        "TNGN03_4.sgf",
        "WOM24_01.sgf"
      };
      string[] someOfGoodCvsVB ={
        "10TH-NEC-CUP-SEMI-FINAL-1.sgf",
        "14TH-MEIJIN-LEAGUE-GAME-32.sgf",
        "23RD-HAYA-GO-CHAMP-SEMI-FINAL-2.sgf",
        "38TH-NHK-CUP-TOURMT-QTR-FINAL-1.sgf"
      };
      return files_carta;
    }
  }
}
