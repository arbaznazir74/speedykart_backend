using Siffrum.Ecom.ServiceModels.Enums;
using Siffrum.Ecom.ServiceModels.Foundation.Base;

namespace Siffrum.Ecom.ServiceModels.v1
{
    public class CashCollectionSM : SiffrumServiceModelBase<long>
    {
        public long SellerId { get; set; }
        public long DeliveryBoyId { get; set; }
        public decimal Amount { get; set; }
        public CashCollectionStatusSM Status { get; set; }
        public DateTime? CollectedAt { get; set; }
        public string? Remarks { get; set; }
        public DateTime DateFrom { get; set; }
        public DateTime DateTo { get; set; }
    }

    public class DeliveryBoyCodSummarySM
    {
        public long DeliveryBoyId { get; set; }
        public string? DeliveryBoyName { get; set; }
        public string? DeliveryBoyMobile { get; set; }
        public decimal TotalCodCollected { get; set; }
        public decimal TotalCashSettled { get; set; }
        public decimal PendingAmount { get; set; }
        public int TotalCodOrders { get; set; }
    }

    public class CodOrderDetailSM
    {
        public long OrderId { get; set; }
        public string OrderNumber { get; set; }
        public decimal Amount { get; set; }
        public string OrderStatus { get; set; }
        public string DeliveryStatus { get; set; }
        public DateTime? DeliveredAt { get; set; }
        public DateTime? OrderDate { get; set; }
        public string? CustomerName { get; set; }
    }

    public class MarkCashCollectedSM
    {
        public long DeliveryBoyId { get; set; }
        public decimal Amount { get; set; }
        public string? Remarks { get; set; }
    }

    public class DeliveryBoyCashLedgerSM
    {
        public decimal TotalCodCollected { get; set; }
        public decimal TotalCashSettled { get; set; }
        public decimal PendingAmount { get; set; }
        public List<CashCollectionSM> Settlements { get; set; } = new();
    }
}
