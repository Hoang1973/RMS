using Microsoft.AspNetCore.Mvc;
using RMS.Models;
using RMS.Services;

namespace RMS.Controllers
{
    public class PaymentsController : Controller
    {
        private readonly IPaymentService _paymentService;
        private readonly IBillService _billService;
        private readonly ITableService _tableService;
        public PaymentsController(IPaymentService paymentService, IBillService billService, ITableService tableService)
        {
            _paymentService = paymentService;
            _billService = billService;
            _tableService = tableService;
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
                return NotFound();
            var table = await _tableService.GetByIdAsync(model.TableId);

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

            // Tạo bản ghi Payment
            var payment = new PaymentViewModel
            {
                OrderId = model.OrderId,
                TableNumber = table.TableNumber,
                AmountPaid = totalDue,  // Set AmountPaid equal to TotalDue
                PaymentMethod = model.PaymentMethod,
            };
            await _paymentService.CreateAsync(payment);

            return Json(new { success = true, message = "Thanh toán thành công!" });
        }
    }
}
