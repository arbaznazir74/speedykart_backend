using Siffrum.Ecom.ServiceModels.Enums;
using Siffrum.Ecom.ServiceModels.Foundation.Base;

namespace Siffrum.Ecom.ServiceModels.v1
{
    public class SellerSM : SiffrumServiceModelBase<long>
    {
        public string? Name { get; set; }

        public string? Username { get; set; }

        public string? StoreName { get; set; }

        public string? Slug { get; set; }

        public string? Email { get; set; }

        public string? Mobile { get; set; }

        public double Balance { get; set; }

        public string? StoreUrl { get; set; }

        public string? Logo { get; set; }
        public string? NetworkLogo { get; set; }

        public string? StoreDescription { get; set; }

        public string? Street { get; set; }

        public long? PincodeId { get; set; }

        public string? City { get; set; }

        public string? Country { get; set; }
        public long? AdminId { get; set; }


        public string? State { get; set; }

        public string? Categories { get; set; }

        public string? AccountNumber { get; set; }

        public string? BankIfscCode { get; set; }

        public string? AccountName { get; set; }

        public string? BankName { get; set; }
        public decimal Commission { get; set; }
        public SellerStatusSM Status { get; set; }

        public LoginStatusSM LoginStatus { get; set; }

        public short RequireProductsApproval { get; set; }

        public string? FcmId { get; set; }

        public string? NationalIdentityCard { get; set; }
        public string? NetworkNationalIdentityCard { get; set; }

        public string? AddressProof { get; set; }
        public string? NetworkAddressProof { get; set; }

        public string? PanNumber { get; set; }

        public string? TaxName { get; set; }

        public string? TaxNumber { get; set; }

        public short? CustomerPrivacy { get; set; }

        public decimal? Latitude { get; set; }

        public decimal? Longitude { get; set; }

        public string? PlaceName { get; set; }

        public string? FormattedAddress { get; set; }

        public decimal? PendingLatitude { get; set; }

        public decimal? PendingLongitude { get; set; }

        public short LocationStatus { get; set; } // 0=None, 1=Pending, 2=Approved, 3=Rejected

        public string? ForgotPasswordCode { get; set; }

        public string? Password { get; set; }

        public short ViewOrderOtp { get; set; }

        public short AssignDeliveryBoy { get; set; }

        public string? FssaiLicNo { get; set; }

        public bool SelfPickupMode { get; set; }
        public bool IsPickupModeEnabled { get; set; }

        public bool DoorStepMode { get; set; }

        public string? PickupStoreAddress { get; set; }

        public decimal? PickupLatitude { get; set; }

        public decimal? PickupLongitude { get; set; }

        public string? PickupStoreTimings { get; set; }        

        public DateTime? DeletedAt { get; set; }

        public string? Remark { get; set; }

        public RoleTypeSM RoleType { get; set; }

        public string? ChangeOrderStatusDelivered { get; set; }

        public bool IsEmailConfirmed { get; set; }

        public bool IsMobileConfirmed { get; set; }
    }
}
