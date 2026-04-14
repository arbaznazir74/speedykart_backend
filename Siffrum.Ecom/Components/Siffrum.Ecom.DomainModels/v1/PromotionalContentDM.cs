using Siffrum.Ecom.DomainModels.Enums;
using Siffrum.Ecom.DomainModels.Foundation.Base;
using System.ComponentModel.DataAnnotations.Schema;

namespace Siffrum.Ecom.DomainModels.v1
{
    [Table("promotional_content")]
    public class PromotionalContentDM : SiffrumDomainModelBase<long>
    {
        [Column("title")]
        public string Title { get; set; }
        [Column("icon")]
        public string? IconPath { get; set; }
        [Column("extension")]
        public ExtensionTypeDM? ExtensionType { get; set; }
        [Column("platform_type")]
        public PlatformTypeDM PlatformType { get; set; }

        [Column("display_location")]
        public PromotionDisplayLocationDM DisplayLocation { get; set; }
    }
}
