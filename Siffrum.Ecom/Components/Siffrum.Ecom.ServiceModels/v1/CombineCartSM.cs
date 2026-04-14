using Siffrum.Ecom.ServiceModels.Enums;
using Siffrum.Ecom.ServiceModels.Foundation.Base;

namespace Siffrum.Ecom.ServiceModels.v1
{
    public class CombineCartSM : SiffrumServiceModelBase<long>
    {
        public CartSM? HotBoxCart { get; set; }
        public CartSM? SpeedyMartCart { get; set; }

        public List<HotBoxCartItemSM>? HotBoxCartItems { get; set; }
        public List<SpeedyMartCartItemSM>? SpeedyMartCartItems { get; set; }
    }
}
