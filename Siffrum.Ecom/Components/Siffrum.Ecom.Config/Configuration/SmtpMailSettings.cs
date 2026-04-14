namespace Siffrum.Ecom.Config.Configuration
{
    public class SmtpMailSettings
    {
        public string UserId { get; set; }
        public string Password { get; set; }
        public int Port { get; set; }
        public string Host { get; set; }
        public bool EnableSSL { get; set; }
    }
}
