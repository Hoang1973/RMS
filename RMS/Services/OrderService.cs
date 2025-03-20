using RMS.Data.Entities;
using RMS.Data;
using RMS.Models;
using Microsoft.EntityFrameworkCore;
using AutoMapper;

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
                var orderItems = model.Dishes
                    .Select(i => new OrderItem
                    {
                        OrderId = entity.Id,
                        DishId = i.DishId,
                        Quantity = i.Quantity,
                        Price = 70000
                    }).ToList();
                entity.OrderItems = orderItems;
            }
            await _context.SaveChangesAsync();
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
