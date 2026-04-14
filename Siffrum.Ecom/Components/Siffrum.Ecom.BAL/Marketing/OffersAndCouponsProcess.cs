using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Siffrum.Ecom.BAL.Base.ImageProcess;
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
    public class OffersAndCouponsProcess
        : SiffrumBalOdataBase<OffersAndCouponsSM>
    {
        private readonly ImageProcess _imageProcess;
        private readonly ILoginUserDetail _loginUserDetail;

        public OffersAndCouponsProcess(
            ApiDbContext apiDbContext,
            IMapper mapper,
            ImageProcess imageProcess,
            ILoginUserDetail loginUserDetail)
            : base(mapper, apiDbContext)
        {
            _imageProcess = imageProcess;
            _loginUserDetail = loginUserDetail;
        }

        #region ODATA
        public override async Task<IQueryable<OffersAndCouponsSM>> GetServiceModelEntitiesForOdata()
        {
            var entitySet = _apiDbContext.OffersAndCoupons
                .AsNoTracking();

            return await base.MapEntityAsToQuerable<
                OffersAndCouponsDM,
                OffersAndCouponsSM>(_mapper, entitySet);
        }
        #endregion

        #region CREATE
        public async Task<BoolResponseRoot> CreateAsync(OffersAndCouponsSM objSM)
        {
            if (objSM == null)
                throw new SiffrumException(
                    ApiErrorTypeSM.ModelError_NoLog,
                    "Offer data is required"
                );

            if (string.IsNullOrWhiteSpace(objSM.Name))
                throw new SiffrumException(
                    ApiErrorTypeSM.InvalidInputData_NoLog,
                    "Offer name is required"
                );

            if (string.IsNullOrEmpty(objSM.PathBase64))
                throw new SiffrumException(
                    ApiErrorTypeSM.InvalidInputData_NoLog,
                    "Offer image is required"
                );

            if (!objSM.Percentage.HasValue && !objSM.OfferValue.HasValue)
                throw new SiffrumException(
                    ApiErrorTypeSM.InvalidInputData_NoLog,
                    "Either Percentage or OfferValue must be provided"
                );
            if (objSM.Percentage.HasValue && objSM.OfferValue.HasValue)
                throw new SiffrumException(
                    ApiErrorTypeSM.InvalidInputData_NoLog,
                    "Please provide either a discount percentage or a fixed offer amount, not both."
                );

            bool exists = await _apiDbContext.OffersAndCoupons
                .AnyAsync(x => x.Name.ToLower() == objSM.Name.ToLower());

            if (exists)
                throw new SiffrumException(
                    ApiErrorTypeSM.InvalidInputData_NoLog,
                    "Offer already exists"
                );

            var dm = _mapper.Map<OffersAndCouponsDM>(objSM);

            dm.Base64Path = await _imageProcess.SaveFromBase64(
                objSM.PathBase64,
                objSM.ExtensionType.ToString().ToLower(),
                "wwwroot/content/offers"
            );

            dm.ExtensionType = (ExtensionTypeDM)objSM.ExtensionType;
            dm.CreatedAt = DateTime.UtcNow;
            dm.CreatedBy = _loginUserDetail.LoginId;

            await _apiDbContext.OffersAndCoupons.AddAsync(dm);

            if (await _apiDbContext.SaveChangesAsync() > 0)
                return new BoolResponseRoot(true, "Offer created successfully");

            throw new SiffrumException(
                ApiErrorTypeSM.Fatal_Log,
                "Failed to create offer"
            );
        }
        #endregion

        #region READ
        public async Task<OffersAndCouponsSM?> GetByIdAsync(long id)
        {
            var dm = await _apiDbContext.OffersAndCoupons.FindAsync(id);
            if (dm == null) return null;

            var sm = _mapper.Map<OffersAndCouponsSM>(dm);

            if (!string.IsNullOrEmpty(dm.Base64Path))
            {
                var oImg = await _imageProcess.ResolveImage(dm.Base64Path);
                sm.PathBase64 = oImg.Base64;
                sm.NetworkPath = oImg.NetworkUrl;
            }

            return sm;
        }

        public async Task<List<OffersAndCouponsSM>> GetAll(int skip, int top)
        {
            var ids = await _apiDbContext.OffersAndCoupons
                .AsNoTracking()
                .Skip(skip)
                .Take(top)
                .Select(x => x.Id)
                .ToListAsync();

            var response = new List<OffersAndCouponsSM>();
            foreach (var id in ids)
            {
                var offer = await GetByIdAsync(id);
                if (offer != null)
                    response.Add(offer);
            }
            return response;
        }

        public async Task<IntResponseRoot> GetCount()
        {
            var count = await _apiDbContext.OffersAndCoupons
                .CountAsync();
            return new IntResponseRoot(count, "Total Offers");
        }       

        #endregion

        #region UPDATE
        public async Task<OffersAndCouponsSM?> UpdateAsync(long id, OffersAndCouponsSM objSM)
        {
            var dm = await _apiDbContext.OffersAndCoupons
                .FirstOrDefaultAsync(x => x.Id == id);

            if (dm == null)
                throw new SiffrumException(
                    ApiErrorTypeSM.InvalidInputData_NoLog,
                    "Offer not found"
                );

            string oldImage = null;

            _mapper.Map(objSM, dm);

            if (!string.IsNullOrEmpty(objSM.PathBase64))
            {
                var newPath = await _imageProcess.SaveFromBase64(
                    objSM.PathBase64,
                    objSM.ExtensionType.ToString().ToLower(),
                    "wwwroot/content/offers"
                );
                if (!string.IsNullOrEmpty(dm.Base64Path))
                {
                    oldImage = dm.Base64Path;
                    dm.Base64Path = newPath;
                }
                
            }
            dm.Id = id;
            dm.ExtensionType = (ExtensionTypeDM)objSM.ExtensionType;
            dm.UpdatedAt = DateTime.UtcNow;
            dm.UpdatedBy = _loginUserDetail.LoginId;

            if (await _apiDbContext.SaveChangesAsync() > 0)
            {
                if (!string.IsNullOrEmpty(oldImage) && File.Exists(oldImage))
                    File.Delete(oldImage);

                return await GetByIdAsync(id);
            }

            throw new SiffrumException(
                ApiErrorTypeSM.Fatal_Log,
                "Failed to update offer"
            );
        }
        #endregion

        #region DELETE
        public async Task<DeleteResponseRoot> DeleteAsync(long id)
        {
            var dm = await _apiDbContext.OffersAndCoupons
                .FirstOrDefaultAsync(x => x.Id == id);

            if (dm == null)
                throw new SiffrumException(
                    ApiErrorTypeSM.InvalidInputData_Log,
                    "Offer not found"
                );

            string oldImage = dm.Base64Path;

            _apiDbContext.OffersAndCoupons.Remove(dm);

            if (await _apiDbContext.SaveChangesAsync() > 0)
            {
                if (!string.IsNullOrEmpty(oldImage) && File.Exists(oldImage))
                    File.Delete(oldImage);

                return new DeleteResponseRoot(true, "Offer deleted successfully");
            }

            throw new SiffrumException(
                ApiErrorTypeSM.Fatal_Log,
                "Failed to delete offer"
            );
        }
        #endregion

        #region Handle Offer per user

        private async Task<UserDM> GetUser(long userId)
        {
            var dm = await _apiDbContext.User
                .FirstOrDefaultAsync(x => x.Id == userId);

            if (dm == null)
            {
                throw new SiffrumException( ApiErrorTypeSM.InvalidInputData_Log,$"User with Id {userId} not found tries to access offers", "User not found");

            }

            return dm;
        }

        private List<UserOfferHandlerSM> GetOfferList(string json)
        {
            if (string.IsNullOrEmpty(json))
                return new List<UserOfferHandlerSM>();

            var list = JsonConvert.DeserializeObject<List<UserOfferHandlerSM>>(json)
                       ?? new List<UserOfferHandlerSM>();

            // ✅ Remove duplicates (keep latest occurrence)
            return list
                .GroupBy(x => x.OfferId)
                .Select(g => g.Last())
                .ToList();
        }

        private string SaveOfferList(List<UserOfferHandlerSM> list)
        {
            // ✅ Ensure no duplicates before saving
            var distinctList = list
                .GroupBy(x => x.OfferId)
                .Select(g => g.Last())
                .ToList();

            return JsonConvert.SerializeObject(distinctList);
        }

        // ✅ GET by userId + offerId
        public async Task<BoolResponseRoot?> IsOfferValid(long userId, long offerId)
        {
            var dm = await GetUser(userId);
            var oDm = await GetByIdAsync(offerId);

            if (oDm == null)
            {
                throw new SiffrumException(ApiErrorTypeSM.InvalidInputData_Log, $"Offer with Id {offerId} not found tries to access offers", "Offer not found");

            }
            var list = GetOfferList(dm.OfferJsonDetails);

            var sm = list.FirstOrDefault(x => x.OfferId == offerId);
            if(sm == null)
            {
                return new BoolResponseRoot(true, "Offer is valid to use");
            }
            return new BoolResponseRoot(false, "Offer is already being used by you");
        }

        // ✅ ADD or INCREMENT (no duplicates guaranteed)
        public async Task<BoolResponseRoot> AddOrIncrementOffer(long userId, long offerId)
        {
            var dm = await GetUser(userId);
            var oDm = await GetByIdAsync(offerId);

            if (oDm == null)
            {
                throw new SiffrumException(ApiErrorTypeSM.InvalidInputData_Log, $"Offer with Id {offerId} not found tries to access offers", "Offer not found");

            }

            var list = GetOfferList(dm.OfferJsonDetails);

            var offer = list.FirstOrDefault(x => x.OfferId == offerId);

            if (offer == null)
            {
                list.Add(new UserOfferHandlerSM
                {
                    OfferId = offerId,
                    Count = 1
                });
            }
            else
            {
                offer.Count++;
            }

            dm.OfferJsonDetails = SaveOfferList(list);
            await _apiDbContext.SaveChangesAsync();
            return new BoolResponseRoot(true, "Added data successfully");
        }

        #endregion Handle Offer per user
    }
}
