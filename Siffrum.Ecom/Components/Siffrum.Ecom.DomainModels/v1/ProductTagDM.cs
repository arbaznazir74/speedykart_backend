using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Siffrum.Ecom.DomainModels.v1
{
    [Table("product_tag")]
    public class ProductTagDM
    {
        [Key]
        [Column("id")]
        public long Id { get; set; }

        [Required]
        [Column("product_variant_id")]
        public long ProductVariantId { get; set; }

        [ForeignKey(nameof(ProductVariantId))]
        public virtual ProductVariantDM ProductVariant { get; set; }

        [Required]
        [Column("tag_id")]
        public long TagId { get; set; }

        [ForeignKey(nameof(TagId))]
        public virtual TagDM Tag { get; set; }
    }
}
