using Microsoft.AspNetCore.Mvc;
using RMS.Models;
using RMS.Services;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace RMS.Controllers
{
    [Authorize(Policy = "AdminOnly")]
    public class DiscountsController : Controller
    {
        private readonly IDiscountService _discountService;

        public DiscountsController(IDiscountService discountService)
        {
            _discountService = discountService;
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
            return View(model);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var discount = await _discountService.GetByIdAsync(id);
            if (discount == null) return NotFound();
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
