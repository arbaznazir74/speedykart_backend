namespace Siffrum.Ecom.Config.Configuration
{
    public class APIConfiguration : APIConfigRoot
    {
        #region General Config Settings
        public string ApiDbConnectionString { get; set; }
        public string JwtTokenSigningKey { get; set; }
        public double DefaultTokenValidityDays { get; set; }
        public string JwtIssuerName { get; set; }
        public string AuthTokenEncryptionKey { get; set; }
        public string AuthTokenDecryptionKey { get; set; }
        public string SuperAdminUserAdditionKey { get; set; }
        public SuperAdminSettings SuperAdminSettings { get; set; }

        #region External App Integration
        //public ExternalIntegrations ExternalIntegrations { get; set; }

        #endregion External App Integration

        #endregion General Config Settings

        #region SmtpMail Settings
        public SmtpMailSettings SmtpMailSettings { get; set; }
        public AppleAuth AppleAuth { get; set; }
        public RazorpaySettings RazorpaySettings { get; set; }
        public GoogleCloudLocation GoogleCloudLocation { get; set; }
        public OneSignalSettings OneSignalSettings { get; set; }
        public SmsSettings SmsSettings { get; set; }


        #endregion
    }
}
