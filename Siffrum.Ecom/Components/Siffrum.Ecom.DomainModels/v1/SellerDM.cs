using Microsoft.EntityFrameworkCore;
using Siffrum.Ecom.DomainModels.Enums;
using Siffrum.Ecom.DomainModels.Foundation.Base;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Siffrum.Ecom.DomainModels.v1
{
    [Table("sellers")]
    [Index(nameof(Email), IsUnique = true)]
    [Index(nameof(Username), IsUnique = true)]
    public class SellerDM : SiffrumDomainModelBase<long>
    {
        [Column("name")]
        public string? Name { get; set; }
        [Column("username")]
        public string? Username { get; set; }

        [Column("store_name")]
        public string? StoreName { get; set; }

        [Column("slug")]
        public string? Slug { get; set; }

        [Column("email")]
        public string? Email { get; set; }

        [Column("mobile")]
        public string? Mobile { get; set; }

        [Column("balance")]
        public double Balance { get; set; } = 0;

        [Column("store_url")]
        public string? StoreUrl { get; set; }

        [Column("logo")]
        public string? Logo { get; set; }

        [Column("store_description")]
        public string? StoreDescription { get; set; }

        [Column("street")]
        public string? Street { get; set; }

        [Column("pincode_id")]
        public long? PincodeId { get; set; }        

        [Column("state")]
        public string? State { get; set; }
        [Column("city")]
        public string? City { get; set; }

        [Column("country")]
        public string? Country { get; set; }

        [Column("categories")]
        public string? Categories { get; set; }

        [Column("account_number")]
        public string? AccountNumber { get; set; }

        [Column("bank_ifsc_code")]
        public string? BankIfscCode { get; set; }

        [Column("account_name")]
        public string? AccountName { get; set; }

        [Column("bank_name")]
        public string? BankName { get; set; }

        [Column("commission")]
        public decimal Commission { get; set; } = 0;

        [Column("status")]
        public SellerStatusDM Status { get; set; }

        [Column("login_status")]
        public LoginStatusDM LoginStatus { get; set; }

        [Column("require_products_approval")]
        public short RequireProductsApproval { get; set; } = 0;

        [Column("fcm_id")]
        public string? FcmId { get; set; }

        [Column("national_identity_card")]
        public string? NationalIdentityCard { get; set; }

        [Column("address_proof")]
        public string? AddressProof { get; set; }

        [Column("pan_number")]
        public string? PanNumber { get; set; }

        [Column("tax_name")]
        public string? TaxName { get; set; }

        [Column("tax_number")]
        public string? TaxNumber { get; set; }

        [Column("customer_privacy")]
        public short? CustomerPrivacy { get; set; } = 0;

        [Column("latitude")]
        public decimal? Latitude { get; set; }

        [Column("longitude")]
        public decimal? Longitude { get; set; }

        [Column("place_name")]
        [MaxLength(191)]
        public string? PlaceName { get; set; }

        [Column("formatted_address")]
        [MaxLength(191)]
        public string? FormattedAddress { get; set; }

        [Column("pending_latitude")]
        public decimal? PendingLatitude { get; set; }

        [Column("pending_longitude")]
        public decimal? PendingLongitude { get; set; }

        [Column("location_status")]
        public short LocationStatus { get; set; } = 0; // 0=None, 1=Pending, 2=Approved, 3=Rejected

        [Column("forgot_password_code")]
        [MaxLength(191)]
        public string? ForgotPasswordCode { get; set; }
        [Column("password")]
        public string? Password { get; set; }

        [Column("view_order_otp")]
        public short ViewOrderOtp { get; set; } = 0;

        [Column("assign_delivery_boy")]
        public short AssignDeliveryBoy { get; set; } = 0;

        [Column("fssai_lic_no")]
        [MaxLength(191)]
        public string? FssaiLicNo { get; set; }

        [Column("self_pickup_mode")]
        public bool SelfPickupMode { get; set; }

        [Column("is_pickup_mode_enabled")]
        public bool IsPickupModeEnabled { get; set; }

        [Column("door_step_mode")]
        public bool DoorStepMode { get; set; } 

        [Column("pickup_store_address")]
        public string? PickupStoreAddress { get; set; }

        [Column("pickup_latitude")]
        public decimal? PickupLatitude { get; set; }

        [Column("pickup_longitude")]
        public decimal? PickupLongitude { get; set; }

        [Column("pickup_store_timings")]
        public string? PickupStoreTimings { get; set; }   

        [Column("deleted_at")]
        public DateTime? DeletedAt { get; set; }

        [Column("remark")]
        public string? Remark { get; set; }
        public RoleTypeDM RoleType { get; set; }

        [Column("change_order_status_delivered")]
        public string? ChangeOrderStatusDelivered { get; set; }
        [Column("is_email_confirmed")]
        public bool IsEmailConfirmed { get; set; }
        [Column("is_mobile_confirmed")]
        public bool IsMobileConfirmed { get; set; }

        [ForeignKey(nameof(Admin))]
        [Column("admin_id")]
        public long? AdminId { get; set; }
        public virtual AdminDM? Admin { get; set; }
        public HashSet<DeliveryPlacesDM> DeliveryPlaces { get; set; }
        public HashSet<SellerSettingsDM> SellerSettings { get; set; }
       
    }
}
