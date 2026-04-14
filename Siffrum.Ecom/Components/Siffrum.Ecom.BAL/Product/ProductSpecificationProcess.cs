using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
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
    public class ProductSpecificationProcess
        : SiffrumBalOdataBase<ProductSpecificationSM>
    {
        private readonly ILoginUserDetail _loginUserDetail;

        public ProductSpecificationProcess(
            IMapper mapper,
            ApiDbContext apiDbContext,
            ILoginUserDetail loginUserDetail)
            : base(mapper, apiDbContext)
        {
            _loginUserDetail = loginUserDetail;
        }

        #region ODATA
        public override async Task<IQueryable<ProductSpecificationSM>> GetServiceModelEntitiesForOdata()
        {
            IQueryable<ProductSpecificationDM> entitySet =
                _apiDbContext.ProductSpecifications.AsNoTracking();

            return await base.MapEntityAsToQuerable<ProductSpecificationDM, ProductSpecificationSM>(
                _mapper, entitySet);
        }
        #endregion


        #region ADD MULTIPLE SPECIFICATIONS TO VARIANT

        public async Task<BoolResponseRoot> AddSpecificationsAsync(
            long productVariantId,
            List<ProductSpecificationSM> specifications)
        {
            if (specifications == null || !specifications.Any())
                throw new SiffrumException(
                    ApiErrorTypeSM.ModelError_NoLog,
                    "Specification list required");

            var variantExists = await _apiDbContext.ProductVariant
                .AnyAsync(x => x.Id == productVariantId);

            if (!variantExists)
                throw new SiffrumException(
                    ApiErrorTypeSM.InvalidInputData_NoLog,
                    "Product variant not found");

            var dmList = new List<ProductSpecificationDM>();

            foreach (var spec in specifications)
            {
                if (string.IsNullOrWhiteSpace(spec.Key))
                    throw new SiffrumException(
                        ApiErrorTypeSM.InvalidInputData_NoLog,
                        "Specification key required");

                if (string.IsNullOrWhiteSpace(spec.Value))
                    throw new SiffrumException(
                        ApiErrorTypeSM.InvalidInputData_NoLog,
                        "Specification value required");

                var dm = new ProductSpecificationDM
                {
                    Key = spec.Key,
                    Value = spec.Value,
                    ProductVariantId = productVariantId,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = _loginUserDetail.LoginId
                };

                dmList.Add(dm);
            }

            await _apiDbContext.ProductSpecifications.AddRangeAsync(dmList);
            await _apiDbContext.SaveChangesAsync();

            return new BoolResponseRoot(true, "Specifications added successfully");
        }

        #endregion


        #region UPDATE SPECIFICATION

        public async Task<ProductSpecificationSM?> UpdateAsync(
            long id,
            ProductSpecificationSM objSM)
        {
            var dm = await _apiDbContext.ProductSpecifications
                .FirstOrDefaultAsync(x => x.Id == id);

            if (dm == null)
                throw new SiffrumException(
                    ApiErrorTypeSM.InvalidInputData_NoLog,
                    "Specification not found");

            if (string.IsNullOrWhiteSpace(objSM.Key))
                throw new SiffrumException(
                    ApiErrorTypeSM.InvalidInputData_NoLog,
                    "Specification key required");

            if (string.IsNullOrWhiteSpace(objSM.Value))
                throw new SiffrumException(
                    ApiErrorTypeSM.InvalidInputData_NoLog,
                    "Specification value required");
            objSM.Id = id;
            objSM.ProductVariantId = dm.ProductVariantId;
            _mapper.Map(objSM, dm);
            dm.UpdatedAt = DateTime.UtcNow;
            dm.UpdatedBy = _loginUserDetail.LoginId;
            if (await _apiDbContext.SaveChangesAsync() > 0)
            {
                return await GetByIdAsync(id);
            }

            throw new SiffrumException(
                ApiErrorTypeSM.Fatal_Log, $"Product Specification with Id: {id} failed to update",
                "Failed to update product Specification"
            );
        }

        #endregion


        #region DELETE SPECIFICATION

        public async Task<DeleteResponseRoot> DeleteAsync(long id)
        {
            var dm = await _apiDbContext.ProductSpecifications
                .FirstOrDefaultAsync(x => x.Id == id);

            if (dm == null)
                throw new SiffrumException(
                    ApiErrorTypeSM.InvalidInputData_Log,
                    "Specification not found");

            _apiDbContext.ProductSpecifications.Remove(dm);

            await _apiDbContext.SaveChangesAsync();

            return new DeleteResponseRoot(true, "Specification deleted successfully");
        }

        #endregion


        #region GET BY PRODUCT VARIANT

        public async Task<List<ProductSpecificationSM>> GetByVariantIdAsync(long productVariantId)
        {
            var exists = await _apiDbContext.ProductVariant
                .AnyAsync(x => x.Id == productVariantId);

            if (!exists)
                throw new SiffrumException(
                    ApiErrorTypeSM.InvalidInputData_NoLog,
                    "Product variant not found");
            var ids = await _apiDbContext.ProductSpecifications.AsNoTracking()
                .Where(x => x.ProductVariantId == productVariantId).Select(x => x.Id).ToListAsync();
            var response = new List<ProductSpecificationSM>();
            if(ids.Count == 0)
            {
                return response;
            }
            foreach (var id in ids) {
                var spec = await GetByIdAsync(id);
                if (spec != null)
                {
                    response.Add(spec);
                }
            }
            return response;
            /*return await _apiDbContext.ProductSpecifications
                .AsNoTracking()
                .Where(x => x.ProductVariantId == productVariantId)
                .ProjectTo<ProductSpecificationSM>(_mapper.ConfigurationProvider)
                .ToListAsync();*/
        }

        public async Task<ProductSpecificationSM> GetByIdAsync(long specificationId)
        {
            var dm = await _apiDbContext.ProductSpecifications.FindAsync(specificationId);
            if(dm == null)
            {
                return null;
            }

            var sm = _mapper.Map<ProductSpecificationSM>(dm);
            return sm;
        }

        #endregion


        #region ADMIN ONLY — GET ALL

        public async Task<List<ProductSpecificationSM>> GetAllForAdmin(int skip, int top)
        {
            return await _apiDbContext.ProductSpecifications
                .AsNoTracking()
                .Skip(skip)
                .Take(top)
                .ProjectTo<ProductSpecificationSM>(_mapper.ConfigurationProvider)
                .ToListAsync();
            /*return await _apiDbContext.ProductSpecifications
                .AsNoTracking()
                .Skip(skip)
                .Take(top)
                .ProjectTo<ProductSpecificationSM>(_mapper.ConfigurationProvider)
                .ToListAsync();*/
        }

        public async Task<IntResponseRoot> GetAllForAdminCount()
        {
            var count = await _apiDbContext.ProductSpecifications.CountAsync();

            return new IntResponseRoot(count, "Total Specifications");
        }

        #endregion
    }
}
