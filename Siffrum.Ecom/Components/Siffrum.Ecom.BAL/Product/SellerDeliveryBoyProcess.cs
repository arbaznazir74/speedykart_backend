using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Siffrum.Ecom.BAL.ExceptionHandler;
using Siffrum.Ecom.BAL.Foundation.Base;
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
    public class SellerDeliveryBoyProcess : SiffrumBalBase
    {
        private readonly IPasswordEncryptHelper _passwordEncryptHelper;
        private readonly ILoginUserDetail _loginUserDetail;

        public SellerDeliveryBoyProcess(IMapper mapper, ApiDbContext apiDbContext,
            IPasswordEncryptHelper passwordEncryptHelper, ILoginUserDetail loginUserDetail)
            : base(mapper, apiDbContext)
        {
            _passwordEncryptHelper = passwordEncryptHelper;
            _loginUserDetail = loginUserDetail;
        }

        #region Seller - Register Delivery Boy

        public async Task<BoolResponseRoot> RegisterDeliveryBoy(long sellerId, DeliveryBoySM sm)
        {
            if (sm == null)
                throw new SiffrumException(ApiErrorTypeSM.InvalidInputData_NoLog, "Please provide delivery boy details");

            if (string.IsNullOrEmpty(sm.Username) || sm.Username.Length < 5)
                throw new SiffrumException(ApiErrorTypeSM.InvalidInputData_NoLog, "Username must be at least 5 characters");

            if (string.IsNullOrEmpty(sm.Email))
                throw new SiffrumException(ApiErrorTypeSM.InvalidInputData_NoLog, "Email is required");

            if (string.IsNullOrEmpty(sm.Password))
                throw new SiffrumException(ApiErrorTypeSM.InvalidInputData_NoLog, "Password is required");

            var emailExists = await _apiDbContext.DeliveryBoy.AnyAsync(d => d.Email == sm.Email);
            if (emailExists)
                throw new SiffrumException(ApiErrorTypeSM.InvalidInputData_NoLog, "Email already registered");

            var usernameExists = await _apiDbContext.DeliveryBoy.AnyAsync(d => d.Username == sm.Username);
            if (usernameExists)
                throw new SiffrumException(ApiErrorTypeSM.InvalidInputData_NoLog, "Username already taken");

            var dm = _mapper.Map<DeliveryBoyDM>(sm);
            dm.Password = await _passwordEncryptHelper.ProtectAsync<string>(sm.Password);
            dm.RoleType = RoleTypeDM.DeliveryBoy;
            dm.IsEmailConfirmed = true;
            dm.IsMobileConfirmed = true;
            dm.CreatedAt = DateTime.UtcNow;
            dm.Status = DeliveryBoyStatusDM.Active;
            dm.LoginStatus = LoginStatusDM.Enabled;
            dm.SellerId = sellerId;
            dm.CreatedBy = _loginUserDetail.LoginId;

            await _apiDbContext.DeliveryBoy.AddAsync(dm);
            if (await _apiDbContext.SaveChangesAsync() > 0)
                return new BoolResponseRoot(true, "Delivery boy registered successfully");

            throw new SiffrumException(ApiErrorTypeSM.NoRecord_NoLog, "Failed to register delivery boy");
        }

        #endregion

        #region Seller - List My Delivery Boys

        public async Task<List<DeliveryBoySM>> GetMyDeliveryBoys(long sellerId, int skip, int top)
        {
            var list = await _apiDbContext.DeliveryBoy
                .AsNoTracking()
                .Where(d => d.SellerId == sellerId)
                .OrderByDescending(d => d.CreatedAt)
                .Skip(skip).Take(top)
                .ToListAsync();

            return _mapper.Map<List<DeliveryBoySM>>(list);
        }

        public async Task<IntResponseRoot> GetMyDeliveryBoysCount(long sellerId)
        {
            var count = await _apiDbContext.DeliveryBoy
                .AsNoTracking()
                .CountAsync(d => d.SellerId == sellerId);
            return new IntResponseRoot(count, "Total Delivery Boys");
        }

        public async Task<DeliveryBoySM> GetMyDeliveryBoyById(long sellerId, long dBoyId)
        {
            var dm = await _apiDbContext.DeliveryBoy
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.Id == dBoyId && d.SellerId == sellerId);

            if (dm == null)
                throw new SiffrumException(ApiErrorTypeSM.InvalidInputData_NoLog, "Delivery boy not found");

            return _mapper.Map<DeliveryBoySM>(dm);
        }

        #endregion

        #region Seller - Update Delivery Boy

        public async Task<DeliveryBoySM> UpdateDeliveryBoy(long sellerId, long dBoyId, DeliveryBoySM sm)
        {
            var dm = await _apiDbContext.DeliveryBoy
                .FirstOrDefaultAsync(d => d.Id == dBoyId && d.SellerId == sellerId);

            if (dm == null)
                throw new SiffrumException(ApiErrorTypeSM.InvalidInputData_NoLog, "Delivery boy not found");

            dm.Name = sm.Name ?? dm.Name;
            dm.Mobile = sm.Mobile ?? dm.Mobile;
            dm.Address = sm.Address ?? dm.Address;
            dm.PaymentType = (DeliveryBoyPaymentTypeDM)sm.PaymentType;
            dm.UpdatedAt = DateTime.UtcNow;
            dm.UpdatedBy = _loginUserDetail.LoginId;

            await _apiDbContext.SaveChangesAsync();
            return _mapper.Map<DeliveryBoySM>(dm);
        }

        public async Task<BoolResponseRoot> ToggleDeliveryBoyStatus(long sellerId, long dBoyId, bool activate)
        {
            var dm = await _apiDbContext.DeliveryBoy
                .FirstOrDefaultAsync(d => d.Id == dBoyId && d.SellerId == sellerId);

            if (dm == null)
                throw new SiffrumException(ApiErrorTypeSM.InvalidInputData_NoLog, "Delivery boy not found");

            dm.Status = activate ? DeliveryBoyStatusDM.Active : DeliveryBoyStatusDM.Deactivated;
            dm.LoginStatus = activate ? LoginStatusDM.Enabled : LoginStatusDM.Disabled;
            dm.UpdatedAt = DateTime.UtcNow;

            await _apiDbContext.SaveChangesAsync();
            return new BoolResponseRoot(true, activate ? "Activated" : "Deactivated");
        }

        #endregion

        #region Seller - COD Summary Per Delivery Boy

        public async Task<List<DeliveryBoyCodSummarySM>> GetCodSummary(long sellerId, DateTime? date = null)
        {
            var myBoys = await _apiDbContext.DeliveryBoy
                .AsNoTracking()
                .Where(d => d.SellerId == sellerId && d.Status == DeliveryBoyStatusDM.Active)
                .ToListAsync();

            var result = new List<DeliveryBoyCodSummarySM>();
            foreach (var boy in myBoys)
            {
                var codTotal = await GetTotalCodForDeliveryBoy(sellerId, boy.Id, date);
                var settledQuery = _apiDbContext.CashCollection
                    .AsNoTracking()
                    .Where(c => c.SellerId == sellerId && c.DeliveryBoyId == boy.Id
                        && c.Status == CashCollectionStatusDM.Collected);
                if (date.HasValue)
                {
                    var dayStart = date.Value.Date.AddHours(-5).AddMinutes(-30);
                    var dayEnd = dayStart.AddDays(1);
                    settledQuery = settledQuery.Where(c =>
                        (c.CollectedAt != null && c.CollectedAt >= dayStart && c.CollectedAt < dayEnd)
                        || (c.CollectedAt == null && c.CreatedAt >= dayStart && c.CreatedAt < dayEnd));
                }
                var settled = await settledQuery.SumAsync(c => (decimal?)c.Amount) ?? 0;

                result.Add(new DeliveryBoyCodSummarySM
                {
                    DeliveryBoyId = boy.Id,
                    DeliveryBoyName = boy.Name,
                    DeliveryBoyMobile = boy.Mobile,
                    TotalCodCollected = codTotal,
                    TotalCashSettled = settled,
                    PendingAmount = codTotal - settled,
                    TotalCodOrders = await GetCodOrderCount(sellerId, boy.Id, date)
                });
            }
            return result;
        }

        public async Task<List<CodOrderDetailSM>> GetCodOrdersForDeliveryBoy(long sellerId, long dBoyId, int skip, int top)
        {
            var boy = await _apiDbContext.DeliveryBoy
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.Id == dBoyId && d.SellerId == sellerId);

            if (boy == null)
                throw new SiffrumException(ApiErrorTypeSM.InvalidInputData_NoLog, "Delivery boy not found");

            var deliveries = await _apiDbContext.Deliveries
                .AsNoTracking()
                .Where(d => d.DeliveryBoyId == dBoyId && d.Status == DeliveryStatusDM.Delivered)
                .OrderByDescending(d => d.DeliveredAt)
                .Skip(skip).Take(top)
                .ToListAsync();

            var result = new List<CodOrderDetailSM>();
            foreach (var del in deliveries)
            {
                var order = await _apiDbContext.Order
                    .AsNoTracking()
                    .FirstOrDefaultAsync(o => o.Id == del.OrderId && o.SellerId == sellerId
                        && o.PaymentMode == PaymentModeDM.CashOnDelivery);

                if (order == null) continue;

                var user = await _apiDbContext.User
                    .AsNoTracking()
                    .FirstOrDefaultAsync(u => u.Id == order.UserId);

                result.Add(new CodOrderDetailSM
                {
                    OrderId = order.Id,
                    OrderNumber = order.OrderNumber,
                    Amount = order.Amount,
                    OrderStatus = ((OrderStatusSM)order.OrderStatus).ToString(),
                    DeliveryStatus = ((DeliveryStatusSM)del.Status).ToString(),
                    DeliveredAt = del.DeliveredAt,
                    OrderDate = order.CreatedAt,
                    CustomerName = user?.Name ?? user?.Mobile
                });
            }
            return result;
        }

        #endregion

        #region Seller - Mark Cash Collected

        public async Task<CashCollectionSM> MarkCashCollected(long sellerId, MarkCashCollectedSM sm)
        {
            var boy = await _apiDbContext.DeliveryBoy
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.Id == sm.DeliveryBoyId && d.SellerId == sellerId);

            if (boy == null)
                throw new SiffrumException(ApiErrorTypeSM.InvalidInputData_NoLog, "Delivery boy not found");

            if (sm.Amount <= 0)
                throw new SiffrumException(ApiErrorTypeSM.InvalidInputData_NoLog, "Amount must be greater than 0");

            var dm = new CashCollectionDM
            {
                SellerId = sellerId,
                DeliveryBoyId = sm.DeliveryBoyId,
                Amount = sm.Amount,
                Status = CashCollectionStatusDM.Collected,
                CollectedAt = DateTime.UtcNow,
                Remarks = sm.Remarks,
                DateFrom = DateTime.UtcNow.Date,
                DateTo = DateTime.UtcNow.Date,
                CreatedAt = DateTime.UtcNow
            };

            await _apiDbContext.CashCollection.AddAsync(dm);
            await _apiDbContext.SaveChangesAsync();
            return _mapper.Map<CashCollectionSM>(dm);
        }

        public async Task<List<CashCollectionSM>> GetCashCollections(long sellerId, long dBoyId, int skip, int top)
        {
            var list = await _apiDbContext.CashCollection
                .AsNoTracking()
                .Where(c => c.SellerId == sellerId && c.DeliveryBoyId == dBoyId)
                .OrderByDescending(c => c.CreatedAt)
                .Skip(skip).Take(top)
                .ToListAsync();

            return _mapper.Map<List<CashCollectionSM>>(list);
        }

        #endregion

        #region Delivery Boy - My Cash Ledger

        public async Task<DeliveryBoyCashLedgerSM> GetMyCashLedger(long dBoyId)
        {
            var boy = await _apiDbContext.DeliveryBoy
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.Id == dBoyId);

            if (boy == null || boy.SellerId == null)
                throw new SiffrumException(ApiErrorTypeSM.InvalidInputData_NoLog, "Delivery boy not found");

            var sellerId = boy.SellerId.Value;

            var codTotal = await GetTotalCodForDeliveryBoy(sellerId, dBoyId);
            var settled = await _apiDbContext.CashCollection
                .AsNoTracking()
                .Where(c => c.DeliveryBoyId == dBoyId && c.Status == CashCollectionStatusDM.Collected)
                .SumAsync(c => (decimal?)c.Amount) ?? 0;

            var settlements = await _apiDbContext.CashCollection
                .AsNoTracking()
                .Where(c => c.DeliveryBoyId == dBoyId)
                .OrderByDescending(c => c.CreatedAt)
                .Take(50)
                .ToListAsync();

            return new DeliveryBoyCashLedgerSM
            {
                TotalCodCollected = codTotal,
                TotalCashSettled = settled,
                PendingAmount = codTotal - settled,
                Settlements = _mapper.Map<List<CashCollectionSM>>(settlements)
            };
        }

        #endregion

        #region Seller - Delivery Boy Stats

        public async Task<DeliveryBoyStatsSM> GetMyDeliveryBoyStats(long sellerId, long dBoyId)
        {
            var dm = await _apiDbContext.DeliveryBoy
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.Id == dBoyId && d.SellerId == sellerId);

            if (dm == null)
                throw new SiffrumException(ApiErrorTypeSM.InvalidInputData_NoLog, "Delivery boy not found");

            var deliveredOrderIds = await _apiDbContext.Deliveries
                .AsNoTracking()
                .Where(d => d.DeliveryBoyId == dBoyId && d.Status == DeliveryStatusDM.Delivered)
                .Select(d => d.OrderId)
                .ToListAsync();

            var sellerDeliveredCount = deliveredOrderIds.Any()
                ? await _apiDbContext.Order.AsNoTracking()
                    .CountAsync(o => deliveredOrderIds.Contains(o.Id) && o.SellerId == sellerId)
                : 0;

            var totalAssigned = await _apiDbContext.Deliveries
                .AsNoTracking()
                .CountAsync(d => d.DeliveryBoyId == dBoyId);

            var sellerTotalAmount = deliveredOrderIds.Any()
                ? await _apiDbContext.Order.AsNoTracking()
                    .Where(o => deliveredOrderIds.Contains(o.Id) && o.SellerId == sellerId)
                    .SumAsync(o => (decimal?)o.Amount) ?? 0
                : 0;

            var sellerName = await _apiDbContext.Seller.AsNoTracking()
                .Where(s => s.Id == sellerId)
                .Select(s => s.Name)
                .FirstOrDefaultAsync();

            return new DeliveryBoyStatsSM
            {
                DeliveryBoyId = dBoyId,
                DeliveryBoyName = dm.Name,
                TotalDeliveries = totalAssigned,
                DeliveredCount = sellerDeliveredCount,
                AssignedCount = await _apiDbContext.Deliveries.AsNoTracking()
                    .CountAsync(d => d.DeliveryBoyId == dBoyId && d.Status == DeliveryStatusDM.Assigned),
                PickedUpCount = await _apiDbContext.Deliveries.AsNoTracking()
                    .CountAsync(d => d.DeliveryBoyId == dBoyId && d.Status == DeliveryStatusDM.PickedUp),
                TotalDeliveredAmount = sellerTotalAmount,
                IsOnline = dm.IsAvailable == 1,
                PaymentType = ((DeliveryBoyPaymentTypeSM)dm.PaymentType).ToString(),
                SellerName = sellerName
            };
        }

        #endregion

        #region Private Helpers

        private async Task<decimal> GetTotalCodForDeliveryBoy(long sellerId, long dBoyId, DateTime? date = null)
        {
            var delQuery = _apiDbContext.Deliveries
                .AsNoTracking()
                .Where(d => d.DeliveryBoyId == dBoyId && d.Status == DeliveryStatusDM.Delivered);
            if (date.HasValue)
            {
                var dayStart = date.Value.Date.AddHours(-5).AddMinutes(-30);
                var dayEnd = dayStart.AddDays(1);
                delQuery = delQuery.Where(d =>
                    (d.DeliveredAt != null && d.DeliveredAt >= dayStart && d.DeliveredAt < dayEnd)
                    || (d.DeliveredAt == null && d.CreatedAt >= dayStart && d.CreatedAt < dayEnd));
            }
            var deliveredOrderIds = await delQuery.Select(d => d.OrderId).ToListAsync();

            if (!deliveredOrderIds.Any()) return 0;

            return await _apiDbContext.Order
                .AsNoTracking()
                .Where(o => deliveredOrderIds.Contains(o.Id)
                    && o.SellerId == sellerId
                    && o.PaymentMode == PaymentModeDM.CashOnDelivery)
                .SumAsync(o => (decimal?)o.Amount) ?? 0;
        }

        private async Task<int> GetCodOrderCount(long sellerId, long dBoyId, DateTime? date = null)
        {
            var delQuery = _apiDbContext.Deliveries
                .AsNoTracking()
                .Where(d => d.DeliveryBoyId == dBoyId && d.Status == DeliveryStatusDM.Delivered);
            if (date.HasValue)
            {
                var dayStart = date.Value.Date.AddHours(-5).AddMinutes(-30);
                var dayEnd = dayStart.AddDays(1);
                delQuery = delQuery.Where(d =>
                    (d.DeliveredAt != null && d.DeliveredAt >= dayStart && d.DeliveredAt < dayEnd)
                    || (d.DeliveredAt == null && d.CreatedAt >= dayStart && d.CreatedAt < dayEnd));
            }
            var deliveredOrderIds = await delQuery.Select(d => d.OrderId).ToListAsync();

            if (!deliveredOrderIds.Any()) return 0;

            return await _apiDbContext.Order
                .AsNoTracking()
                .CountAsync(o => deliveredOrderIds.Contains(o.Id)
                    && o.SellerId == sellerId
                    && o.PaymentMode == PaymentModeDM.CashOnDelivery);
        }

        #endregion
    }
}
