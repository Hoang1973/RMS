using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RMS.Data;
using RMS.Models;
using RMS.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static RMS.Data.Entities.Order;
using static RMS.Data.Entities.Table;

namespace RMS.Controllers
{
    public class DashboardController : Controller
    {
        private readonly RMSDbContext _context;
        private readonly IOrderService _orderService;
        private readonly ITableService _tableService;
        private readonly IIngredientService _ingredientService;
        private readonly IBillService _billService;
        private readonly IDishService _dishService;
        private static readonly TimeZoneInfo VietnamTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");

        public DashboardController(
            RMSDbContext context,
            IOrderService orderService,
            ITableService tableService,
            IIngredientService ingredientService,
            IBillService billService,
            IDishService dishService)
        {
            _context = context;
            _orderService = orderService;
            _tableService = tableService;
            _ingredientService = ingredientService;
            _billService = billService;
            _dishService = dishService;
        }

        public async Task<IActionResult> Index()
        {
            var viewModel = new DashboardViewModel();

            // Lấy tổng doanh thu
            viewModel.TotalRevenue = await _context.Bills.SumAsync(b => b.TotalDue);

            // Lấy tổng số đơn hàng
            viewModel.TotalOrdersCount = await _context.Orders.CountAsync();

            // Lấy số bàn trống
            var availableTables = await _context.Tables
                .Where(t => t.Status == TableStatus.Available)
                .ToListAsync();
            viewModel.AvailableTablesCount = availableTables.Count;
            viewModel.AvailableTables = availableTables.Select(t => new TableViewModel
            {
                Id = t.Id,
                TableNumber = t.TableNumber,
                Status = t.Status
            });

            // Lấy nguyên liệu sắp hết (dưới 10 đơn vị)
            var lowStockIngredients = await _context.Ingredients
                .Where(i => i.StockQuantity < 10)
                .ToListAsync();
            viewModel.LowStockIngredientsCount = lowStockIngredients.Count;
            viewModel.LowStockIngredients = lowStockIngredients.Select(i => new IngredientViewModel
            {
                Id = i.Id,
                Name = i.Name,
                StockQuantity = i.StockQuantity,
                Unit = i.Unit,
                Type = i.Type
            });

            // Lấy đơn hàng gần đây
            var recentOrders = await _context.Orders
                .Include(o => o.Table)
                .OrderByDescending(o => o.CreatedAt)
                .Take(5)
                .ToListAsync();
            viewModel.TotalOrders = recentOrders.Select(o => new OrderViewModel
            {
                Id = o.Id,
                TableNumber = o.Table.TableNumber,
                Status = o.Status,
                CreatedAt = o.CreatedAt
            });

            // Lấy hóa đơn gần đây
            var recentBills = await _context.Bills
                .Include(b => b.Order)
                    .ThenInclude(o => o.Table)
                .OrderByDescending(b => b.CreatedAt)
                .Take(5)
                .ToListAsync();
            viewModel.RecentBills = recentBills.Select(b => new BillViewModel
            {
                Id = b.Id,
                TableNumber = b.Order.Table.TableNumber,
                TotalDue = b.TotalDue,
                CreatedAt = b.CreatedAt
            });

            // Tính doanh thu theo thời gian (sử dụng múi giờ Việt Nam)
            var now = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, VietnamTimeZone);
            var today = now.Date;
            var startOfWeek = today.AddDays(-(int)today.DayOfWeek);
            var startOfMonth = new DateTime(today.Year, today.Month, 1);

            // Lấy tất cả hóa đơn và chuyển đổi múi giờ ở phía client
            var allBills = await _context.Bills
                .Where(b => b.CreatedAt.HasValue)
                .ToListAsync();

            // Tính doanh thu theo ngày
            viewModel.DailyRevenue = allBills
                .Where(b => TimeZoneInfo.ConvertTimeFromUtc(b.CreatedAt.Value, VietnamTimeZone).Date == today)
                .Sum(b => b.TotalDue);

            // Tính doanh thu theo tuần
            viewModel.WeeklyRevenue = allBills
                .Where(b => {
                    var billDate = TimeZoneInfo.ConvertTimeFromUtc(b.CreatedAt.Value, VietnamTimeZone).Date;
                    return billDate >= startOfWeek && billDate <= today;
                })
                .Sum(b => b.TotalDue);

            // Tính doanh thu theo tháng
            viewModel.MonthlyRevenue = allBills
                .Where(b => {
                    var billDate = TimeZoneInfo.ConvertTimeFromUtc(b.CreatedAt.Value, VietnamTimeZone).Date;
                    return billDate >= startOfMonth && billDate <= today;
                })
                .Sum(b => b.TotalDue);

            // Thống kê doanh thu theo giờ hôm nay
            var hourlyRevenue = allBills
                .Where(b => TimeZoneInfo.ConvertTimeFromUtc(b.CreatedAt.Value, VietnamTimeZone).Date == today)
                .GroupBy(b => TimeZoneInfo.ConvertTimeFromUtc(b.CreatedAt.Value, VietnamTimeZone).Hour)
                .Select(g => new DashboardViewModel.HourlyRevenueData
                {
                    Hour = g.Key,
                    Revenue = g.Sum(b => b.TotalDue)
                })
                .OrderBy(h => h.Hour)
                .ToList();

            // Đảm bảo có dữ liệu cho tất cả các giờ trong ngày
            var allHours = Enumerable.Range(0, 24)
                .Select(hour => new DashboardViewModel.HourlyRevenueData
                {
                    Hour = hour,
                    Revenue = hourlyRevenue.FirstOrDefault(h => h.Hour == hour)?.Revenue ?? 0
                })
                .ToList();

            viewModel.HourlyRevenue = allHours;

            // Lấy top món ăn bán chạy
            var topDishes = await _context.OrderItems
                .Include(oi => oi.Dish)
                .GroupBy(oi => new { oi.DishId, oi.Dish.Name })
                .Select(g => new DashboardViewModel.TopDish
                {
                    Name = g.Key.Name,
                    TotalQuantity = g.Sum(oi => oi.Quantity),
                    TotalRevenue = g.Sum(oi => oi.Quantity * oi.Price)
                })
                .OrderByDescending(d => d.TotalQuantity)
                .Take(5)
                .ToListAsync();

            viewModel.TopDishes = topDishes;

            return View(viewModel);
        }

        public async Task<IActionResult> GetRevenueData(string period)
        {
            var now = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, VietnamTimeZone);
            var today = now.Date;
            var startOfWeek = today.AddDays(-(int)today.DayOfWeek);
            var startOfMonth = new DateTime(today.Year, today.Month, 1);

            // Lấy tất cả hóa đơn và chuyển đổi múi giờ ở phía client
            var allBills = await _context.Bills
                .Where(b => b.CreatedAt.HasValue)
                .ToListAsync();

            switch (period)
            {
                case "daily":
                    var hourlyData = allBills
                        .Where(b => TimeZoneInfo.ConvertTimeFromUtc(b.CreatedAt.Value, VietnamTimeZone).Date == today)
                        .GroupBy(b => TimeZoneInfo.ConvertTimeFromUtc(b.CreatedAt.Value, VietnamTimeZone).Hour)
                        .Select(g => new { Hour = g.Key, Revenue = g.Sum(b => b.TotalDue) })
                        .OrderBy(x => x.Hour)
                        .ToList();

                    // Đảm bảo có dữ liệu cho tất cả các giờ
                    var allHours = Enumerable.Range(0, 24)
                        .Select(hour => new { Hour = hour, Revenue = hourlyData.FirstOrDefault(h => h.Hour == hour)?.Revenue ?? 0 })
                        .ToList();

                    return Json(new { 
                        labels = allHours.Select(h => h.Hour + ":00"),
                        revenues = allHours.Select(h => h.Revenue)
                    });

                case "weekly":
                    var weeklyData = allBills
                        .Where(b => {
                            var billDate = TimeZoneInfo.ConvertTimeFromUtc(b.CreatedAt.Value, VietnamTimeZone).Date;
                            return billDate >= startOfWeek && billDate <= today;
                        })
                        .GroupBy(b => TimeZoneInfo.ConvertTimeFromUtc(b.CreatedAt.Value, VietnamTimeZone).DayOfWeek)
                        .Select(g => new { Day = g.Key, Revenue = g.Sum(b => b.TotalDue) })
                        .OrderBy(x => x.Day)
                        .ToList();

                    var daysOfWeek = new[] { "Chủ nhật", "Thứ 2", "Thứ 3", "Thứ 4", "Thứ 5", "Thứ 6", "Thứ 7" };
                    var allDays = Enumerable.Range(0, 7)
                        .Select(day => new { Day = day, Revenue = weeklyData.FirstOrDefault(w => (int)w.Day == day)?.Revenue ?? 0 })
                        .ToList();

                    return Json(new { 
                        labels = allDays.Select(d => daysOfWeek[d.Day]),
                        revenues = allDays.Select(d => d.Revenue)
                    });

                case "monthly":
                    var monthlyData = allBills
                        .Where(b => {
                            var billDate = TimeZoneInfo.ConvertTimeFromUtc(b.CreatedAt.Value, VietnamTimeZone).Date;
                            return billDate >= startOfMonth && billDate <= today;
                        })
                        .GroupBy(b => TimeZoneInfo.ConvertTimeFromUtc(b.CreatedAt.Value, VietnamTimeZone).Day)
                        .Select(g => new { Day = g.Key, Revenue = g.Sum(b => b.TotalDue) })
                        .OrderBy(x => x.Day)
                        .ToList();

                    var daysInMonth = DateTime.DaysInMonth(today.Year, today.Month);
                    var allDaysInMonth = Enumerable.Range(1, daysInMonth)
                        .Select(day => new { Day = day, Revenue = monthlyData.FirstOrDefault(m => m.Day == day)?.Revenue ?? 0 })
                        .ToList();

                    return Json(new { 
                        labels = allDaysInMonth.Select(d => "Ngày " + d.Day),
                        revenues = allDaysInMonth.Select(d => d.Revenue)
                    });

                default:
                    return BadRequest("Invalid period");
            }
        }
    }
} 