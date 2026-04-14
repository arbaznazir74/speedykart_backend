namespace Siffrum.Ecom.ServiceModels.v1
{
    public class VariantInfoSM
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public decimal? DiscountedPrice { get; set; }
        public decimal? Stock { get; set; }
        public string? UnitName { get; set; }
        public string? ImageBase64 { get; set; }
        public string? NetworkImage { get; set; }
    }
}
