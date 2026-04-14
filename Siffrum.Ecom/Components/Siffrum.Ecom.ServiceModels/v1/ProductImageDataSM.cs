namespace Siffrum.Ecom.ServiceModels.v1
{
    public class ProductImageDataSM 
    {
        public long Id { get; set; }
        public string? Name { get; set; }
        public string? ImageBase64 { get; set; }
        public string? NetworkImage { get; set; }
    }
}
