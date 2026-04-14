
using CoreVisionServiceModels.Enums;

namespace CoreVisionServiceModels.v1.General.License
{
    public class CheckoutSessionRequestSM
    {
        //public string ProductId { get; set; }
        public string PriceId { get; set; }
        public string SuccessUrl { get; set; }
        public string FailureUrl { get; set; }
        public PaymentMode PaymentMode { get; set; }
    }
}
