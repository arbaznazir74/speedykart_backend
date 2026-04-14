using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Siffrum.Ecom.BAL.ExceptionHandler;
using Siffrum.Ecom.BAL.Foundation.Base;
using Siffrum.Ecom.DAL.Context;
using Siffrum.Ecom.DomainModels.Enums;
using Siffrum.Ecom.DomainModels.v1;
using Siffrum.Ecom.ServiceModels.Foundation.Base.CommonResponseRoot;
using Siffrum.Ecom.ServiceModels.Foundation.Base.Enums;
using Siffrum.Ecom.ServiceModels.Foundation.Base.Interfaces;
using Siffrum.Ecom.ServiceModels.v1;
using System.Data;

namespace Siffrum.Ecom.BAL.LoginUsers
{
    public class DeliveryBoyOrderTransactionsProcess : SiffrumBalOdataBase<DeliveryBoyOrderTransactionsSM>
    {
        #region Properties
        

        private readonly ILoginUserDetail _loginUserDetail;
        private readonly IPasswordEncryptHelper _passwordEncryptHelper;

        #endregion Properties

        #region Constructor
        
        public DeliveryBoyOrderTransactionsProcess(IMapper mapper,ApiDbContext apiDbContext,
            ILoginUserDetail loginUserDetail)
            : base(mapper, apiDbContext)
        {
            _loginUserDetail = loginUserDetail;
        }

        #endregion Constructor

        #region OData
        public override async Task<IQueryable<DeliveryBoyOrderTransactionsSM>> GetServiceModelEntitiesForOdata()
        {
            IQueryable<DeliveryBoyOrderTransactionsDM> entitySet = _apiDbContext.DeliveryBoyOrderTransactions.AsNoTracking();
            return await base.MapEntityAsToQuerable<DeliveryBoyOrderTransactionsDM, DeliveryBoyOrderTransactionsSM>(_mapper, entitySet);
        }
        #endregion

        #region CREATE

        // 🔹 Delivery Boy pays amount to Admin
        public async Task<DeliveryBoyOrderTransactionsSM> AddPaymentAsync(DeliveryBoyOrderTransactionsSM request)
        {
            if (request.Amount <= 0)
                throw new SiffrumException(ApiErrorTypeSM.InvalidInputData_NoLog, "Amount must be greater than 0");
            var dBoy = await _apiDbContext.DeliveryBoy
                .FindAsync(request.DeliveryBoyId);

            if (dBoy == null)
                throw new SiffrumException(ApiErrorTypeSM.InvalidInputData_NoLog,
                    "Delivery boy not found");
            if (dBoy.Status != DeliveryBoyStatusDM.Active || dBoy.PaymentType != DeliveryBoyPaymentTypeDM.CommissionBased)
                throw new SiffrumException(ApiErrorTypeSM.InvalidInputData_NoLog,
                    "Delivery boy must be active and commission-based");
            var summary = await GetLedgerSummaryAsync(request.DeliveryBoyId);
            if (summary.OutstandingAmount <= 0)
            {
                throw new SiffrumException(ApiErrorTypeSM.InvalidInputData_NoLog,
                    "No outstanding balance to pay");
            }
            if (request.Amount > summary.OutstandingAmount)
            {
                throw new SiffrumException(ApiErrorTypeSM.InvalidInputData_NoLog,
                    "Amount exceeds outstanding balance");
            }
            var entity = new DeliveryBoyOrderTransactionsDM
            {
                Amount = request.Amount,
                DeliveryBoyId = request.DeliveryBoyId,
                OrderId = request.OrderId,
                PaymentType = DeliveryOrderPaymentTypeDM.OrderPaidAmount,
                TransactionDate = DateTime.UtcNow,
                TransactionId = request.TransactionId,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = _loginUserDetail.LoginId
            };

            await _apiDbContext.DeliveryBoyOrderTransactions.AddAsync(entity);
            await _apiDbContext.SaveChangesAsync();

            return _mapper.Map<DeliveryBoyOrderTransactionsSM>(entity);
        }

        #endregion

        #region READ

        public async Task<DeliveryBoyOrderTransactionsSM> GetByIdAsync(long id)
        {
            var entity = await _apiDbContext.DeliveryBoyOrderTransactions
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id);

            if (entity == null)
                throw new SiffrumException(ApiErrorTypeSM.InvalidInputData_NoLog, "Transaction not found");

            return _mapper.Map<DeliveryBoyOrderTransactionsSM>(entity);
        }
        public async Task<DeliveryBoyOrderTransactionsSM> GetMineByIdAsync(long id, long deliveryBoyId)
        {
            var entity = await _apiDbContext.DeliveryBoyOrderTransactions
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id && x.DeliveryBoyId == deliveryBoyId);

            if (entity == null)
                throw new SiffrumException(ApiErrorTypeSM.InvalidInputData_NoLog, "Transaction not found");

            return _mapper.Map<DeliveryBoyOrderTransactionsSM>(entity);
        }

        public async Task<List<DeliveryBoyOrderTransactionsSM>> GetAllAsync( int skip, int top)
        {
            var list = await _apiDbContext.DeliveryBoyOrderTransactions
                .OrderByDescending(x => x.TransactionDate)
                .Skip(skip).Take(top)
                .ToListAsync();

            return _mapper.Map<List<DeliveryBoyOrderTransactionsSM>>(list);
        }

        public async Task<IntResponseRoot> GetAllCountAsync()
        {
            var count = await _apiDbContext.DeliveryBoyOrderTransactions
                .CountAsync();

            return new IntResponseRoot(count, "Total Count of transactions");
        }

        public async Task<List<DeliveryBoyOrderTransactionsSM>> GetByDeliveryBoyIdAsync(long deliveryBoyId, int skip, int top)
        {
            var list = await _apiDbContext.DeliveryBoyOrderTransactions
                .Where(x => x.DeliveryBoyId == deliveryBoyId)
                .OrderByDescending(x => x.TransactionDate)
                .Skip(skip).Take(top)
                .ToListAsync();

            return _mapper.Map<List<DeliveryBoyOrderTransactionsSM>>(list);
        }

        public async Task<IntResponseRoot> GetByDeliveryBoyIdCountAsync(long deliveryBoyId)
        {
            var count = await _apiDbContext.DeliveryBoyOrderTransactions
                .Where(x => x.DeliveryBoyId == deliveryBoyId)
                .CountAsync();

            return new IntResponseRoot(count, "Total Count of transactions");
        }

        #endregion

        #region UPDATE

        public async Task<DeliveryBoyOrderTransactionsSM> UpdateAsync(DeliveryBoyOrderTransactionsSM request)
        {
            var entity = await _apiDbContext.DeliveryBoyOrderTransactions
                .FirstOrDefaultAsync(x => x.Id == request.Id);

            if (entity == null)
                throw new SiffrumException(ApiErrorTypeSM.InvalidInputData_NoLog, "Transaction not found");

            entity.Amount = request.Amount;
            entity.TransactionId = request.TransactionId;
            entity.TransactionDate = request.TransactionDate;
            entity.UpdatedAt = DateTime.UtcNow;
            entity.UpdatedBy = _loginUserDetail.LoginId;

            await _apiDbContext.SaveChangesAsync();

            return _mapper.Map<DeliveryBoyOrderTransactionsSM>(entity);
        }

        #endregion

        #region DELETE

        public async Task<BoolResponseRoot> DeleteAsync(long id)
        {
            var entity = await _apiDbContext.DeliveryBoyOrderTransactions
                .FirstOrDefaultAsync(x => x.Id == id);

            if (entity == null)
                throw new SiffrumException(ApiErrorTypeSM.InvalidInputData_NoLog, "Transaction not found");

            _apiDbContext.DeliveryBoyOrderTransactions.Remove(entity);
            await _apiDbContext.SaveChangesAsync();

            return new BoolResponseRoot(true, "Deleted successfully");
        }

        #endregion

        #region CALCULATIONS

        public async Task<DeliveryBoyLedgerSummarySM> GetLedgerSummaryAsync(long deliveryBoyId)
        {
            // 🔹 Validate Delivery Boy
            var exists = await _apiDbContext.DeliveryBoy
                .AnyAsync(x => x.Id == deliveryBoyId);

            if (!exists)
                throw new SiffrumException(ApiErrorTypeSM.InvalidInputData_NoLog,
                    "Delivery boy not found");

            // 🔹 Get totals
            var totalReceived = await _apiDbContext.DeliveryBoyOrderTransactions
                .Where(x => x.DeliveryBoyId == deliveryBoyId &&
                            x.PaymentType == DeliveryOrderPaymentTypeDM.OrderReceivedAmount)
                .SumAsync(x => (decimal?)x.Amount) ?? 0;

            var totalPaid = await _apiDbContext.DeliveryBoyOrderTransactions
                .Where(x => x.DeliveryBoyId == deliveryBoyId &&
                            x.PaymentType == DeliveryOrderPaymentTypeDM.OrderPaidAmount)
                .SumAsync(x => (decimal?)x.Amount) ?? 0;

            // 🔹 Calculate outstanding
            var outstanding = totalReceived - totalPaid;

            return new DeliveryBoyLedgerSummarySM
            {
                DeliveryBoyId = deliveryBoyId,
                TotalAmountReceived = totalReceived,
                TotalAmountPaid = totalPaid,
                OutstandingAmount = outstanding
            };
        }
        #endregion

    }

}
