using System.ComponentModel.DataAnnotations.Schema;

namespace Siffrum.Ecom.DomainModels.v1
{
    [Table("cart_items")]
    public class CartItemDM 
    {
        [Column("id")]
        public long Id { get; set; }
         
        [ForeignKey(nameof(Cart))]
        [Column("cart_id")]
        public long CartId { get; set; }
        public virtual CartDM Cart { get; set; }

        [ForeignKey(nameof(ProductVariant))]
        [Column("product_variant_id")]
        public long ProductVariantId { get; set; }
        public virtual ProductVariantDM ProductVariant { get; set; }

        [Column("qty")]
        public int Quantity { get; set; }

        [Column("unit_price")]
        public decimal UnitPrice { get; set; }

        [Column("total_price")]
        public decimal TotalPrice { get; set; }

        [Column("selected_toppings_json")]
        public string? SelectedToppingsJson { get; set; }

        [Column("selected_addons_json")]
        public string? SelectedAddonsJson { get; set; }

        [Column("toppings_total")]
        public decimal ToppingsTotal { get; set; }

        [Column("addons_total")]
        public decimal AddonsTotal { get; set; }
    }
}