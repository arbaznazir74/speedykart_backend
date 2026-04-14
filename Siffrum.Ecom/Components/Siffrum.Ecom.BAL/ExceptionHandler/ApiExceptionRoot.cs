using Siffrum.Ecom.ServiceModels.Foundation.Base.Enums;
namespace Siffrum.Ecom.BAL.ExceptionHandler
{
    public class ApiExceptionRoot : Exception
    {
        public ApiErrorTypeSM ExceptionType { get; private set; }

        public string DisplayMessage { get; private set; }

        public bool IsToLogInDb
        {
            get
            {
                return IsToBeLoggedInDb();
            }
            set
            {
                SetIsToBeLoggedInDb(value);
            }
        }

        public ApiExceptionRoot(ApiErrorTypeSM exceptionType, string devMessage, string displayMessage = "", Exception? innerException = null)
            : base(devMessage, innerException)
        {
            DisplayMessage = string.IsNullOrEmpty(displayMessage) ? devMessage : displayMessage;
            ExceptionType = exceptionType;
            if (exceptionType.ToString().Contains("_NoLog"))
            {
                IsToLogInDb = false;
            }
            else
            {
                IsToLogInDb = true;
            }
        }

        private void SetIsToBeLoggedInDb(bool value)
        {
            if (Data.Contains("IsToLogInDb"))
            {
                Data["IsToLogInDb"] = value;
            }
            else
            {
                Data.Add("IsToLogInDb", value);
            }
        }

        private bool IsToBeLoggedInDb()
        {
            if (Data.Contains("IsToLogInDb"))
            {
                if (bool.TryParse(Data["IsToLogInDb"]?.ToString(), out var result))
                {
                    return result;
                }

                return true;
            }

            return true;
        }
    }
}
