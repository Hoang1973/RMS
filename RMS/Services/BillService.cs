using RMS.Data;
using RMS.Data.Entities;
using RMS.Models;
using AutoMapper;

namespace RMS.Services
{
    public interface IBillService : IBaseService<BillViewModel, Bill> 
    { 
        Task<bool> CompletePaymentAndCreateBillAsync(int orderId, int tableId, decimal subtotal, decimal discount, decimal total, string paymentMethod);
    }

    public class BillService : BaseService<BillViewModel, Bill>, IBillService
    {
        public BillService(RMSDbContext context, IMapper mapper) : base(context, mapper) { }
        // Custom logic nếu cần
        public async Task<bool> CompletePaymentAndCreateBillAsync(int orderId, int tableId, decimal subtotal, decimal discount, decimal total, string paymentMethod)
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order == null) return false;

            var table = await _context.Tables.FindAsync(tableId);
            if (table == null) return false;

            // 1. Cập nhật trạng thái order
            order.Status = Order.OrderStatus.Completed;

            // 2. Cập nhật trạng thái bàn
            table.Status = Table.TableStatus.Available;

            // 3. Tạo Bill sử dụng ViewModel và logic generic
            var billVM = new BillViewModel
            {
                OrderId = orderId,
                Subtotal = subtotal,
                TotalAmount = total,
                TableNumber = table.TableNumber,
                // Nếu muốn lưu discount/payment method thì cần mở rộng BillViewModel và entity Bill
            };
            await this.CreateAsync(billVM);

            await _context.SaveChangesAsync();
            return true;
        }
    }
}
