using Siffrum.Ecom.ServiceModels.Enums;

namespace Siffrum.Ecom.ServiceModels.v1
{
    public class OrderInvoiceSM
    {
        // Invoice meta
        public string InvoiceNumber { get; set; }
        public DateTime InvoiceDate { get; set; }
        public long OrderId { get; set; }
        public string OrderNumber { get; set; }

        // Seller info
        public InvoiceSellerSM Seller { get; set; }

        // Customer info
        public InvoiceCustomerSM Customer { get; set; }

        // Delivery address
        public InvoiceAddressSM DeliveryAddress { get; set; }

        // Order items
        public List<InvoiceItemSM> Items { get; set; }

        // Charges breakdown
        public decimal Subtotal { get; set; }
        public decimal DeliveryCharge { get; set; }
        public decimal PlatformCharge { get; set; }
        public decimal CutleryCharge { get; set; }
        public decimal LowCartFeeCharge { get; set; }
        public decimal TipAmount { get; set; }
        public decimal TotalAmount { get; set; }

        // Payment
        public string Currency { get; set; }
        public string PaymentMode { get; set; }
        public string PaymentStatus { get; set; }
        public string OrderStatus { get; set; }
    }

    public class InvoiceSellerSM
    {
        public long Id { get; set; }
        public string? Name { get; set; }
        public string? StoreName { get; set; }
        public string? Email { get; set; }
        public string? Mobile { get; set; }
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? Country { get; set; }
        public string? FssaiLicNo { get; set; }
        public string? TaxName { get; set; }
        public string? TaxNumber { get; set; }
    }

    public class InvoiceCustomerSM
    {
        public long Id { get; set; }
        public string? Name { get; set; }
        public string? Mobile { get; set; }
        public string? Email { get; set; }
    }

    public class InvoiceAddressSM
    {
        public string? Name { get; set; }
        public string? Mobile { get; set; }
        public string? Address { get; set; }
        public string? Landmark { get; set; }
        public string? Area { get; set; }
        public string? Pincode { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? Country { get; set; }
    }

    public class InvoiceItemSM
    {
        public long ProductVariantId { get; set; }
        public string? BaseProductName { get; set; }
        public string? VariantName { get; set; }
        public string? Indicator { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
        public List<OrderToppingDetailSM>? Toppings { get; set; }
        public decimal ToppingsTotal { get; set; }
        public List<OrderAddonDetailSM>? Addons { get; set; }
        public decimal AddonsTotal { get; set; }
        public decimal LineTotal { get; set; }
    }
}
