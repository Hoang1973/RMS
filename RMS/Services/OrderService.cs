using RMS.Data.Entities;
using RMS.Data;
using RMS.Models;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using RMS.Controllers;

namespace RMS.Services
{
    public interface IOrderService : IBaseService<OrderViewModel, Order> { }

    public class OrderService : BaseService<OrderViewModel, Order>, IOrderService
    {
        public OrderService(RMSDbContext context, IMapper mapper)
            : base(context, mapper)
        {
        }

        protected override async Task CreateRelationshipsAsync(Order entity, OrderViewModel model)
        {
            if (model.Dishes != null && model.Dishes.Any())
            {
                var dishIds = model.Dishes.Select(d => d.DishId).ToList();

                // Lấy giá món ăn từ database
                var dishPrices = await _context.Dishes
                    .Where(d => dishIds.Contains(d.Id))
                    .ToDictionaryAsync(d => d.Id, d => d.Price);

                // Gộp món trùng nhau
                var groupedDishes = model.Dishes
                    .GroupBy(d => d.DishId)
                    .Select(g => new { DishId = g.Key, Quantity = g.Sum(d => d.Quantity) })
                    .ToList();

                var orderItems = groupedDishes
                    .Select(i => new OrderItem
                    {
                        OrderId = entity.Id,
                        DishId = i.DishId,
                        Quantity = i.Quantity,
                        Price = dishPrices.ContainsKey(i.DishId) ? dishPrices[i.DishId] * i.Quantity : 0
                    }).ToList();

                entity.OrderItems = orderItems;
            }
        }

        protected override async Task UpdateRelationshipsAsync(Order entity, OrderViewModel model)
        {
            // Delete all existing relationships
            await _context.OrderItems
                .Where(oi => oi.OrderId == entity.Id)
                .ExecuteDeleteAsync();
            // Add the new dishes
            if (model.Dishes != null && model.Dishes.Any())
            {
                var newDishes = model.Dishes.Select(i => new OrderItem
                {
                    OrderId = entity.Id,
                    DishId = i.DishId,
                    Price = i.Price
                });
                await _context.OrderItems.AddRangeAsync(newDishes);
            }
        }
    }

}
