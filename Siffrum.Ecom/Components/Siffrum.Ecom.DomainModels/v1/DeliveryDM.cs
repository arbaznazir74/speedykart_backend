using Siffrum.Ecom.DomainModels.Enums;
using Siffrum.Ecom.DomainModels.Foundation.Base;
using System.ComponentModel.DataAnnotations.Schema;

namespace Siffrum.Ecom.DomainModels.v1
{
    [Table("deliveries")]
    public class DeliveryDM :  SiffrumDomainModelBase<long>
    {

        [Column("order_id")]
        public long OrderId { get; set; }

        [Column("delivery_boy_id")]
        public long DeliveryBoyId { get; set; }

        [Column("status")]
        public DeliveryStatusDM Status { get; set; }
        [Column("payment_mode")]
        public PaymentModeDM PaymentMode { get; set; }

        [Column("start_lat")]
        public double? StartLat { get; set; }

        [Column("start_long")]
        public double? StartLong { get; set; }

        [Column("end_lat")]
        public double? EndLat { get; set; }

        [Column("end_long")]
        public double? EndLong { get; set; }

        [Column("expected_delivery_date")]
        public DateTime? ExpectedDeliveryDate { get; set; }
        [Column("amount")]
        public decimal Amount { get; set; }

        [Column("assigned_at")]
        public DateTime AssignedAt { get; set; }

        [Column("delivered_at")]
        public DateTime? DeliveredAt { get; set; }

        [Column("commission")]
        public decimal Commission { get; set; }

        [Column("distance_in_km")]
        public double DistanceInKm { get; set; }

        public OrderDM Order { get; set; }

        public DeliveryBoyDM DeliveryBoy { get; set; }

        public ICollection<DeliveryTrackingDM> Trackings { get; set; }
    }
}
