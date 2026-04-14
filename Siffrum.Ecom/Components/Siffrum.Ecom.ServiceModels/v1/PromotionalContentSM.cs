using Siffrum.Ecom.ServiceModels.Enums;
using Siffrum.Ecom.ServiceModels.Foundation.Base;

namespace Siffrum.Ecom.ServiceModels.v1
{
    public class PromotionalContentSM : SiffrumServiceModelBase<long>
    {
        public string Title { get; set; }

        public string? IconBase64 { get; set; }

        public PlatformTypeSM PlatformType { get; set; }

        public ExtensionTypeSM ExtensionType { get; set; }

        public PromotionDisplayLocationSM DisplayLocation { get; set; }
    }
}
