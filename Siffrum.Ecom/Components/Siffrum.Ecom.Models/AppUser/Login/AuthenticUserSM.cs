using CoreVisionServiceModels.Enums;
using Microsoft.AspNetCore.Identity;

namespace CoreVisionServiceModels.AppUser.Login
{
    public class AuthenticUserSM : IdentityUser
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public RoleTypeSM Role { get; set; }
    }
}
