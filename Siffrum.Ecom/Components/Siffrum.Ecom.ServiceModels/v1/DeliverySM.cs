using Siffrum.Ecom.ServiceModels.Enums;
using Siffrum.Ecom.ServiceModels.Foundation.Base;

namespace Siffrum.Ecom.ServiceModels.v1
{
    public class DeliverySM : SiffrumServiceModelBase<long>
    {
        public long OrderId { get; set; }

        public long DeliveryBoyId { get; set; }

        public string? DeliveryBoyName { get; set; }

        public string? DeliveryBoyMobie { get; set; }

        public DeliveryStatusSM? Status { get; set; }

        public PaymentModeSM PaymentMode { get; set; }

        public decimal Amount { get; set; }

        public double? StartLat { get; set; }

        public double? StartLong { get; set; }

        public double? EndLat { get; set; }

        public double? EndLong { get; set; }
        public DateTime? ExpectedDeliveryDate { get; set; }

        public DateTime AssignedAt { get; set; }

        public DateTime? DeliveredAt { get; set; }

        public decimal Commission { get; set; }

        public double DistanceInKm { get; set; }
    }
}
