using Siffrum.Ecom.DomainModels.Foundation.Base;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Siffrum.Ecom.DomainModels.v1
{
    [Table("product_toppings")]
    public class ProductToppingDM : SiffrumDomainModelBase<long>
    {
        [ForeignKey(nameof(Product))]
        [Required]
        [Column("product_id")]
        public long ProductId { get; set; }
        public virtual ProductDM Product { get; set; }

        [ForeignKey(nameof(Topping))]
        [Required]
        [Column("topping_id")]
        public long ToppingId { get; set; }
        public virtual ToppingDM Topping { get; set; }

        [Required]
        [Column("price")]
        public decimal Price { get; set; }

        [Column("is_default")]
        public bool IsDefault { get; set; }
    }
}
