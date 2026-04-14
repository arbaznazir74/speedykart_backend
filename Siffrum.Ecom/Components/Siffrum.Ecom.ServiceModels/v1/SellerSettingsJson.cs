namespace Siffrum.Ecom.ServiceModels.v1
{
    public class SellerSettingsJson
    {
        public bool IsOrderPossible { get; set; }
        public bool IsFreeDelivery { get; set; }
        public bool IsSurge { get; set; }
        public bool IsCodAvailable { get; set; }
        public int MinRadiusInKms { get; set; }
        public int MaxRadiusInKms { get; set; }
        public int SurgeCount { get; set; } = 0;
        public decimal SurgeCharge { get; set; }

        public decimal MinDeliveryCharge { get; set; }
        public decimal DeliveryChargeAterMinRadius { get; set; }

        public decimal CommissionPerKm { get; set; }
    }
}