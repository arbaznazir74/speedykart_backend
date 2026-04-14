using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Siffrum.Ecom.DomainModels.Foundation.Base;

namespace Siffrum.Ecom.DomainModels.v1
{
    [Table("units")]
    public class UnitDM : SiffrumDomainModelBase<long>
    {

        [Required]
        [Column("name")]
        [MaxLength(191)]
        public string Name { get; set; } 

        [Required]
        [Column("short_code")]
        [MaxLength(191)]
        public string ShortCode { get; set; } 

        [Column("parent_id")]
        public long? ParentId { get; set; }

        [Column("conversion")]
        public int? Conversion { get; set; }

        [Column("seller_id")]
        public long? SellerId { get; set; }

        public ICollection<ProductVariantDM> ProductVariants { get; set; }
    }
}
