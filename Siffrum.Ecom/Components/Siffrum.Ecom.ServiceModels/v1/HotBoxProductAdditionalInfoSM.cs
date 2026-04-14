namespace Siffrum.Ecom.ServiceModels.v1
{
    public class HotBoxProductAdditionalInfoSM
    {
        public List<ProductImagesSM> Images { get; set; }
        public List<ProductTagSM> ProductTags { get; set; }
        public List<ProductFaqSM> ProductFaqs { get; set; }
        public List<ProductRatingSM> ProductRatings { get; set; }
        public ProductUnitSM ProductUnit { get; set; }
        public ProductNutritionDataSM Nutrition { get; set; }
        public List<ProductToppingSM> Toppings { get; set; }
        public AddonProductResponseSM Addons { get; set; }

    }
}