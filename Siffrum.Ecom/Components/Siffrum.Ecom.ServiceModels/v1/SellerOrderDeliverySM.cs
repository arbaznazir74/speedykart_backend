namespace Siffrum.Ecom.ServiceModels.v1
{
    public class SellerOrderDeliverySM
    {
        public long OrderId { get; set; }
        public string OrderNumber { get; set; }
        public string OrderStatus { get; set; }
        public string PaymentMode { get; set; }
        public decimal Amount { get; set; }
        public DateTime? OrderDate { get; set; }

        public string DeliveryStatus { get; set; }
        public long? DeliveryBoyId { get; set; }
        public string? DeliveryBoyName { get; set; }
        public string? DeliveryBoyMobile { get; set; }
        public DateTime? AssignedAt { get; set; }
        public DateTime? DeliveredAt { get; set; }
        public bool IsCodCollected { get; set; }

        public decimal Commission { get; set; }
        public double DistanceInKm { get; set; }

        public string? CustomerName { get; set; }
    }
}
