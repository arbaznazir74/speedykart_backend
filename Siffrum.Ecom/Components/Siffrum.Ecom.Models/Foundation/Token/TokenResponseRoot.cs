namespace CoreVisionServiceModels.Foundation.Token
{
    public class TokenResponseRoot
    {
        public string AccessToken { get; set; }

        public DateTime ExpiresUtc { get; set; }
    }
}