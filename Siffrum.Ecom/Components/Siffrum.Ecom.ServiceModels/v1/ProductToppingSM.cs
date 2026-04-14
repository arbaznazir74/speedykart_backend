using Siffrum.Ecom.ServiceModels.Foundation.Base;

namespace Siffrum.Ecom.ServiceModels.v1
{
    public class ProductToppingSM : SiffrumServiceModelBase<long>
    {
        public long ProductId { get; set; }
        public long ToppingId { get; set; }
        public string? ToppingName { get; set; }
        public string? ProductName { get; set; }
        public string? SellerName { get; set; }
        public long SellerId { get; set; }
        public decimal Price { get; set; }
        public bool IsDefault { get; set; }
    }
}
