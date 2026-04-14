using Siffrum.Ecom.ServiceModels.Enums;
using Siffrum.Ecom.ServiceModels.Foundation.Base;

namespace Siffrum.Ecom.ServiceModels.v1
{
    public class ProductVariantSM : SiffrumServiceModelBase<long>
    {
        public string Name { get; set; }
        public int TotalAllowedQuantity { get; set; }
        public bool IsTaxIncludedInPrice { get; set; }
        public ProductStatusSM Status { get; set; }
        public ProductReturnPolicySM ReturnPolicy { get; set; }

        public decimal Measurement { get; set; }

        public decimal Price { get; set; }

        public decimal DiscountedPrice { get; set; } = 0.00m;

        public decimal Stock { get; set; } = 0.00m;

        public string SKU { get; set; } = string.Empty;

        public DateTime? DeletedAt { get; set; }

        public long ProductId { get; set; }

        public long SellerId { get; set; }

        public ProductIndicatorSM Indicator { get; set; }

        public string? Manufacturer { get; set; }

        public string? MadeIn { get; set; }

        public bool IsCancelable { get; set; }

        public string? ImageBase64 { get; set; }
        public string? NetworkImage { get; set; }
        public string Description { get; set; } 

        public PlatformTypeSM PlatformType { get; set; } 


        public int ReturnDays { get; set; }

        public bool IsUnlimitedStock { get; set; }

        public bool IsCodAllowed { get; set; }

        public string FssaiLicNo { get; set; } 

        public string? Barcode { get; set; }

        public string? MetaTitle { get; set; }

        public string? MetaKeywords { get; set; }

        public string? SchemaMarkup { get; set; }

        public string? MetaDescription { get; set; }

        public int? ViewCount { get; set; }

        public long? CategoryId { get; set; }
    }
}
