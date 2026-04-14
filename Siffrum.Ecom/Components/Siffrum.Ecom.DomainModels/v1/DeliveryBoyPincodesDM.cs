using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Siffrum.Ecom.DomainModels.v1
{
    [Table("delibery_boy_pincodes")]
    public class DeliveryBoyPincodesDM
    {
        [Key]
        [Column("id")]
        public long Id { get; set; }
        [Column("pincode")]
        public string Pincode { get; set; }

        [ForeignKey(nameof(DeliveryBoy))]
        [Column("delivery_boy_id")]
        public long DeliveryBoyId { get; set; }

        public virtual DeliveryBoyDM DeliveryBoy { get; set; }
        
    }
}
