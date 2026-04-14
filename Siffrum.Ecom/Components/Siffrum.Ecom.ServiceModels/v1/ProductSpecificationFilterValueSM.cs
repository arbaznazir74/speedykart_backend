namespace Siffrum.Ecom.ServiceModels.v1
{
    public class ProductSpecificationFilterValueSM
    {
        public long Id { get; set; }
        public ProductSpecificationFilterSM ProductSpecificationFilter { get; set; }
        public ProductSpecificationValueSM ProductSpecificationValue { get; set; }
        public long ProductVariantId { get; set; }
    }
}
