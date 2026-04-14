using Siffrum.Ecom.DomainModels.Enums;
using Siffrum.Ecom.DomainModels.Foundation.Base;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Siffrum.Ecom.DomainModels.v1
{
    [Table("product_banners")]
    public class ProductBannerDM 
    {
        [Key]
        [Column("id")]
        public long Id { get; set; }

        [ForeignKey(nameof(ProductVariant))]
        [Column("product_id")]
        public long ProductId { get; set; }
        public virtual ProductVariantDM ProductVariant { get; set; }
        [ForeignKey(nameof(Banner))]
        [Column("banner_id")]
        public long BannerId { get; set; }
        public virtual BannerDM Banner { get; set; }
    }
}
