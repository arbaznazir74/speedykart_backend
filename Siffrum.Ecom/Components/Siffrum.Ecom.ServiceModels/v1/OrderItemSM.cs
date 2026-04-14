using Siffrum.Ecom.ServiceModels.Enums;

namespace Siffrum.Ecom.ServiceModels.v1
{
    public class OrderItemSM
    {
        public long Id { get; set; }
        public long OrderId { get; set; }

        public string? CustomerName { get; set; }
        public long ProductVariantId { get; set; }

        public string? ProductName { get; set; }

        public string? ProductImage { get; set; }
        public string? NetworkProductImage { get; set; }
        public int Quantity { get; set; }

        public decimal UnitPrice { get; set; }

        public decimal TotalPrice { get; set; }

        public PaymentStatusSM? PaymentStatus { get; set; }

        public OrderStatusSM? OrderStatus { get; set; } 

        public PaymentModeSM? PaymentMode { get; set; }

        public List<SelectedToppingItem>? SelectedToppings { get; set; }
        public List<SelectedAddonItem>? SelectedAddons { get; set; }
    }
}
