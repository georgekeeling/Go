using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.IO;
using System.Windows.Forms;

// this class is copy of GoCarta\modScore.cs which was created by me and Copilot from GoCarta\modScore.vb
// It has a new entry point "ScoreCompute1", which calls "ScoreCompute" quickly

// how to use this module:
// the routine "ScoreCompute" must be called with the following parameters:
// - NN1 = goban's dimension - 1 (usually 18)
// - japanese = true when territory scoring, = false when area scoring
    // See https://en.wikipedia.org/wiki/Rules_of_Go#Optional_rules
    // an empty point is in a player's territory
    // if all stones adjacent to it or to an empty intersection connected to it are of that player's color.
    //  a point belongs to a player's area if either: 1) it belongs to that player's territory;
    //  or 2) it is occupied by a stone of that player's color.
// - komi = komi's value
// - hc = number of handicap stones (0 = no handicap)
// - whitemoves = number of white moves played
// - blackmoves = number of black moves played
// also, the module exposes the integer matrix "position" (18 x 18) *** GK this is Board, I think ***
// that must be filled in advance according to the following rules:
// 0 = empty intersection; 1 = black stone; -1 = white stone; it may be filled manually or by means of an SGF file
// the matrix may be used by the main program in order to show the position whose score is computed and so on
// the module returns an integer value according to the following rules:
// 0 = jigo (draw)
// +n = black wins by n
// -n = white wins by n
// 1000 = black wins by resignation (= black is leading, and dame are > NN1*NN1/6 (usually 54)
// -1000 = white wins by resignation (= white is leading, and dame are > NN1*NN1/6 (usually 54)

// if compiled with the global variable GUI the module has GUI capabilities:
// GK **** the mehods below are in ScGui.cs ****
// it calls an external routine called "draw", that can draw something over an image of the goban,
// with the following parameters:
// - "element" (0 = stone; 1 = mark on a stone; 2 = point of territory; 3 = point of forced connection;
// 4 = stone dead, thus virtually removed )
// - coordinates on the goban (x, y, usually between 0 and 18)
// - color of "element"

// it calls an external routine called "debug", that displays (usually on a textBox) some informations about the strings
// (that happens if the main program calls the public routine "stringAnalysis" specifying the number of the string
// plus the parameter "true":
// the number of the string is kept in the public matrix ID(), indexed by means of the goban's coordinates,
// i.e. ID(9,9) contains the number of the string including the stone (if there is one) on the Tengen point)

// it calls an external routine called "score", that displays (usually on a textBox)
// extended informationss regarding the score of the game

namespace GoPlanner
{
  public static class ModScore
  {
    public const int Stone = 0;
    public const int MiniStone = 1;
    public const int Territory = 2;
    public const int Connection = 3;
    public const int Removing = 4;

    public static int[,] Board = new int[19, 19];
    public static int[,] ID = new int[19, 19];
    public static int Str;
    public static bool Shows;
    public static int N1;
    public static int GroupsNumber;     // GK renamed from GroupNumber
    public static int ChainsNumber;     // GK renamed from StringNumber
    public static string Rs = "";       // GK added initialisation. Only used if GUI is enabled

    public struct Chain
    {
      public int Id;
      public int Size;
      public int Colour;
      public Point[] P;     // GK: Chains[].P[X] are populated from 1. X = 0 not used 
      public int Status;
      public ArrayList Liberty;
      public double Liberties;
      public ArrayList Eyes;
      public int EyesNumber;
      public ArrayList EyeLikes;
      public int EyeLikesNumber;
      public ArrayList SpecialEyes;
      public int SpecialEyesNumber;
      public int Group;
      public int GroupTerritory;
      public ArrayList EmptyNeighbourPoints;
      public ArrayList NeighbourPoints;
    }

    public struct Group
    {
      public int Id;
      public int ElementCount;      // GK: was Size
      public int[] Element;         // GK: 51 of these when group created. Use from 1 to 50, like VB
      public int Stones;
      public int Colour;
      public ArrayList Eyes;
      public int EyesNumber;
      public ArrayList EyeLikes;
      public ArrayList SpecialEyes;
      public int Territory;
      public ArrayList Liberty;
    }

    public static Chain[] Chains = new Chain[101];    // GK: indexes from 0 to 100, Use from 1 to 100, like VB
    public static Group[] Groups = new Group[71];     // GK: indexes from 0 to 70, Use from 1 to 70, like VB
    public static bool[,] Controlled = new bool[19, 19];
    private static double WhiteArea;
    private static double BlackArea;
    private static int[,] BackupBoard = new int[19, 19];
    private static int[,] BackupIntensity = new int[19, 19];
    private static bool[] Bonus = new bool[101];
    private static int[,] BufferGoban = new int[19, 19];
    private static int[,] BufferIntensity = new int[19, 19];
    private static int WhiteCaptured;
    private static int BlackCaptured;
    private static bool CheckSpecial;
    private static int[,] CleanGoban = new int[19, 19];
    private static int Gr;
    private static int[,] TempGroup = new int[19, 19];
    private static int[,] IDGR = new int[19, 19];
    private static int[,] Intensity = new int[19, 19];
    private static ArrayList[] KillingEyes = new ArrayList[3];
    private static Point[] KOPoint = new Point[11];
    private static int StrongLinkCount;     // GK was Sl, number of [,] StrongLinks
    private static int GLColour;
    private static int GLTotal;
    private static int[,] StrongLinks = new int[101, 2];    // GK use from 1 to 100, like VB. Tuples use 0,1 obv.
                                                            // VB used 1,2 for tuples, 0 was unused 
    private static int WhiteStones;
    private static int BlackStones;
    private static int[,] OrigBoard = new int[19, 19];
    private static Point[] Seki = new Point[901];
    private static double WhiteTerritory;
    private static double BlackTerritory;

    private static bool Friendly(int x, int y, int colour)
    {
      // verifies the point (x,y) to be friendly (either outside the goban or
      // inside it and empty/same colour/opposite colour cannot play there)
      return (Internal(x, y) && Board[x, y] != -colour && !InAtari(x, y, colour)) || !Internal(x, y);
    }
    public static bool TestFriendly(byte c0)
    {
      bool c1, c2, c3;
      c1 = (c0 & 1) == 1;
      c2 = (c0 & 2) == 2;
      c3 = (c0 & 4) == 4;
      return (c1 && c2 && !c3 || !c1);
    }
    public static void StringAnalysis(int s, bool display, int killed = 0)
    {
      // Determines the basic properties of a string
      if (display)
      {
        ScGui.ResetImage();
      }
      int i, j, r;
      int x, y;
      int libReal, libs;

      // the points that will count as an eye (if they are "stealing eyes") are initizialised
      KillingEyes[0] = new ArrayList();
      KillingEyes[2] = new ArrayList();
      Gr = Chains[s].Group;

      for (i = 1; i <= Groups[Gr].ElementCount; i++)
      {
        for (j = 1; j <= Chains[Groups[Gr].Element[i]].Size; j++)
        {
          x = Chains[Groups[Gr].Element[i]].P[j].X;
          y = Chains[Groups[Gr].Element[i]].P[j].Y;
          if (display)
          {
            if (Groups[Gr].Element[i] == s)
            {
              ScGui.Plot(MiniStone, x, y, Color.Red);
            }
            else
            {
              ScGui.Plot(MiniStone, x, y, Color.Magenta);
            }
          }
        }
      }
      // chain's liberties are computed
      Chains[s].Liberty.Clear();
      Chains[s].Liberties = Funct(s, "LS");
      libReal = (int)Chains[s].Liberties;
      // liberty = points of liberty for the chain; liberties = their number, that will be updated...
      foreach (Point liberty in Chains[s].Liberty)
      {
        // because the ones the enemy cannot occupy are worth twice (if there is more than one)
        if (InAtari(liberty.X, liberty.Y, -Chains[s].Colour, true) && Chains[s].Liberties > 1)
        {
          Chains[s].Liberties += 1;
        }
        // the ones that may be occupied
        // by stones of the same colour without decreasing the overall number are worth 1/3 more...
        Board[liberty.X, liberty.Y] = Chains[s].Colour;
        libs = GroupLib(liberty.X, liberty.Y);
        if (libs >= libReal && libReal > 1)
        {
          Chains[s].Liberties += 1.0 / 3;
        }
        // furthermore, if liberties increases a lot when one of them is occupied by a
        // friendly stone, this one is worth 1/2 more
        if (libs - libReal >= 2 && libReal > 1)
        {
          Chains[s].Liberties += 0.5;
        }

        Board[liberty.X, liberty.Y] = 0;
      }
      // first such liberty is worth 1/2 more, not just 1/3 (another 1/6 is added to the count)
      if (Chains[s].Liberties - libReal > 0)
      {
        Chains[s].Liberties += 1.0 / 6;
      }
      // if a liberty deals with a KO, a compensation is added
      if (Bonus[s])
      {
        Chains[s].Liberties += 0.5;
      }
      // eyes are computed; "eyes" are the actual points; eyesnumber is their number;
      // the same for eyelikes and special eyes
      Chains[s].EyesNumber = Funct(s, "E1");
      Chains[s].EyesNumber += Funct(s, "E2");

      if (killed != 0)
      {
        for (r = 1; r <= Chains[killed].Size; r++)
        {
          Board[Chains[killed].P[r].X, Chains[killed].P[r].Y] = 0;
        }
      }

      Chains[s].EyesNumber += Funct(s, "E3", killed);

      if (killed != 0)
      {
        for (r = 1; r <= Chains[killed].Size; r++)
        {
          Board[Chains[killed].P[r].X, Chains[killed].P[r].Y] = Chains[killed].Colour;
        }
      }

      Chains[s].SpecialEyesNumber = Funct(s, "SE");
      Chains[s].EyeLikesNumber = Funct(s, "EL");
      Chains[s].GroupTerritory = TerritoryCompute(Gr);
    }
    private static void Bouzy()
    {
      // Bouzy's routine for counting territory
      int i, j;
      int dilationsNumber = 9;
      int erosionsNumber = 21;
      WhiteArea = 0;
      BlackArea = 0;
      WhiteTerritory = 0;
      BlackTerritory = 0;
      ArrayList noTerritory = new ArrayList();
      int cx, cy;
      int countGood;
      int countDame;

      Array.Copy(Board, BackupBoard, Board.Length);

      for (i = 0; i <= N1; i++)
      {
        for (j = 0; j <= N1; j++)
        {
          Intensity[i, j] = 0;
        }
      }

      // Standard routine
      for (i = 0; i <= N1; i++)
      {
        for (j = 0; j <= N1; j++)
        {
          Intensity[i, j] = 0;
          if (Board[i, j] == 1) Intensity[i, j] = 64;
          if (Board[i, j] == -1) Intensity[i, j] = -64;
        }
      }

      for (i = 1; i <= dilationsNumber; i++) Dilate();
      for (i = 1; i <= erosionsNumber; i++) Erode();

      Array.Copy(BackupBoard, Board, Board.Length);

      // If a point of territory is close to a dame point, then it's not counted
      // (unless it's also close to at least two other points of territory)
      for (i = 0; i <= N1; i++)
      {
        for (j = 0; j <= N1; j++)
        {
          countGood = 0;
          countDame = 0;
          if (Intensity[i, j] != 0 && Board[i, j] == 0)
          {
            if (Internal(i + 1, j) && Board[i + 1, j] == 0 && Intensity[i + 1, j] == 0) countDame++;
            if (Internal(i + 1, j) && Board[i + 1, j] == 0 && Intensity[i + 1, j] != 0) countGood++;
            if (Internal(i - 1, j) && Board[i - 1, j] == 0 && Intensity[i - 1, j] == 0) countDame++;
            if (Internal(i - 1, j) && Board[i - 1, j] == 0 && Intensity[i - 1, j] != 0) countGood++;
            if (Internal(i, j + 1) && Board[i, j + 1] == 0 && Intensity[i, j + 1] == 0) countDame++;
            if (Internal(i, j + 1) && Board[i, j + 1] == 0 && Intensity[i, j + 1] != 0) countGood++;
            if (Internal(i, j - 1) && Board[i, j - 1] == 0 && Intensity[i, j - 1] == 0) countDame++;
            if (Internal(i, j - 1) && Board[i, j - 1] == 0 && Intensity[i, j - 1] != 0) countGood++;
          }
          if (countDame > 0 && countGood < 2)
          {
            if (!noTerritory.Contains(new Point(i, j))) noTerritory.Add(new Point(i, j));
          }
        }
      }

      foreach (Point pos in noTerritory)
      {
        cx = pos.X;
        cy = pos.Y;
        Intensity[cx, cy] = 0;
      }

      // Area and territory are counted and possibly plotted in the main program
      for (i = 0; i <= N1; i++)
      {
        for (j = 0; j <= N1; j++)
        {
          if (Board[i, j] == -1)
          {
            WhiteArea++;
            WhiteStones++;
          }
          if (Board[i, j] == 1)
          {
            BlackArea++;
            BlackStones++;
          }
          if (Board[i, j] == 0 && Intensity[i, j] < 0 && !noTerritory.Contains(new Point(i, j)))
          {
            WhiteArea++;
            WhiteTerritory++;
            if (Shows) {ScGui.Plot(Territory, i, j, Color.White);}
          }
          if (Board[i, j] == 0 && Intensity[i, j] > 0 && !noTerritory.Contains(new Point(i, j)))
          {
            BlackArea++;
            BlackTerritory++;
            if (Shows) { ScGui.Plot(Territory, i, j, Color.Black); }
          }
        }
      }
    }
    public static int GroupsDefine()
    {
      int k;
      int index, indexg;
      bool found1, found2;
      int c, d;
      int[,] links = new int[901, 2];
      int ch1, ch2;
      int op;
      int[] counthalf = new int[101];
      bool linked = false;
      int HC;
      bool FC;
      int i, j, z;

      // Reset group IDs and sizes
      for (i = 0; i <= N1; i++)
      {
        for (j = 0; j <= N1; j++)
        {
          IDGR[i, j] = 0;
        }
      }
      for (i = 1; i <= 70; i++)
      {
        Groups[i].ElementCount = 0;
      }

      // Chains are linked into groups
      index = 0;
      StrongLinkCount = 0;
      for (i = 1; i <= 99; i++)
      {
        // Chains in atari, including at least 2 stones, don't belong to any group
        if (Chains[i].Size > 0 && (Funct(i, "LS") > 1 || (Funct(i, "LS") == 1 && Chains[i].Size == 1)))
        {
          for (j = i + 1; j <= 100; j++)
          {
            if (Chains[j].Size > 0 && Chains[i].Colour == Chains[j].Colour && Funct(j, "LS") > 1)
            {
              linked = false;
              for (k = 1; k <= Chains[i].Size; k++)
              {
                c = Chains[i].P[k].X;
                d = Chains[i].P[k].Y;

                // For each pair of chains, half connection (may be prevented) and full connection (can't) points are searched
                HC = HalfConnection(c, d, j);
                FC = FullConnection(c, d, j, true);

                // A half connection point has been found (the point may belong to other connections)
                if (HC > 0 && !FC)
                {
                  counthalf[i] += HC;
                  counthalf[j] += HC;
                }

                if (FC || (HC > 0 && counthalf[i] >= 1))
                {
                  // Two chains sharing a full connection point or at least 2 half connection points are linked
                  if (links[index, 0] != i || links[index, 1] != j)
                  {
                    index++;
                    links[index, 0] = i;
                    links[index, 1] = j;
                    linked = true;
                  }

                  // Full connections will be useful later
                  if (FC && (StrongLinks[StrongLinkCount, 0] != i || StrongLinks[StrongLinkCount, 1] != j))
                  {
                    StrongLinkCount++;
                    StrongLinks[StrongLinkCount, 0] = i;
                    StrongLinks[StrongLinkCount, 1] = j;
                  }
                }
              }

              if (!linked)
              {
                for (c = 0; c <= N1; c++)
                {
                  for (d = 0; d <= N1; d++)
                  {
                    // Two chains sharing a full connection "in between" are also linked
                    if (FullConnection(c, d, i, false) && FullConnection(c, d, j, false))
                    {
                      if (links[index, 0] != i || links[index, 1] != j)
                      {
                        index++;
                        links[index, 0] = i;
                        links[index, 1] = j;
                        linked = true;
                      }
                    }
                    if (linked) break;
                  }
                  if (linked) break;
                }
              }
            }
          }
        }
      }

      // groups are built by browsing the links
      indexg = 0;
      k = 0;
      do
      {
        k++;
        if (links[k, 0] != 0)
        {
          ch1 = links[k, 0];
          ch2 = links[k, 1];

          // A new group is created
          indexg++;
          Groups[indexg].Element = new int[51];
          Groups[indexg].ElementCount = 2;
          Groups[indexg].Stones = 0;
          Groups[indexg].Colour = Chains[ch1].Colour;
          Groups[indexg].Element[1] = ch1;
          Groups[indexg].Element[2] = ch2;
          Groups[indexg].Id = indexg;
          Groups[indexg].Eyes = new ArrayList();
          Groups[indexg].EyeLikes = new ArrayList();
          Groups[indexg].SpecialEyes = new ArrayList();
          Groups[indexg].Liberty = new ArrayList();
          Chains[ch1].Group = indexg;
          Chains[ch2].Group = indexg;
          links[k, 0] = 0;
          links[k, 1] = 0;

          do
          {
            op = 0;

            // Now looking for other chains, already linked, to be added to the newly created group
            for (z = 1; z <= index; z++)
            {
              for (i = 1; i <= Groups[indexg].ElementCount; i++)
              {
                if (links[z, 0] == Groups[indexg].Element[i])
                {
                  found2 = true;
                  for (j = 1; j <= Groups[indexg].ElementCount; j++)
                  {
                    if (links[z, 1] == Groups[indexg].Element[j]) found2 = false;
                  }
                  if (found2)
                  {
                    Groups[indexg].Element[Groups[indexg].ElementCount + 1] = links[z, 1];
                    Groups[indexg].ElementCount++;
                    Chains[links[z, 1]].Group = indexg;
                    op++;
                  }
                  links[z, 0] = 0;
                  links[z, 1] = 0;
                }
                if (links[z, 1] == Groups[indexg].Element[i])
                {
                  found1 = true;
                  for (j = 1; j <= Groups[indexg].ElementCount; j++)
                  {
                    if (links[z, 0] == Groups[indexg].Element[j]) found1 = false;
                  }
                  if (found1)
                  {
                    Groups[indexg].Element[Groups[indexg].ElementCount + 1] = links[z, 0];
                    Groups[indexg].ElementCount++;
                    Chains[links[z, 0]].Group = indexg;
                    op++;
                  }
                  links[z, 0] = 0;
                  links[z, 1] = 0;
                }
              }
            }

            // Exit when all links have been scanned
            if (op == 0) break;
            ScGui.DoEvents();
          } while (true);
        }

        // Exit the main cycle after every possible pair of chains has been examined
        if (k >= index) break;
        ScGui.DoEvents();
      } while (true);

      // Now looking for the remaining chains, the ones that cannot be linked to other ones
      for (k = 1; k <= 100; k++)
      {
        if (Chains[k].Size > 0)
        {
          found1 = true;
          for (i = 1; i <= indexg; i++)
          {
            for (j = 1; j <= Groups[i].ElementCount; j++)
            {
              if (Groups[i].Element[j] == k) found1 = false;
            }
          }
          // One found; its group is created
          if (found1)
          {
            indexg++;
            Groups[indexg].Element = new int[21];
            Groups[indexg].Colour = Chains[k].Colour;
            Groups[indexg].Element[1] = k;
            Groups[indexg].Id = indexg;
            Groups[indexg].ElementCount = 1;
            Groups[indexg].Stones = 0;
            Groups[indexg].Eyes = new ArrayList();
            Groups[indexg].EyeLikes = new ArrayList();
            Groups[indexg].SpecialEyes = new ArrayList();
            Groups[indexg].Liberty = new ArrayList();
            Chains[k].Group = indexg;
          }
        }
      }

      for (i = 1; i <= indexg; i++)
      {
        for (j = 1; j <= Groups[i].ElementCount; j++)
        {
          for (k = 1; k <= Chains[Groups[i].Element[j]].Size; k++)
          {
            c = Chains[Groups[i].Element[j]].P[k].X;
            d = Chains[Groups[i].Element[j]].P[k].Y;
            IDGR[c, d] = i;
          }
        }
      }

      return indexg;
    }
    public static void ScoreCompute1(ref double score, ref int timeMs, GoPlanner data)
    {
      Stopwatch stopWatch = new Stopwatch();
      stopWatch.Start();
      // japanese = true when territory scoring, = false when area scoring
      // komi = komi's value
      // hc = number of handicap stones (0 = no handicap)
      // translate for ScoreCompute
      // Board: 0 = empty intersection; 1 = black stone; -1 = white stone
      // thePoints.color: 0 blank, 1 white, 2 black, top bit set = pasting
      int whitemoves = data.capturedWhites, blackmoves = data.capturedBlacks;
      for (int i = 0; i < data.bSide; i++)
      {
        for (int j = 0; j < data.bSide; j++)
        {
          int code = data.thePoints[i, j].color & 0x7F;
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
              Console.WriteLine("Error in ScoreCompute1: invalid color code");
              break;
          }
        }
      }
      try 
      {
        ScGui.gp = data;
        score = ScoreCompute(data.bSide - 1, true, data.komi, data.handicap, whitemoves, blackmoves);
      }
      catch (Exception ex) 
      {
        // Get the stack trace with file info
        var st = new StackTrace(ex, true);
        // Get the top stack frame
        var frame = st.GetFrame(0);
        int line = frame?.GetFileLineNumber() ?? 0;
        string file = Path.GetFileName (frame?.GetFileName());
        Console.WriteLine($"Error: {ex.Message} File: {file} Line: {line}");
        score = double.MaxValue;
        ScGui.haveScore = false;
      }
      stopWatch.Stop();
      TimeSpan ts = stopWatch.Elapsed;
      timeMs = ts.Seconds * 1000 + ts.Milliseconds;
    }
    public static double ScoreCompute(int NN1, bool japanese, double komi, int hc, int whitemoves, int blackmoves)
    {
      // (was) Main routine
      int i, j, k, z, ws, bs, KON;
      bool remov;
      double winner;
      int[,] formerlib = new int[19, 19];
      int[,] formerstat = new int[19, 19];
      Console.WriteLine("ScoreCompute C#");
      if (Shows)
      {
        ScGui.ResetImage();
      }

      N1 = NN1;
      WhiteStones = 0;
      BlackStones = 0;

      // Memorizes the original position
      Array.Copy(Board, OrigBoard, Board.Length);

      // Bonuses for KOs that won't be filled (not enough liberties)
      for (i = 1; i <= 100; i++) Bonus[i] = false;

      // Looks for KOs' connection points
      KON = KOfilling();

      // For the last cycle: removing the "stealing eyes" chains
      CheckSpecial = true;

      do
      {
        // Main cycle: dead chains are removed
        for (i = 1; i < Chains.Length; i++) Chains[i] = default;
        for (i = 1; i < Groups.Length; i++) Groups[i] = default;

        // Chains, groups, and statuses are defined

        ChainsNumber = ChainDefine();
        GroupsNumber = GroupsDefine();
        StatusCompute();

        // Looking for snap-backs
        SnapbackSearch();

        // Looking for sekis
        z = SekiSearch();

        // Dead chains are removed
        remov = false;
        int stat = 520;
        do
        {
          if (!remov)
          {
            for (i = 1; i <= 100; i++)
            {
              if (Chains[i].Status == stat)
              {
                ChainRemove(i);
                remov = true;
              }
            }
            stat--;
          }
          else
          {
            break;
          }
        } while (stat != 498);
        // Last cycle: chains that look alive but include some "stealing eyes" stones are also removed
        if (stat == 498)
        {
          if (!remov && !CheckSpecial) 
            break;
          else 
            CheckSpecial = false;
        }
        ScGui.DoEvents();
      } while (true);

      // After dead chains' removal, the new position is memorized
      Array.Copy(Board, CleanGoban, Board.Length);
      for (i = 0; i <= N1; i++)
      {
        for (j = 0; j <= N1; j++)
        {
          // For each point on the goban, the status of the chain standing upon (if the point is not empty) is checked
          if (ID[i, j] != 0) formerstat[i, j] = Chains[ID[i, j]].Status;
        }
      }

      // First territory computing
      Bouzy();

      // Dame points and forced connections are checked
      int s;
      bool connected;
      ArrayList whiteConnections = new ArrayList();
      ArrayList blackConnections = new ArrayList();
      int tl, status;
      Point position;
      int cx, cy;

      // The original position is restored, and the chains are again defined
      Array.Copy(OrigBoard, Board, Board.Length);
      Array.Copy(Intensity, BackupIntensity, Intensity.Length);
      ChainsNumber = ChainDefine();

      // For each point on the goban, the number of liberties of the chain possibly standing upon is now computed
      for (i = 0; i <= N1; i++)
      {
        for (j = 0; j <= N1; j++)
        {
          formerlib[i, j] = 2;
          if (Board[i, j] != 0 && ID[i, j] == 0) formerlib[i, j] = 1;
          if (Board[i, j] != 0 && ID[i, j] != 0) formerlib[i, j] = Funct(ID[i, j], "LS");
          if (Board[i, j] != 0 && formerstat[i, j] == 0) formerstat[i, j] = 500;
        }
      }

      int contadame = 0;
      // **** filling the dame -  with black stones (dame = neutral points)
      // repeated later with white stones
      for (i = 0; i <= N1; i++)
      {
        for (j = 0; j <= N1; j++)
        {
          if (Board[i, j] == 0 && Intensity[i, j] == 0)
          {
            // All dame points are filled with black stones
            Board[i, j] = 1;
            contadame++;
          }
        }
      }

      // Now chains and groups are again defined; liberties are again counted
      ChainsNumber = ChainDefine();
      GroupsNumber = GroupsDefine();
      for (k = 1; k <= 100; k++)
      {
        if (Chains[k].Size > 0) Chains[k].Liberties = Funct(k, "LS");
      }

      do
      {
        // After dame filling, it's time to check if some chains are now in atari or dead
        connected = false;
        for (s = 1; s <= 100; s++)
        {
          if (Chains[s].Size > 0)
          {
            tl = 0;
            for (k = 1; k <= Chains[s].Size; k++)
            {
              tl += formerlib[Chains[s].P[k].X, Chains[s].P[k].Y];
            }

            // This is the chain status before dame filling (with black stones)
            status = formerstat[Chains[s].P[1].X, Chains[s].P[1].Y];

            // If liberties are 0/1, a forced connection is needed
            if (Chains[s].Liberties <= 1 && tl > Chains[s].Size && status < 498)
            {
              // Is it possible to capture some dead chain in order to increase liberties?
              position = RemoveAtari(s, (int)Chains[s].Liberties);
              cx = position.X;
              cy = position.Y;

              // If it is not, the chain's remaining liberty is found
              if (cx == 99)
              {
                if (Chains[s].Liberty.Count > 0)
                {
                  position = (Point)Chains[s].Liberty[0];
                  cx = position.X;
                  cy = position.Y;
                }
              }

              // This point (remaining liberty or capture's point) could be a forced connection
              if (cx != 99 && !blackConnections.Contains(position))
              {
                blackConnections.Add(position);

                // Thus it's filled, and again chains and groups are defined and liberties computed
                Board[cx, cy] = Chains[s].Colour;
                CheckCaptures(cx, cy);
                ChainsNumber = ChainDefine();
                GroupsNumber = GroupsDefine();
                for (k = 1; k <= 100; k++)
                {
                  if (Chains[k].Size > 0) Chains[k].Liberties = Funct(k, "LS");
                }
                connected = true;
              }
              else
              {
                // If no point has been found, another way is tried: fill the chain's neighbor points with white stones instead of black ones
                if (Chains[s].Liberties == 0)
                {
                  foreach (Point p in Chains[s].NeighbourPoints)
                  {
                    if (Intensity[p.X, p.Y] == 0 && Board[p.X, p.Y] == -Chains[s].Colour)
                    {
                      Board[p.X, p.Y] = Chains[s].Colour;

                      // As soon as the chain has got more liberties, no more points are filled with white stones
                      if (GroupLib(p.X, p.Y) > 0) break;
                    }
                  }
                }
              }
            }
          }
          if (connected) break;
        }
        ScGui.DoEvents();
        // everything is repeated until each connection has been found
      } while (connected);

      Array.Copy(OrigBoard, Board, Board.Length);
      Array.Copy(BackupIntensity, Intensity, Intensity.Length);

      // the task (**** filling the dame) is repeated one more, this time filling dame with white stones
      for (i = 0; i <= N1; i++)
      {
        for (j = 0; j <= N1; j++)
        {
          if (Board[i, j] == 0 && Intensity[i, j] == 0)
          {
            Board[i, j] = -1;
          }
        }
      }

      ChainsNumber = ChainDefine();
      GroupsNumber = GroupsDefine();
      for (k = 1; k <= 100; k++)
      {
        if (Chains[k].Size > 0) Chains[k].Liberties = Funct(k, "LS");
      }

      do
      {
        connected = false;
        for (s = 1; s <= 100; s++)
        {
          if (Chains[s].Size > 0)
          {
            tl = 0;
            for (k = 1; k <= Chains[s].Size; k++)
            {
              tl += formerlib[Chains[s].P[k].X, Chains[s].P[k].Y];
            }

            status = formerstat[Chains[s].P[1].X, Chains[s].P[1].Y];

            if (Chains[s].Liberties <= 1 && tl > Chains[s].Size && status < 498)
            {
              // va effettuata la connessione [connection must be made]
              position = RemoveAtari(s, (int)Chains[s].Liberties);
              cx = position.X;
              cy = position.Y;

              if (cx == 99)
              {
                if (Chains[s].Liberty.Count > 0)
                {
                  position = (Point)Chains[s].Liberty[0];
                  cx = position.X;
                  cy = position.Y;
                }
              }

              if (cx != 99 && !whiteConnections.Contains(position))
              {
                whiteConnections.Add(position);
                Board[cx, cy] = Chains[s].Colour;
                CheckCaptures(cx, cy);
                ChainsNumber = ChainDefine();
                GroupsNumber = GroupsDefine();
                for (k = 1; k <= 100; k++)
                {
                  if (Chains[k].Size > 0) Chains[k].Liberties = Funct(k, "LS");
                }
                connected = true;
              }
              else
              {
                if (Chains[s].Liberties == 0)
                {
                  foreach (Point p in Chains[s].NeighbourPoints)
                  {
                    if (Intensity[p.X, p.Y] == 0 && Board[p.X, p.Y] == -Chains[s].Colour)
                    {
                      Board[p.X, p.Y] = Chains[s].Colour;
                      if (GroupLib(p.X, p.Y) > 0) break;
                    }
                  }
                }
              }
            }
          }
          if (connected) break;
        }
        ScGui.DoEvents();
      } while (connected);

      int minusBlack = 0;
      int minusWhite = 0;
      // now the forced connections found by filling dame with black stones are
      // compared with the ones found by filling dame with white stones
      // if a forced connection does exist in both cases, it is real
      foreach (Point position2 in blackConnections)
      {
        if (whiteConnections.Contains(position2))
        {
          cx = position2.X;
          cy = position2.Y;

          if (Math.Sign(Intensity[cx, cy]) > 0)
          {
            // if the point to force connect was under black influence, black territory
            // will be decreased
            minusBlack++;
            if (Shows) ScGui.Plot(Connection, cx, cy, Color.Black);
          }
          else
          {
            minusWhite++;
            if (Shows) ScGui.Plot(Connection, cx, cy, Color.White);
          }
          CheckCaptures(cx, cy);
        }
      }

      // again sekis are looked for, as sometimes the forced connections would highlight some others
      z = SekiSearch();

      // the original position is now restored
      Array.Copy(OrigBoard, Board, Board.Length);
      ChainsNumber = ChainDefine();
      GroupsNumber = GroupsDefine();

      ArrayList connections = new ArrayList();
      int[,] colour = new int[N1 + 1, N1 + 1];

      // other forced connections: the ones required to capture a dead chain
      // that otherwise would prevent friendly territory from being counted
      for (i = 0; i <= N1; i++)
      {
        for (j = 0; j <= N1; j++)
        {
          if (Intensity[i, j] == 0 && Board[i, j] != 0)
          {
            k = ID[i, j];
            if (Funct(k, "LS") == 1)
            {
              // this chain is in atari, thus its last liberty is connected
              Point atari = (Point)Chains[k].Liberty[0];
              if (!connections.Contains(atari))
              {
                connections.Add(atari);
                colour[atari.X, atari.Y] = Chains[k].Colour;
                Board[atari.X, atari.Y] = Chains[k].Colour;

                if (GroupLib(atari.X, atari.Y) <= 1)
                {
                  // if the chain remains in atari, then the chain must be captured
                  colour[atari.X, atari.Y] = -Chains[k].Colour;
                  // bonus point to compensate the stone required to capture the chain:
                  // this type of connection is quite different from the previous one!
                  if (Chains[k].Colour == 1) minusBlack++; else minusWhite++;
                }
                Board[atari.X, atari.Y] = 0;
              }
            }
          }
        }
      }

      Array.Copy(CleanGoban, Board, Board.Length);
      // the board after the removal of dead chains is restored, and connection piints are counted
      foreach (Point position2 in connections)
      {
        cx = position2.X;
        cy = position2.Y;
        Board[cx, cy] = colour[cx, cy];

        if (Board[cx, cy] == 1)
        {
          // black connection point: black territory will decrease
          minusBlack++;
          if (Shows) ScGui.Plot(Connection, cx, cy, Color.Black);
        }
        else
        {
          minusWhite++;
          if (Shows) ScGui.Plot(Connection, cx, cy, Color.White);
        }
        CheckCaptures(cx, cy);
      }

      // now it's the moment for KOs filling
      for (k = 1; k <= KON; k++)
      {
        cx = Math.Abs(KOPoint[k].X);
        cy = Math.Abs(KOPoint[k].Y);
        if (Math.Sign(Intensity[cx, cy]) == Math.Sign(KOPoint[k].X))
        {
          Board[cx, cy] = Math.Sign(KOPoint[k].X);

          if (Board[cx, cy] == 1)
          {
            // again, if KO is black, black territory will decrease
            minusBlack++;
            if (Shows) ScGui.Plot(Connection, cx, cy, Color.Black);
          }
          else
          {
            minusWhite++;
            if (Shows) ScGui.Plot(Connection, cx, cy, Color.White);
          }
        }
      }

      // now it's moment to bring back to life stones in seki
      for (k = 1; k <= z; k++)
      {
        cx = Math.Abs(Seki[k].X);
        cy = Math.Abs(Seki[k].Y);

        if (Board[cx, cy] == 0 && Intensity[cx, cy] != 0)
        {
          if (Math.Sign(Seki[k].X) > 0) BlackStones++;
          else WhiteStones++;
        }
        Board[cx, cy] = Math.Sign(Seki[k].X);
      }
      // not really necessary, unless the main program is interested in checking
      // the ultimate status of the chains
      StatusCompute();

      // territory is computed (dead strings removed, KOs closed, seki brought back to life,
      // connections counted)
      ws = WhiteStones;
      bs = BlackStones;
      Bouzy();
      WhiteStones = ws;
      BlackStones = bs;

      // compensation for handicap, if present (territory scoring)
      BlackStones -= hc;
      BlackCaptured = blackmoves - BlackStones;
      WhiteCaptured = whitemoves - WhiteStones;
      // territory is decreased because of the forced connections...
      BlackTerritory -= minusBlack;
      WhiteTerritory -= minusWhite;
      //  ... but area is increased instead
      WhiteArea += minusBlack;
      BlackArea += minusWhite;
      // compensation for handicap, if present (area scoring)
      WhiteArea += hc;

      if (japanese)
      {
        winner = BlackTerritory + WhiteCaptured - WhiteTerritory - BlackCaptured - komi;
      }
      else
      {
        winner = BlackArea - WhiteArea - komi;
      }
      // too many dame points suggest a possible resignation (54 for 19x19, 24 for 13x13)
      if (contadame >= (N1 + 1) * (N1 + 1) / 6)
      {
        winner = 1000 * Math.Sign(winner);
      }

    string victory = "";
    switch (winner)
    {
        case 0:
            victory = "Jigo";
            Rs = "W + 0";
            break;
        case 1000:
          victory = "Black wins by resignation";
          Rs = "B + resign";
          break;
        case -1000:
          victory = "White wins by resignation";
          Rs = "W + resign";
          break;
        default:
          // The winner is determined by the sign of the score
          if (winner > 0)
          {
            victory = $"Black wins by {winner:##0} points";
            Rs = $"B + {winner}";
          }
          else
          {
            victory = $"White wins by {-winner:##0} points";
            Rs = $"W + {-winner}";
          }
          break;
    }

    if (Shows)
    {
        ScGui.Score($"Black territory:      {BlackTerritory:##0}");
        ScGui.Score($"\nWhite prisoners:      {WhiteCaptured:##0}");
        ScGui.Score($"\n--------------------  {BlackTerritory + WhiteCaptured:##0}");
        ScGui.Score($"\nWhite territory:      {WhiteTerritory:##0}");
        ScGui.Score($"\nBlack prisoners:      {BlackCaptured:##0}");
        ScGui.Score($"\n--------------------  {WhiteTerritory + BlackCaptured}");
        ScGui.Score($"\nBlack area:          {BlackArea:##0}");
        ScGui.Score($"\nWhite area:          {WhiteArea:##0}");
        ScGui.Score($"\n{victory}\n");
    }

      return winner;
    }
    private static void StatusCompute()
    {
      int i, j, status, h, k, l, ch, neighboursNumber, eyesN;
      bool remove;
      Point pt1, pt2, pt3;
      ArrayList pts = new ArrayList();
      ArrayList neighbours = new ArrayList();
      int cxh, cyh, cxk, cyk, cxl, cyl, d12, d23, d13;

      // A backup is made of the intersections' previous "intensity", as this computation will vary it
      // ("intensity" is important when computing territory)
      Array.Copy(Intensity, BackupIntensity, Intensity.Length);
      for (i = 1; i <= 70; i++)
      {
        if (Groups[i].ElementCount > 0)
        {
          Groups[i].Eyes.Clear();
          Groups[i].EyeLikes.Clear();
          Groups[i].SpecialEyes.Clear();
          eyesN = 0;
          do
          {
            Groups[i].Stones = 0;
            eyesN = Groups[i].EyesNumber;
            for (j = 1; j <= Groups[i].ElementCount; j++)
            {
              ch = Groups[i].Element[j];
              if (ch > 0 && Chains[ch].Size > 0)
              {
                // For each chain belonging to the group, eyes and liberties are counted
                StringAnalysis(ch, false);
                // Chain's temporary status - from 498 to 520
                Chains[ch].Status = (int)(520 - Math.Round(Chains[ch].EyesNumber / 2.0 + 
                  Chains[ch].Liberties * 2 + Chains[ch].GroupTerritory / 2.0));
              }
            }
            // Chains' stones, eyes, liberties counted before are added to the group
            ch = 0;       // GK avoids unassigned variable error
            for (j = 1; j <= Groups[i].ElementCount; j++)
            {
              ch = Groups[i].Element[j];
              if (ch > 0 && Chains[ch].Size > 0)
              {
                foreach (var eyePoint in Chains[ch].Eyes)
                {
                  if (!Groups[i].Eyes.Contains(eyePoint))
                  {
                    Groups[i].Eyes.Add(eyePoint);
                    Groups[i].EyesNumber++;
                  }
                }
                foreach (var eyeLike in Chains[ch].EyeLikes)
                {
                  if (!Groups[i].EyeLikes.Contains(eyeLike))
                  {
                    Groups[i].EyeLikes.Add(eyeLike);
                  }
                }
                foreach (var specialEye in Chains[ch].SpecialEyes)
                {
                  if (!Groups[i].SpecialEyes.Contains(specialEye))
                  {
                    Groups[i].SpecialEyes.Add(specialEye);
                  }
                }
                foreach (var libertyPoint in Chains[ch].Liberty)
                {
                  if (!Groups[i].Liberty.Contains(libertyPoint))
                  {
                    Groups[i].Liberty.Add(libertyPoint);
                  }
                }
                Groups[i].Stones += Chains[ch].Size;
              }
            }
            Groups[i].Territory = Chains[ch].GroupTerritory;
            // Two neighbor eyelikes/special eyes (at least one of them must be a special eye) count as one eye,
            // so these configurations are looked for
            pts.Clear();
            pts.AddRange(Groups[i].SpecialEyes);
            foreach (Point eyeLike in Groups[i].EyeLikes)
            {
              if (!pts.Contains(eyeLike))
              {
                pts.Add(new Point(100 + eyeLike.X, eyeLike.Y));
              }
            }

            neighboursNumber = 0;
            neighbours.Clear();
            if (pts.Count > 1)
            {
              remove = false;
              for (h = 0; h <= pts.Count - 2; h++)
              {
                Point ph = (Point)pts[h];
                cxh = ph.X > 100 ? ph.X - 100 : ph.X;
                cyh = ph.Y;

                for (k = h + 1; k < pts.Count; k++)
                {
                  Point pk = (Point)pts[k];
                  cxk = pk.X > 100 ? pk.X - 100 : pk.X;
                  cyk = pk.Y;

                  if (((cxh == cxk && Math.Abs(cyh - cyk) == 1) || (cyh == cyk && Math.Abs(cxh - cxk) == 1)) &&
                      (ph.X < 100 || pk.X < 100))
                  {
                    // Two neighbors have been found!
                    neighboursNumber++;
                    if (!neighbours.Contains(new Point(cxh, cyh)))
                    {
                      neighbours.Add(new Point(cxh, cyh));
                    }

                    if (!neighbours.Contains(new Point(cxk, cyk)))
                    {
                      neighbours.Add(new Point(cxk, cyk));
                    }
                  }
                }
              }
            }

            switch (neighboursNumber)
            {
              case 0:
                break;
              case 1:
                pt1 = (Point)neighbours[0];     // GK neighbours[0] copied to pt1
                pt2 = (Point)neighbours[1];

                // Two neighbor points (eyelikes/special eyes)
                if (!Groups[i].Eyes.Contains(pt1))
                {
                  // Notice that both points behave like eyes, but the number of eyes only increases by 1
                  Groups[i].Eyes.Add(pt1);
                  Groups[i].EyesNumber++;
                  if (!Groups[i].Eyes.Contains(pt2))
                  {
                    Groups[i].Eyes.Add(pt2);
                  }
                }

                // The eyelikes/special eyes that were merged into a true eye are removed
                if (Groups[i].EyeLikes.Contains(pt1)) Groups[i].EyeLikes.Remove(pt1);
                if (Groups[i].EyeLikes.Contains(pt2)) Groups[i].EyeLikes.Remove(pt2);
                if (Groups[i].SpecialEyes.Contains(pt1)) Groups[i].SpecialEyes.Remove(pt1);
                if (Groups[i].SpecialEyes.Contains(pt2)) Groups[i].SpecialEyes.Remove(pt2);
                break;
              case 2:
                if (neighbours.Count == 3)
                {
                  // Three points, two neighbors are again worth one eye
                  pt1 = (Point)neighbours[0];
                  pt2 = (Point)neighbours[1];
                  pt3 = (Point)neighbours[2];

                  if (!Groups[i].Eyes.Contains(pt1))
                  {
                    // The same as before; now all three points behave like eyes
                    Groups[i].Eyes.Add(pt1);
                    Groups[i].EyesNumber++;
                    if (!Groups[i].Eyes.Contains(pt2))
                    {
                      Groups[i].Eyes.Add(pt2);
                    }

                    if (!Groups[i].Eyes.Contains(pt3))
                    {
                      Groups[i].Eyes.Add(pt3);
                    }
                  }

                  // Removal of the eyelikes/special eyes
                  Groups[i].EyeLikes.Remove(pt1);
                  Groups[i].EyeLikes.Remove(pt2);
                  Groups[i].EyeLikes.Remove(pt3);
                  Groups[i].SpecialEyes.Remove(pt1);
                  Groups[i].SpecialEyes.Remove(pt2);
                  Groups[i].SpecialEyes.Remove(pt3);
                }
                else
                {
                  // Four or more points, two neighbors, large territory are worth two eyes
                  if (Groups[i].Territory > 2)
                  {
                    Groups[i].EyesNumber = 2;
                  }
                }
                // More than two neighbors, large territory are also worth two eyes
                break;
              default:
                if (Groups[i].Territory > 3)
                {
                  Groups[i].EyesNumber = 2;
                }
                break;
            } // end case

            // Three neighbors, all eyelikes (in the previous cases at least one special eye was needed) may be merged into a true eye
            if (Groups[i].EyeLikes.Count > 2)
            {
              do
              {
                remove = false;
                for (h = 0; h <= Groups[i].EyeLikes.Count - 3; h++)
                {
                  cxh = ((Point)Groups[i].EyeLikes[h]).X;
                  cyh = ((Point)Groups[i].EyeLikes[h]).Y;
                  for (k = h + 1; k <= Groups[i].EyeLikes.Count - 2; k++)
                  {
                    cxk = ((Point)Groups[i].EyeLikes[k]).X;
                    cyk = ((Point)Groups[i].EyeLikes[k]).Y;
                    for (l = k + 1; l <= Groups[i].EyeLikes.Count - 1; l++)
                    {
                      cxl = ((Point)Groups[i].EyeLikes[l]).X;
                      cyl = ((Point)Groups[i].EyeLikes[l]).Y;

                      d12 = Math.Abs(cxh - cxk) + Math.Abs(cyh - cyk);
                      d23 = Math.Abs(cxh - cxl) + Math.Abs(cyh - cyl);
                      d13 = Math.Abs(cxk - cxl) + Math.Abs(cyk - cyl);

                      if ((d12 == 1 && d23 == 1) || (d12 == 1 && d13 == 1) || (d23 == 1 && d12 == 1))
                      {
                        // That's the case: one eye is added, three eyelikes are removed, just like before
                        if (!Groups[i].Eyes.Contains(Groups[i].EyeLikes[h]))
                        {
                          Groups[i].Eyes.Add(Groups[i].EyeLikes[h]);
                          if (!Groups[i].Eyes.Contains(Groups[i].EyeLikes[k]))
                          {
                            Groups[i].Eyes.Add(Groups[i].EyeLikes[k]);
                          }

                          if (!Groups[i].Eyes.Contains(Groups[i].EyeLikes[l]))
                          {
                            Groups[i].Eyes.Add(Groups[i].EyeLikes[l]);
                          }

                          Groups[i].EyeLikes.RemoveAt(l);
                          Groups[i].EyeLikes.RemoveAt(k);
                          Groups[i].EyeLikes.RemoveAt(h);
                          Groups[i].EyesNumber++;
                          remove = true;
                        }
                      }
                      if (remove) { break; }
                    }
                    if (remove) { break; }
                  }
                  if (remove) { break; }
                }
              } while (remove);
            }

            // Each time an eye is added, the chains' properties are again counted
            // (the presence of new eyes may possibly change them)
            ScGui.DoEvents();
          } while (eyesN != Groups[i].EyesNumber);
        }
      }

      for (i = 1; i <= 100; i++)
      {
        if (Chains[i].Size > 0)
        {
          // Chains linked by means of a full connection get the status of the best one
          // among them (the lowest status)
          for (l = 1; l <= StrongLinkCount; l++)
          {
            if (StrongLinks[l, 0] == i)
            {
              status = Chains[StrongLinks[l, 1]].Status;
              if (status < Chains[i].Status)
              {
                Chains[i].Status = status;
              }
            }

            if (StrongLinks[l, 1] == i)
            {
              status = Chains[StrongLinks[l, 0]].Status;
              if (status < Chains[i].Status)
              {
                Chains[i].Status = status;
              }
            }
          }

          // Groups with two eyes are alive
          if (Groups[Chains[i].Group].EyesNumber >= 2)
          {
            Chains[i].Status = 100;
          }

          // Groups with one eye only are alive if...
          if (Groups[Chains[i].Group].EyesNumber == 1)
          {
            switch (Chains[i].GroupTerritory)
            {
              // ... either control three points of territory, and each one is eye/special eye
              case 3:
                if (Groups[Chains[i].Group].EyesNumber + Groups[Chains[i].Group].SpecialEyes.Count == 3)
                {
                  Chains[i].Status = 100;
                }

                break;
              // ... or control more than three points of territory
              default:
                if (Chains[i].GroupTerritory > 3)
                {
                  Chains[i].Status = 100;
                }
                break;
            }
          }

          // Groups with no eyes are alive if control at least 6 points of territory
          if (Groups[Chains[i].Group].EyesNumber == 0 && Chains[i].GroupTerritory >= 6)
          {
            Chains[i].Status = 100;
          }

          // "Stealing eyes" chains ("rabbit ears" and so on) are alive despite looking dead
          if (CheckSpecial && Killing(i))
          {
            Chains[i].Status = 100;
          }
        }
      }

      Array.Copy(BackupIntensity, Intensity, Intensity.Length);
    }
    public static int ChainDefine()
    {
      int stringNumber = 0;       // is really number of chains
      int size, oldString, newString, ch, cx, cy;

      // Reset IDs and chain sizes
      for (int i = 0; i <= N1; i++)
      {
        for (int j = 0; j <= N1; j++)
        {
          ID[i, j] = 0;
        }
      }

      for (int i = 1; i <= 100; i++)
      {
        Chains[i].Size = 0;
      }

      // Identifies the goban's chains
      for (int i = 0; i <= N1; i++)
      {
        for (int j = 0; j <= N1; j++)
        {
          if (Board[i, j] != 0)
          {
            if (i > 0 && ID[i - 1, j] != 0 && Chains[ID[i - 1, j]].Colour == Board[i, j])
            {
              // On the left there is a stone, and it belongs to a chain already identified
              ch = ID[i - 1, j];
              ID[i, j] = ch;
              Chains[ch].Size++;
              size = Chains[ch].Size;
              Chains[ch].P[size] = new Point(i, j);
            }

            if (j > 0 && ID[i, j - 1] != 0 && Chains[ID[i, j - 1]].Colour == Board[i, j])
            {
              // Above there is a stone, and it belongs to a chain already identified
              if (ID[i, j] == 0)
              {
                ch = ID[i, j - 1];
                ID[i, j] = ch;
                Chains[ch].Size++;
                size = Chains[ch].Size;
                Chains[ch].P[size] = new Point(i, j);
              }
              else
              {
                // Two chains are found to be the same one, so they are merged
                oldString = ID[i, j - 1];
                newString = ID[i, j];
                if (oldString != newString)
                {
                  for (int s = 1; s <= Chains[oldString].Size; s++)
                  {
                    ID[Chains[oldString].P[s].X, Chains[oldString].P[s].Y] = newString;
                    Chains[newString].Size++;
                    size = Chains[newString].Size;
                    Chains[newString].P[size] = Chains[oldString].P[s];
                  }
                  Chains[oldString].Size = 0;
                }
              }
            }

            // A new chain is created
            if (ID[i, j] == 0)
            {
              stringNumber++;
              Chains[stringNumber].Id = stringNumber;
              Chains[stringNumber].Colour = Board[i, j];
              Chains[stringNumber].Size = 1;
              Chains[stringNumber].P = new Point[151];
              Chains[stringNumber].P[1] = new Point(i, j);
              Chains[stringNumber].EmptyNeighbourPoints = new ArrayList();
              Chains[stringNumber].NeighbourPoints = new ArrayList();
              Chains[stringNumber].Eyes = new ArrayList();
              Chains[stringNumber].EyeLikes = new ArrayList();
              Chains[stringNumber].SpecialEyes = new ArrayList();
              Chains[stringNumber].Liberty = new ArrayList();
              ID[i, j] = stringNumber;
            }
          }
        }
      }

      // Count neighbor points for each chain
      // CoPilot cleverly introduced method AddNeighborPoint
      // but forgot to recount chains (stringNumber++)
      stringNumber = 0;
      for (int iChain = 1; iChain <= 100; iChain++)
      {
        if (Chains[iChain].Size > 0)
        {
          stringNumber++;
          // the chain's neighbour points are counted; empty points are counted again
          for (int j = 1; j <= Chains[iChain].Size; j++)
          {
            cx = Chains[iChain].P[j].X;
            cy = Chains[iChain].P[j].Y;
            AddNeighborPoint(iChain, cx - 1, cy - 1);
            AddNeighborPoint(iChain, cx, cy - 1);
            AddNeighborPoint(iChain, cx + 1, cy - 1);
            AddNeighborPoint(iChain, cx - 1, cy);
            AddNeighborPoint(iChain, cx + 1, cy);
            AddNeighborPoint(iChain, cx - 1, cy + 1);
            AddNeighborPoint(iChain, cx, cy + 1);
            AddNeighborPoint(iChain, cx + 1, cy + 1);
          }
        }
      }
      return stringNumber;
    }

    private static void AddNeighborPoint(int iChain, int x, int y)
    {
      if (Internal(x, y) && !Chains[iChain].NeighbourPoints.Contains(new Point(x, y)))
      {
        Chains[iChain].NeighbourPoints.Add(new Point(x, y));
        if (Board[x, y] == 0 && !Chains[iChain].EmptyNeighbourPoints.Contains(new Point(x, y)))
        {
          Chains[iChain].EmptyNeighbourPoints.Add(new Point(x, y));
        }
      }
    }
    public static int TerritoryCompute(int gro)
    {
      // The territory under a group's control is counted
      int[,] bg = new int[19, 19];
      int[,] intBackup = new int[19, 19];
      ArrayList noTerritory = new ArrayList();
      bool good;

      // to perform this calculation, friendly stones not belonging to the group must be ignored;
      // a backup of the current situation is needed
      Array.Copy(Board, bg, Board.Length);
      Array.Copy(Intensity, intBackup, Intensity.Length);

      for (int i = 0; i <= N1; i++)
      {
        for (int j = 0; j <= N1; j++)
        {
          Controlled[i, j] = false;     // GK: CP did not put this in
          if (Board[i, j] != 0 && Chains[ID[i, j]].Group != gro && Groups[Chains[ID[i, j]].Group].Colour == Groups[gro].Colour)
          {
            Board[i, j] = 0;
          }
        }
      }

      // Bouzy routine
      int dilationsNumber = 9;
      int erosionsNumber = 21;

      for (int i = 0; i <= N1; i++)
      {
        for (int j = 0; j <= N1; j++)
        {
          Intensity[i, j] = 0;
          if (Board[i, j] == 1) Intensity[i, j] = 64;
          if (Board[i, j] == -1) Intensity[i, j] = -64;
        }
      }

      for (int i = 1; i <= dilationsNumber; i++) Dilate();
      for (int i = 1; i <= erosionsNumber; i++) Erode();

      Array.Copy(bg, Board, Board.Length);

      // points that seem under the group's control but are close to dame points are marked...
      for (int i = 0; i <= N1; i++)
      {
        for (int j = 0; j <= N1; j++)
        {
          good = false;
          if (Math.Sign(Intensity[i, j]) == Groups[gro].Colour && Board[i, j] == 0)
          {
            AddIf(i, 1, j, 0);
            AddIf(i, -1, j, 0);
            AddIf(i, 0, j, 1);
            AddIf(i, 0, j, -1);
            Controlled[i, j] = true;  // GK: CP did not put this in
            if (noTerritory.Contains(new Point(i,j)) && good)
            {
              noTerritory.Remove(new Point(i, j));      // GK: CP did not put this in, but how could it be reached?
            }
          }
        }
      }

      Array.Copy(intBackup, Intensity, Intensity.Length);

      // Count territory excluding marked points
      int total = 0;
      for (int i = 0; i <= N1; i++)
      {
        for (int j = 0; j <= N1; j++)
        {
          if (Controlled[i, j] && !noTerritory.Contains(new Point(i, j)))
          {
            total++;
          }
        }
      }

      return total;
    
      void AddIf(int i, int di, int j, int dj)
      {
        // Copilot did not bother with !noTerritory.Contains(new Point(i, j))
        if (Internal(i + di, j + dj) && Board[i + di, j + dj] == 0 && Intensity[i + di, j + dj] == 0 &&
          !noTerritory.Contains(new Point(i, j))) 
        { noTerritory.Add(new Point(i, j)); }
      }
    }
    private static void Erode()
    {
      // Bouzy routine: erosion sub-routine
      Array.Copy(Board, BufferGoban, Board.Length);
      Array.Copy(Intensity, BufferIntensity, Intensity.Length);

      for (int i = 0; i <= N1; i++)
      {
        for (int j = 0; j <= N1; j++)
        {
          int nature = BufferGoban[i, j];
          if (nature != 0)
          {
            for (int r = 1; r <= 4; r++)
            {
              int iv = VeryClose(r, i, j).X;
              int jv = VeryClose(r, i, j).Y;
              if (Internal(iv, jv) && BufferGoban[iv, jv] != nature)
              {
                Intensity[i, j] -= Math.Sign(nature);
              }
              if (Internal(iv, jv) && Intensity[i, j] == 0)
              {
                Board[i, j] = 0;
                break;
              }
            }
          }
        }
      }
    }
    private static int SekiSearch()
    {
      // Searches for sekis (impasse that cannot be resolved into simple life and death)
      int i, j, k, gro, ch, cx, cy, z, tti = 0, ttj = 0;
      ArrayList falseEyes = new ArrayList();

      for (z = 1; z <= 100; z++) Seki[z] = new Point(99, 99);
      z = 0; // GK: z is number of sekis
      // falseEyes must be located: they count neither as liberties nor as territory (at least in seki)
      for (gro = 1; gro <= 70; gro++)
      {
        if (Groups[gro].ElementCount > 0)
        {
          falseEyes.Clear();
          foreach (Point l in Groups[gro].Liberty)
          {
            if (FalseEye(l, gro)) falseEyes.Add(l);
          }

          foreach (Point fe in falseEyes)
          {
            Groups[gro].Liberty.Remove(fe);
            Groups[gro].Territory--;
          }
        }
      }

      for (i = 1; i <= 69; i++)
      {
        // potential sekis:
        // at least two stones and two liberties in each groups, no territory (not counting eyes),
        // groups are opposite colour and look dead
        for (j = i + 1; j <= 70; j++)
        {
          if (Groups[i].ElementCount > 0) tti = Groups[i].Territory - Groups[i].Eyes.Count;
          if (Groups[j].ElementCount > 0) ttj = Groups[j].Territory - Groups[j].Eyes.Count;
          if (Groups[i].ElementCount > 0 && Groups[i].Stones > 2 &&
            Groups[j].ElementCount > 0 && Groups[j].Stones > 2 &&
            Chains[Groups[j].Element[1]].Colour != Chains[Groups[i].Element[1]].Colour &&
            Groups[i].Liberty.Count >= 2 && Groups[j].Liberty.Count >= 2 &&
            tti == 0 && ttj == 0 &&
            Chains[Groups[i].Element[1]].Status > 100 &&
            Chains[Groups[j].Element[1]].Status > 100)
          {
            // the group with less liberties is the "small" one (usually the inner one)
            int small, large;
            if (Groups[i].Liberty.Count < Groups[j].Liberty.Count)
            {
              small = i;
              large = j;
            }
            else
            {
              large = i;
              small = j;
            }

            bool same = false;
            // two kinds of seki:
            // - the "small" group contains 3 stones or 4 squared (killing shape)
            // - number of liberties is the same for both groups (but liberties are <= 5)
            if (KillingShape(Groups[small].Element[1]) ||
                (Groups[small].Liberty.Count <= 5 && Groups[small].Liberty.Count == Groups[large].Liberty.Count))
            {
              same = true;
            }
            // first: if the groups contain an eye it does not count as liberty
            foreach (Point p in Groups[i].Eyes)
            {
              if (Groups[i].Liberty.Contains(p)) Groups[i].Liberty.Remove(p);
            }
            foreach (Point p in Groups[j].Eyes)
            {
              if (Groups[j].Liberty.Contains(p)) Groups[j].Liberty.Remove(p);
            }
            // second: let's check if after the removal of the eye
            // the liberties are still the same number (second kind of seki only)
            if (!KillingShape(Groups[small].Element[1]) &&
                Groups[small].Liberty.Count != Groups[large].Liberty.Count)
            {
              same = false;
            }
            // third and last: for both kinds of seki the small group's liberties
            // must be included in the large group's
            foreach (Point liberty2 in Groups[small].Liberty)
            {
              if (!Groups[large].Liberty.Contains(liberty2)) same = false;
            }
            // they are, so this is a seki...
            if (same)
            {
              // and all the chains belonging to both groups are brought back to life (status = 100)
              for (gro = 1; gro <= Groups[i].ElementCount; gro++)
              {
                ch = Groups[i].Element[gro];
                Chains[ch].Status = 100;
                for (k = 1; k <= Chains[ch].Size; k++)
                {
                  cx = Chains[ch].P[k].X;
                  cy = Chains[ch].P[k].Y;
                  z++;
                  Seki[z] = Chains[ch].Colour == 1 ? new Point(cx, cy) : new Point(-cx, -cy);
                  if (Shows)
                  {
                    if (Chains[ch].Colour == 1)
                      ScGui.Plot(Stone, cx, cy, Color.DimGray);
                    else
                      ScGui.Plot(Stone, cx, cy, Color.LightGray);
                  }
                }
              }

              for (gro = 1; gro <= Groups[j].ElementCount; gro++)
              {
                ch = Groups[j].Element[gro];
                Chains[ch].Status = 100;
                for (k = 1; k <= Chains[ch].Size; k++)
                {
                  cx = Chains[ch].P[k].X;
                  cy = Chains[ch].P[k].Y;
                  z++;
                  Seki[z] = Chains[ch].Colour == 1 ? new Point(cx, cy) : new Point(-cx, -cy);
                  if (Shows)
                  {
                    if (Chains[ch].Colour == 1)
                      ScGui.Plot(Stone, cx, cy, Color.DimGray);
                    else
                      ScGui.Plot(Stone, cx, cy, Color.LightGray);
                  }
                }
              }
            }
          }
        }
      }
      return z;
    }
    private static void SnapbackSearch()
    {
      // Searches for snap-back situations
      int[,] backupSnap = new int[19, 19];
      int i, j, x1, y1, newLib;

      for (i = 1; i <= 99; i++)
      {
        for (j = i + 1; j <= 100; j++)
        {
          // snap-back may occur when two chains have only one common liberty and look dead
          if (Chains[i].Size > 0 && Chains[j].Size > 0 &&
              Chains[i].Colour == -Chains[j].Colour &&
              Funct(i, "LS") == 1 && Funct(j, "LS") == 1 &&
              Chains[i].Status > 100 && Chains[j].Status > 100)
          {
            // GK added pi, pj because Copilot translation had compile error
            Point pi = (Point)(Chains[i].Liberty[0]);
            Point pj = (Point)(Chains[j].Liberty[0]);
            if (pi.X == pj.X && pi.Y == pj.Y)
            {
              x1 = pi.X;
              y1 = pi.Y;
              Array.Copy(Board, backupSnap, Board.Length);
              // let's occupy the liberty with a stone of the first color
              Board[x1, y1] = Chains[i].Colour;
              CheckCaptures(x1, y1);
              ID[x1, y1] = i;
              newLib = Funct(i, "LS");

              Array.Copy(backupSnap, Board, Board.Length);
              ID[x1, y1] = 0;

              // if the chain whose colour we tried still has got one liberty, it dies; the other one lives
              if (newLib == 1)
              {
                Chains[i].Status = 520;
                Chains[j].Status = 100;
              }
              else
              {
                Board[x1, y1] = Chains[j].Colour;
                CheckCaptures(x1, y1);
                ID[x1, y1] = j;
                newLib = Funct(j, "LS");

                Array.Copy(backupSnap, Board, Board.Length);
                ID[x1, y1] = 0;

                if (newLib == 1)
                {
                  Chains[j].Status = 520;
                  Chains[i].Status = 100;
                }
              }
            }
          }
        }
      }
    }
    private static void CheckCaptures(int x, int y)
    {
      // Deletes from the virtual board a stone that has been captured by another one played in (x, y)
      for (int xx = x - 1; xx <= x + 1; xx++)
      {
        for (int yy = y - 1; yy <= y + 1; yy++)
        {
          if (Math.Abs(xx - x) + Math.Abs(yy - y) == 1 && xx >= 0 && xx <= N1 && yy >= 0 && yy <= N1)
          {
            if (Board[xx, yy] == -Board[x, y])
            {
              if (GroupLib(xx, yy) == 0)
              {
                for (int xxx = 0; xxx <= N1; xxx++)
                {
                  for (int yyy = 0; yyy <= N1; yyy++)
                  {
                    if (TempGroup[xxx, yyy] == 1)
                    {
                      Board[xxx, yyy] = 0;
                    }
                  }
                }
              }
            }
          }
        }
      }
    }
    private static void Dilate()
    {
      // Bouzy routine: dilation sub-routine
      Array.Copy(Board, BufferGoban, Board.Length);
      Array.Copy(Intensity, BufferIntensity, Intensity.Length);

      for (int i = 0; i <= N1; i++)
      {
        for (int j = 0; j <= N1; j++)
        {
          bool outOfBounds = false;
          int nature = BufferGoban[i, j];

          for (int r = 1; r <= 4; r++)
          {
            int iv = VeryClose(r, i, j).X;
            int jv = VeryClose(r, i, j).Y;

            if (Internal(iv, jv) && BufferGoban[iv, jv] != 0 && BufferGoban[iv, jv] != nature)
            {
              if (nature != 0) outOfBounds = true;
              else nature = BufferGoban[iv, jv];
            }
          }

          if (!outOfBounds && nature != 0)
          {
            Board[i, j] = nature;
            for (int r = 1; r <= 4; r++)
            {
              int iv = VeryClose(r, i, j).X;
              int jv = VeryClose(r, i, j).Y;

              if (Internal(iv, jv) && BufferGoban[iv, jv] == nature)
              {
                Intensity[i, j] += Math.Sign(nature);
              }
            }
          }
        }
      }
    }
    private static bool KillingShape(int ch)
    {
      // Chains that have a "killing shape" are checked, because they could become part of a seki
      bool good = false;
      double meanX = 0, meanY = 0;
      int z;
      int[,] temp = new int[19, 19];
      int[,] tempF = new int[19, 19];

      // Three stones are a "killing shape"
      if (Chains[ch].Size == 3 && Groups[Chains[ch].Group].ElementCount == 1) good = true;

      // Four stones in a square are a "killing shape" too
      if (Chains[ch].Size == 4 && Groups[Chains[ch].Group].ElementCount == 1)
      {
        good = true;
        meanX = 0;
        meanY = 0;

        for (z = 1; z <= 4; z++)
        {
          meanX += Chains[ch].P[z].X;     // Chains[ch].P[] are populated from 1 to 4
          meanY += Chains[ch].P[z].Y;
        }

        meanX /= 4;
        meanY /= 4;

        for (z = 1; z <= 4; z++)
        {
          if (Math.Abs(meanX - Chains[ch].P[z].X) != 0.5) good = false;
          if (Math.Abs(meanY - Chains[ch].P[z].Y) != 0.5) good = false;
        }
      }

      if (good)
      {
        // "Killing shape" by itself is not enough: the chain's liberties must be "isolated",
        // meaning that no others appear if they all are filled
        Array.Copy(ID, temp, ID.Length);
        Array.Copy(Board, tempF, Board.Length);

        foreach (Point lib in Chains[ch].Liberty)
        {
          ID[lib.X, lib.Y] = ch;
          Board[lib.X, lib.Y] = Chains[ch].Colour;
        }

        int newLiberties = Funct(ch, "LS");

        // New liberties have appeared, so the "killing shape" is not enough,
        // and the chain cannot belong to a seki
        if (newLiberties > 0) good = false;

        Array.Copy(temp, ID, ID.Length);
        Array.Copy(tempF, Board, Board.Length);
      }

      return good;
    }
    private static bool FullConnection(int x, int y, int ch, bool checkLink)
    {
      // Determines if point (x, y) is a possible extension for chain ch
      int col = Chains[ch].Colour;

      // Point not empty = no extension, unless checkLink = true; in that case,
      // the point may work as a link if the stone's color matches the chain's
      if (Board[x, y] != 0 && !checkLink)
      {
        return false;
      }
      else
      {
        // Diagonal jump: both side points must be empty/
        // one of them is occupied by a friendly stone/opposite color cannot play there
        if (Internal(x + 1, y + 1) && ID[x + 1, y + 1] == ch)
        {
          if ((Empty(x + 1, y) && Empty(x, y + 1)) ||
              Board[x + 1, y] == col || Board[x, y + 1] == col ||
              InAtari(x + 1, y, -col) || InAtari(x, y + 1, -col))
            return true;
        }
        if (Internal(x - 1, y + 1) && ID[x - 1, y + 1] == ch)
        {
          if ((Empty(x - 1, y) && Empty(x, y + 1)) ||
              Board[x - 1, y] == col || Board[x, y + 1] == col ||
              InAtari(x - 1, y, -col) || InAtari(x, y + 1, -col))
            return true;
        }
        if (Internal(x + 1, y - 1) && ID[x + 1, y - 1] == ch)
        {
          if ((Empty(x + 1, y) && Empty(x, y - 1)) ||
              Board[x + 1, y] == col || Board[x, y - 1] == col ||
              InAtari(x + 1, y, -col) || InAtari(x, y - 1, -col))
            return true;
        }
        if (Internal(x - 1, y - 1) && ID[x - 1, y - 1] == ch)
        {
          if ((Empty(x - 1, y) && Empty(x, y - 1)) ||
              Board[x - 1, y] == col || Board[x, y - 1] == col ||
              InAtari(x - 1, y, -col) || InAtari(x, y - 1, -col))
            return true;
        }

        // One-point jump: the middle point must be empty, the two side points empty or friendly
        if (Internal(x + 2, y) && ID[x + 2, y] == ch)
        {
          if ((Friendly(x, y - 1, col) || Friendly(x, y + 1, col)) &&
              Empty(x + 1, y - 1) && Empty(x + 1, y) && Empty(x + 1, y + 1) &&
              (Friendly(x + 2, y - 1, col) || Friendly(x + 2, y + 1, col)))
            return true;
        }
        if (Internal(x - 2, y) && ID[x - 2, y] == ch)
        {
          if ((Friendly(x, y - 1, col) || Friendly(x, y + 1, col)) &&
              Empty(x - 1, y - 1) && Empty(x - 1, y) && Empty(x - 1, y + 1) &&
              (Friendly(x - 2, y - 1, col) || Friendly(x - 2, y + 1, col)))
            return true;
        }
        if (Internal(x, y + 2) && ID[x, y + 2] == ch)
        {
          if ((Friendly(x - 1, y, col) || Friendly(x + 1, y, col)) &&
              Empty(x - 1, y + 1) && Empty(x, y + 1) && Empty(x + 1, y + 1) &&
              (Friendly(x - 1, y + 2, col) || Friendly(x + 1, y + 2, col)))
            return true;
        }
        if (Internal(x, y - 2) && ID[x, y - 2] == ch)
        {
          if ((Friendly(x - 1, y, col) || Friendly(x + 1, y, col)) &&
              Empty(x - 1, y - 1) && Empty(x, y - 1) && Empty(x + 1, y - 1) &&
              (Friendly(x - 1, y - 2, col) || Friendly(x + 1, y - 2, col)))
            return true;
        }

        // Knight's jump: the two middle points must be empty, the two side points empty or friendly
        if (Internal(x - 1, y + 2) && ID[x - 1, y + 2] == ch)
        {
          if (Friendly(x - 1, y, col) && Empty(x - 1, y + 1) && Empty(x, y + 1) && Friendly(x, y + 2, col))
            return true;
        }
        if (Internal(x + 1, y + 2) && ID[x + 1, y + 2] == ch)
        {
          if (Friendly(x + 1, y, col) && Empty(x, y + 1) && Empty(x + 1, y + 1) && Friendly(x, y + 2, col))
            return true;
        }
        if (Internal(x - 1, y - 2) && ID[x - 1, y - 2] == ch)
        {
          if (Friendly(x - 1, y, col) && Empty(x - 1, y - 1) && Empty(x, y - 1) && Friendly(x, y - 2, col))
            return true;
        }
        if (Internal(x + 1, y - 2) && ID[x + 1, y - 2] == ch)
        {
          if (Friendly(x + 1, y, col) && Empty(x, y - 1) && Empty(x + 1, y - 1) && Friendly(x, y - 2, col))
            return true;
        }
        if (Internal(x + 2, y - 1) && ID[x + 2, y - 1] == ch)
        {
          if (Friendly(x, y - 1, col) && Empty(x + 1, y - 1) && Empty(x + 1, y) && Friendly(x + 2, y, col))
            return true;
        }
        if (Internal(x + 2, y + 1) && ID[x + 2, y + 1] == ch)
        {
          if (Friendly(x, y + 1, col) && Empty(x + 1, y) && Empty(x + 1, y + 1) && Friendly(x + 2, y, col))
            return true;
        }
        if (Internal(x - 2, y - 1) && ID[x - 2, y - 1] == ch)
        {
          if (Friendly(x, y - 1, col) && Empty(x - 1, y - 1) && Empty(x - 1, y) && Friendly(x - 2, y, col))
            return true;
        }
        if (Internal(x - 2, y + 1) && ID[x - 2, y + 1] == ch)
        {
          if (Friendly(x, y + 1, col) && Empty(x - 1, y) && Empty(x - 1, y + 1) && Friendly(x - 2, y, col))
            return true;
        }
        // GK: CoPilot did not translate the remainder of the method. Except return false;
        // Does it know something we don't? Only 4/623 tests failed without it.
        // 2 of the 4 are fixed by this addition.
        
        // two points jump: the middle points must be empty, the neighbours empty or friendly
        if (Internal(x, y + 3) && ID[x, y + 3] == ch)
        {
          if ((Friendly(x - 1, y, col) || InAtari(x, y + 1, -col)) && 
              (Friendly(x + 1, y, col) || InAtari(x, y +1, -col)) && Friendly(x -1, y + 1, col) && 
              Empty(x, y +1) && Friendly(x +1, y + 1, col) && Friendly(x -1, y + 2, col) &&
              Empty(x, y +2) && Friendly(x +1, y + 2, col) && 
              (Friendly(x - 1, y + 3, col) || InAtari(x, y + 2, -col)) && 
              (Friendly(x + 1, y + 3, col) || InAtari(x, y + 2, -col)))
            return true;
        }
        if (Internal(x, y -3) && ID[x, y -3] == ch)
        {
          if ((Friendly(x - 1, y, col) || InAtari(x, y - 1, -col)) && 
              (Friendly(x + 1, y, col) || InAtari(x, y - 1, -col)) && Friendly(x - 1, y - 1, col) && 
              Empty(x, y - 1) && Friendly(x + 1, y - 1, col) && Friendly(x - 1, y - 2, col) && 
              Empty(x, y - 2) && Friendly(x + 1, y - 2, col) && 
              (Friendly(x - 1, y - 3, col) || InAtari(x, y - 2, -col)) && 
              (Friendly(x + 1, y - 3, col) || InAtari(x, y - 2, -col))) 
            return true;
        }
        if (Internal(x - 3, y) && ID[x -3, y] == ch)
        {
          if ((Friendly(x, y - 1, col) || InAtari(x - 1, y, -col)) && 
              (Friendly(x, y + 1, col) || InAtari(x - 1, y, -col)) && Friendly(x - 1, y - 1, col) && 
              Empty(x - 1, y) && Friendly(x - 1, y + 1, col) && Friendly(x - 2, y + 1, col) && 
              Empty(x - 2, y) && Friendly(x - 2, y - 1, col) && 
              (Friendly(x - 3, y - 1, col) || InAtari(x - 2, y, -col)) && 
              (Friendly(x - 3, y + 1, col) || InAtari(x - 2, y, -col))) 
            return true;
        }
        if (Internal(x + 3, y) && ID[x +3, y] == ch)
        {
          if ((Friendly(x, y - 1, col) || InAtari(x + 1, y, -col)) && 
              (Friendly(x, y + 1, col) || InAtari(x + 1, y, -col)) && Friendly(x + 1, y - 1, col) && 
              Empty(x + 1, y) && Friendly(x + 1, y + 1, col) && Friendly(x + 2, y + 1, col) && 
              Empty(x + 2, y) && Friendly(x + 2, y - 1, col) && 
              (Friendly(x + 3, y - 1, col) || InAtari(x + 2, y, -col)) && 
              (Friendly(x + 3, y + 1, col) || InAtari(x + 2, y, -col))) 
            return true;
        }
      }

      return false;
    }
    public static int Funct(int ch, string what, int killed = 0)
    {
      int i, j;
      // Liberties and eyes/special eyes/eyelikes are counted
      int funct = 0;

      switch (what)
      {
        // Liberties
        case "LS":
          for (i = 0; i <= N1; i++)
          {
            for (j = 0; j <= N1; j++)
            {
              if (Board[i, j] == 0)
              {
                if (i > 0 && ID[i - 1, j] == ch)
                {
                  funct++;
                  if (!Chains[ch].Liberty.Contains(new Point(i, j)))
                    Chains[ch].Liberty.Add(new Point(i, j));
                }
                else if (i < N1 && ID[i + 1, j] == ch)
                {
                  funct++;
                  if (!Chains[ch].Liberty.Contains(new Point(i, j)))
                    Chains[ch].Liberty.Add(new Point(i, j));
                }
                else if (j > 0 && ID[i, j - 1] == ch)
                {
                  funct++;
                  if (!Chains[ch].Liberty.Contains(new Point(i, j)))
                    Chains[ch].Liberty.Add(new Point(i, j));
                }
                else if (j < N1 && ID[i, j + 1] == ch)
                {
                  funct++;
                  if (!Chains[ch].Liberty.Contains(new Point(i, j)))
                    Chains[ch].Liberty.Add(new Point(i, j));
                }
              }
            }
          }
          break;
        // First type of eyes
        case "E1":
          foreach (Point point in Chains[ch].EmptyNeighbourPoints)
          {
            if (Eye1(point, ch))
            {
              funct++;
              if (!Chains[ch].Eyes.Contains(point))
                Chains[ch].Eyes.Add(point);
            }
          }
          break;
        // Second type of eyes
        case "E2":
          foreach (Point point in Chains[ch].EmptyNeighbourPoints)
          {
            if (Eye2(point, ch))
            {
              funct++;
              if (!Chains[ch].Eyes.Contains(point))
                Chains[ch].Eyes.Add(point);
            }
          }
          break;
        // Third type of eyes
        case "E3":
          foreach (Point point in Chains[ch].EmptyNeighbourPoints)
          {
            if (Eye3(point, ch, killed))
            {
              funct++;
              if (!Chains[ch].Eyes.Contains(point))
                Chains[ch].Eyes.Add(point);
            }
          }
          break;
        // Special eyes
        case "SE":
          foreach (Point point in Chains[ch].EmptyNeighbourPoints)
          {
            if (SpecialEye(point, ch))
            {
              if (!Chains[ch].Eyes.Contains(point))
              {
                funct++;
                if (!Chains[ch].SpecialEyes.Contains(point))
                  Chains[ch].SpecialEyes.Add(point);
              }
            }
          }
          break;
        // Eyelikes
        case "EL":
          foreach (Point point in Chains[ch].EmptyNeighbourPoints)
          {
            if (EyeLike(point, ch))
            {
              funct++;
              if (!Chains[ch].EyeLikes.Contains(point))
                Chains[ch].EyeLikes.Add(point);
            }
          }
          break;

        default:
          break;
      }

      return funct;
    }
    private static int HalfConnection(int x, int y, int ch)
    {
      // Determines if point (x, y) is a possible extension for chain ch, albeit disruptable
      int col = Chains[ch].Colour;
      int halfConnection = 0;

      // Diagonal jump: one of the two side points must be empty
      if (Internal(x + 1, y + 1) && ID[x + 1, y + 1] == ch)
      {
        if (Empty(x + 1, y) || Empty(x, y + 1)) halfConnection++;
      }
      if (Internal(x - 1, y + 1) && ID[x - 1, y + 1] == ch)
      {
        if (Empty(x - 1, y) || Empty(x, y + 1)) halfConnection++;
      }
      if (Internal(x + 1, y - 1) && ID[x + 1, y - 1] == ch)
      {
        if (Empty(x + 1, y) || Empty(x, y - 1)) halfConnection++;
      }
      if (Internal(x - 1, y - 1) && ID[x - 1, y - 1] == ch)
      {
        if (Empty(x - 1, y) || Empty(x, y - 1)) halfConnection++;
      }

      // One-point jump: the middle point must be empty
      if (Internal(x + 2, y) && ID[x + 2, y] == ch)
      {
        if (Empty(x + 1, y)) halfConnection++;
      }
      if (Internal(x - 2, y) && ID[x - 2, y] == ch)
      {
        if (Empty(x - 1, y)) halfConnection++;
      }
      if (Internal(x, y + 2) && ID[x, y + 2] == ch)
      {
        if (Empty(x, y + 1)) halfConnection++;
      }
      if (Internal(x, y - 2) && ID[x, y - 2] == ch)
      {
        if (Empty(x, y - 1)) halfConnection++;
      }

      // Knight's jump: the two middle points must be empty, although the matter is complicated and the neighbors' status matters
      if (Internal(x - 1, y + 2) && ID[x - 1, y + 2] == ch)
      {
        if ((Empty(x, y + 1) && Friendly(x, y + 2, col) && !(Enemy(x - 1, y + 1, col) && Enemy(x - 1, y, col))) ||
            (Empty(x, y + 1) && Friendly(x - 1, y + 1, col)) ||
            (Empty(x - 1, y + 1) && Friendly(x - 1, y, col) && !(Enemy(x, y + 1, col) && Enemy(x, y + 2, col))) ||
            (Empty(x - 1, y + 1) && Friendly(x, y + 1, col)))
        {
          halfConnection++;
        }
      }
      if (Internal(x - 1, y - 2) && ID[x - 1, y - 2] == ch)
      {
        if ((Empty(x, y - 1) && Friendly(x, y - 2, col) && !(Enemy(x - 1, y - 1, col) && Enemy(x - 1, y, col))) ||
            (Empty(x, y - 1) && Friendly(x - 1, y - 1, col)) ||
            (Empty(x - 1, y - 1) && Friendly(x - 1, y, col) && !(Enemy(x, y - 1, col) && Enemy(x, y - 2, col))) ||
            (Empty(x - 1, y - 1) && Friendly(x, y - 1, col)))
        {
          halfConnection++;
        }
      }
      if (Internal(x + 1, y - 2) && ID[x + 1, y - 2] == ch)
      {
        if ((Empty(x, y - 1) && Friendly(x, y - 2, col) && !(Enemy(x + 1, y - 1, col) && Enemy(x + 1, y, col))) ||
            (Empty(x, y - 1) && Friendly(x + 1, y - 1, col)) ||
            (Empty(x + 1, y - 1) && Friendly(x + 1, y, col) && !(Enemy(x, y - 1, col) && Enemy(x, y - 2, col))) ||
            (Empty(x + 1, y - 1) && Friendly(x, y - 1, col)))
        {
          halfConnection++;
        }
      }
      if (Internal(x + 1, y + 2) && ID[x + 1, y + 2] == ch)
      {
        if ((Empty(x, y + 1) && Friendly(x, y + 2, col) && !(Enemy(x + 1, y + 1, col) && Enemy(x + 1, y, col))) ||
            (Empty(x, y + 1) && Friendly(x + 1, y + 1, col)) ||
            (Empty(x + 1, y + 1) && Friendly(x + 1, y, col) && !(Enemy(x, y + 1, col) && Enemy(x, y + 2, col))) ||
            (Empty(x + 1, y + 1) && Friendly(x, y + 1, col)))
        {
          halfConnection++;
        }
      }
      // GK initial translation missed final 4 conditions. I added by hand.
      if (Internal(x + 2, y - 1) && ID[x + 2, y - 1] == ch)
      {

        if ((Empty(x + 1, y) && Friendly(x + 2, y, col) && !(Enemy(x + 1, y - 1, col) && Enemy(x, y - 1, col))) || 
            (Empty(x + 1, y) && Friendly(x + 1, y - 1, col)) || 
            (Empty(x + 1, y - 1) && Friendly(x, y - 1, col) && !(Enemy(x + 1, y, col) && Enemy(x + 2, y, col))) || 
            (Empty(x + 1, y - 1) && Friendly(x + 1, y, col)))
        {
          halfConnection++;
        }
      }
      if (Internal(x + 2, y + 1) && ID[x + 2, y + 1] == ch)
      {
        if ((Empty(x + 1, y) && Friendly(x + 2, y, col) && !(Enemy(x + 1, y + 1, col) && Enemy(x, y + 1, col))) || 
            (Empty(x + 1, y) && Friendly(x + 1, y + 1, col)) || 
            (Empty(x + 1, y + 1) && Friendly(x, y + 1, col) && !(Enemy(x + 1, y, col) && Enemy(x + 2, y, col))) || 
            (Empty(x + 1, y + 1) && Friendly(x + 1, y, col)))
        {
          halfConnection++;
        }
      }
      if (Internal(x - 2, y - 1) && ID[x - 2, y - 1] == ch)
      {
        if ((Empty(x - 1, y) && Friendly(x - 2, y, col) && !(Enemy(x - 1, y - 1, col) && Enemy(x, y - 1, col))) || 
          (Empty(x - 1, y) && Friendly(x - 1, y - 1, col)) || 
          (Empty(x - 1, y - 1) && Friendly(x, y - 1, col) && !(Enemy(x - 1, y, col) && Enemy(x - 2, y, col))) || 
          (Empty(x - 1, y - 1) && Friendly(x - 1, y, col)))
        {
          halfConnection++;
        }
      }
      if (Internal(x - 2, y + 1) && ID[x - 2, y + 1] == ch)
      {
        if ((Empty(x - 1, y) && Friendly(x - 2, y, col) && !(Enemy(x - 1, y + 1, col) && Enemy(x, y + 1, col))) || 
          (Empty(x - 1, y) && Friendly(x - 1, y + 1, col)) || 
          (Empty(x - 1, y + 1) && Friendly(x, y + 1, col) && !(Enemy(x - 1, y, col) && Enemy(x - 2, y, col))) || 
          (Empty(x - 1, y + 1) && Friendly(x - 1, y, col)))
        {
          halfConnection++;
        }
      }
      return halfConnection;
    }
    public static bool InAtari(int x, int y, int col, bool free = true)
    {
      // point p is checked for colour col: it is a point "in atari" if a stone of colour col,
      // when put there, is captured at once
      int liberty1 = 0, liberty2 = 0;
      int[,] temp = new int[19, 19]; // Assuming the board size is 19x19 (0-based arrays)

      if (Board[x, y] != 0) return false;
      Array.Copy(Board, temp, Board.Length);
      Board[x, y] = col;
      liberty1 = GroupLib(x, y);
      Array.Copy(OrigBoard, Board, Board.Length);

      if (Board[x, y] == 0)
      {
        // Place the stone again and calculate liberties
        Board[x, y] = col;
        liberty2 = GroupLib(x, y);
      }
      else
      {
        liberty2 = 2;
      }

      Array.Copy(temp, Board, Board.Length);

      // if "free" = false then the point is also checked in the original board (restoring dead strings):
      // that could be useful when counting eyes
      if (free)
      {
        return liberty1 <= 1;
      }
      else
      {
        return liberty1 <= 1 || liberty2 <= 1;
      }
    }
    private static bool Internal(int x, int y)
    {
      // Verifies that point (x, y) is inside the goban
      return x >= 0 && x <= N1 && y >= 0 && y <= N1;
    }
    private static bool Killing(int ch)
    {
      bool good = false;
      int ch1, eyes = 0;
      double meanx = 0, meany = 0;
      int l1, lpot, lpot1, lpot2;
      int z;

      // Is the chain ch a "stealing eyes" type? The well-known configurations are looked for.
      bool killing = false;

      // One stone, two liberties
      if (Chains[ch].Size == 1 && Groups[Chains[ch].Group].ElementCount == 1 && Chains[ch].Liberty.Count == 2)
      {
        good = true;
        var pts = new List<Point>(Chains[ch].Liberty.Cast<Point>());
        lpot1 = 0;

        // The two liberties are filled, one by one, and the liberties of the new chain are counted, then summed.
        foreach (var pt in pts)
        {
          Board[pt.X, pt.Y] = Chains[ch].Colour;
          l1 = GroupLib(pt.X, pt.Y);
          lpot1 += l1;

          // If filling one of the liberties doesn't change the overall number (2), the check is repeated on the new string.
          if (l1 == 2)
          {
            ID[pt.X, pt.Y] = ch;
            Chains[ch].Liberty.Clear();
            Funct(ch, "LS");
            lpot2 = 0;

            foreach (Point pt2 in Chains[ch].Liberty)
            {
              Board[pt2.X, pt2.Y] = Chains[ch].Colour;
              lpot2 += GroupLib(pt2.X, pt2.Y);
              Board[pt2.X, pt2.Y] = 0;
            }

            // If the sum of the new liberties found in the secondary control is more than 2,
            // the original chain is not a "stealing eyes" type.
            if (lpot2 > 2) good = false;
            ID[pt.X, pt.Y] = 0;
          }

          Board[pt.X, pt.Y] = 0;
        }

        // If the sum of the new liberties found in the primary control is more than 4,
        // the chain is not a "stealing eyes" type.
        if (lpot1 > 4) good = false;
      }
      // One stone, three liberties; there is no secondary control.
      if (Chains[ch].Size == 1 && Groups[Chains[ch].Group].ElementCount == 1 && Chains[ch].Liberty.Count == 3)
      {
        lpot = 0;
        foreach (var pt in Chains[ch].Liberty.Cast<Point>())
        {
          Board[pt.X, pt.Y] = Chains[ch].Colour;
          lpot += GroupLib(pt.X, pt.Y);
          Board[pt.X, pt.Y] = 0;
        }

        // If the sum of the new liberties is 6, there is a "pyramid four"; if the sum is 8, we get a "bulky five".
        if (lpot == 6 || lpot == 8) good = true;
      }

      // One stone, four liberties, no secondary control.
      if (Chains[ch].Size == 1 && Groups[Chains[ch].Group].ElementCount == 1 && Chains[ch].Liberty.Count == 4)
      {
        lpot = 0;
        foreach (var pt in Chains[ch].Liberty.Cast<Point>())
        {
          Board[pt.X, pt.Y] = Chains[ch].Colour;
          lpot += GroupLib(pt.X, pt.Y);
          Board[pt.X, pt.Y] = 0;
        }

        // If the sum of the new liberties is 12, there is a "crossed five"; if it is 14, we get the "rabbitty six".
        if (lpot == 12 || lpot == 14) good = true;
      }

      // Two stones, two liberties kill if the liberties are "in atari" points for the chain.
      if (Chains[ch].Size == 2 && Groups[Chains[ch].Group].ElementCount == 1 && Chains[ch].Liberty.Count == 2)
      {
        good = true;
        foreach (var pt in Chains[ch].Liberty.Cast<Point>())
        {
          if (!InAtari(pt.X, pt.Y, Chains[ch].Colour)) good = false;
        }
      }

      // Three stones, one liberty of course kill.
      if (Chains[ch].Size == 3 && Groups[Chains[ch].Group].ElementCount == 1 && Chains[ch].Liberty.Count == 1)
      {
        good = true;
        foreach (var pt in Chains[ch].Liberty.Cast<Point>())
        {
          if (!InAtari(pt.X, pt.Y, Chains[ch].Colour)) good = false;
        }
      }

      // Four stones in a square, two liberties: always kill.
      if (Chains[ch].Size == 4 && Groups[Chains[ch].Group].ElementCount == 1 && Chains[ch].Liberty.Count == 2)
      {
        good = true;
        meanx = 0;
        meany = 0;

        for (z = 1; z <= 4; z++)          // GK loop was for (z = 0; z < 4; z++)
        {
          meanx += Chains[ch].P[z].X;
          meany += Chains[ch].P[z].Y;
        }

        meanx /= 4;
        meany /= 4;

        for (z = 1; z <= 4; z++)          // GK loop was for (z = 0; z < 4; z++)
        {
          if (Math.Abs(meanx - Chains[ch].P[z].X) != 0.5) good = false;
          if (Math.Abs(meany - Chains[ch].P[z].Y) != 0.5) good = false;
        }

        if (good)
        {
          foreach (var pt in Chains[ch].Liberty.Cast<Point>())
          {
            if (!InAtari(pt.X, pt.Y, Chains[ch].Colour)) good = false;
          }
        }
      }

      if (good)
      {
        // Are there eyes in the neighbor chains?
        eyes = 0;
        foreach (var pt in Chains[ch].NeighbourPoints.Cast<Point>())
        {
          ch1 = ID[pt.X, pt.Y];
          if (ch1 != 0 && ch1 != ch)
          {
            // If the killing chain looks like dying before its neighbors, it is worth one eye for them.
            if (Chains[ch1].Status < Chains[ch].Status)
            {
              StringAnalysis(ch1, false, ch);
              eyes += Chains[ch1].Eyes.Count;
            }

            eyes += Groups[Chains[ch1].Group].Eyes.Count;
          }
        }

        // So neighbor chains with no eyes are dead (with one eye they live instead).
        if (eyes == 0) killing = true;

        foreach (var pt in Chains[ch].Liberty.Cast<Point>())
        {
          // z = 0 means the "stealing eyes" chain is black, also worth an eye for white neighbor chains;
          // z = 2 means the opposite.
          z = 2 - (Chains[ch].Colour + 1);
          if (!KillingEyes[z].Contains(pt)) KillingEyes[z].Add(pt);
        }
      }

      return killing;
    }
    private static int KO(int x, int y)
    {
      int col1, col2, col3, col4;
      int colour = 0;   // GK, initialiasation added by me
      int p, v;

      // Check if the point (x, y) is already occupied
      if (Board[x, y] != 0) return Board[x, y];

      // Determine the color of the KO (if it exists)
      col1 = Internal(x + 1, y) ? Board[x + 1, y] : 2;
      col2 = Internal(x - 1, y) ? Board[x - 1, y] : 2;
      col3 = Internal(x, y + 1) ? Board[x, y + 1] : 2;
      col4 = Internal(x, y - 1) ? Board[x, y - 1] : 2;

      switch (Minimum(col1, col2, col3, col4))
      {
        case -1: // All surrounding points are white
          if ((col1 == -1 || col1 == 2) && (col2 == -1 || col2 == 2) &&
              (col3 == -1 || col3 == 2) && (col4 == -1 || col4 == 2))
            colour = -1;
          else
            return 0;
          break;
        case 0: // Not the same color, no KO
          return 0;
        case 1: // All surrounding points are black
          colour = 1;
          break;
        //default:        // GK these lines invented by CoPilot
        //  return 0;
      }

      // Check four possible configurations for KO
      // First configuration
      p = 0; v = 0;
      if (Internal(x - 1, y - 1) && Board[x - 1, y - 1] == -colour) p++;
      if (!Internal(x - 1, y - 1)) v++;
      if (Internal(x, y - 2) && Board[x, y - 2] == -colour) p++;
      if (!Internal(x, y - 2)) v++;
      if (Internal(x + 1, y - 1) && Board[x + 1, y - 1] == -colour) p++;
      if (!Internal(x + 1, y - 1)) v++;
      if (p + v == 3 && p >= 1) return colour;

      // Second configuration
      p = 0; v = 0;
      if (Internal(x + 1, y - 1) && Board[x + 1, y - 1] == -colour) p++;
      if (!Internal(x + 1, y - 1)) v++;
      if (Internal(x + 2, y) && Board[x + 2, y] == -colour) p++;
      if (!Internal(x + 2, y)) v++;
      if (Internal(x + 1, y + 1) && Board[x + 1, y + 1] == -colour) p++;
      if (!Internal(x + 1, y + 1)) v++;
      if (p + v == 3 && p >= 1) return colour;

      // Third configuration
      p = 0; v = 0;
      if (Internal(x - 1, y + 1) && Board[x - 1, y + 1] == -colour) p++;
      if (!Internal(x - 1, y + 1)) v++;
      if (Internal(x, y + 2) && Board[x, y + 2] == -colour) p++;
      if (!Internal(x, y + 2)) v++;
      if (Internal(x + 1, y + 1) && Board[x + 1, y + 1] == -colour) p++;
      if (!Internal(x + 1, y + 1)) v++;
      if (p + v == 3 && p >= 1) return colour;

      // Fourth configuration
      p = 0; v = 0;
      if (Internal(x - 1, y - 1) && Board[x - 1, y - 1] == -colour) p++;
      if (!Internal(x - 1, y - 1)) v++;
      if (Internal(x - 2, y) && Board[x - 2, y] == -colour) p++;
      if (!Internal(x - 2, y)) v++;
      if (Internal(x - 1, y + 1) && Board[x - 1, y + 1] == -colour) p++;
      if (!Internal(x - 1, y + 1)) v++;
      if (p + v == 3 && p >= 1) return colour;

      return 0;
    }
    private static int GroupLib(int X, int Y, bool GLinit = true)
    {
      // given the point (x,y), the stone occupying it, and the chain including the stone,
      // how many liberties this chain has got?
      // the function is recursive, hence the parameter GLinit
      // (= false when the function is called recursively)
      if (GLinit)
      {
        GLColour = Board[X, Y];
        GLTotal = 0;

        for (int i = 0; i <= N1; i++)
        {
          for (int j = 0; j <= N1; j++)
          {
            TempGroup[i, j] = 0;
          }
        }

        if (GLColour == 0) return -1;
      }

      // modScore.vb did this with a switch. GK
      if (Board[X, Y] == GLColour)
      {
        if (TempGroup[X, Y] == 0)
        {
          TempGroup[X, Y] = 1;

          // Recursively check adjacent points
          if (Y > 0) GroupLib(X, Y - 1, false);
          if (Y < N1) GroupLib(X, Y + 1, false);
          if (X > 0) GroupLib(X - 1, Y, false);
          if (X < N1) GroupLib(X + 1, Y, false);
        }
      }
      if (Board[X, Y] == 0)
      {
        if (TempGroup[X, Y] == 0)
        {
          GLTotal += 1;
          TempGroup[X, Y] = 2;
        }
      }

      return GLTotal;
    }
    private static int Minimum(int a, int b, int c, int d)
    {
      // Returns the minimum among four numbers
      int e = a < b ? a : b;
      int f = c < d ? c : d;
      return e < f ? e : f;
    }
    private static bool Enemy(int x, int y, int col)
    {
      // True if point (x, y) is occupied by a stone of enemy color (the opposite of "col")
      if (Internal(x, y) && Board[x, y] == -col)
      {
        return true;
      }
      else
      {
        return false;
      }
    }
    private static bool Eye1(Point p, int ch)
    {
      // First type of eye, when point p is surrounded by stones belonging to the same chain
      int count = 0;
      if (!Internal(p.X, p.Y)) return false;

      if ((p.Y > 0 && ID[p.X, p.Y - 1] == ch) || p.Y == 0) count++;
      if ((p.X < N1 && ID[p.X + 1, p.Y] == ch) || p.X == N1) count++;
      if ((p.Y < N1 && ID[p.X, p.Y + 1] == ch) || p.Y == N1) count++;
      if ((p.X > 0 && ID[p.X - 1, p.Y] == ch) || p.X == 0) count++;

      return count == 4;
    }
    private static bool Eye2(Point p, int ch)
    {
      // Second type of eye
      int count = 0;
      int emptyAngles = 0;
      int x = p.X;
      int y = p.Y;

      // Not a second type of eye if already a first type, as well as there is another eye in some neighbor point
      if (Eye1(p, ch)) return false;
      for (int r = 1; r <= 4; r++)
      {
        if (Chains[ch].Eyes.Contains(VeryClose(r, p.X, p.Y))) return false;
      }

      // The 8 neighbor points are checked
      if (Chains[ch].EmptyNeighbourPoints.Contains(p))
      {
        // Point is on the border
        if (x == 0 || x == N1 || y == 0 || y == N1) count = 3;
        // Point is on the corner
        if ((x == 0 && (y == 0 || y == N1)) || (x == N1 && (y == 0 || y == N1))) count = 5;

        for (int cx = x - 1; cx <= x + 1; cx++)
        {
          for (int cy = y - 1; cy <= y + 1; cy++)
          {
            if (Internal(cx, cy) && !(x == cx && y == cy))
            {
              // One of the neighbor points is occupied by an enemy stone: no eye
              if (Board[cx, cy] == -Chains[ch].Colour) return false;
              // Either one of them is occupied by a friendly stone or is already an eye
              if (Board[cx, cy] == Chains[ch].Colour) count++;
              if (Board[cx, cy] == 0)
              {
                if (Groups[Chains[ch].Group].Eyes.Contains(new Point(cx, cy)))
                {
                  count++;
                }
                else if (Math.Abs(cx - x) == 1 && Math.Abs(cy - y) == 1)
                {
                  emptyAngles++;
                }
              }
            }
          }
        }
      }

      // It's an eye if there are no enemy stones in the neighbor points, at least 6 friendly stones, and at most two angles are empty
      return count + emptyAngles == 8 && count >= 6;
    }
    private static bool Eye3(Point p, int ch, int killed = 0)
    {
      // Third type of eye
      int count = 0;
      int enemyAngles = 0;
      int x = p.X;
      int y = p.Y;
      bool contiguous = false;

      // Special control for third type of eye
      // Stones in a dead chain count like friendly stones, on condition
      // they are NOT contiguous to the possible eye (if they are in the angles it's OK)
      if (killed != 0)
      {
        for (int r = 0; r < Chains[killed].EmptyNeighbourPoints.Count; r++)
        {
          if (p == (Point)Chains[killed].EmptyNeighbourPoints[r])
          {
            for (int s = 1; s <= Chains[killed].Size; s++)
            {
              if (p.X == Chains[killed].P[s].X || p.Y == Chains[killed].P[s].Y)
              {
                contiguous = true;
              }
            }
          }
        }
        if (contiguous) return false;
      }

      // Not a third type of eye if already a first/second type, as well if there is another eye in some neighbor point
      if (Eye1(p, ch) || Eye2(p, ch)) return false;
      for (int r = 1; r <= 4; r++)
      {
        if (Chains[ch].Eyes.Contains(VeryClose(r, p.X, p.Y))) return false;
      }

      // Point is on the border
      if (x == 0 || x == N1 || y == 0 || y == N1) count = 3;
      // Point is on the corner
      if ((x == 0 && (y == 0 || y == N1)) || (x == N1 && (y == 0 || y == N1))) count = 5;

      for (int cx = x - 1; cx <= x + 1; cx++)
      {
        for (int cy = y - 1; cy <= y + 1; cy++)
        {
          if (Internal(cx, cy) && !(x == cx && y == cy))
          {
            // No eye: an enemy stone is in one of the neighbor points, and not in an angle
            if (Board[cx, cy] == -Chains[ch].Colour && (cx == x || cy == y)) return false;
            // No eye: we are on a border and there is an enemy stone in a neighbor point
            if (Board[cx, cy] == -Chains[ch].Colour && (x == 0 || x == N1 || y == 0 || y == N1)) return false;
            // There is an enemy stone in one of the neighbor points, but it is an angle
            if (Board[cx, cy] == -Chains[ch].Colour && (cx != x && cy != y) && (ID[cx, cy] != killed || killed == 0))
            {
              enemyAngles++;
            }
            // The neighbor point is occupied by a friendly stone/an eye/is point "in atari"/there is an enemy stone but surely dead
            else if (Board[cx, cy] == Chains[ch].Colour || Groups[Chains[ch].Group].Eyes.Contains(new Point(cx, cy)) || InAtari(cx, cy, -Chains[ch].Colour, false) || (ID[cx, cy] == killed && killed != 0))
            {
              count++;
            }
          }
        }
      }

      // It's an eye if at least 7 out of 8 neighbor points are friendly (or similar) and the last one is an angle
      return (count == 7 && enemyAngles == 1) || count == 8;
    }
    private static bool FalseEye(Point p, int gr)
    {
      // False eye (useful in sekis): 6 out of 8 neighbor points must be friendly,
      // the remaining ones must be angles and enemies (on the border, 7 and 1 respectively)
      int count = 0;
      int enemyAngles = 0;
      int x = p.X;
      int y = p.Y;
      int cx, cy;

      // Point is on the border
      if (x == 0 || x == N1 || y == 0 || y == N1) count = 3;

      // Point is on the corner
      if ((x == 0 && (y == 0 || y == N1)) || (x == N1 && (y == 0 || y == N1))) count = 5;

      for (cx = x - 1; cx <= x + 1; cx++)
      {
        for (cy = y - 1; cy <= y + 1; cy++)
        {
          if (Internal(cx, cy) && !(x == cx && y == cy))
          {
            if (Board[cx, cy] == -Groups[gr].Colour && (cx != x && cy != y)) enemyAngles++;
            if (Board[cx, cy] == Groups[gr].Colour ||
                Groups[gr].Eyes.Contains(new Point(cx, cy)) ||
                Groups[gr].SpecialEyes.Contains(new Point(cx, cy)) ||
                InAtari(cx, cy, -Groups[gr].Colour))
            {
              count++;
            }
          }
        }
      }

      if (count == 6 && enemyAngles == 2) return true;
      if ((x == 0 || x == N1 || y == 0 || y == N1) && count == 7 && enemyAngles == 1) return true;

      return false;
    }
    private static bool SpecialEye(Point p, int ch)
    {
      // Special eye (if point p is not an eye already)
      int count = 0;
      int enemyAngles = 0;
      int x = p.X;
      int y = p.Y;

      // Check if the point is already an eye
      if (Eye1(p, ch) || Eye2(p, ch) || Eye3(p, ch)) return false;

      // Point on the border
      if (x == 0 || x == N1 || y == 0 || y == N1) count = 3;

      // Point on the corner
      if ((x == 0 && (y == 0 || y == N1)) || (x == N1 && (y == 0 || y == N1))) count = 5;

      for (int cx = x - 1; cx <= x + 1; cx++)
      {
        for (int cy = y - 1; cy <= y + 1; cy++)
        {
          if (Internal(cx, cy) && !(x == cx && y == cy))
          {
            // Not a special eye: one of the neighbor points is enemy and not an angle
            if (Board[cx, cy] == -Chains[ch].Colour && (cx == x || cy == y)) return false;

            // Not a special eye: one of the neighbor points is enemy and we are on the border
            if (Board[cx, cy] == -Chains[ch].Colour && (x == 0 || x == N1 || y == 0 || y == N1)) return false;

            // Enemy stone in an angle
            if (Board[cx, cy] == -Chains[ch].Colour && (cx != x && cy != y)) enemyAngles++;

            // Neighbor point is friendly/eye/special eye/point in atari
            if (Board[cx, cy] == Chains[ch].Colour ||
                Chains[ch].Eyes.Contains(new Point(cx, cy)) ||
                Chains[ch].SpecialEyes.Contains(new Point(cx, cy)) ||
                InAtari(cx, cy, -Chains[ch].Colour))
            {
              count++;
            }
          }
        }
      }

      // Special eye if at least 6 out of 8 neighbor points are friendly and at most one angle is enemy
      return count >= 6 && enemyAngles < 2;
    }
    private static int KOfilling()
    {
      int z = 0;

      for (int i = 0; i <= N1; i++)
      {
        for (int j = 0; j <= N1; j++)
        {
          if (Board[i, j] == 0)
          {
            int KOPOS = KO(i, j);
            bool Taboo = InAtari(i, j, KOPOS);

            // There is a KO in point (i, j)
            if (KOPOS != 0)
            {
              // It is connected
              Board[i, j] = KOPOS;
              z++;
              KOPoint[z].X = i * KOPOS;
              KOPoint[z].Y = j * KOPOS;

              if (Taboo)
              {
                ChainsNumber = ChainDefine();
                int ch = ID[i, j];

                if (Funct(ch, "LS") > 0)
                {
                  Point position = (Point)Chains[ch].Liberty[0];

                  // After connection, does the chain remain in atari?
                  // were that the case, let's try an inner connection
                  Board[position.X, position.Y] = KOPOS;
                  z++;
                  KOPoint[z].X = position.X * KOPOS;
                  KOPoint[z].Y = position.Y * KOPOS;
                  ChainsNumber = ChainDefine();

                  // If the inner connection is not possible...
                  if (!InAtari(position.X, position.Y, KOPOS))
                  {
                    // ... the KO is connected by the opposite color...
                    Board[i, j] = -KOPOS;
                    CheckCaptures(i, j);
                    ChainsNumber = ChainDefine();

                    // ... which is compensated by means of one point of territory
                    Bonus[ID[position.X, position.Y]] = true;
                  }
                }
              }
            }
          }
        }
      }

      return z;
    }
    private static Point RemoveAtari(int s, int l)
    {
      int colour = Chains[s].Colour;
      int[,] dameBackup = new int[19, 19];
      Array.Copy(Board, dameBackup, Board.Length);

      // When a chain remains in atari after dame filling, we may look for dead chains to capture
      for (int i = 0; i <= N1; i++)
      {
        for (int j = 0; j <= N1; j++)
        {
          if (Board[i, j] == 0 && Math.Sign(Intensity[i, j]) == colour)
          {
            Board[i, j] = colour;
            CheckCaptures(i, j);

            if ((Funct(s, "LS") > 1 && l == 1) || (Funct(s, "LS") >= 1 && l == 0))
            {
              Array.Copy(dameBackup, Board, Board.Length);
              return new Point(i, j);
            }

            Array.Copy(dameBackup, Board, Board.Length);
          }
        }
      }

      return new Point(99, 99);   
    }
    private static void ChainRemove(int s)
    {
      // Removal (from the virtual board) of a dead chain
      for (int k = 1; k <= Chains[s].Size; k++)
      {
        int i = Chains[s].P[k].X;
        int j = Chains[s].P[k].Y;

        Board[i, j] = 0;
        ID[i, j] = 0;
        IDGR[i, j] = 0;

        if (Shows)
        {
            ScGui.Plot(Removing, i, j, Color.Red);
        }
      }

      Chains[s].Size = 0;
      Chains[s].Status = 0;
    }
    private static bool EyeLike(Point p, int ch)
    {
      // Eyelike (if point is not eye or special eye already)
      int count = 0;
      int spaces = 0;
      int x = p.X;
      int y = p.Y;

      // Check if the point is already an eye or special eye
      if (Eye1(p, ch) || Eye2(p, ch) || Eye3(p, ch) || SpecialEye(p, ch)) return false;

      // Point on the border
      if (x == 0 || x == N1 || y == 0 || y == N1) count = 3;

      // Point on the corner
      if ((x == 0 && (y == 0 || y == N1)) || (x == N1 && (y == 0 || y == N1))) count = 5;

      for (int cx = x - 1; cx <= x + 1; cx++)
      {
        for (int cy = y - 1; cy <= y + 1; cy++)
        {
          if (Internal(cx, cy) && !(x == cx && y == cy))
          {
            // Not an eyelike: one of the neighbor points is enemy
            if (Board[cx, cy] == -Chains[ch].Colour) return false;

            // Neighbor point is friendly/eye/special eye/point in atari/occupied by a dead enemy stone
            if (Board[cx, cy] == Chains[ch].Colour ||
                Chains[ch].Eyes.Contains(new Point(cx, cy)) ||
                Chains[ch].SpecialEyes.Contains(new Point(cx, cy)) ||
                InAtari(cx, cy, -Chains[ch].Colour) ||
                (Board[cx, cy] == 0 && OrigBoard[cx, cy] == -Chains[ch].Colour))
            {
              count++;
            }

            if (Board[cx, cy] == 0) spaces++;
          }
        }
      }

      // Eyelike if at least 5 out of 8 neighbor points are friendly, and the other ones are empty
      return count >= 5 && count + spaces >= 8;
    }
    private static Point VeryClose(int r, int i, int j)
    {
      // according to r it returns one of the four contiguous points to point (i,j):
      // the points above/under/on the right/on the left of point (i,j)
      switch (r)
      {
        case 1: return new Point(i, j - 1); // left
        case 2: return new Point(i + 1, j); // down
        case 3: return new Point(i, j + 1); // right
        case 4: return new Point(i - 1, j); // up
        default: 
          Console.WriteLine("Error in VeryClose");
          return new Point(i, j);
      }
    }
    private static bool Empty(int x, int y)
    {
      // Point (x, y) is empty if it is empty (of course) or outside the goban
      if ((Internal(x, y) && Board[x, y] == 0) || !Internal(x, y))
      {
        return true;
      }
      else
      {
        return false;
      }
    }

  }
}
