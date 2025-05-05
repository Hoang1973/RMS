using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RMS.Data;
using RMS.Models;
using RMS.Services;
using static RMS.Data.Entities.Order;
using static RMS.Data.Entities.Table;

namespace RMS.Controllers
{
    public class OrdersController : Controller
    {
        private readonly IOrderService _orderService;
        private readonly IDishService _dishService;
        private readonly ITableService _tableService;
        private readonly IBillService _billService;
        private readonly IPaymentService _paymentService;
        private readonly RMSDbContext _context;
        private readonly IMapper _mapper;

        public OrdersController
        (IOrderService orderService, 
        IDishService dishService, 
        ITableService tableService, 
        IBillService billService, 
        IPaymentService paymentService, 
        RMSDbContext context, IMapper mapper)
        {
            _orderService = orderService;
            _dishService = dishService;
            _tableService = tableService;
            _billService = billService;
            _paymentService = paymentService;
            _context = context;
            _mapper = mapper;
        }

        // GET: Orders
        public async Task<IActionResult> Index()
        {
            var orders = await _context.Orders
                .Include(o => o.Table)
                .ProjectTo<OrderViewModel>(_mapper.ConfigurationProvider) // Tự động map theo cấu hình
                .ToListAsync();

            return View(orders);
        }

        // GET: Orders/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var model = await _orderService.GetByIdAsync(id.Value);
            if (model == null)
            {
                return NotFound();
            }

            return View(model);
        }



        // GET: Orders/DetailsJson/5
        [HttpGet]
        public async Task<IActionResult> DetailsJson(int id)
        {
            // Lấy thông tin đơn hàng đúng ViewModel, không dùng GetInvoiceAsync
            var model = await _orderService.GetByIdAsync(id);
            if (model == null)
                return NotFound();

            return Json(new {
                id = model.Id,
                tableId = model.TableId,
                tableNumber = model.TableNumber,
                customerId = model.CustomerId,
                status = model.Status.ToString(),
                createdAt = model.CreatedAt?.ToString("HH:mm dd/MM/yyyy"),
                totalAmount = model.TotalAmount,
                dishes = model.Dishes?.Select(d => new {
                    name = d.Name,
                    quantity = d.Quantity,
                    price = d.Price,
                    subtotal = d.Quantity * d.Price
                })
            });
        }

        // GET: Orders/Create
        public async Task<IActionResult> Create()
        {
            ViewData["Dishes"] = new SelectList(await _dishService.GetAllAsync(), "Id", "Name");
            ViewData["Tables"] = new SelectList(await _tableService.GetAvailableTablesAsync(), "Id", "TableNumber");

            // Tạo dictionary chứa giá món ăn
            ViewData["DishPrices"] = _context.Dishes.ToDictionary(d => d.Id.ToString(), d => d.Price);
            return View(new OrderViewModel());
        }

        // POST: Orders/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(OrderViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ModelState.AddModelError("", "Order could not be added. Please check the details and try again.");
                ViewData["Dishes"] = new SelectList(await _dishService.GetAllAsync(), "Id", "Name");
                ViewData["Tables"] = new SelectList(await _tableService.GetAvailableTablesAsync(), "Id", "TableNumber");
                return View(model);
            }
            await _orderService.CreateAsync(model);
            return RedirectToAction(nameof(Index));
        }

        // GET: Orders/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var model = await _orderService.GetByIdAsync(id.Value);
            if (model == null)
            {
                return NotFound();
            }
            ViewData["Dishes"] = new SelectList(await _dishService.GetAllAsync(), "Id", "Name");
            ViewData["Tables"] = new SelectList(await _tableService.GetAvailableTablesAsync(), "Id", "TableNumber");
            ViewData["DishPrices"] = _context.Dishes.ToDictionary(d => d.Id.ToString(), d => d.Price);
            return View(model);
        }

        // POST: Orders/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, OrderViewModel model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    await _orderService.UpdateAsync(model);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await _orderService.ExistsAsync(model.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        // GET: Orders/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var model = await _orderService.GetByIdAsync(id.Value);
            if (model == null)
            {
                return NotFound();
            }

            return View(model);
        }

        // POST: Orders/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            bool deleted = await _orderService.DeleteByIdAsync(id);
            if (!deleted)
            {
                return NotFound();  // Handle case where order was not found
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> CompletePayment(int orderId, int tableId)
        {
            bool success = await _billService.CompletePaymentAndCreateBillAsync(orderId, tableId);
            if (!success) return NotFound();

            return RedirectToAction(nameof(Index));
        }


    }
}
