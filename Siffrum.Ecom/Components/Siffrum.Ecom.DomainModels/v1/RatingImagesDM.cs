using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Siffrum.Ecom.DomainModels.v1
{
    [Table("rating_images")]
    public class RatingImagesDM
    {
        [Key]
        [Column("id")]
        public long Id { get; set; }
        

        [Required]
        [Column("image")]
        [MaxLength(191)]
        public string Image { get; set; } = string.Empty;
        [ForeignKey(nameof(ProductRating))]

        [Required]
        [Column("product_rating_id")]
        public long ProductRatingId { get; set; }
        public ProductRatingDM ProductRating { get; set; }
    }
}
