namespace Siffrum.Ecom.ServiceModels.v1
{
    public class AdminOrderLifecycleSM
    {
        public long OrderId { get; set; }
        public string OrderNumber { get; set; }
        public string OrderStatus { get; set; }
        public string PaymentMode { get; set; }
        public string PaymentStatus { get; set; }
        public decimal Amount { get; set; }
        public DateTime? OrderDate { get; set; }

        public string? CustomerName { get; set; }
        public string? CustomerMobile { get; set; }

        public string? SellerName { get; set; }

        public string DeliveryStatus { get; set; }
        public string? DeliveryBoyName { get; set; }
        public string? DeliveryBoyMobile { get; set; }
        public DateTime? AssignedAt { get; set; }
        public DateTime? DeliveredAt { get; set; }

        public bool IsCod { get; set; }
        public bool CodCollected { get; set; }

        public List<DeliveryStatusHistorySM> DeliveryStatusHistory { get; set; } = new();
        public DeliveryTrackingSM? LatestTracking { get; set; }
        public int TotalItems { get; set; }
    }
}
