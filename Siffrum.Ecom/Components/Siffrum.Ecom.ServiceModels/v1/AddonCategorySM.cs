namespace Siffrum.Ecom.ServiceModels.v1
{
    public class AddonCategorySM
    {
        public long CategoryId { get; set; }
        public string CategoryName { get; set; }
        public List<AddonProductItemSM> Products { get; set; } = new();
    }
}