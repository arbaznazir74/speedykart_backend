using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Siffrum.Ecom.BAL.Base.Email;
using Siffrum.Ecom.BAL.Base.ImageProcess;
using Siffrum.Ecom.BAL.ExceptionHandler;
using Siffrum.Ecom.BAL.Foundation.Base;
using Siffrum.Ecom.DAL.Context;
using Siffrum.Ecom.DomainModels.v1;
using Siffrum.Ecom.ServiceModels.Foundation.Base.CommonResponseRoot;
using Siffrum.Ecom.ServiceModels.Foundation.Base.Enums;
using Siffrum.Ecom.ServiceModels.Foundation.Base.Interfaces;
using Siffrum.Ecom.ServiceModels.v1;
using System.Data;

namespace Siffrum.Ecom.BAL.LoginUsers
{
    public class DeliveryBoyTransactionProcess : SiffrumBalOdataBase<DeliveryBoyTransactionsSM>
    {
        #region Properties
        

        private readonly ILoginUserDetail _loginUserDetail;
        private readonly IPasswordEncryptHelper _passwordEncryptHelper;
        private readonly EmailProcess _emailProcess;
        private readonly ImageProcess _imageProcess;

        #endregion Properties

        #region Constructor
        
        public DeliveryBoyTransactionProcess(IMapper mapper,ApiDbContext apiDbContext,ImageProcess imageProcess,
            ILoginUserDetail loginUserDetail, IPasswordEncryptHelper passwordEncryptHelper, EmailProcess emailProcess)
            : base(mapper, apiDbContext)
        {
            _loginUserDetail = loginUserDetail;
            _passwordEncryptHelper = passwordEncryptHelper;
            _emailProcess = emailProcess;
            _imageProcess = imageProcess;
        }

        #endregion Constructor

        #region OData
        public override async Task<IQueryable<DeliveryBoyTransactionsSM>> GetServiceModelEntitiesForOdata()
        {
            IQueryable<DeliveryBoyTransactionsDM> entitySet = _apiDbContext.DeliveryBoyTransactions.AsNoTracking();
            return await base.MapEntityAsToQuerable<DeliveryBoyTransactionsDM, DeliveryBoyTransactionsSM>(_mapper, entitySet);
        }
        #endregion

        #region CREATE

        public async Task<BoolResponseRoot> CreateAsync(DeliveryBoyTransactionsSM objSM)
        {
            if (objSM == null)
                throw new SiffrumException(ApiErrorTypeSM.ModelError_NoLog, "Transaction data is required");

            if (objSM.Amount <= 0)
                throw new SiffrumException(ApiErrorTypeSM.InvalidInputData_NoLog, "Amount must be greater than zero");

            var deliveryBoy = await _apiDbContext.DeliveryBoy
                .FindAsync(objSM.DeliveryBoyId);

            if (deliveryBoy == null)
                throw new SiffrumException(ApiErrorTypeSM.InvalidInputData_NoLog, "Delivery boy not found");

            var dm = _mapper.Map<DeliveryBoyTransactionsDM>(objSM);

            dm.CreatedAt = DateTime.UtcNow;
            dm.CreatedBy = _loginUserDetail.LoginId;

            await _apiDbContext.DeliveryBoyTransactions.AddAsync(dm);

            if (await _apiDbContext.SaveChangesAsync() > 0)
                return new BoolResponseRoot(true, "Transaction added successfully");

            throw new SiffrumException(ApiErrorTypeSM.Fatal_Log, "Failed to create transaction");
        }

        #endregion

        #region READ

        public async Task<List<DeliveryBoyTransactionsSM>> GetAll( int skip, int top)
        {
            var list = await _apiDbContext.DeliveryBoyTransactions
                .AsNoTracking()
                .OrderByDescending(x => x.TransactionDate)
                .Skip(skip)
                .Take(top)
                .ToListAsync();

            return _mapper.Map<List<DeliveryBoyTransactionsSM>>(list);
        }

        public async Task<List<DeliveryBoyTransactionsSM>> GetAllByDeliveryBoyId(long deliveryBoyId,int skip, int top)
        {
            var list = await _apiDbContext.DeliveryBoyTransactions
                .AsNoTracking()
                .Where(x => x.DeliveryBoyId == deliveryBoyId)
                .OrderByDescending(x => x.TransactionDate)
                .Skip(skip)
                .Take(top)
                .ToListAsync();

            return _mapper.Map<List<DeliveryBoyTransactionsSM>>(list);
        }

        public async Task<DeliveryBoyTransactionsSM?> GetById(long id)
        {
            var dm = await _apiDbContext.DeliveryBoyTransactions
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id);

            return dm == null ? null : _mapper.Map<DeliveryBoyTransactionsSM>(dm);
        }

        public async Task<IntResponseRoot> GetCount()
        {
            var count = await _apiDbContext.DeliveryBoyTransactions
                .AsNoTracking()
                .CountAsync();

            return new IntResponseRoot(count, "Total transactions");
        }
        public async Task<IntResponseRoot> GetCountByDeliveryBoyId(long deliveryBoyId)
        {
            var count = await _apiDbContext.DeliveryBoyTransactions
                .AsNoTracking()
                .Where(x => x.DeliveryBoyId == deliveryBoyId)
                .CountAsync();

            return new IntResponseRoot(count, "Total transactions");
        }

        #endregion

        #region UPDATE

        public async Task<DeliveryBoyTransactionsSM> UpdateAsync(long id, DeliveryBoyTransactionsSM objSM)
        {
            if (objSM == null)
                throw new SiffrumException(ApiErrorTypeSM.ModelError_NoLog, "Transaction data is required");

            var dm = await _apiDbContext.DeliveryBoyTransactions
                .FirstOrDefaultAsync(x => x.Id == id);

            if (dm == null)
                throw new SiffrumException(ApiErrorTypeSM.InvalidInputData_NoLog, "Transaction not found");

            if (objSM.Amount <= 0)
                throw new SiffrumException(ApiErrorTypeSM.InvalidInputData_NoLog, "Amount must be greater than zero");

            // Update fields
            dm.Id = id;
            dm.DeliveryBoyId = dm.DeliveryBoyId;
            dm.Amount = objSM.Amount;
            dm.TransactionDate = objSM.TransactionDate;
            dm.TransactionId = objSM.TransactionId;

            dm.UpdatedAt = DateTime.UtcNow;
            dm.UpdatedBy = _loginUserDetail.LoginId;

            await _apiDbContext.SaveChangesAsync();

            return _mapper.Map<DeliveryBoyTransactionsSM>(dm);
        }

        #endregion

        #region DELETE

        public async Task<DeleteResponseRoot> DeleteAsync(long id)
        {
            var dm = await _apiDbContext.DeliveryBoyTransactions
                .FirstOrDefaultAsync(x => x.Id == id);

            if (dm == null)
                throw new SiffrumException(ApiErrorTypeSM.InvalidInputData_Log, "Transaction not found");

            _apiDbContext.DeliveryBoyTransactions.Remove(dm);

            await _apiDbContext.SaveChangesAsync();

            return new DeleteResponseRoot(true, "Transaction deleted successfully");
        }

        #endregion

        #region TOTAL AMOUNT

        public async Task<DeliveryBoyPaymentsSM> GetTotalPaidAmount(long deliveryBoyId)
        {
            var total = await _apiDbContext.DeliveryBoyTransactions
                .AsNoTracking()
                .Where(x => x.DeliveryBoyId == deliveryBoyId)
                .SumAsync(x => (decimal?)x.Amount) ?? 0;

            return new DeliveryBoyPaymentsSM()
            {
                TotalPayments = total
            };
        }

        #endregion
    }

}
