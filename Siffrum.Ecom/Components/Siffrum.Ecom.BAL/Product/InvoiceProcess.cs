using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Siffrum.Ecom.BAL.ExceptionHandler;
using Siffrum.Ecom.BAL.Foundation.Base;
using Siffrum.Ecom.BAL.LoginUsers;
using Siffrum.Ecom.DAL.Context;
using Siffrum.Ecom.DomainModels.Enums;
using Siffrum.Ecom.DomainModels.v1;
using Siffrum.Ecom.ServiceModels.Enums;
using Siffrum.Ecom.ServiceModels.Foundation.Base.CommonResponseRoot;
using Siffrum.Ecom.ServiceModels.Foundation.Base.Enums;
using Siffrum.Ecom.ServiceModels.Foundation.Base.Interfaces;
using Siffrum.Ecom.ServiceModels.v1;

namespace Siffrum.Ecom.BAL.Product
{
    public class InvoiceProcess : SiffrumBalOdataBase<InvoiceSM>
    {
        private readonly ILoginUserDetail _loginUserDetail;
        private readonly UserProcess _userProcess;
        private readonly UserAddressProcess _userAddressProcess;

        public InvoiceProcess(
            IMapper mapper,
            ApiDbContext apiDbContext, UserAddressProcess userAddressProcess, UserProcess userProcess,
            ILoginUserDetail loginUserDetail)
            : base(mapper, apiDbContext)
        {
            _loginUserDetail = loginUserDetail;
            _userAddressProcess = userAddressProcess;
            _userProcess = userProcess;
        }

        #region ODATA

        public override async Task<IQueryable<InvoiceSM>> GetServiceModelEntitiesForOdata()
        {
            IQueryable<InvoiceDM> entitySet = _apiDbContext.Invoice
                .AsNoTracking();

            return await base.MapEntityAsToQuerable<InvoiceDM, InvoiceSM>(_mapper, entitySet);
        }

        #endregion

        // =========================================================
        // CREATE INVOICE (Called after Payment Success)
        // =========================================================

        public async Task<InvoiceSM> CreateInvoiceFromOrderAsync(long orderId)
        {
            using var transaction = await _apiDbContext.Database.BeginTransactionAsync();

            try
            {
                var order = await _apiDbContext.Order
                    .FirstOrDefaultAsync(x => x.Id == orderId);

                if (order == null)
                    throw new SiffrumException(
                        ApiErrorTypeSM.InvalidInputData_NoLog,
                        "Order not found");

                if (order.PaymentStatus != PaymentStatusDM.Paid)
                    throw new SiffrumException(
                        ApiErrorTypeSM.InvalidInputData_NoLog,
                        "Invoice can only be generated for paid orders");

                var existingInvoice = await _apiDbContext.Invoice
                    .FirstOrDefaultAsync(x => x.OrderId == orderId);

                if (existingInvoice != null)
                    return _mapper.Map<InvoiceSM>(existingInvoice);

                var invoice = new InvoiceDM
                {
                    TransactionId = order.TransactionId,
                    InvoiceDate = DateTime.UtcNow,
                    OrderId = order.Id,
                    Amount = order.Amount,
                    PaymentStatus = order.PaymentStatus,
                    OrderStatus = order.OrderStatus,
                    Currency = order.Currency,
                    RazorpayInvoiceId = null,
                    CreatedBy = _loginUserDetail.LoginId,
                    CreatedAt = DateTime.UtcNow
                };

                await _apiDbContext.Invoice.AddAsync(invoice);
                await _apiDbContext.SaveChangesAsync();
                await transaction.CommitAsync();

                return _mapper.Map<InvoiceSM>(invoice);
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        // =========================================================
        // USER - GET MY INVOICES
        // =========================================================

        public async Task<List<InvoiceSM>> GetMyInvoicesAsync(
            long userId,
            int skip,
            int top)
        {
            var invoices = await _apiDbContext.Invoice
                .AsNoTracking()
                .Include(x => x.Order)
                .Where(x => x.Order.UserId == userId)
                .OrderByDescending(x => x.Id)
                .Skip(skip)
                .Take(top)
                .ToListAsync();

            return _mapper.Map<List<InvoiceSM>>(invoices);
        }

        public async Task<IntResponseRoot> GetMyInvoicesCountAsync(long userId)
        {
            var count = await _apiDbContext.Invoice
                .Include(x => x.Order)
                .Where(x => x.Order.UserId == userId)
                .CountAsync();

            return new IntResponseRoot(count, "Total Invoices");
        }

        public async Task<InvoiceExtendedSM> GetMyInvoiceByIdAsync(long id, long userId)
        {
            var invoice = await _apiDbContext.Invoice
                .AsNoTracking()
                .Include(x => x.Order)
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    x.Order.UserId == userId);

            if (invoice == null)
                throw new SiffrumException(
                    ApiErrorTypeSM.InvalidInputData_NoLog,
                    "Invoice not found");
            return await GetInvoiceExtended(id);
        }

        // =========================================================
        // ADMIN - GET ALL
        // =========================================================

        public async Task<List<InvoiceSM>> GetAllAsync(int skip, int top)
        {
            var invoices = await _apiDbContext.Invoice
                .AsNoTracking()
                .OrderByDescending(x => x.Id)
                .Skip(skip)
                .Take(top)
                .ToListAsync();

            return _mapper.Map<List<InvoiceSM>>(invoices);
        }

        public async Task<IntResponseRoot> GetCountAsync()
        {
            var count = await _apiDbContext.Invoice.CountAsync();
            return new IntResponseRoot(count, "Total Invoices");
        }

        public async Task<InvoiceExtendedSM> GetByIdAsync(long id)
        {
            var invoice = await _apiDbContext.Invoice
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id);

            if (invoice == null)
                throw new SiffrumException(
                    ApiErrorTypeSM.InvalidInputData_NoLog,
                    "Invoice not found");
            return await GetInvoiceExtended(id);
            //return _mapper.Map<InvoiceSM>(invoice);
        }

        public async Task<InvoiceExtendedSM> GetByOrderIdAsync(long orderId)
        {
            var invoice = await _apiDbContext.Invoice
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.OrderId == orderId);

            if (invoice == null)
                throw new SiffrumException(
                    ApiErrorTypeSM.InvalidInputData_NoLog,
                    "Invoice not found");
            return await GetInvoiceExtended(invoice.Id);
            //return _mapper.Map<InvoiceSM>(invoice);
        }

        // =========================================================
        // UPDATE STATUS (Admin Sync With Order)
        // =========================================================

        public async Task<BoolResponseRoot> UpdatePaymentStatusAsync(
            long invoiceId,
            PaymentStatusSM status)
        {
            var invoice = await _apiDbContext.Invoice
                .Include(x => x.Order)
                .FirstOrDefaultAsync(x => x.Id == invoiceId);

            if (invoice == null)
                throw new SiffrumException(
                    ApiErrorTypeSM.InvalidInputData_NoLog,
                    "Invoice not found");

            invoice.PaymentStatus = (PaymentStatusDM)status;
            invoice.Order.PaymentStatus = (PaymentStatusDM)status;

            await _apiDbContext.SaveChangesAsync();

            return new BoolResponseRoot(true, "Payment status updated");
        }

        public async Task<BoolResponseRoot> UpdateOrderStatusAsync(
            long invoiceId,
            OrderStatusSM status)
        {
            var invoice = await _apiDbContext.Invoice
                .Include(x => x.Order)
                .FirstOrDefaultAsync(x => x.Id == invoiceId);

            if (invoice == null)
                throw new SiffrumException(
                    ApiErrorTypeSM.InvalidInputData_NoLog,
                    "Invoice not found");

            invoice.OrderStatus = (OrderStatusDM)status;
            invoice.Order.OrderStatus = (OrderStatusDM)status;

            await _apiDbContext.SaveChangesAsync();

            return new BoolResponseRoot(true, "Order status updated");
        }

        // =========================================================
        // DELETE (ADMIN ONLY)
        // =========================================================

        public async Task<DeleteResponseRoot> DeleteAsync(long id)
        {
            var invoice = await _apiDbContext.Invoice
                .FirstOrDefaultAsync(x => x.Id == id);

            if (invoice == null)
                throw new SiffrumException(
                    ApiErrorTypeSM.InvalidInputData_NoLog,
                    "Invoice not found");

            _apiDbContext.Invoice.Remove(invoice);
            await _apiDbContext.SaveChangesAsync();

            return new DeleteResponseRoot(true, "Invoice deleted successfully");
        }

        public async Task<InvoiceExtendedSM> GetInvoiceExtended(long invoiceId)
        {
            // 🔹 Validate input
            if (invoiceId <= 0)
            {
                throw new SiffrumException(
                    ApiErrorTypeSM.InvalidInputData_NoLog,
                    "Invalid invoice id");
            }

            // 🔹 Fetch invoice
            var invoice = await _apiDbContext.Invoice.FindAsync(invoiceId);

            if (invoice == null)
            {
                throw new SiffrumException(
                    ApiErrorTypeSM.InvalidInputData_NoLog,
                    $"Invoice with Id {invoiceId} not found",
                    "Invoice not found");
            }

            // 🔹 Fetch order (safe)
            var order = await _apiDbContext.Order
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == invoice.OrderId);

            if (order == null)
            {
                throw new SiffrumException(
                    ApiErrorTypeSM.InvalidInputData_Log,
                    $"Order not found for InvoiceId {invoiceId}",
                    "Associated order not found");
            }

            // 🔹 Fetch user
            var user = await _userProcess.GetByIdAsync(order.UserId);

            if (user == null)
            {
                throw new SiffrumException(
                    ApiErrorTypeSM.InvalidInputData_Log,
                    $"User not found for OrderId {order.Id}",
                    "User not found");
            }

            // 🔹 Fetch user address (optional but safe)
            var userAddress = await _userAddressProcess.GetDefaultAddress(user.Id);

            // 🔹 Build response
            var response = new InvoiceExtendedSM
            {
                InvoiceDetails = _mapper.Map<InvoiceSM>(invoice),
                OrderDetails = _mapper.Map<OrderSM>(order),
                UserDetails = user,
                AddressDetails = userAddress
            };

            return response;
        }
    }
}