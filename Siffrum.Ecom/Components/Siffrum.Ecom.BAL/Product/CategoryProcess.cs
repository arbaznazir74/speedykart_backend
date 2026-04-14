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
    public class CategoryProcess : SiffrumBalOdataBase<CategorySM>
    {
        private readonly ImageProcess _imageProcess;
        private readonly ILoginUserDetail _loginUserDetail;
        private readonly ProductVariantProcess _productVariantProcess;
        private readonly ComboProcess _comboProcess;

        public CategoryProcess(
            ApiDbContext apiDbContext,
            IMapper mapper,
            ImageProcess imageProcess, 
            ILoginUserDetail loginUserDetail,
            ProductVariantProcess productVariantProcess,
            ComboProcess comboProcess)
            : base(mapper, apiDbContext)
        {
            _imageProcess = imageProcess;
            _loginUserDetail = loginUserDetail;
            _productVariantProcess = productVariantProcess;
            _comboProcess = comboProcess;
        }

        #region OData
        public override async Task<IQueryable<CategorySM>> GetServiceModelEntitiesForOdata()
        {
            IQueryable<CategoryDM> entitySet = _apiDbContext.Category
                .AsNoTracking();

            return await base.MapEntityAsToQuerable<CategoryDM, CategorySM>(_mapper, entitySet);
        }
        #endregion

        #region Add Parent or Base Category
        public async Task<BoolResponseRoot> CreateAsync(CategorySM objSM)
        {
            if (objSM == null)
                throw new SiffrumException(ApiErrorTypeSM.ModelError_NoLog, "Category data is required");

            if (string.IsNullOrWhiteSpace(objSM.Name))
                throw new SiffrumException(ApiErrorTypeSM.InvalidInputData_NoLog, "Category name is required");
            if (string.IsNullOrEmpty(objSM.ImageBase64))
            {
                throw new SiffrumException(ApiErrorTypeSM.InvalidInputData_NoLog, "Category image is required");
            }
            var exists = await _apiDbContext.Category
                .AsNoTracking()
                .AnyAsync(x => x.Name.ToLower() == objSM.Name.ToLower());

            if (exists)
                throw new SiffrumException(ApiErrorTypeSM.InvalidInputData_NoLog, "Category already exists");

            if (objSM.Level == 1)
            {
                if (objSM.ParentCategoryId.HasValue)
                {
                    throw new SiffrumException(ApiErrorTypeSM.InvalidInputData_NoLog, "Parent category should have null value for parent category");
                }
                
            }            
            else
            {
                if (!objSM.ParentCategoryId.HasValue)
                {
                    throw new SiffrumException(ApiErrorTypeSM.InvalidInputData_NoLog, "Sub Category should have parent category id");
                }
                if (string.IsNullOrEmpty(objSM.WebImage))
                {
                    throw new SiffrumException(ApiErrorTypeSM.InvalidInputData_NoLog, "Category web image is required");
                }
                var parentExists = await _apiDbContext.Category
                    .AnyAsync(x => x.Id == objSM.ParentCategoryId);

                if (!parentExists)
                {
                    throw new SiffrumException(ApiErrorTypeSM.InvalidInputData_NoLog, "Parent category not found");
                }
            }

            var dm = _mapper.Map<CategoryDM>(objSM);

            if (!string.IsNullOrWhiteSpace(objSM.ImageBase64))
            {
                dm.Image = await _imageProcess.SaveFromBase64(objSM.ImageBase64,"jpg","wwwroot/content/categories");
            } 
            if (!string.IsNullOrWhiteSpace(objSM.WebImage))
            {
                dm.WebImage = await _imageProcess.SaveFromBase64(objSM.WebImage,"jpg","wwwroot/content/categories");
            }

            dm.Status = objSM.Status == 0 ? StatusDM.Active : (StatusDM)objSM.Status;
            dm.CreatedAt = DateTime.UtcNow;
            dm.CreatedBy = _loginUserDetail.LoginId;

            await _apiDbContext.Category.AddAsync(dm);

            if (await _apiDbContext.SaveChangesAsync() > 0)
                return new BoolResponseRoot(true, "Category created successfully");

            throw new SiffrumException(ApiErrorTypeSM.Fatal_Log, "Failed to create category");
        }

        #endregion Add Parent or Base Category

        #region Get Parent Categories for Admin and EndUser with count     
        public async Task<List<CategorySM>> GetParentCategoriesForAdmin(int skip, int top, PlatformTypeSM? platform = null)
        {
            var query = _apiDbContext.Category
                .AsNoTracking()
                .Where(x=>x.Level == 1 && x.ParentCategoryId == null );
            if (platform.HasValue)
            {
                query = query.Where(x => x.Platform == (PlatformTypeDM)platform.Value);
            }
            var pCatIds = await query
                .OrderBy(x => x.SortOrder)
                .Skip(skip).Take(top)
                .Select(x => x.Id)                
                .ToListAsync();
            var response = new List<CategorySM>();
            if(pCatIds.Count == 0)
            {
                return response;
            }
            foreach(var id in pCatIds)
            {
                var category = await GetByIdAsync(id);
                if (category != null)
                {
                    response.Add(category);
                }
                    
            }
            return response;
        }
        public async Task<IntResponseRoot> GetParentCategoriesForAdminCount(PlatformTypeSM? platform = null)
        {
            var query = _apiDbContext.Category
                .AsNoTracking()
                .Where(x => x.Level == 1 && x.ParentCategoryId == null );
            if (platform.HasValue)
            {
                query = query.Where(x => x.Platform == (PlatformTypeDM)platform.Value);
            }
            var count = await query
                .Select(x => x.Id)
                .CountAsync();
            return new IntResponseRoot(count, "Total Parent Categories");
            
        }       

        public async Task<List<CategorySM>> GetSubCategoriesForUser(PlatformTypeSM platform, int skip, int top)
        {
            
            var categoryIds = await _apiDbContext.Category
                .AsNoTracking()
                .Where(c =>
                c.Level > 1 &&
                c.ParentCategoryId != null &&
                c.Platform == (PlatformTypeDM)platform && c.Status == StatusDM.Active &&
                c.Products.Any(p => p.ProductVariants.Any(v => v.Status == ProductStatusDM.Active)))
                .OrderBy(c => c.SortOrder)
                .Skip(skip)
                .Take(top)
                .Select(x => x.Id)
                .ToListAsync();
            var response = new List<CategorySM>();
            if(categoryIds.Count == 0)
            {
                return response;
            }
            foreach (var id in categoryIds)
            {
                var category = await GetByIdAsync(id);
                if (category != null)
                {

                    response.Add(category);
                }

            }
            return response;
        }

        public async Task<List<CategorySM>> GetRandomSubCategoriesForUser(PlatformTypeSM platform, int top)
        {

            var categoryIds = await _apiDbContext.Category
                .AsNoTracking()
                .Where(c =>
                c.Level > 1 && c.ParentCategoryId != null &&
                c.Platform == (PlatformTypeDM)platform &&
                c.Status == StatusDM.Active &&
                c.Products.Any(p => p.ProductVariants.Any(v => v.Status == ProductStatusDM.Active)))
                .OrderBy(x => EF.Functions.Random())
                .Take(top)
                .Select(x => x.Id)
                .ToListAsync();
            var response = new List<CategorySM>();
            if (categoryIds.Count == 0)
            {
                return response;
            }
            foreach (var id in categoryIds)
            {
                var category = await GetByIdAsync(id);
                if (category != null)
                {
                    response.Add(category);
                }

            }
            return response;
        }

        public async Task<List<SearchResponseSM>> GetSubCategoriesForUserAsync(PlatformTypeSM platform, int skip, int top)
        {

            var categories = await _apiDbContext.Category
                .AsNoTracking()
                .Where(c =>
                c.Level > 1 &&
                c.ParentCategoryId != null &&
                c.Platform == (PlatformTypeDM)platform && c.Status == StatusDM.Active &&
                c.Products.Any(p => p.ProductVariants.Any(v => v.Status == ProductStatusDM.Active)))
                .OrderBy(c => c.SortOrder)
                .Skip(skip)
                .Take(top)
                .Select(x => new SearchResponseSM
                {
                    Id = x.Id,
                    Title = x.Name
                })
                .ToListAsync();
            return categories;            
        }
        public async Task<IntResponseRoot> GetSubCategoriesForUserCount(PlatformTypeSM platform)
        {
            var count = await _apiDbContext.Category
                .AsNoTracking()
                .Where(c =>
                c.Level > 1 &&
                c.ParentCategoryId != null &&
                c.Platform == (PlatformTypeDM)platform && c.Status == StatusDM.Active &&
                c.Products.Any(p => p.ProductVariants.Any(v => v.Status == ProductStatusDM.Active)))
                .Select(x => x.Id)
                .CountAsync();
            return new IntResponseRoot(count, "Total sub level Categories");

        }

        public async Task<List<CategorySM>> GetSubCategoriesForSeller(PlatformTypeSM platform, int skip, int top)
        {

            var categoryIds = await _apiDbContext.Category
                .AsNoTracking()
                .Where(c =>
                c.Level > 1 &&
                c.ParentCategoryId != null &&
                c.Platform == (PlatformTypeDM)platform && c.Status == StatusDM.Active)
                .OrderBy(c => c.SortOrder)
                .Skip(skip)
                .Take(top)
                .Select(x => x.Id)
                .ToListAsync();
            var response = new List<CategorySM>();
            if (categoryIds.Count == 0)
            {
                return response;
            }
            foreach (var id in categoryIds)
            {
                var category = await GetByIdAsync(id);
                if (category != null)
                {
                    response.Add(category);
                }

            }
            return response;
        }
        public async Task<IntResponseRoot> GetSubCategoriesForSellerCount(PlatformTypeSM platform)
        {
            var count = await _apiDbContext.Category
                .AsNoTracking()
                .Where(c =>
                c.Level > 1 &&
                c.ParentCategoryId != null &&
                c.Platform == (PlatformTypeDM)platform && c.Status == StatusDM.Active )
                .Select(x => x.Id)
                .CountAsync();
            return new IntResponseRoot(count, "Total sub level Categories");

        }

        public async Task<List<CategorySM>> GetParentCategoriesForEndUser(PlatformTypeSM platform, int skip, int top)
        {
            var parentIds = await _apiDbContext.Category
                .AsNoTracking()
                .Where(parent => parent.Level == 1
                                 && parent.ParentCategoryId == null
                                 && parent.Status == StatusDM.Active
                                 && _apiDbContext.Category.Any(child =>
                                        child.ParentCategoryId == parent.Id
                                        && child.Platform == (PlatformTypeDM)platform
                                        && child.Status == StatusDM.Active))

                .OrderBy(x => x.SortOrder)
                .Skip(skip)
                .Take(top)
                .Select(x => x.Id)
                .ToListAsync();

            var response = new List<CategorySM>();

            foreach (var id in parentIds)
            {
                var category = await GetByIdAsync(id);
                if (category != null)
                {
                    response.Add(category);
                }
            }

            return response;
        }
        public async Task<IntResponseRoot> GetParentCategoriesForEndUserCount(PlatformTypeSM platform)
        {
            var count = await _apiDbContext.Category
                .AsNoTracking()
                .Where(parent => parent.Level == 1
                                 && parent.ParentCategoryId == null
                                 && parent.Status == StatusDM.Active
                                 && _apiDbContext.Category.Any(child =>
                                        child.ParentCategoryId == parent.Id
                                        && child.Platform == (PlatformTypeDM)platform
                                        && child.Status == StatusDM.Active))
                .Select(x => x.Id)
                .CountAsync();
            return new IntResponseRoot(count, "Total Parent Categories");

            
        }

        public async Task<List<CategorySM>> GetParentCategoriesForSeller(int skip, int top, PlatformTypeSM? platform = null, long sellerId = 0)
        {
            var query = _apiDbContext.Category
                .AsNoTracking()
                .Where(parent => parent.Level == 1
                                 && parent.ParentCategoryId == null
                                 && (parent.Status == StatusDM.Active
                                     || (parent.Status == StatusDM.Pending && parent.SuggestedBySellerId == sellerId)));
            if (platform.HasValue)
            {
                query = query.Where(x => x.Platform == (PlatformTypeDM)platform.Value);
            }
            var parentIds = await query
                .OrderBy(x => x.SortOrder)
                .Skip(skip)
                .Take(top)
                .Select(x => x.Id)
                .ToListAsync();

            var response = new List<CategorySM>();

            foreach (var id in parentIds)
            {
                var category = await GetByIdAsync(id);
                if (category != null)
                {
                    response.Add(category);
                }
            }

            return response;
        }
        public async Task<IntResponseRoot> GetParentCategoriesForSellerCount(PlatformTypeSM? platform = null, long sellerId = 0)
        {
            var query = _apiDbContext.Category
                .AsNoTracking()
                .Where(parent => parent.Level == 1
                                 && parent.ParentCategoryId == null
                                 && (parent.Status == StatusDM.Active
                                     || (parent.Status == StatusDM.Pending && parent.SuggestedBySellerId == sellerId)));
            if (platform.HasValue)
            {
                query = query.Where(x => x.Platform == (PlatformTypeDM)platform.Value);
            }
            var count = await query
                .Select(x => x.Id)
                .CountAsync();
            return new IntResponseRoot(count, "Total Parent Categories");


        }

        public async Task<List<SearchResponseSM>> SearchCategories(
           PlatformTypeSM platform,
           string searchText,
           int skip,
           int top,
           bool isToShowAllCategories)
        {
            if (string.IsNullOrWhiteSpace(searchText))
                return new List<SearchResponseSM>();

            var words = searchText
                .Trim()
                .Split(' ', StringSplitOptions.RemoveEmptyEntries);

            IQueryable<CategoryDM> query = _apiDbContext.Category.Where(x => x.Level == 2 && x.Platform == (PlatformTypeDM)platform)
                .AsNoTracking();
            if (!isToShowAllCategories)
            {
                query = query.Where(x => x.Status == StatusDM.Active);
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


        #endregion Get Parent Categories

        #region Category Summary (optimized single query)

        public async Task<List<UserCategorySummarySM>> GetCategorySummaryForUser(PlatformTypeSM platform, int skip, int top, long userId = 0)
        {
            // Look up the user's assigned seller
            long? assignedSellerId = null;
            if (userId > 0)
            {
                assignedSellerId = await _apiDbContext.User
                    .AsNoTracking()
                    .Where(u => u.Id == userId)
                    .Select(u => u.AssignedSellerId)
                    .FirstOrDefaultAsync();
            }

            var query = _apiDbContext.Category
                .AsNoTracking()
                .Where(c =>
                    c.Status == StatusDM.Active &&
                    c.Platform == (PlatformTypeDM)platform &&
                    c.Products.Any());

            // If user has an assigned seller, only show categories assigned to that seller
            if (assignedSellerId.HasValue && assignedSellerId.Value > 0)
            {
                query = query.Where(c =>
                    _apiDbContext.CategorySellers.Any(cs =>
                        cs.CategoryId == c.Id && cs.SellerId == assignedSellerId.Value));
            }

            var categories = await query
                .OrderBy(c => c.SortOrder)
                .Skip(skip)
                .Take(top)
                .Select(c => new
                {
                    c.Id,
                    c.Name,
                    c.Image,
                    ProductsCount = c.Products.Count()
                })
                .ToListAsync();

            var result = new List<UserCategorySummarySM>();
            foreach (var c in categories)
            {
                var cImg = await _imageProcess.ResolveImage(c.Image);
                result.Add(new UserCategorySummarySM
                {
                    Id = c.Id,
                    Name = c.Name,
                    ImageBase64 = cImg.Base64,
                    NetworkImage = cImg.NetworkUrl,
                    ProductsCount = c.ProductsCount
                });
            }
            return result;
        }

        #endregion Category Summary

        #region Get Products in Category

        public async Task<UserSpeedyMartCategoryProductsSM> GetProductsInSpeedyMartUsingCategory(long categoryId, int skip, int top, int comboProductsCount)
        {
            var categoryProducts = await _productVariantProcess.GetSpeedyMartProductsByCategoryId(categoryId, skip, top);
            var comboProducts = await _comboProcess.GetComboProductsInCategory(categoryId, PlatformTypeSM.SpeedyMart, comboProductsCount);
            var response = new UserSpeedyMartCategoryProductsSM()
            {
                Products = categoryProducts,
                ComboProducts = comboProducts

            };
            return response;
        }

        public async Task<IntResponseRoot> GetProductsInSpeedyMartUsingCategoryCount(long categoryId)
        {
            var categoryProductsCount = await _productVariantProcess.GetSpeedyMartProductsByCategoryIdCount(categoryId);
            return categoryProductsCount;
        }
        public async Task<UserHotBoxCategoryProductsSM> GetProductsInHotBoxUsingCategory(long categoryId, int skip, int top, int comboProductsCount)
        {
            var categoryProducts = await _productVariantProcess.GetHotBoxProductsByCategoryId(categoryId, skip, top);
            var comboProducts = await _comboProcess.GetComboProductsInCategory(categoryId, PlatformTypeSM.HotBox, comboProductsCount);
            var category = await GetByIdAsync(categoryId);
            var response = new UserHotBoxCategoryProductsSM()
            {
                Id = category.Id,
                CategoryName = category.Name,
                ImageBase64 = category?.ImageBase64,
                Products = categoryProducts,
                ComboProducts = comboProducts

            };
            return response;
        }
        public async Task<UserHotBoxCategoryFullProductsSM> GetFullProductsInHotBoxUsingCategory(long categoryId, int skip, int top, int comboProductsCount)
        {
            var fullProducts = await _productVariantProcess.GetHotBoxFullProductsByCategoryId(categoryId, skip, top);
            var comboProducts = await _comboProcess.GetComboProductsInCategory(categoryId, PlatformTypeSM.HotBox, comboProductsCount);
            var category = await GetByIdAsync(categoryId);
            return new UserHotBoxCategoryFullProductsSM()
            {
                Id = category.Id,
                CategoryName = category.Name,
                ImageBase64 = category?.ImageBase64,
                Products = fullProducts,
                ComboProducts = comboProducts
            };
        }

        public async Task<IntResponseRoot> GetProductsInHotBoxUsingCategoryCount(long categoryId)
        {
            var categoryProductsCount = await _productVariantProcess.GetHotBoxProductsByCategoryIdCount(categoryId);
            return categoryProductsCount;
        }

        public async Task<UserHotBoxCategoryProductsSM> GetTopHotBoxCategoryByTiming(CategoryTimingSM timing,
            int skip,int top)
        {
            // STEP 1: Get eligible categories
            var eligibleCategories = await _apiDbContext.Category
                .AsNoTracking()
                .Where(c =>
                    c.Platform == PlatformTypeDM.HotBox &&
                    c.Status == StatusDM.Active &&
                    c.Timings == (CategoryTimingDM)timing)
                .Select(c => new
                {
                    Category = c,
                    ProductCount = _apiDbContext.ProductVariant.Count(p =>
                        p.Product.CategoryId == c.Id &&
                        p.PlatformType == PlatformTypeDM.HotBox &&
                        p.Status == ProductStatusDM.Active)
                })
                .Where(x => x.ProductCount > 0)
                .OrderByDescending(x => x.ProductCount) // Top category by product count
                .FirstOrDefaultAsync();

            if (eligibleCategories == null)
            {
                throw new SiffrumException(
                    ApiErrorTypeSM.InvalidInputData_NoLog,
                    "No HotBox category available for selected timing");
            }

            var category = eligibleCategories.Category;

            // STEP 2: Get top products of that category
            var productIds = await _apiDbContext.ProductVariant
                .AsNoTracking()
                .Where(p =>
                    p.Product.CategoryId == category.Id &&
                    p.PlatformType == PlatformTypeDM.HotBox &&
                    p.Status == ProductStatusDM.Active)
                .OrderByDescending(p => p.ViewCount)
                .Skip(skip)
                .Take(top)
                .Select(p => p.Id)
                .ToListAsync();

            var products =  await _productVariantProcess.GetHotBoxProductsByBanner(productIds);
            var comboProducts = await _comboProcess.GetComboProductsInCategory(category.Id, PlatformTypeSM.HotBox, 2);

            // STEP 3: Prepare response
            var catImg = await _imageProcess.ResolveImage(category?.Image);
            return new UserHotBoxCategoryProductsSM
            {
                Id = category.Id,
                CategoryName = category?.Name,
                ImageBase64 = catImg.Base64,
                NetworkImage = catImg.NetworkUrl,
                Products = products,
                ComboProducts = comboProducts
            };
        }

        public async Task<IntResponseRoot> GetProductsInHotBoxUsingTimingsCount(CategoryTimingSM timing)
        {
            var eligibleCategories = await _apiDbContext.Category
                .AsNoTracking()
                .Where(c =>
                    c.Platform == PlatformTypeDM.HotBox &&
                    c.Status == StatusDM.Active &&
                    c.Timings == (CategoryTimingDM)timing)
                .Select(c => new
                {
                    Category = c,
                    ProductCount = _apiDbContext.ProductVariant.Count(p =>
                        p.Product.CategoryId == c.Id &&
                        p.PlatformType == PlatformTypeDM.HotBox &&
                        p.Status == ProductStatusDM.Active)
                })
                .Where(x => x.ProductCount > 0)
                .OrderByDescending(x => x.ProductCount) // Top category by product count
                .FirstOrDefaultAsync();

            if (eligibleCategories == null)
            {
                throw new SiffrumException(
                    ApiErrorTypeSM.InvalidInputData_NoLog,
                    "No HotBox category available for selected timing");
            }

            var category = eligibleCategories.Category;

            // STEP 2: Get top products of that category
            var count = await _apiDbContext.ProductVariant
                .AsNoTracking()
                .Where(p =>
                    p.Product.CategoryId == category.Id &&
                    p.PlatformType == PlatformTypeDM.HotBox &&
                    p.Status == ProductStatusDM.Active)
                .OrderByDescending(p => p.ViewCount)
                .Select(p => p.Id)
                .CountAsync();
            return new IntResponseRoot(count, "Total products");
        }



        #endregion Get Products in Category

        #region Sub Categories with count
        public async Task<List<CategorySM>> GetSubCategoriesForAdmin(long parentCategoryId, int skip, int top)
        {
            var parentExists = await _apiDbContext.Category
                .AnyAsync(x => x.Id == parentCategoryId);

            if (!parentExists)
            {
                throw new SiffrumException(ApiErrorTypeSM.InvalidInputData_NoLog, "Parent category not found");

            }

            var subCatIds = await _apiDbContext.Category
                .AsNoTracking()
                .OrderBy(x => x.SortOrder)
                .Where(x => x.ParentCategoryId == parentCategoryId )
                .Skip(skip).Take(top)
                .Select(x => x.Id)
                .ToListAsync();
            var response = new List<CategorySM>();
            if (subCatIds.Count == 0)
            {
                return response;
            }
            foreach (var id in subCatIds)
            {
                var category = await GetByIdAsync(id);
                if (category != null)
                {
                    response.Add(category);
                }

            }
            return response;
        }

        public async Task<IntResponseRoot> GetSubCategoriesForAdminCount(long parentCategoryId)
        {
            var parentExists = await _apiDbContext.Category
                .AnyAsync(x => x.Id == parentCategoryId);

            if (!parentExists)
            {
                throw new SiffrumException(ApiErrorTypeSM.InvalidInputData_NoLog, "Parent category not found");

            }

            var count = await _apiDbContext.Category
                .AsNoTracking()
                .Where(x => x.ParentCategoryId == parentCategoryId)
                .Select(x => x.Id)
                .CountAsync();
            return new IntResponseRoot(count, "Total Sub Categories");
        }

        public async Task<List<CategorySM>> GetSubCategoriesForEndUserByParentCategory(long parentCategoryId, PlatformTypeSM platform, int skip, int top)
        {
            var parentExists = await _apiDbContext.Category
                .AnyAsync(x => x.Id == parentCategoryId);

            if (!parentExists)
            {
                throw new SiffrumException(ApiErrorTypeSM.InvalidInputData_NoLog, "Parent category not found");

            }

            var subCatIds = await _apiDbContext.Category
                .AsNoTracking()
                .OrderBy(x => x.SortOrder)
                .Where(x =>
        x.ParentCategoryId == parentCategoryId &&
        x.Status == StatusDM.Active &&
        x.Platform == (PlatformTypeDM)platform &&
        x.Products.Any(p => p.ProductVariants.Any(v => v.Status == ProductStatusDM.Active))
    )
    .Skip(skip)
    .Take(top)
    .Select(x => x.Id)
    .ToListAsync();

            var response = new List<CategorySM>();
            if (subCatIds.Count == 0)
            {
                return response;
            }
            foreach (var id in subCatIds)
            {
                var category = await GetByIdAsync(id);
                if (category != null)
                {
                    response.Add(category);
                }

            }
            return response;
        }

        

        public async Task<IntResponseRoot> GetSubCategoriesForEndUserCount(long parentCategoryId, PlatformTypeSM platform)
        {
            var parentExists = await _apiDbContext.Category
                .AnyAsync(x => x.Id == parentCategoryId);

            if (!parentExists)
            {
                throw new SiffrumException(ApiErrorTypeSM.InvalidInputData_NoLog, "Parent category not found");

            }

            var count = await _apiDbContext.Category
                .AsNoTracking()
                .OrderBy(x => x.SortOrder)
                .Where(x =>
                x.ParentCategoryId == parentCategoryId &&
                x.Status == StatusDM.Active && x.Platform == (PlatformTypeDM)platform &&
                 x.Products.Any(p => p.ProductVariants.Any(v => v.Status == ProductStatusDM.Active)))
                .CountAsync();
           
            return new IntResponseRoot(count, "Total Sub Categories");
        }


        public async Task<List<CategorySM>> GetSubCategoriesForSellerByParentCategory(long parentCategoryId, PlatformTypeSM platform, int skip, int top)
        {
            var parentExists = await _apiDbContext.Category
                .AnyAsync(x => x.Id == parentCategoryId);

            if (!parentExists)
            {
                throw new SiffrumException(ApiErrorTypeSM.InvalidInputData_NoLog, "Parent category not found");

            }

            var subCatIds = await _apiDbContext.Category
                .AsNoTracking()
                .OrderBy(x => x.SortOrder)
                .Where(x =>
        x.ParentCategoryId == parentCategoryId &&
        x.Status == StatusDM.Active &&
        x.Platform == (PlatformTypeDM)platform)
                .Skip(skip)
                .Take(top)
                .Select(x => x.Id)
                .ToListAsync();

            var response = new List<CategorySM>();
            if (subCatIds.Count == 0)
            {
                return response;
            }
            foreach (var id in subCatIds)
            {
                var category = await GetByIdAsync(id);
                if (category != null)
                {
                    response.Add(category);
                }

            }
            return response;
        }



        public async Task<IntResponseRoot> GetSubCategoriesForSellerCount(long parentCategoryId, PlatformTypeSM platform)
        {
            var parentExists = await _apiDbContext.Category
                .AnyAsync(x => x.Id == parentCategoryId);

            if (!parentExists)
            {
                throw new SiffrumException(ApiErrorTypeSM.InvalidInputData_NoLog, "Parent category not found");

            }

            var count = await _apiDbContext.Category
                .AsNoTracking()
                .OrderBy(x => x.SortOrder)
                .Where(x =>
                x.ParentCategoryId == parentCategoryId &&
                x.Status == StatusDM.Active && x.Platform == (PlatformTypeDM)platform)
                .CountAsync();
            return new IntResponseRoot(count, "Total Sub Categories");
        }

        public async Task<List<CategorySM>> GetSubCategoriesForSellerByParentCategory(long parentCategoryId, int skip, int top)
        {
            var parentExists = await _apiDbContext.Category
                .AnyAsync(x => x.Id == parentCategoryId);

            if (!parentExists)
            {
                throw new SiffrumException(ApiErrorTypeSM.InvalidInputData_NoLog, "Parent category not found");

            }

            var subCatIds = await _apiDbContext.Category
                .AsNoTracking()
                .OrderBy(x => x.SortOrder)
                .Where(x =>
                x.ParentCategoryId == parentCategoryId &&
                x.Status == StatusDM.Active)
                .Skip(skip)
                .Take(top)
                .Select(x => x.Id)
                .ToListAsync();

            var response = new List<CategorySM>();
            if (subCatIds.Count == 0)
            {
                return response;
            }
            foreach (var id in subCatIds)
            {
                var category = await GetByIdAsync(id);
                if (category != null)
                {
                    response.Add(category);
                }

            }
            return response;
        }



        public async Task<IntResponseRoot> GetSubCategoriesForSellerCount(long parentCategoryId)
        {
            var parentExists = await _apiDbContext.Category
                .AnyAsync(x => x.Id == parentCategoryId);

            if (!parentExists)
            {
                throw new SiffrumException(ApiErrorTypeSM.InvalidInputData_NoLog, "Parent category not found");

            }

            var count = await _apiDbContext.Category
                .AsNoTracking()
                .OrderBy(x => x.SortOrder)
                .Where(x =>
                x.ParentCategoryId == parentCategoryId &&
                x.Status == StatusDM.Active)
                .CountAsync();
            return new IntResponseRoot(count, "Total Sub Categories");
        }

        #endregion Sub Categories with count

        #region Get By Id
        public async Task<CategorySM?> GetByIdAsync(long id)
        {
            var dm = await _apiDbContext.Category
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id);

            if (dm == null)
            {
                return null;
            }

            var role = _loginUserDetail.UserType;

            if (role == RoleTypeSM.User)
            {
                if (dm.Status != StatusDM.Active)
                {
                    return null;
                }
            }
            else if (role == RoleTypeSM.Seller)
            {
                // Sellers can see Active categories and their own Pending suggestions
                if (dm.Status != StatusDM.Active
                    && !(dm.Status == StatusDM.Pending && dm.SuggestedBySellerId.HasValue))
                {
                    return null;
                }
            }

            var sm = _mapper.Map<CategorySM>(dm);

            // Images
            if (!string.IsNullOrEmpty(dm.Image))
            {
                var catImg = await _imageProcess.ResolveImage(dm.Image);
                sm.ImageBase64 = catImg.Base64;
                sm.NetworkImage = catImg.NetworkUrl;
            }

            if (!string.IsNullOrEmpty(dm.WebImage))
            {
                var webImg = await _imageProcess.ResolveImage(dm.WebImage);
                sm.WebImage = webImg.Base64;
                sm.NetworkWebImage = webImg.NetworkUrl;
            }

            // Product count: count unique products (by name) in category, not seller duplicates
            var productCount = await _apiDbContext.Product
                .Where(p => p.CategoryId == id)
                .Select(p => p.Name.ToLower().Trim())
                .Distinct()
                .CountAsync();

            sm.ProductsCount = productCount;

            return sm;
        }

        #endregion Get By Id

        #region Update
        public async Task<CategorySM?> UpdateAsync(long id, CategorySM objSM)
        {
            var dm = await _apiDbContext.Category.FirstOrDefaultAsync(x => x.Id == id);

            if (dm == null)
                throw new SiffrumException(ApiErrorTypeSM.InvalidInputData_NoLog, "Category not found");

            if (!string.Equals(dm.Name, objSM.Name, StringComparison.OrdinalIgnoreCase))
            {
                var exists = await _apiDbContext.Category
                    .AsNoTracking()
                    .AnyAsync(x => x.Name.ToLower() == objSM.Name.ToLower() && x.Id != id);

                if (exists)
                    throw new SiffrumException(ApiErrorTypeSM.InvalidInputData_NoLog, "Category name already exists");
            }

            string oldImage = null;
            string oldWebImage = null;

            
            _mapper.Map(objSM, dm);
            if (!string.IsNullOrEmpty(objSM.ImageBase64))
            {
                var catImagePath = await _imageProcess.SaveFromBase64(objSM.ImageBase64, "jpg", "wwwroot/content/categories");
                if (!string.IsNullOrEmpty(catImagePath))
                {
                    oldImage = dm.Image;
                    dm.Image = catImagePath;
                }
            }
            if (!string.IsNullOrEmpty(objSM.WebImage))
            {
                var webImagePath = await _imageProcess.SaveFromBase64(objSM.WebImage, "jpg", "wwwroot/content/categories");
                if (!string.IsNullOrEmpty(webImagePath))
                {
                    oldWebImage = dm.WebImage;
                    dm.WebImage = webImagePath;
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

            throw new SiffrumException(ApiErrorTypeSM.Fatal_Log,$"Failed to update category with id {id}", "Failed to update category");
        }

        #endregion Update


        #region Delete
        
        public async Task<DeleteResponseRoot> UpdateCategoryStatus(long id, StatusSM status)
        {
            var dm = await _apiDbContext.Category.FirstOrDefaultAsync(x => x.Id == id);

            if (dm == null)
                throw new SiffrumException(ApiErrorTypeSM.InvalidInputData_Log, "Category not found");
            
            
            if (dm.Status == (StatusDM)status)
            {
                return new DeleteResponseRoot(false, $"Category Status already updated to {status.ToString()}");
            }
            dm.Status = (StatusDM)status;
            dm.UpdatedAt = DateTime.UtcNow;
            dm.UpdatedBy = _loginUserDetail.LoginId;

            if (await _apiDbContext.SaveChangesAsync() > 0)
            {
                return new DeleteResponseRoot(true, "Category Status updated successfully");

            }

            throw new SiffrumException(ApiErrorTypeSM.Fatal_Log, "Failed to update category");
        }
        public async Task<DeleteResponseRoot> DeleteAsync(long id)
        {
            var dm = await _apiDbContext.Category.FirstOrDefaultAsync(x => x.Id == id);

            if (dm == null)
                throw new SiffrumException(ApiErrorTypeSM.InvalidInputData_Log, "Category not found");
            var existingRelation = await _apiDbContext.Category.Where(x => x.ParentCategoryId == id).FirstOrDefaultAsync();
            if (existingRelation != null)
            {
                throw new SiffrumException(ApiErrorTypeSM.FrameworkException_Log, "Parent Category having sub categories cannot be deleted ");
            }
            string oldImage = null;
            if (!string.IsNullOrEmpty(dm.Image))
            {
                oldImage = dm.Image;
            }
             _apiDbContext.Remove(dm);
            if (await _apiDbContext.SaveChangesAsync() > 0)
            {
                if (File.Exists(oldImage)) File.Delete(oldImage);
                return new DeleteResponseRoot(true, "Category deleted successfully");

            }

            throw new SiffrumException(ApiErrorTypeSM.Fatal_Log, "Failed to delete category");
        }        

        #endregion Delete

        #region SELLER SUGGEST

        public async Task<CategorySM> SellerSuggestCategoryAsync(CategorySM objSM, long sellerId, PlatformTypeSM platform)
        {
            if (string.IsNullOrWhiteSpace(objSM.Name))
                throw new SiffrumException(ApiErrorTypeSM.InvalidInputData_NoLog, "Category name is required");

            if (string.IsNullOrEmpty(objSM.ImageBase64))
                throw new SiffrumException(ApiErrorTypeSM.InvalidInputData_NoLog, "Category image is required");

            var exists = await _apiDbContext.Category
                .AsNoTracking()
                .AnyAsync(x => x.Name.ToLower() == objSM.Name.ToLower().Trim()
                    && x.Platform == (PlatformTypeDM)platform);

            if (exists)
                throw new SiffrumException(ApiErrorTypeSM.InvalidInputData_NoLog,
                    "A category with this name already exists");

            var slug = objSM.Name.Trim().ToLower()
                .Replace(" ", "-")
                .Replace("--", "-");

            var imagePath = await _imageProcess.SaveFromBase64(objSM.ImageBase64, "jpg", "wwwroot/content/categories");

            var dm = new CategoryDM
            {
                Name = objSM.Name.Trim(),
                Slug = slug,
                Image = imagePath,
                Status = StatusDM.Pending,
                Platform = (PlatformTypeDM)platform,
                Level = 1,
                SuggestedBySellerId = sellerId,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = _loginUserDetail.LoginId
            };

            await _apiDbContext.Category.AddAsync(dm);

            if (await _apiDbContext.SaveChangesAsync() > 0)
            {
                return _mapper.Map<CategorySM>(dm);
            }

            throw new SiffrumException(ApiErrorTypeSM.Fatal_Log, "Failed to suggest category");
        }

        public async Task<List<CategorySM>> GetPendingCategories()
        {
            var ids = await _apiDbContext.Category
                .AsNoTracking()
                .Where(x => x.Status == StatusDM.Pending)
                .OrderBy(x => x.CreatedAt)
                .Select(x => x.Id)
                .ToListAsync();

            var response = new List<CategorySM>();
            foreach (var id in ids)
            {
                var dm = await _apiDbContext.Category.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
                if (dm == null) continue;
                var sm = _mapper.Map<CategorySM>(dm);
                if (!string.IsNullOrEmpty(dm.Image))
                {
                    var sImg = await _imageProcess.ResolveImage(dm.Image);
                    sm.ImageBase64 = sImg.Base64;
                    sm.NetworkImage = sImg.NetworkUrl;
                }
                response.Add(sm);
            }
            return response;
        }

        public async Task<BoolResponseRoot> ApproveCategoryAsync(long id)
        {
            var dm = await _apiDbContext.Category.FirstOrDefaultAsync(x => x.Id == id);
            if (dm == null)
                throw new SiffrumException(ApiErrorTypeSM.InvalidInputData_NoLog, "Category not found");

            if (dm.Status != StatusDM.Pending)
                throw new SiffrumException(ApiErrorTypeSM.InvalidInputData_NoLog, "Category is not pending approval");

            dm.Status = StatusDM.Active;
            dm.UpdatedAt = DateTime.UtcNow;
            dm.UpdatedBy = _loginUserDetail.LoginId;

            // Auto-assign category to the seller who suggested it
            if (dm.SuggestedBySellerId.HasValue && dm.SuggestedBySellerId.Value > 0)
            {
                var alreadyAssigned = await _apiDbContext.CategorySellers
                    .AnyAsync(cs => cs.CategoryId == dm.Id && cs.SellerId == dm.SuggestedBySellerId.Value);
                if (!alreadyAssigned)
                {
                    _apiDbContext.CategorySellers.Add(new CategorySellerDM
                    {
                        CategoryId = dm.Id,
                        SellerId = dm.SuggestedBySellerId.Value,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    });
                }
            }

            await _apiDbContext.SaveChangesAsync();
            return new BoolResponseRoot(true, "Category approved successfully");
        }

        public async Task<BoolResponseRoot> RejectCategoryAsync(long id)
        {
            var dm = await _apiDbContext.Category.FirstOrDefaultAsync(x => x.Id == id);
            if (dm == null)
                throw new SiffrumException(ApiErrorTypeSM.InvalidInputData_NoLog, "Category not found");

            if (dm.Status != StatusDM.Pending)
                throw new SiffrumException(ApiErrorTypeSM.InvalidInputData_NoLog, "Category is not pending");

            _apiDbContext.Category.Remove(dm);
            await _apiDbContext.SaveChangesAsync();
            return new BoolResponseRoot(true, "Category rejected and removed");
        }

        #endregion SELLER SUGGEST
    }
}
