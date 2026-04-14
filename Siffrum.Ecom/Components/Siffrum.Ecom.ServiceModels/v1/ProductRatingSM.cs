using Siffrum.Ecom.ServiceModels.Enums;
using Siffrum.Ecom.ServiceModels.Foundation.Base;

namespace Siffrum.Ecom.ServiceModels.v1
{
    public class ProductRatingSM : SiffrumServiceModelBase<long>
    {
        public long UserId { get; set; }

        public string UserName { get ; set; }

        public short Rate { get; set; }

        public string Review { get; set; }

        public StatusSM Status { get; set; }
        public long ProductVariantId { get; set; }
        public List<string> Images { get; set; } 
    }
}
