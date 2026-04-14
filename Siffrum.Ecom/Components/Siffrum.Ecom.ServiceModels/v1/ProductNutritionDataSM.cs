using Siffrum.Ecom.ServiceModels.Foundation.Base;

namespace Siffrum.Ecom.ServiceModels.v1
{
    public class ProductNutritionDataSM : SiffrumServiceModelBase<long>
    {
        public long ProductVariantId { get; set; }
        public string? ServeSize { get; set; }

        public decimal? Calories { get; set; }

        public decimal? Proteins { get; set; }

        public decimal? Carbohydrates { get; set; }

        public decimal? Fats { get; set; }

        public decimal? Sugars { get; set; }

        public decimal? Fiber { get; set; }

        public decimal? Sodium { get; set; }

        public List<IngredientsSM> Ingredients { get; set; }
    }
}
