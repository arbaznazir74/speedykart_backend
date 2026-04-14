using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Siffrum.Ecom.DomainModels.Enums;
using Siffrum.Ecom.DomainModels.Foundation.Base;

namespace Siffrum.Ecom.DomainModels.v1
{
    [Table("promo_codes")]
    public class PromoCodeDM : SiffrumDomainModelBase<long>
    {

        [Required]
        [Column("code")]
        [MaxLength(50)]
        public string Code { get; set; }

        [Required]
        [Column("type")]
        public CouponTypeDM Type { get; set; }

        [Required]
        [Column("discount_value")]
        public decimal DiscountValue { get; set; }

        [Column("max_discount_amount")]
        public decimal? MaxDiscountAmount { get; set; }

        [Column("minimum_cart_amount")]
        public decimal? MinimumCartAmount { get; set; }

        [Required]
        [Column("usage_limit")]
        public int? UsageLimit { get; set; }

        
        [Column("used_count")]
        public int? UsedCount { get; set; } 

        [Column("usage_per_user_limit")]
        public int? UsagePerUserLimit { get; set; }

        [Required]
        [Column("is_active")]
        public bool IsActive { get; set; } = true;

        [Required]
        [Column("is_first_order_only")]
        public bool IsFirstOrderOnly { get; set; } = false;

        [Column("platform_type")]
        public PlatformTypeDM? PlatformType { get; set; }

        public ICollection<UserPromocodesDM> UserPromocodes { get; set; } 

    }
}
