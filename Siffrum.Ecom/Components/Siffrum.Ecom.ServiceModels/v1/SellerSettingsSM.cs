using Siffrum.Ecom.ServiceModels.Foundation.Base;

namespace Siffrum.Ecom.ServiceModels.v1
{
    public class SellerSettingsSM : SiffrumServiceModelBase<long>
    {
       public long SellerId { get; set; }

        public SellerSettingsJson SellerSettingsJson { get; set; }

    }
}
