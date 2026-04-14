namespace Siffrum.Ecom.ServiceModels.v1
{
    public class ProductSpecificationFilterSM
    {
        public long Id { get; set; }
        public string Name { get; set; }

        public List<ProductSpecificationValueSM> SpecificationValues { get; set; }
    }
}
