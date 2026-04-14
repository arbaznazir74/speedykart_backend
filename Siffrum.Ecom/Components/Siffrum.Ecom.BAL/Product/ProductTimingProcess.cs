using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Siffrum.Ecom.BAL.Foundation.Base;
using Siffrum.Ecom.DAL.Context;
using Siffrum.Ecom.DomainModels.Enums;
using Siffrum.Ecom.DomainModels.v1;
using Siffrum.Ecom.ServiceModels.Enums;
using Siffrum.Ecom.ServiceModels.Foundation.Base.CommonResponseRoot;
using Siffrum.Ecom.ServiceModels.Foundation.Base.Interfaces;
using Siffrum.Ecom.ServiceModels.v1;

namespace Siffrum.Ecom.BAL.Product
{
    public class ProductTimingProcess : SiffrumBalBase
    {
        private readonly ILoginUserDetail _loginUserDetail;
        private readonly ProductVariantProcess _productVariantProcess;

        public ProductTimingProcess(
            IMapper mapper,
            ApiDbContext apiDbContext,
            ILoginUserDetail loginUserDetail,
            ProductVariantProcess productVariantProcess)
            : base(mapper, apiDbContext)
        {
            _loginUserDetail = loginUserDetail;
            _productVariantProcess = productVariantProcess;
        }

        #region Seller CRUD

        public async Task<ProductTimingSM> Create(ProductTimingSM sm)
        {
            var dm = new ProductTimingDM
            {
                SellerId = _loginUserDetail.DbRecordId,
                ProductId = sm.ProductId,
                CategoryId = sm.CategoryId,
                StartHour = sm.StartHour,
                StartMinute = sm.StartMinute,
                EndHour = sm.EndHour,
                EndMinute = sm.EndMinute,
                IsActive = true,
                CreatedBy = _loginUserDetail.LoginId,
                CreatedAt = DateTime.UtcNow
            };

            await _apiDbContext.ProductTimings.AddAsync(dm);
            await _apiDbContext.SaveChangesAsync();

            sm.Id = dm.Id;
            sm.SellerId = dm.SellerId;
            sm.IsActive = dm.IsActive;
            return sm;
        }

        public async Task<List<ProductTimingSM>> GetBySeller(int skip, int top)
        {
            var sellerId = _loginUserDetail.DbRecordId;

            return await _apiDbContext.ProductTimings
                .AsNoTracking()
                .Where(x => x.SellerId == sellerId)
                .OrderByDescending(x => x.Id)
                .Skip(skip).Take(top)
                .Select(x => new ProductTimingSM
                {
                    Id = x.Id,
                    SellerId = x.SellerId,
                    ProductId = x.ProductId,
                    CategoryId = x.CategoryId,
                    StartHour = x.StartHour,
                    StartMinute = x.StartMinute,
                    EndHour = x.EndHour,
                    EndMinute = x.EndMinute,
                    IsActive = x.IsActive,
                    ProductName = x.Product.Name,
                    CategoryName = x.Category.Name
                })
                .ToListAsync();
        }

        public async Task<IntResponseRoot> GetBySellerCount()
        {
            var sellerId = _loginUserDetail.DbRecordId;
            var count = await _apiDbContext.ProductTimings
                .AsNoTracking()
                .Where(x => x.SellerId == sellerId)
                .CountAsync();
            return new IntResponseRoot(count, "Total product timings");
        }

        public async Task<ProductTimingSM> Update(long id, ProductTimingSM sm)
        {
            var dm = await _apiDbContext.ProductTimings
                .FirstOrDefaultAsync(x => x.Id == id && x.SellerId == _loginUserDetail.DbRecordId);
            if (dm == null) return null;

            dm.ProductId = sm.ProductId;
            dm.CategoryId = sm.CategoryId;
            dm.StartHour = sm.StartHour;
            dm.StartMinute = sm.StartMinute;
            dm.EndHour = sm.EndHour;
            dm.EndMinute = sm.EndMinute;
            dm.IsActive = sm.IsActive;
            dm.UpdatedBy = _loginUserDetail.LoginId;
            dm.UpdatedAt = DateTime.UtcNow;

            await _apiDbContext.SaveChangesAsync();

            sm.Id = dm.Id;
            sm.SellerId = dm.SellerId;
            return sm;
        }

        public async Task<BoolResponseRoot> Delete(long id)
        {
            var dm = await _apiDbContext.ProductTimings
                .FirstOrDefaultAsync(x => x.Id == id && x.SellerId == _loginUserDetail.DbRecordId);
            if (dm == null) return new BoolResponseRoot(false, "Not found");

            _apiDbContext.ProductTimings.Remove(dm);
            await _apiDbContext.SaveChangesAsync();
            return new BoolResponseRoot(true, "Deleted");
        }

        #endregion Seller CRUD

        #region User Timing Query

        public async Task<UserHotBoxCategoryProductsSM> GetProductsByTiming(
            CategoryTimingSM timing, int skip, int top)
        {
            var (startHour, endHour) = MapTimingToHours(timing);

            // Find all active product timings whose range overlaps the requested slot
            var query = _apiDbContext.ProductTimings
                .AsNoTracking()
                .Where(x => x.IsActive && x.Product.ProductVariants.Any(
                    v => v.PlatformType == PlatformTypeDM.HotBox && v.Status == ProductStatusDM.Active));

            // Overlap logic: timing slot [startHour..endHour) overlaps entry [StartHour..EndHour)
            // Handles cross-midnight too (e.g. 22:00 → 05:00)
            query = query.Where(x =>
                // Normal range (no midnight crossing on either side)
                (x.StartHour < x.EndHour && startHour < endHour &&
                    x.StartHour < endHour && startHour < x.EndHour) ||
                // Entry crosses midnight
                (x.StartHour >= x.EndHour &&
                    (startHour < x.EndHour || startHour >= x.StartHour || endHour > x.StartHour || endHour <= x.EndHour)) ||
                // Slot crosses midnight  
                (startHour >= endHour &&
                    (x.StartHour < endHour || x.StartHour >= startHour || x.EndHour > startHour || x.EndHour <= endHour))
            );

            var timings = await query
                .OrderByDescending(x => x.Id)
                .Skip(skip).Take(top)
                .Select(x => new
                {
                    x.ProductId,
                    x.CategoryId,
                    CategoryName = x.Category.Name
                })
                .ToListAsync();

            if (timings.Count == 0)
            {
                return new UserHotBoxCategoryProductsSM
                {
                    Id = 0,
                    CategoryName = "No items available",
                    Products = new List<UserHotBoxProductSM>(),
                    ComboProducts = new List<ComboProductSM>()
                };
            }

            // Get one variant per product (lowest priced)
            var productIds = timings.Select(x => x.ProductId).Distinct().ToList();
            var variantIds = await _apiDbContext.ProductVariant
                .AsNoTracking()
                .Where(v => productIds.Contains(v.ProductId) &&
                            v.PlatformType == PlatformTypeDM.HotBox &&
                            v.Status == ProductStatusDM.Active)
                .GroupBy(v => v.ProductId)
                .Select(g => g.OrderBy(v => v.DiscountedPrice > 0 ? v.DiscountedPrice : v.Price).First().Id)
                .ToListAsync();

            var products = await _productVariantProcess.GetHotBoxProductsByBanner(variantIds);

            var firstCat = timings.First();
            return new UserHotBoxCategoryProductsSM
            {
                Id = firstCat.CategoryId,
                CategoryName = firstCat.CategoryName,
                Products = products,
                ComboProducts = new List<ComboProductSM>()
            };
        }

        public async Task<IntResponseRoot> GetProductsByTimingCount(CategoryTimingSM timing)
        {
            var (startHour, endHour) = MapTimingToHours(timing);

            var query = _apiDbContext.ProductTimings
                .AsNoTracking()
                .Where(x => x.IsActive && x.Product.ProductVariants.Any(
                    v => v.PlatformType == PlatformTypeDM.HotBox && v.Status == ProductStatusDM.Active));

            query = query.Where(x =>
                (x.StartHour < x.EndHour && startHour < endHour &&
                    x.StartHour < endHour && startHour < x.EndHour) ||
                (x.StartHour >= x.EndHour &&
                    (startHour < x.EndHour || startHour >= x.StartHour || endHour > x.StartHour || endHour <= x.EndHour)) ||
                (startHour >= endHour &&
                    (x.StartHour < endHour || x.StartHour >= startHour || x.EndHour > startHour || x.EndHour <= endHour))
            );

            var count = await query.Select(x => x.ProductId).Distinct().CountAsync();
            return new IntResponseRoot(count, "Total products for timing");
        }

        private static (int startHour, int endHour) MapTimingToHours(CategoryTimingSM timing)
        {
            return timing switch
            {
                CategoryTimingSM.EarlyMorning => (5, 8),
                CategoryTimingSM.Morning => (8, 11),
                CategoryTimingSM.Brunch => (11, 13),
                CategoryTimingSM.Lunch => (13, 16),
                CategoryTimingSM.Evening => (16, 19),
                CategoryTimingSM.Dinner => (19, 22),
                CategoryTimingSM.LateNight => (22, 5),   // crosses midnight
                CategoryTimingSM.AllDay => (0, 24),
                _ => (0, 24)
            };
        }

        #endregion User Timing Query
    }
}
