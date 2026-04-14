using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Siffrum.Ecom.DomainModels.Foundation.Base;
using Siffrum.Ecom.DomainModels.Enums;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Siffrum.Ecom.DomainModels.v1
{
    [Table("delivery_places")]
    public class DeliveryPlacesDM : SiffrumDomainModelBase<long>
    {
        [Required]
        [Column("pincode")]
        [MaxLength(191)]
        public string Pincode { get; set; } = string.Empty;

        [Column("status")]
        public StatusDM Status { get; set; }

        [Column("latitude")]
        [MaxLength(191)]
        public double? Latitude { get; set; }

        [Column("longitude")]
        [MaxLength(191)]
        public double? Longitude { get; set; }

        [Column("delivery_charges")]
        public decimal DeliveryCharges { get; set; }

        [ForeignKey(nameof(Seller))]
        [Column("sellerId")]
        public long SellerId { get; set; }

        public virtual SellerDM Seller { get; set; }
    }
}
