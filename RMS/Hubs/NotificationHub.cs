using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace RMS.Hubs
{
    public class NotificationHub : Hub
    {
        public async Task SendNotification(string eventType, object data)
        {
            await Clients.All.SendAsync("DataChanged", new { Event = eventType, Data = data });
        }
    }
}