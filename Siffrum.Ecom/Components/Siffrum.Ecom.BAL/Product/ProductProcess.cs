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
    public class ProductProcess : SiffrumBalOdataBase<ProductSM>
    {
        private readonly ILoginUserDetail _loginUserDetail;
        private readonly BrandProcess _brandProcess;
        private readonly CategoryProcess _categoryProcess;
        public ProductProcess(IMapper mapper, ApiDbContext apiDbContext, ILoginUserDetail loginUserDetail,
            BrandProcess brandProcess, CategoryProcess categoryProcess)
            : base(mapper, apiDbContext)
        {
            _loginUserDetail = loginUserDetail;
            _brandProcess = brandProcess;
            _categoryProcess = categoryProcess;
        }

        #region OData
        public override async Task<IQueryable<ProductSM>> GetServiceModelEntitiesForOdata()
        {
            IQueryable<ProductDM> entitySet = _apiDbContext.Product.AsNoTracking();
            return await base.MapEntityAsToQuerable<ProductDM, ProductSM>(_mapper, entitySet);
        }
        #endregion

        #region Get All and Counts

        #region All

        public async Task<List<ProductSM>> GetAll(int skip, int top, PlatformTypeSM? platformType = null)
        {
            var query = _apiDbContext.Product.AsNoTracking().AsQueryable();
            if (platformType.HasValue)
            {
                var platformDm = (PlatformTypeDM)(int)platformType.Value;
                query = query.Where(x => x.Category != null && x.Category.Platform == platformDm);
            }
            var dms = await query.OrderByDescending(x => x.Id).Skip(skip).Take(top).Select(x => x.Id).ToListAsync();
            var response = new List<ProductSM>();
            if(dms.Count == 0)
            {
                return response;
            }
            foreach (var id in dms)
            {
                var sm = await GetProductById(id);
                if(sm != null)
                {
                    response.Add(sm);
                }
                
            }
            return response;
        }

        public async Task<IntResponseRoot> GetAllCount(PlatformTypeSM? platformType = null)
        {
            var query = _apiDbContext.Product.AsNoTracking().AsQueryable();
            if (platformType.HasValue)
            {
                var platformDm = (PlatformTypeDM)(int)platformType.Value;
                query = query.Where(x => x.Category != null && x.Category.Platform == platformDm);
            }
            var count = await query.CountAsync();
            return new IntResponseRoot(count, "Total Products");
        }

        public async Task<List<ProductSM>> GetAllSellerProducts(long sellerId, int skip, int top)
        {
            var dms = await _apiDbContext.Product.AsNoTracking()
                .Where(x=>x.SellerId == sellerId)
                .OrderByDescending(x => x.Id)
                .Skip(skip).Take(top).Select(x => x.Id).ToListAsync();
            var response = new List<ProductSM>();
            if (dms.Count == 0)
            {
                return response;
            }
            foreach (var id in dms)
            {
                var sm = await GetProductById(id);
                if (sm != null)
                {
                    response.Add(sm);
                }

            }
            return response;
        }

        public async Task<IntResponseRoot> GetAllSellerProductsCount(long sellerId)
        {
            var count = await _apiDbContext.Product.AsNoTracking().Where(x=>x.SellerId == sellerId).CountAsync();
            return new IntResponseRoot(count, "Total Products");
        }

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

            IQueryable<ProductDM> query = _apiDbContext.Product
                .AsNoTracking();

            // Seller filter
            if (sellerId > 0)
            {
                query = query.Where(x => x.SellerId == sellerId);
            }

            // Multi-word search using LIKE (Better performance)
            foreach (var word in words)
            {
                query = query.Where(x => x.Name.ToLower().Contains(word.ToLower()));
            }

            return await query
                .OrderBy(x => x.Name)
                .Skip(skip)
                .Take(top)
                .Select(x => new SearchResponseSM
                {
                    Id = x.Id,
                    Title = x.Name
                })
                .ToListAsync();
        }

        #endregion All

        #endregion  Get All and Counts

        #region Get By Id

        public async Task<ProductSM> GetProductById(long id)
        {
            var dm = await _apiDbContext.Product.AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id);
            if(dm == null)
            {
                return null;
            }
            var sm = _mapper.Map<ProductSM>(dm);
            var category = await _apiDbContext.Category.FindAsync(dm.CategoryId);
            var brand = dm.BrandId.HasValue ? await _apiDbContext.Brand.FindAsync(dm.BrandId.Value) : null;
            sm.CategoryName = category?.Name;
            sm.BrandName = brand?.Name;
            return sm;
        }

        #endregion Get By Id

        #region Add

        public async Task<ProductSM> AddProduct(long sellerId, ProductSM request)
        {
            if (request == null)
            {
                throw new SiffrumException(ApiErrorTypeSM.ModelError_NoLog, "Product data is required");
            }
            if (string.IsNullOrEmpty(request.Name))
            {
                throw new SiffrumException(ApiErrorTypeSM.ModelError_NoLog, "Product name is required");
            }
            if (request.BrandId.HasValue && request.BrandId.Value > 0)
            {
                var exisitngBrand = await _brandProcess.GetByIdAsync(request.BrandId.Value, "Seller");
                if (exisitngBrand == null)
                {
                    throw new SiffrumException(ApiErrorTypeSM.ModelError_NoLog, "Brand not found");
                }
            }
            var existingCategory = await _categoryProcess.GetByIdAsync(request.CategoryId);
            if (existingCategory == null)
            {
                throw new SiffrumException(ApiErrorTypeSM.ModelError_NoLog, "Category not found or invalid category");
            }
            var existingProduct = await GetSellerProductByName(sellerId, request.Name);
            if (existingProduct != null)
            {
                return existingProduct;
            }
            var existingWithSlug = await GetSellerProductBySlug(request.Slug);
            if (existingWithSlug != null)
            {
                throw new SiffrumException(ApiErrorTypeSM.ModelError_NoLog, "Product slug already exists");
            }

            var dm = _mapper.Map<ProductDM>(request);
            dm.SellerId = sellerId;
            _apiDbContext.Product.Add(dm);
            if (await _apiDbContext.SaveChangesAsync() > 0)
            {
                return await GetProductById(dm.Id);
            }

            throw new SiffrumException(ApiErrorTypeSM.Fatal_Log,
               $"Error in adding product by Seller with SellerId:{sellerId}"
               , "Something went wrong while adding product details. Please try again.");
        }


        #endregion Add

        #region Update Product Status

        public async Task<BoolResponseRoot> UpdateProduct(long sellerId, long id, ProductSM objSM)
        {
            var dm = await _apiDbContext.Product.FindAsync(id);
            if (dm == null)
            {
                throw new SiffrumException(ApiErrorTypeSM.InvalidInputData_Log, "Product not found");
            }
            if(dm.SellerId != sellerId)
            {
                throw new SiffrumException(ApiErrorTypeSM.Access_Denied_Log,
                    $"Seller with Id: {sellerId} tries to update product id: {dm.Id} which is not product of this seller", 
                    "You are not authorized to update this product");
            }
            if (!string.Equals(dm.Name, objSM.Name, StringComparison.OrdinalIgnoreCase))
            {
                var exists = await _apiDbContext.Product
                    .AsNoTracking()
                    .AnyAsync(x => x.Id != id && x.SellerId == dm.SellerId && x.Name == objSM.Name);

                if (exists)
                    throw new SiffrumException(
                        ApiErrorTypeSM.InvalidInputData_NoLog,
                        "Product name already exists"
                    );
            }

            if (!string.Equals(dm.Slug, objSM.Slug, StringComparison.OrdinalIgnoreCase))
            {
                var exists = await _apiDbContext.Product
                    .AsNoTracking()
                    .AnyAsync(x => x.Id != id && x.Slug == objSM.Slug);

                if (exists)
                    throw new SiffrumException(
                        ApiErrorTypeSM.InvalidInputData_NoLog,
                        "Product slug already exists"
                    );
            }
            if(objSM.CategoryId != dm.CategoryId)
            {
                var exisitingCategory = await _categoryProcess.GetByIdAsync(objSM.CategoryId);
                if(exisitingCategory == null)
                {
                    throw new SiffrumException(ApiErrorTypeSM.InvalidInputData_NoLog, "Category not found");
                }
                if(exisitingCategory.Level == 1 || exisitingCategory.Status == StatusSM.Inactive)
                {
                    throw new SiffrumException(ApiErrorTypeSM.InvalidInputData_NoLog, "Invalid category");
                }
                dm.CategoryId = objSM.CategoryId;
            }

            if (objSM.BrandId != dm.BrandId)
            {
                if (objSM.BrandId.HasValue && objSM.BrandId.Value > 0)
                {
                    var existingBrand = await _brandProcess.GetByIdAsync(objSM.BrandId.Value, "Seller");
                    if (existingBrand == null)
                    {
                        throw new SiffrumException(ApiErrorTypeSM.InvalidInputData_NoLog, "Brand not found");
                    }
                }
                dm.BrandId = objSM.BrandId;
            }
            dm.Name = objSM.Name;
            dm.Slug = objSM.Slug;
            dm.TaxPercentage = objSM.TaxPercentage;
            dm.Tags = objSM.Tags;
            dm.UpdatedAt = DateTime.UtcNow;
            dm.UpdatedBy = _loginUserDetail.LoginId;
            if (await _apiDbContext.SaveChangesAsync() > 0)
            {
                return new BoolResponseRoot(true, "Product updated successfully");
            }
            throw new SiffrumException(ApiErrorTypeSM.Fatal_Log, $"Product with Id:{id} updation failed", "Failed to update product details");
        }

        #endregion Update

        #region Delete Product

        public async Task<DeleteResponseRoot> DeleteProduct(long sellerId, long id)
        {
            var product = await _apiDbContext.Product
                    .FirstOrDefaultAsync(x => x.Id == id && x.SellerId == sellerId);

            if (product == null)
            {
                throw new SiffrumException(
                    ApiErrorTypeSM.InvalidInputData_Log,
                    "Product not found"
                );
            }

            var variants = await _apiDbContext.ProductVariant.AnyAsync(x => x.ProductId == id); 
            if (variants)
            {
                throw new SiffrumException(
                    ApiErrorTypeSM.InvalidInputData_Log,
                    "Product has variants and cannot be deleted"
                );
            }

            _apiDbContext.Product.Remove(product);

            await _apiDbContext.SaveChangesAsync();

            return new DeleteResponseRoot(true, "Product deleted successfully");
        }

        public async Task<DeleteResponseRoot> DeleteProductByAdmin( long id)
        {
            var product = await _apiDbContext.Product
                    .FirstOrDefaultAsync(x => x.Id == id);

            if (product == null)
            {
                throw new SiffrumException(
                    ApiErrorTypeSM.InvalidInputData_Log,
                    "Product not found"
                );
            }

            var variants = await _apiDbContext.ProductVariant.AnyAsync(x => x.ProductId == id);
            if (variants)
            {
                throw new SiffrumException(
                    ApiErrorTypeSM.InvalidInputData_Log,
                    "Product has variants and cannot be deleted"
                );
            }

            // 🔹 Remove DB records
            _apiDbContext.Product.Remove(product);

            await _apiDbContext.SaveChangesAsync();

            return new DeleteResponseRoot(true, "Product deleted successfully");
        }


        #endregion Delete Product

        #region Product Already Present By Seller

        public async Task<ProductSM> GetSellerProductByName(long sellerId, string name)
        {
            var dm = await _apiDbContext.Product.AsNoTracking()
                .FirstOrDefaultAsync(x => x.Name.ToLower() == name.ToLower() && x.SellerId == sellerId);
            if(dm != null)
            {
                var sm = _mapper.Map<ProductSM>(dm);
                return sm;
            }
            return null;
            
        }
        public async Task<ProductSM> GetSellerProductBySlug( string slug)
        {
            var dm = await _apiDbContext.Product.AsNoTracking()
                .FirstOrDefaultAsync(x => x.Slug.ToLower() == slug.ToLower());
            if(dm != null)
            {
                var sm = _mapper.Map<ProductSM>(dm);
                return sm;
            }
            return null;
            
        }

        #endregion Product Already Present By Seller

        #region Admin Product Management

        public async Task<List<ProductSM>> AddProductByAdmin(AdminProductCreateSM request)
        {
            if (request?.Product == null)
                throw new SiffrumException(ApiErrorTypeSM.ModelError_NoLog, "Product data is required");

            if (string.IsNullOrEmpty(request.Product.Name))
                throw new SiffrumException(ApiErrorTypeSM.ModelError_NoLog, "Product name is required");

            if (request.SellerIds == null || request.SellerIds.Count == 0)
                throw new SiffrumException(ApiErrorTypeSM.ModelError_NoLog, "At least one seller must be selected");

            var existingCategory = await _categoryProcess.GetByIdAsync(request.Product.CategoryId);
            if (existingCategory == null)
                throw new SiffrumException(ApiErrorTypeSM.ModelError_NoLog, "Category not found");

            var results = new List<ProductSM>();

            foreach (var sellerId in request.SellerIds)
            {
                var seller = await _apiDbContext.Seller.FindAsync(sellerId);
                if (seller == null) continue;

                var existingProduct = await GetSellerProductByName(sellerId, request.Product.Name);
                if (existingProduct != null)
                {
                    results.Add(existingProduct);
                    continue;
                }

                var slug = request.Product.Slug;
                if (!string.IsNullOrEmpty(slug) && request.SellerIds.Count > 1)
                {
                    slug = $"{slug}-{sellerId}";
                }

                var dm = _mapper.Map<ProductDM>(request.Product);
                dm.SellerId = sellerId;
                dm.Slug = slug;
                dm.CreatedAt = DateTime.UtcNow;
                dm.CreatedBy = _loginUserDetail.LoginId;
                _apiDbContext.Product.Add(dm);

                if (await _apiDbContext.SaveChangesAsync() > 0)
                {
                    var created = await GetProductById(dm.Id);
                    if (created != null) results.Add(created);
                }
            }

            if (results.Count == 0)
                throw new SiffrumException(ApiErrorTypeSM.Fatal_Log, "Failed to create product for any seller");

            return results;
        }

        public async Task<BoolResponseRoot> UpdateProductByAdmin(long id, ProductSM objSM)
        {
            var dm = await _apiDbContext.Product.FindAsync(id);
            if (dm == null)
                throw new SiffrumException(ApiErrorTypeSM.InvalidInputData_Log, "Product not found");

            if (!string.Equals(dm.Name, objSM.Name, StringComparison.OrdinalIgnoreCase))
            {
                var exists = await _apiDbContext.Product
                    .AsNoTracking()
                    .AnyAsync(x => x.Id != id && x.SellerId == dm.SellerId && x.Name == objSM.Name);
                if (exists)
                    throw new SiffrumException(ApiErrorTypeSM.InvalidInputData_NoLog, "Product name already exists for this seller");
            }

            if (!string.Equals(dm.Slug, objSM.Slug, StringComparison.OrdinalIgnoreCase))
            {
                var exists = await _apiDbContext.Product
                    .AsNoTracking()
                    .AnyAsync(x => x.Id != id && x.Slug == objSM.Slug);
                if (exists)
                    throw new SiffrumException(ApiErrorTypeSM.InvalidInputData_NoLog, "Product slug already exists");
            }

            if (objSM.CategoryId != dm.CategoryId)
            {
                var cat = await _categoryProcess.GetByIdAsync(objSM.CategoryId);
                if (cat == null)
                    throw new SiffrumException(ApiErrorTypeSM.InvalidInputData_NoLog, "Category not found");
                dm.CategoryId = objSM.CategoryId;
            }

            if (objSM.SellerId > 0 && objSM.SellerId != dm.SellerId)
            {
                var seller = await _apiDbContext.Seller.FindAsync(objSM.SellerId);
                if (seller == null)
                    throw new SiffrumException(ApiErrorTypeSM.InvalidInputData_NoLog, "Seller not found");
                dm.SellerId = objSM.SellerId;
            }

            dm.Name = objSM.Name;
            dm.Slug = objSM.Slug;
            dm.TaxPercentage = objSM.TaxPercentage;
            dm.BrandId = objSM.BrandId;
            dm.Tags = objSM.Tags;
            dm.UpdatedAt = DateTime.UtcNow;
            dm.UpdatedBy = _loginUserDetail.LoginId;

            if (await _apiDbContext.SaveChangesAsync() > 0)
                return new BoolResponseRoot(true, "Product updated successfully");

            throw new SiffrumException(ApiErrorTypeSM.Fatal_Log, "Failed to update product");
        }

        #endregion Admin Product Management

    }
}
