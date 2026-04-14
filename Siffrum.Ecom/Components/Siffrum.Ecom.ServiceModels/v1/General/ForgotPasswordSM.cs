namespace Siffrum.Ecom.ServiceModels.v1.General
{
    public class ForgotPasswordSM
    {
        public string UserName { get; set; }

        public DateTime Expiry { get; set; }
    }
}
