namespace Siffrum.Ecom.ServiceModels.v1.General
{
    public class ResetPasswordRequestSM
    {
        public string NewPassword { get; set; }
        public string AuthCode { get; set; }
    }
}
