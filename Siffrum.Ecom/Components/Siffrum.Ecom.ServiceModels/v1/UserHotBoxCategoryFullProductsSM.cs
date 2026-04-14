namespace Siffrum.Ecom.ServiceModels.v1
{
    public class UserHotBoxCategoryFullProductsSM
    {
        public long Id { get; set; }
        public string CategoryName { get; set; }
        public string? ImageBase64 { get; set; }
        public List<HotBoxProductVariantSM> Products { get; set; }
        public List<ComboProductSM> ComboProducts { get; set; }
    }
}
