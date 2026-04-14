using Siffrum.Ecom.ServiceModels.Enums;
using Siffrum.Ecom.ServiceModels.Foundation.Base;

namespace Siffrum.Ecom.ServiceModels.v1
{
    public class BannerSM : SiffrumServiceModelBase<long>
    {
        public ExtensionTypeSM Extension { get; set; }
        public BannerTypeSM BannerType { get; set; }

        public PlatformTypeSM PlatformType { get; set; }
        public string Title { get; set; }
        public string SubTitle { get; set; }
        public string ContentBase64 { get; set; }
        public string? NetworkContent { get; set; }

        public string? SliderUrl { get; set; }

        public bool IsDefault { get; set; }
        public int Priority { get; set; }
    }
}
