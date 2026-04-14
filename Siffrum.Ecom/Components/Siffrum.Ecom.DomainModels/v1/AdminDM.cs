using Siffrum.Ecom.DomainModels.Enums;
using Siffrum.Ecom.DomainModels.Foundation.Base;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Siffrum.Ecom.DomainModels.v1
{
    [Table("admins")]
    public class AdminDM : SiffrumDomainModelBase<long>
    {       

        [Column("username")]
        [MaxLength(191)]
        public string? Username { get; set; } 

        [Required]
        [Column("email")]
        [MaxLength(191)]
        public string Email { get; set; } = null!;

        [Column("password")]
        public string? Password { get; set; }   

        [Column("forgot_password_code")]
        //[MaxLength(191)]
        public string? ForgotPasswordCode { get; set; }

        [Column("fcm_id")]
        [MaxLength(191)]
        public string? FcmId { get; set; }

        [Column("remember_token")]
        [MaxLength(100)]
        public string? RememberToken { get; set; }

        [Required]
        [Column("status")]
        public StatusDM Status { get; set; }

        [Column("login_status")]
        public LoginStatusDM LoginStatus { get; set; }

        [Column("login_at")]
        public DateTime? LoginAt { get; set; }

        [Column("last_active_at")]
        public DateTime? LastActiveAt { get; set; }

        [Column("role_type")]
        public RoleTypeDM RoleType { get; set; }
        // Navigation
        //public RoleDM Role { get; set; }
        //public HashSet<SellerDM> Seller { get; set; }
        public HashSet<DeliveryBoyDM> DeliveryBoy { get; set; }
        
    }
}
