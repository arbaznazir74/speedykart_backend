namespace Siffrum.Ecom.ServiceModels.v1
{
    public class SpeedyMartProductVariantSM
    {
        public ProductVariantSM ProductVariant { get; set; }

        public SpeedyMartProductAdditionalInfoSM ProductAdditionalInfo { get; set; }

        public List<VariantInfoSM> AllVariants { get; set; }

        public string? Tags { get; set; }
    }
}
