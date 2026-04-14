using Siffrum.Ecom.DomainModels.Foundation.Base;
using System.ComponentModel.DataAnnotations.Schema;

namespace Siffrum.Ecom.DomainModels.v1
{
    [Table("user_promo_codes")]
    public class UserPromocodesDM : SiffrumDomainModelBase<long>
    {
        [ForeignKey(nameof(Promocode))]
        [Column("promocode_id")]
        public long PromocodeId { get; set; }

        public virtual PromoCodeDM Promocode { get; set; }

        [ForeignKey(nameof(User))]

        [Column("user_id")]
        public long UserId { get; set; }

        public virtual UserDM User { get; set; }

        [Column("usage_count")]
        public int UsageCount { get; set; }

    }
}
