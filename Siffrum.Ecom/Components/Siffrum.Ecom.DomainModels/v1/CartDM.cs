using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Siffrum.Ecom.DomainModels.Foundation.Base;
using Siffrum.Ecom.DomainModels.Enums;

namespace Siffrum.Ecom.DomainModels.v1
{
    [Table("carts")]
    public class CartDM : SiffrumDomainModelBase<long>
    {

        [ForeignKey(nameof(User))]
        [Column("user_id")]
        public long UserId { get; set; }
        public virtual UserDM User { get; set; }
        [Column("platform_type")]
        public PlatformTypeDM PlatformType { get; set; }


        [Column("subtotal")]
        public decimal SubTotal { get; set; }

        [Column("tax_amount")]
        public decimal TaxAmount { get; set; }

        [Column("discount_amount")]
        public decimal DiscountAmount { get; set; }

        [Column("grand_total")]
        public decimal GrandTotal { get; set; }

        public virtual ICollection<CartItemDM> Items { get; set; } = new List<CartItemDM>();
    }
}
