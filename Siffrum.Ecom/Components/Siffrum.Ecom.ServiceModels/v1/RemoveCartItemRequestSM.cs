using Siffrum.Ecom.ServiceModels.Enums;

namespace Siffrum.Ecom.ServiceModels.v1
{
    public class RemoveCartItemRequestSM
    {
        public long ProductVariantId { get; set; }
        public PlatformTypeSM PlatformType { get; set; }
    }
}
