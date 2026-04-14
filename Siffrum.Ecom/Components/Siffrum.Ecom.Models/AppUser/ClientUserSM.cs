using CoreVisionServiceModels.AppUser.Login;
using CoreVisionServiceModels.Enums;

namespace CoreVisionServiceModels.AppUser
{
    public class ClientUserSM : LoginUserSM
    {
        public GenderSM Gender { get; set; }
        public int? ClientCompanyDetailId { get; set; }

    }
}
