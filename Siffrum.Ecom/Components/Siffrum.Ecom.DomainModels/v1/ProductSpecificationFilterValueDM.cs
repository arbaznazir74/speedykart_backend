using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Siffrum.Ecom.DomainModels.v1
{
    [Table("product_specification_filter_value")]
    public class ProductSpecificationFilterValueDM
    {

        [Column("id")]
        public long Id { get; set; }

        [ForeignKey(nameof(ProductFilter))]
        [Required]
        [Column("product_filter_id")]
        public long ProductFilterId { get; set; }
        public virtual ProductSpecificationFilterDM ProductFilter { get; set; }

        [ForeignKey(nameof(ProductFilterValue))]
        [Required]
        [Column("product_filter_value_id")]
        public long ProductFilterValueId { get; set; }
        public virtual ProductSpecificationValueDM ProductFilterValue { get; set; }

        [ForeignKey(nameof(ProductVariant))]
        [Required]
        [Column("product_variant_id")]
        public long ProductVariantId { get; set; }
        public virtual ProductVariantDM ProductVariant { get; set; }
       

    }
}
