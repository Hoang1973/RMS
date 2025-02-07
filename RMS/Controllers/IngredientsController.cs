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
    public class IngredientsController : Controller
    {
        private readonly IngredientService _ingredientService;

        public IngredientsController(IngredientService ingredientService)
        {
            _ingredientService = ingredientService;
        }

        // GET: Ingredients
        public async Task<IActionResult> Index()
        {
            var models = await _ingredientService.GetAllIngredientsAsync();
            return View(models);
        }

        // GET: Ingredients/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var model = await _ingredientService.GetIngredientByIdAsync(id.Value);
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
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(IngredientViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ModelState.AddModelError("", "Ingredient could not be added. Please check the details and try again.");
                return View(model);
            }

            await _ingredientService.CreateIngredientAsync(model);
            return RedirectToAction(nameof(Index));

        }
        //    ModelState.AddModelError("", "Ingredient could not be added. Please check the details and try again.");

        // GET: Ingredients/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var model = await _ingredientService.GetIngredientByIdAsync(id.Value);
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
                    await _ingredientService.UpdateIngredientAsync(model);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await _ingredientService.IngredientExistsAsync(model.Id))
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

            var model = await _ingredientService.GetIngredientByIdAsync(id.Value);
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
            bool deleted = await _ingredientService.DeleteIngredientByIdAsync(id);
            if (!deleted)
            {
                return NotFound();  // Handle case where ingredient was not found
            }

            return RedirectToAction(nameof(Index));
        }

    }
}
