using Siffrum.Ecom.ServiceModels.Foundation.Base;

namespace Siffrum.Ecom.ServiceModels.v1
{
    public class ProductFaqSM : SiffrumServiceModelBase<long>
    {
        public string Question { get; set; }
        public string Answer { get; set; }
        public bool Status { get; set; }
        public long? ProductVariantId { get; set; }
    }
}
