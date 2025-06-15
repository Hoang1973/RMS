using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using RMS.Models;
using RMS.Services;
using RMS.Data;
using RMS.Data.Entities;

namespace RMS.Controllers
{
    [Authorize(Policy = "StaffOnly")]
    public class PaymentsController : Controller
    {
        private readonly IPaymentService _paymentService;
        private readonly IBillService _billService;
        private readonly ITableService _tableService;
        private readonly RMSDbContext _context;
        public PaymentsController(IPaymentService paymentService, IBillService billService, ITableService tableService, RMSDbContext context)
        {
            _paymentService = paymentService;
            _billService = billService;
            _tableService = tableService;
            _context = context;
        }

        // GET: Payments
        public async Task<IActionResult> Index()
        {
            var payments = await _paymentService.GetAllAsync();
            return View(payments);
        }

        // POST: /Payments/Order
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Order([FromBody] RMS.Models.OrderPaymentViewModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { success = false, message = "Dữ liệu không hợp lệ" });
            
            // Đồng bộ hóa logic: luôn gọi BillService để cập nhật trạng thái, tạo bill và payment
            bool success = await _billService.CompletePaymentAndCreateBillAsync(model);
            if (!success)
                return NotFound(new { success = false, message = "Không thể hoàn tất thanh toán" });

            var table = await _tableService.GetByIdAsync(model.TableId);
            if (table == null)
                return NotFound(new { success = false, message = "Không tìm thấy bàn" });

            // Calculate total due
            decimal vatAmount = Math.Round(model.Subtotal * model.VatPercent / 100, 0);
            decimal totalAmount = model.Subtotal + vatAmount;
            decimal discountAmount = 0;
            if (model.DiscountType == "percent")
            {
                discountAmount = Math.Round(totalAmount * model.DiscountValue / 100, 0);
            }
            else
            {
                discountAmount = model.DiscountValue;
            }
            decimal totalDue = totalAmount - discountAmount;
            if (totalDue < 0) totalDue = 0;

            // Tạo payment entity trực tiếp
            var payment = new Payment
            {
                OrderId = model.OrderId,
                AmountPaid = totalDue,
                PaymentMethod = model.PaymentMethod == "cash" ? Payment.PaymentMethodEnum.Cash : Payment.PaymentMethodEnum.Card,
                CreatedAt = DateTime.Now
            };
            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Thanh toán thành công!" });
        }

        // GET: Payments/Details/5
        [HttpGet("Payments/Details/{id}")]
        public async Task<IActionResult> Details(int id)
        {
            var payment = await _paymentService.GetByIdAsync(id);
            if (payment == null)
            {
                return NotFound();
            }

            return Json(payment);
        }
    }
}
