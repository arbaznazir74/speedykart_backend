using Microsoft.AspNetCore.Identity;
using Siffrum.Ecom.ServiceModels.Enums;
using Siffrum.Ecom.ServiceModels.Foundation.Base.CommonResponseRoot;
using Siffrum.Ecom.ServiceModels.Foundation.Base.Enums;
using System.Text.Json.Serialization;

namespace Siffrum.Ecom.ServiceModels.AppUser.Login
{
    public class TokenUserSM 
    {
        public long Id { get; set; }
        public StatusSM Status { get; set; }
        public LoginStatusSM LoginStatus { get; set; }
        public RoleTypeSM RoleType { get; set; }
        public string? Username { get; set; }
        public string Email { get; set; }
        public string? Image { get; set; }

        public string? Mobile { get; set; }
        /*[IgnorePropertyOnWrite(AutoMapConversionType.Dm2SmOnly)]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]*/
        public string? Password { get; set; }
        public bool IsMobileConfirmed { get; set; }
        public bool IsEmailConfirmed { get; set; }
        //public RoleTypeSM Role { get; set; }
        public RoleTypeSM Role
        {
            get => RoleType;
            set => RoleType = value;
        }
    }

}
