using Microsoft.EntityFrameworkCore;
using Siffrum.Ecom.DomainModels.Enums;
using Siffrum.Ecom.DomainModels.Foundation.Base;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Siffrum.Ecom.DomainModels.v1
{
    [Table("invoice")]
    [Index(nameof(TransactionId), IsUnique = true)]
    public class InvoiceDM : SiffrumDomainModelBase<long>
    {
        [Column("transaction_id")]
        public long TransactionId { get; set; }

        [Required]
        [Column("invoice_date")]
        public DateTime InvoiceDate { get; set; }

        [ForeignKey(nameof(Order))]

        [Required]
        [Column("order_id")]
        public long OrderId { get; set; }

        public virtual OrderDM Order { get; set; }
        [Column("razprpay_invoice_id")]
        public string? RazorpayInvoiceId { get; set; }

        [Column("currency")]
        [MaxLength(10)]
        public string Currency { get; set; } = "INR";

        [Required]
        [Column("amount")]
        public decimal Amount { get; set; }

        [Column("payment_status")]
        public PaymentStatusDM PaymentStatus { get; set; } 

        [Column("order_status")]
        public OrderStatusDM OrderStatus { get; set; }
    }
}
