namespace Siffrum.Ecom.ServiceModels.v1
{
    public class HotBoxSearchResponseSM
    {
        public List<UserHotBoxProductSM> Products { get; set; } = new();
        public List<ComboProductSM> Combos { get; set; } = new();
    }
}
