namespace CoreVisionServiceModels.Foundation.Base.CommonResponseRoot
{
    public class IntResponseRoot
    {
        public int IntResponse { get; set; }
        public string? Message { get; set; }
        public IntResponseRoot(int intResponse, string message = "")
        {
            IntResponse = intResponse;
            Message = message;
        }
    }
}
