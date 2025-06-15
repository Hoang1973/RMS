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
                    Created = order.CreatedAt,
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
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Dish)
                .FirstOrDefaultAsync(o => o.Id == orderId);
                
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
                new { 
                    orderId = orderId, 
                    status = "Processing", 
                    message = $"Order #{orderId} is now being processed",
                    dishNames = order.OrderItems.Select(oi => oi.Dish?.Name ?? "Unknown Dish").ToList()
                });

            return true;
        }

        public async Task<bool> CompleteOrderAsync(int orderId)
        {
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Dish)
                .FirstOrDefaultAsync(o => o.Id == orderId);
                
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
                new { 
                    orderId = orderId, 
                    status = "Ready", 
                    message = $"Order #{orderId} is now ready",
                    dishNames = order.OrderItems.Select(oi => oi.Dish?.Name ?? "Unknown Dish").ToList()
                });

            return true;
        }

        public async Task<bool> CompleteOrderItem(int orderItemId)
        {
            var item = await _context.OrderItems
                .Include(oi => oi.Dish)
                .Include(oi => oi.Order)
                    .ThenInclude(o => o.OrderItems)
                .FirstOrDefaultAsync(x => x.Id == orderItemId);
            if (item == null) return false;

            if (item.IsCompleted) return true; // Đã hoàn thành rồi thì không làm gì nữa

            item.IsCompleted = true;

            // Lấy đơn hàng và kiểm tra trạng thái các món
            var order = item.Order;

            if (order == null)
            {
                await _context.SaveChangesAsync();
                return true;
            }

            // Kiểm tra xem tất cả món đã hoàn thành chưa
            var allItemsCompleted = order.OrderItems.All(i => i.IsCompleted);

            // Nếu tất cả món đã hoàn thành, cập nhật trạng thái đơn hàng
            if (allItemsCompleted)
            {
                order.Status = OrderStatus.Ready;
            }

            await _context.SaveChangesAsync();

            // Gửi thông báo khi hoàn thành một món
            await _notificationService.NotifyAllAsync("OrderItemCompleted",
                new { 
                    orderId = order.Id, 
                    itemId = item.Id,
                    dishName = item.Dish?.Name ?? "Unknown Dish",
                    message = $"Món {item.Dish?.Name ?? "Unknown"} của đơn hàng #{order.Id} đã hoàn thành",
                    allItemsCompleted = allItemsCompleted
                });

            // Chỉ gửi thông báo đơn hàng hoàn thành khi tất cả món đã hoàn thành
            if (allItemsCompleted)
            {
                await _notificationService.NotifyAllAsync("OrderStatusChanged",
                    new { 
                        orderId = order.Id, 
                        status = "Ready", 
                        message = $"Đơn hàng #{order.Id} đã sẵn sàng phục vụ",
                        dishNames = order.OrderItems.Select(oi => oi.Dish?.Name ?? "Unknown Dish").ToList()
                    });
            }

            return true;
        }
    }
}