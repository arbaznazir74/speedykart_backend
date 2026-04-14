using Siffrum.Ecom.ServiceModels.Enums;
using Siffrum.Ecom.ServiceModels.Foundation.Base;

namespace Siffrum.Ecom.ServiceModels.v1
{
    public class DeliveryBoySM : SiffrumServiceModelBase<long>
    {
        public string Name { get; set; } = null!;

        public string? Username { get; set; }

        public string Email { get; set; } = null!;

        public string? Mobile { get; set; }

        public string? OrderNote { get; set; }

        public string? Address { get; set; }

        public int BonusType { get; set; }

        public double BonusPercentage { get; set; }

        public double BonusMinAmount { get; set; }

        public double BonusMaxAmount { get; set; }

        public double Balance { get; set; }

        public string? DrivingLicense { get; set; }

        public string? DrivingLicensePhoto { get; set; }

        public string? NationalIdentityCard { get; set; }

        public string? AadhaarNumber { get; set; }

        public string? AadhaarPhoto { get; set; }

        public string? PassportPhoto { get; set; }

        public DateTime? DateOfBirth { get; set; }

        public string? BankAccountNumber { get; set; }

        public string? BankName { get; set; }

        public string? AccountName { get; set; }

        public string? IfscCode { get; set; }

        public string? OtherPaymentInformation { get; set; }

        public DeliveryBoyStatusSM Status { get; set; }

        public LoginStatusSM LoginStatus { get; set; }

        public DeliveryBoyPaymentTypeSM PaymentType { get; set; }
        public int IsAvailable { get; set; }

        public string? FcmId { get; set; }

        public int? PincodeId { get; set; }

        public double CashReceived { get; set; }

        public string? Password { get; set; }

        public string? Remark { get; set; }

        public bool IsEmailConfirmed { get; set; }

        public bool IsMobileConfirmed { get; set; }

        public long? AdminId { get; set; }

        public long? SellerId { get; set; }

        public string? SellerName { get; set; }

        public long? CityId { get; set; }

        public RoleTypeSM RoleType { get; set; }
    }
}
