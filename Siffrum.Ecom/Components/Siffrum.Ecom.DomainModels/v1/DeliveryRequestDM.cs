using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Siffrum.Ecom.DomainModels.Foundation.Base;
using Siffrum.Ecom.DomainModels.Enums;

namespace Siffrum.Ecom.DomainModels.v1
{
    [Table("DeliveryRequest")]
    public class DeliveryRequestDM : SiffrumDomainModelBase<long>
    {

        [Required]
        [Column("user_id")]
        public long UserId { get; set; }

        [ForeignKey(nameof(UserId))]
        public virtual UserDM User { get; set; }

        // 🔹 Location Details
        [Required]
        [MaxLength(10)]
        [Column("pincode")]
        public string Pincode { get; set; }

        [Column("latitude")]
        [MaxLength(191)]
        public double Latitude { get; set; }

        [Column("longitude")]
        [MaxLength(191)]
        public double Longitude { get; set; }

        [MaxLength(250)]
        [Column("address")]
        public string? Address { get; set; }

        [Column("platform")]
        public PlatformTypeDM Platform { get; set; }
        [Column("admin_remarks")]
        public string AdminRemarks { get; set; }

        [Column("is_resolved")]
        public bool IsResolved { get; set; } = false;

    }
}
