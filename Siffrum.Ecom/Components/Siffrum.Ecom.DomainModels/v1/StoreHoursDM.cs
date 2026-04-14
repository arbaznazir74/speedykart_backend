using Siffrum.Ecom.DomainModels.Foundation.Base;
using System.ComponentModel.DataAnnotations.Schema;

namespace Siffrum.Ecom.DomainModels.v1
{
    [Table("store_hours")]
    public class StoreHoursDM : SiffrumDomainModelBase<long>
    {
        [ForeignKey(nameof(Seller))]
        [Column("seller_id")]
        public long SellerId { get; set; }

        public virtual SellerDM Seller { get; set; }

        [Column("day_of_week")]
        public short DayOfWeek { get; set; } // 0=Sunday, 1=Monday ... 6=Saturday

        [Column("open_time")]
        public TimeSpan? OpenTime { get; set; }

        [Column("close_time")]
        public TimeSpan? CloseTime { get; set; }

        [Column("is_closed")]
        public bool IsClosed { get; set; } = false;
    }
}
