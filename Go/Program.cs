using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading;
using System.Windows.Forms;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace GoPlanner
{
  internal static class Program
  {
    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main()
    {
      Application.EnableVisualStyles();
      Application.SetCompatibleTextRenderingDefault(false);
      Thread thread = Thread.CurrentThread;
      thread.SetApartmentState(ApartmentState.STA);
      Application.Run(new GoPlanner());
    }
  }
}
