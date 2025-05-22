using System;
using System.Collections.Generic;

namespace RMS.Models
{
    public class KitchenDisplayViewModel
    {
        public List<KitchenOrderViewModel> PendingOrders { get; set; } = new List<KitchenOrderViewModel>();
        public List<KitchenOrderViewModel> InProgressOrders { get; set; } = new List<KitchenOrderViewModel>();
    }

    public class KitchenOrderViewModel
    {
        public int Id { get; set; }
        public int? TableId { get; set; }
        public string TableName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime? Created { get; set; }
        public DateTime? StartedCooking { get; set; }
        public List<KitchenOrderItemViewModel> Items { get; set; } = new List<KitchenOrderItemViewModel>();
        public string? Note { get; set; }

        public int ElapsedMinutes
        {
            get
            {
                var startTime = StartedCooking ?? Created ?? DateTime.Now;
                return (int)Math.Floor((DateTime.Now - startTime).TotalMinutes);
            }
        }
    }

    public class KitchenOrderItemViewModel
    {
        public int Id { get; set; }
        public int DishId { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Quantity { get; set; }

    }
}