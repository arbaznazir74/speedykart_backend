namespace Siffrum.Ecom.ServiceModels.v1
{
    public class ComboCategorySM
    {
        public long Id { get; set; }
        public long CategoryId { get; set; }
        public string CategoryName { get; set; }
        public long ComboProductId { get; set; }

        public string ComboName { get; set; }
    }
}
