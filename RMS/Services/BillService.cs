using RMS.Data;
using RMS.Data.Entities;
using RMS.Models;
using AutoMapper;

namespace RMS.Services
{
    public interface IBillService : IBaseService<BillViewModel, Bill> 
    { 
        Task<bool> CompletePaymentAndCreateBillAsync(OrderPaymentViewModel model);
    }

    public class BillService : BaseService<BillViewModel, Bill>, IBillService
    {
        public BillService(RMSDbContext context, IMapper mapper) : base(context, mapper) { }
        // Custom logic nếu cần
        public async Task<bool> CompletePaymentAndCreateBillAsync(OrderPaymentViewModel model)
        {
            var order = await _context.Orders.FindAsync(model.OrderId);
            if (order == null) return false;

            var table = await _context.Tables.FindAsync(model.TableId);
            if (table == null) return false;

            // 1. Cập nhật trạng thái order & bàn
            order.Status = Order.OrderStatus.Completed;
            table.Status = Table.TableStatus.Available;

            // 2. Tính toán các giá trị hóa đơn
            decimal vatAmount = Math.Round(model.Subtotal * model.VatPercent / 100, 0);
            decimal totalAmount = model.Subtotal + vatAmount;
            decimal discountAmount = 0;
            if (model.DiscountType == "percent")
            {
                discountAmount = Math.Round(totalAmount * model.DiscountValue / 100, 0);
            }
            else
            {
                discountAmount = model.DiscountValue;
            }
            decimal totalDue = totalAmount - discountAmount;
            if (totalDue < 0) totalDue = 0;

            // 3. Tạo BillViewModel chuẩn hóa
            var billVM = new BillViewModel
            {
                OrderId = model.OrderId,
                Subtotal = model.Subtotal,
                VatPercent = model.VatPercent,
                VatAmount = vatAmount,
                TotalAmount = totalAmount,
                DiscountAmount = discountAmount,
                TotalDue = totalDue,
                TableNumber = table.TableNumber,
                AmountPaid = totalDue,
            };
            await this.CreateAsync(billVM);

            await _context.SaveChangesAsync();
            return true;
        }
    }
}
