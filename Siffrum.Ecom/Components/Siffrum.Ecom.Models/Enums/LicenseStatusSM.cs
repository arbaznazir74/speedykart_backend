namespace CoreVisionServiceModels.Enums
{
    public enum LicenseStatusSM
    {
        Active = 0,           // License is currently valid and in use
        Expired = 1,          // License validity period is over
        Pending = 2,          // License is created but not yet activated
        Cancelled = 3,        // License was cancelled manually
        Suspended = 4,        // Temporarily disabled due to a violation or payment issue
        Used = 5,             // License was consumed (e.g., trial used up)
        Renewed = 6,          // License has been renewed and is awaiting activation or usage
        Deactivated = 7       // Permanently disabled license
    }
}
