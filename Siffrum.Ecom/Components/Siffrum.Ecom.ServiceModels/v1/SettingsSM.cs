using Siffrum.Ecom.ServiceModels.Foundation.Base;

namespace Siffrum.Ecom.ServiceModels.v1
{
    public class SettingsSM : SiffrumServiceModelBase<long>
    {
        public bool IsOrderPossible { get; set; }
        public bool IsFreeDelivery { get; set; }
        public bool IsSurge { get; set; }
        public int DeliveryInMinutes { get; set; }
        public int SurgeCount { get; set; } = 0;
        public decimal SurgeCharge { get; set; }
        public int ReferralPercentage { get; set; }
        public decimal PlatormCharge { get; set; }
        public decimal CutlaryCharge { get; set; }

        public decimal LowCartFeeCharge { get; set; }

        public decimal LowCartAmountValue { get; set; }

        public bool IsCodAvailable { get; set; }

        public decimal CommissionPerKm { get; set; }

    }
}
