using Siffrum.Ecom.ServiceModels.Enums;
using Siffrum.Ecom.ServiceModels.Foundation.Base;

namespace Siffrum.Ecom.ServiceModels.v1
{
    public class UserAddressSM : SiffrumServiceModelBase<long>
    {
        public long UserId { get; set; }

        public AddressTypeSM Type { get; set; }

        public string? Name { get; set; } 

        public string? Mobile { get; set; } 

        public string? AlternateMobile { get; set; }

        public string Address { get; set; } 

        public string Landmark { get; set; } 

        public string Area { get; set; }

        public string Pincode { get; set; }

        public string City { get; set; }

        public string State { get; set; }

        public string Country { get; set; } 

        public bool IsDefault { get; set; }

        public double? Latitude { get; set; }

        public double? Longitude { get; set; }
    }
}
