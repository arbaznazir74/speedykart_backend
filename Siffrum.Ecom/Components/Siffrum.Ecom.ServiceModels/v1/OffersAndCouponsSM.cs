using Siffrum.Ecom.ServiceModels.Enums;
using Siffrum.Ecom.ServiceModels.Foundation.Base;

namespace Siffrum.Ecom.ServiceModels.v1
{
    public class OffersAndCouponsSM : SiffrumServiceModelBase<long>
    {
        public string Name { get; set; }
        public string? Description { get; set; }
        public string PathBase64 { get; set; }
        public ExtensionTypeSM ExtensionType { get; set; }
        public PlatformTypeSM PlatformType { get; set; }
        public decimal? Percentage { get; set; }
        public decimal? OfferValue { get; set; }
        public decimal MinAmount { get; set; }
        public decimal MaxDiscountAmount { get; set; }
    }
}
