using AutoMapper;
using Google.Api.Gax;
using Microsoft.EntityFrameworkCore;
using Siffrum.Ecom.BAL.Base.ImageProcess;
using Siffrum.Ecom.BAL.ExceptionHandler;
using Siffrum.Ecom.BAL.Foundation.Base;
using Siffrum.Ecom.BAL.Product;
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
    public class BannerProcess : SiffrumBalOdataBase<BannerSM>
    {
        private readonly ILoginUserDetail _loginUserDetail;
        private readonly ImageProcess _imageProcess;
        private readonly ProductVariantProcess _productVariantProcess;

        public BannerProcess( ApiDbContext apiDbContext, IMapper mapper,ILoginUserDetail loginUserDetail, 
            ImageProcess imageProcess, ProductVariantProcess productVariantProcess)
            : base(mapper, apiDbContext)
        {
            _loginUserDetail = loginUserDetail;
            _imageProcess = imageProcess;
            _productVariantProcess = productVariantProcess;
        }

        #region ODATA
        public override async Task<IQueryable<BannerSM>> GetServiceModelEntitiesForOdata()
        {
            var entitySet = _apiDbContext.Banners.AsNoTracking();

            return await base.MapEntityAsToQuerable<BannerDM,BannerSM>(_mapper, entitySet);
        }
        #endregion

        #region CREATE
        public async Task<BoolResponseRoot> CreateAsync(BannerSM objSM)
        {
            if (objSM == null)
                throw new SiffrumException(
                    ApiErrorTypeSM.ModelError_NoLog,
                    "Banner data is required"
                );
            var dm = _mapper.Map<BannerDM>(objSM);
            if (!string.IsNullOrEmpty(objSM.ContentBase64))
            {
                var path = await _imageProcess.SaveFromBase64(objSM.ContentBase64,dm.Extension.ToString().ToLower(),"wwwroot/content/banners");
                if (!string.IsNullOrEmpty(path))
                {
                    dm.ContentPath = path;
                }                
            }

            dm.CreatedAt = DateTime.UtcNow;
            dm.CreatedBy = _loginUserDetail.LoginId;

            await _apiDbContext.Banners.AddAsync(dm);

            if (await _apiDbContext.SaveChangesAsync() > 0)
                return new BoolResponseRoot(true, "Banner created successfully");

            throw new SiffrumException(
                ApiErrorTypeSM.Fatal_Log,
                "Failed to create new Banner"
            );
        }
        #endregion

        #region READ
        public async Task<BannerSM?> GetByIdAsync(long id)
        {
            var dm = await _apiDbContext.Banners.FindAsync(id);
            if (dm == null) return null;

            var sm = _mapper.Map<BannerSM>(dm);
            if (!string.IsNullOrEmpty(dm.ContentPath))
            {
                var base64 = await _imageProcess.ConvertToBase64(dm.ContentPath);
                if (!string.IsNullOrEmpty(base64))
                {
                    sm.ContentBase64 = base64;
                }
            }
            return sm;
        }

        public async Task<BannerSM?> GetByBannerTypeAndDefaultAsync(BannerTypeSM banner, PlatformTypeSM platform)
        {
            var dm =  _apiDbContext.Banners.AsNoTracking()
                .Where(x=>x.BannerType == (BannerTypeDM)banner && x.PlatformType == (PlatformTypeDM)platform && x.IsDefault == true)
                .FirstOrDefault();
            if (dm == null)
            {
                return null;
            }

            var sm = _mapper.Map<BannerSM>(dm);
            if (!string.IsNullOrEmpty(dm.ContentPath))
            {
                var base64 = await _imageProcess.ConvertToBase64(dm.ContentPath);
                if (!string.IsNullOrEmpty(base64))
                {
                    sm.ContentBase64 = base64;
                }
            }
            return sm;
        }

        public async Task<List<BannerSM>> GetAll(int skip, int top)
        {
            var ids = await _apiDbContext.Banners
                .AsNoTracking()
                .OrderBy(x=>x.Priority)
                .Skip(skip)
                .Take(top)
                .Select(x => x.Id)
                .ToListAsync();

            var response = new List<BannerSM>();
            foreach (var id in ids)
            {
                var banner = await GetByIdAsync(id);
                if (banner != null)
                    response.Add(banner);
            }
            return response;
        }

        public async Task<IntResponseRoot> GetCount()
        {
            var count = await _apiDbContext.Banners
                .CountAsync();
            return new IntResponseRoot(count, "Total Banners");
        }
        public async Task<List<BannerSM>> GetAllByBannerType(BannerTypeSM bannerType,PlatformTypeSM platform, int skip, int top)
        {
            var ids = await _apiDbContext.Banners
                .AsNoTracking()                
                .Where(x=>x.BannerType == (BannerTypeDM)bannerType && x.PlatformType == (PlatformTypeDM)platform)
                .OrderBy(x => x.Priority)
                .Skip(skip)
                .Take(top)
                .Select(x => x.Id)
                .ToListAsync();

            var response = new List<BannerSM>();
            foreach (var id in ids)
            {
                var banner = await GetByIdAsync(id);
                if (banner != null)
                    response.Add(banner);
            }
            return response;
        }

        public async Task<IntResponseRoot> GetAllByBannerTypeCount(BannerTypeSM banner, PlatformTypeSM platform)
        {
            var count = await _apiDbContext.Banners
                .Where(x => x.BannerType == (BannerTypeDM)banner && x.PlatformType == (PlatformTypeDM)platform)
                .CountAsync();
            return new IntResponseRoot(count, "Total Banners");
        }

        public async Task<List<BannerSM>> GetAllByProductBannerType(PlatformTypeSM platform, int skip, int top)
        {
            var ids = await _apiDbContext.Banners
                .AsNoTracking()
                .Where(x =>
                x.BannerType == BannerTypeDM.ProductBanner && x.PlatformType == (PlatformTypeDM)platform &&
                x.BannerProducts.Any(bp => bp.ProductVariant.PlatformType == (PlatformTypeDM)platform)
                )
                .OrderBy(x => x.Priority)
                .Skip(skip)
                .Take(top)
                .Select(x => x.Id)
                .ToListAsync();
            var response = new List<BannerSM>();
            foreach (var id in ids)
            {
                var banner = await GetByIdAsync(id);
                if (banner != null)
                    response.Add(banner);
            }
            return response;
        }

        public async Task<IntResponseRoot> GetAllByProductBannerTypeCount(PlatformTypeSM platform)
        {
            var count = await _apiDbContext.Banners
                .AsNoTracking()
                .Where(x =>
                x.BannerType == BannerTypeDM.ProductBanner && x.PlatformType == (PlatformTypeDM)platform &&
                x.BannerProducts.Any(bp => bp.ProductVariant.PlatformType == (PlatformTypeDM)platform)
                )
                .OrderBy(x => x.Priority)
                .Select(x => x.Id)
                .CountAsync();
            return new IntResponseRoot(count, "Total Product Banners");
        }

        #endregion

        #region UPDATE
        public async Task<BannerSM?> UpdateAsync(long id, BannerSM objSM)
        {
            var dm = await _apiDbContext.Banners
                .FirstOrDefaultAsync(x => x.Id == id);

            if (dm == null)
                throw new SiffrumException(
                    ApiErrorTypeSM.InvalidInputData_NoLog,
                    "Banner not found"
                );
            
            _mapper.Map(objSM, dm);
            string oldImage = null;
            if (!string.IsNullOrEmpty(objSM.ContentBase64))
            {
                if(objSM.Extension == null)
                {
                    throw new SiffrumException(
                        ApiErrorTypeSM.InvalidInputData_NoLog,
                        "Extension not found"
                    );
                }
                var imagePath = await _imageProcess.SaveFromBase64(objSM.ContentBase64, objSM.Extension.ToString().ToLower(), "wwwroot/content/banners");
                if (!string.IsNullOrEmpty(imagePath))
                {
                    oldImage = dm.ContentPath;
                    dm.ContentPath = imagePath;
                }
            }
            dm.Id = id;
            dm.UpdatedAt = DateTime.UtcNow;
            dm.UpdatedBy = _loginUserDetail.LoginId;

            if (await _apiDbContext.SaveChangesAsync() > 0)
            {
                if (File.Exists(oldImage)) File.Delete(oldImage);
                return await GetByIdAsync(id);
            }

            throw new SiffrumException(
                ApiErrorTypeSM.Fatal_Log,
                "Failed to update banner"
            );
        }

        public async Task<BoolResponseRoot?> UpdateDefaultStatusAsync(long id, bool isDefault)
        {
            var dm = await _apiDbContext.Banners
                .FirstOrDefaultAsync(x => x.Id == id);

            if (dm == null)
                throw new SiffrumException(
                    ApiErrorTypeSM.InvalidInputData_NoLog,
                    "Banner not found"
                );
            if(dm.IsDefault == isDefault)
            {
                return new BoolResponseRoot(false, "Banner default status already updated");
            }
            dm.IsDefault = isDefault;
            if (await _apiDbContext.SaveChangesAsync() > 0)
            {
                return new BoolResponseRoot(true, "Banner default status updated successfully");
            }

            throw new SiffrumException(
                ApiErrorTypeSM.Fatal_Log,
                "Failed to update banner status"
            );
        }
        #endregion

        #region DELETE
        public async Task<DeleteResponseRoot> DeleteAsync(long id)
        {
            var dm = await _apiDbContext.Banners
                .FirstOrDefaultAsync(x => x.Id == id);

            if (dm == null)
                throw new SiffrumException(
                    ApiErrorTypeSM.InvalidInputData_Log,
                    "Banner not found"
                );

            string oldPath = null;
            if (!string.IsNullOrEmpty(dm.ContentPath))
            {
                oldPath = dm.ContentPath;
            }
            _apiDbContext.Banners.Remove(dm);

            if (await _apiDbContext.SaveChangesAsync() > 0)
            {
                if (File.Exists(oldPath)) File.Delete(oldPath);
                return new DeleteResponseRoot(true, "Banner deleted successfully");
            }

            throw new SiffrumException(
                ApiErrorTypeSM.Fatal_Log,
                "Failed to delete banner details"
            );
        }
        #endregion

        #region Product Banner

        #region Get

        public async Task<List<UserHotBoxProductSM>> GetAllHotBoxProductsInBanner(long id, int skip, int top)
        {
            var banner = await GetByIdAsync(id);
            if(banner == null || banner.BannerType != BannerTypeSM.ProductBanner)
            {
                throw new SiffrumException(ApiErrorTypeSM.InvalidInputData_Log,$"Either banner not found or invalid banner type for getting products for banner id: {id}",
                    "Banner not found or invalid banner type");
            }
            var ids = await _apiDbContext.ProductBanners
                .AsNoTracking()
                .Where(x => x.BannerId == id && x.ProductVariant.PlatformType == PlatformTypeDM.HotBox && x.ProductVariant.Status == ProductStatusDM.Active)
                .Skip(skip)
                .Take(top)
                .Select(x => x.ProductId)
                .ToListAsync();
            if (ids.Count == 0)
            {
                return new List<UserHotBoxProductSM>();
            }
            var products = await _productVariantProcess.GetHotBoxProductsByBanner(ids);
            return products;
        }

        public async Task<IntResponseRoot> GetAllHotBoxProductsInBannerCount(long id)
        {
            var banner = await GetByIdAsync(id);
            if (banner == null || banner.BannerType != BannerTypeSM.ProductBanner)
            {
                throw new SiffrumException(ApiErrorTypeSM.InvalidInputData_Log, $"Either banner not found or invalid banner type for getting products for banner id: {id}",
                    "Banner not found or invalid banner type");
            }
            var ids = await _apiDbContext.ProductBanners
                .AsNoTracking()
                .Where(x => x.BannerId == id && x.ProductVariant.PlatformType == PlatformTypeDM.HotBox)
                .Select(x => x.ProductId)
                .CountAsync();
            return new IntResponseRoot(ids, "Total products in banner");
        }

        public async Task<List<UserSpeedyMartProductSM>> GetAllSpeedyMartProductsInBanner(long id, int skip, int top)
        {
            var banner = await GetByIdAsync(id);
            if (banner == null || banner.BannerType != BannerTypeSM.ProductBanner)
            {
                throw new SiffrumException(ApiErrorTypeSM.InvalidInputData_Log, $"Either banner not found or invalid banner type for getting products for banner id: {id}",
                    "Banner not found or invalid banner type");
            }
            var ids = await _apiDbContext.ProductBanners
                .AsNoTracking()
                .Where(x => x.BannerId == id && x.ProductVariant.PlatformType == PlatformTypeDM.HotBox && x.ProductVariant.Status == ProductStatusDM.Active)
                .Skip(skip)
                .Take(top)
                .Select(x => x.ProductId)
                .ToListAsync();
            if(ids.Count == 0)
            {
                return new List<UserSpeedyMartProductSM>();
            }
            var products = await _productVariantProcess.GetSpeedyMartProductsByBanner(ids);
            return products;
        }

        public async Task<IntResponseRoot> GetAllSpeedyMartProductsInBannerCount(long id)
        {
            var banner = await GetByIdAsync(id);
            if (banner == null || banner.BannerType != BannerTypeSM.ProductBanner)
            {
                throw new SiffrumException(ApiErrorTypeSM.InvalidInputData_Log, $"Either banner not found or invalid banner type for getting products for banner id: {id}",
                    "Banner not found or invalid banner type");
            }
            var ids = await _apiDbContext.ProductBanners
                .AsNoTracking()
                .Where(x => x.BannerId == id && x.ProductVariant.PlatformType == PlatformTypeDM.SpeedyMart)
                .Select(x => x.ProductId)
                .CountAsync();
            return new IntResponseRoot(ids, "Total products in banner");
        }
        #endregion Get

        #region Add Product To Banner

        public async Task<ProductBannerSM> AddProductsToBanner(ProductBannerSM objSM)
        {
            var banner = await GetByIdAsync(objSM.BannerId);
            if (banner == null || banner.BannerType != BannerTypeSM.ProductBanner)
            {
                throw new SiffrumException(ApiErrorTypeSM.InvalidInputData_Log, $"Either banner not found or invalid banner type for adding products to banner id: {objSM.BannerId}",
                    "Banner not found or invalid banner type");
            }
            var product = await _productVariantProcess.GetProductVariantById(objSM.ProductId);
            if (product == null)
            {
                throw new SiffrumException(ApiErrorTypeSM.InvalidInputData_Log, 
                    "Product not found");
            }
            var existingProducts = await _apiDbContext.ProductBanners.Where(x=>x.ProductId == objSM.ProductId && x.BannerId == objSM.BannerId).FirstOrDefaultAsync();
            if(existingProducts != null)
            {
                return _mapper.Map<ProductBannerSM>(existingProducts);
            }
            var pb = _mapper.Map<ProductBannerDM>(objSM);
            await _apiDbContext.ProductBanners.AddAsync(pb);
            if (await _apiDbContext.SaveChangesAsync() > 0)
            {
                return _mapper.Map<ProductBannerSM>(pb);
            }
            throw new SiffrumException(ApiErrorTypeSM.Fatal_Log, "Failed to add product to banner", "Failed to add product to banner");
        }
        #endregion Add Product To Banner

        #region Delete Product From Banner

        public async Task<DeleteResponseRoot> DeleteProductFromBanner(long productBannerId)
        {

            var pb = await _apiDbContext.ProductBanners.FirstOrDefaultAsync(x => x.Id == productBannerId);
            if (pb == null)
            {
                throw new SiffrumException(ApiErrorTypeSM.InvalidInputData_Log,
                    "Product not found in banner");
            }
            _apiDbContext.ProductBanners.Remove(pb);
            if (await _apiDbContext.SaveChangesAsync() > 0)
            {
                return new DeleteResponseRoot(true, "Product deleted from banner successfully");
            }
            throw new SiffrumException(ApiErrorTypeSM.Fatal_Log, "Failed to delete product from banner", "Failed to delete product from banner");
        }

        #endregion Delete Product From Banner

        #endregion Product Banner
    }
}
