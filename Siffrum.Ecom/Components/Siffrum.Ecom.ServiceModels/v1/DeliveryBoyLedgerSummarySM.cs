namespace Siffrum.Ecom.ServiceModels.v1
{
    public class DeliveryBoyLedgerSummarySM
    {
        public long DeliveryBoyId { get; set; }
        public decimal TotalAmountReceived { get; set; }
        public decimal TotalAmountPaid { get; set; }
        public decimal OutstandingAmount { get; set; }
    }
}
