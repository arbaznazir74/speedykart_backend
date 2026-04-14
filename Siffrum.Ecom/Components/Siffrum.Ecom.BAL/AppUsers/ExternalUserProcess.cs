using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Siffrum.Ecom.BAL.ExceptionHandler;
using Siffrum.Ecom.BAL.Foundation.Base;
using Siffrum.Ecom.BAL.LoginUsers;
using Siffrum.Ecom.DAL.Context;
using Siffrum.Ecom.DomainModels.AppUser;
using Siffrum.Ecom.DomainModels.Enums;
using Siffrum.Ecom.DomainModels.v1;
using Siffrum.Ecom.ServiceModels.AppUser;
using Siffrum.Ecom.ServiceModels.AppUser.Login;
using Siffrum.Ecom.ServiceModels.Enums;
using Siffrum.Ecom.ServiceModels.Foundation.Base.Enums;
using Siffrum.Ecom.ServiceModels.Foundation.Base.Interfaces;
using Siffrum.Ecom.ServiceModels.Foundation.Token;
using Siffrum.Ecom.ServiceModels.v1;

namespace Siffrum.Ecom.BAL.AppUsers
{
    public class ExternalUserProcess : SiffrumBalBase
    {
        #region Fields

        private readonly ILoginUserDetail _loginUserDetail;
        private readonly UserProcess _userProcess;
        private readonly SellerProcess _sellerProcess;

        #endregion

        #region Constructor

        public ExternalUserProcess(
            IMapper mapper,
            ApiDbContext context,
            UserProcess userProcess,
            SellerProcess sellerProcess,
            ILoginUserDetail loginUserDetail)
            : base(mapper, context)
        {
            _loginUserDetail = loginUserDetail;
            _userProcess = userProcess;
            _sellerProcess = sellerProcess;
        }

        #endregion

        #region Get

        public async Task<ExternalUserSM?> GetExternalUserByClientUserIdandTypeAsync(
            long userId,
            ExternalUserTypeSM userType,
            RoleTypeSM roleType)
        {
            var dm = await _apiDbContext.ExternalUsers
                .AsNoTracking()
                .FirstOrDefaultAsync(x =>
                    x.UserId == userId &&
                    x.ExternalUserType == (ExternalUserTypeDM)userType &&
                    x.RoleType == (RoleTypeDM)roleType);

            return dm == null ? null : _mapper.Map<ExternalUserSM>(dm);
        }

        public async Task<string?> GetExternalUserEmailByAppleUserIdToken(
            string appleUserIdToken,
            RoleTypeSM roleType)
        {
            var dm = await _apiDbContext.ExternalUsers
                .AsNoTracking()
                .FirstOrDefaultAsync(x =>
                    x.IdToken == appleUserIdToken &&
                    x.ExternalUserType == ExternalUserTypeDM.Apple &&
                    x.RoleType == (RoleTypeDM)roleType);

            if(dm == null)
            {
                return null;
            }
            string email = string.Empty;
            if(roleType == RoleTypeSM.Seller)
            {
                var seller = await _sellerProcess.GetByIdAsync(dm.UserId);
                if(seller == null)
                {
                    return null;
                }
                email = seller.Email;
            }
            else if(roleType == RoleTypeSM.User)
            {
                var user = await _userProcess.GetByIdAsync(dm.UserId);
                if(user == null)
                {
                    return null;
                }
                email = user.Email;
            }
            else
            {
                return null;
            }
            return email;
        }

        #endregion

        #region Add External User

        private async Task<ExternalUserSM> AddExternalUserAsync(ExternalUserSM sm)
        {
            var dm = _mapper.Map<ExternalUserDM>(sm);
            dm.CreatedAt = DateTime.UtcNow;
            dm.CreatedBy = _loginUserDetail.LoginId;

            await _apiDbContext.ExternalUsers.AddAsync(dm);
            await _apiDbContext.SaveChangesAsync();

            return _mapper.Map<ExternalUserSM>(dm);
        }

        #endregion

        #region Social Login Orchestration

        public async Task<(TokenUserSM tokenUser, long adminId)> AddSocialLoginAndUserDetails(
            DecodedTokenSM decodedToken,
            SocialLoginSM socialLoginSM)
        {
            await using var transaction = await _apiDbContext.Database.BeginTransactionAsync();

            try
            {
                return socialLoginSM.RoleType switch
                {
                    RoleTypeSM.Seller => await HandleSellerSocialLogin(decodedToken, socialLoginSM, transaction),
                    RoleTypeSM.User => await HandleUserSocialLogin(decodedToken, socialLoginSM, transaction),
                    _ => throw new SiffrumException(
                        ApiErrorTypeSM.InvalidInputData_Log,
                        $"Invalid role type: {socialLoginSM.RoleType}",
                        "Invalid role type")
                };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                throw new SiffrumException(
                    ApiErrorTypeSM.NoRecord_Log,
                    ex.Message,
                    "Error while processing social login");
            }
        }

        #endregion

        #region Seller Social Login

        private async Task<(TokenUserSM, long)> HandleSellerSocialLogin(
            DecodedTokenSM token,
            SocialLoginSM socialLoginSM,
            IDbContextTransaction transaction)
        {
            long adminId = default;

            var seller = await _sellerProcess.GetSellerDetailsByEmail(token.Email);

            if (seller != null)
            {
                ValidateSellerStatus(seller);

                var externalUser = await GetExternalUserByClientUserIdandTypeAsync(
                    seller.Id,
                    socialLoginSM.ExternalUserType,
                    socialLoginSM.RoleType);

                if (externalUser == null)
                {
                    await AddExternalUserAsync(CreateExternalUserSM(socialLoginSM, seller.Id));
                }

                await transaction.CommitAsync();

                var tokenUser = _mapper.Map<TokenUserSM>(seller);
                tokenUser.Status = StatusSM.Active;

                if (seller.AdminId.HasValue)
                    adminId = seller.AdminId.Value;

                return (tokenUser, adminId);
            }

            var newSeller = new SellerSM
            {
                Email = token.Email
            };

            var createdSeller = await _sellerProcess.RegisterSocialUser(newSeller);

            await AddExternalUserAsync(CreateExternalUserSM(socialLoginSM, createdSeller.Id));           

            var newTokenUser = _mapper.Map<TokenUserSM>(createdSeller);
            newTokenUser.Status = StatusSM.Active;
            await transaction.CommitAsync();
            return (newTokenUser, adminId);
        }

       
        #endregion

        #region User Social Login

        private async Task<(TokenUserSM, long)> HandleUserSocialLogin(
            DecodedTokenSM token,
            SocialLoginSM socialLoginSM,
            IDbContextTransaction transaction)
        {
            long adminId = default;

            var user = await _userProcess.GetUserDetailsByEmail(token.Email);

            if (user != null)
            {
                var externalUser = await GetExternalUserByClientUserIdandTypeAsync(
                    user.Id,
                    socialLoginSM.ExternalUserType,
                    socialLoginSM.RoleType);

                if (externalUser == null)
                {
                    await AddExternalUserAsync(CreateExternalUserSM(socialLoginSM, user.Id));
                }

                await transaction.CommitAsync();

                var tokenUser = _mapper.Map<TokenUserSM>(user);
                tokenUser.Status = StatusSM.Active;//Todo: Check this

                return (tokenUser, adminId);
            }

            var newUser = new UserSM
            {
                Email = token.Email,
                Image = token.ImageBase64
            };

            var createdUser = await _userProcess.RegisterSocialUser(newUser);

            await AddExternalUserAsync(CreateExternalUserSM(socialLoginSM, createdUser.Id));

            await transaction.CommitAsync();

            var newTokenUserSM = _mapper.Map<TokenUserSM>(createdUser);
            newTokenUserSM.Status = StatusSM.Active;

            return (newTokenUserSM, adminId);
        }


        #region Handle Phone Number Login

        public async Task<(TokenUserSM, long)> HandleUserPhoneLogin(
            PhoneLoginResponseSM token)
        {
            long adminId = default;
            UserDM? user = await _apiDbContext.User.AsNoTracking().FirstOrDefaultAsync(x => x.Mobile == token.PhoneNumber);
            if (user != null)
            {
                if(user.Status != StatusDM.Active)
                {
                    throw new SiffrumException(ApiErrorTypeSM.InvalidInputData_Log, "User is not active", "User has not permission to create account...");
                }
                user.FcmId = token.FCMToken;
                user.IsMobileConfirmed = true;
                await _apiDbContext.SaveChangesAsync();
                var tokenUser = _mapper.Map<TokenUserSM>(user);
                tokenUser.Status = StatusSM.Active;

                return (tokenUser, adminId);
            }

            var newUser = new UserSM
            {
                Mobile = token.PhoneNumber,
                IsMobileConfirmed = true,
                CountryCode = token.CountryCode,
            };

            var createdUser = await _userProcess.RegisterSocialUserByPhone(newUser);

            var newTokenUserSM = _mapper.Map<TokenUserSM>(createdUser);
            newTokenUserSM.Status = StatusSM.Active;

            return (newTokenUserSM, adminId);
        }

        #endregion Handle Phone Number Login

        #endregion

        #region Update

        public async Task<ExternalUserSM?> UpdateSocialLoginDetails(ExternalUserSM sm)
        {
            var dm = await _apiDbContext.ExternalUsers.FirstOrDefaultAsync(x =>
                x.UserId == sm.UserId &&
                x.ExternalUserType == (ExternalUserTypeDM)sm.ExternalUserType &&
                x.RoleType == (RoleTypeDM)sm.RoleType);

            if (dm == null)
                return null;

            dm.IdToken = sm.IdToken;
            dm.UpdatedAt = DateTime.UtcNow;
            dm.UpdatedBy = _loginUserDetail.LoginId;

            await _apiDbContext.SaveChangesAsync();

            return _mapper.Map<ExternalUserSM>(dm);
        }

        #endregion

        #region Helpers

        private static ExternalUserSM CreateExternalUserSM(SocialLoginSM socialLoginSM, long userId)
        {
            var sm = new ExternalUserSM
            {
                UserId = userId,
                ExternalUserType = socialLoginSM.ExternalUserType,
                RoleType = socialLoginSM.RoleType,
                IdToken = socialLoginSM.IdToken
            };
            return sm;
        }

        private static void ValidateSellerStatus(SellerSM seller)
        {
            if (seller.Status == SellerStatusSM.Removed)
                throw new SiffrumException(ApiErrorTypeSM.InvalidToken_Log,
                    "Removed seller attempted login",
                    "Your account is removed");

            if (seller.Status == SellerStatusSM.Blocked ||
                seller.Status == SellerStatusSM.Rejected ||
                seller.Status == SellerStatusSM.Deactivated)
                throw new SiffrumException(ApiErrorTypeSM.InvalidToken_Log,
                    "Inactive seller attempted login",
                    "Your account is disabled");
        }

        #endregion
    }
}

