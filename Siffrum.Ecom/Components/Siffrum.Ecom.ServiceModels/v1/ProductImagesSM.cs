using Siffrum.Ecom.ServiceModels.Foundation.Base;

namespace Siffrum.Ecom.ServiceModels.v1
{
    public class ProductImagesSM : SiffrumServiceModelBase<long>
    {
        
        public long ProductVariantId { get; set; }
        public string ImageBase64 { get; set; }
    }
}
