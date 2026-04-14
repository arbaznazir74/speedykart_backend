using Siffrum.Ecom.DomainModels.Enums;
using Siffrum.Ecom.DomainModels.Foundation.Base;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Siffrum.Ecom.DomainModels.v1
{
    [Table("user_addresses")]
    public class UserAddressDM : SiffrumDomainModelBase<long>
    {       

        [Required]
        [Column("type")]
        public AddressTypeDM Type { get; set; }

        [Required]
        [Column("name")]
        [MaxLength(191)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [Column("mobile")]
        [MaxLength(191)]
        public string Mobile { get; set; } = string.Empty;

        [Column("alternate_mobile")]
        [MaxLength(191)]
        public string? AlternateMobile { get; set; }

        [Required]
        [Column("address")]
        public string Address { get; set; } = string.Empty;

        [Required]
        [Column("landmark")]
        public string Landmark { get; set; } = string.Empty;

        [Required]
        [Column("area")]
        [MaxLength(191)]
        public string Area { get; set; }

        [Required]
        [Column("pincode")]
        [MaxLength(191)]
        public string Pincode { get; set; }

        [Required]
        [Column("city")]
        [MaxLength(191)]
        public string City { get; set; }

        [Required]
        [Column("state")]
        [MaxLength(191)]
        public string State { get; set; } = string.Empty;

        [Required]
        [Column("country")]
        [MaxLength(191)]
        public string Country { get; set; } = string.Empty;

        [Column("is_default")]
        public bool IsDefault { get; set; }

        [Column("latitude")]
        [MaxLength(191)]
        public double Latitude { get; set; }

        [Column("longitude")]
        [MaxLength(191)]
        public double Longitude { get; set; }
        [ForeignKey(nameof(User))]
        [Required]
        [Column("user_id")]
        public long UserId { get; set; }
        public UserDM User { get; set; }
    }
}
