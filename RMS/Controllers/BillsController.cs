using Microsoft.AspNetCore.Mvc;
using RMS.Models;
using RMS.Services;
using Microsoft.EntityFrameworkCore;

namespace RMS.Controllers
{
    public class BillsController : Controller
    {
        private readonly IBillService _billService;
        
        public BillsController(IBillService billService)
        {
            _billService = billService;
        }

        // GET: Bills
        public async Task<IActionResult> Index()
        {
            var bills = await _billService.GetAllAsync();
            return View(bills);
        }

        public async Task<IActionResult> Details(int id)
        {
            var bill = await _billService.GetByIdAsync(id);
            if (bill == null) return NotFound();
            return View(bill);
        }

        // GET: Bills/GetBillDetails/5
        [HttpGet("Bills/GetBillDetails/{id}")]
        public async Task<IActionResult> GetBillDetails(int id)
        {
            var bill = await _billService.GetByIdAsync(id);
            if (bill == null) return NotFound();

            var billDetails = new
            {
                Id = bill.Id,
                OrderId = bill.OrderId,
                TableNumber = bill.TableNumber,
                Subtotal = bill.Subtotal,
                VatPercent = bill.VatPercent,
                VatAmount = bill.VatAmount,
                TotalAmount = bill.TotalAmount,
                DiscountAmount = bill.DiscountAmount,
                TotalDue = bill.TotalDue,
                AmountPaid = bill.AmountPaid,
                Items = bill.Items?.Select(item => new
                {
                    DishId = item.DishId,
                    Name = item.Name,
                    Quantity = item.Quantity,
                    Price = item.Price,
                    Total = item.Total
                }).ToList()
            };

            return Json(billDetails);
        }
    }
}
