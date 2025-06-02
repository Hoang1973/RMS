using Microsoft.EntityFrameworkCore;
using RMS.Data;
using RMS.Data.Entities;
using RMS.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using static RMS.Data.Entities.Order;

namespace RMS.Services
{
    public interface IKitchenDisplayService
    {
        Task<KitchenDisplayViewModel> GetKitchenDisplayDataAsync();
        Task<bool> StartCookingOrderAsync(int orderId);
        Task<bool> CompleteOrderAsync(int orderId);
        Task<bool> CompleteOrderItem(int orderItemId);
    }

    public class KitchenDisplayService : IKitchenDisplayService
    {
        private readonly RMSDbContext _context;
        private readonly INotificationService _notificationService;

        public KitchenDisplayService(RMSDbContext context, INotificationService notificationService)
        {
            _context = context;
            _notificationService = notificationService;
        }

        public async Task<KitchenDisplayViewModel> GetKitchenDisplayDataAsync()
        {
            var viewModel = new KitchenDisplayViewModel();

            var kitchenOrders = await _context.Orders
                .Include(o => o.Table)
                .Include(o => o.OrderItems).ThenInclude(oi => oi.Dish)
                .Where(o => o.Status == Order.OrderStatus.Pending ||
                            o.Status == Order.OrderStatus.Processing ||
                            o.Status == Order.OrderStatus.Ready)
                .OrderBy(o => o.CreatedAt)
                .ToListAsync();

            foreach (var order in kitchenOrders)
            {
                var kitchenOrderViewModel = new RMS.Models.KitchenOrderViewModel
                {
                    Id = order.Id,
                    TableId = order.TableId,
                    TableName = order.Table?.TableNumber ?? "Mang về",
                    Status = order.Status.ToString(),
                    Created = order.CreatedAt.HasValue
                        ? DateTime.SpecifyKind(order.CreatedAt.Value, DateTimeKind.Utc).ToLocalTime()
                        : (DateTime?)null,
                    Note = order.Note,
                    Items = order.OrderItems.Select(oi => new RMS.Models.KitchenOrderViewModel.Item
                    {
                        Id = oi.Id,
                        DishId = oi.DishId,
                        Name = oi.Dish?.Name ?? "Unknown Dish",
                        Quantity = oi.Quantity,
                        IsCompleted = oi.IsCompleted,
                    }).ToList()
                };

                if (order.Status == Order.OrderStatus.Pending)
                    viewModel.PendingOrders.Add(kitchenOrderViewModel);
                else if (order.Status == Order.OrderStatus.Processing)
                    viewModel.InProgressOrders.Add(kitchenOrderViewModel);
                else if (order.Status == Order.OrderStatus.Ready)
                    viewModel.ReadyOrders.Add(kitchenOrderViewModel);
            }

            return viewModel;
        }

        public async Task<bool> StartCookingOrderAsync(int orderId)
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order == null) return false;

            if (order.Status == OrderStatus.Completed || order.Status == OrderStatus.Cancelled)
            {
                // Không cho cập nhật nữa
                return false;
            }
            order.Status = Order.OrderStatus.Processing;

            await _context.SaveChangesAsync();

            // Notify all clients that an order is now being processed
            await _notificationService.NotifyAllAsync("OrderStatusChanged",
                new { OrderId = orderId, Status = "Processing", Message = $"Order #{orderId} is now being processed" });

            return true;
        }

        public async Task<bool> CompleteOrderAsync(int orderId)
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order == null) return false;

            if (order.Status == OrderStatus.Completed || order.Status == OrderStatus.Cancelled)
            {
                // Không cho cập nhật nữa
                return false;
            }

            order.Status = Order.OrderStatus.Ready;

            await _context.SaveChangesAsync();

            // Notify all clients that an order is ready
            await _notificationService.NotifyAllAsync("OrderStatusChanged",
                new { OrderId = orderId, Status = "Ready", Message = $"Order #{orderId} is now ready" });

            return true;
        }

        public async Task<bool> CompleteOrderItem(int orderItemId)
        {
            var item = await _context.OrderItems.FirstOrDefaultAsync(x => x.Id == orderItemId);
            if (item == null) return false;

            if (item.IsCompleted) return true; // Đã hoàn thành rồi thì không làm gì nữa

            item.IsCompleted = true;

            // Lấy đơn hàng và kiểm tra trạng thái các món
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.Id == item.OrderId);

            if (order == null)
            {
                await _context.SaveChangesAsync();
                return true;
            }

            order.CreatedAt = DateTime.UtcNow; // Cập nhật thời gian tạo đơn hàng nếu cần

            // Nếu tất cả món đã hoàn thành, cập nhật trạng thái đơn hàng
            if (order.OrderItems.All(i => i.IsCompleted))
            {
                order.Status = OrderStatus.Ready;
            }

            await _context.SaveChangesAsync();

            // Gửi thông báo nếu đơn hàng đã sẵn sàng phục vụ
            if (order.OrderItems.All(i => i.IsCompleted))
            {
                await _notificationService.NotifyAllAsync("OrderStatusChanged",
                    new { OrderId = order.Id, Status = "Ready", Message = $"Order #{order.Id} is now ready" });
            }

            return true;
        }
    }
}