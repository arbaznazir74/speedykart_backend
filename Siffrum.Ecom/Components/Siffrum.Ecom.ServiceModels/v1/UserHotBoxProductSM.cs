using Siffrum.Ecom.ServiceModels.Enums;

namespace Siffrum.Ecom.ServiceModels.v1
{
    public class UserHotBoxProductSM
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string ImageBase64 { get; set; }
        public string? NetworkImage { get; set; }
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public decimal? DiscountedPercentage { get; set; } 
        public decimal? DiscountedPrice { get; set; } 
        public short Rate { get; set; }
        public int TotalRatings { get; set; }
        public ProductIndicatorSM Indicator { get; set; }
        public string? ServeSize { get; set; }
        public decimal? Proteins { get; set; }
        public int? TotalAllowedQuantity { get; set; }
        
        public bool IsCodAllowed { get; set; } 
        public decimal? Stock { get; set; } = 0.00m;
        public List<ProductTagSM> ProductTags { get; set; }

        public bool IsFreshArrival { get; set; }
        public bool IsBestSeller { get; set; }

        public long? CategoryId { get; set; }
        public List<VariantInfoSM> Variants { get; set; } = new();
        public List<string> ProductData
        {
            get
            {
                var data = new List<string>();

                if (!string.IsNullOrWhiteSpace(ServeSize))
                    data.Add(ServeSize);

                if (Proteins != null)
                    data.Add($"{Proteins} grams");

                return data;
            }
        }
    }

}
