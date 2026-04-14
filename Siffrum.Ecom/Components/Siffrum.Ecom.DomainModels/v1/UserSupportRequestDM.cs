using Siffrum.Ecom.DomainModels.Foundation.Base;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Siffrum.Ecom.DomainModels.v1
{
    [Table("UserSupportRequest")]
    public class UserSupportRequestDM : SiffrumDomainModelBase<long>
    {
        [Required]
        [Column("user_id")]
        public long UserId { get; set; }

        [ForeignKey(nameof(UserId))]
        public virtual UserDM User { get; set; }

        [Required]
        [MaxLength(200)]
        [Column("subject")]
        public string Subject { get; set; }

        [Required]
        [MaxLength(2000)]
        [Column("message")]
        public string Message { get; set; }

        [MaxLength(100)]
        [Column("email")]
        public string? Email { get; set; }

        [MaxLength(20)]
        [Column("mobile")]
        public string? Mobile { get; set; }

        [MaxLength(1000)]
        [Column("admin_response")]
        public string? AdminResponse { get; set; }

        [Column("is_resolved")]
        public bool IsResolved { get; set; }
    }
}