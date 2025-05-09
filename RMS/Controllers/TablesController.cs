using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RMS.Data;
using RMS.Data.Entities;
using RMS.Models;
using RMS.Services;

namespace RMS.Controllers
{
    public class TablesController : Controller
    {
        private readonly ITableService _tableService;
        private readonly IIngredientService _ingredientService;
        private readonly RMSDbContext _context;

        public TablesController(ITableService tableService, IIngredientService ingredientService, RMSDbContext context)
        {
            _tableService = tableService;
            _ingredientService = ingredientService;
            _context = context;
        }

        // GET: Tables
        public async Task<IActionResult> Index()
        {
            var models = await _tableService.GetAllAsync();
            return View(models);
        }

        // GET: Tables/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var model = await _tableService.GetByIdAsync(id.Value);
            if (model == null)
            {
                return NotFound();
            }

            // Check if the request is AJAX
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new
                {
                    id = model.Id,
                    tableNumber = model.TableNumber,
                    capacity = model.Capacity,
                    status = (int)model.Status,
                    createdAt = model.CreatedAt,
                    updatedAt = model.UpdatedAt
                });
            }

            return View(model);
        }

        // GET: Tables/Create
        public async Task<IActionResult> Create()
        {
            ViewData["Ingredients"] = new SelectList(await _ingredientService.GetAllAsync(), "Id", "Name");
            return View(new TableViewModel());
        }

        // POST: Tables/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TableViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ModelState.AddModelError("", "Table could not be added. Please check the details and try again.");
                //ViewData["Ingredients"] = new SelectList(await _ingredientService.GetAllAsync(), "Id", "Name");
                return View(model);
            }
            await _tableService.CreateAsync(model);
            return RedirectToAction(nameof(Index));
        }

        // GET: Tables/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var model = await _tableService.GetByIdAsync(id.Value);
            if (model == null)
            {
                return NotFound();
            }
            //ViewData["Ingredients"] = new SelectList(await _ingredientService.GetAllAsync(), "Id", "Name");

            return View(model);
        }

        // POST: Tables/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, TableViewModel model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    await _tableService.UpdateAsync(model);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await _tableService.ExistsAsync(model.Id))
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

        // GET: Tables/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var model = await _tableService.GetByIdAsync(id.Value);
            if (model == null)
            {
                return NotFound();
            }

            return View(model);
        }

        // POST: Tables/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            bool deleted = await _tableService.DeleteByIdAsync(id);
            if (!deleted)
            {
                return NotFound();
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> SaveLayout([FromBody] List<TableLayoutModel> layout)
        {
            try
            {
                foreach (var item in layout)
                {
                    var table = await _context.Tables.FindAsync(item.TableId);
                    if (table != null)
                    {
                        table.PositionX = item.X;
                        table.PositionY = item.Y;
                    }
                }
                await _context.SaveChangesAsync();
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetLayout()
        {
            try
            {
                var layout = await _context.Tables
                    .Where(t => t.PositionX.HasValue && t.PositionY.HasValue)
                    .Select(t => new TableLayoutModel
                    {
                        TableId = t.Id,
                        X = t.PositionX.Value,
                        Y = t.PositionY.Value
                    })
                    .ToListAsync();

                return Json(layout);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }

    public class TableLayoutModel
    {
        public int TableId { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
    }
}
