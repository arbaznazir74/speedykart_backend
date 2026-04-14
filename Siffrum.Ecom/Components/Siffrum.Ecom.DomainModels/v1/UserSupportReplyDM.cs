using Siffrum.Ecom.DomainModels.Foundation.Base;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Siffrum.Ecom.DomainModels.v1
{
    [Table("user_support_replies")]
    public class UserSupportReplyDM : SiffrumDomainModelBase<long>
    {
        [Column("support_request_id")]
        public long SupportRequestId { get; set; }

        [ForeignKey(nameof(SupportRequestId))]
        public virtual UserSupportRequestDM SupportRequest { get; set; }

        [Required]
        [MaxLength(2000)]
        [Column("message")]
        public string Message { get; set; }

        [Required]
        [MaxLength(20)]
        [Column("sender_role")]
        public string SenderRole { get; set; } // "Admin" or "User"

        [Column("sender_id")]
        public long SenderId { get; set; }
    }
}
