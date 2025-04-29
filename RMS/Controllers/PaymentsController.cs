using Microsoft.AspNetCore.Mvc;
using RMS.Models;
using RMS.Services;

namespace RMS.Controllers
{
    public class PaymentsController : Controller
    {
        private readonly IPaymentService _paymentService;
        private readonly IBillService _billService;
        public PaymentsController(IPaymentService paymentService, IBillService billService)
        {
            _paymentService = paymentService;
            _billService = billService;
        }

        // GET: Payments
        public async Task<IActionResult> Index()
        {
            var payments = await _paymentService.GetAllAsync();
            return View(payments);
        }

        // POST: /Payment/Order
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Order([FromBody] RMS.Models.OrderPaymentViewModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { success = false, message = "Dữ liệu không hợp lệ" });

            // 1. Tạo Bill
            var bill = new BillViewModel
            {
                OrderId = model.OrderId,
                Subtotal = model.Subtotal,
                TotalAmount = model.TotalAmount > 0 ? model.TotalAmount : model.TotalDue > 0 ? model.TotalDue : model.AmountPaid,
                TotalDue = model.TotalDue > 0 ? model.TotalDue : model.AmountPaid,
                // Discount có thể bổ sung nếu cần
            };
            await _billService.CreateAsync(bill);

            // 2. Tạo Payment
            var payment = new PaymentViewModel
            {
                OrderId = model.OrderId,
                AmountPaid = model.AmountPaid,
                PaymentMethod = model.PaymentMethod
            };
            await _paymentService.CreateAsync(payment);

            // 3. (Optional) cập nhật trạng thái Order nếu cần
            // ...

            return Json(new { success = true, message = "Thanh toán thành công!" });
        }
    }
}
