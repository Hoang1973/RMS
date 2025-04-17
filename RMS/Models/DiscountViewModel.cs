using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using RMS.Data.Entities;

namespace RMS.Models
{
    public class DiscountViewModel
    {
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public Discount.DiscountTypeEnum DiscountType { get; set; }
        [Required]
        [Range(0, double.MaxValue)]
        public decimal DiscountValue { get; set; }

    }
}
