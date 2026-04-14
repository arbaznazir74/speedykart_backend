using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Siffrum.Ecom.BAL.ExceptionHandler;
using Siffrum.Ecom.BAL.Foundation.Base;
using Siffrum.Ecom.DAL.Context;
using Siffrum.Ecom.DomainModels.v1;
using Siffrum.Ecom.ServiceModels.Foundation.Base.CommonResponseRoot;
using Siffrum.Ecom.ServiceModels.Foundation.Base.Enums;
using Siffrum.Ecom.ServiceModels.Foundation.Base.Interfaces;
using Siffrum.Ecom.ServiceModels.v1;

namespace Siffrum.Ecom.BAL.LoginUsers
{
    public class SellerSettingsProcess : SiffrumBalBase
    {
        private readonly ILoginUserDetail _loginUserDetail;

        public SellerSettingsProcess(
            IMapper mapper,
            ApiDbContext apiDbContext,
            ILoginUserDetail loginUserDetail)
            : base(mapper, apiDbContext)
        {
            _loginUserDetail = loginUserDetail;
        }

        #region Helpers (JSON)

        private SellerSettingsJson Deserialize(string json)
        {
            return string.IsNullOrEmpty(json)
                ? new SellerSettingsJson()
                : JsonConvert.DeserializeObject<SellerSettingsJson>(json);
        }

        private string Serialize(SellerSettingsJson sm)
        {
            return JsonConvert.SerializeObject(sm);
        }

        #endregion

        #region CREATE (Only One Row Per Seller)

        public async Task<SellerSettingsSM> CreateAsync(SellerSettingsSM objSM)
        {
            if (objSM == null)
                throw new SiffrumException(
                    ApiErrorTypeSM.ModelError_NoLog,
                    "Seller settings data is required");

            if (objSM.SellerId <= 0)
                throw new SiffrumException(
                    ApiErrorTypeSM.ModelError_NoLog,
                    "Invalid SellerId");

            // Check Seller Exists
            var sellerExists = await _apiDbContext.Seller
                .AnyAsync(x => x.Id == objSM.SellerId);

            if (!sellerExists)
                throw new SiffrumException(
                    ApiErrorTypeSM.InvalidInputData_NoLog,
                    "Seller not found");

            // Only one settings per seller
            var exists = await _apiDbContext.SellerSettings
                .AnyAsync(x => x.SellerId == objSM.SellerId);

            if (exists)
                throw new SiffrumException(
                    ApiErrorTypeSM.InvalidInputData_NoLog,
                    "Seller settings already exist. Use update.");

            var dm = new SellerSettingsDM
            {
                SellerId = objSM.SellerId,
                JsonData = Serialize(objSM.SellerSettingsJson),
                CreatedAt = DateTime.UtcNow,
                CreatedBy = _loginUserDetail.LoginId
            };

            await _apiDbContext.SellerSettings.AddAsync(dm);

            if (await _apiDbContext.SaveChangesAsync() > 0)
                return await GetBySellerIdAsync(objSM.SellerId);

            throw new SiffrumException(
                ApiErrorTypeSM.Fatal_Log,
                "Failed to create seller settings");
        }

        #endregion

        #region READ

        public async Task<SellerSettingsSM> GetBySellerIdAsync(long sellerId)
        {
            if (sellerId <= 0)
                throw new SiffrumException(
                    ApiErrorTypeSM.ModelError_NoLog,
                    "Invalid SellerId");

            var dm = await _apiDbContext.SellerSettings
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.SellerId == sellerId);

            if (dm == null)
                return new SellerSettingsSM
                {
                    SellerId = sellerId,
                    SellerSettingsJson = new SellerSettingsJson()
                };

            var sm = new SellerSettingsSM
            {
                Id = dm.Id,
                SellerId = dm.SellerId,
                SellerSettingsJson = Deserialize(dm.JsonData)
            };

            return sm;
        }

        public async Task<SellerSettingsSM> SellerSettingsByPincode(string pincode)
        {
            var sellerPincode = await _apiDbContext.DeliveryPlaces
        .FirstOrDefaultAsync(x => x.Pincode == pincode);
            if (sellerPincode == null)
            {
                return null;
            }
                
            var sellerId = sellerPincode.SellerId;

            var dm = await _apiDbContext.SellerSettings
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.SellerId == sellerId);

            if (dm == null)
            {
                /*return new SellerSettingsSM
                {
                    SellerId = sellerId,
                    SellerSettingsJson = new SellerSettingsJson()
                };*/
                return null;
            }
                

            var sm = new SellerSettingsSM
            {
                Id = dm.Id,
                SellerId = dm.SellerId,
                SellerSettingsJson = Deserialize(dm.JsonData)
            };

            return sm;
        }

        #endregion

        #region UPDATE

        public async Task<SellerSettingsSM> UpdateAsync(SellerSettingsSM objSM)
        {
            if (objSM == null)
                throw new SiffrumException(
                    ApiErrorTypeSM.ModelError_NoLog,
                    "Seller settings data is required");

            if (objSM.SellerId <= 0)
                throw new SiffrumException(
                    ApiErrorTypeSM.ModelError_NoLog,
                    "Invalid SellerId");

            var dm = await _apiDbContext.SellerSettings
                .FirstOrDefaultAsync(x => x.SellerId == objSM.SellerId);

            if (dm == null)
            {
                var res = await CreateAsync(objSM);
                return res;
            }
            else
            {
                dm.JsonData = Serialize(objSM.SellerSettingsJson);
                dm.UpdatedAt = DateTime.UtcNow;
                dm.UpdatedBy = _loginUserDetail.LoginId;

                await _apiDbContext.SaveChangesAsync();
                var result = new SellerSettingsSM
                {
                    Id = dm.Id,
                    SellerId = dm.SellerId,
                    SellerSettingsJson = Deserialize(dm.JsonData)
                };
                return result;
            }
            
        }

        #endregion

        #region DELETE

        public async Task<DeleteResponseRoot> DeleteAsync(long sellerId)
        {
            if (sellerId <= 0)
                throw new SiffrumException(
                    ApiErrorTypeSM.ModelError_NoLog,
                    "Invalid SellerId");

            var dm = await _apiDbContext.SellerSettings
                .FirstOrDefaultAsync(x => x.SellerId == sellerId);

            if (dm == null)
                throw new SiffrumException(
                    ApiErrorTypeSM.InvalidInputData_Log,
                    "Seller settings not found");

            _apiDbContext.SellerSettings.Remove(dm);

            await _apiDbContext.SaveChangesAsync();

            return new DeleteResponseRoot(true, "Seller settings deleted successfully");
        }

        #endregion
    }
}