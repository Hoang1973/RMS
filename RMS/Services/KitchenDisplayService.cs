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
                            o.Status == Order.OrderStatus.Ready) // Thêm Ready
                .OrderBy(o => o.CreatedAt)
                .ToListAsync();

            foreach (var order in kitchenOrders)
            {
                var kitchenOrderViewModel = new KitchenOrderViewModel
                {
                    Id = order.Id,
                    TableId = order.TableId,
                    TableName = order.Table?.TableNumber ?? "Mang về",
                    Status = order.Status.ToString(),
                    Created = order.CreatedAt,
                    Note = order.Note,
                    Items = order.OrderItems.Select(oi => new KitchenOrderItemViewModel
                    {
                        Id = oi.Id,
                        DishId = oi.DishId,
                        Name = oi.Dish?.Name ?? "Unknown Dish",
                        Quantity = oi.Quantity
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
    }
}