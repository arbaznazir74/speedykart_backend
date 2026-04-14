using CoreVisionServiceModels.AppUser.Login;

namespace CoreVisionServiceModels.Foundation.Token
{
    public class TokenResponseSM : TokenResponseRoot
    {
        public LoginUserSM LoginUserDetails { get; set; }
        public string SuccessMessage { get; set; }
        public int ClientCompanyId { get; set; }
    }
}
