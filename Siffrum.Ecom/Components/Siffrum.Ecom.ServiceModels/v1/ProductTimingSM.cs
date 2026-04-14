namespace Siffrum.Ecom.ServiceModels.v1
{
    public class ProductTimingSM
    {
        public long Id { get; set; }
        public long SellerId { get; set; }
        public long ProductId { get; set; }
        public long CategoryId { get; set; }
        public int StartHour { get; set; }
        public int StartMinute { get; set; }
        public int EndHour { get; set; }
        public int EndMinute { get; set; }
        public bool IsActive { get; set; }

        // Read-only display fields
        public string? ProductName { get; set; }
        public string? CategoryName { get; set; }
    }
}
