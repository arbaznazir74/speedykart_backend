using Microsoft.AspNetCore.Mvc.Filters;

namespace Siffrum.Ecom.BAL.ExceptionHandler
{
    public abstract class APIExceptionFilterRoot : ExceptionFilterAttribute
    {
        public APIExceptionFilterRoot()
        {
        }

        public override void OnException(ExceptionContext context)
        {
            base.OnException(context);
        }
    }
}
