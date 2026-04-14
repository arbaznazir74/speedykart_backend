namespace Siffrum.Ecom.Foundation.AutoMapperBindings
{
    public class AutoMappingResponse
    {
        public List<string> SuccessDmToSmMaps { get; set; }

        public List<string> SuccessSmToDmMaps { get; set; }

        public List<string> UnsuccessfullPaths { get; set; }

        public AutoMappingResponse()
        {
            SuccessDmToSmMaps = new List<string>();
            SuccessSmToDmMaps = new List<string>();
            UnsuccessfullPaths = new List<string>();
        }
    }
}
