using AutoMapper;
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
    public class ProductFaqProcess : SiffrumBalOdataBase<ProductFaqSM>
    {
        private readonly ILoginUserDetail _loginUserDetail;
        public ProductFaqProcess(
            IMapper mapper,
            ApiDbContext apiDbContext,
            ILoginUserDetail loginUserDetail)
            : base(mapper, apiDbContext)
        {
            _loginUserDetail = loginUserDetail;
        }

        #region OData
        public override async Task<IQueryable<ProductFaqSM>> GetServiceModelEntitiesForOdata()
        {
            IQueryable<ProductFaqDM> entitySet = _apiDbContext.ProductFaq
                .AsNoTracking();

            return await base.MapEntityAsToQuerable<ProductFaqDM, ProductFaqSM>(_mapper, entitySet);
        }
        #endregion

        #region CREATE

        #region Create List Of Product Faqs

        public async Task<BoolResponseRoot> CreateListAsync(long productVariantId, List<ProductFaqSM> objSM)
        {
            if (objSM == null)
            {
                throw new SiffrumException(ApiErrorTypeSM.ModelError_NoLog, "Product Faq Data is required");

            }
            var existingProductVariant = await _apiDbContext.ProductVariant.FindAsync(productVariantId);
            if (existingProductVariant == null)
            {
                throw new SiffrumException(ApiErrorTypeSM.InvalidInputData_NoLog, "Product Variant not found");
            }
            if (objSM.Count > 0)
            {
                foreach (var item in objSM)
                {
                    item.ProductVariantId = productVariantId;
                    await CreateAsync(item);
                }
                return new BoolResponseRoot(true, "Product Faqs created successfully");
            }            
            throw new SiffrumException(ApiErrorTypeSM.Fatal_Log, "provide details for Product faqs");
        }

        #endregion Create List Of Product Faqs

        public async Task<BoolResponseRoot> CreateAsync(ProductFaqSM objSM)
        {
            if (objSM == null)
                throw new SiffrumException(ApiErrorTypeSM.ModelError_NoLog, "Product Faq Data is required");

            //Todo validate Product Variant here
            var dm = _mapper.Map<ProductFaqDM>(objSM);
            dm.CreatedAt = DateTime.UtcNow;
            dm.CreatedBy = _loginUserDetail.LoginId;

            await _apiDbContext.ProductFaq.AddAsync(dm);

            if (await _apiDbContext.SaveChangesAsync() > 0)
            {
                return new BoolResponseRoot(true, "Product Faq created successfully");
            }

            throw new SiffrumException(ApiErrorTypeSM.Fatal_Log, "Failed to create product faq");
        }
        #endregion       

        #region READ

        #region Get by Id

        public async Task<ProductFaqSM?> GetByIdAsync(long id)
        {
            var dm = await _apiDbContext.ProductFaq
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id);
            if (dm != null)
            {
                var sm = _mapper.Map<ProductFaqSM>(dm);
                return sm;
            }

            return null;
        }
        #endregion Get by Id

        #region Get All and Count

        public async Task<List<ProductFaqSM>> GetAllProductFaqs(int skip, int top)
        {
            var ids = await _apiDbContext.ProductFaq.AsNoTracking()
                .Skip(skip)
                .Take(top)
                .Select(x => x.Id)
                .ToListAsync();
            var response = new List<ProductFaqSM>();

            foreach (var id in ids)
            {
                var unit = await GetByIdAsync(id);
                if (unit != null)
                    response.Add(unit);
            }

            return response;
        }

        public async Task<IntResponseRoot> GetCount()
        {
            var count = await _apiDbContext.ProductFaq.AsNoTracking().CountAsync();

            return new IntResponseRoot(count, "Total Units");
        }

        #endregion Get All and Count

        #region Get All By Product Variant Id

        public async Task<List<ProductFaqSM>> GetAllProductFaqsByProductVariantId(long productVariantId, int skip, int top)
        {
            var ids = await _apiDbContext.ProductFaq.AsNoTracking()
                .Where(x=>x.ProductVariantId == productVariantId)
                .Skip(skip)
                .Take(top)
                .Select(x => x.Id)
                .ToListAsync();
            var response = new List<ProductFaqSM>();

            foreach (var id in ids)
            {
                var unit = await GetByIdAsync(id);
                if (unit != null)
                    response.Add(unit);
            }

            return response;
        }

        public async Task<IntResponseRoot> GetCountOfProductVariantId(long productVariantId)
        {
            var count = await _apiDbContext.ProductFaq.AsNoTracking().Where(x => x.ProductVariantId == productVariantId).CountAsync();

            return new IntResponseRoot(count, "Total Product Faqs");
        }

        #endregion Get All By Product Variant Id        

        #endregion

        #region UPDATE
        public async Task<ProductFaqSM?> UpdateAsync(long id, ProductFaqSM objSM)
        {
            if (objSM == null)
                throw new SiffrumException(
                    ApiErrorTypeSM.ModelError_NoLog,
                    "Faq data is required"
                );

            var dm = await _apiDbContext.ProductFaq
                .FirstOrDefaultAsync(x => x.Id == id);

            if (dm == null)
            {
                throw new SiffrumException(
                    ApiErrorTypeSM.InvalidInputData_NoLog,
                    $"Product Faq with Id: {id} not found",
                    "Product Faq not found"
                );
            }         

            // 🔑 Preserve immutable fields
            objSM.Id = dm.Id;
            objSM.ProductVariantId = dm.ProductVariantId;

            // ✅ CORRECT: Map INTO existing tracked entity
            _mapper.Map(objSM, dm);

            // Audit fields
            dm.UpdatedAt = DateTime.UtcNow;
            dm.UpdatedBy = _loginUserDetail.LoginId;
            if (await _apiDbContext.SaveChangesAsync() > 0)
            {
                return await GetByIdAsync(id);
            }

            throw new SiffrumException(
                ApiErrorTypeSM.Fatal_Log,$"Product faq with Id: {id} failed to update",
                "Failed to update product faq"
            );
        }

        #endregion

        #region DELETE (SOFT DELETE)
        public async Task<DeleteResponseRoot> DeleteAsync(long id)
        {
            var dm = await _apiDbContext.ProductFaq.FirstOrDefaultAsync(x => x.Id == id);
            if (dm == null)
            {
                throw new SiffrumException(ApiErrorTypeSM.InvalidInputData_Log, "Product Faq not found");
            }
            _apiDbContext.ProductFaq.Remove(dm);

            if (await _apiDbContext.SaveChangesAsync() > 0)
            {
                return new DeleteResponseRoot(true, "Product Faq deleted successfully");
            }

            throw new SiffrumException(ApiErrorTypeSM.Fatal_Log, "Failed to delete unit");
        }
        #endregion
    }
}
