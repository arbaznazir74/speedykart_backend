using Siffrum.Ecom.DomainModels.Foundation.Base;
using Siffrum.Ecom.DomainModels.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace Siffrum.Ecom.DomainModels.v1
{
    [Table("offers_and_coupons")]
    public class OffersAndCouponsDM : SiffrumDomainModelBase<long>
    {
        [Column("name")]
        public string Name { get; set; }
        [Column("description")]
        public string? Description { get; set; }
        [Column("base64path")]
        public string Base64Path { get; set; }
        [Column("extension_type")]
        public  ExtensionTypeDM ExtensionType { get; set; }
        [Column("percentage")]        
        public decimal? Percentage { get; set; }

        [Column("platform_type")]
        public PlatformTypeDM PlatformType { get; set; }

        [Column("offer_value")]
        public decimal? OfferValue { get; set; }
        [Column("min_amount")]
        public decimal MinAmount { get; set; }
        [Column("max_discount_amount")]
        public decimal MaxDiscountAmount { get; set; }
    }
}
