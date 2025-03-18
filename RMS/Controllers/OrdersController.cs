using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using RMS.Data;
using RMS.Data.Entities;
using RMS.Models;
using RMS.Services;

namespace RMS.Controllers
{
    public class OrdersController : Controller
    {
        private readonly IOrderService _orderService;
        private readonly IDishService _dishService;
        private readonly RMSDbContext _context;    

        public OrdersController(IOrderService orderService, IDishService dishService, RMSDbContext context)
        {
            _orderService = orderService;
            _dishService = dishService;
            _context = context;
        }

        // GET: Orders
        public async Task<IActionResult> Index()
        {
            var orders = await _context.Orders
            .Include(o => o.Table) // Include Table để lấy thông tin liên quan
            .Select(o => new OrderViewModel
            {
                Id = o.Id,
                TableId = o.TableId,
                TableNumber = o.Table.TableNumber, // Lấy TableNumber trực tiếp từ navigation property
                CustomerId = o.CustomerId,
            })
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

        // GET: Orders/Create
        public async Task<IActionResult> Create()
        {
            var dishes = await _dishService.GetAllAsync();
            ViewData["Dishes"] = new SelectList(await _dishService.GetAllAsync(), "Id", "Name");
            TempData["DishPrices"] = JsonConvert.SerializeObject(dishes.Select(d => new { Id = d.Id.ToString(), d.Price })); var model = new OrderViewModel
            {
                AvailableTables = new SelectList(_context.Set<Table>().Where(t => t.Status == Table.TableStatus.Available), "Id", "TableNumber")
            };
            return View(model);
        }

        // POST: Orders/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(OrderViewModel model, int[] selectedDishes)
        {
            var dishes = await _dishService.GetAllAsync();
            ViewData["Dishes"] = new SelectList(dishes, "Id", "Name");
            TempData["DishPrices"] = JsonConvert.SerializeObject(dishes.Select(d => new { Id = d.Id.ToString(), d.Price }));
            if (!ModelState.IsValid)
            {
                ModelState.AddModelError("", "Order could not be added. Please check the details and try again.");
                model.AvailableTables = new SelectList(_context.Set<Table>().Where(t => t.Status == Table.TableStatus.Available), "Id", "TableNumber",model.TableId);
                return View(model);
            }

            // Lấy danh sách Dish từ selectedDishes
            if (selectedDishes == null || selectedDishes.Any())
            {
                foreach (var dish in model.Dishes)
                {
                    var dishData = dishes.FirstOrDefault(d => d.Id == dish.DishId);
                    if (dishData != null) dish.Price = dishData.Price;
                }
                model.TotalAmount = model.Dishes.Sum(d => d.Price * d.Quantity);
            }
            else
            {
                model.TotalAmount = 0;
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

    }
}
