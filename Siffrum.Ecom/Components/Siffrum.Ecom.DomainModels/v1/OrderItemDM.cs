using System.ComponentModel.DataAnnotations.Schema;

namespace Siffrum.Ecom.DomainModels.v1
{
    [Table("order_items")]
    public class OrderItemDM
    {
        [Column("id")]
        public long Id { get; set; }

        [ForeignKey(nameof(Order))]
        [Column("order_id")]
        public long OrderId { get; set; }

        public virtual OrderDM Order { get; set; }

        [ForeignKey(nameof(ProductVariant))]

        [Column("product_variant_id")]
        public long ProductVariantId { get; set; }
        public virtual ProductVariantDM ProductVariant { get; set; }

        [Column("quantity")]
        public int Quantity { get; set; }

        [Column("unit_price", TypeName = "decimal(18,2)")]
        public decimal UnitPrice { get; set; }

        [Column("total_price", TypeName = "decimal(18,2)")]
        public decimal TotalPrice { get; set; }

        [Column("selected_toppings")]
        public string? SelectedToppings { get; set; }

        [Column("selected_addons")]
        public string? SelectedAddons { get; set; }
    }
}
