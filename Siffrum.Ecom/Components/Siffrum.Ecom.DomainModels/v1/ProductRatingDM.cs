using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Siffrum.Ecom.DomainModels.Enums;
using Siffrum.Ecom.DomainModels.Foundation.Base;

namespace Siffrum.Ecom.DomainModels.v1
{
    [Table("product_ratings")]
    public class ProductRatingDM : SiffrumDomainModelBase<long>
    {
        [ForeignKey(nameof(UserDM))]
        [Required]
        [Column("user_id")]
        public long UserId { get; set; }
        public virtual UserDM User { get; set; }

        [Required]
        [Column("rate")]
        public short Rate { get; set; }

        [Required]
        [Column("review")]
        public string Review { get; set; } = string.Empty;

        [Column("status")]
        public StatusDM Status { get; set; }
        [ForeignKey(nameof(ProductVariantDM))]
        [Required]
        [Column("product_variant_id")]
        public long ProductVariantId { get; set; }
        public virtual ProductVariantDM ProductVariant { get; set; }
        public ICollection<RatingImagesDM> Images { get; set; } = new List<RatingImagesDM>();
    }
}
