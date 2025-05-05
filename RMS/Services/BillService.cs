using RMS.Data;
using RMS.Data.Entities;
using RMS.Models;
using AutoMapper;

namespace RMS.Services
{
    public interface IBillService : IBaseService<BillViewModel, Bill> 
    { 
        Task<bool> CompletePaymentAndCreateBillAsync(int orderId, int tableId);
    }

    public class BillService : BaseService<BillViewModel, Bill>, IBillService
    {
        public BillService(RMSDbContext context, IMapper mapper) : base(context, mapper) { }
        // Custom logic nếu cần
        public async Task<bool> CompletePaymentAndCreateBillAsync(int orderId, int tableId)
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order == null) return false;

            var table = await _context.Tables.FindAsync(tableId);
            if (table == null) return false;

            // 1. Cập nhật trạng thái order
            order.Status = Order.OrderStatus.Completed;

            // 2. Cập nhật trạng thái bàn
            table.Status = Table.TableStatus.Available;

            // 3. Tạo Bill
            var bill = new Bill
            {
                OrderId = orderId,
                Subtotal = order.TotalAmount, // hoặc tính lại nếu cần
                TotalAmount = order.TotalAmount,
                // Thêm các trường cần thiết khác
            };
            _context.Bills.Add(bill);

            await _context.SaveChangesAsync();
            return true;
        }
    }
}
