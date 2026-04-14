using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
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
    public class ProductNutritionProcess : SiffrumBalOdataBase<ProductNutritionDataSM>
    {
        private readonly ILoginUserDetail _loginUserDetail;

        public ProductNutritionProcess(
            IMapper mapper,
            ApiDbContext apiDbContext,
            ILoginUserDetail loginUserDetail)
            : base(mapper, apiDbContext)
        {
            _loginUserDetail = loginUserDetail;
        }

        #region ODATA
        public override async Task<IQueryable<ProductNutritionDataSM>> GetServiceModelEntitiesForOdata()
        {
            IQueryable<ProductNutritionDataDM> entitySet =
                _apiDbContext.ProductNutritionData.AsNoTracking();

            return await base.MapEntityAsToQuerable<ProductNutritionDataDM, ProductNutritionDataSM>(
                _mapper, entitySet);
        }
        #endregion

        #region ADD / UPDATE NUTRITION
        public async Task<BoolResponseRoot> AddOrUpdateAsync(
            long productVariantId,
            ProductNutritionDataSM sm)
        {
            var existingProduct = await _apiDbContext.ProductVariant.FindAsync(productVariantId);
            if (existingProduct == null)
            {
                throw new SiffrumException(ApiErrorTypeSM.Fatal_Log,$"Product With Id {productVariantId} Not Found for adding nutrient details", "Product not found");
            }
            var existing = await _apiDbContext.ProductNutritionData
                .FirstOrDefaultAsync(x => x.ProductVariantId == productVariantId);

            if (existing == null)
            {
                var dm = _mapper.Map<ProductNutritionDataDM>(sm);

                dm.ProductVariantId = productVariantId;
                dm.IngredientsJson = SerializeIngredients(sm.Ingredients);

                await _apiDbContext.ProductNutritionData.AddAsync(dm);
            }
            else
            {
                // Map only scalar fields
                _mapper.Map(sm, existing);

                existing.IngredientsJson = SerializeIngredients(sm.Ingredients);
            }

            await _apiDbContext.SaveChangesAsync();

            return new BoolResponseRoot(true, "Nutrition saved successfully");
        }

        #endregion

        #region GET BY PRODUCT VARIANT

        public async Task<ProductNutritionDataSM> GetByVariantIdAsync(long productVariantId)
        {
            var existingProduct = await _apiDbContext.ProductVariant.FindAsync(productVariantId);
            if (existingProduct == null)
            {
                throw new SiffrumException(ApiErrorTypeSM.Fatal_Log, $"Product With Id {productVariantId} Not Found for getting nutrient details", "Product not found");
            }
            var dm = await _apiDbContext.ProductNutritionData
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.ProductVariantId == productVariantId);

            if (dm == null)
            {
                return null;
               /* throw new SiffrumException(
                    ApiErrorTypeSM.InvalidInputData_Log,
                    "Nutrition data not found");*/
            }

            var sm = _mapper.Map<ProductNutritionDataSM>(dm);

            sm.Ingredients = DeserializeIngredients(dm.IngredientsJson);

            return sm;
        }


        public async Task<ProductNutritionDataSM> GetByIdAsync(long id)
        {
            var dm = await _apiDbContext.ProductNutritionData.FindAsync(id);

            if (dm == null)
            {
                return null;
            }

            var sm = _mapper.Map<ProductNutritionDataSM>(dm);

            sm.Ingredients = DeserializeIngredients(dm.IngredientsJson);

            return sm;
        }

        #endregion

        #region DELETE

        public async Task<DeleteResponseRoot> DeleteAsync(long id)
        {
            var dm = await _apiDbContext.ProductNutritionData
                .FirstOrDefaultAsync(x => x.Id == id);

            if (dm == null)
            {
                throw new SiffrumException(
                    ApiErrorTypeSM.InvalidInputData_Log,
                    "Nutrition data not found");
            }

            _apiDbContext.ProductNutritionData.Remove(dm);

            await _apiDbContext.SaveChangesAsync();

            return new DeleteResponseRoot(true, "Nutrition deleted successfully");
        }

        #endregion

        #region ADMIN GET ALL

        public async Task<List<ProductNutritionDataSM>> GetAllForAdmin(int skip, int top)
        {
            var ids = await _apiDbContext.ProductNutritionData
                .AsNoTracking()
                .Skip(skip)
                .Take(top)
                .Select(x => x.Id)
                .ToListAsync();

            var smList = new List<ProductNutritionDataSM>();

            foreach (var id in ids)
            {
                var sm = await GetByIdAsync(id);
                if (sm != null)
                    smList.Add(sm);
            }

            return smList;
        }

        #endregion

        #region ADMIN COUNT

        public async Task<IntResponseRoot> GetAllForAdminCount()
        {
            var count = await _apiDbContext.ProductNutritionData.CountAsync();

            return new IntResponseRoot(count,"Total Nutritions Data Count");
        }

        #endregion

        #region INGREDIENT SERIALIZATION

        private string? SerializeIngredients(List<IngredientsSM>? ingredients)
        {
            if (ingredients == null || !ingredients.Any())
                return null;

            return JsonConvert.SerializeObject(ingredients);
        }

        private List<IngredientsSM> DeserializeIngredients(string? json)
        {
            if (string.IsNullOrWhiteSpace(json))
                return new List<IngredientsSM>();

            return JsonConvert.DeserializeObject<List<IngredientsSM>>(json)
                   ?? new List<IngredientsSM>();
        }

        #endregion
    }
}
