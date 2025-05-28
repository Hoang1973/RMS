using System;

namespace RMS.Models
{
    public class StockHistoryViewModel
    {
        public int Id { get; set; }
        public string IngredientName { get; set; }
        public int StockChange { get; set; } // + nhập, - xuất
        public DateTime StockDate { get; set; }
        public string OperationType => StockChange > 0 ? "Import" : "Export";
    }
}
