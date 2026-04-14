using Siffrum.Ecom.DomainModels.Foundation.Base;
using System.ComponentModel.DataAnnotations.Schema;

namespace Siffrum.Ecom.DomainModels.v1
{
    [Table("settings")]
    public class SettingsDM : SiffrumDomainModelBase<long>
    {
        [Column("json-data")]
        public string JsonData { get; set; }
    }
}
