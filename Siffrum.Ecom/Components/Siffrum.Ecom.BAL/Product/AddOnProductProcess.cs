using AutoMapper; 
using Microsoft.EntityFrameworkCore;
using Siffrum.Ecom.BAL.Base.ImageProcess;
using Siffrum.Ecom.BAL.ExceptionHandler;
using Siffrum.Ecom.BAL.Foundation.Base;
using Siffrum.Ecom.DAL.Context;
using Siffrum.Ecom.DomainModels.v1;
using Siffrum.Ecom.ServiceModels.Foundation.Base.CommonResponseRoot;
using Siffrum.Ecom.ServiceModels.Foundation.Base.Enums;
using Siffrum.Ecom.ServiceModels.Foundation.Base.Interfaces;
using Siffrum.Ecom.ServiceModels.v1;

namespace Siffrum.Ecom.BAL.Product
{
    public class AddOnProductProcess : SiffrumBalOdataBase<AddOnProductsSM>
    {
        private readonly ILoginUserDetail _loginUserDetail;
        private readonly BrandProcess _brandProcess;
        private readonly CategoryProcess _categoryProcess;
        private readonly ImageProcess _imageProcess;
        public AddOnProductProcess(IMapper mapper, ApiDbContext apiDbContext, ILoginUserDetail loginUserDetail,
            BrandProcess brandProcess, CategoryProcess categoryProcess, ImageProcess imageProcess)
            : base(mapper, apiDbContext)
        {
            _loginUserDetail = loginUserDetail;
            _brandProcess = brandProcess;
            _categoryProcess = categoryProcess;
            _imageProcess = imageProcess;
        }

        #region OData
        public override async Task<IQueryable<AddOnProductsSM>> GetServiceModelEntitiesForOdata()
        {
            IQueryable<AddOnProductsDM> entitySet = _apiDbContext.AddonProducts.AsNoTracking();
            return await base.MapEntityAsToQuerable<AddOnProductsDM, AddOnProductsSM>(_mapper, entitySet);
        }
        #endregion

        #region Get

        #region Get All with Count

        public async Task<List<AddOnProductsSM>> GetAllAddOns(int skip, int top)
        {
            var dms = await _apiDbContext.AddonProducts.AsNoTracking()
                .OrderBy(x=>x.MainProductId)
                .Skip(skip).Take(top).ToListAsync();
            var sms = _mapper.Map<List<AddOnProductsSM>>(dms);
            return sms;
        }

        public async Task<IntResponseRoot> GetAllAddOnsCount()
        {
            var count = await _apiDbContext.AddonProducts.AsNoTracking()
                .CountAsync();

            return new IntResponseRoot(count, "Total AddOns");
        }

        #endregion Get All with Count

        #region Get by Id

        public async Task<AddOnProductsSM> GetAddOnById(long id)
        {
            var dm = await _apiDbContext.AddonProducts.FindAsync(id);
            if(dm == null)
            {
                return null;
            }
            var mainVariant = await _apiDbContext.ProductVariant
                .Include(x => x.Product)
                .AsNoTracking()
                .Where(x => x.Id == dm.MainProductId)
                .Select(x => new { x.Name, SellerId = x.Product.SellerId })
                .FirstOrDefaultAsync();
            var addonProductName = await _apiDbContext.ProductVariant.Where(x=>x.Id == dm.AddonProductId).Select(x=>x.Name).FirstOrDefaultAsync();
            
            var sm = _mapper.Map<AddOnProductsSM>(dm);
            sm.MainProductName = mainVariant?.Name;
            sm.AddonProductName = addonProductName;
            sm.SellerId = mainVariant?.SellerId ?? 0;
            return sm;
        }

        public async Task<List<AddOnProductsSM>> GetAddOnByMainProductId(long mainProductId)
        {
            var dm = await _apiDbContext.AddonProducts.AsNoTracking()
                .Where(x => x.MainProductId == mainProductId).ToListAsync();
            var response = new List<AddOnProductsSM>();
            if(dm.Count == 0)
            {
                return response;
            }
            foreach (var item in dm)
            {
                var res = await GetAddOnById(item.Id);
                if (res != null)
                {
                    response.Add(res);
                }
            }
            return response;
        }

        #endregion Get by Id

        #region Get By Main Product Id

        public async Task<AddonProductResponseSM> GetByMainProduct(long mainProductId)
        {
            var data = await _apiDbContext.AddonProducts
                .AsNoTracking()
                .Where(x => x.MainProductId == mainProductId)
                .Select(x => new
                {
                    x.Id,
                    x.AddonProductId,
                    ProductName = x.AddOnProduct.Name,
                    ProductImage = x.AddOnProduct.Image,
                    x.AddOnProduct.Price,
                    x.AddOnProduct.DiscountedPrice,
                    CategoryId = x.AddOnProduct.Product.CategoryId,
                    CategoryName = x.AddOnProduct.Product.Category.Name,
                    AllowedQuantity = x.AddOnProduct.TotalAllowedQuantity,
                    Stock = x.AddOnProduct.Stock,
                    IsCodAllowed = x.AddOnProduct.IsCodAllowed,
                })
                .ToListAsync();

            if (!data.Any())
                return null;

            var mainProduct = await _apiDbContext.ProductVariant
                .Include(x=>x.Product)
                .AsNoTracking()
                .Where(x => x.Id == mainProductId)
                .Select(x => new
                {
                    x.Id,
                    x.Name,
                    x.Image,
                    x.TotalAllowedQuantity,
                    x.Stock,
                    x.Price,
                    x.DiscountedPrice,
                    IsCodAllowed = x.IsCodAllowed,
                    CategoryId = x.Product.CategoryId,
                })
                .FirstOrDefaultAsync();

            var response = new AddonProductResponseSM
            {
                MainProductId = mainProduct.Id,
                Name = mainProduct.Name,
                Price = mainProduct.Price,
                DiscountedPrice = mainProduct.DiscountedPrice,
                AllowedQuantity = (int)mainProduct.TotalAllowedQuantity,
                Stock = (int)mainProduct.Stock   ,
                IsCodAllowed = mainProduct.IsCodAllowed,
                CategoryId = mainProduct.CategoryId,
            };
            var mainImg = await _imageProcess.ResolveImage(mainProduct.Image);
            response.Image = mainImg.Base64;
            response.NetworkImage = mainImg.NetworkUrl;

            var categories = data
                .GroupBy(x => new { x.CategoryId, x.CategoryName })
                .ToList();

            foreach (var g in categories)
            {
                var category = new AddonCategorySM
                {
                    CategoryId = g.Key.CategoryId,
                    CategoryName = g.Key.CategoryName
                };

                foreach (var p in g)
                {
                    category.Products.Add(new AddonProductItemSM
                    {
                        ProductVariantId = p.AddonProductId,
                        Name = p.ProductName,
                        Price = p.Price,
                        DiscountedPrice = p.DiscountedPrice,
                        AllowedQuantity = (int)p.AllowedQuantity,
                        Stock = (int)p.Stock,
                        IsCodAllowed = p.IsCodAllowed,
                        CategoryId = p.CategoryId,
                    });
                    var pImg = await _imageProcess.ResolveImage(p.ProductImage);
                    category.Products.Last().Image = pImg.Base64;
                    category.Products.Last().NetworkImage = pImg.NetworkUrl;
                }

                response.Categories.Add(category);
            }
            return response;
        }

        #endregion Get By Main Product Id

        #region Get By Product Id (product-level, not variant-level)

        public async Task<List<AddOnProductsSM>> GetAddOnsByProductId(long productId)
        {
            // Get all variant IDs belonging to this product
            var variantIds = await _apiDbContext.ProductVariant
                .AsNoTracking()
                .Where(v => v.ProductId == productId)
                .Select(v => v.Id)
                .ToListAsync();

            // Get all addon links where any of those variants is the main product
            var dms = await _apiDbContext.AddonProducts
                .AsNoTracking()
                .Where(x => variantIds.Contains(x.MainProductId))
                .ToListAsync();

            var response = new List<AddOnProductsSM>();
            foreach (var item in dms)
            {
                var res = await GetAddOnById(item.Id);
                if (res != null)
                    response.Add(res);
            }
            return response;
        }

        #endregion Get By Product Id

        #endregion Get

        #region Create 

        public async Task<AddOnProductsSM> CreateAsync(AddOnProductsSM objSM)
        {
            if(objSM == null)
            {
                throw new SiffrumException(
                    ApiErrorTypeSM.InvalidInputData_NoLog,
                    "Details to add not found"
                );
            }
            var existingDetails = await _apiDbContext.AddonProducts
                .Where(x=>x.MainProductId == objSM.MainProductId && x.AddonProductId == objSM.AddonProductId).FirstOrDefaultAsync();

            if(existingDetails != null)
            {
                throw new SiffrumException(
                    ApiErrorTypeSM.InvalidInputData_NoLog,
                    "Details to add already exists"
                );
            }

            var existingMainProduct = await _apiDbContext.ProductVariant.FindAsync(objSM.MainProductId);
            var existingAddOnProduct = await _apiDbContext.ProductVariant.FindAsync(objSM.AddonProductId);
            if(existingMainProduct == null || existingAddOnProduct == null)
            {
                throw new SiffrumException(ApiErrorTypeSM.Fatal_Log,
                    $"User tries to add Add on products to MainProduct with Id {objSM.MainProductId} or AddOnProduct with Id {objSM.AddonProductId} which does not exist",
                    "Product details not found");
            }
            if(existingMainProduct.PlatformType != existingAddOnProduct.PlatformType)
            {
                throw new SiffrumException(ApiErrorTypeSM.Fatal_Log,
                    $"User tries to add Add on products to MainProduct with Id {objSM.MainProductId} or AddOnProduct with Id {objSM.AddonProductId} which does not have the same platform type",
                    "Only products with same platform can be added");
            }
            var baseProduct = await _apiDbContext.Product.FindAsync(existingMainProduct.ProductId);
            if(baseProduct == null)
            {
                throw new SiffrumException(ApiErrorTypeSM.Fatal_Log,
                    $"User tries to add Add on products to MainProduct with Id {objSM.MainProductId} or AddOnProduct with Id {objSM.AddonProductId} and AddonProductId does not have a base product",
                    "Something went wrong while adding add on product");
            }
            var categoryId = baseProduct.CategoryId;
            var dm = new AddOnProductsDM
            {
                MainProductId = objSM.MainProductId,
                AddonProductId = objSM.AddonProductId,
                CategoryId = categoryId,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = _loginUserDetail.LoginId
            };

            _apiDbContext.AddonProducts.Add(dm);
            if( await _apiDbContext.SaveChangesAsync() > 0)
            {
                return await GetAddOnById(dm.Id);
            }
            throw new SiffrumException(ApiErrorTypeSM.Fatal_Log, "Failed to add add on product");
            
        }

        public async Task<AddOnProductsSM> CreateByProductAsync(long mainProductId, long addonProductId)
        {
            // Resolve main product → first variant
            var mainVariant = await _apiDbContext.ProductVariant
                .AsNoTracking()
                .Where(v => v.ProductId == mainProductId)
                .OrderBy(v => v.Id)
                .FirstOrDefaultAsync();

            if (mainVariant == null)
            {
                throw new SiffrumException(
                    ApiErrorTypeSM.InvalidInputData_NoLog,
                    "Main product has no variants. Please add at least one variant first.");
            }

            // Resolve addon product → first variant
            var addonVariant = await _apiDbContext.ProductVariant
                .AsNoTracking()
                .Where(v => v.ProductId == addonProductId)
                .OrderBy(v => v.Id)
                .FirstOrDefaultAsync();

            if (addonVariant == null)
            {
                throw new SiffrumException(
                    ApiErrorTypeSM.InvalidInputData_NoLog,
                    "Add-on product has no variants.");
            }

            var sm = new AddOnProductsSM
            {
                MainProductId = mainVariant.Id,
                AddonProductId = addonVariant.Id
            };
            return await CreateAsync(sm);
        }

        #endregion Create 

        #region Delete

        public async Task<DeleteResponseRoot> DeleteAsync(long id)
        {
            var dm = await _apiDbContext.AddonProducts.FirstOrDefaultAsync(x => x.Id == id);
            if (dm == null)
            {
                throw new SiffrumException(ApiErrorTypeSM.InvalidInputData_Log, "Add on product not found");
            }
            _apiDbContext.AddonProducts.Remove(dm);
            if(await _apiDbContext.SaveChangesAsync() > 0)
            {
                return new DeleteResponseRoot(true, "Add on product deleted successfully");
            }
            throw new SiffrumException(ApiErrorTypeSM.Fatal_Log, "Failed to delete add on product");
        }

        #endregion Delete
    }
}
