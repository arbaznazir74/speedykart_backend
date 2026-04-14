using Siffrum.Ecom.ServiceModels.Foundation.Base;

namespace Siffrum.Ecom.ServiceModels.v1
{
    public class ComboProductSM : SiffrumServiceModelBase<long>
    {
        public long CategoryId { get; set; }
        public string? CategoryName { get; set; }
        public bool IsInHotBox { get; set; }
        public ProductIdsSM ProductIds { get; set; }
        public List<ProductImageDataSM> ProductData { get; set; }
        public int TotalProducts { get; set; }
        public int BestFor { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public ComboJsonDataSM JsonDetails { get; set; }
    }
}
