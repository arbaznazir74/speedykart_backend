using Siffrum.Ecom.ServiceModels.Enums;
using Siffrum.Ecom.ServiceModels.Foundation.Base;

namespace Siffrum.Ecom.ServiceModels.v1
{
    public class ProductSM : SiffrumServiceModelBase<long>
    {

        public string Name { get; set; }

        public string Slug { get; set; }       

        public long SellerId { get; set; }
        public long CategoryId { get; set; }

        public string? CategoryName { get; set; }
        public long? BrandId { get; set; }

        public string? BrandName { get; set; }
        public decimal? TaxPercentage { get; set; }

        public string? Tags { get; set; }
    }
}
