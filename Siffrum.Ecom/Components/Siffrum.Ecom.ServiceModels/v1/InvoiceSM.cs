using Siffrum.Ecom.ServiceModels.Enums;
using Siffrum.Ecom.ServiceModels.Foundation.Base;

namespace Siffrum.Ecom.ServiceModels.v1
{
    public class InvoiceSM : SiffrumServiceModelBase<long>
    {
        public long TransactionId { get; set; }
        public DateTime InvoiceDate { get; set; }
        public long OrderId { get; set; }
        public decimal Amount { get; set; }
        public PaymentStatusSM PaymentStatus { get; set; }
        public OrderStatusSM OrderStatus { get; set; }
        public string? RazorpayInvoiceId { get; set; }
        public string Currency { get; set; } 

    }
}
