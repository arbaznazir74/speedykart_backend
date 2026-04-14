namespace Siffrum.Ecom.ServiceModels.v1
{
    public class ProductUnitSM
    {
        public long Id { get; set; }

        public long ProductVariantId { get; set; }
        public string? ProductVariantName { get; set; }

        public long UnitId { get; set; }

        public string? UnitName { get; set; }

        public decimal Quantity { get; set; }
    }
}
