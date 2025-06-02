using Microsoft.AspNetCore.Mvc;
using RMS.Models;
using RMS.Services;
using System.Threading.Tasks;

namespace RMS.Controllers
{
    public class KitchenDisplayController : Controller
    {
        private readonly IKitchenDisplayService _kitchenDisplayService;

        public KitchenDisplayController(IKitchenDisplayService kitchenDisplayService)
        {
            _kitchenDisplayService = kitchenDisplayService;
        }

        // GET: KitchenDisplay
        public async Task<IActionResult> Index()
        {
            var model = await _kitchenDisplayService.GetKitchenDisplayDataAsync();
            return View(model);
        }

        // POST: KitchenDisplay/StartCooking
        [HttpPost]
        public async Task<IActionResult> StartCooking(int orderId)
        {
            var result = await _kitchenDisplayService.StartCookingOrderAsync(orderId);
            return Json(new { success = result });
        }

        // POST: KitchenDisplay/CompleteOrder
        [HttpPost]
        public async Task<IActionResult> CompleteOrder(int orderId)
        {
            var result = await _kitchenDisplayService.CompleteOrderAsync(orderId);
            return Json(new { success = result });
        }

        [HttpPost]
        public IActionResult CompleteOrderItem(int orderItemId)
        {
            var result = _kitchenDisplayService.CompleteOrderItem(orderItemId);
            return Json(new { success = result });
        }
    }
}