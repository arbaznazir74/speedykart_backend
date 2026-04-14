using Siffrum.Ecom.DomainModels.Foundation.Base;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Siffrum.Ecom.DomainModels.v1
{
    [Table("product_faqs")]
    public class ProductFaqDM : SiffrumDomainModelBase<long>
    {
        [Required]
        [Column("question")]
        public string Question { get; set; }

        [Required]
        [Column("answer")]
        public string Answer { get; set; }

        [Column("status")]
        [MaxLength(191)]
        public bool Status { get; set; }        

        [ForeignKey(nameof(ProductVariant))]
        [Column("product_variant_id")]
        public long? ProductVariantId { get; set; }
        public virtual ProductVariantDM ProductVariant { get; set; }
    }
}
