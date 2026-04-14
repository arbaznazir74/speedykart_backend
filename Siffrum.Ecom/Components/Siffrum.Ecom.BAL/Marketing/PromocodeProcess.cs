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

namespace Siffrum.Ecom.BAL.Marketing
{
    public class PromocodeProcess : SiffrumBalOdataBase<PromoCodeSM>
    {
        private readonly ILoginUserDetail _loginUserDetail;

        public PromocodeProcess(
            ApiDbContext apiDbContext,
            IMapper mapper,
            ILoginUserDetail loginUserDetail)
            : base(mapper, apiDbContext)
        {
            _loginUserDetail = loginUserDetail;
        }

        #region ODATA
        public override async Task<IQueryable<PromoCodeSM>> GetServiceModelEntitiesForOdata()
        {
            var entitySet = _apiDbContext.PromoCodes.AsNoTracking();
            return await base.MapEntityAsToQuerable<PromoCodeDM, PromoCodeSM>(_mapper, entitySet);
        }
        #endregion

        #region CREATE
        public async Task<BoolResponseRoot> CreateAsync(PromoCodeSM objSM)
        {
            if (objSM == null)
                throw new SiffrumException(ApiErrorTypeSM.ModelError_NoLog, "Promo code data is required");

            bool exists = await _apiDbContext.PromoCodes
                .AnyAsync(x => x.Code.ToLower() == objSM.Code.ToLower());

            if (exists)
                throw new SiffrumException(ApiErrorTypeSM.InvalidInputData_NoLog, "Promo code already exists");

            var dm = _mapper.Map<PromoCodeDM>(objSM);

            dm.Code = dm.Code.Trim().ToUpper();
            dm.UsedCount = 0;
            dm.CreatedAt = DateTime.UtcNow;
            dm.CreatedBy = _loginUserDetail.LoginId;

            await _apiDbContext.PromoCodes.AddAsync(dm);

            if (await _apiDbContext.SaveChangesAsync() > 0)
                return new BoolResponseRoot(true, "Promo code created successfully");

            throw new SiffrumException(ApiErrorTypeSM.Fatal_Log, "Failed to create promo code");
        }
        #endregion

        #region READ

        public async Task<PromoCodeSM?> GetByIdAsync(long id)
        {
            var dm = await _apiDbContext.PromoCodes
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id);

            return dm == null ? null : _mapper.Map<PromoCodeSM>(dm);
        }

        public async Task<PromoCodeSM?> GetByCodeAsync(string code)
        {
            var dm = await _apiDbContext.PromoCodes
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Code == code.ToUpper());

            return dm == null ? null : _mapper.Map<PromoCodeSM>(dm);
        }

        public async Task<List<PromoCodeSM>> GetAll(int skip, int top)
        {
            var list = await _apiDbContext.PromoCodes
                .AsNoTracking()
                .OrderByDescending(x => x.CreatedAt)
                .Skip(skip)
                .Take(top)
                .ToListAsync();

            return _mapper.Map<List<PromoCodeSM>>(list);
        }

        public async Task<IntResponseRoot> GetCount()
        {
            var count = await _apiDbContext.PromoCodes
                .CountAsync();
            return new IntResponseRoot(count, "Total Promo Codes");
        }

        #endregion

        #region UPDATE

        public async Task<PromoCodeSM?> UpdateAsync(long id, PromoCodeSM objSM)
        {
            var dm = await _apiDbContext.PromoCodes
                .FirstOrDefaultAsync(x => x.Id == id);

            if (dm == null)
                throw new SiffrumException(ApiErrorTypeSM.InvalidInputData_NoLog, "Promo code not found");

            _mapper.Map(objSM, dm);

            dm.Id = id;
            dm.UpdatedAt = DateTime.UtcNow;
            dm.UpdatedBy = _loginUserDetail.LoginId;

            if (await _apiDbContext.SaveChangesAsync() > 0)
                return await GetByIdAsync(id);

            throw new SiffrumException(ApiErrorTypeSM.Fatal_Log, "Failed to update promo code");
        }

        public async Task<BoolResponseRoot> UpdateStatusAsync(long id, bool isActive)
        {
            var dm = await _apiDbContext.PromoCodes
                .FirstOrDefaultAsync(x => x.Id == id);

            if (dm == null)
                throw new SiffrumException(ApiErrorTypeSM.InvalidInputData_NoLog, "Promo code not found");
            if(dm.IsActive == isActive)
            {
                return new BoolResponseRoot(true, "Promo code status already updated");
            }
            dm.IsActive = isActive;

            if (await _apiDbContext.SaveChangesAsync() > 0)
                return new BoolResponseRoot(true, "Promo code status updated");

            throw new SiffrumException(ApiErrorTypeSM.Fatal_Log, "Failed to update status");
        }

        #endregion

        #region DELETE

        public async Task<DeleteResponseRoot> DeleteAsync(long id)
        {
            var dm = await _apiDbContext.PromoCodes
                .FirstOrDefaultAsync(x => x.Id == id);

            if (dm == null)
                throw new SiffrumException(ApiErrorTypeSM.InvalidInputData_Log, "Promo code not found");

            _apiDbContext.PromoCodes.Remove(dm);

            if (await _apiDbContext.SaveChangesAsync() > 0)
                return new DeleteResponseRoot(true, "Promo code deleted successfully");

            throw new SiffrumException(ApiErrorTypeSM.Fatal_Log, "Failed to delete promo code");
        }

        #endregion

        /*#region APPLY PROMO CODE

        public async Task<decimal> ApplyPromoCodeAsync(
            string code,
            decimal cartSubtotal,
            long userId,
            PlatformTypeDM platform)
        {
            var promo = await _apiDbContext.PromoCodes
                .FirstOrDefaultAsync(x => x.Code == code.ToUpper());

            if (promo == null || !promo.IsActive)
                throw new SiffrumException(ApiErrorTypeSM.InvalidInputData_NoLog, "Invalid promo code");

            var now = DateTime.UtcNow;

            if (now < promo.StartDate || now > promo.EndDate)
                throw new SiffrumException(ApiErrorTypeSM.InvalidInputData_NoLog, "Promo code expired");

            if (promo.PlatformType.HasValue && promo.PlatformType != platform)
                throw new SiffrumException(ApiErrorTypeSM.InvalidInputData_NoLog, "Promo not valid for this platform");

            if (promo.MinimumCartAmount.HasValue && cartSubtotal < promo.MinimumCartAmount.Value)
                throw new SiffrumException(ApiErrorTypeSM.InvalidInputData_NoLog, "Minimum cart amount not reached");

            if (promo.UsageLimit.HasValue && promo.UsedCount >= promo.UsageLimit.Value)
                throw new SiffrumException(ApiErrorTypeSM.InvalidInputData_NoLog, "Promo usage limit reached");

            if (promo.UsagePerUserLimit.HasValue)
            {
                var userUsage = await _apiDbContext.CouponUsages
                    .CountAsync(x => x.UserId == userId && x.CouponId == promo.Id);

                if (userUsage >= promo.UsagePerUserLimit.Value)
                    throw new SiffrumException(ApiErrorTypeSM.InvalidInputData_NoLog, "User usage limit reached");
            }

            decimal discount;

            if (promo.Type == CouponTypeDM.Percentage)
            {
                discount = cartSubtotal * (promo.DiscountValue / 100m);

                if (promo.MaxDiscountAmount.HasValue)
                    discount = Math.Min(discount, promo.MaxDiscountAmount.Value);
            }
            else
            {
                discount = promo.DiscountValue;
            }

            return Math.Round(discount, 2);
        }

        #endregion*/
    }
}
