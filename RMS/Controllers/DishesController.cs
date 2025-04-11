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
    public class DishesController : Controller
    {
        private readonly IDishService _dishService;
        private readonly IIngredientService _ingredientService;

        public DishesController(IDishService dishService, IIngredientService ingredientService)
        {
            _dishService = dishService;
            _ingredientService = ingredientService;
        }

        // GET: Dishes
        public async Task<IActionResult> Index()
        {
            var models = await _dishService.GetAllAsync();
            return View(models);
        }

        // GET: Dishes/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var model = await _dishService.GetByIdAsync(id.Value);
            if (model == null)
            {
                return NotFound();
            }

            return View(model);
        }

        // GET: Dishes/Create
        public async Task<IActionResult> Create()
        {
            ViewData["Ingredients"] = new SelectList(await _ingredientService.GetAllAsync(), "Id", "Name");
            return View(new DishViewModel());
        }

        // POST: Dishes/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(DishViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ModelState.AddModelError("", "Dish could not be added. Please check the details and try again.");
                ViewData["Ingredients"] = new SelectList(await _ingredientService.GetAllAsync(), "Id", "Name");
                return View(model);
            }
            await _dishService.CreateAsync(model);
            return RedirectToAction(nameof(Index));
        }

        // GET: Dishes/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var model = await _dishService.GetByIdAsync(id.Value);
            if (model == null)
            {
                return NotFound();
            }
            ViewData["Ingredients"] = new SelectList(await _ingredientService.GetAllAsync(), "Id", "Name");

            return View(model);
        }

        // POST: Dishes/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, DishViewModel model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    await _dishService.UpdateAsync(model);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await _dishService.ExistsAsync(model.Id))
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

        // GET: Dishes/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var model = await _dishService.GetByIdAsync(id.Value);
            if (model == null)
            {
                return NotFound();
            }

            return View(model);
        }

        // POST: Dishes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            bool deleted = await _dishService.DeleteByIdAsync(id);
            if (!deleted)
            {
                return NotFound();
            }

            return RedirectToAction(nameof(Index));
        }

    }
}
