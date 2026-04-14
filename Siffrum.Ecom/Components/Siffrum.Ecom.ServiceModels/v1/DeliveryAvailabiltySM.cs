namespace Siffrum.Ecom.ServiceModels.v1
{
    public class DeliveryAvailabiltySM
    {
        public bool IsDeliveryAvailable { get; set; }

        public string? Address { get; set; }

        public decimal? Latitude { get; set; }

        public decimal? Longitude { get; set; }
        public string? Pincode { get; set; }
        public bool SurgeResponse { get; set; }
    }
}
