using Siffrum.Ecom.DomainModels.Foundation.Base;
using System.ComponentModel.DataAnnotations.Schema;

namespace Siffrum.Ecom.DomainModels.v1
{
    [Table("seller_settings")]
    public class SellerSettingsDM : SiffrumDomainModelBase<long>
    {
        [ForeignKey(nameof(Seller))]
        [Column("seller_id")]
        public long SellerId { get; set; }

        public virtual SellerDM Seller { get; set; } 

        [Column("json-data")]
        public string JsonData { get; set; }
    }
}
