using Microsoft.AspNetCore.Mvc;
using RMS.Models;
using RMS.Data.Entities;
using System.Linq;
using RMS.Data;

namespace RMS.Controllers
{
    public class PaymentController : Controller
    {
        private readonly RMSDbContext _context;
        public PaymentController(RMSDbContext context)
        {
            _context = context;
        }
        // GET: /Payment
        public IActionResult Index()
        {
            var orders = _context.Orders
                .Where(o => o.Status == RMS.Data.Entities.Order.OrderStatus.Pending)
                .Select(o => new Models.PaymentViewModel
                {
                    OrderId = o.Id,
                    TableNumber = o.Table.TableNumber,
                    CreatedAt = o.CreatedAt ?? DateTime.Now,
                    Dishes = o.OrderItems.Select(oi => new Models.OrderViewModel.DishItem
                    {
                        DishId = oi.DishId,
                        Name = oi.Dish.Name,
                        Quantity = oi.Quantity,
                        Price = oi.Price
                    }).ToList(),
                    Subtotal = o.OrderItems.Sum(oi => oi.Price * oi.Quantity),
                    Vat = o.OrderItems.Sum(oi => oi.Price * oi.Quantity) * 0.08m,
                    Discount = 0,
                    Total = o.OrderItems.Sum(oi => oi.Price * oi.Quantity) * 1.08m,
                    AmountPaid = 0,
                    PaymentMethod = 0
                })
                .ToList();
            return View("Index", orders);
        }

        // GET: /Payment/Order/5
        public IActionResult Order(int id)
        {
            var order = _context.Orders
                .Where(o => o.Id == id)
                .Select(o => new PaymentViewModel
                {
                    OrderId = o.Id,
                    TableNumber = o.Table.TableNumber,
                    CreatedAt = o.CreatedAt ?? DateTime.Now,
                    Dishes = o.OrderItems.Select(oi => new OrderViewModel.DishItem
                    {
                        DishId = oi.DishId,
                        Name = oi.Dish.Name,
                        Quantity = oi.Quantity,
                        Price = oi.Price
                    }).ToList(),
                    Subtotal = o.OrderItems.Sum(oi => oi.Price * oi.Quantity),
                    Vat = o.OrderItems.Sum(oi => oi.Price * oi.Quantity) * 0.08m,
                    Discount = 0,
                    Total = o.OrderItems.Sum(oi => oi.Price * oi.Quantity) * 1.08m
                }).FirstOrDefault();
            if (order == null) return NotFound();
            return View("OrderPayment", order);
        }

        // POST: /Payment/Order
        [HttpPost]
        public IActionResult Order(PaymentViewModel model)
        {
            if (!ModelState.IsValid) return View("OrderPayment", model);
            var order = _context.Orders.FirstOrDefault(o => o.Id == model.OrderId);
            if (order == null) return NotFound();

            var payment = new Payment
            {
                OrderId = model.OrderId,
                AmountPaid = model.AmountPaid,
                PaymentMethod = model.PaymentMethod
            };
            _context.Payments.Add(payment);

            var bill = new Bill
            {
                OrderId = model.OrderId,
                Subtotal = model.Subtotal,
                TotalAmount = model.Total,
                TotalDue = model.Total
            };
            _context.Bills.Add(bill);

            order.Status = RMS.Data.Entities.Order.OrderStatus.Completed;
            _context.SaveChanges();

            // Trả về dữ liệu hóa đơn để hiển thị modal trên trang payment
            return Json(new {
                success = true,
                bill = new {
                    TableNumber = model.TableNumber,
                    CreatedAt = model.CreatedAt.ToString("HH:mm dd/MM/yyyy"),
                    Dishes = model.Dishes,
                    Subtotal = model.Subtotal,
                    Vat = model.Vat,
                    Discount = model.Discount,
                    Total = model.Total
                }
            });
        }
    }
}
