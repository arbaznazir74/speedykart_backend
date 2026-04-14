using Siffrum.Ecom.DomainModels.Enums;
using Siffrum.Ecom.DomainModels.Foundation.Base;
using System.ComponentModel.DataAnnotations.Schema;

namespace Siffrum.Ecom.DomainModels.v1
{
    [Table("delivery_status_history")]
    public class DeliveryStatusHistoryDM : SiffrumDomainModelBase<long>
    {

        [Column("delivery_id")]
        public long DeliveryId { get; set; }

        [Column("status")]
        public DeliveryStatusDM Status { get; set; }

        [Column("remarks")]
        public string? Remarks { get; set; }

        public DeliveryDM Delivery { get; set; }
    }
}
