namespace Siffrum.Ecom.ServiceModels.v1
{
    public class UserCategorySummarySM
    {
        public long Id { get; set; }
        public string Name { get; set; } = null!;
        public string? ImageBase64 { get; set; }
        public string? NetworkImage { get; set; }
        public int ProductsCount { get; set; }
    }
}
