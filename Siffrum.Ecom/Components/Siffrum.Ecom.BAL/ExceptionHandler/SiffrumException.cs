using Siffrum.Ecom.ServiceModels.Foundation.Base.Enums;
namespace Siffrum.Ecom.BAL.ExceptionHandler
{
    public class SiffrumException : ApiExceptionRoot
    {
        
        public SiffrumException(ApiErrorTypeSM exceptionType, string devMessage,
           string displayMessage = "", Exception innerException = null)
            : base(exceptionType, devMessage, displayMessage, innerException)
        { }
    }
}
