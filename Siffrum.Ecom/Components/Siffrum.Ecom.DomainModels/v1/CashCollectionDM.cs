using Siffrum.Ecom.DomainModels.Enums;
using Siffrum.Ecom.DomainModels.Foundation.Base;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Siffrum.Ecom.DomainModels.v1
{
    [Table("cash_collections")]
    public class CashCollectionDM : SiffrumDomainModelBase<long>
    {
        [Column("seller_id")]
        public long SellerId { get; set; }

        [Column("delivery_boy_id")]
        public long DeliveryBoyId { get; set; }

        [ForeignKey(nameof(DeliveryBoyId))]
        public virtual DeliveryBoyDM DeliveryBoy { get; set; }

        [Column("amount", TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        [Column("status")]
        public CashCollectionStatusDM Status { get; set; } = CashCollectionStatusDM.Pending;

        [Column("collected_at")]
        public DateTime? CollectedAt { get; set; }

        [Column("remarks")]
        public string? Remarks { get; set; }

        [Column("date_from")]
        public DateTime DateFrom { get; set; }

        [Column("date_to")]
        public DateTime DateTo { get; set; }
    }
}
