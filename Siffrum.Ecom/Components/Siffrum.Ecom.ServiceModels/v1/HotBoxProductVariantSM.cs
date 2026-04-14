namespace Siffrum.Ecom.ServiceModels.v1
{
    public class HotBoxProductVariantSM
    {
        public ProductVariantSM ProductVariant { get; set; }

        public HotBoxProductAdditionalInfoSM ProductAdditionalInfo { get; set; }

        public List<VariantInfoSM> AllVariants { get; set; }

        public string? Tags { get; set; }
    }
}
