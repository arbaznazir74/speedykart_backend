using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using System.Text.Encodings.Web;

namespace Siffrum.Ecom.Foundation.Security
{
    public partial class APIBearerTokenAuthHandler : SiffrumBearerTokenAuthHandlerRoot
    {
        public APIBearerTokenAuthHandler(IOptionsMonitor<SiffrumAuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock, JwtHandler jwtHandler)
            : base(options, logger, encoder, clock, jwtHandler)
        {
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            return base.HandleAuthenticateAsync();
        }

        protected override Task HandleForbiddenAsync(AuthenticationProperties properties)
        {
            return base.HandleForbiddenAsync(properties);
        }

        protected override Task HandleChallengeAsync(AuthenticationProperties properties)
        {
            return base.HandleChallengeAsync(properties);
        }

        private Task<AuthenticationTicket> ValidateTokenAndGetTicket(string tokenString)
        {
            return base.ValidateTokenAndGetTicket(tokenString);
        }

        protected AuthenticateResult GetFailureResult(string message)
        {
            return base.GetFailureResult(message);
        }
    }
}
