using Siffrum.Ecom.DomainModels.Foundation.Base;
using System.ComponentModel.DataAnnotations.Schema;

namespace Siffrum.Ecom.DomainModels.v1
{
    [Table("delivery_tracking")]
    public class DeliveryTrackingDM : SiffrumDomainModelBase<long>
    {
        [Column("delivery_id")]
        public long DeliveryId { get; set; }

        [Column("current_lat")]
        public double CurrentLat { get; set; }

        [Column("current_long")]
        public double CurrentLong { get; set; }
        [Column("address")]
        public string? Address { get; set; }

        public DeliveryDM Delivery { get; set; }
    }
}
