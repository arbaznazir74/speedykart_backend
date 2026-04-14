using Siffrum.Ecom.ServiceModels.Foundation.Base;

namespace Siffrum.Ecom.ServiceModels.v1
{
    public class AddOnProductsSM : SiffrumServiceModelBase<long>
    {
        public long MainProductId { get; set; }

        public string? MainProductName { get; set; }
        public long AddonProductId { get; set; }

        public string? AddonProductName { get; set; }
        public long CategoryId { get; set; }
        public long SellerId { get; set; }
    }
}
