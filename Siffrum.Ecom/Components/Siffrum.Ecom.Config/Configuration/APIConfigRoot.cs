namespace Siffrum.Ecom.Config.Configuration
{
    public abstract class APIConfigRoot
    {
        public bool IsSwaggerEnabled { get; set; }

        public bool IsCorsEnabled { get; set; }

        public bool IsOdataEnabled { get; set; }

        public bool EnableFullRequestResponseLog { get; set; }

        public bool EnableLogForNoLog { get; set; }

        public string FileExceptionLogPath { get; set; }
    }
}
