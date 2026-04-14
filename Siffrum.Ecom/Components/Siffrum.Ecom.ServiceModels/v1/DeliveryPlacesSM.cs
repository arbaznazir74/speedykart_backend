using Siffrum.Ecom.ServiceModels.Enums;
using Siffrum.Ecom.ServiceModels.Foundation.Base;

namespace Siffrum.Ecom.ServiceModels.v1
{
    public class DeliveryPlacesSM : SiffrumServiceModelBase<long>
    {
        public string Pincode { get; set; }

        public StatusSM Status { get; set; }

        public double? Latitude { get; set; }

        public double? Longitude { get; set; }
        public decimal DeliveryCharges { get; set; }
        public int DeliveryTimeInMinutes { get; set; }
        public long SellerId { get; set; }
    }
}
