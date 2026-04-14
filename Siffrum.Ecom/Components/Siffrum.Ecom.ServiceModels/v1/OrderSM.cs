using Siffrum.Ecom.ServiceModels.Enums;
using Siffrum.Ecom.ServiceModels.Foundation.Base;

namespace Siffrum.Ecom.ServiceModels.v1
{
    public class OrderSM : SiffrumServiceModelBase<long>
    {
        public string OrderNumber { get; set; } = string.Empty;

        public long TransactionId { get; set; }

        public string? RazorpayOrderId { get; set; }

        public string? RazorpayPaymentId { get; set; }

        public long UserId { get; set; }

        public string? CustomerName { get; set; }
        public string? Receipt { get; set; }

        public string Currency { get; set; } = "INR";
        public decimal Amount { get; set; }
        public decimal TipAmount { get; set; }
        public decimal PaidAmount { get; set; } 

        public decimal DueAmount { get; set; }
        public decimal RefundAmount { get; set; } = 0;
        public string? FailureReason { get; set; }
        public bool IsCutlaryInculded { get; set; }

        public decimal DeliveryCharge { get; set; }
        public decimal PlatormCharge { get; set; }
        public decimal CutlaryCharge { get; set; }

        public decimal LowCartFeeCharge { get; set; }

        public long? AddressId { get; set; }
        public DateTime? ExpectedDeliveryDate { get; set; }
        public PaymentStatusSM PaymentStatus { get; set; } = PaymentStatusSM.Pending;

        public OrderStatusSM OrderStatus { get; set; } = OrderStatusSM.Created;

        public PaymentModeSM PaymentMode { get; set; }

        public long? SellerId { get; set; }

        public int PreparationTimeInMinutes { get; set; }
        public DateTime? SellerAcceptedAt { get; set; }

        public string? PreparationStatus { get; set; }
        public string? PreparationStatusMessage { get; set; }
    }
}
