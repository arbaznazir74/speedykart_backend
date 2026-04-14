namespace Siffrum.Ecom.ServiceModels.AppUser.Login
{
    public class SendNotificationMessageSM
    {
        public List<long> UserIds { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public Dictionary<string, string> AdditionalData { get; set; }
    }

}
