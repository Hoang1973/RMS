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
using System.IO;
using Microsoft.AspNetCore.Hosting;

namespace RMS.Controllers
{
    [Authorize(Policy = "AdminOnly")]
    public class DishesController : Controller
    {
        private readonly IDishService _dishService;
        private readonly IIngredientService _ingredientService;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public DishesController(
            IDishService dishService, 
            IIngredientService ingredientService,
            IWebHostEnvironment webHostEnvironment)
        {
            _dishService = dishService;
            _ingredientService = ingredientService;
            _webHostEnvironment = webHostEnvironment;
        }

        private async Task<string> SaveImage(IFormFile imageFile)
        {
            if (imageFile == null || imageFile.Length == 0)
                return null;

            string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images/dishes");
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            string uniqueFileName = Guid.NewGuid().ToString() + "_" + imageFile.FileName;
            string filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await imageFile.CopyToAsync(fileStream);
            }

            return "/images/dishes/" + uniqueFileName;
        }

        // GET: Dishes
        public async Task<IActionResult> Index()
        {
            var models = await _dishService.GetAllAsync();
            ViewData["Ingredients"] = new SelectList(await _ingredientService.GetAllAsync(), "Id", "Name");
            return View(models);
        }

        // GET: Dishes/GetDish/5 (JSON)
        [HttpGet]
        public async Task<IActionResult> GetDish(int id)
        {
            var model = await _dishService.GetByIdAsync(id);
            if (model == null)
            {
                return NotFound();
            }
            return Json(model);
        }

        // POST: Dishes/Create (JSON)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateJson([FromForm] DishViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try 
            {
                if (model.ImageFile != null)
                {
                    model.ImageUrl = await SaveImage(model.ImageFile);
                }
                else if (string.IsNullOrEmpty(model.ImageUrl))
                {
                    // Set a default image URL if no image is provided and no ImageUrl is set
                    model.ImageUrl = "/images/dishes/default-dish.png";
                }
                await _dishService.CreateAsync(model);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Could not create dish. " + ex.Message });
            }
        }

        // POST: Dishes/Edit/5 (JSON)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditJson(int id, [FromForm] DishViewModel model)
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
                if (model.ImageFile != null)
                {
                    // Delete old image if exists
                    var oldDish = await _dishService.GetByIdAsync(id);
                    if (oldDish != null && !string.IsNullOrEmpty(oldDish.ImageUrl))
                    {
                        var oldImagePath = Path.Combine(_webHostEnvironment.WebRootPath, oldDish.ImageUrl.TrimStart('/'));
                        if (System.IO.File.Exists(oldImagePath))
                        {
                            System.IO.File.Delete(oldImagePath);
                        }
                    }

                    model.ImageUrl = await SaveImage(model.ImageFile);
                }

                await _dishService.UpdateAsync(model);
                return Json(new { success = true });
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _dishService.ExistsAsync(model.Id))
                {
                    return NotFound();
                }
                throw;
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Could not update dish. " + ex.Message });
            }
        }

        // POST: Dishes/Delete/5 (JSON)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteJson(int id)
        {
            try
            {
                var dish = await _dishService.GetByIdAsync(id);
                if (dish != null && !string.IsNullOrEmpty(dish.ImageUrl))
                {
                    var imagePath = Path.Combine(_webHostEnvironment.WebRootPath, dish.ImageUrl.TrimStart('/'));
                    if (System.IO.File.Exists(imagePath))
                    {
                        System.IO.File.Delete(imagePath);
                    }
                }

                bool deleted = await _dishService.DeleteByIdAsync(id);
                if (!deleted)
                {
                    return NotFound();
                }
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Could not delete dish. " + ex.Message });
            }
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

            if (model.ImageFile != null)
            {
                model.ImageUrl = await SaveImage(model.ImageFile);
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
                    if (model.ImageFile != null)
                    {
                        // Delete old image if exists
                        var oldDish = await _dishService.GetByIdAsync(id);
                        if (oldDish != null && !string.IsNullOrEmpty(oldDish.ImageUrl))
                        {
                            var oldImagePath = Path.Combine(_webHostEnvironment.WebRootPath, oldDish.ImageUrl.TrimStart('/'));
                            if (System.IO.File.Exists(oldImagePath))
                            {
                                System.IO.File.Delete(oldImagePath);
                            }
                        }

                        model.ImageUrl = await SaveImage(model.ImageFile);
                    }

                    await _dishService.UpdateAsync(model);
                    return RedirectToAction(nameof(Index));
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
            }
            ViewData["Ingredients"] = new SelectList(await _ingredientService.GetAllAsync(), "Id", "Name");
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
            var dish = await _dishService.GetByIdAsync(id);
            if (dish != null && !string.IsNullOrEmpty(dish.ImageUrl))
            {
                var imagePath = Path.Combine(_webHostEnvironment.WebRootPath, dish.ImageUrl.TrimStart('/'));
                if (System.IO.File.Exists(imagePath))
                {
                    System.IO.File.Delete(imagePath);
                }
            }

            bool deleted = await _dishService.DeleteByIdAsync(id);
            if (!deleted)
            {
                return NotFound();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
