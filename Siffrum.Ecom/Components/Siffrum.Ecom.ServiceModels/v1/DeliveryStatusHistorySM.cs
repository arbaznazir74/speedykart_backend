using Siffrum.Ecom.ServiceModels.Enums;
using Siffrum.Ecom.ServiceModels.Foundation.Base;

namespace Siffrum.Ecom.ServiceModels.v1
{
    public class DeliveryStatusHistorySM : SiffrumServiceModelBase<long>
    {
        public long DeliveryId { get; set; }

        public DeliveryStatusSM Status { get; set; }

        public string? Remarks { get; set; }
    }
}
