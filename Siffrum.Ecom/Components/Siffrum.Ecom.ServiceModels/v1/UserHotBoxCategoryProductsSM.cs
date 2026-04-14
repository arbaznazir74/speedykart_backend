namespace Siffrum.Ecom.ServiceModels.v1
{
    public class UserHotBoxCategoryProductsSM
    {
        public long Id { get; set; }
        public string CategoryName { get; set; }

        public string? ImageBase64 { get; set; }
        public string? NetworkImage { get; set; }
        public List<UserHotBoxProductSM> Products { get; set; }

        public List<ComboProductSM> ComboProducts { get; set; }
    }
}
