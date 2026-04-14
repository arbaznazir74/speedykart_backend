namespace CoreVisionServiceModels.v1.General
{
    public class ForgotPasswordSM
    {
        public string CompanyCode { get; set; }

        public string UserName { get; set; }

        public DateTime Expiry { get; set; }
    }
}
