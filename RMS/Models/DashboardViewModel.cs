using System.Collections.Generic;
using RMS.Data.Entities;
using static RMS.Data.Entities.Table;
using static RMS.Data.Entities.Order;

namespace RMS.Models
{
    public class DashboardViewModel
    {
        public IEnumerable<OrderViewModel> TotalOrders { get; set; }
        public IEnumerable<TableViewModel> AvailableTables { get; set; }
        public IEnumerable<IngredientViewModel> LowStockIngredients { get; set; }
        public IEnumerable<BillViewModel> RecentBills { get; set; }

        // Thống kê cơ bản
        public decimal TotalRevenue { get; set; }
        public int TotalOrdersCount { get; set; }
        public int AvailableTablesCount { get; set; }
        public int LowStockIngredientsCount { get; set; }

        // Thống kê doanh thu theo thời gian
        public decimal DailyRevenue { get; set; }
        public decimal WeeklyRevenue { get; set; }
        public decimal MonthlyRevenue { get; set; }

        // Thống kê món ăn bán chạy
        public IEnumerable<TopDish> TopDishes { get; set; }

        // Thống kê doanh thu theo giờ
        public IEnumerable<HourlyRevenueData> HourlyRevenue { get; set; }

        public class TopDish
        {
            public string Name { get; set; }
            public int TotalQuantity { get; set; }
            public decimal TotalRevenue { get; set; }
        }

        public class HourlyRevenueData
        {
            public int Hour { get; set; }
            public decimal Revenue { get; set; }
        }
    }
} 