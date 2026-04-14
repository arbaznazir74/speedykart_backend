using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Siffrum.Ecom.BAL.ExceptionHandler;
using Siffrum.Ecom.BAL.Foundation.Base;
using Siffrum.Ecom.DAL.Context;
using Siffrum.Ecom.DomainModels.v1;
using Siffrum.Ecom.ServiceModels.Foundation.Base.Enums;
using Siffrum.Ecom.ServiceModels.v1;

namespace Siffrum.Ecom.BAL.Product
{
    public class ProductUnitProcess : SiffrumBalBase
    {

        public ProductUnitProcess(
            IMapper mapper,
            ApiDbContext apiDbContext)
            : base(mapper, apiDbContext)
        {
        }


        #region Get Product Unit By Id      

        public async Task<ProductUnitSM> GetByProductVariantId(long productVariantId)
        {
            var dm = await _apiDbContext.ProductUnit.AsNoTracking()
                .FirstOrDefaultAsync(x => x.ProductVariantId == productVariantId);
            if (dm != null)
            {
                var sm = _mapper.Map<ProductUnitSM>(dm);
                UnitDM unit = await _apiDbContext.Unit.FindAsync(dm.UnitId);
                if (unit != null)
                {
                    sm.UnitName = unit.Name;
                }
                var productVariant = await _apiDbContext.ProductVariant.FindAsync(dm.ProductVariantId);
                if(productVariant != null)
                {
                    sm.ProductVariantName = productVariant.Name;
                    
                }
                return sm;
            }
            return null;
        }

        #region Get By Id
        public async Task<ProductUnitSM> GetByPId(long id)
        {
            var dm = await _apiDbContext.ProductUnit.FindAsync(id);
            if (dm != null)
            {
                var sm = _mapper.Map<ProductUnitSM>(dm);
                var unit = await _apiDbContext.Unit.FindAsync(dm.UnitId);
                if (unit != null)
                {
                    sm.UnitName = unit.Name;
                }
                var productVariant = await _apiDbContext.ProductVariant.FindAsync(dm.ProductVariantId);
                if (productVariant != null)
                {
                    sm.ProductVariantName = productVariant.Name;

                }
                return sm;
            }
            return null;
        }

        #endregion Get By Id


        #endregion Get Product Unit By Id

        #region Add Product Unit

        public async Task<ProductUnitSM> AddProductUnit(ProductUnitSM objSM)
        {
            if (objSM == null)
            {
                throw new SiffrumException(ApiErrorTypeSM.ModelError_NoLog, "Product unit data is required");
            }
            var productVariant = await _apiDbContext.ProductVariant.FindAsync(objSM.ProductVariantId);
            var unit = await _apiDbContext.Unit.AsNoTracking().Where(x=>x.Id == objSM.UnitId).FirstOrDefaultAsync();
            if(productVariant == null)
            {
                throw new SiffrumException(ApiErrorTypeSM.Fatal_Log,$"Adding unit to product with Id {objSM.ProductVariantId} is not found", "Product variant not found");
            }

            if (unit == null)
            {
                throw new SiffrumException(ApiErrorTypeSM.Fatal_Log, $"Adding unit with Id : { objSM.UnitId} to product with Id {objSM.ProductVariantId} is not found", "Unit not found");
            }
            var productUnit = await _apiDbContext.ProductUnit.FirstOrDefaultAsync(x => x.ProductVariantId == objSM.ProductVariantId && x.UnitId == objSM.UnitId);
            if(productUnit != null)
            {
                throw new SiffrumException(ApiErrorTypeSM.Fatal_Log, $"Product with Id {objSM.ProductVariantId} already has unit with Id {objSM.UnitId}", "Product unit already exists");
            }
            var dm = _mapper.Map<ProductUnitDM>(objSM);
            _apiDbContext.ProductUnit.Add(dm);
            if (await _apiDbContext.SaveChangesAsync() > 0)
            {
                var sm = _mapper.Map<ProductUnitSM>(dm);
                sm.UnitName = unit?.Name;
                sm.ProductVariantName = dm.ProductVariants?.Name;
                return sm;
            }
            throw new SiffrumException(ApiErrorTypeSM.Fatal_Log, "Failed to add product unit");
        }

        #endregion Add Product Unit

        #region Delete Product Unit

        public async Task<bool> DeleteProductUnit(long id)
        {
            var dm = await _apiDbContext.ProductUnit
               .FirstOrDefaultAsync(x => x.Id == id);
            if (dm != null)
            {
                
                _apiDbContext.ProductUnit.Remove(dm);
                if (await _apiDbContext.SaveChangesAsync() > 0)
                {
                    return true;
                }
            }
            throw new SiffrumException(ApiErrorTypeSM.Fatal_Log, "Product Unit not found");
        }

        #endregion Delete Product Unit
    }
}
