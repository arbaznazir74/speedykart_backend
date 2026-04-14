namespace Siffrum.Ecom.ServiceModels.v1
{
    public class SpeedyMartProductAdditionalInfoSM
    {
        public List<ProductImagesSM> Images { get; set; }
        public List<ProductTagSM> ProductTags { get; set; }
        public List<ProductFaqSM> ProductFaqs { get; set; }
        public List<ProductRatingSM> ProductRatings { get; set; }
        public List<ProductSpecificationSM> Specifications { get; set; }
        public List<ProductSpecificationFilterValueSM> Filters { get; set; } //Filters>
        public List<ProductToppingSM> Toppings { get; set; }

    }
}