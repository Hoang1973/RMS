using RMS.Models;
using System.Threading.Tasks;

namespace RMS.Services
{
    public interface IKitchenDisplayService
    {
        Task<KitchenDisplayViewModel> GetKitchenDisplayDataAsync();
        Task<bool> StartCookingOrderAsync(int orderId);
        Task<bool> CompleteOrderAsync(int orderId);
    }
}