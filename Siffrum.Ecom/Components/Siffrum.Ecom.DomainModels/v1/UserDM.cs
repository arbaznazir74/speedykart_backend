using Siffrum.Ecom.DomainModels.Enums;
using Siffrum.Ecom.DomainModels.Foundation.Base;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Siffrum.Ecom.DomainModels.v1
{
    [Table("users")]
    //[Index(nameof(Email), IsUnique = true)]
    //[Index(nameof(Username), IsUnique = true)]
    public class UserDM : SiffrumDomainModelBase<long>
    {
        
        [Column("name")]
        [MaxLength(191)]
        public string? Name { get; set; }

        [Column("username")]
        [MaxLength(191)]
        public string? Username { get; set; }

       // [Required]
        [Column("email")]
        [MaxLength(191)]
        public string? Email { get; set; }

        [Column("password")]
        [MaxLength(191)]
        public string? Password { get; set; }

        [Column("email_verification_code")]
        [MaxLength(191)]
        public string? EmailVerificationCode { get; set; }

        [Column("profile")]
        [MaxLength(191)]
        public string? Image { get; set; }

        [Column("country_code")]
        [MaxLength(191)]
        public string? CountryCode { get; set; } 

        [Column("mobile")]
        [MaxLength(191)]
        [Required]
        public string Mobile { get; set; }

        [Column("balance")]
        public double Balance { get; set; } = 0;

        [Column("referral_code")]
        [MaxLength(191)]
        public string? ReferralCode { get; set; }

        [Column("friends_code")]
        [MaxLength(191)]
        public string? FriendsCode { get; set; }

        [Column("status")]
        public StatusDM Status { get; set; }

        [Column("login_status")]
        public LoginStatusDM LoginStatus { get; set; }       

        [Column("deleted_at")]
        public DateTime? DeletedAt { get; set; }

        [Column("payment_id")]
        [MaxLength(191)]
        public string? PaymentId { get; set; } //Stripe/Razorpay id

        [Column("pm_type")]
        [MaxLength(191)]
        public string? PmType { get; set; }

        [Column("pm_last_four")]

        [MaxLength(4)]
        public string? PmLastFour { get; set; }

        [Column("trial_ends_at")]
        public DateTime? TrialEndsAt { get; set; }

        [Column("role_type")]
        public RoleTypeDM RoleType { get; set; }

        [Column("device_type")]
        public DeviceTypeDM? DeviceType { get; set; }

        [Column("fcm_id")]
        public string? FcmId { get; set; }

        [Column("is_email_confirmed")]
        public bool IsEmailConfirmed { get; set; }
        [Column("is_mobile_confirmed")]
        public bool IsMobileConfirmed { get; set; }
        [Column("otp")]
        public int OTP { get; set; }

        [Column("offer_json_details")]
        public string OfferJsonDetails { get; set; } = "";

        [Column("assigned_seller_id")]
        public long? AssignedSellerId { get; set; }

        [Column("assigned_store_id")]
        public long? AssignedStoreId { get; set; }

        [Column("seller_assigned_at")]
        public DateTime? SellerAssignedAt { get; set; }

        public ICollection<ProductRatingDM> ProductRatings { get; set; }
        public ICollection<UserAddressDM> Addresses { get; set; }
        public ICollection<OrderDM> Orders { get; set; }
        public ICollection<UserPromocodesDM> UserPromocodes { get; set; }
        public DeliveryInstructionsDM DeliveryInstruction { get; set; }

    }
}
