using Microsoft.AspNetCore.SignalR;

namespace RMS.Services
{
    public interface INotificationService
    {
        Task NotifyAllAsync(string eventName, object? data = null);
    }

    public class NotificationService : INotificationService
    {
        private readonly IHubContext<NotificationHub> _hubContext;

        public NotificationService(IHubContext<NotificationHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public async Task NotifyAllAsync(string eventName, object? data = null)
        {
            await _hubContext.Clients.All.SendAsync(eventName, data);
        }
    }
}