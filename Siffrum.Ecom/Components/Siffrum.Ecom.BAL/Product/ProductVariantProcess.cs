using AutoMapper;
using Microsoft.EntityFrameworkCore;
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

namespace Siffrum.Ecom.BAL.Product
{
    public class ProductVariantProcess : SiffrumBalOdataBase<ProductVariantSM>
    {
        private readonly ILoginUserDetail _loginUserDetail;
        private readonly ImageProcess  _imageProcess;
        private readonly ProductImagesProcess _productImages;
        private readonly ProductNutritionProcess _productNutrition;
        private readonly ProductRatingProcess _productRating;
        private readonly ProductUnitProcess _productUnit;
        private readonly ProductFaqProcess _productFaq;
        private readonly ProductSpecificationProcess _productSpecification;
        private readonly ProductSpecificationFilterProcess _productSpecificationFilter;
        private readonly ComboProcess _comboProcess;
        private readonly ProductToppingProcess _productToppingProcess;


        public ProductVariantProcess(IMapper mapper, ApiDbContext apiDbContext, ILoginUserDetail loginUserDetail, ProductImagesProcess productImages, ProductNutritionProcess productNutrition,
            ProductRatingProcess productRating, ProductUnitProcess productUnit, ProductFaqProcess productFaq, ImageProcess imageProcess,ComboProcess comboProcess,
            ProductSpecificationProcess productSpecificationProcess, ProductSpecificationFilterProcess productSpecificationFilterProcess,
            ProductToppingProcess productToppingProcess)
            : base(mapper, apiDbContext)
        {
            _loginUserDetail = loginUserDetail;
            _imageProcess = imageProcess;
            _productImages = productImages;
            _productNutrition = productNutrition;
            _productRating = productRating;
            _productUnit = productUnit;
            _productFaq = productFaq;
            _productSpecification = productSpecificationProcess;
            _productSpecificationFilter = productSpecificationFilterProcess;
            _comboProcess = comboProcess;
            _productToppingProcess = productToppingProcess;
        }

        #region OData
        public override async Task<IQueryable<ProductVariantSM>> GetServiceModelEntitiesForOdata()
        {
            IQueryable<ProductVariantDM> entitySet = _apiDbContext.ProductVariant.AsNoTracking();
            return await base.MapEntityAsToQuerable<ProductVariantDM, ProductVariantSM>(_mapper, entitySet);
        }
        #endregion

        #region Get All and Counts

        #region All

        #region Get All

        public async Task<List<ProductVariantSM>> GetAll(int skip, int top)
        {
            var variantIds = await _apiDbContext.ProductVariant
                .AsNoTracking()
                .OrderBy(x => x.ProductId)
                .ThenBy(x => x.Id)
                .Skip(skip).Take(top)
                .Select(x => x.Id)
                .ToListAsync();

            if (variantIds.Count == 0)
            {
                return new List<ProductVariantSM>();
            }

            var result = new List<ProductVariantSM>();

            foreach (var id in variantIds)
            {
                var response = await GetProductVariantById(id);
                if (response != null)
                {
                    result.Add(response);
                }
            }

            return _mapper.Map<List<ProductVariantSM>>(result);
        }


        public async Task<IntResponseRoot> GetAllCount()
        {
            var count = await _apiDbContext.ProductVariant.AsNoTracking().CountAsync();
            return new IntResponseRoot(count, "Total Product variants");
        }

        public async Task<List<ProductVariantSM>> GetAllByCategoryId(long categoryId, int skip, int top)
        {
            var variantIds = await _apiDbContext.ProductVariant
                .AsNoTracking()
                .Where(x=>x.Product.CategoryId == categoryId)
                .OrderBy(x => x.ProductId)
                .ThenBy(x => x.Id)
                .Skip(skip).Take(top)
                .Select(x => x.Id)
                .ToListAsync();

            if (variantIds.Count == 0)
            {
                return new List<ProductVariantSM>();
            }

            var result = new List<ProductVariantSM>();

            foreach (var id in variantIds)
            {
                var response = await GetProductVariantById(id);
                if (response != null)
                {
                    result.Add(response);
                }
            }

            return _mapper.Map<List<ProductVariantSM>>(result);
        }


        public async Task<IntResponseRoot> GetAllByCategoryIdCount(long categoryId)
        {
            var count = await _apiDbContext.ProductVariant
                .AsNoTracking()
                .Where(x => x.Product.CategoryId == categoryId)
                .OrderBy(x => x.ProductId)
                .ThenBy(x => x.Id)
                .Select(x => x.Id)
                .CountAsync();
            return new IntResponseRoot(count, "Total Product variants");
        }
        
        public async Task<List<ProductVariantSM>> GetAllVariantsByProductID(long productId)
        {
            var variantIds = await _apiDbContext.ProductVariant
                .AsNoTracking()
                .Where(x => x.ProductId == productId)
                .Select(x => x.Id)
                .ToListAsync();

            if (variantIds.Count == 0)
            {
                return new List<ProductVariantSM>();
            }

            var result = new List<ProductVariantSM>();

            foreach (var id in variantIds)
            {
                var response = await GetProductVariantById(id);
                if (response != null)
                {
                    result.Add(response);
                }
            }

            return _mapper.Map<List<ProductVariantSM>>(result);
        }

        #endregion Get All              

        #endregion All

        #region Search

        public async Task<List<SearchResponseSM>> SearchProducts(
            long sellerId,
            string searchText,
            int skip,
            int top)
        {
            if (string.IsNullOrWhiteSpace(searchText))
                return new List<SearchResponseSM>();

            var words = searchText
                .Trim()
                .Split(' ', StringSplitOptions.RemoveEmptyEntries);

            IQueryable<ProductVariantDM> query = _apiDbContext.ProductVariant
                .AsNoTracking();

            // Seller filter
            if (sellerId > 0)
            {
                query = query.Where(x => x.Product != null &&
                                         x.Product.SellerId == sellerId);
            }

            // Multi-word search using LIKE (Better performance)
            foreach (var word in words)
            {
                query = query.Where(x => x.Name.ToLower().Contains(word.ToLower()));
            }

            var response = await query
                .OrderBy(x => x.Name)
                .Skip(skip)
                .Take(top)
                .Select(x => new SearchResponseSM
                {
                    Id = x.Id,
                    Title = x.Name
                })
                .ToListAsync();
            return response;
        }

        #endregion Search

        #region Mine
        public async Task<List<ProductVariantSM>> GetAllMine(long sellerId, int skip, int top)
        {
            // STEP 1 → Get paginated seller product ids
            var productIds = await _apiDbContext.Product
                .AsNoTracking()
                .Where(x => x.SellerId == sellerId)
                .OrderBy(x => x.Id)
                .Skip(skip)
                .Take(top)
                .Select(x => x.Id)
                .ToListAsync();

            if (!productIds.Any())
                return new List<ProductVariantSM>();

            // STEP 2 → Get variant ids for selected products
            var variantIds = await _apiDbContext.ProductVariant
                .AsNoTracking()
                .Where(x => productIds.Contains(x.ProductId))
                .OrderBy(x => x.ProductId)
                .ThenBy(x => x.Id)
                .Select(x => x.Id)
                .ToListAsync();
            if (variantIds.Count == 0)
            {
                return new List<ProductVariantSM>();
            }

            var result = new List<ProductVariantSM>();

            foreach (var id in variantIds)
            {
                var response = await GetProductVariantById(id);
                if (response != null)
                {
                    result.Add(response);
                }
            }

            return result;
        }
        public async Task<IntResponseRoot> GetAllMineCount(long sellerId)
        {
            var count = await _apiDbContext.ProductVariant
                .AsNoTracking()
                .Where(x => x.Product.SellerId == sellerId)
                .CountAsync();

            return new IntResponseRoot(count, "Total Product variants");
        }

        public async Task<List<ProductVariantSM>> GetAllMineVariantsByProductId(long sellerId,long productId, int skip, int top)
        {
            // STEP 1 → Get paginated seller product ids
            var product = await _apiDbContext.Product
                .AsNoTracking()
                .Where(x => x.SellerId == sellerId && x.Id == productId)
                .FirstOrDefaultAsync();

            if (product == null)
            {
                return new List<ProductVariantSM>();
            }

            var variantIds = await _apiDbContext.ProductVariant
                .AsNoTracking()
                .Where(x => x.ProductId == product.Id)
                .Skip(skip).Take(top)
                .Select(x => x.Id)
                .ToListAsync();
            if (variantIds.Count == 0)
            {
                return new List<ProductVariantSM>();
            }

            var result = new List<ProductVariantSM>();

            foreach (var id in variantIds)
            {
                var response = await GetProductVariantById(id);
                if (response != null)
                {
                    result.Add(response);
                }
            }

            return result;
        }
        public async Task<IntResponseRoot> GetAllMineVariantsByProductIdCount(long sellerId, long productId)
        {
            var product = await _apiDbContext.Product
               .AsNoTracking()
               .Where(x => x.SellerId == sellerId && x.Id == productId)
               .FirstOrDefaultAsync();
            var count = 0;
            if (product == null)
            {
                return new IntResponseRoot(count, "Total Product variants");
            }

            count = await _apiDbContext.ProductVariant
                .AsNoTracking()
                .Where(x => x.ProductId == product.Id)
                .Select(x => x.Id)
                .CountAsync();

            return new IntResponseRoot(count, "Total Product variants");
        }


        #endregion Mine

        #region HotBox/Speedy Mart Products in Catgeory

        #region Hot Box

        public async Task<UserHotBoxProductSM> GetHotBoxProductsById(long id)
        {           

            // STEP 1: Fetch products
            var product = await _apiDbContext.ProductVariant
                .Include((x=>x.Product))
                .AsNoTracking()
                .Where(x => x.PlatformType == PlatformTypeDM.HotBox && x.Status == ProductStatusDM.Active &&
                            x.Product.Id == id)
                .FirstOrDefaultAsync();


            // STEP 2: Fetch ratings in bulk
            var ratings = await _apiDbContext.ProductRating
                .Where(x => x.ProductVariantId == product.Id)
                .GroupBy(x => x.ProductVariantId)
                .Select(g => new
                {
                    ProductVariantId = g.Key,
                    AverageRating = g.Average(x => x.Rate),
                    TotalUsers = g.Count()
                })
                .ToDictionaryAsync(x => x.ProductVariantId);

            // STEP 3: Fetch nutrition in bulk
            var nutritions = await _apiDbContext.ProductNutritionData
                .Where(x => x.ProductVariantId == product.Id)
                .Select(x => new
                {
                    x.ProductVariantId,
                    x.ServeSize,
                    x.Proteins
                })
                .ToDictionaryAsync(x => x.ProductVariantId);

            var tags = await _apiDbContext.ProductTag
                .AsNoTracking()
                .Where(x => x.ProductVariantId == id)
                .Select(x => new ProductTagSM
                {
                    ProductVariantId = x.ProductVariantId,
                    TagId = x.TagId,
                    Name = x.Tag.Name
                })
                .ToListAsync();
            var orderCounts = await _apiDbContext.OrderItem
                .Where(x => x.ProductVariantId == id)
                .CountAsync();
            decimal discountedPrice = product.DiscountedPrice ?? product.Price;
            decimal discountPercentage = product.Price > 0
                    ? Math.Round(((product.Price - discountedPrice) / product.Price) * 100m, 2)
                    : 0m;
            ratings.TryGetValue(product.Id, out var ratingData);
            nutritions.TryGetValue(product.Id, out var nutritionData);
            var img = await _imageProcess.ResolveImage(product.Image);
            bool isFreshArrival = product.CreatedAt >= DateTime.UtcNow.AddDays(-3);
            bool isBestSeller = orderCounts > 10;
            return new UserHotBoxProductSM
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                ImageBase64 = img.Base64,
                NetworkImage = img.NetworkUrl,
                DiscountedPercentage = discountPercentage,
                DiscountedPrice = product.DiscountedPrice,
                Rate = (short)Math.Round(ratingData?.AverageRating ?? 0),
                TotalRatings = ratingData?.TotalUsers ?? 0,
                ProductTags = tags,
                Stock = product.Stock,
                IsCodAllowed = product.IsCodAllowed,
                TotalAllowedQuantity = product.TotalAllowedQuantity,
                Indicator = (ProductIndicatorSM)product.Indicator,
                ServeSize = nutritionData?.ServeSize,
                Proteins = nutritionData?.Proteins,
                Price = product.Price,
                IsBestSeller = isBestSeller,
                IsFreshArrival = isFreshArrival,
                CategoryId = product?.Product?.CategoryId,

            };
            

        }

        public async Task<List<UserHotBoxProductSM>> GetHotBoxProductsByCategoryId(long categoryId, int skip, int top)
        {
            var category = await _apiDbContext.Category.FindAsync(categoryId);

            if (category == null || category.Platform != PlatformTypeDM.HotBox)
            {
                if (category.Status == StatusDM.Inactive)
                {
                    throw new SiffrumException(
                    ApiErrorTypeSM.Fatal_Log,
                    $"User tried to access inactive category  for Id: {categoryId}",
                    "Category not found");
                }
                throw new SiffrumException(
                    ApiErrorTypeSM.Fatal_Log,
                    $"User tried to access category with different platform or category not found for Id: {categoryId}",
                    "Category details are missing");
            }

            // STEP 1: Fetch products
            var products = await _apiDbContext.ProductVariant
                .AsNoTracking()
                .Where(x => x.PlatformType == PlatformTypeDM.HotBox && x.Status == ProductStatusDM.Active &&
                            x.Product.CategoryId == categoryId)
                .OrderByDescending(x => x.ViewCount)
                .Skip(skip)
                .Take(top)
                .ToListAsync();

            if (!products.Any())
                return new List<UserHotBoxProductSM>();

            var productIds = products.Select(x => x.Id).ToList();
            return await GetHotBoxProductsByBanner(productIds);            
        }

        public async Task<List<HotBoxProductVariantSM>> GetHotBoxFullProductsByCategoryId(long categoryId, int skip, int top)
        {
            var category = await _apiDbContext.Category.FindAsync(categoryId);
            if (category == null || category.Platform != PlatformTypeDM.HotBox || category.Status == StatusDM.Inactive)
            {
                throw new SiffrumException(
                    ApiErrorTypeSM.Fatal_Log,
                    $"Category not found or inactive for Id: {categoryId}",
                    "Category not found");
            }

            // Group by product and pick the lowest-priced variant per product
            var variantIds = await _apiDbContext.ProductVariant
                .AsNoTracking()
                .Where(x => x.PlatformType == PlatformTypeDM.HotBox &&
                            x.Status == ProductStatusDM.Active &&
                            x.Product.CategoryId == categoryId)
                .GroupBy(x => x.ProductId)
                .Select(g => g.OrderBy(x => x.DiscountedPrice > 0 ? x.DiscountedPrice : x.Price).First().Id)
                .Skip(skip)
                .Take(top)
                .ToListAsync();

            var result = new List<HotBoxProductVariantSM>();
            foreach (var vid in variantIds)
            {
                var detail = await GetProductVariantByHotBoxId(vid);
                if (detail != null)
                    result.Add(detail);
            }
            return result;
        }

        public async Task<List<UserHotBoxProductSM>> GetHotBoxAssociatedProductsByVariantId(
    long productVariantId)
        {
            // STEP 1: Get variant
            var variant = await _apiDbContext.ProductVariant
                .FirstOrDefaultAsync(x => x.Id == productVariantId);

            if (variant == null)
            {
                throw new SiffrumException(
                    ApiErrorTypeSM.Fatal_Log,
                    $"Variant not found for Id: {productVariantId}",
                    "Product not found");
            }

            // STEP 2: Validate variant
            if (variant.Status == ProductStatusDM.Inactive)
            {
                throw new SiffrumException(
                    ApiErrorTypeSM.Fatal_Log,
                    $"User tried to access inactive variant for Id: {productVariantId}",
                    "Product not found");
            }

            if (variant.PlatformType != PlatformTypeDM.HotBox)
            {
                throw new SiffrumException(
                    ApiErrorTypeSM.Fatal_Log,
                    $"Variant is not HotBox type for Id: {productVariantId}",
                    "Product not found");
            }

            // STEP 3: Get base product
            var baseProduct = await _apiDbContext.Product
                .FirstOrDefaultAsync(x => x.Id == variant.ProductId);

            if (baseProduct == null)
            {
                throw new SiffrumException(
                    ApiErrorTypeSM.Fatal_Log,
                    $"No base product found for variant Id: {productVariantId}",
                    "Base product not found");
            }

            // STEP 4: Get all related variant IDs (excluding current)
            var variantIds = await _apiDbContext.ProductVariant
                .Where(x =>
                    x.ProductId == baseProduct.Id &&
                    x.Status == ProductStatusDM.Active &&
                    x.PlatformType == PlatformTypeDM.HotBox &&
                    x.Id != productVariantId)
                .Select(x => x.Id)
                .ToListAsync();

            // STEP 5: Fetch products
            return await GetHotBoxProductsByBanner(variantIds);
        }

        public async Task<IntResponseRoot> GetHotBoxProductsByCategoryIdCount(long categoryId)
        {
            var category = await _apiDbContext.Category.FindAsync(categoryId);

            if (category == null || category.Platform != PlatformTypeDM.HotBox)
            {
                if (category.Status == StatusDM.Inactive)
                {
                    throw new SiffrumException(
                    ApiErrorTypeSM.Fatal_Log,
                    $"User tried to access inactive category  for Id: {categoryId}",
                    "Category not found");
                }
                throw new SiffrumException(
                    ApiErrorTypeSM.Fatal_Log,
                    $"User tried to access category with different platform or category not found for Id: {categoryId}",
                    "Category details are missing");
            }

            // Count base products in the category
            var count = await _apiDbContext.Product
                .AsNoTracking()
                .Where(x => x.CategoryId == categoryId)
                .CountAsync();

            return new IntResponseRoot(count, "Total products in the category");
        }

        #endregion Hot Box

        #region Speedy Mart

        public async Task<List<UserSpeedyMartProductSM>> GetSpeedyMartProductsByCategoryId(long categoryId, int skip, int top)
        {
            var category = await _apiDbContext.Category.FindAsync(categoryId);

            if (category == null || category.Platform != PlatformTypeDM.SpeedyMart)
            {
                if (category.Status == StatusDM.Inactive)
                {
                    throw new SiffrumException(
                    ApiErrorTypeSM.Fatal_Log,
                    $"User tried to access inactive category  for Id: {categoryId}",
                    "Category not found");
                }
                throw new SiffrumException(
                    ApiErrorTypeSM.Fatal_Log,
                    $"User tried to access category with different platform or category not found for Id: {categoryId}",
                    "Category details are missing");
            }

            // STEP 1: Fetch products
            var products = await _apiDbContext.ProductVariant
                .AsNoTracking()
                .Where(x => x.PlatformType == PlatformTypeDM.SpeedyMart && x.Status == ProductStatusDM.Active &&
                            x.Product.CategoryId == categoryId)
                .OrderByDescending(x => x.ViewCount)
                .Skip(skip)
                .Take(top)
                .ToListAsync();

            if (!products.Any())
                return new List<UserSpeedyMartProductSM>();

            var productIds = products.Select(x => x.Id).ToList();
            return await GetSpeedyMartProductsByBanner(productIds);
            
        }


        public async Task<IntResponseRoot> GetSpeedyMartProductsByCategoryIdCount(long categoryId)
        {
            var category = await _apiDbContext.Category.FindAsync(categoryId);

            if (category == null || category.Platform != PlatformTypeDM.SpeedyMart)
            {
                if (category.Status == StatusDM.Inactive)
                {
                    throw new SiffrumException(
                    ApiErrorTypeSM.Fatal_Log,
                    $"User tried to access inactive category  for Id: {categoryId}",
                    "Category not found");
                }
                throw new SiffrumException(
                    ApiErrorTypeSM.Fatal_Log,
                    $"User tried to access category with different platform or category not found for Id: {categoryId}",
                    "Category details are missing");
            }

            // STEP 1: Fetch products
            var count = await _apiDbContext.ProductVariant
                .AsNoTracking()
                .Where(x => x.PlatformType == PlatformTypeDM.SpeedyMart && x.Status == ProductStatusDM.Active &&
                            x.Product.CategoryId == categoryId)
                .CountAsync();

            return new IntResponseRoot(count, "Total products in the category");
        }

        #endregion Speedy Mart

        #region Get HotBox/SpeedyMart Product By Ids

        public async Task<List<UserSpeedyMartProductSM>> GetSpeedyMartProductsByBanner(List<long> productIds)
        {
            var allVariants = await _apiDbContext.ProductVariant
                .Include(x=>x.Product)
                .AsNoTracking()
                .Where(x => productIds.Contains(x.Id))
                .OrderByDescending(x => x.ViewCount)
                .ToListAsync();

            if (!allVariants.Any())
                return new List<UserSpeedyMartProductSM>();

            // Group by parent ProductId, keep only the cheapest variant per product
            var products = allVariants
                .GroupBy(x => x.ProductId)
                .Select(g => g.OrderBy(x => x.Price).First())
                .ToList();
            productIds = products.Select(x => x.Id).ToList();

            var ratings = await _apiDbContext.ProductRating
                .Where(x => productIds.Contains(x.ProductVariantId))
                .GroupBy(x => x.ProductVariantId)
                .Select(g => new
                {
                    ProductVariantId = g.Key,
                    AverageRating = g.Average(x => x.Rate),
                    TotalUsers = g.Count()
                })
                .ToDictionaryAsync(x => x.ProductVariantId);
            var tagData = await _apiDbContext.ProductTag
    .AsNoTracking()
    .Where(x => productIds.Contains(x.ProductVariantId))
    .Select(x => new
    {
        x.ProductVariantId,
        x.TagId,
        TagName = x.Tag.Name
    })
    .ToListAsync();

            var tags = tagData
                .GroupBy(x => x.ProductVariantId)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(t => new ProductTagSM
                    {
                        ProductVariantId = t.ProductVariantId,
                        TagId = t.TagId,
                        Name = t.TagName
                    }).ToList()
                );
            var orderCounts = await _apiDbContext.OrderItem
                .Where(x => productIds.Contains(x.ProductVariantId))
                .GroupBy(x => x.ProductVariantId)
                .Select(g => new
                {
                    ProductVariantId = g.Key,
                    TotalOrders = g.Count()
                })
                .ToDictionaryAsync(x => x.ProductVariantId);
            var tasks = products.Select(async product =>
            {
                decimal discountedPrice = product.DiscountedPrice ?? product.Price;

                decimal discountPercentage = product.Price > 0
                    ? Math.Round(((product.Price - discountedPrice) / product.Price) * 100m, 2)
                    : 0m;

                ratings.TryGetValue(product.Id, out var ratingData);

                var img = await _imageProcess.ResolveImage(product.Image);
                orderCounts.TryGetValue(product.Id, out var orderData);
                tags.TryGetValue(product.Id, out var tagList);
                bool isFreshArrival = product.CreatedAt >= DateTime.UtcNow.AddDays(-3);
                bool isBestSeller = (orderData?.TotalOrders ?? 0) > 10;
                return new UserSpeedyMartProductSM
                {
                    Id = product.Id,
                    Name = product.Product?.Name ?? product.Name,
                    Price = product.Price,
                    Description = product.Description,
                    DiscountedPrice = product.DiscountedPrice,
                    ImageBase64 = img.Base64,
                    NetworkImage = img.NetworkUrl,
                    DiscountedPercentage = discountPercentage,
                    Rate = (short)Math.Round(ratingData?.AverageRating ?? 0),
                    ProductTags = tagList ?? new List<ProductTagSM>(),
                    TotalAllowedQuantity = product.TotalAllowedQuantity,
                    IsCodAllowed = product.IsCodAllowed,
                    Stock = product.Stock,
                    TotalRatings = ratingData?.TotalUsers ?? 0,
                    IsFreshArrival = isFreshArrival,
                    IsBestSeller = isBestSeller,
                    CategoryId = product?.Product?.CategoryId,
                };
            });

            return (await Task.WhenAll(tasks)).ToList();
        }

        public async Task<List<UserHotBoxProductSM>> GetHotBoxProductsByBanner(List<long> productIds)
        {
            var allVariants = await _apiDbContext.ProductVariant
               .Include(x => x.Product)
               .AsNoTracking()
               .Where(x => productIds.Contains(x.Id))
               .OrderByDescending(x => x.ViewCount)
               .ToListAsync();

            if (!allVariants.Any())
                return new List<UserHotBoxProductSM>();

            // Group by parent ProductId
            var grouped = allVariants.GroupBy(x => x.ProductId).ToList();

            // Keep only the cheapest variant per product as the "lead"
            var products = grouped
                .Select(g => g.OrderBy(x => x.Price).First())
                .ToList();
            productIds = products.Select(x => x.Id).ToList();

            // Build variant lookup per ProductId for nesting
            var variantsByProduct = grouped.ToDictionary(
                g => g.Key,
                g => g.OrderBy(x => x.Price).ToList());

            // STEP 2: Fetch ratings in bulk
            var ratings = await _apiDbContext.ProductRating
                .Where(x => productIds.Contains(x.ProductVariantId))
                .GroupBy(x => x.ProductVariantId)
                .Select(g => new
                {
                    ProductVariantId = g.Key,
                    AverageRating = g.Average(x => x.Rate),
                    TotalUsers = g.Count()
                })
                .ToDictionaryAsync(x => x.ProductVariantId);

            // STEP 3: Fetch nutrition in bulk
            var nutritions = await _apiDbContext.ProductNutritionData
                .Where(x => productIds.Contains(x.ProductVariantId))
                .Select(x => new
                {
                    x.ProductVariantId,
                    x.ServeSize,
                    x.Proteins
                })
                .ToDictionaryAsync(x => x.ProductVariantId);
            var tagData = await _apiDbContext.ProductTag
    .AsNoTracking()
    .Where(x => productIds.Contains(x.ProductVariantId))
    .Select(x => new
    {
        x.ProductVariantId,
        x.TagId,
        TagName = x.Tag.Name
    })
    .ToListAsync();

            var tags = tagData
                .GroupBy(x => x.ProductVariantId)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(t => new ProductTagSM
                    {
                        ProductVariantId = t.ProductVariantId,
                        TagId = t.TagId,
                        Name = t.TagName
                    }).ToList()
                );
            var orderCounts = await _apiDbContext.OrderItem
                .Where(x => productIds.Contains(x.ProductVariantId))
                .GroupBy(x => x.ProductVariantId)
                .Select(g => new
                {
                    ProductVariantId = g.Key,
                    TotalOrders = g.Count()
                })
                .ToDictionaryAsync(x => x.ProductVariantId);

            // STEP 4: Map response
            var tasks = products.Select(async product =>
            {
                decimal discountedPrice = product.DiscountedPrice ?? product.Price;

                decimal discountPercentage = product.Price > 0
                    ? Math.Round(((product.Price - discountedPrice) / product.Price) * 100m, 2)
                    : 0m;

                ratings.TryGetValue(product.Id, out var ratingData);
                nutritions.TryGetValue(product.Id, out var nutritionData);

                var img = await _imageProcess.ResolveImage(product.Image);
                orderCounts.TryGetValue(product.Id, out var orderData);
                tags.TryGetValue(product.Id, out var tagList);
                bool isFreshArrival = product.CreatedAt >= DateTime.UtcNow.AddDays(-3);
                bool isBestSeller = (orderData?.TotalOrders ?? 0) > 10;

                // Build nested variants list
                var variantList = new List<VariantInfoSM>();
                if (variantsByProduct.TryGetValue(product.ProductId, out var siblings))
                {
                    foreach (var v in siblings)
                    {
                        var vImg = await _imageProcess.ResolveImage(v.Image);
                        variantList.Add(new VariantInfoSM
                        {
                            Id = v.Id,
                            Name = v.Name,
                            Price = v.Price,
                            DiscountedPrice = v.DiscountedPrice,
                            Stock = v.Stock,
                            ImageBase64 = vImg.Base64,
                            NetworkImage = vImg.NetworkUrl
                        });
                    }
                }

                return new UserHotBoxProductSM
                {
                    Id = product.Id,
                    Name = product.Product?.Name ?? product.Name,
                    ImageBase64 = img.Base64,
                    NetworkImage = img.NetworkUrl,
                    Description = product.Description,
                    DiscountedPercentage = discountPercentage,
                    DiscountedPrice = product.DiscountedPrice,
                    Rate = (short)Math.Round(ratingData?.AverageRating ?? 0),
                    TotalRatings = ratingData?.TotalUsers ?? 0,
                    Indicator = (ProductIndicatorSM)product.Indicator,
                    ServeSize = nutritionData?.ServeSize,
                    Proteins = nutritionData?.Proteins,
                    ProductTags = tagList ?? new List<ProductTagSM>(),
                    TotalAllowedQuantity = product.TotalAllowedQuantity,
                    Stock = product.Stock,
                    Price = product.Price,
                    IsCodAllowed = product.IsCodAllowed,
                    IsBestSeller = isBestSeller,
                    IsFreshArrival = isFreshArrival,
                    CategoryId = product?.Product?.CategoryId,
                    Variants = variantList,
                };
            });

            return (await Task.WhenAll(tasks)).ToList();
        }

        #region Filter
        public async Task<List<UserHotBoxProductSM>> GetHotBoxProductsByIndicator(ProductIndicatorSM indicator, int skip, int top)
        {
            var product = await _apiDbContext.ProductVariant
               .AsNoTracking()
               .Where(x => x.Indicator == (ProductIndicatorDM)indicator && x.PlatformType == PlatformTypeDM.HotBox && x.Status == ProductStatusDM.Active)
               .OrderByDescending(x => x.ViewCount)
               .Skip(skip).Take(top)
               .ToListAsync();
            var productIds = product.Select(x => x.Id).ToList();
            if (!product.Any())
                return new List<UserHotBoxProductSM>();
            return await GetHotBoxProductsByBanner(productIds);
            
        }

        public async Task<IntResponseRoot> GetHotBoxProductsByIndicatorCount(ProductIndicatorSM indicator)
        {
            var count = await _apiDbContext.ProductVariant
               .AsNoTracking()
               .Where(x => x.Indicator == (ProductIndicatorDM)indicator && x.PlatformType == PlatformTypeDM.HotBox && x.Status == ProductStatusDM.Active)               
               .CountAsync();
            return new IntResponseRoot(count, "Total Products");
        }

        public async Task<List<UserHotBoxProductSM>> GetHotBoxProductsByHighestProtien(int skip, int top)
        {
            var product = await _apiDbContext.ProductVariant
                .AsNoTracking()
                .Where(x => x.PlatformType == PlatformTypeDM.HotBox && x.Status == ProductStatusDM.Active)
                .OrderByDescending(x => x.NutritionValues.Max(n => n.Proteins))
                .Skip(skip)
                .Take(top)
                .Include(x => x.NutritionValues)
                .ToListAsync();
            var productIds = product.Select(x => x.Id).ToList();
            if (!product.Any())
                return new List<UserHotBoxProductSM>();
            return await GetHotBoxProductsByBanner(productIds);
            
        }

        public async Task<IntResponseRoot> GetHotBoxProductsByHighestProtienCount()
        {
            var count = await _apiDbContext.ProductVariant
                .AsNoTracking()
                .Where(x => x.PlatformType == PlatformTypeDM.HotBox
                            && x.Status == ProductStatusDM.Active)
                .OrderByDescending(x => x.NutritionValues.Max(n => n.Proteins))
                .CountAsync();

            return new IntResponseRoot(count, "Total Products");
        }


        public async Task<List<UserHotBoxProductSM>> GetHotBoxProductsBySearchString(ProductIndicatorSM indicator, string searchString, int skip, int top)
        {
            
            if (string.IsNullOrWhiteSpace(searchString))
                return new List<UserHotBoxProductSM>();

            var words = searchString
                .Trim()
                .Split(' ', StringSplitOptions.RemoveEmptyEntries);
            
            IQueryable<ProductVariantDM> query = _apiDbContext.ProductVariant.Where(x=>x.PlatformType == PlatformTypeDM.HotBox && x.Status == ProductStatusDM.Active)
                .AsNoTracking();

            if(indicator != ProductIndicatorSM.None)
            {
                query = query.Where(x => x.Indicator == (ProductIndicatorDM)indicator);
            }
            // Multi-word search using LIKE (Better performance)
            foreach (var word in words)
            {
                query = query.Where(x => x.Name.ToLower().Contains(word.ToLower()));
            }
            var product = await query 
                .Skip(skip)
                .Take(top)
                .ToListAsync();
            var productIds = product.Select(x => x.Id).ToList();
            if (!product.Any())
                return new List<UserHotBoxProductSM>();
            return await GetHotBoxProductsByBanner(productIds);
            
        }

        public async Task<HotBoxSearchResponseSM> GetHotBoxProductsBySearchStringWithCombos(
            ProductIndicatorSM indicator, string searchString, int skip, int top, long userId = 0)
        {
            if (string.IsNullOrWhiteSpace(searchString))
                return new HotBoxSearchResponseSM();

            var words = searchString
                .Trim()
                .Split(' ', StringSplitOptions.RemoveEmptyEntries);

            IQueryable<ProductVariantDM> query = _apiDbContext.ProductVariant
                .Include(x => x.Product)
                .Where(x => x.PlatformType == PlatformTypeDM.HotBox && x.Status == ProductStatusDM.Active)
                .AsNoTracking();

            // Filter by user's assigned seller
            if (userId > 0)
            {
                var assignedSellerId = await _apiDbContext.User
                    .AsNoTracking()
                    .Where(u => u.Id == userId)
                    .Select(u => u.AssignedSellerId)
                    .FirstOrDefaultAsync();
                if (assignedSellerId.HasValue && assignedSellerId.Value > 0)
                {
                    query = query.Where(x => x.Product.SellerId == assignedSellerId.Value);
                }
            }

            if (indicator != ProductIndicatorSM.None)
            {
                query = query.Where(x => x.Indicator == (ProductIndicatorDM)indicator);
            }

            // Smart search: match each word across product name, variant name, or description
            foreach (var word in words)
            {
                var w = word.ToLower();
                query = query.Where(x =>
                    x.Name.ToLower().Contains(w) ||
                    (x.Product != null && x.Product.Name.ToLower().Contains(w)) ||
                    (x.Description != null && x.Description.ToLower().Contains(w)));
            }

            var products = await query
                .OrderByDescending(x => x.ViewCount)
                .Skip(skip)
                .Take(top)
                .ToListAsync();

            if (!products.Any())
                return new HotBoxSearchResponseSM();

            var productIds = products.Select(x => x.Id).ToList();

            // Fetch combos
            var combos = await _comboProcess.GetComboByProductIds(productIds, 2);
            var productSms = await GetHotBoxProductsByBanner(productIds);
            
            return new HotBoxSearchResponseSM
            {
                Products = productSms,
                Combos = combos
            };
        }

        public async Task<IntResponseRoot> GetHotBoxProductsBySearchStringCount(ProductIndicatorSM indicator, string searchString, long userId = 0)
        {
            if (string.IsNullOrWhiteSpace(searchString))
                return new IntResponseRoot(0, "Total Products");

            var words = searchString
                .Trim()
                .Split(' ', StringSplitOptions.RemoveEmptyEntries);

            IQueryable<ProductVariantDM> query = _apiDbContext.ProductVariant
                .Include(x => x.Product)
                .Where(x => x.PlatformType == PlatformTypeDM.HotBox && x.Status == ProductStatusDM.Active)
                .AsNoTracking();

            // Filter by user's assigned seller
            if (userId > 0)
            {
                var assignedSellerId = await _apiDbContext.User
                    .AsNoTracking()
                    .Where(u => u.Id == userId)
                    .Select(u => u.AssignedSellerId)
                    .FirstOrDefaultAsync();
                if (assignedSellerId.HasValue && assignedSellerId.Value > 0)
                {
                    query = query.Where(x => x.Product.SellerId == assignedSellerId.Value);
                }
            }

            if (indicator != ProductIndicatorSM.None)
            {
                query = query.Where(x => x.Indicator == (ProductIndicatorDM)indicator);
            }

            // Smart search across product name, variant name, description
            foreach (var word in words)
            {
                var w = word.ToLower();
                query = query.Where(x =>
                    x.Name.ToLower().Contains(w) ||
                    (x.Product != null && x.Product.Name.ToLower().Contains(w)) ||
                    (x.Description != null && x.Description.ToLower().Contains(w)));
            }

            var count = await query.CountAsync();
            return new IntResponseRoot(count, "Total Products");
        }

        public async Task<List<UserSpeedyMartProductSM>> GetSpeedyMartProductsBySearch(string searchString, int skip, int top)
        {
            if (string.IsNullOrWhiteSpace(searchString))
                return new List<UserSpeedyMartProductSM>();

            var words = searchString
                .Trim()
                .Split(' ', StringSplitOptions.RemoveEmptyEntries);

            IQueryable<ProductVariantDM> query = _apiDbContext.ProductVariant.Where(x => x.PlatformType == PlatformTypeDM.SpeedyMart && x.Status == ProductStatusDM.Active)
                .AsNoTracking();


            // Multi-word search using LIKE (Better performance)
            foreach (var word in words)
            {
                query = query.Where(x => x.Name.ToLower().Contains(word.ToLower()));
            }

            var products = await query
                .ToListAsync();
            var productIds = products.Select(x => x.Id).ToList();
            return await GetSpeedyMartProductsByBanner(productIds);            
        }

        public async Task<IntResponseRoot> GetSpeedyMartProductsBySearchCount(string searchString)
        {
            if (string.IsNullOrWhiteSpace(searchString))
                return new IntResponseRoot(0, "Total Products");

            var words = searchString
                .Trim()
                .Split(' ', StringSplitOptions.RemoveEmptyEntries);

            IQueryable<ProductVariantDM> query = _apiDbContext.ProductVariant.Where(x => x.PlatformType == PlatformTypeDM.SpeedyMart && x.Status == ProductStatusDM.Active)
                .AsNoTracking();


            // Multi-word search using LIKE (Better performance)
            foreach (var word in words)
            {
                query = query.Where(x => x.Name.ToLower().Contains(word.ToLower()));
            }

            var count = await query
                .CountAsync();
            return new IntResponseRoot(count, "Total Products");
        }

        #region Products By Most orders and Products by Latest added

        #region HotBox
        private async Task<long?> GetAssignedSellerIdForCurrentUser()
        {
            var userId = _loginUserDetail.DbRecordId;
            if (userId <= 0 || _loginUserDetail.UserType != RoleTypeSM.User)
                return null;
            return await _apiDbContext.User
                .AsNoTracking()
                .Where(u => u.Id == userId)
                .Select(u => u.AssignedSellerId)
                .FirstOrDefaultAsync();
        }

        public async Task<List<HotBoxProductVariantSM>> GetHotBoxMostOrderedProducts(int skip, int top)
        {
            var assignedSellerId = await GetAssignedSellerIdForCurrentUser();

            // Group by base product to get most ordered products (not variants)
            var orderQuery = _apiDbContext.OrderItem
                .Where(o => o.ProductVariant.PlatformType == PlatformTypeDM.HotBox);
            if (assignedSellerId.HasValue && assignedSellerId.Value > 0)
                orderQuery = orderQuery.Where(o => o.ProductVariant.Product.SellerId == assignedSellerId.Value);

            var topProductIds = await orderQuery
                .GroupBy(o => o.ProductVariant.ProductId)
                .Select(g => new
                {
                    ProductId = g.Key,
                    TotalOrders = g.Count()
                })
                .OrderByDescending(x => x.TotalOrders)
                .Skip(skip)
                .Take(top)
                .Select(x => x.ProductId)
                .ToListAsync();
            if(topProductIds.Count == 0)
            {
                return new List<HotBoxProductVariantSM>();
            }
            // Pick the lowest-priced active variant per product
            var variantIds = await _apiDbContext.ProductVariant
                .AsNoTracking()
                .Where(v => topProductIds.Contains(v.ProductId) &&
                            v.Status == ProductStatusDM.Active &&
                            v.PlatformType == PlatformTypeDM.HotBox)
                .GroupBy(v => v.ProductId)
                .Select(g => g.OrderBy(v => v.DiscountedPrice > 0 ? v.DiscountedPrice : v.Price).First().Id)
                .ToListAsync();
            var result = new List<HotBoxProductVariantSM>();
            foreach (var vid in variantIds)
            {
                var detail = await GetProductVariantByHotBoxId(vid);
                if (detail != null) result.Add(detail);
            }
            return result;
        }

        public async Task<IntResponseRoot> GetHotBoxMostOrderedProductsCount()
        {
            var assignedSellerId = await GetAssignedSellerIdForCurrentUser();

            var orderQuery = _apiDbContext.OrderItem
                .Where(o => o.ProductVariant.PlatformType == PlatformTypeDM.HotBox);
            if (assignedSellerId.HasValue && assignedSellerId.Value > 0)
                orderQuery = orderQuery.Where(o => o.ProductVariant.Product.SellerId == assignedSellerId.Value);

            var count = await orderQuery
                .GroupBy(o => o.ProductVariant.ProductId)
                .CountAsync();
            return new IntResponseRoot(count, "Total Products");
        }

        public async Task<List<HotBoxProductVariantSM>> GetHotBoxLatestProducts(int skip, int top)
        {
            var assignedSellerId = await GetAssignedSellerIdForCurrentUser();

            var query = _apiDbContext.ProductVariant
                .Where(p =>
                    p.PlatformType == PlatformTypeDM.HotBox &&
                    p.Status == ProductStatusDM.Active);
            if (assignedSellerId.HasValue && assignedSellerId.Value > 0)
                query = query.Where(p => p.Product.SellerId == assignedSellerId.Value);

            var variantIds = await query
                .GroupBy(p => p.ProductId)
                .Select(g => new
                {
                    VariantId = g.OrderBy(v => v.DiscountedPrice > 0 ? v.DiscountedPrice : v.Price).First().Id,
                    LatestCreated = g.Max(v => v.CreatedAt)
                })
                .OrderByDescending(x => x.LatestCreated)
                .Skip(skip)
                .Take(top > 20 ? 20 : top)
                .Select(x => x.VariantId)
                .ToListAsync();
            if (variantIds.Count == 0)
            {
                return new List<HotBoxProductVariantSM>();
            }
            var result = new List<HotBoxProductVariantSM>();
            foreach (var vid in variantIds)
            {
                var detail = await GetProductVariantByHotBoxId(vid);
                if (detail != null) result.Add(detail);
            }
            return result;
        }

        public async Task<IntResponseRoot> GetHotBoxLatestProductsCount()
        {
            var assignedSellerId = await GetAssignedSellerIdForCurrentUser();

            var query = _apiDbContext.ProductVariant
                .Where(p =>
                    p.PlatformType == PlatformTypeDM.HotBox &&
                    p.Status == ProductStatusDM.Active);
            if (assignedSellerId.HasValue && assignedSellerId.Value > 0)
                query = query.Where(p => p.Product.SellerId == assignedSellerId.Value);

            var count = await query
                .GroupBy(p => p.ProductId)
                .CountAsync();
            count = count > 20 ? 20 : count;
            return new IntResponseRoot(count, "Total Products");
        }

        #endregion HotBox

        #region Speedy Mart

        public async Task<List<UserSpeedyMartProductSM>> GetSpeedyMartMostOrderedProducts(int skip, int top)
        {
            var productIds = await _apiDbContext.OrderItem
                .Where(o => o.ProductVariant.PlatformType == PlatformTypeDM.SpeedyMart)
                .GroupBy(o => o.ProductVariantId)
                .Select(g => new
                {
                    ProductVariantId = g.Key,
                    TotalOrders = g.Count()
                })
                .OrderByDescending(x => x.TotalOrders)
                .Skip(skip)
                .Take(top)
                .Select(x => x.ProductVariantId)
                .ToListAsync();
            if (productIds.Count == 0)
            {
                return new List<UserSpeedyMartProductSM>();
            }
            var products = await GetSpeedyMartProductsByBanner(productIds);
            return products;
        }

        public async Task<IntResponseRoot> GetSpeedyMartMostOrderedProductsCount()
        {
            var count = await _apiDbContext.OrderItem
                .Where(o => o.ProductVariant.PlatformType == PlatformTypeDM.SpeedyMart)
                .GroupBy(o => o.ProductVariantId)
                .Select(g => new
                {
                    ProductVariantId = g.Key,
                    TotalOrders = g.Count()
                })
                .OrderByDescending(x => x.TotalOrders)
                .Select(x => x.ProductVariantId)
                .CountAsync();
            return new IntResponseRoot(count, "Total Products");
        }

        public async Task<List<UserSpeedyMartProductSM>> GetSpeedyMartLatestProducts(int skip, int top)
        {
            var threeDaysAgo = DateTime.UtcNow.AddDays(-3);

            var productIds = await _apiDbContext.ProductVariant
                .Where(p =>
                    p.PlatformType == PlatformTypeDM.SpeedyMart &&
                    p.Status == ProductStatusDM.Active &&
                    p.CreatedAt >= threeDaysAgo)
                .OrderByDescending(p => p.CreatedAt)
                .Skip(skip)
                .Take(top)
                .Select(p => p.Id)
                .ToListAsync();
            if (productIds.Count == 0)
            {
                return new List<UserSpeedyMartProductSM>();
            }
            var products = await GetSpeedyMartProductsByBanner(productIds);
            return products;
        }

        public async Task<IntResponseRoot> GetSpeedyMartLatestProductsCount()
        {
            var threeDaysAgo = DateTime.UtcNow.AddDays(-3);

            var count = await _apiDbContext.ProductVariant
                .Where(p =>
                    p.PlatformType == PlatformTypeDM.SpeedyMart &&
                    p.Status == ProductStatusDM.Active &&
                    p.CreatedAt >= threeDaysAgo)
                .OrderByDescending(p => p.CreatedAt)
                .Select(p => p.Id)
                .CountAsync();
            return new IntResponseRoot(count, "Total Products");
        }

        #endregion Speedy Mart

        #endregion Products By Most orders and Products by Latest added

        #endregion Filter

        #endregion Get HotBox/SpeedyMart Product By Ids

        #endregion HotBox/Speedy Mart Products in Catgeory

        #endregion  Get All and Counts

        #region Get By Id

        public async Task<ProductVariantSM> GetProductVariantById(long id)
        {
            var dm = await _apiDbContext.ProductVariant
                .Include(x=>x.Product)
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id);
            if(dm == null)
            {
                return null;
            }
            
            var sm = _mapper.Map<ProductVariantSM>(dm);
            sm.PlatformType = (PlatformTypeSM)dm.PlatformType;
            sm.Indicator = (ProductIndicatorSM)dm.Indicator;
            sm.CategoryId = dm?.Product?.CategoryId;
            sm.SellerId = dm?.Product?.SellerId ?? 0;
            if (!string.IsNullOrEmpty(dm.Image))
            {
                var img = await _imageProcess.ResolveImage(dm.Image);
                sm.ImageBase64 = img.Base64;
                sm.NetworkImage = img.NetworkUrl;
            }
            return sm;
        }

        public async Task<HotBoxProductVariantSM> GetProductVariantByHotBoxId(long id)
        {
            var response = new HotBoxProductVariantSM();
            var dm = await _apiDbContext.ProductVariant
                .Include(x => x.Product)
                .Where(x=>x.PlatformType == PlatformTypeDM.HotBox)
                .FirstOrDefaultAsync(x => x.Id == id);
            if(dm == null)
            {
                return null;
            }
            dm.ViewCount += 1;
            await _apiDbContext.SaveChangesAsync();
            response.ProductVariant = await GetProductVariantById(dm.Id);
            response.ProductVariant.Name = dm.Product?.Name ?? dm.Name;
            response.Tags = dm.Product?.Tags;

            var additionalInfo = new HotBoxProductAdditionalInfoSM();
            var images = await _productImages.GetProductImages(id);
            var tag = await GetTagsByProductVariantId(id);
            var unit = await _productUnit.GetByProductVariantId(id);
            var nutrition = await _productNutrition.GetByVariantIdAsync(id);
            var rating = await _productRating.GetAllProductRatings(id,0,5);
            var faqs = await _productFaq.GetAllProductFaqsByProductVariantId(id, 0, 5);
            additionalInfo.ProductTags = tag;
            additionalInfo.ProductUnit = unit;
            additionalInfo.Nutrition = nutrition;
            additionalInfo.ProductRatings = rating;
            additionalInfo.ProductFaqs = faqs;
            additionalInfo.Images = images;
            additionalInfo.Toppings = await _productToppingProcess.GetByProductId(dm.ProductId);
            additionalInfo.Addons = await GetAddonResponseInline(id);
            response.ProductAdditionalInfo = additionalInfo;
            response.AllVariants = await GetAllVariantsForProduct(dm.ProductId);
            return response;
        }

        /*public async Task<HotBoxProductVariantSM> GetProductVariantByHotBoxId2(long id)
        {
            var dm = await _apiDbContext.ProductVariant
                .Where(x => x.PlatformType == PlatformTypeDM.HotBox && x.Id == id)
                .FirstOrDefaultAsync();

            if (dm == null)
            {
                return null;
            }

            dm.ViewCount++;
            await _apiDbContext.SaveChangesAsync();

            var response = new HotBoxProductVariantSM
            {
                ProductVariant = await GetProductVariantById(dm.Id)
            };

            var additionalInfo = new HotBoxProductAdditionalInfoSM();

            var imagesTask = _productImages.GetProductImages(id);
            var tagsTask = GetTagsByProductVariantId(id);
            var unitTask = _productUnit.GetByProductVariantId(id);
            var nutritionTask = _productNutrition.GetByVariantIdAsync(id);
            var ratingTask = _productRating.GetAllProductRatings(id, 0, 5);
            var faqTask = _productFaq.GetAllProductFaqsByProductVariantId(id, 0, 5);

            await Task.WhenAll(imagesTask, tagsTask, unitTask, nutritionTask, ratingTask, faqTask);

            additionalInfo.Images = imagesTask.Result;
            additionalInfo.ProductTags = tagsTask.Result;
            additionalInfo.ProductUnit = unitTask.Result;
            additionalInfo.Nutrition = nutritionTask.Result;
            additionalInfo.ProductRatings = ratingTask.Result;
            additionalInfo.ProductFaqs = faqTask.Result;

            response.ProductAdditionalInfo = additionalInfo;

            return response;
        }*/
        public async Task<SpeedyMartProductVariantSM> GetProductVariantBySpeedyKartId(long id)
        {
            var response = new SpeedyMartProductVariantSM();
            var dm = await _apiDbContext.ProductVariant
                .Include(x => x.Product)
                .Where(x => x.PlatformType == PlatformTypeDM.SpeedyMart)
                .FirstOrDefaultAsync(x => x.Id == id);
            if (dm == null)
            {
                return null;
            }
            dm.ViewCount += 1;
            await _apiDbContext.SaveChangesAsync();
            response.ProductVariant = await GetProductVariantById(dm.Id);
            response.ProductVariant.Name = dm.Product?.Name ?? dm.Name;
            response.Tags = dm.Product?.Tags;

            var additionalInfo = new SpeedyMartProductAdditionalInfoSM();
            var images = await _productImages.GetProductImages(id);
            var tag = await GetTagsByProductVariantId(id);
            var rating = await _productRating.GetAllProductRatings(id, 0, 5);
            var faqs = await _productFaq.GetAllProductFaqsByProductVariantId(id, 0, 5);
            var specification = await _productSpecification.GetByVariantIdAsync(id);
            var specificationFilters = await _productSpecificationFilter.GetProductSpecificationFiltersAsync(id);
            additionalInfo.ProductTags = tag;
            additionalInfo.ProductRatings = rating;
            additionalInfo.ProductFaqs = faqs;
            additionalInfo.Specifications = specification;
            additionalInfo.Filters = specificationFilters;
            additionalInfo.Images = images;
            additionalInfo.Toppings = await _productToppingProcess.GetByProductId(dm.ProductId);
            response.ProductAdditionalInfo = additionalInfo;
            response.AllVariants = await GetAllVariantsForProduct(dm.ProductId);
            return response;
        }

        private async Task<List<VariantInfoSM>> GetAllVariantsForProduct(long productId)
        {
            var siblings = await _apiDbContext.ProductVariant
                .AsNoTracking()
                .Where(x => x.ProductId == productId && x.Status == ProductStatusDM.Active)
                .OrderBy(x => x.Price)
                .ToListAsync();

            var siblingIds = siblings.Select(x => x.Id).ToList();
            var unitNames = await _apiDbContext.ProductUnit
                .AsNoTracking()
                .Where(x => siblingIds.Contains(x.ProductVariantId))
                .Join(_apiDbContext.Unit, pu => pu.UnitId, u => u.Id, (pu, u) => new { pu.ProductVariantId, UnitName = u.Name })
                .ToDictionaryAsync(x => x.ProductVariantId, x => x.UnitName);

            var result = new List<VariantInfoSM>();
            foreach (var v in siblings)
            {
                var vImg = await _imageProcess.ResolveImage(v.Image);
                unitNames.TryGetValue(v.Id, out var uName);
                result.Add(new VariantInfoSM
                {
                    Id = v.Id,
                    Name = v.Name,
                    Price = v.Price,
                    DiscountedPrice = v.DiscountedPrice,
                    Stock = v.Stock,
                    UnitName = uName,
                    ImageBase64 = vImg.Base64,
                    NetworkImage = vImg.NetworkUrl,
                });
            }
            return result;
        }

        private async Task<AddonProductResponseSM> GetAddonResponseInline(long mainVariantId)
        {
            var data = await _apiDbContext.AddonProducts
                .AsNoTracking()
                .Where(x => x.MainProductId == mainVariantId)
                .Select(x => new
                {
                    x.Id,
                    x.AddonProductId,
                    ProductName = x.AddOnProduct.Name,
                    ProductImage = x.AddOnProduct.Image,
                    Price = x.AddOnProduct.DiscountedPrice ?? x.AddOnProduct.Price,
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
                .Include(x => x.Product)
                .AsNoTracking()
                .Where(x => x.Id == mainVariantId)
                .Select(x => new
                {
                    x.Id,
                    x.Name,
                    x.Image,
                    x.TotalAllowedQuantity,
                    x.Stock,
                    Price = x.DiscountedPrice ?? x.Price,
                    IsCodAllowed = x.IsCodAllowed,
                    CategoryId = x.Product.CategoryId,
                })
                .FirstOrDefaultAsync();

            var response = new AddonProductResponseSM
            {
                MainProductId = mainProduct.Id,
                Name = mainProduct.Name,
                Price = mainProduct.Price,
                AllowedQuantity = (int)mainProduct.TotalAllowedQuantity,
                Stock = (int)mainProduct.Stock,
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

        #endregion Get By Id

        #region Add

        public async Task<ProductVariantSM> AddProduct(long sellerId, ProductVariantSM request)
        {
            if (request == null)
            {
                throw new SiffrumException(ApiErrorTypeSM.ModelError_NoLog, "Product data is required");
            }
            if (string.IsNullOrEmpty(request.Name))
            {
                throw new SiffrumException(ApiErrorTypeSM.ModelError_NoLog, "Product name is required");
            }
            var existingBaseProduct = await _apiDbContext.Product.Where(x=>x.Id== request.ProductId && x.SellerId == sellerId).FirstOrDefaultAsync();
            if(existingBaseProduct == null)
            {
                throw new SiffrumException(ApiErrorTypeSM.InvalidInputData_NoLog, "Base Product not found for given Product Id");
            }
            var existingProduct = await GetSellerProductVariantByName(sellerId, request.Name);
            if (existingProduct != null)
            {
                return existingProduct;
            }
            
            var dm = _mapper.Map<ProductVariantDM>(request);
            if (string.IsNullOrEmpty(request.ImageBase64))
            {
                throw new SiffrumException(ApiErrorTypeSM.ModelError_NoLog, "Product image is required");
            }
            else
            {
                var imageBase64 = await _imageProcess.SaveFromBase64(request.ImageBase64, "jpg", "wwwroot/content/products");
                if (!string.IsNullOrEmpty(imageBase64))
                {
                    dm.Image = imageBase64;
                }
                else
                {
                    dm.Image = null;
                }

            }
            dm.Status = ProductStatusDM.PendingApproval;
            _apiDbContext.ProductVariant.Add(dm);
            if (await _apiDbContext.SaveChangesAsync() > 0)
            {
                return await GetProductVariantById(dm.Id);
            }

            throw new SiffrumException(ApiErrorTypeSM.Fatal_Log,
               $"Error in adding product by Seller with SellerId:{sellerId}"
               , "Something went wrong while adding product details. Please try again.");
        }


        #endregion Add

        #region Assign Product to Seller

        public async Task<ProductVariantSM> AssignProductToSeller(SellerProductAssociationsSM objSM)
        {
            // STEP 1: Fetch Global Variant with Product
            var globalVariant = await _apiDbContext.ProductVariant
                .Include(x => x.Product)
                .FirstOrDefaultAsync(x => x.Id == objSM.ProductVariantId);

            if (globalVariant == null)
                throw new SiffrumException(ApiErrorTypeSM.InvalidInputData_Log, "Product variant not found");
            var seller = await _apiDbContext.Seller.FindAsync(objSM.SellerId);
            if(seller == null)
            {
                throw new SiffrumException(ApiErrorTypeSM.InvalidInputData_Log, "Seller details not found");
            }
            var globalProduct = globalVariant.Product;

            if (globalProduct == null)
                throw new SiffrumException(ApiErrorTypeSM.InvalidInputData_Log, "Base product not found");

            // Prevent assigning to same seller
            if (globalProduct.SellerId == objSM.SellerId)
                throw new SiffrumException(ApiErrorTypeSM.InvalidInputData_Log, "Product already belongs to this seller");
           
            // Normalize name
            var normalizedProductName = globalProduct.Name.ToLower().Trim();
            var normalizedVariantName = globalVariant.Name.ToLower().Trim();

            // 🔹 Check only variant duplication for same seller + same product
            var variantExists = await _apiDbContext.ProductVariant
                .Include(x => x.Product)
                .AnyAsync(x =>
                    x.Product.SellerId == objSM.SellerId &&
                    x.Product.Name.ToLower().Trim() == normalizedProductName &&
                    x.Name.ToLower().Trim() == normalizedVariantName);

            if (variantExists)
            {
                throw new SiffrumException(ApiErrorTypeSM.InvalidInputData_Log,
                    "This product variant is already assigned to this seller");
            }

            // STEP 2: Check if Seller already has Product
            var sellerProduct = await _apiDbContext.Product
                .FirstOrDefaultAsync(x =>
                    x.SellerId == objSM.SellerId &&
                    x.Name.ToLower().Trim() == normalizedProductName);

            // STEP 3: Clone Product if NOT exists
            if (sellerProduct == null)
            {
                sellerProduct = new ProductDM
                {
                    Name = globalProduct.Name,
                    Slug = GenerateSlug(globalProduct.Name, objSM.SellerId),
                    SellerId = objSM.SellerId,
                    CategoryId = globalProduct.CategoryId,
                    BrandId = globalProduct.BrandId,
                    TaxPercentage = globalProduct.TaxPercentage
                };

                await _apiDbContext.Product.AddAsync(sellerProduct);
                await _apiDbContext.SaveChangesAsync();
            }

            // STEP 4: Check if Variant already exists
            var existingVariant = await _apiDbContext.ProductVariant
                .FirstOrDefaultAsync(x =>
                    x.ProductId == sellerProduct.Id &&
                    x.Name.ToLower().Trim() == normalizedVariantName);

            if (existingVariant != null)
            {
                return await GetProductVariantById(existingVariant.Id);
            }

            // STEP 5: Clone Variant (NEW OBJECT - no tracking issues)
            var newVariant = new ProductVariantDM
            {
                Name = globalVariant.Name,
                Indicator = globalVariant.Indicator,
                Manufacturer = globalVariant.Manufacturer,
                MadeIn = globalVariant.MadeIn,
                IsCancelable = globalVariant.IsCancelable,
                Image = globalVariant.Image,
                Description = globalVariant.Description,
                Status = ProductStatusDM.PendingApproval,
                PlatformType = globalVariant.PlatformType,
                ReturnDays = globalVariant.ReturnDays,
                IsUnlimitedStock = globalVariant.IsUnlimitedStock,
                IsCodAllowed = globalVariant.IsCodAllowed,
                FssaiLicNo = globalVariant.FssaiLicNo,
                Barcode = globalVariant.Barcode,
                MetaTitle = globalVariant.MetaTitle,
                MetaKeywords = globalVariant.MetaKeywords,
                SchemaMarkup = globalVariant.SchemaMarkup,
                MetaDescription = globalVariant.MetaDescription,
                TotalAllowedQuantity = globalVariant.TotalAllowedQuantity,
                IsTaxIncludedInPrice = globalVariant.IsTaxIncludedInPrice,
                ReturnPolicy = globalVariant.ReturnPolicy,
                Measurement = globalVariant.Measurement,
                Price = globalVariant.Price,
                DiscountedPrice = globalVariant.DiscountedPrice,
                Stock = globalVariant.Stock,
                SKU = GenerateSKU(globalVariant.SKU, objSM.SellerId),
                ProductId = sellerProduct.Id,
                ViewCount = 0
            };

            await _apiDbContext.ProductVariant.AddAsync(newVariant);
            await _apiDbContext.SaveChangesAsync();

            return await GetProductVariantById(newVariant.Id);
        }

        private string GenerateSlug(string name, long sellerId)
        {
            return $"{name.ToLower().Replace(" ", "-")}-{sellerId}-{Guid.NewGuid().ToString().Substring(0, 5)}";
        }
        private string GenerateSKU(string baseSku, long sellerId)
        {
            return $"{baseSku}-{sellerId}-{Guid.NewGuid().ToString().Substring(0, 5)}";
        }

        public async Task<List<SearchResponseSM>> GetAssignedSellers(long productVariantId)
        {
            // 🔹 Step 1: Get global variant + product
            var globalVariant = await _apiDbContext.ProductVariant
                .Include(x => x.Product)
                .FirstOrDefaultAsync(x => x.Id == productVariantId);

            if (globalVariant == null)
                throw new SiffrumException(ApiErrorTypeSM.InvalidInputData_NoLog,
                    "Product variant not found");

            var normalizedProductName = globalVariant.Product.Name.Trim().ToLower();
            var normalizedVariantName = globalVariant.Name.Trim().ToLower();

            // 🔹 Step 2: Find all sellers having same product + variant
            var sellers = await _apiDbContext.ProductVariant
                .Where(x =>
                    x.Product.Name.ToLower().Trim() == normalizedProductName &&
                    x.Name.ToLower().Trim() == normalizedVariantName)
                .Select(x => new SearchResponseSM
                {
                    Id = x.Product.SellerId,
                    Title = x.Product.Seller.Name
                })
                .Distinct()
                .ToListAsync();

            return sellers;
        }

        #endregion Assign Product to Seller

        #region Update Product Variant

        public async Task<ProductVariantSM> UpdateProduct(long sellerId, long id, ProductVariantSM objSM)
        {
            var dm = await _apiDbContext.ProductVariant.FindAsync(id);
            if (dm == null)
            {
                throw new SiffrumException(ApiErrorTypeSM.InvalidInputData_Log, "Product not found");
            }
            if (!string.Equals(dm.Name, objSM.Name, StringComparison.OrdinalIgnoreCase))
            {
                var exists = await SellerVariantNameExistsForUpdate(sellerId, id, objSM.Name);

                if (exists)
                    throw new SiffrumException(
                        ApiErrorTypeSM.InvalidInputData_NoLog,
                        "Product variant name already exists"
                    );
            }
            string oldImage = null;
            if (!string.IsNullOrEmpty(objSM.ImageBase64))
            {
                var imagePath = await _imageProcess.SaveFromBase64(objSM.ImageBase64, "jpg", "wwwroot/content/products");
                if (!(string.IsNullOrEmpty(imagePath)))
                {
                    oldImage = dm.Image;
                    dm.Image = imagePath;
                }
            }
            objSM.Status = (ProductStatusSM)dm.Status;
            objSM.ViewCount = dm.ViewCount;
            objSM.ProductId = dm.ProductId;
            _mapper.Map(objSM, dm);
            dm.Id = id;
            dm.PlatformType = (PlatformTypeDM)objSM.PlatformType;
            dm.UpdatedAt = DateTime.UtcNow;
            dm.UpdatedBy = _loginUserDetail.LoginId;
            if (await _apiDbContext.SaveChangesAsync() > 0)
            {
                if (File.Exists(oldImage)) File.Delete(oldImage);
                return await GetProductVariantById(dm.Id);
            }
            throw new SiffrumException(ApiErrorTypeSM.Fatal_Log, $"Product with Id:{id} updation failed", "Failed to update product details");
        }

        public async Task<ProductVariantSM> UpdateStock(long sellerId, long variantId, decimal stock)
        {
            var dm = await _apiDbContext.ProductVariant
                .Include(x => x.Product)
                .FirstOrDefaultAsync(x => x.Id == variantId && x.Product.SellerId == sellerId);
            if (dm == null)
                throw new SiffrumException(ApiErrorTypeSM.InvalidInputData_NoLog, "Product variant not found");

            dm.Stock = stock;
            dm.IsUnlimitedStock = false;
            dm.UpdatedAt = DateTime.UtcNow;
            dm.UpdatedBy = _loginUserDetail.LoginId;

            if (await _apiDbContext.SaveChangesAsync() > 0)
                return await GetProductVariantById(dm.Id);

            throw new SiffrumException(ApiErrorTypeSM.Fatal_Log, "Failed to update stock");
        }

        public async Task<ProductVariantSM> UpdateProductStatus(long id, ProductStatusSM status)
        {
            var dm = await _apiDbContext.ProductVariant.FindAsync(id);
            if (dm == null)
            {
                throw new SiffrumException(ApiErrorTypeSM.InvalidInputData_Log, "Product not found");
            }
            if(dm.Status == (ProductStatusDM)status)
            {
                throw new SiffrumException(ApiErrorTypeSM.InvalidInputData_Log, "Product status is already updated");
            }
            
            dm.Status = (ProductStatusDM)status;
            dm.UpdatedAt = DateTime.UtcNow;
            dm.UpdatedBy = _loginUserDetail.LoginId;
            if (await _apiDbContext.SaveChangesAsync() > 0)
            {
                return await GetProductVariantById(dm.Id);
            }
            throw new SiffrumException(ApiErrorTypeSM.Fatal_Log, $"Product with Id:{id} updation failed", "Failed to update product status");
        }

        public async Task<BoolResponseRoot> BulkUpdateProductStatus(List<long> ids, ProductStatusSM status)
        {
            if (ids == null || ids.Count == 0)
                throw new SiffrumException(ApiErrorTypeSM.InvalidInputData_NoLog, "No product variant IDs provided");

            var variants = await _apiDbContext.ProductVariant
                .Where(x => ids.Contains(x.Id))
                .ToListAsync();

            if (variants.Count == 0)
                throw new SiffrumException(ApiErrorTypeSM.InvalidInputData_NoLog, "No matching product variants found");

            foreach (var dm in variants)
            {
                dm.Status = (ProductStatusDM)status;
                dm.UpdatedAt = DateTime.UtcNow;
                dm.UpdatedBy = _loginUserDetail.LoginId;
            }

            var saved = await _apiDbContext.SaveChangesAsync();
            return new BoolResponseRoot(saved > 0, $"{variants.Count} product variant(s) updated to {status}");
        }

        public async Task<BoolResponseRoot> BulkAssignProductToSellers(long productVariantId, List<long> sellerIds)
        {
            if (sellerIds == null || sellerIds.Count == 0)
                throw new SiffrumException(ApiErrorTypeSM.InvalidInputData_NoLog, "No seller IDs provided");

            int successCount = 0;
            var errors = new List<string>();

            foreach (var sellerId in sellerIds)
            {
                try
                {
                    await AssignProductToSeller(new SellerProductAssociationsSM
                    {
                        ProductVariantId = productVariantId,
                        SellerId = sellerId
                    });
                    successCount++;
                }
                catch (SiffrumException ex)
                {
                    errors.Add($"Seller {sellerId}: {ex.Message}");
                }
            }

            var msg = $"Assigned to {successCount}/{sellerIds.Count} seller(s)";
            if (errors.Count > 0)
                msg += $". Errors: {string.Join("; ", errors)}";

            return new BoolResponseRoot(successCount > 0, msg);
        }

        public async Task<BoolResponseRoot> BulkUpdatePrice(long productVariantId, double? price, double? discountedPrice, long? sellerId)
        {
            IQueryable<ProductVariantDM> query;

            if (sellerId.HasValue && sellerId.Value > 0)
            {
                // Per-store: find the variant copy belonging to this seller
                var globalVariant = await _apiDbContext.ProductVariant
                    .Include(x => x.Product)
                    .FirstOrDefaultAsync(x => x.Id == productVariantId);

                if (globalVariant == null)
                    throw new SiffrumException(ApiErrorTypeSM.InvalidInputData_NoLog, "Product variant not found");

                var normalizedProductName = globalVariant.Product.Name.Trim().ToLower();
                var normalizedVariantName = globalVariant.Name.Trim().ToLower();

                query = _apiDbContext.ProductVariant
                    .Include(x => x.Product)
                    .Where(x =>
                        x.Product.SellerId == sellerId.Value &&
                        x.Product.Name.ToLower().Trim() == normalizedProductName &&
                        x.Name.ToLower().Trim() == normalizedVariantName);
            }
            else
            {
                // Global: find all copies of this variant across all sellers
                var globalVariant = await _apiDbContext.ProductVariant
                    .Include(x => x.Product)
                    .FirstOrDefaultAsync(x => x.Id == productVariantId);

                if (globalVariant == null)
                    throw new SiffrumException(ApiErrorTypeSM.InvalidInputData_NoLog, "Product variant not found");

                var normalizedProductName = globalVariant.Product.Name.Trim().ToLower();
                var normalizedVariantName = globalVariant.Name.Trim().ToLower();

                query = _apiDbContext.ProductVariant
                    .Include(x => x.Product)
                    .Where(x =>
                        x.Product.Name.ToLower().Trim() == normalizedProductName &&
                        x.Name.ToLower().Trim() == normalizedVariantName);
            }

            var variants = await query.ToListAsync();
            if (variants.Count == 0)
                throw new SiffrumException(ApiErrorTypeSM.InvalidInputData_NoLog, "No matching variants found");

            foreach (var v in variants)
            {
                if (price.HasValue) v.Price = (decimal)price.Value;
                if (discountedPrice.HasValue) v.DiscountedPrice = (decimal)discountedPrice.Value;
                v.UpdatedAt = DateTime.UtcNow;
                v.UpdatedBy = _loginUserDetail.LoginId;
            }

            var saved = await _apiDbContext.SaveChangesAsync();
            var scope = sellerId.HasValue ? $"seller {sellerId}" : "all stores";
            return new BoolResponseRoot(saved > 0, $"Price updated for {variants.Count} variant(s) across {scope}");
        }

        #endregion Update

        #region Delete Rating

        public async Task<DeleteResponseRoot> DeleteProductByAdmin(long id)
        {
            var product = await _apiDbContext.ProductVariant
                    .FindAsync(id);

            if (product == null)
            {
                throw new SiffrumException(
                    ApiErrorTypeSM.InvalidInputData_Log,
                    "Product not found"
                );
            }
            _apiDbContext.ProductVariant.Remove(product);

            await _apiDbContext.SaveChangesAsync();

            return new DeleteResponseRoot(true, "Product variant deleted successfully");
        }

        public async Task<DeleteResponseRoot> DeleteMineProduct(long sellerId,  long id)
        {
            var variant = await _apiDbContext.ProductVariant
                .AsNoTracking()
                .Include(x => x.Product)
                .FirstOrDefaultAsync(x =>
                    x.Product.SellerId == sellerId &&
                    x.Id == id);
            if (variant == null)
            {
                throw new SiffrumException(
                    ApiErrorTypeSM.InvalidInputData_Log,
                    $"Seller with id:{sellerId} tries to delete product variant id:{id} which does not belong to this seller" +
                    "Product variant not found"
                );
            }
            string oldImage = null;
            if(!string.IsNullOrEmpty(variant.Image))
            {
                oldImage = variant.Image;
            }
               
            //Todo: Handle other relations here

            // 🔹 Remove DB records
            _apiDbContext.ProductVariant.Remove(variant);

            await _apiDbContext.SaveChangesAsync();
            if (File.Exists(oldImage)) File.Delete(oldImage);
            return new DeleteResponseRoot(true, "Product variant deleted successfully");
        }


        #endregion Delete Rating

        #region Product Already Present By Seller

        public async Task<ProductVariantSM?> GetSellerProductVariantByName(long sellerId, string name)
        {
            var dm = await _apiDbContext.ProductVariant
                .AsNoTracking()
                .Include(x => x.Product)
                .FirstOrDefaultAsync(x =>
                    x.Product.SellerId == sellerId &&
                    x.Name.ToLower() == name.ToLower());

            return dm == null ? null : _mapper.Map<ProductVariantSM>(dm);
        }
        public async Task<bool> SellerVariantNameExistsForUpdate(long sellerId,long variantId, string name)
        {
            return await _apiDbContext.ProductVariant
                .AsNoTracking()
                .Include(x => x.Product)
                .AnyAsync(x =>
                    x.Product.SellerId == sellerId &&
                    x.Id != variantId &&
                    EF.Functions.ILike(x.Name, name));
        }

        #endregion Product Already Present By Seller

        #region Tags

        public async Task<List<ProductTagSM>> GetTagsByProductVariantId(long productVariantId)
        {
            var dms = await _apiDbContext.ProductTag.AsNoTracking()
                .Where(x => x.ProductVariantId == productVariantId).ToListAsync();
            if (dms.Count == 0)
            {
                return new List<ProductTagSM>();
            }
            var response = new List<ProductTagSM>();
            foreach (var dm in dms)
            {
                var sm = _mapper.Map<ProductTagSM>(dm);
                var existingTag = await GetTagById(dm.TagId);
                if (existingTag != null)
                {
                    sm.Name = existingTag.Name;
                }
                response.Add(sm);
            }
            return response;
        }

        public async Task<TagSM> GetTagById(long id)
        {
            var dm = await _apiDbContext.Tag.FindAsync(id);
            if (dm != null)
            {
                var sm = _mapper.Map<TagSM>(dm);
                return sm;
            }
            return null;
        }

        #endregion Tags

        #region Admin Variant Management

        public async Task<ProductVariantSM> AddProductByAdmin(ProductVariantSM request)
        {
            if (request == null)
                throw new SiffrumException(ApiErrorTypeSM.ModelError_NoLog, "Product variant data is required");
            if (string.IsNullOrEmpty(request.Name))
                throw new SiffrumException(ApiErrorTypeSM.ModelError_NoLog, "Variant name is required");

            var baseProduct = await _apiDbContext.Product.FindAsync(request.ProductId);
            if (baseProduct == null)
                throw new SiffrumException(ApiErrorTypeSM.InvalidInputData_NoLog, "Base product not found");

            var dm = _mapper.Map<ProductVariantDM>(request);

            if (!string.IsNullOrEmpty(request.ImageBase64))
            {
                var imagePath = await _imageProcess.SaveFromBase64(request.ImageBase64, "jpg", "wwwroot/content/products");
                if (!string.IsNullOrEmpty(imagePath))
                    dm.Image = imagePath;
            }

            dm.Status = ProductStatusDM.Active;
            dm.PlatformType = (PlatformTypeDM)request.PlatformType;
            dm.CreatedAt = DateTime.UtcNow;
            dm.CreatedBy = _loginUserDetail.LoginId;
            _apiDbContext.ProductVariant.Add(dm);

            if (await _apiDbContext.SaveChangesAsync() > 0)
                return await GetProductVariantById(dm.Id);

            throw new SiffrumException(ApiErrorTypeSM.Fatal_Log, "Failed to create product variant");
        }

        public async Task<ProductVariantSM> UpdateProductByAdmin(long id, ProductVariantSM objSM)
        {
            var dm = await _apiDbContext.ProductVariant.FindAsync(id);
            if (dm == null)
                throw new SiffrumException(ApiErrorTypeSM.InvalidInputData_Log, "Product variant not found");

            string oldImage = null;
            if (!string.IsNullOrEmpty(objSM.ImageBase64))
            {
                var imagePath = await _imageProcess.SaveFromBase64(objSM.ImageBase64, "jpg", "wwwroot/content/products");
                if (!string.IsNullOrEmpty(imagePath))
                {
                    oldImage = dm.Image;
                    dm.Image = imagePath;
                }
            }

            objSM.Status = (ProductStatusSM)dm.Status;
            objSM.ViewCount = dm.ViewCount;
            objSM.ProductId = dm.ProductId;
            _mapper.Map(objSM, dm);
            dm.Id = id;
            dm.PlatformType = (PlatformTypeDM)objSM.PlatformType;
            dm.UpdatedAt = DateTime.UtcNow;
            dm.UpdatedBy = _loginUserDetail.LoginId;

            if (await _apiDbContext.SaveChangesAsync() > 0)
            {
                if (File.Exists(oldImage)) File.Delete(oldImage);
                return await GetProductVariantById(dm.Id);
            }

            throw new SiffrumException(ApiErrorTypeSM.Fatal_Log, "Failed to update product variant");
        }

        #endregion Admin Variant Management

    }
}
