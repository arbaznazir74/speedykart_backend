using Siffrum.Ecom.ServiceModels.Enums;
using Siffrum.Ecom.ServiceModels.Foundation.Base;

namespace Siffrum.Ecom.ServiceModels.v1
{
    public class OrderComplaintSM : SiffrumServiceModelBase<long>
    {
        public long OrderId { get; set; }
        public long UserId { get; set; }
        public long? SellerId { get; set; }
        public string Email { get; set; }
        public string Message { get; set; }
        public ComplaintStatusSM Status { get; set; } = ComplaintStatusSM.Open;
        public string? SellerReply { get; set; }
        public DateTime? RepliedAt { get; set; }
    }

    public class OrderComplaintDetailSM
    {
        // Complaint info
        public long Id { get; set; }
        public string Email { get; set; }
        public string Message { get; set; }
        public string Status { get; set; }
        public string? SellerReply { get; set; }
        public DateTime? RepliedAt { get; set; }
        public DateTime? CreatedAt { get; set; }

        // Order info
        public long OrderId { get; set; }
        public string OrderNumber { get; set; }
        public decimal OrderAmount { get; set; }
        public string OrderStatus { get; set; }
        public string PaymentStatus { get; set; }
        public string PaymentMode { get; set; }
        public decimal DeliveryCharge { get; set; }
        public decimal PlatformCharge { get; set; }
        public decimal CutleryCharge { get; set; }
        public decimal LowCartFeeCharge { get; set; }
        public decimal TipAmount { get; set; }
        public DateTime? OrderDate { get; set; }

        // Customer info
        public long UserId { get; set; }
        public string? CustomerName { get; set; }
        public string? CustomerMobile { get; set; }
        public string? CustomerEmail { get; set; }

        // Delivery address
        public OrderComplaintAddressSM? DeliveryAddress { get; set; }

        // Delivery boy info
        public OrderComplaintDeliveryBoySM? DeliveryBoy { get; set; }

        // Order items summary
        public List<OrderComplaintItemSM> Items { get; set; } = new();
    }

    public class OrderComplaintAddressSM
    {
        public string? Name { get; set; }
        public string? Mobile { get; set; }
        public string? Address { get; set; }
        public string? Landmark { get; set; }
        public string? Pincode { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
    }

    public class OrderComplaintDeliveryBoySM
    {
        public long Id { get; set; }
        public string? Name { get; set; }
        public string? Mobile { get; set; }
        public string? Email { get; set; }
        public string? DeliveryStatus { get; set; }
        public DateTime? AssignedAt { get; set; }
        public DateTime? DeliveredAt { get; set; }
    }

    public class OrderComplaintItemSM
    {
        public string? ProductName { get; set; }
        public string? VariantName { get; set; }
        public string? Indicator { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
    }
}
