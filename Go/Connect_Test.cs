using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.AspNetCore.SignalR.Protocol;

// following
// https://learn.microsoft.com/en-us/aspnet/core/signalr/dotnet-client?view=aspnetcore-9.0&tabs=visual-studio

namespace GoPlanner
{
  public partial class Connect_Test : Form
  {
    HubConnection connection;
    public Connect_Test()
    {
      InitializeComponent();
      string server;
#if DEBUG
      server = "https://localhost:7122/ChatHub";
#else
      server = "https://go.racingdemon.net/ChatHub";
#endif
      connection = new HubConnectionBuilder()
          .WithUrl(server)
          .Build();
      Console.WriteLine("Connect_Test " + connection.ConnectionId);
      Text = "Connect_Test " + server + ", " + connection.ConnectionId;
      ConnectButton.Enabled = true;
      GetGamesButton.Enabled = false;
      PingButton.Enabled = false;
      // see racingDemon main.ts & Program.cs for setting these
      const int kAIsecs = 100;
      connection.KeepAliveInterval = TimeSpan.FromSeconds(kAIsecs);
      connection.ServerTimeout = TimeSpan.FromSeconds(2 * kAIsecs);

      connection.Closed += async (error) =>
      {
        // Default: if no action, connection is closed after 30s
        // If KeepAliveInterval & ServerTimeout set as above, connection does not
        // get closed.
        int delay = new Random().Next(0, 5) * 1000;
        DateTime now = DateTime.Now;
        Console.WriteLine("connection.Closed " + now.Minute + ":" + now.Second + " error " + error);
        await Task.Delay(delay);
        now = DateTime.Now;
        Console.WriteLine("connection.Closed " + now.Minute + ":" + now.Second + " restarted after " + delay);
        await connection.StartAsync();
      };

    }

    private async void ConnectButton_Click(object sender, EventArgs e)
    {
      connection.On<string[]>("GroupList", (groupList) =>
      {
        AddMessage("Games are");
        foreach (var group in groupList)
        {
          AddMessage(group);
        }
        AddMessage("");
      });

      connection.On<string>("pingBack", GotPing);
     
      try
      {
        await connection.StartAsync();
        ConnectButton.Enabled = false;
        GetGamesButton.Enabled = true;
        PingButton.Enabled = true;
      }
      catch (Exception ex)
      {
        AddMessage("Error starting");
        AddMessage(ex.Message);
      }
    }
    private void GotPing(string id)
    {
      AddMessage("I was Pinged back with id " + id + "\r\n");
    }
    private async void GetGamesButton_Click (object sender, EventArgs e)
    {
      try
      {
        await connection.InvokeAsync("TellMeGroups");
      }
      catch (Exception ex)
      {
        AddMessage(ex.Message);
      }
    }
    private async void PinPingButton_Click(object sender, EventArgs e)
    {
      try
      {
        await connection.InvokeAsync("Ping", "message");
      }
      catch (Exception ex)
      {
        AddMessage(ex.Message);
      }

    }
    private void AddMessage (string mess)
    {
      Invoke(new Action(() => { messagesList.AppendText (mess + "\r\n"); }));
    }
  }
}
