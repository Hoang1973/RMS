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
        private readonly IIngredientService _ingredientService;
        public OrderService(RMSDbContext context, IMapper mapper, INotificationService notificationService, IIngredientService ingredientService)
            : base(context, mapper, notificationService)
        {
            _ingredientService = ingredientService;
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
            // 1. Kiểm tra tồn kho nguyên liệu
            var requiredIngredients = new Dictionary<int, double>(); // IngredientId -> total needed
            var dishNames = new Dictionary<int, string>();
            if (model.Dishes != null)
            {
                foreach (var dishItem in model.Dishes)
                {
                    // Lấy nguyên liệu và số lượng cần cho món này
                    var dishIngredients = await _context.DishIngredients
                        .Where(di => di.DishId == dishItem.DishId)
                        .ToListAsync();
                    foreach (var di in dishIngredients)
                    {
                        var totalNeeded = Convert.ToDouble(di.QuantityNeeded) * dishItem.Quantity;
                        if (!requiredIngredients.ContainsKey(di.IngredientId))
                            requiredIngredients[di.IngredientId] = 0;
                        requiredIngredients[di.IngredientId] += totalNeeded;
                    }
                    // Lưu tên món để báo lỗi dễ hiểu
                    if (!dishNames.ContainsKey(dishItem.DishId))
                        dishNames[dishItem.DishId] = dishItem.Name;
                }
            }
            var ingredientIds = requiredIngredients.Keys.ToList();
            var stocks = await _context.Ingredients
                .Where(i => ingredientIds.Contains(i.Id))
                .ToDictionaryAsync(i => i.Id, i => new { i.StockQuantity, i.Name });
            foreach (var kvp in requiredIngredients)
            {
                if (!stocks.ContainsKey(kvp.Key) || stocks[kvp.Key].StockQuantity < kvp.Value)
                {
                    var ingName = stocks.ContainsKey(kvp.Key) ? stocks[kvp.Key].Name : $"ID={kvp.Key}";
                    throw new InvalidOperationException($"Không đủ nguyên liệu: {ingName}. Cần {kvp.Value}, còn {stocks.GetValueOrDefault(kvp.Key)?.StockQuantity ?? 0}");
                }
            }
            // 2. Xuất kho nguyên liệu cho từng nguyên liệu sử dụng
            foreach (var kvp in requiredIngredients)
            {
                // Chuyển đổi double về int cho ExportAsync (nếu cần làm tròn)
                int exportQty = (int)Math.Ceiling(kvp.Value);
                await _ingredientService.ExportAsync(kvp.Key, exportQty);
            }
            // Tiếp tục tạo đơn hàng như cũ
            var entity = _mapper.Map<Order>(model);
            await _dbSet.AddAsync(entity);

            var table = await _context.Tables.FindAsync(model.TableId);
            if (table != null)
            {
                table.Status = Table.TableStatus.Occupied;
            }

            await CreateRelationshipsAsync(entity, model);
            await _context.SaveChangesAsync();
            await _notificationService.NotifyAllAsync("OrderChanged", model);
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
