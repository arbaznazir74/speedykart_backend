namespace CoreVisionServiceModels.v1.General
{
    public class ResetPasswordRequestSM
    {
        public string NewPassword { get; set; }
        public string authCode { get; set; }
    }
}
