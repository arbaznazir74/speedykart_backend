using Siffrum.Ecom.DomainModels.Enums;
using Siffrum.Ecom.DomainModels.Foundation.Base;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Siffrum.Ecom.DomainModels.v1
{
    [Table("order_complaints")]
    public class OrderComplaintDM : SiffrumDomainModelBase<long>
    {
        [Column("order_id")]
        public long OrderId { get; set; }

        [ForeignKey(nameof(OrderId))]
        public virtual OrderDM Order { get; set; }

        [Column("user_id")]
        public long UserId { get; set; }

        [Column("seller_id")]
        public long? SellerId { get; set; }

        [Required]
        [Column("email")]
        [MaxLength(255)]
        public string Email { get; set; }

        [Required]
        [Column("message")]
        public string Message { get; set; }

        [Column("status")]
        public ComplaintStatusDM Status { get; set; } = ComplaintStatusDM.Open;

        [Column("seller_reply")]
        public string? SellerReply { get; set; }

        [Column("replied_at")]
        public DateTime? RepliedAt { get; set; }
    }
}
