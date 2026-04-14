using Siffrum.Ecom.ServiceModels.Foundation.Base;

namespace Siffrum.Ecom.ServiceModels.v1
{
    public class ProductSpecificationSM : SiffrumServiceModelBase<long>
    {
        public string Key { get; set; }
        public string Value { get; set; }
        public long ProductVariantId { get; set; }
    }
}
