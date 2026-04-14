using Siffrum.Ecom.DomainModels.Foundation.Base;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Siffrum.Ecom.DomainModels.v1
{
    [Table("combo_products")]
    public class ComboProductDM : SiffrumDomainModelBase<long>
    {
        [Column("is_in_hot_box")]
        public bool IsInHotBox { get; set; }

        [Column("product_ids")]
        public string ProductIds { get; set; }
        [Column("items")]
        public int TotalProducts{ get; set; }
        [Column("best_for")]
        public int BestFor { get; set; }
        [Column("name")]
        public string Name { get; set; }
        [Column("description")]
        public string Description { get; set; }
        [Column("json_details")]
        public string JsonDetails { get; set; }
        public ICollection<ComboCategoryDM> ComboCategory { get; set; }

    }
}
