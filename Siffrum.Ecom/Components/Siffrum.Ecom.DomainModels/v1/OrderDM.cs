using Microsoft.EntityFrameworkCore;
using Siffrum.Ecom.DomainModels.Enums;
using Siffrum.Ecom.DomainModels.Foundation.Base;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Siffrum.Ecom.DomainModels.v1
{
    [Table("orders")]
    [Index(nameof(OrderNumber), IsUnique = true)]
    [Index(nameof(TransactionId), IsUnique = true)]
    public class OrderDM : SiffrumDomainModelBase<long>
    {
        [Required]
        [Column("order_number")]
        [MaxLength(8)]
        public string OrderNumber { get; set; } = string.Empty;

        [Column("transaction_id")]
        public long TransactionId { get; set; }

        [Column("razorpay_order_id")]
        [MaxLength(100)]
        public string? RazorpayOrderId { get; set; }

        [Column("razorpay_payment_id")]
        [MaxLength(100)]
        public string? RazorpayPaymentId { get; set; }


        [ForeignKey(nameof(User))]
        [Column("user_id")]
        public long UserId { get; set; }

        public virtual UserDM User { get; set; }

        [Column("receipt")]
        [MaxLength(100)]
        public string? Receipt { get; set; }

        [Column("currency")]
        [MaxLength(10)]
        public string Currency { get; set; } = "INR";

        [Column("amount", TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        [Column("paid_amount", TypeName = "decimal(18,2)")]
        public decimal PaidAmount { get; set; } = 0;

        [Column("due_amount", TypeName = "decimal(18,2)")]
        public decimal DueAmount { get; set; } = 0;
        [Column("refund_amount", TypeName = "decimal(18,2)")]
        public decimal RefundAmount { get; set; } = 0;
        [Column("tip_amount", TypeName = "decimal(18,2)")]
        public decimal TipAmount { get; set; } = 0;
        [Column("failure_reason")]
        public string? FailureReason { get; set; }
        [Column("expected_delivery_date")]
        public DateTime? ExpectedDeliveryDate { get; set; }

        [Column("payment_status")]
        public PaymentStatusDM PaymentStatus { get; set; } = PaymentStatusDM.Pending;

        [Column("order_status")]
        public OrderStatusDM OrderStatus { get; set; } = OrderStatusDM.Created;
        [Column("payment_mode")]
        public PaymentModeDM PaymentMode { get; set; } = PaymentModeDM.CashOnDelivery;
        [Column("address_id")]
        public long? AddressId { get; set; }
        [Column("is_cutlary_allowed")]
        public bool IsCutlaryInculded { get; set; }
        [Column("delivery_charge")]
        public decimal DeliveryCharge { get; set; }

        [Column("platorm_charge")]
        public decimal PlatormCharge { get; set; } = 0;

        [Column("cutlary_charge")]
        public decimal CutlaryCharge { get; set; } = 0;
        [Column("low_cart_fee_charge")]
        public decimal LowCartFeeCharge { get; set; } = 0;

        [Column("seller_id")]
        public long? SellerId { get; set; }

        [Column("preparation_time_in_minutes")]
        public int PreparationTimeInMinutes { get; set; } = 0;

        [Column("seller_accepted_at")]
        public DateTime? SellerAcceptedAt { get; set; }

        public ICollection<OrderItemDM> OrderItems { get; set; } = new List<OrderItemDM>();
        public ICollection<InvoiceDM> Invoices { get; set; } = new List<InvoiceDM>();
    }
}
