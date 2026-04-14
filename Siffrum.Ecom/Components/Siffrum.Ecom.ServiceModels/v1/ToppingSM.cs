using Siffrum.Ecom.ServiceModels.Foundation.Base;

namespace Siffrum.Ecom.ServiceModels.v1
{
    public class ToppingSM : SiffrumServiceModelBase<long>
    {
        public string Name { get; set; }
        public decimal Price { get; set; }
        public string? Image { get; set; }
        public string? Status { get; set; }
        public long? SuggestedBySellerId { get; set; }
    }
}
