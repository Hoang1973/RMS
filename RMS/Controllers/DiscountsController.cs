using Microsoft.AspNetCore.Mvc;
using RMS.Models;
using RMS.Services;
using System.Threading.Tasks;

namespace RMS.Controllers
{
    public class DiscountsController : Controller
    {
        private readonly IDiscountService _discountService;
        private readonly IIngredientService _ingredientService;

        public DiscountsController(IDiscountService discountService, IIngredientService ingredientService)
        {
            _discountService = discountService;
            _ingredientService = ingredientService;
        }

        public async Task<IActionResult> Index()
        {
            var discounts = await _discountService.GetAllAsync();
            return View(discounts);
        }

        public async Task<IActionResult> Details(int id)
        {
            var discount = await _discountService.GetByIdAsync(id);
            if (discount == null) return NotFound();
            return View(discount);
        }

        public async Task<IActionResult> Create()
        {
            ViewBag.Ingredients = await _ingredientService.GetAllAsync();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(DiscountViewModel model)
        {
            if (ModelState.IsValid)
            {
                await _discountService.CreateAsync(model);
                return RedirectToAction(nameof(Index));
            }
            ViewBag.Ingredients = await _ingredientService.GetAllAsync();
            return View(model);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var discount = await _discountService.GetByIdAsync(id);
            if (discount == null) return NotFound();
            ViewBag.Ingredients = await _ingredientService.GetAllAsync();
            return View(discount);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(DiscountViewModel model)
        {
            if (ModelState.IsValid)
            {
                await _discountService.UpdateAsync(model);
                return RedirectToAction(nameof(Index));
            }
            ViewBag.Ingredients = await _ingredientService.GetAllAsync();
            return View(model);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var discount = await _discountService.GetByIdAsync(id);
            if (discount == null) return NotFound();
            return View(discount);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _discountService.DeleteByIdAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}
