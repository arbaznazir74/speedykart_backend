using Siffrum.Ecom.DomainModels.Enums;
using Siffrum.Ecom.DomainModels.Foundation.Base;
using System.ComponentModel.DataAnnotations.Schema;

namespace Siffrum.Ecom.DomainModels.AppUser
{
    [Table("external_user")]
    public class ExternalUserDM : SiffrumDomainModelBase<long>
    {        
        [Column("id_token")]
        public string IdToken { get; set; }

        [Column("user_id")]
        public long UserId { get; set; }

        [Column("external_user_type")]
        public ExternalUserTypeDM ExternalUserType { get; set; }

        [Column("role_type")]
        public RoleTypeDM RoleType { get; set; }        

    }
}
