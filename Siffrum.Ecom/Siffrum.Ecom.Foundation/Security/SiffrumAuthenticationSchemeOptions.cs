using Microsoft.AspNetCore.Authentication;

namespace Siffrum.Ecom.Foundation.Security
{
    public class SiffrumAuthenticationSchemeOptions : AuthenticationSchemeOptions
    {
        public string JwtTokenSigningKey { get; set; }
    }
}
