using Siffrum.Ecom.ServiceModels.Enums;

namespace Siffrum.Ecom.ServiceModels.v1
{
    public class StoreSM
    {
        public long Id { get; set; }
        public long SellerId { get; set; }
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public SellerStatusSM Status { get; set; }
        public SellerSettingsJson? SellerSettingsJson { get; set; }
    }
}
