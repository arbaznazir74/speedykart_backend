using Siffrum.Ecom.DomainModels.Foundation.Base;
using System.ComponentModel.DataAnnotations.Schema;

namespace Siffrum.Ecom.DomainModels.v1
{
    [Table("delivery_boy_transactions")]
    public class DeliveryBoyTransactionsDM : SiffrumDomainModelBase<long>
    {
        [Column("amount")]
        public decimal Amount { get; set; }
        [Column("transaction_date")]
        public DateTime TransactionDate { get; set; }
        [Column("transaction_id")]
        public string? TransactionId { get; set; }

        [ForeignKey(nameof(DeliveryBoy))]
        [Column("delivery_boy_id")]
        public long DeliveryBoyId { get; set; }
        public DeliveryBoyDM DeliveryBoy { get; set; }
    }
}
