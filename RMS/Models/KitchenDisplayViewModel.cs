using System;
using System.Collections.Generic;

namespace RMS.Models
{
    public class KitchenDisplayViewModel
    {
        public List<KitchenOrderViewModel> PendingOrders { get; set; } = new List<KitchenOrderViewModel>();
        public List<KitchenOrderViewModel> InProgressOrders { get; set; } = new List<KitchenOrderViewModel>();
        public List<KitchenOrderViewModel> ReadyOrders { get; set; } = new List<KitchenOrderViewModel>();
    }

    public class KitchenOrderViewModel
    {
        public int Id { get; set; }
        public int TableId { get; set; }
        public string TableName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime? Created { get; set; }
        public string? Note { get; set; }
        public List<Item> Items { get; set; } = new List<Item>();

        public int ElapsedMinutes
        {
            get
            {
                if (!Created.HasValue) return 0;
                return (int)Math.Floor((DateTime.Now - Created.Value).TotalMinutes);
            }
        }

        public class Item
        {
            public int Id { get; set; }
            public int DishId { get; set; }
            public string Name { get; set; } = string.Empty;
            public int Quantity { get; set; }
            public bool IsCompleted { get; set; }
        }
    }
}