using Siffrum.Ecom.ServiceModels.Foundation.Base;

namespace Siffrum.Ecom.ServiceModels.v1
{
    public class DeliveryTrackingSM : SiffrumServiceModelBase<long>
    {
        public long DeliveryId { get; set; }
        public double CurrentLat { get; set; }
        public double CurrentLong { get; set; }
        public string? Address { get; set; }
    }
}
