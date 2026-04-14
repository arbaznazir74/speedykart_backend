namespace CoreVisionServiceModels.Foundation.Base.CommonResponseRoot
{
    public class BoolResponseRoot
    {
        public bool BoolResponse { get; set; }

        public string ResponseMessage { get; set; }

        public BoolResponseRoot(bool boolValue, string responseMessage = "")
        {
            BoolResponse = boolValue;
            ResponseMessage = responseMessage;
        }
    }
}
