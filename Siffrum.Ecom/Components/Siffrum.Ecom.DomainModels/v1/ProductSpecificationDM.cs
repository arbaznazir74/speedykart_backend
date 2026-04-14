using Siffrum.Ecom.DomainModels.Foundation.Base;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Siffrum.Ecom.DomainModels.v1
{
    [Table("product_specifications")]
    public class ProductSpecificationDM : SiffrumDomainModelBase<long>
    {

        [Column("key")]
        public string Key { get; set; }
        [Column("value")]
        public string Value { get; set; }
        [ForeignKey(nameof(ProductVariant))]
        [Required]
        [Column("product_variant_id")]
        public long ProductVariantId { get; set; }
        public virtual ProductVariantDM? ProductVariant { get; set; }
       

    }
}
