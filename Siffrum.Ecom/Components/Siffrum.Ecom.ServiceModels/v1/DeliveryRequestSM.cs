using Siffrum.Ecom.ServiceModels.Foundation.Base;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Siffrum.Ecom.ServiceModels.Enums;

namespace Siffrum.Ecom.ServiceModels.v1
{
    public class DeliveryRequestSM : SiffrumServiceModelBase<long>
    {
        public long UserId { get; set; }
        public string Pincode { get; set; }

        public double Latitude { get; set; }

        public double Longitude { get; set; }

        public string? Address { get; set; }

        public PlatformTypeSM Platform { get; set; }
        public string AdminRemarks { get; set; }

        public bool IsResolved { get; set; } = false;
    }
}
