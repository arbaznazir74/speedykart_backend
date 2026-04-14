namespace Siffrum.Ecom.ServiceModels.v1
{
    public class DeliveryBoyStatsSM
    {
        public long DeliveryBoyId { get; set; }
        public string DeliveryBoyName { get; set; }
        public int TotalDeliveries { get; set; }
        public int DeliveredCount { get; set; }
        public int AssignedCount { get; set; }
        public int PickedUpCount { get; set; }
        public decimal TotalDeliveredAmount { get; set; }
        public bool IsOnline { get; set; }
        public string PaymentType { get; set; }
        public string? SellerName { get; set; }
    }
}
