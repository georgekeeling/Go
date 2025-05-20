using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Windows.ApplicationModel.Appointments;

namespace GoPlanner
{
  internal static class ScGui
  {
    private const int bSide = 19;
    public static Color[,] Territories = new Color[bSide, bSide];
    public static Color[,] Connections = new Color[bSide, bSide];
    public static Color[,] Removals = new Color[bSide, bSide];
    private static string[] sTypes = { "Stone", "MiniStone" , "Territory", "Connection", "Removing" };
    public static bool haveScore = false;
    public static GoPlanner gp;
    private const string bullshit = "Thinking";
    private static string progressMeter = bullshit;
    private static int doEventsCalls = 0;
    public static void ResetImage()
    {
      progressMeter = bullshit;
      doEventsCalls = 0;
      haveScore = true;
      for (int i = 0; i < bSide; i++)
      {
        for (int j = 0; j < bSide; j++)
        {
          Territories[i, j] = Color.Empty;
          Connections[i, j] = Color.Empty;
          Removals[i, j] = Color.Empty;
        }
      }
    }
    public static void Plot(int type, int x, int y, Color col)
    {
      switch (type)
      {
        case 2:
          Territories[x, y] = col;
          break;
        case 3:
          Connections[x, y] = col;
          break;
        case 4:
          Removals[x, y] = col;
          break;
        default:
          Console.WriteLine($"Plot Error: Invalid type {type}");
          return;
      }
      //if (ReportTypes[x, y] != -1)
      //{
      //  Console.Write("Plot duplicate: ");
      //}
      //Console.WriteLine($"Plot at [{x}, {y}] type {sTypes[type]} {col}.");
    }
    public static void Score (string message)
    {
      Console.Write(message);
    }
    public static void DoEvents()
    {
      // Process any pending events
      if (doEventsCalls == 0)
      {
        // If we do this on every entry, it slows things a lot.
        progressMeter += ".";
        if (progressMeter.Length > 30) progressMeter = bullshit;
        gp.statusM.Set(progressMeter);
      }
      if (doEventsCalls++ > 100) doEventsCalls = 0;
      System.Windows.Forms.Application.DoEvents();
    }

  }
 
}
