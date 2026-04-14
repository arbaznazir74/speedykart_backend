using Siffrum.Ecom.ServiceModels.Enums;

namespace Siffrum.Ecom.ServiceModels.v1
{
    public class OrderItemDetailSM
    {
        public long Id { get; set; }
        public long OrderId { get; set; }
        public string? CustomerName { get; set; }

        // Product info
        public long ProductId { get; set; }
        public string? BaseProductName { get; set; }

        // Variant info
        public long ProductVariantId { get; set; }
        public string? VariantName { get; set; }
        public string? VariantImageBase64 { get; set; }
        public string? NetworkVariantImage { get; set; }
        public string? Indicator { get; set; }

        // Pricing
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }

        // Status
        public PaymentStatusSM? PaymentStatus { get; set; }
        public OrderStatusSM? OrderStatus { get; set; }
        public PaymentModeSM? PaymentMode { get; set; }

        // Toppings & Addons (parsed)
        public List<OrderToppingDetailSM>? Toppings { get; set; }
        public List<OrderAddonDetailSM>? Addons { get; set; }
    }

    public class OrderToppingDetailSM
    {
        public long ToppingId { get; set; }
        public string? ToppingName { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
    }

    public class OrderAddonDetailSM
    {
        public long AddonProductId { get; set; }
        public string? AddonName { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public long CategoryId { get; set; }
        public string? CategoryName { get; set; }
    }
}
