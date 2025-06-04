using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RMS.Data;
using RMS.Data.Entities;
using RMS.Models;
using RMS.Services;

namespace RMS.Controllers
{
    [Authorize(Policy = "AdminOnly")]
    public class IngredientsController : Controller
    {
        private readonly IIngredientService _ingredientService;

        public IngredientsController(IIngredientService ingredientService)
        {
            _ingredientService = ingredientService;
        }

        // GET: Ingredients
        public async Task<IActionResult> Index()
        {
            var models = await _ingredientService.GetAllAsync();
            return View(models);
        }

        // GET: Ingredients/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var model = await _ingredientService.GetByIdAsync(id.Value);
            if (model == null)
            {
                return NotFound();
            }

            return View(model);
        }

        // GET: Ingredients/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Ingredients/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(IngredientViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ModelState.AddModelError("", "Ingredient could not be added. Please check the details and try again.");
                return View(model);
            }

            await _ingredientService.CreateAsync(model);
            return RedirectToAction(nameof(Index));

        }

        // GET: Ingredients/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var model = await _ingredientService.GetByIdAsync(id.Value);
            if (model == null)
            {
                return NotFound();
            }

            return View(model);
        }

        // POST: Ingredients/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, IngredientViewModel model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    await _ingredientService.UpdateAsync(model);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await _ingredientService.ExistsAsync(model.Id))
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

        // GET: Ingredients/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var model = await _ingredientService.GetByIdAsync(id.Value);
            if (model == null)
            {
                return NotFound();
            }

            return View(model);
        }

        // POST: Ingredients/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            bool deleted = await _ingredientService.DeleteByIdAsync(id);
            if (!deleted)
            {
                return NotFound();  // Handle case where ingredient was not found
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Ingredients/GetIngredient/5 (JSON)
        [HttpGet]
        public async Task<IActionResult> GetIngredient(int id)
        {
            var model = await _ingredientService.GetByIdAsync(id);
            if (model == null)
            {
                return NotFound();
            }
            return Json(model);
        }

        // POST: Ingredients/CreateJson (JSON)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateJson([FromBody] IngredientViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            
            try 
            {
                await _ingredientService.CreateAsync(model);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Could not create ingredient. " + ex.Message });
            }
        }

        // POST: Ingredients/EditJson/5 (JSON)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditJson(int id, [FromBody] IngredientViewModel model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                await _ingredientService.UpdateAsync(model);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Could not update ingredient. " + ex.Message });
            }
        }
        // GET: Ingredients/Import/{id}
        public async Task<IActionResult> Import(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var model = await _ingredientService.GetByIdAsync(id.Value);
            if (model == null)
            {
                return NotFound();
            }
            ViewData["IngredientName"] = model.Name;
            return View(model);
        }

        // POST: Ingredients/Import/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Import(int id, int quantity)
        {
            if (quantity <= 0)
            {
                ModelState.AddModelError("", "Số lượng nhập phải lớn hơn 0.");
                var model = await _ingredientService.GetByIdAsync(id);
                ViewData["IngredientName"] = model?.Name;
                return View(model);
            }
            await _ingredientService.ImportAsync(id, quantity);
            return RedirectToAction("Index");
        }

        // GET: Ingredients/Export/{id}
        public async Task<IActionResult> Export(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var model = await _ingredientService.GetByIdAsync(id.Value);
            if (model == null)
            {
                return NotFound();
            }
            ViewData["IngredientName"] = model.Name;
            return View(model);
        }

        // POST: Ingredients/Export/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Export(int id, int quantity)
        {
            if (quantity <= 0)
            {
                ModelState.AddModelError("", "Số lượng xuất phải lớn hơn 0.");
                var model = await _ingredientService.GetByIdAsync(id);
                ViewData["IngredientName"] = model?.Name;
                return View(model);
            }
            var ingredient = await _ingredientService.GetByIdAsync(id);
            if (ingredient == null)
            {
                return NotFound();
            }
            if (ingredient.StockQuantity < quantity)
            {
                ModelState.AddModelError("", $"Không đủ tồn kho. Hiện còn: {ingredient.StockQuantity}");
                ViewData["IngredientName"] = ingredient.Name;
                return View(ingredient);
            }
            await _ingredientService.ExportAsync(id, quantity);
            return RedirectToAction("Index");
        }
    }
}
