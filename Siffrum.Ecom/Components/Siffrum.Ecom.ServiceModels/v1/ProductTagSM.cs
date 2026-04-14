namespace Siffrum.Ecom.ServiceModels.v1
{
    public class ProductTagSM
    {
        public long Id { get; set; }
        public long ProductVariantId { get; set; }
        public long TagId { get; set; }
        public string? Name { get; set; }
    }
}
