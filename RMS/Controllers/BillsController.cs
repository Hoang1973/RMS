using Microsoft.AspNetCore.Mvc;
using RMS.Models;
using RMS.Services;

namespace RMS.Controllers
{
    public class BillsController : Controller
    {
        private readonly IBillService _billService;
        public BillsController(IBillService billService)
        {
            _billService = billService;
        }

        // GET: Bills
        public async Task<IActionResult> Index()
        {
            var bills = await _billService.GetAllAsync();
            return View(bills);
        }
    }
}
