using RMS.Data.Entities;
using RMS.Data;
using RMS.Models;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using RMS.Controllers;
using static RMS.Data.Entities.Table;

namespace RMS.Services
{
    public interface IOrderService : IBaseService<OrderViewModel, Order> 
    {
        Task<bool> CompletePaymentAsync(int orderId, int tableId);
    }

    public class OrderService : BaseService<OrderViewModel, Order>, IOrderService
    {
        public OrderService(RMSDbContext context, IMapper mapper)
            : base(context, mapper)
        {
        }

        public override async Task<OrderViewModel?> GetByIdAsync(int id)
        {
            var order = await _context.Orders
                .Include(o => o.Table)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Dish)
                .FirstOrDefaultAsync(o => o.Id == id);
            if (order == null) return null;
            var viewModel = _mapper.Map<OrderViewModel>(order);
            // TableNumber
            viewModel.TableNumber = order.Table?.TableNumber;
            // Dishes
            viewModel.Dishes = order.OrderItems.Select(oi => new OrderViewModel.DishItem
            {
                DishId = oi.DishId,
                Name = oi.Dish?.Name ?? string.Empty,
                Quantity = oi.Quantity,
                Price = oi.Dish?.Price ?? 0
            }).ToList();
            return viewModel;
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

        public override async Task CreateAsync(OrderViewModel model)
        {
            var entity = _mapper.Map<Order>(model);
            await _dbSet.AddAsync(entity);

            var table = await _context.Tables.FindAsync(model.TableId);
            if (table != null)
            {
                table.Status = Table.TableStatus.Occupied;
            }

            await CreateRelationshipsAsync(entity, model);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> CompletePaymentAsync(int orderId, int tableId)
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order == null) return false;

            var table = await _context.Tables.FindAsync(tableId);
            if (table == null) return false;

            order.Status = Order.OrderStatus.Completed;
            table.Status = TableStatus.Available;

            await _context.SaveChangesAsync();
            return true;
        }


    }

}
