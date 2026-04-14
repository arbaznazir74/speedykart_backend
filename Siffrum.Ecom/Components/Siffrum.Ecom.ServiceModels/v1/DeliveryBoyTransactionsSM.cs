using Siffrum.Ecom.ServiceModels.Foundation.Base;
using System.ComponentModel.DataAnnotations.Schema;

namespace Siffrum.Ecom.ServiceModels.v1
{
    public class DeliveryBoyTransactionsSM : SiffrumServiceModelBase<long>
    {
        public decimal Amount { get; set; }
        public DateTime TransactionDate { get; set; }
        public string? TransactionId { get; set; }

        public long DeliveryBoyId { get; set; }
    }
}
