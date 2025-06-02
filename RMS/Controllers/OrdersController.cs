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
        private readonly ILogger<OrdersController> _logger;

        // AJAX: Kiểm tra tồn kho nguyên liệu cho danh sách món
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CheckStock([FromBody] List<DishDto> dishes)
        {
            if (dishes == null || !dishes.Any())
                return Json(new { success = true });
            var requiredIngredients = new Dictionary<int, double>();
            foreach (var dishItem in dishes)
            {
                var dishIngredients = await _context.DishIngredients
                    .Where(di => di.DishId == dishItem.DishId)
                    .ToListAsync();
                foreach (var di in dishIngredients)
                {
                    var totalNeeded = Convert.ToDouble(di.QuantityNeeded) * dishItem.Quantity;
                    if (!requiredIngredients.ContainsKey(di.IngredientId))
                        requiredIngredients[di.IngredientId] = 0;
                    requiredIngredients[di.IngredientId] += totalNeeded;
                }
            }
            var ingredientIds = requiredIngredients.Keys.ToList();
            var stocks = await _context.Ingredients
                .Where(i => ingredientIds.Contains(i.Id))
                .ToDictionaryAsync(i => i.Id, i => new { i.StockQuantity, i.Name });
            foreach (var kvp in requiredIngredients)
            {
                if (!stocks.ContainsKey(kvp.Key) || stocks[kvp.Key].StockQuantity < kvp.Value)
                {
                    var ingName = stocks.ContainsKey(kvp.Key) ? stocks[kvp.Key].Name : $"ID={kvp.Key}";
                    return Json(new { success = false, message = $"Không đủ nguyên liệu: {ingName}. Cần {kvp.Value}, còn {stocks.GetValueOrDefault(kvp.Key)?.StockQuantity ?? 0}" });
                }
            }
            return Json(new { success = true });
        }
        public class DishDto
        {
            public int DishId { get; set; }
            public int Quantity { get; set; }
        }

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
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Dish)
                .Include(o => o.Table)
                .ToListAsync();

            var vms = orders.Select(order => new OrderViewModel
            {
                Id = order.Id,
                TableId = order.TableId,
                TableNumber = order.Table?.TableNumber,
                TotalAmount = order.TotalAmount,
                CreatedAt = order.CreatedAt,
                isPaid = order.isPaid,
                Dishes = order.OrderItems != null
                    ? order.OrderItems.Select(x => new OrderViewModel.DishItem
                        {
                            DishId = x.DishId,
                            Name = x.Dish?.Name,
                            Quantity = x.Quantity,
                            Price = x.Price
                        }).ToList()
                    : new List<OrderViewModel.DishItem>(),
            }).ToList();
            // Lấy danh sách món ăn đầy đủ thông tin cho JS
            var dishEntities = await _dishService.GetAllAsync();
            ViewData["DishList"] = dishEntities.Select(d => new {
                Id = d.Id,
                Name = d.Name,
                Type = d.Type.ToString(),
                Price = d.Price
            }).ToList();

            // Lấy danh sách loại món ăn từ enum
            ViewData["DishTypes"] = Enum.GetValues(typeof(RMS.Data.Entities.Dish.DishType))
                .Cast<RMS.Data.Entities.Dish.DishType>()
                .Select(dt => new {
                    Value = dt.ToString(),
                    Text = dt switch
                    {
                        RMS.Data.Entities.Dish.DishType.MainCourse => "Món chính",
                        RMS.Data.Entities.Dish.DishType.Appetizer => "Khai vị",
                        RMS.Data.Entities.Dish.DishType.Dessert => "Tráng miệng",
                        _ => dt.ToString()
                    }
                }).ToList();

            // Bổ sung các ViewData cần thiết cho form popup
            ViewData["Dishes"] = new SelectList(await _dishService.GetAllAsync(), "Id", "Name");
            ViewData["Tables"] = new SelectList(await _tableService.GetAvailableTablesAsync(), "Id", "TableNumber");
            ViewData["DishPrices"] = _context.Dishes.ToDictionary(d => d.Id.ToString(), d => d.Price);

            return View(vms);
        }

        // GET: Orders/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Dish)
                .Include(o => o.Table)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
            {
                return NotFound();
            }

            var vm = new OrderViewModel
            {
                Id = order.Id,
                TableId = order.TableId,
                TableNumber = order.Table?.TableNumber,
                TotalAmount = order.TotalAmount,
                CreatedAt = order.CreatedAt,
                isPaid = order.isPaid,
                Dishes = order.OrderItems != null
                    ? order.OrderItems.Select(x => new OrderViewModel.DishItem
                        {
                            DishId = x.DishId,
                            Name = x.Dish?.Name,
                            Quantity = x.Quantity,
                            Price = x.Price
                        }).ToList()
                    : new List<OrderViewModel.DishItem>(),
            };

            return View(vm);
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
                status = model.Status.ToString(),
                createdAt = model.CreatedAt?.ToString("HH:mm dd/MM/yyyy"),
                totalAmount = model.TotalAmount,
                isPaid = model.isPaid,
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
        // [HttpPost]
        // [ValidateAntiForgeryToken]
        // public async Task<IActionResult> Create(OrderViewModel model)
        // {
        //     if (!ModelState.IsValid)
        //     {
        //         ModelState.AddModelError("", "Order could not be added. Please check the details and try again.");
        //         ViewData["Dishes"] = new SelectList(await _dishService.GetAllAsync(), "Id", "Name");
        //         ViewData["Tables"] = new SelectList(await _tableService.GetAvailableTablesAsync(), "Id", "TableNumber");
        //         return View(model);
        //     }
        //     await _orderService.CreateAsync(model);
        //     return RedirectToAction(nameof(Index));
        // }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(OrderViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return Json(new { success = false, message = "Dữ liệu không hợp lệ", errors });
            }
            try
            {
                await _orderService.CreateAsync(model);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.ToString(), inner = ex.InnerException?.ToString() });
            }
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
        //First option: cash payment with no discount
        public async Task<IActionResult> CompletePayment(int orderId, int tableId)
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order == null) return NotFound();
            // Thanh toán nhanh mặc định: tiền mặt, không giảm giá
            var paymentModel = new RMS.Models.OrderPaymentViewModel {
                OrderId = orderId,
                TableId = tableId,
                Subtotal = order.TotalAmount,
                DiscountValue = 0,
                DiscountType = "amount",
                VatPercent = 8,
                PaymentMethod = "cash"
            };
            bool success = await _billService.CompletePaymentAndCreateBillAsync(paymentModel);

            if (!success) return NotFound();

            return RedirectToAction(nameof(Index));
        }

    }
}
