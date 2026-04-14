using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Siffrum.Ecom.DomainModels.v1
{
    [Table("product_units")]
    public class ProductUnitDM
    {
        [Key]
        [Column("id")]
        public long Id { get; set; }

        [ForeignKey(nameof(ProductVariants))]
        [Required]
        [Column("product_variant_id")]
        public long ProductVariantId { get; set; }
        public virtual ProductVariantDM ProductVariants { get; set; }

        public decimal Quantity { get; set; }

        [ForeignKey(nameof(Unit))]
        [Required]
        [Column("unit_id")]
        public long UnitId { get; set; }
        public virtual UnitDM Unit { get; set; }


    }
}
