using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Siffrum.Ecom.BAL.ExceptionHandler;
using Siffrum.Ecom.BAL.Foundation.Base;
using Siffrum.Ecom.DAL.Context;
using Siffrum.Ecom.DomainModels.v1;
using Siffrum.Ecom.ServiceModels.Foundation.Base.CommonResponseRoot;
using Siffrum.Ecom.ServiceModels.Foundation.Base.Enums;
using Siffrum.Ecom.ServiceModels.v1;

namespace Siffrum.Ecom.BAL.Product
{
    public class ProductSpecificationFilterProcess : SiffrumBalBase
    {
        public ProductSpecificationFilterProcess(
            IMapper mapper,
            ApiDbContext apiDbContext)
            : base(mapper, apiDbContext)
        {
        }  
        #region CREATE
        public async Task<BoolResponseRoot> CreateAsync(ProductSpecificationFilterSM objSM,long categoryId)
        {
            if (objSM == null)
                throw new SiffrumException(
                    ApiErrorTypeSM.ModelError_NoLog,
                    "Specification data is required");

            if (string.IsNullOrWhiteSpace(objSM.Name))
                throw new SiffrumException(
                    ApiErrorTypeSM.InvalidInputData_NoLog,
                    "Specification name is required");

            var category = await _apiDbContext.Category
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == categoryId && x.Level == 2);

            if (category == null)
                throw new SiffrumException(
                    ApiErrorTypeSM.InvalidInputData_NoLog,
                    "Category must be level 2");

            // Check filter already exists
            var filterDM = await _apiDbContext.ProductSpecificationFilters
                .Include(x => x.SpecificationValues)
                .FirstOrDefaultAsync(x =>
                    x.Name.ToLower() == objSM.Name.ToLower());

            if (filterDM == null)
            {
                filterDM = new ProductSpecificationFilterDM
                {
                    Name = objSM.Name,
                    SpecificationValues = new List<ProductSpecificationValueDM>()
                };

                await _apiDbContext.ProductSpecificationFilters.AddAsync(filterDM);
                await _apiDbContext.SaveChangesAsync();
            }

            // Add values (avoid duplicates)
            if (objSM.SpecificationValues != null)
            {
                foreach (var valueSM in objSM.SpecificationValues)
                {
                    var exists = filterDM.SpecificationValues
                        .Any(x => x.Value.ToLower() == valueSM.Value.ToLower());

                    if (!exists)
                    {
                        filterDM.SpecificationValues.Add(
                            new ProductSpecificationValueDM
                            {
                                Value = valueSM.Value,
                                SpecificationFilterId = filterDM.Id
                            });
                    }
                }
            }

            // Category mapping
            var mappingExists = await _apiDbContext.CategorySpecifications
                .AnyAsync(x =>
                    x.CategoryId == categoryId &&
                    x.SpecificationId == filterDM.Id);

            if (!mappingExists)
            {
                await _apiDbContext.CategorySpecifications.AddAsync(
                    new CategorySpecificationDM
                    {
                        CategoryId = categoryId,
                        SpecificationId = filterDM.Id
                    });
            }

            if (await _apiDbContext.SaveChangesAsync() > 0)
            {
                return new BoolResponseRoot(true, "Specification created successfully");
            }

            throw new SiffrumException(
                ApiErrorTypeSM.Fatal_Log,
                "Failed to create specification");
        }

        public async Task<BoolResponseRoot> CreateSpecificationValuesAsync(List<ProductSpecificationValueSM> objSM, long filterId )
        {
            if (objSM.Count == 0)
                throw new SiffrumException(
                    ApiErrorTypeSM.ModelError_NoLog,
                    "Specification values are required");

            var filterDM = await _apiDbContext.ProductSpecificationFilters
                .Include(x => x.SpecificationValues)
                .FirstOrDefaultAsync(x => x.Id == filterId);
            // Add values (avoid duplicates)
            if (objSM != null)
            {
                foreach (var valueSM in objSM)
                {
                    var exists = filterDM.SpecificationValues
                        .Any(x => x.Value.ToLower() == valueSM.Value.ToLower());

                    if (!exists)
                    {
                        filterDM.SpecificationValues.Add(
                            new ProductSpecificationValueDM
                            {
                                Value = valueSM.Value,
                                SpecificationFilterId = filterDM.Id
                            });
                    }
                }
            }

            if (await _apiDbContext.SaveChangesAsync() > 0)
            {
                return new BoolResponseRoot(true, "Specification values created successfully");
            }

            throw new SiffrumException(
                ApiErrorTypeSM.Fatal_Log,
                "Specification Value already present");
        }
        #endregion

        #region READ

        public async Task<ProductSpecificationFilterSM?> GetByIdAsync(long id)
        {
            var dm = await _apiDbContext.ProductSpecificationFilters
                .Include(x => x.SpecificationValues)
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id);

            return dm == null ? null : _mapper.Map<ProductSpecificationFilterSM>(dm);
        }

        public async Task<ProductSpecificationValueSM?> GetFilterValueByIdAsync(long id)
        {
            var dm = await _apiDbContext.ProductSpecificationValues
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id);

            return dm == null ? null : _mapper.Map<ProductSpecificationValueSM>(dm);
        }

        public async Task<List<ProductSpecificationFilterSM>> GetByCategoryIdAsync(long categoryId)
        {
            var specificationIds = await _apiDbContext.CategorySpecifications
                .Where(x => x.CategoryId == categoryId)
                .Select(x => x.SpecificationId)
                .ToListAsync();

            var filters = await _apiDbContext.ProductSpecificationFilters
                .Include(x => x.SpecificationValues)
                .Where(x => specificationIds.Contains(x.Id))
                .AsNoTracking()
                .ToListAsync();

            return _mapper.Map<List<ProductSpecificationFilterSM>>(filters);
        }

        #endregion

        #region UPDATE

        public async Task<ProductSpecificationFilterSM> UpdateFilterAsync(long id,ProductSpecificationFilterSM objSM)
        {
            if (objSM == null || string.IsNullOrWhiteSpace(objSM.Name))
                throw new SiffrumException(
                    ApiErrorTypeSM.InvalidInputData_NoLog,
                    "Specification name is required");

            var dm = await _apiDbContext.ProductSpecificationFilters
                .FirstOrDefaultAsync(x => x.Id == id);

            if (dm == null)
                throw new SiffrumException(
                    ApiErrorTypeSM.InvalidInputData_NoLog,
                    "Specification filter not found");
            if (dm.Name == objSM.Name)
            {
                return await GetByIdAsync(id);
            }
            var nameExists = await _apiDbContext.ProductSpecificationFilters
                .AnyAsync(x =>
                    x.Id != id &&
                    x.Name.ToLower() == objSM.Name.ToLower());

            if (nameExists)
                throw new SiffrumException(
                    ApiErrorTypeSM.InvalidInputData_NoLog,
                    "Specification filter already exists");
            
            dm.Name = objSM.Name;

            if (await _apiDbContext.SaveChangesAsync() > 0)
                return await GetByIdAsync(id);

            throw new SiffrumException(
                ApiErrorTypeSM.Fatal_Log,
                "Failed to update specification filter");
        }

        public async Task<ProductSpecificationValueSM> UpdateValueAsync(long valueId, ProductSpecificationValueSM objSM)
        {
            if(objSM == null)
            {
                throw new SiffrumException(
                    ApiErrorTypeSM.ModelError_NoLog,
                    "Specification value object is required");
            }
            if (string.IsNullOrWhiteSpace(objSM.Value))
                throw new SiffrumException(
                    ApiErrorTypeSM.InvalidInputData_NoLog,
                    "Specification value is required");

            var valueDM = await _apiDbContext.ProductSpecificationValues
                .FirstOrDefaultAsync(x => x.Id == valueId);

            if (valueDM == null)
                throw new SiffrumException(
                    ApiErrorTypeSM.InvalidInputData_NoLog,
                    "Specification value not found");
            if (valueDM.Value == objSM.Value)
            {
                return _mapper.Map<ProductSpecificationValueSM>(valueDM);
            }
            var exists = await _apiDbContext.ProductSpecificationValues
                .AnyAsync(x =>
                    x.Id != valueId &&
                    x.SpecificationFilterId == valueDM.SpecificationFilterId &&
                    x.Value.ToLower() == objSM.Value.ToLower());

            if (exists)
                throw new SiffrumException(
                    ApiErrorTypeSM.InvalidInputData_NoLog,
                    "Specification value already exists");
            
            valueDM.Value = objSM.Value;

            if (await _apiDbContext.SaveChangesAsync() > 0)
                return _mapper.Map<ProductSpecificationValueSM>(valueDM);

            throw new SiffrumException(
                ApiErrorTypeSM.Fatal_Log,
                "Failed to update specification value");
        }
        #endregion UPDATE

        #region Delete

        public async Task<DeleteResponseRoot> DeleteFilterAsync(long id)
        {
            var filterDM = await _apiDbContext.ProductSpecificationFilters
                .Include(x => x.SpecificationValues)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (filterDM == null)
                throw new SiffrumException(
                    ApiErrorTypeSM.InvalidInputData_Log,
                    "Specification filter not found");

            var usedInProduct = await _apiDbContext.ProductSpecificationFilterValues
                .AnyAsync(x => x.ProductFilterId == id);

            if (usedInProduct)
                throw new SiffrumException(
                    ApiErrorTypeSM.InvalidInputData_NoLog,
                    "Cannot delete Specification filter as it is used in products");

            var usedInCategory = await _apiDbContext.CategorySpecifications
                .AnyAsync(x => x.SpecificationId == id);

            if (usedInCategory)
                throw new SiffrumException(
                    ApiErrorTypeSM.InvalidInputData_NoLog,
                    "Cannot delete Specification filter as it is associated with category");

            if (filterDM.SpecificationValues.Any())
                throw new SiffrumException(
                    ApiErrorTypeSM.InvalidInputData_NoLog,
                    "Cannot delete specification filter. Remove specification values first");

            _apiDbContext.ProductSpecificationFilters.Remove(filterDM);

            if (await _apiDbContext.SaveChangesAsync() > 0)
                return new DeleteResponseRoot(true, "Specification filter deleted");

            throw new SiffrumException(
                ApiErrorTypeSM.Fatal_Log,
                "Failed to delete specification filter");
        }

        public async Task<DeleteResponseRoot> DeleteValueAsync(long valueId)
        {
            var valueDM = await _apiDbContext.ProductSpecificationValues
                .FirstOrDefaultAsync(x => x.Id == valueId);

            if (valueDM == null)
                throw new SiffrumException(
                    ApiErrorTypeSM.InvalidInputData_Log,
                    "Specification value not found");

            var usedInProduct = await _apiDbContext.ProductSpecificationFilterValues
                .AnyAsync(x => x.ProductFilterValueId == valueId);

            if (usedInProduct)
                throw new SiffrumException(
                    ApiErrorTypeSM.InvalidInputData_NoLog,
                    "Cannot delete Specification value as it is used in products");

            _apiDbContext.ProductSpecificationValues.Remove(valueDM);

            if (await _apiDbContext.SaveChangesAsync() > 0)
                return new DeleteResponseRoot(true, "Specification value deleted");

            throw new SiffrumException(
                ApiErrorTypeSM.Fatal_Log,
                "Failed to delete specification value");
        }


        #endregion Delete


        #region Product Specification Filters

        public async Task<List<ProductSpecificationFilterValueSM>> GetProductSpecificationFiltersAsync(long productVariantId)
        {
            var productFilters = await _apiDbContext.ProductSpecificationFilterValues
                .Where(x => x.ProductVariantId == productVariantId)
                .ToListAsync();
            var response = new List<ProductSpecificationFilterValueSM>();
            if(productFilters.Count == 0)
            {
                return response;
            }
            foreach (var filter in productFilters)
            {
                var existingFilter = await GetByIdAsync(filter.ProductFilterId);
                var existingValue = await GetFilterValueByIdAsync(filter.ProductFilterValueId);

                if (existingFilter != null && existingValue != null)
                {
                    response.Add(new ProductSpecificationFilterValueSM()
                    {
                        Id = filter.Id,
                        ProductSpecificationFilter = existingFilter,
                        ProductSpecificationValue = existingValue,
                        ProductVariantId = productVariantId
                    });
                }
            }
            return response;
            
        }

        public async Task<ProductSpecificationFilterValueSM> GetProductSpecificationFiltersValueByIdAsync(long id)
        {
            var productFilters = await _apiDbContext.ProductSpecificationFilterValues
                .FindAsync(id);

            var existingFilter = await GetByIdAsync(productFilters.ProductFilterId);
            var existingValue = await GetFilterValueByIdAsync(productFilters.ProductFilterValueId);

            if (existingFilter != null && existingValue != null)
            {
                return new ProductSpecificationFilterValueSM()
                {
                    Id = id,
                    ProductSpecificationFilter = existingFilter,
                    ProductSpecificationValue = existingValue,
                    ProductVariantId = existingFilter.Id
                };
            }
            return null;
        }

        public async Task<List<ProductSpecificationFilterValueSM>> AddProductSpecificationFiltersAsync(
    long productVariantId,
    List<ProductSpecificationFilterValueSM> filters)
        {
            if (filters == null || !filters.Any())
                throw new SiffrumException(
                    ApiErrorTypeSM.InvalidInputData_NoLog,
                    "Filters are required");

            var variantExists = await _apiDbContext.ProductVariant
                .AnyAsync(x => x.Id == productVariantId);

            if (!variantExists)
                throw new SiffrumException(
                    ApiErrorTypeSM.InvalidInputData_NoLog,
                    "Product Variant not found");

            foreach (var filter in filters)
            {
                var exists = await _apiDbContext.ProductSpecificationFilterValues
                    .AnyAsync(x =>
                        x.ProductVariantId == productVariantId &&
                        x.ProductFilterId == filter.ProductSpecificationFilter.Id &&
                        x.ProductFilterValueId == filter.ProductSpecificationValue.Id);

                if (exists)
                    continue; // Skip duplicates


                var dm = new ProductSpecificationFilterValueDM
                {
                    ProductVariantId = productVariantId,
                    ProductFilterId = filter.ProductSpecificationFilter.Id,
                    ProductFilterValueId = filter.ProductSpecificationValue.Id,
                };

                await _apiDbContext.ProductSpecificationFilterValues.AddAsync(dm);
            }

            await _apiDbContext.SaveChangesAsync();

            return await GetProductSpecificationFiltersAsync(productVariantId);
        }

        public async Task<List<ProductSpecificationFilterValueSM>> UpdateProductSpecificationFilterAsync(
    long id,
    ProductSpecificationFilterValueSM objSM)
        {
            if (objSM == null)
                throw new SiffrumException(
                    ApiErrorTypeSM.ModelError_NoLog,
                    "Filter data is required");

            var dm = await _apiDbContext.ProductSpecificationFilterValues
                .FirstOrDefaultAsync(x => x.Id == id);

            if (dm == null)
                throw new SiffrumException(
                    ApiErrorTypeSM.InvalidInputData_NoLog,
                    "Filter mapping not found");

            // Prevent duplicates
            var duplicate = await _apiDbContext.ProductSpecificationFilterValues
                .AnyAsync(x =>
                    x.Id != id &&
                    x.ProductVariantId == dm.ProductVariantId &&
                    x.ProductFilterId == objSM.ProductSpecificationFilter.Id &&
                    x.ProductFilterValueId == objSM.ProductSpecificationValue.Id);

            if (duplicate)
                throw new SiffrumException(
                    ApiErrorTypeSM.InvalidInputData_NoLog,
                    "Same filter & value already exists for this variant");


            dm.ProductFilterId = objSM.ProductSpecificationFilter.Id;
            dm.ProductFilterValueId = objSM.ProductSpecificationValue.Id;


            await _apiDbContext.SaveChangesAsync();

            return await GetProductSpecificationFiltersAsync(dm.ProductVariantId);
        }

        public async Task<DeleteResponseRoot> DeleteProductSpecificationFilterAsync(long id)
        {
            var dm = await _apiDbContext.ProductSpecificationFilterValues
                .FirstOrDefaultAsync(x => x.Id == id);

            if (dm == null)
                throw new SiffrumException(
                    ApiErrorTypeSM.InvalidInputData_NoLog,
                    "Product Filter not found");

            _apiDbContext.ProductSpecificationFilterValues.Remove(dm);

            await _apiDbContext.SaveChangesAsync();

            return new DeleteResponseRoot(true, "Filter removed successfully");
        }


        #endregion Product Specification Filters
    }
}
