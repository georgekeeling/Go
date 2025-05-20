using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace SignalRChat.Hubs
{
  public class ChatHub : Hub
  {
    public override Task OnConnectedAsync()
    {
      Debug.WriteLine($"Client connected: {Context.ConnectionId}");
      return base.OnConnectedAsync();
    }
    public override Task OnDisconnectedAsync(Exception exception)
    {
      Debug.WriteLine($"Client disconnected: {Context.ConnectionId}");
      return base.OnDisconnectedAsync(exception);
    }
    public async Task Ping(string message)
    {
      var cID = Context.ConnectionId;
      Debug.WriteLine("ping " + message);
      await Clients.Caller.SendAsync("pingBack", cID);           // that works
      // await Clients.Client(cID).SendAsync("pingBack", cID);   // that also works
    }
    public async Task TellMeGroups()
    {
      var cID = Context.ConnectionId;
      Debug.WriteLine("TellMeGroups");
      string[] fakeGroups = ["There are no", "games", "on this server. Ha!"];
      await Clients.Caller.SendAsync("GroupList", fakeGroups); 
    }
  }
}
