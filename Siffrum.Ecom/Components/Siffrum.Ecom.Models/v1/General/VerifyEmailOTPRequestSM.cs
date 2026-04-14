namespace CoreVisionServiceModels.v1.General
{
    public class VerifyEmailOTPRequestSM
    {
        public string Email { get; set; }

        public long OTP { get; set; }
    }
}
