using Siffrum.Ecom.ServiceModels.Foundation.Base;

namespace Siffrum.Ecom.ServiceModels.v1
{
    public class UnitSM : SiffrumServiceModelBase<long>
    {
        public string? Name { get; set; }

        public string ShortCode { get; set; } = string.Empty;        
        public long? ParentId { get; set; }        
        public int? Conversion { get; set; }
        public long? SellerId { get; set; }
    }
}
