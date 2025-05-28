using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RMS.Data;
using RMS.Models;
using System.Linq;
using System.Threading.Tasks;

namespace RMS.Controllers
{
    public class StockHistoryController : Controller
    {
        private readonly RMSDbContext _context;
        public StockHistoryController(RMSDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var histories = await _context.Stocks
                .Include(s => s.Ingredient)
                .OrderByDescending(s => s.StockDate)
                .Select(s => new StockHistoryViewModel {
                    Id = s.Id,
                    IngredientName = s.Ingredient.Name,
                    StockChange = s.StockChange,
                    StockDate = s.StockDate
                })
                .ToListAsync();
            return View(histories);
        }
    }
}
