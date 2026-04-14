namespace CoreVisionServiceModels.Foundation.Base.CommonResponseRoot
{
    public class DeleteResponseRoot
    {
        public bool DeleteResult { get; set; }

        public string DeleteMessage { get; set; }

        public DeleteResponseRoot(bool deleteResult, string deleteMessage = "")
        {
            DeleteResult = deleteResult;
            DeleteMessage = deleteMessage;
        }
    }
}
