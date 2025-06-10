using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace RMS.Hubs
{
    public class NotificationHub : Hub
    {
        public async Task SendNotification(string eventType, object data)
        {
            // Xác định loại thông báo dựa trên eventType
            string notificationType = GetNotificationType(eventType);
            
            await Clients.All.SendAsync("DataChanged", new { 
                Event = eventType, 
                Data = data,
                NotificationType = notificationType
            });
        }

        private string GetNotificationType(string eventType)
        {
            // Phân loại các sự kiện để phát âm thanh phù hợp
            return eventType switch
            {
                "OrderChanged" => "order",
                "PaymentChanged" => "payment",
                "TableChanged" => "table",
                "DishChanged" => "dish",
                "IngredientChanged" => "ingredient",
                _ => "default"
            };
        }
    }
}