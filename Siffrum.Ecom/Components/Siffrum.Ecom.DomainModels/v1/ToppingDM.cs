using Siffrum.Ecom.DomainModels.Enums;
using Siffrum.Ecom.DomainModels.Foundation.Base;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Siffrum.Ecom.DomainModels.v1
{
    [Table("toppings")]
    public class ToppingDM : SiffrumDomainModelBase<long>
    {
        [Required]
        [Column("name")]
        [MaxLength(191)]
        public string Name { get; set; }

        [Column("price")]
        public decimal Price { get; set; }

        [Column("image")]
        [MaxLength(500)]
        public string? Image { get; set; }

        [Column("status")]
        public StatusDM Status { get; set; }

        [Column("suggested_by_seller_id")]
        public long? SuggestedBySellerId { get; set; }
    }
}
