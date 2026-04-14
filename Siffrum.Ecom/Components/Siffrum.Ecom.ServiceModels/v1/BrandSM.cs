using Siffrum.Ecom.ServiceModels.Enums;
using Siffrum.Ecom.ServiceModels.Foundation.Base;

namespace Siffrum.Ecom.ServiceModels.v1
{
    public class BrandSM : SiffrumServiceModelBase<long>
    {
        public string Name { get; set; }

        public string? Image { get; set; } = null!;
        public string? NetworkImage { get; set; }

        public StatusSM Status { get; set; }
    }
}
