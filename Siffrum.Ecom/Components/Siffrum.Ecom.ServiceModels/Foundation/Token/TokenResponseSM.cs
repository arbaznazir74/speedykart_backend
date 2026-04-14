using Siffrum.Ecom.ServiceModels.AppUser.Login;

namespace Siffrum.Ecom.ServiceModels.Foundation.Token
{
    public class TokenResponseSM : TokenResponseRoot
    {
        public TokenUserSM LoginUserDetails { get; set; }
        public string SuccessMessage { get; set; }
    }
}
