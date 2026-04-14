namespace Siffrum.Ecom.ServiceModels.v1
{
    public class CategorySellerSM
    {
        public long Id { get; set; }
        public long CategoryId { get; set; }
        public long SellerId { get; set; }
        public string? SellerName { get; set; }
        public string? CategoryName { get; set; }
    }
}
