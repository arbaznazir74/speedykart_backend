using FirebaseAdmin.Auth;
using Google.Apis.Auth;
using Google.Apis.Auth.OAuth2.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Siffrum.Ecom.BAL.AppUsers;
using Siffrum.Ecom.BAL.ExceptionHandler;
using Siffrum.Ecom.BAL.LoginUsers;
using Siffrum.Ecom.BAL.Token;
using Siffrum.Ecom.Config.Configuration;
using Siffrum.Ecom.Foundation.Controllers.Base;
using Siffrum.Ecom.Foundation.Security;
using Siffrum.Ecom.ServiceModels.AppUser;
using Siffrum.Ecom.ServiceModels.AppUser.Login;
using Siffrum.Ecom.ServiceModels.Enums;
using Siffrum.Ecom.ServiceModels.Foundation.Base.CommonResponseRoot;
using Siffrum.Ecom.ServiceModels.Foundation.Base.Enums;
using Siffrum.Ecom.ServiceModels.Foundation.Token;
using Siffrum.Ecom.ServiceModels.v1;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Json;
namespace Siffrum.Ecom.Foundation.Controllers.Token
{
    public partial class TokenController : ApiControllerRoot
    {
        #region Properties

        private readonly TokenProcess _tokenProcess;
        private readonly JwtHandler _jwtHandler;
        private readonly APIConfiguration _apiConfiguration;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ExternalUserProcess _socialLoginProcess;
        private readonly UserProcess _userProcess;

        #endregion Properties

        #region Constructor
        public TokenController(TokenProcess TokenProcess, JwtHandler jwtHandler, ExternalUserProcess socialLoginProcess,UserProcess userProcess,
            APIConfiguration aPIConfiguration, IHttpClientFactory httpClientFactory)
        {
            _tokenProcess = TokenProcess;
            _jwtHandler = jwtHandler;
            _apiConfiguration = aPIConfiguration;
            _httpClientFactory = httpClientFactory;
            _socialLoginProcess = socialLoginProcess;
            _userProcess = userProcess;
        }
        #endregion Constructor

        #region Validate Login And Generate Token 


        [HttpPost]
        [Route("login")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse<TokenResponseSM>>> ValidateLoginAndGenerateToken(ApiRequest<TokenRequestSM> apiRequest)
        {
            #region Check Request

            var innerReq = apiRequest?.ReqData;
            if (innerReq == null)
            {
                return BadRequest(ModelConverter.FormNewErrorResponse(DomainConstants.DisplayMessagesRoot.Display_ReqDataNotFormed, ApiErrorTypeSM.InvalidInputData_Log));
            }

            if (string.IsNullOrWhiteSpace(innerReq.LoginId) || /*string.IsNullOrWhiteSpace(innerReq.Password) ||*/ innerReq.RoleType == RoleTypeSM.Unknown)
            {
                return BadRequest(ModelConverter.FormNewErrorResponse(DomainConstants.DisplayMessages.Display_InvalidRequiredDataInputs));
            }

            #endregion Check Request

            (TokenUserSM userSM, long adminId) = await _tokenProcess.ValidateLoginAndGenerateToken(innerReq);
            if (userSM == null)
            {
                return NotFound(ModelConverter.FormNewErrorResponse("Invalid Credentials",
                    ApiErrorTypeSM.InvalidInputData_Log));
            }
            else if (userSM.LoginStatus == LoginStatusSM.PasswordResetRequired)
            {
                return Unauthorized(ModelConverter.FormNewErrorResponse(DomainConstants.DisplayMessages.Display_UserPasswordResetRequired, ApiErrorTypeSM.Access_Denied_Log));
            }
            else if (userSM.LoginStatus == LoginStatusSM.Disabled)
            {
                return Unauthorized(ModelConverter.FormNewErrorResponse(DomainConstants.DisplayMessages.Display_UserPasswordResetRequired, ApiErrorTypeSM.Access_Denied_Log));
            }
            else if (userSM.RoleType != RoleTypeSM.SuperAdmin &&
                userSM.RoleType != RoleTypeSM.SystemAdmin &&
                !userSM.IsEmailConfirmed)
            {
                return Unauthorized(ModelConverter.FormNewErrorResponse(
                    DomainConstants.DisplayMessages.Display_UserNotVerified,
                    ApiErrorTypeSM.Access_Denied_Log));
            }
            else
            {
                ICollection<Claim> claims = new List<Claim>()
                {
                    new Claim(ClaimTypes.Name,innerReq.LoginId),
                    new Claim(ClaimTypes.Role,userSM.RoleType.ToString()),
                    new Claim(ClaimTypes.Email,userSM.Email),
                    new Claim(DomainConstants.ClaimsRoot.Claim_DbRecordId,userSM.Id.ToString())
                };
                if (adminId != default)
                {
                    claims.Add(new Claim(DomainConstants.ClaimsRoot.Claim_AdminId, adminId.ToString()));
                }

                var expiryDate = DateTime.Now.AddDays(_apiConfiguration.DefaultTokenValidityDays);

                var token = await _jwtHandler.ProtectAsync(_apiConfiguration.JwtTokenSigningKey, claims, new DateTimeOffset(DateTime.Now), new DateTimeOffset(expiryDate), "Siffrum");
                // here if user is derived class, all properties will be sent
                userSM.Password = null;  //Handle password here
                var tokenResponse = new TokenResponseSM()
                {
                    AccessToken = token,
                    LoginUserDetails = userSM,
                    ExpiresUtc = expiryDate,
                    SuccessMessage = "Login Successful"
                };

                return Ok(ModelConverter.FormNewSuccessResponse(tokenResponse));
            }
        }

        [HttpPost("social-login")]
        [HttpPost("social-signup")]
        public async Task<ActionResult<ApiResponse<TokenResponseSM>>> GenerateSocialLoginToken(ApiRequest<SocialLoginSM> apiRequest)
        {
            #region Check Request

            var innerReq = apiRequest?.ReqData;
            if (innerReq == null)
            {
                return BadRequest(ModelConverter.FormNewErrorResponse(DomainConstants.DisplayMessagesRoot.Display_ReqDataNotFormed, ApiErrorTypeSM.InvalidInputData_Log));
            }
            DecodedTokenSM decodedToken = new DecodedTokenSM();

            if (innerReq.ExternalUserType == ExternalUserTypeSM.Google)
            {
                var payload = await GoogleJsonWebSignature.ValidateAsync(innerReq.IdToken);
                if (payload == null)
                {
                    throw new SiffrumException(ApiErrorTypeSM.Fatal_Log,
                        $"Error in decoding Google id token, token is {innerReq.IdToken}",
                        "We couldn't verify your Google login. Please try again.");
                }
                if (string.IsNullOrEmpty(payload.Email))
                {
                    throw new SiffrumException(ApiErrorTypeSM.Fatal_Log,
                       $"Google ID token is missing an email address. This may occur if the user denied email permissions or if there was an issue with token generation. Token: {innerReq.IdToken}",
                       "We couldn't retrieve your email from Google. Please check your Google account settings and try again.");
                }
                try
                {
                    using (var httpClient = _httpClientFactory.CreateClient())
                    {
                        var imageBytes = await httpClient.GetByteArrayAsync(payload?.Picture);
                        decodedToken.ImageBase64 = Convert.ToBase64String(imageBytes);
                    }
                }
                catch (Exception ex)
                {
                    decodedToken.ImageBase64 = null;
                }
                decodedToken.Email = payload.Email;
                var tokenResponse = await GenerateSocialLoginTokenResponse(decodedToken, innerReq);

                return Ok(ModelConverter.FormNewSuccessResponse(tokenResponse));

            }
            else if (innerReq.ExternalUserType == ExternalUserTypeSM.Apple)
            {
                if (string.IsNullOrEmpty(innerReq.IdToken))
                {
                    throw new SiffrumException(ApiErrorTypeSM.InvalidInputData_Log,
                        "Apple id token missing", "Apple login failed");
                }                   

                var appleUser = await ValidateAppleToken(innerReq.IdToken);
                if (!string.IsNullOrEmpty(appleUser.Email))
                {
                    decodedToken.Email = appleUser.Email;
                    var tokenResponse = await GenerateSocialLoginTokenResponse(decodedToken, innerReq);
                    return Ok(ModelConverter.FormNewSuccessResponse(tokenResponse));
                }
                else if (!string.IsNullOrEmpty(appleUser.AppleUserId))
                {
                    var email = await _socialLoginProcess.GetExternalUserEmailByAppleUserIdToken(innerReq.IdToken, innerReq.RoleType);
                    if (string.IsNullOrEmpty(email))
                    {
                        throw new SiffrumException(ApiErrorTypeSM.InvalidInputData_Log,
                        "No User with Apple id token", "Apple login failed");
                    }
                    decodedToken.Email = email;
                    var appleIdResponse = await GenerateSocialLoginTokenResponse(decodedToken, innerReq);
                    return Ok(ModelConverter.FormNewSuccessResponse(appleIdResponse));
                    
                }
                else
                {
                    throw new SiffrumException(ApiErrorTypeSM.Fatal_Log,
                        "Apple user id missing", "Apple login failed");
                }
                
            }
            else if(innerReq.ExternalUserType == ExternalUserTypeSM.Facebook)
            {
                if (string.IsNullOrEmpty(innerReq.IdToken))
                {
                    throw new SiffrumException(ApiErrorTypeSM.Fatal_Log, "Id Token cannot be null or empty", "Id Token cannot be null or empty");
                }

                var httpClient = _httpClientFactory.CreateClient();

                var userInfoUrl = $"https://graph.facebook.com/me?fields=id,name,email,first_name,last_name,picture&access_token={innerReq.IdToken}";
                var userInfoResponse = await httpClient.GetStringAsync(userInfoUrl);

                using var userInfoDoc = JsonDocument.Parse(userInfoResponse);
                var root = userInfoDoc.RootElement;

                // Check if the necessary properties exist before accessing them
                //string userId = root.TryGetProperty("id", out var idProperty) ? idProperty.GetString() : null;
                string emailId = root.TryGetProperty("email", out var emailProperty) ? emailProperty.GetString() : null;
                //string firstName = root.TryGetProperty("first_name", out var firstNameProperty) ? firstNameProperty.GetString() : null;
                //string lastName = root.TryGetProperty("last_name", out var lastNameProperty) ? lastNameProperty.GetString() : null;
                string pictureUrl = root.TryGetProperty("picture", out var pictureProperty) &&
                                    pictureProperty.TryGetProperty("data", out var dataProperty) ?
                                    dataProperty.GetProperty("url").GetString() : null;
                
                try
                {
                    using (var httpClients = _httpClientFactory.CreateClient())
                    {
                        var imageBytes = await httpClient.GetByteArrayAsync(pictureUrl);
                        decodedToken.ImageBase64 = Convert.ToBase64String(imageBytes);
                    }
                }
                catch (Exception ex)
                {
                    decodedToken.Email = null;
                }
                decodedToken.Email = emailId;
                var tokenResponse = await GenerateSocialLoginTokenResponse(decodedToken, innerReq);

                return Ok(ModelConverter.FormNewSuccessResponse(tokenResponse));
            }
            else
            {
                return BadRequest(ModelConverter.FormNewErrorResponse(DomainConstants.DisplayMessagesRoot.Display_ReqDataNotFormed, ApiErrorTypeSM.InvalidInputData_Log));
            }

            #endregion Check Request
        }

        #region Phone Login

        [HttpPost("phone-login")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse<TokenResponseSM>>> PhoneLogin([FromBody] ApiRequest<OTPRegistrationSM> apiRequest)
        {
            #region Check Request

            var innerReq = apiRequest?.ReqData;
            if (innerReq == null)
            {
                return BadRequest(ModelConverter.FormNewErrorResponse(DomainConstants.DisplayMessagesRoot.Display_ReqDataNotFormed, ApiErrorTypeSM.InvalidInputData_NoLog));
            }

            #endregion Check Request         

            var userSM = await _userProcess.GetUserDetailsByOtpVerification(innerReq);
            if (userSM == null)
            {
                return NotFound(ModelConverter.FormNewErrorResponse("User Not Found, Please provide valid details for login your account",
                    ApiErrorTypeSM.InvalidInputData_Log));
            }
            long adminId = default;
            var tokenResponse = await GeneratePhoneLoginTokenResponse(userSM, adminId);
            if (tokenResponse == null)
            {
                return NotFound(ModelConverter.FormNewErrorResponse("User Not Found, Please provide valid details for login your account",
                    ApiErrorTypeSM.InvalidInputData_Log));
            }
            return Ok(ModelConverter.FormNewSuccessResponse(tokenResponse));
        }

        #endregion Phone Login


        #region Social Login Token

        private async Task<TokenResponseSM> GenerateSocialLoginTokenResponse(DecodedTokenSM decodedToken, SocialLoginSM loginRequest)
        {
            (TokenUserSM userSM, long adminId) = await _socialLoginProcess.AddSocialLoginAndUserDetails(decodedToken, loginRequest);
            if (userSM == null)
            {
                return null;
            }
            else
            {
                ICollection<Claim> claims = new List<Claim>()
                {
                    new Claim(ClaimTypes.Name,userSM.Email),
                    new Claim(ClaimTypes.Role,userSM.RoleType.ToString()),
                    new Claim(ClaimTypes.Email,userSM.Email),
                    new Claim(DomainConstants.ClaimsRoot.Claim_DbRecordId,userSM.Id.ToString())
                };
                if (adminId != default)
                {
                    claims.Add(new Claim(DomainConstants.ClaimsRoot.Claim_AdminId, adminId.ToString()));
                }

                var expiryDate = DateTime.Now.AddDays(_apiConfiguration.DefaultTokenValidityDays);

                var token = await _jwtHandler.ProtectAsync(_apiConfiguration.JwtTokenSigningKey, claims, new DateTimeOffset(DateTime.Now), new DateTimeOffset(expiryDate), "Siffrum");
                // here if user is derived class, all properties will be sent
                userSM.Password = null;  //Handle password here
                var tokenResponse = new TokenResponseSM()
                {
                    AccessToken = token,
                    LoginUserDetails = userSM,
                    ExpiresUtc = expiryDate,
                    SuccessMessage = "Login Successful"
                };
                return tokenResponse;
            }
        }

        private async Task<TokenResponseSM> GeneratePhoneLoginTokenResponse(TokenUserSM userSM, long adminId)
        {
            if (userSM == null)
            {
                return null;
            }            
            else
            {
                if (string.IsNullOrEmpty(userSM.Email))
                {
                    userSM.Email = "";
                }
                ICollection<Claim> claims = new List<Claim>()
                {
                    new Claim(ClaimTypes.Name,userSM.Mobile),
                    new Claim(ClaimTypes.Role,userSM.RoleType.ToString()),
                    new Claim(ClaimTypes.Email,userSM.Email),
                    new Claim(DomainConstants.ClaimsRoot.Claim_DbRecordId,userSM.Id.ToString())
                };
                if (adminId != default)
                {
                    claims.Add(new Claim(DomainConstants.ClaimsRoot.Claim_AdminId, adminId.ToString()));
                }

                var expiryDate = DateTime.Now.AddDays(_apiConfiguration.DefaultTokenValidityDays);

                var token = await _jwtHandler.ProtectAsync(_apiConfiguration.JwtTokenSigningKey, claims, new DateTimeOffset(DateTime.Now), new DateTimeOffset(expiryDate), "Siffrum");
                // here if user is derived class, all properties will be sent
                userSM.Password = null;  //Handle password here
                var tokenResponse = new TokenResponseSM()
                {
                    AccessToken = token,
                    LoginUserDetails = userSM,
                    ExpiresUtc = expiryDate,
                    SuccessMessage = "Login Successful"
                };
                return tokenResponse;
            }
        }
        #endregion Social Login Token

        #region Apple Token Handler
        private async Task<AppleUserInfoSM> ValidateAppleToken(string identityToken)
        {
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(identityToken);
            if (jwtToken.Issuer != "https://appleid.apple.com")
            {
                throw new SiffrumException(ApiErrorTypeSM.Fatal_Log,
                    $"Invalid issuer for apple Login, JWT issue is: {jwtToken.Issuer}",
                    "Apple login failed");
            }
            var clientId = _apiConfiguration.AppleAuth.ClientId;
            if (!jwtToken.Audiences.Contains(clientId))
            {
                throw new SiffrumException(ApiErrorTypeSM.Fatal_Log,
                    $"Inavlid audience for apple Login, JWT audience is: {jwtToken.Audiences.FirstOrDefault()}",
                    "Apple login failed");
            }
            var email = jwtToken.Claims.FirstOrDefault(c => c.Type == "email")?.Value;
            var appleUserId = jwtToken.Claims.First(c => c.Type == "sub").Value;
            return new AppleUserInfoSM
            {
                Email = email,
                AppleUserId = appleUserId
            };
        }
        


        #endregion Apple Token Handler

        #endregion Validate Login And Generate Token 

    }
}
