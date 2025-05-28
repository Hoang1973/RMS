using RMS.Data.Entities;
using RMS.Data;
using RMS.Models;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace RMS.Services
{
    public interface IIngredientService : IBaseService<IngredientViewModel, Ingredient>
    {
        Task<IngredientViewModel> ImportAsync(int id, int quantity);
        Task<IngredientViewModel> ExportAsync(int id, int quantity);
    }

    public class IngredientService : BaseService<IngredientViewModel, Ingredient>, IIngredientService
    {
        public IngredientService(RMSDbContext context, IMapper mapper, INotificationService notificationService)
            : base(context, mapper, notificationService)
        {
        }

        public async Task<IngredientViewModel> ImportAsync(int id, int quantity)
        {
            var ingredient = await _context.Set<Ingredient>().FindAsync(id);
            if (ingredient == null)
            {
                throw new KeyNotFoundException($"Ingredient with ID {id} not found");
            }

            ingredient.StockQuantity += quantity;

            // Ghi log lịch sử nhập kho
            _context.Stocks.Add(new Stock {
                IngredientId = id,
                StockChange = quantity,
                StockDate = DateTime.Now
            });

            await _context.SaveChangesAsync();
            return _mapper.Map<IngredientViewModel>(ingredient);
        }

        public async Task<IngredientViewModel> ExportAsync(int id, int quantity)
        {
            var ingredient = await _context.Set<Ingredient>().FindAsync(id);
            if (ingredient == null)
            {
                throw new KeyNotFoundException($"Ingredient with ID {id} not found");
            }
            if (ingredient.StockQuantity < quantity)
            {
                throw new InvalidOperationException($"Not enough stock to export. Current: {ingredient.StockQuantity}, Requested: {quantity}");
            }
            ingredient.StockQuantity -= quantity;

            // Ghi log lịch sử xuất kho
            _context.Stocks.Add(new Stock {
                IngredientId = id,
                StockChange = -quantity,
                StockDate = DateTime.Now
            });

            await _context.SaveChangesAsync();
            return _mapper.Map<IngredientViewModel>(ingredient);
        }
    }
}
