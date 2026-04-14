using System.Security.Claims;
using Siffrum.Ecom.ServiceModels.Enums;
using Siffrum.Ecom.ServiceModels.Foundation.Base.Interfaces;
namespace Siffrum.Ecom.Foundation.MiddleWare
{
    public class LoginUserDetailMiddleware
    {
        private readonly RequestDelegate _next;

        public LoginUserDetailMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, ILoginUserDetail loginUserDetail)
        {
            if (context.User.Identity.IsAuthenticated)
            {
                var claimsPrincipal = context.User;

                // Use the correct claim types based on what you provided
                var nameClaim = claimsPrincipal.FindFirst(ClaimTypes.Name)?.Value;
                var roleClaim = claimsPrincipal.FindFirst(ClaimTypes.Role)?.Value;
                var dbRecordIdClaim = claimsPrincipal.FindFirst("dbRid")?.Value;
                if (!string.IsNullOrEmpty(dbRecordIdClaim) && int.TryParse(dbRecordIdClaim, out var dbRecordId))
                {
                    loginUserDetail.DbRecordId = dbRecordId;
                }

                loginUserDetail.LoginId = nameClaim;

                if (!string.IsNullOrEmpty(roleClaim) && Enum.TryParse<RoleTypeSM>(roleClaim, out var userType))
                {
                    loginUserDetail.UserType = userType;
                }
            }

            await _next(context);
        }
    }
}
