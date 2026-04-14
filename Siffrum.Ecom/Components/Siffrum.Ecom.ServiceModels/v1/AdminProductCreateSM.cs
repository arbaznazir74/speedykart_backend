namespace Siffrum.Ecom.ServiceModels.v1
{
    public class AdminProductCreateSM
    {
        public ProductSM Product { get; set; }
        public List<long> SellerIds { get; set; }
    }
}
