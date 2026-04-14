using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Siffrum.Ecom.BAL.Base.Email;
using Siffrum.Ecom.BAL.Base.ImageProcess;
using Siffrum.Ecom.BAL.Base.Location;
using Siffrum.Ecom.BAL.Base.OneSignal;
using Siffrum.Ecom.BAL.ExceptionHandler;
using Siffrum.Ecom.BAL.Foundation.Base;
using Siffrum.Ecom.DAL.Context;
using Siffrum.Ecom.DomainModels.Enums;
using Siffrum.Ecom.DomainModels.v1;
using Siffrum.Ecom.ServiceModels.AppUser;
using Siffrum.Ecom.ServiceModels.AppUser.Login;
using Siffrum.Ecom.ServiceModels.Enums;
using Siffrum.Ecom.ServiceModels.Foundation.Base.CommonResponseRoot;
using Siffrum.Ecom.ServiceModels.Foundation.Base.Enums;
using Siffrum.Ecom.ServiceModels.Foundation.Base.Interfaces;
using Siffrum.Ecom.ServiceModels.Foundation.Token;
using Siffrum.Ecom.ServiceModels.v1;
using Siffrum.Ecom.ServiceModels.v1.General;
using System.Text;

namespace Siffrum.Ecom.BAL.LoginUsers
{
    public class UserProcess : SiffrumBalOdataBase<UserSM>
    {
        private readonly ILoginUserDetail _loginUserDetail;
        private readonly IPasswordEncryptHelper _passwordEncryptHelper;
        private readonly EmailProcess _emailProcess;
        private readonly ImageProcess _imageProcess;
        private readonly GeoLocationService _geoService;
        private readonly NotificationProcess _notificationProcess;
        private readonly SettingsProcess _settingsProcess;

        public  UserProcess(IMapper mapper,ApiDbContext apiDbContext,ImageProcess imageProcess, GeoLocationService geoService, NotificationProcess notificationProcess,
            ILoginUserDetail loginUserDetail, IPasswordEncryptHelper passwordEncryptHelper, EmailProcess emailProcess,SettingsProcess settingProcess)
            : base(mapper, apiDbContext)
        {
            _loginUserDetail = loginUserDetail;
            _passwordEncryptHelper = passwordEncryptHelper;
            _emailProcess = emailProcess;
            _imageProcess = imageProcess;
            _geoService = geoService;
            _notificationProcess = notificationProcess;
            _settingsProcess = settingProcess;
        }

        #region OData
        public override async Task<IQueryable<UserSM>> GetServiceModelEntitiesForOdata()
        {
            IQueryable<UserDM> entitySet = _apiDbContext.User.AsNoTracking();
            return await base.MapEntityAsToQuerable<UserDM, UserSM>(_mapper, entitySet);
        }
        #endregion

        #region CREATE
        public async Task<BoolResponseRoot> RegisterUser(UserSM objSM)
        {
            if (objSM == null)
            {
                throw new SiffrumException(ApiErrorTypeSM.InvalidInputData_NoLog, "Please provide details for Sign Up", "Please provide details for registration");
            }
            if (objSM.Username.Length < 5)
            {
                throw new SiffrumException(ApiErrorTypeSM.InvalidInputData_Log, "Please provide LoginId with minimum 5 characters", "Please provide LoginId with minimum 5 characters");
            }

            if (string.IsNullOrEmpty(objSM.Email))
            {
                throw new SiffrumException(ApiErrorTypeSM.InvalidInputData_NoLog, "Please provide EmailId ", "Please provide EmailId");
            }
            
            if (string.IsNullOrEmpty(objSM.Username))
            {
                throw new SiffrumException(ApiErrorTypeSM.InvalidInputData_NoLog, "Please provide Username ", "Please provide Username");
            }
            if (string.IsNullOrEmpty(objSM.Password))
            {
                throw new SiffrumException(ApiErrorTypeSM.InvalidInputData_NoLog, "Password is required ", "Password cannot be empty");
            }
            var isUserPresentWithEmail = await GetUserByEmail(objSM.Email);
            if (isUserPresentWithEmail)
            {
                throw new SiffrumException(
                    ApiErrorTypeSM.InvalidInputData_NoLog,
                    "Email already registered",
                    "An account with this email address already exists. Please use a different email or log in to your existing account."
                );
            }

            var isUsernameAvailable = await GetUserByUsername(objSM.Username);
            if (!isUsernameAvailable)
            {
                throw new SiffrumException(
                    ApiErrorTypeSM.InvalidInputData_NoLog,
                    "Username already taken",
                    "This username is already in use. Please choose a different username or log in to your existing account."
                );
            }
            var dm = _mapper.Map<UserDM>(objSM);
            var passwordHash = await _passwordEncryptHelper.ProtectAsync<string>(objSM.Password);
            dm.OfferJsonDetails = "";
            dm.Password = passwordHash;
            dm.RoleType = RoleTypeDM.User;
            dm.IsEmailConfirmed = false;
            dm.IsMobileConfirmed = false;
            dm.CreatedAt = DateTime.UtcNow;
            dm.Status = StatusDM.Active;
            dm.LoginStatus = LoginStatusDM.Enabled;
            dm.DeletedAt = null;
            dm.CreatedBy = _loginUserDetail.LoginId;

            await _apiDbContext.User.AddAsync(dm);
            if(await _apiDbContext.SaveChangesAsync() > 0)
            {
                var link = "https://speedykart.org/verify-email";
                var emailRequest = new EmailSM()
                {
                    Email = objSM.Email,
                };
                await SendEmailVerificationLink(emailRequest, link);
                return new BoolResponseRoot(true, "Your account created Successfully, Please verify email to Login in to your account");
            }
            throw new SiffrumException(ApiErrorTypeSM.Fatal_Log,"Error in creating user details. Please try again.");

        }
        public async Task<BoolResponseRoot> RegisterUserUsingPhoneNumber(OTPRegistrationSM objSM)
        {
            if (objSM == null)
            {
                throw new SiffrumException(ApiErrorTypeSM.InvalidInputData_NoLog, "Please provide details for Sign Up", "Please provide details for registration");
            }
            if (string.IsNullOrEmpty(objSM.PhoneNumber) || string.IsNullOrEmpty(objSM.CountryCode))
            {
                throw new SiffrumException(ApiErrorTypeSM.InvalidInputData_NoLog, "Please provide Mobile Number", "Please provide Mobile Number and Country Code");
            }
            var existingUser = await _apiDbContext.User.Where(x => x.CountryCode == objSM.CountryCode && x.Mobile == objSM.PhoneNumber).FirstOrDefaultAsync();
            var otp = _notificationProcess.GenerateOtp();
            var mobileNumber = $"{objSM.CountryCode}{objSM.PhoneNumber}";
            if (existingUser != null)
            {
                if(existingUser.Status == StatusDM.Inactive)
                {
                    //Handle here if we allow user to create account again after deleting account
                    existingUser.Status = StatusDM.Active;
                }
                existingUser.OTP = otp;
                existingUser.UpdatedAt = DateTime.UtcNow;
                if (!string.IsNullOrEmpty(objSM.SubscriptionId))
                {
                    existingUser.FcmId = objSM.SubscriptionId;
                }
                
                if (await _apiDbContext.SaveChangesAsync() > 0)
                {
                    var response = await _notificationProcess.SendOtpSms(mobileNumber, otp);
                    if(response.BoolResponse == true)
                    {
                        return new BoolResponseRoot(true, "Paste OTP for login your account");
                        /*var isOtpSent = await _notificationProcess.GetSmsDeliveryStatus(response.ResponseMessage);
                        if(isOtpSent.BoolResponse == true)
                        {
                            return new BoolResponseRoot(true, "Paste OTP for login your account");
                        }
                        else
                        {
                            return new BoolResponseRoot(false, "Something went wrong while sending otp to your number, kindly try again");
                        }*/
                    }
                    return new BoolResponseRoot(false, "Something went wrong while sending otp, Please try again");
                }
                throw new SiffrumException(ApiErrorTypeSM.Fatal_Log, "", "Something went wrong while saving your detials, Please try again later");
            }
            var referalCode = await GenerateUniqueReferralCode();

            var dm = new UserDM()
            {
                CountryCode = objSM.CountryCode,
                Mobile = objSM.PhoneNumber,
                OTP = otp,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = _loginUserDetail.LoginId,
                Status = StatusDM.Active,
                LoginStatus = LoginStatusDM.Disabled,
                DeletedAt = null,
                Password = null,
                FcmId = objSM.SubscriptionId,
                ReferralCode = referalCode,
                FriendsCode = null,
                RoleType = RoleTypeDM.User,
                IsEmailConfirmed = false,
                UpdatedAt = DateTime.UtcNow,
                IsMobileConfirmed = false
            };


            await _apiDbContext.User.AddAsync(dm);
            if (await _apiDbContext.SaveChangesAsync() > 0)
            {
                var response = await _notificationProcess.SendOtpSms(mobileNumber, otp);
                if (response.BoolResponse == true)
                {
                    return new BoolResponseRoot(true, "Paste OTP for login your account");
                    /*var isOtpSent = await _notificationProcess.GetSmsDeliveryStatus(response.ResponseMessage);
                    if (isOtpSent.BoolResponse == true)
                    {
                        return new BoolResponseRoot(true, "Paste OTP for login your account");
                    }
                    else
                    {
                        return new BoolResponseRoot(false, "Something went wrong while sending otp to your number, kindly try again");
                    }*/
                }
                return new BoolResponseRoot(false, "Something went wrong while sending otp, Please try again");

            }
            throw new SiffrumException(ApiErrorTypeSM.Fatal_Log, "Something went wrong while creating user details. Please try again.");

        }

        public async Task<ReferalCodeSM> GetMineReferralCode(long userId)
        {
            var user = await _apiDbContext.User.FindAsync(userId);

            if (user == null)
            {
                throw new SiffrumException(
                    ApiErrorTypeSM.Fatal_Log,
                    $"User with Id: {userId} not found",
                    "User not found");
            }

            if (string.IsNullOrEmpty(user.ReferralCode))
            {
                var referalCode = await GenerateUniqueReferralCode();

                user.ReferralCode = referalCode;
                await _apiDbContext.SaveChangesAsync();
            }

            return new ReferalCodeSM
            {
                ReferalCode = user.ReferralCode
            };
        }

        public async Task<BoolResponseRoot> IsFriendsCodeApplied(long userId)
        {
            var user = await _apiDbContext.User.FindAsync(userId);

            if (user == null)
            {
                throw new SiffrumException(
                    ApiErrorTypeSM.Fatal_Log,
                    $"User with Id: {userId} not found",
                    "User not found");
            }

            var count = await _apiDbContext.Order
                .AsNoTracking()
                .Where(x => x.UserId == userId && x.PaymentStatus == PaymentStatusDM.Paid)
                .CountAsync();

            if (string.IsNullOrEmpty(user.FriendsCode) && count == 0)
            {
                return new BoolResponseRoot(false, "Referral code is not applied");
            }

            return new BoolResponseRoot(true, "Referral code is applied");
        }

        public async Task<BoolResponseRoot> AddReferralCode(long userId, ReferalCodeSM sm)
        {
            if (sm == null || string.IsNullOrEmpty(sm.ReferalCode))
            {
                throw new SiffrumException(
                    ApiErrorTypeSM.InvalidInputData_NoLog,
                    "Referral code is required",
                    "Please provide a valid referral code");
            }

            // 1️⃣ Get current user
            var user = await _apiDbContext.User
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
            {
                throw new SiffrumException(
                    ApiErrorTypeSM.Fatal_Log,
                    $"User with Id: {userId} not found",
                    "User not found");
            }
            if(user.Status == StatusDM.Inactive)
            {
                throw new SiffrumException(
                    ApiErrorTypeSM.Fatal_Log,
                    $"User with Id: {userId} is inactive and trying to assign referral code",
                    "User not found");
            }
            // 2️⃣ Check if already applied
            if (!string.IsNullOrEmpty(user.FriendsCode))
            {
                return new BoolResponseRoot(false, "Referral code already applied");
            }

            // 3️⃣ Prevent self-referral
            if (user.ReferralCode == sm.ReferalCode)
            {
                return new BoolResponseRoot(false, "You cannot use your own referral code");
            }

            // 4️⃣ Validate referral code exists
            var refUser = await _apiDbContext.User
                .FirstOrDefaultAsync(u => u.ReferralCode == sm.ReferalCode);

            if (refUser == null)
            {
                return new BoolResponseRoot(false, "Invalid referral code");
            }

            // 5️⃣ Save friend's referral code
            user.FriendsCode = sm.ReferalCode;
            user.UpdatedAt = DateTime.UtcNow;

            if (await _apiDbContext.SaveChangesAsync() > 0)
            {
                return new BoolResponseRoot(true, "Referral code applied successfully");
            }

            throw new SiffrumException(
                ApiErrorTypeSM.Fatal_Log,
                "Error while saving referral code",
                "Something went wrong, please try again later");
        }

        public async Task<TokenUserSM> GetUserDetailsByOtpVerification(OTPRegistrationSM objSM)
        {
            if(objSM == null)
            {
                throw new SiffrumException(ApiErrorTypeSM.InvalidInputData_NoLog, "OTP is missing, Please provide OTP for login your account");
            }
            var existingUser = await _apiDbContext.User.Where(x => x.CountryCode == objSM.CountryCode && x.Mobile == objSM.PhoneNumber && x.OTP == objSM.OTP).FirstOrDefaultAsync();
            if (existingUser != null)
            {
                if(existingUser.Status == StatusDM.Inactive)
                {
                    return null;
                }

                if (existingUser.UpdatedAt.HasValue && DateTime.UtcNow - existingUser.UpdatedAt.Value > TimeSpan.FromMinutes(2))
                {
                    throw new SiffrumException(ApiErrorTypeSM.Success_NoLog, "OTP is expired,Kindly try again...");
                }
                existingUser.IsMobileConfirmed = true;
                existingUser.LoginStatus = LoginStatusDM.Enabled;
                await _apiDbContext.SaveChangesAsync();
                var sm = _mapper.Map<TokenUserSM>(existingUser);
                if (!string.IsNullOrEmpty(existingUser.Image))
                {
                    var base64 = await _imageProcess.ConvertToBase64(existingUser.Image);
                    if (!string.IsNullOrEmpty(base64))
                    {
                        sm.Image = base64;
                    }
                    else
                    {
                        sm.Image = null;
                    }
                }
                return sm;
            }

            return null;
            //throw new SiffrumException(ApiErrorTypeSM.InvalidInputData_NoLog, "User Not Found, Please provide valid details for login your account");
        }

        public async Task<UserSM> RegisterSocialUser(UserSM objSM)
        {
            if (objSM == null)
            {
                throw new SiffrumException(ApiErrorTypeSM.InvalidInputData_NoLog, "Please provide details for Sign Up", "Please provide details for registration");
            }

            if (string.IsNullOrEmpty(objSM.Email))
            {
                throw new SiffrumException(ApiErrorTypeSM.InvalidInputData_NoLog, "Please provide EmailId ", "Please provide EmailId");
            }

            var dm = _mapper.Map<UserDM>(objSM);
            dm.Password = null;
            dm.Username = null;
            dm.RoleType = RoleTypeDM.User;
            dm.IsEmailConfirmed = true;
            dm.IsMobileConfirmed = false;
            dm.CreatedAt = DateTime.UtcNow;
            dm.Status = StatusDM.Active;
            dm.LoginStatus = LoginStatusDM.Enabled;
            dm.DeletedAt = null;
            dm.CreatedBy = _loginUserDetail.LoginId;
            if (string.IsNullOrEmpty(objSM.Image))
            {
                dm.Image = null;
            }
            else
            {
                var imagePath = await _imageProcess.SaveFromBase64(objSM.Image, "jpg", "wwwroot/content/loginusers/users/images");
                if (!string.IsNullOrEmpty(imagePath))
                {
                    dm.Image = imagePath;
                }
            }
            await _apiDbContext.User.AddAsync(dm);
            if (await _apiDbContext.SaveChangesAsync() > 0)
            {
                return await GetByIdAsync(dm.Id);
            }
            throw new SiffrumException(ApiErrorTypeSM.Fatal_Log, 
                $"Error in creating user details using social login",
                "Something went wrong while creating your account, Please contact support team");
        }

        public async Task<BoolResponseRoot> LinkEmailAndPassword(long id, LinkEmailPasswordSM request)
        {
            var existingUser = await _apiDbContext.User.FindAsync(id);
            if (existingUser == null)
            {
                throw new SiffrumException(ApiErrorTypeSM.Fatal_Log, "User details not found...");
            }
            if(existingUser.Status != StatusDM.Active)
            {
                throw new SiffrumException(ApiErrorTypeSM.InvalidInputData_Log, "User details not found");
            }
            if (!string.IsNullOrEmpty(existingUser.Email)) 
            {
                return new BoolResponseRoot(false, "Email is already set for this account.");
            }
            if (!string.IsNullOrEmpty(existingUser.Password))
            {
                return new BoolResponseRoot(false, "Password is already set for this account.");
            }
            var existingUserWithEmail = await GetUserByEmail(request.Email);
            if (existingUserWithEmail)
            {
                throw new SiffrumException(ApiErrorTypeSM.InvalidInputData_Log, "Email is already registered, Please use different email.");
            }
            var passwordHash = await _passwordEncryptHelper.ProtectAsync<string>(request.Password);
            existingUser.Password = passwordHash;
            existingUser.Email = request.Email;
            existingUser.IsEmailConfirmed = false;
            existingUser.UpdatedBy = _loginUserDetail.LoginId;
            existingUser.UpdatedAt = DateTime.UtcNow;
            if (await _apiDbContext.SaveChangesAsync() > 0)
            {
                var link = "https://speedykart.org/verify-email";
                var emailRequest = new EmailSM()
                {
                    Email = request.Email,
                };
                await SendEmailVerificationLink(emailRequest, link);
                return new BoolResponseRoot(true, "Email is linked succcessfully with your account, Please verify email to Login in to your account");
            }
            else
            {
                throw new SiffrumException(ApiErrorTypeSM.Fatal_Log, "Error in updating user details");
            }
        }

        public async Task<UserSM> RegisterSocialUserByPhone(UserSM objSM)
        {
            if (objSM == null)
            {
                throw new SiffrumException(ApiErrorTypeSM.InvalidInputData_NoLog, "Please provide details for Sign Up", "Please provide details for registration");
            }

            if (string.IsNullOrEmpty(objSM.Mobile))
            {
                throw new SiffrumException(ApiErrorTypeSM.InvalidInputData_NoLog, "Please provide Phone Number for registration purpose ", "Something went rong while registering your phone number");
            }
            var existingWithUserName = await GetUserByUsername(objSM.Mobile);
            if(existingWithUserName == false)
            {
                var uName = Guid.NewGuid().ToString();
                objSM.Username = uName.ToString();
            }
            else
            {
                objSM.Username = objSM.Mobile;
            }

            var dm = _mapper.Map<UserDM>(objSM);
            dm.Password = null;
            dm.RoleType = RoleTypeDM.User;
            dm.IsEmailConfirmed = false;
            dm.CreatedAt = DateTime.UtcNow;
            dm.Status = StatusDM.Active;
            dm.LoginStatus = LoginStatusDM.Enabled;
            dm.DeletedAt = null;
            dm.CreatedBy = _loginUserDetail.LoginId;
            if (string.IsNullOrEmpty(objSM.Image))
            {
                dm.Image = null;
            }
            else
            {
                var imagePath = await _imageProcess.SaveFromBase64(objSM.Image, "jpg", "wwwroot/content/loginusers/users/images");
                if (!string.IsNullOrEmpty(imagePath))
                {
                    dm.Image = imagePath;
                }
            }
            await _apiDbContext.User.AddAsync(dm);
            if (await _apiDbContext.SaveChangesAsync() > 0)
            {
                return await GetByIdAsync(dm.Id);
            }
            throw new SiffrumException(ApiErrorTypeSM.Fatal_Log, 
                $"Error in creating user details using social login",
                "Something went wrong while creating your account, Please contact support team");
        }
        #endregion

        #region READ

        public async Task<List<UserSM>> GetAll(int skip, int top)
        {
            var userIds = await _apiDbContext.User.Select(x=>x.Id).Skip(skip).Take(top).ToListAsync();
            if(userIds.Count > 0)
            {
                var response = new List<UserSM>();

                foreach(var id in userIds)
                {
                    var user = await GetByIdAsync(id);
                    if(user != null)
                    {
                        response.Add(user);
                    }
                }
                return response;
            }
            return new List<UserSM>();
        }

        public async Task<IntResponseRoot> GetAllUsersCount()
        {
            var count = await _apiDbContext.User.CountAsync();
            return new IntResponseRoot(count, "Total Count of users");
            
        }

        public async Task<UserSM?> GetByIdAsync(long id)
        {
            var dm = await _apiDbContext.User
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id);
            string imagePath = null;
            if (dm != null)
            {
                var response = _mapper.Map<UserSM>(dm);
                if (!string.IsNullOrEmpty(response.Image))
                {
                    var imgPath = await _imageProcess.ConvertToBase64(response.Image);
                    if (!string.IsNullOrEmpty(imgPath)) 
                    { 
                        response.Image = imgPath;
                    }
                    else
                    {
                        response.Image = null;
                    }
                }
                response.Password = null;
                return response;
            }

            return null;
        }
        
        public async Task<UserSM?> GetMineDetailsAsync(long id)
        {
            
            var dm = await _apiDbContext.User
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id);
            string imagePath = null;
            if (dm != null)
            {
                if (dm.Status == StatusDM.Inactive)
                {
                    throw new SiffrumException(ApiErrorTypeSM.InvalidToken_Log,
                        $"User with Id {id} is inactive or already deleted but trying to delete the data again",
                        "User details not found");
                }
                
                var response = _mapper.Map<UserSM>(dm);
                if (!string.IsNullOrEmpty(dm.Image))
                {
                    var imgPath = await _imageProcess.ConvertToBase64(response.Image);
                    if (!string.IsNullOrEmpty(imgPath)) 
                    { 
                        response.Image = imgPath;
                    }
                    else
                    {
                        response.Image = null;
                    }
                }
                response.Password = null;
                return response;
            }

            return null;
        }
        #endregion

        #region UPDATE
        public async Task<UserSM?> UpdateAsync(long id, UserSM objSM, bool IsSocialLogin = false, bool isUserCall = false)
        {
            if (objSM == null)
            {
                throw new SiffrumException(ApiErrorTypeSM.ModelError_NoLog, "Please provide details to update");
            }
            UserDM dm = await _apiDbContext.User
                .FirstOrDefaultAsync(x => x.Id == id);
            if (dm == null)
            {
                throw new SiffrumException(ApiErrorTypeSM.InvalidToken_Log, $"User tried to update but not found for the Id: {id}", "User not found.");
            }
            if (isUserCall)
            {
                if(dm.Status == StatusDM.Inactive)
                {
                    throw new SiffrumException(ApiErrorTypeSM.InvalidToken_Log,
                    $"User with Id {id} is inactive or already deleted but trying to delete the data again",
                    "User details not found");
                }
            }
            if (objSM.Email != dm.Email)
            {
                throw new SiffrumException(ApiErrorTypeSM.InvalidInputData_NoLog, "The email address cannot be updated, Please contact support, if you need assistence");
            }
            if (objSM.Username != dm.Username)
            {
                throw new SiffrumException(ApiErrorTypeSM.InvalidInputData_NoLog, "Username cannot be updated, Please contact support, if you need assistence");
            }
            string previousLogo = null;
            if (objSM != null)
            {
                objSM.Id = id;
                objSM.IsEmailConfirmed = dm.IsEmailConfirmed;
                objSM.IsMobileConfirmed = dm.IsMobileConfirmed;
                objSM.Password = dm.Password;
                objSM.RoleType = (RoleTypeSM)dm.RoleType;
                objSM.Status = (StatusSM)dm.Status;
                objSM.LoginStatus = (LoginStatusSM)dm.LoginStatus;
                objSM.ReferralCode = dm.ReferralCode;
                objSM.PaymentId = dm.PaymentId;
                objSM.FriendsCode = dm.FriendsCode;
                if (IsSocialLogin == true)
                {
                    objSM.IsEmailConfirmed = true;
                }
                else
                {
                    objSM.IsEmailConfirmed = dm.IsEmailConfirmed;
                }
                if (!string.IsNullOrEmpty(objSM.Image))
                {
                    var imagePath = await _imageProcess.SaveFromBase64(objSM.Image, "jpg", "wwwroot/content/loginusers/users/images");
                    if (!string.IsNullOrEmpty(imagePath))
                    {
                        previousLogo = dm.Image;
                        objSM.Image = imagePath;
                    }                        
                }                
            }
            
            _mapper.Map(objSM, dm);
            dm.OfferJsonDetails = dm.OfferJsonDetails;
            dm.UpdatedAt = DateTime.UtcNow;
            dm.UpdatedBy = _loginUserDetail.LoginId;

            if (await _apiDbContext.SaveChangesAsync() > 0)
            {
                if (File.Exists(previousLogo)) File.Delete(previousLogo);
                return await GetByIdAsync(id);
            }

            throw new SiffrumException(ApiErrorTypeSM.Fatal_Log, $"Error in updating selling details with Id: {id}", "Something went wrong while updating profile details");
            
        }
        #endregion

        #region DELETE (SOFT DELETE)
        public async Task<DeleteResponseRoot> DeleteAsync(long id)
        {
            var dm = await _apiDbContext.User
                .FirstOrDefaultAsync(x => x.Id == id);

            if (dm == null)
            {
                throw new SiffrumException(ApiErrorTypeSM.InvalidInputData_Log, "User details not found");
            }
            if(dm.Status == StatusDM.Inactive)
            {
                throw new SiffrumException(ApiErrorTypeSM.InvalidToken_Log,
                    $"User with Id {id} is inactive or already deleted but trying to delete the data again",
                    "User details not found");
            }
            
            dm.Status = StatusDM.Inactive;
            dm.DeletedAt = DateTime.UtcNow;
            dm.UpdatedBy = _loginUserDetail.LoginId;
            var previousLogo = dm.Image;
            if (await _apiDbContext.SaveChangesAsync() > 0) 
            {
                if (File.Exists(previousLogo)) File.Delete(previousLogo);
                return new DeleteResponseRoot(true, "Your account deleted successfully");
            }
            throw new SiffrumException(ApiErrorTypeSM.Fatal_Log, $"User with Id:{id} is not getting deleted successfully", "Something went wrong while deleting your accunt, Please contact support team");
        }
        #endregion

        #region Email/LoginId checker

        public async Task<bool> GetUserByEmail(string email)
        {
            var existingUser = await _apiDbContext.User.AsNoTracking().FirstOrDefaultAsync(x => x.Email == email);
            if(existingUser == null)
            {
                return false;
            }
            return true;
        }  
        
        
        
        public async Task<UserSM> GetUserDetailsByEmail(string email)
        {
            var existingUser = await _apiDbContext.User.AsNoTracking().FirstOrDefaultAsync(x => x.Email == email);
            if(existingUser != null)
            {
                return _mapper.Map<UserSM>(existingUser);
            }
            return null;
        }

        public async Task<UserSM> GetUserDetailsByPhoneNumber(string phoneNumber)
        {
            var existingUser = await _apiDbContext.User.AsNoTracking().FirstOrDefaultAsync(x => x.Mobile == phoneNumber);
            if (existingUser != null)
            {
                return _mapper.Map<UserSM>(existingUser);
            }
            return null;
        }


        public async Task<bool> GetUserByUsername(string username)
        {
            var existingUser = await _apiDbContext.User.AsNoTracking().FirstOrDefaultAsync(x => x.Username == username);
            if(existingUser == null)
            {
                return true;
            }
            return false;
        }

        #endregion Email/LoginId checker

        #region Email Verification

        public async Task<BoolResponseRoot> SendEmailVerificationLink(EmailSM objSM, string link)
        {
            var authCode = await _passwordEncryptHelper.ProtectAsync(objSM);
            var encodedAuthCode = Convert.ToBase64String(Encoding.UTF8.GetBytes(authCode))
                .Replace("+", "-")
                .Replace("/", "_")
                .TrimEnd('=');
            if (string.IsNullOrWhiteSpace(objSM.Email))
                throw new SiffrumException(ApiErrorTypeSM.Fatal_Log, "Email cannot be empty.");
            
            if (!string.IsNullOrWhiteSpace(objSM.Email))
            {

                link = $"{link}?authCode={encodedAuthCode}";
                var subject = "Email Verification Request";
                /*var body = $"Hi {objSM.Email}, <br/> Your email confirmation requested for your account. " +
                    $"Click the link below to confirm your Email. " +
                     $" <br/><br/><a href='{link}'>{link}</a> <br/><br/>" +
                     "If you did not request an email verification, please ignore this email.<br/><br/> Thank you";*/
               var body = $@"
            <html>
              <head>
                <meta charset='UTF-8'>
                <title>Confirm Your Email</title>
              </head>
              <body style='font-family: Arial, sans-serif; background-color: #f4f4f4; padding: 20px;'>
                <div style='max-width: 600px; margin: auto; background: #ffffff; border-radius: 8px; overflow: hidden; box-shadow: 0 0 10px rgba(0,0,0,0.1);'>
                  <div style='background-color: #007BFF; padding: 20px; text-align: center; color: #ffffff;'>
                    <h1 style='margin: 0; font-size: 24px;'>Confirm Your Email</h1>
                  </div>
                  <div style='padding: 30px;'>
                    <p>Hello {objSM.Email},</p>
                    <p>Thank you for joining us! To complete your registration, please confirm your email by clicking the button below:</p>
                    <p style='text-align: center; margin: 30px 0;'>
                      <a href='{link}' style='display: inline-block; padding: 12px 25px; font-size: 16px; color: #ffffff; background-color: #007BFF; text-decoration: none; border-radius: 5px;'>
                        Verify Email Address
                      </a>
                    </p>
                    <p>If the button doesn't work, copy & paste this link:</p>
                    <p style='word-break: break-all; text-align: center;'>
                      <a href='{link}' style='color: #007BFF;'>{link}</a>
                    </p>
                    <p>If you did not request this, please ignore this message.</p>
                    <p>Warm regards,<br>Team Siffrum</p>
                  </div>
                  <div style='background-color: #f4f4f4; padding: 20px; text-align: center; font-size: 12px; color: #777777;'>
                    <p>Please note: This is an automated email, and replies are not monitored.</p>
                  </div>
                </div>
              </body>
            </html>";
            
            _emailProcess.SendEmail(objSM.Email, subject, string.Format(body, objSM.Email));
                return new BoolResponseRoot(true, "Email Verification Link has been sent Successfully");
            }
            else
            {
                throw new SiffrumException(ApiErrorTypeSM.Fatal_Log,
                    $"User not found with email {objSM.Email}, tries to send email verification by logged in user with loginId {_loginUserDetail.LoginId}",
                    $"User not found with email '{objSM.Email}' found.");
            }
        }


        public async Task<BoolResponseRoot> VerifyEmailRequest(VerifyEmailRequestSM objSM)
        {
            string decodedAuthCode = Encoding.UTF8.GetString(Convert.FromBase64String(objSM.AuthCode
                .Replace("-", "+")
                .Replace("_", "/") + new string('=', (4 - objSM.AuthCode.Length % 4) % 4)));
            EmailSM sm = await _passwordEncryptHelper.UnprotectAsync<EmailSM>(decodedAuthCode);
            
            if (!string.IsNullOrWhiteSpace(sm.Email))
            {
                var user = await _apiDbContext.User.Where(x => x.Email.ToUpper() == sm.Email.ToUpper()).FirstOrDefaultAsync();

                if (user != null)
                {
                    if (user.Status == StatusDM.Inactive)
                    {
                        throw new SiffrumException(ApiErrorTypeSM.InvalidToken_Log,
                            $"User with Id: {user.Id} is inactive or already deleted but trying to verify email",
                            "User details not found");
                    }

                    if (user.IsEmailConfirmed == true)
                    {
                        return new BoolResponseRoot(false, "Your Email is already Verified, Login to your Account now");
                    }
                    user.IsEmailConfirmed = true;
                    user.UpdatedBy = _loginUserDetail.LoginId;
                    user.UpdatedAt = DateTime.UtcNow;
                    if (await _apiDbContext.SaveChangesAsync() > 0)
                    {
                        return new BoolResponseRoot(true, "Your Email Verified Successfully, Login to your Account now");
                    }
                    throw new SiffrumException(ApiErrorTypeSM.Fatal_Log, $"Something went wrong while verifying your email, Please try again later");
                }
                else
                {
                    throw new SiffrumException(ApiErrorTypeSM.Fatal_Log, 
                        $"No User with email '{sm.Email}' found.",
                        $"User with Email '{sm.Email}' not found.");
                }
            }
            else
            {
                throw new SiffrumException(ApiErrorTypeSM.Fatal_Log, "Email can not be empty", "Email can not be empty");
            }
        }

        #endregion Email Verification

        #region Forgot/Update/Validate Password

        public async Task<BoolResponseRoot> SendResetPasswordLink(ForgotPasswordSM forgotPassword)
        {
            var timeExpiry = 30;
            DateTime newTime = DateTime.Now.AddMinutes(timeExpiry);
            forgotPassword.Expiry = newTime;
            var authCode = await _passwordEncryptHelper.ProtectAsync(forgotPassword);

            var encodedAuthCode = Convert.ToBase64String(Encoding.UTF8.GetBytes(authCode))
                .Replace("+", "-")
                .Replace("/", "_")
                .TrimEnd('=');


            var baseUrl = "https://api.speedykart.org";
            var link = $"{baseUrl}/resetpassword?authCode={encodedAuthCode}";

            if (string.IsNullOrEmpty(forgotPassword.UserName))
                throw new SiffrumException(ApiErrorTypeSM.ModelError_NoLog, "User Name cannot be empty.");
            //Todo: Handle deleted users
            var user = await _apiDbContext.User.AsNoTracking().Where(x=>x.Email == forgotPassword.UserName || x.Username == forgotPassword.UserName).FirstOrDefaultAsync();
            if (user == null || user?.Status == StatusDM.Inactive)
            {
                throw new SiffrumException(ApiErrorTypeSM.InvalidToken_Log,
                    $"User with Username or Email: {forgotPassword.UserName} is inactive or already deleted but trying send forgot password request",
                    "User details not found");
            }
            if (string.IsNullOrEmpty(user.Email))
                throw new SiffrumException(ApiErrorTypeSM.Fatal_Log, $"No User with username '{forgotPassword.UserName}' found.");

            var subject = "Password Reset Request";    
            
              var body = $@"
                <html>
                  <head><meta charset='UTF-8'></head>
                  <body style='font-family: Arial, sans-serif; background-color: #f4f4f4; padding: 20px;'>
                    <div style='max-width: 600px; margin: auto; background: #ffffff; border-radius: 8px; overflow: hidden; box-shadow: 0 0 10px rgba(0,0,0,0.1);'>
                      <div style='background-color: #007BFF; padding: 20px; text-align: center; color: #ffffff;'>
                        <h1 style='margin: 0; font-size: 24px;'>Reset Your Password</h1>
                      </div>
                      <div style='padding: 30px;'>
                        <p>Hello {forgotPassword.UserName},</p>
                        <p>We received a request to reset your password. Simply click the button below to set up a new password:</p>
                        <p style='text-align: center; margin: 30px 0;'>
                          <a href='{link}' style='display: inline-block; padding: 12px 25px; font-size: 16px; color: #ffffff; background-color: #007BFF; text-decoration: none; border-radius: 5px;'>
                            Reset Password
                          </a>
                        </p>
                        <p>If the button isn't working, copy and paste the link below into your browser:</p>
                        <p style='word-break: break-all; text-align: center;'>
                          <a href='{link}' style='color: #007BFF;'>{link}</a>
                        </p>
                        <p>If you didn’t request a password reset, no worries—simply ignore this email.</p>
                        <p>Best regards,<br>Team Siffrum</p>
                      </div>
                     <div style='background-color:#f4f4f4; padding:20px; text-align:center; font-size:12px; color:#777;'>
                      <p>Please note: This is an automated email—replies are not monitored.</p>
                     </div>
                    </div>
                  </body>
                </html>";            

            _emailProcess.SendEmail(user.Email, subject, body);

            return new BoolResponseRoot(true, "Reset Password Link has been sent Successfully");
        }

        /// <summary>
        /// Validation of Password Link that has been sent via Email.
        /// </summary>
        /// <param name="authCode">String Object</param>
        /// <returns>The integer Response Object.</returns>

        public async Task<IntResponseRoot> ValidatePassword(string authCode)
        {
            string decodedAuthCode = Encoding.UTF8.GetString(Convert.FromBase64String(authCode
                .Replace("-", "+")
                .Replace("_", "/") + new string('=', (4 - authCode.Length % 4) % 4)));

            ForgotPasswordSM forgotPassword = await _passwordEncryptHelper.UnprotectAsync<ForgotPasswordSM>(decodedAuthCode);
            if (string.IsNullOrWhiteSpace(forgotPassword.UserName))
            {
                return new IntResponseRoot((int)ValidatePasswordLinkStatusSM.Invalid, "UserName Not Found");
            }
            if (forgotPassword.Expiry < DateTime.Now)
            {
                return new IntResponseRoot((int)ValidatePasswordLinkStatusSM.Invalid, "Password reset link expired.");
            }
            return new IntResponseRoot((int)ValidatePasswordLinkStatusSM.Valid, "Success");

        }

        /// <summary>
        /// This is Used for Updating the Password of a User.
        /// </summary>
        /// <param name="resetPasswordRequest">ResetPasswordRequestSM Object</param>
        /// <param name="newPassword">String NewPassword</param>
        /// <returns>The Boolen Response Object.</returns>
        /// <exception cref="SiffrumException"></exception>

        public async Task<BoolResponseRoot> UpdatePassword(ResetPasswordRequestSM resetPasswordRequest)
        {
            string decodedAuthCode = Encoding.UTF8.GetString(Convert.FromBase64String(resetPasswordRequest.AuthCode
                .Replace("-", "+")
                .Replace("_", "/") + new string('=', (4 - resetPasswordRequest.AuthCode.Length % 4) % 4)));
            ForgotPasswordSM forgotPassword = await _passwordEncryptHelper.UnprotectAsync<ForgotPasswordSM>(decodedAuthCode);
            if (forgotPassword.Expiry < DateTime.Now)
            {
                throw new SiffrumException(ApiErrorTypeSM.Fatal_Log, $"Link expired.", $"Password reset link expired.");
            }
            if (!string.IsNullOrWhiteSpace(resetPasswordRequest.NewPassword))
            {
                if (string.IsNullOrEmpty(forgotPassword.UserName))
                {
                    throw new SiffrumException(ApiErrorTypeSM.Fatal_Log, 
                        $"No User with username '{forgotPassword.UserName}' found.", 
                        $"No User with username '{forgotPassword.UserName}' found.");
                }
                var user = (from c in _apiDbContext.User
                            where c.Username.ToUpper() == forgotPassword.UserName.ToUpper()
                               || c.Email.ToUpper() == forgotPassword.UserName.ToUpper()
                            select new { User = c }).FirstOrDefault();

                if (user != null)
                {
                    if (user.User.Status == StatusDM.Inactive)
                    {
                        throw new SiffrumException(ApiErrorTypeSM.InvalidToken_Log,
                            $"User with Id {user.User.Id} is inactive or already deleted but trying to update password",
                            "User details not found");
                    }
                    string decrypt = "";
                    string newPassword = await _passwordEncryptHelper.ProtectAsync<string>(resetPasswordRequest.NewPassword);
                    if (string.Equals(user.User.Password, newPassword))
                    {
                        throw new SiffrumException(ApiErrorTypeSM.Fatal_Log, $"Please don't use old password, use new one.", $"Please don't use old password, use new one.");
                    }
                    else
                    {
                        resetPasswordRequest.NewPassword = await _passwordEncryptHelper.ProtectAsync(resetPasswordRequest.NewPassword);
                        user.User.Password = resetPasswordRequest.NewPassword;
                        await _apiDbContext.SaveChangesAsync();
                        return new BoolResponseRoot(true, "Password Updated Successfully");
                    }
                }
                else
                {
                    throw new SiffrumException(ApiErrorTypeSM.Fatal_Log, 
                        $"No User with username '{forgotPassword.UserName}' found.",
                        $"No User with username '{forgotPassword.UserName}' found.");
                }
            }
            else
            {
                throw new SiffrumException(ApiErrorTypeSM.Fatal_Log, "Password can not be empty", "Password can not be empty");
            }
        }

        public async Task<BoolResponseRoot> ChangePassword(long userId, UpdatePasswordRequestSM objSM)
        {
            if(string.IsNullOrEmpty(objSM.OldPassword) || string.IsNullOrEmpty(objSM.NewPassword))
            {
                throw new SiffrumException(ApiErrorTypeSM.InvalidInputData_NoLog, "Password is required, Please enter password.");
            }
            UserDM dm = await _apiDbContext.User.FindAsync(userId);
            if (dm != null) 
            {
                if (dm.Status == StatusDM.Inactive)
                {
                    throw new SiffrumException(ApiErrorTypeSM.InvalidToken_Log,
                        $"User with Id {dm.Id} is inactive or already deleted but trying to change password",
                        "User details not found");
                }
                if (string.IsNullOrEmpty(dm.Password))
                {
                    return new BoolResponseRoot(false, "Password is not set for this account.");
                }
                else
                {
                    var oldPassHash = await _passwordEncryptHelper.ProtectAsync<string>(objSM.OldPassword);
                    if (!oldPassHash.Equals(dm.Password))
                    {
                        return new BoolResponseRoot(false, "The password you entered is incorrect. You can try again or reset your password.");
                    }
                    var newPassHash = await _passwordEncryptHelper.ProtectAsync<string>(objSM.NewPassword);
                    if (newPassHash.Equals(dm.Password))
                    {
                        return new BoolResponseRoot(false, "Your new password must be different from the old password");
                    }
                    dm.Password = newPassHash;
                    dm.UpdatedAt = DateTime.UtcNow;
                    dm.UpdatedBy = _loginUserDetail.LoginId;
                    if(await _apiDbContext.SaveChangesAsync() > 0)
                    {
                        return new BoolResponseRoot(true, "Your password has been updated successfully.");
                    }
                    else
                    {
                        throw new SiffrumException(ApiErrorTypeSM.Fatal_Log, 
                            $"Password updation failed for user with emailId: {dm.Email}",
                            "Something went wrong while updating your password, Please try again later");
                    }
                }
            }
            return new BoolResponseRoot(false, "User details not found");
        }

        public async Task<BoolResponseRoot> SetPassord(long id, SetPasswordRequestSM objSM)
        {
            var dm = await _apiDbContext.User.FindAsync(id);
            if(dm == null)
            {
                throw new SiffrumException(ApiErrorTypeSM.Fatal_Log, $"User Not Found for Id:{id}",
                    "User Not Found");
            }
            if (!string.IsNullOrEmpty(dm.Password))
            {
                return new BoolResponseRoot(false, "Password is already present, cannot set password");
            }
            if (dm.Status == StatusDM.Inactive)
            {
                throw new SiffrumException(ApiErrorTypeSM.InvalidToken_Log,
                    $"User with Id {id} is inactive or already deleted but trying to set password",
                    "User details not found");
            }
            var passwordHash = await _passwordEncryptHelper.ProtectAsync<string>(objSM.Password);
            dm.Password = passwordHash;
            dm.UpdatedAt = DateTime.UtcNow;
            dm.UpdatedBy = _loginUserDetail.LoginId;
            if(await _apiDbContext.SaveChangesAsync() > 0)
            {
                return new BoolResponseRoot(true, "Password has been set successfully");
            }
            throw new SiffrumException(ApiErrorTypeSM.Fatal_Log, $"Set password has been failed for UserId: {id}, and Password: {objSM.Password}",
                   "Something went wrong while updating password");
        }
        #endregion Forgot/Update/Validate Password

        #region Is Delivery Available

        public async Task<DeliveryAvailabiltySM> IsDeliveryAvailable(LocationRequestSM request)
        {
            var response = new DeliveryAvailabiltySM()
            {
                IsDeliveryAvailable = false,
                Address = "",
                Pincode = "",
                Latitude = null,
                Longitude = null,
                SurgeResponse = false
            };

            var address = await _geoService.GetAddressFromLatLongAsync((double)request.Latitude,(double)request.Longitude);
            if (address == null || string.IsNullOrEmpty(address.Pincode))
            {
                return response;
            }
            
            var todayStart = DateTime.UtcNow.Date;
            var tomorrowStart = todayStart.AddDays(1);
            var orderCount = await _apiDbContext.Order
                .Where(x => x.PaymentStatus == PaymentStatusDM.Paid
                    && x.CreatedAt >= todayStart
                    && x.CreatedAt < tomorrowStart)
                .CountAsync();
            var settings = await _settingsProcess.GetAsync();
            if(orderCount > settings.SurgeCount)
            {
                response.SurgeResponse = true;
            }
            else
            {
                response.SurgeResponse = false;
            }
            var pincodeDetails = await _apiDbContext.DeliveryPlaces.FirstOrDefaultAsync(x => x.Pincode == address.Pincode && x.Status == StatusDM.Active);
            if (pincodeDetails != null)
            {
                var sellerDetails = await _apiDbContext.Seller.FindAsync(pincodeDetails.SellerId);
                response.IsDeliveryAvailable = true;
                response.Address = address.Address;
                response.Pincode = address.Pincode;
                response.Latitude = sellerDetails.Latitude;
                response.Longitude = sellerDetails.Longitude;
                response.SurgeResponse = orderCount > 100;
                return response;
            }
            else
            {
                response.Address = address.Address;
                response.Pincode = address.Pincode;
                return response;
            }
        }


        #endregion Is Delivery Available

        #region Find By Pincode
              

        public async Task<List<UserSM>> GetUserByPincodeAsync(
    string? pincode,
    string? searchString,
    int skip,
    int top)
        {
            if (string.IsNullOrWhiteSpace(pincode) && string.IsNullOrWhiteSpace(searchString))
            {
                throw new SiffrumException(
                    ApiErrorTypeSM.InvalidInputData_NoLog,
                    "Either pincode or searchString must be provided");
            }

            var userIds = new HashSet<long>();

            // -----------------------------
            // Search by Pincode
            // -----------------------------
            if (!string.IsNullOrWhiteSpace(pincode))
            {
                var pincodeUsers = await _apiDbContext.UserAddress
                    .AsNoTracking()
                    .Where(x => x.Pincode == pincode)
                    .Select(x => x.UserId)
                    .Distinct()
                    .ToListAsync();

                foreach (var id in pincodeUsers)
                    userIds.Add(id);
            }

            // -----------------------------
            // Search by Name
            // -----------------------------
            if (!string.IsNullOrWhiteSpace(searchString))
            {
                var words = searchString
                    .Trim()
                    .Split(' ', StringSplitOptions.RemoveEmptyEntries);

                IQueryable<UserDM> query = _apiDbContext.User
                    .AsNoTracking();

                foreach (var word in words)
                {
                    var lowerWord = word.ToLower();
                    query = query.Where(x => x.Name.ToLower().Contains(lowerWord));
                }

                var nameUsers = await query
                    .Select(x => x.Id)
                    .ToListAsync();

                foreach (var id in nameUsers)
                    userIds.Add(id);
            }

            var pagedUserIds = userIds
                .Skip(skip)
                .Take(top)
                .ToList();

            var response = new List<UserSM>();

            foreach (var id in pagedUserIds)
            {
                var user = await GetByIdAsync(id);
                if (user != null)
                {
                    response.Add(user);
                }
            }

            return response;
        }


        public async Task<IntResponseRoot> GetUserByPincodeCountAsync(string? pincode, string? searchString)
        {
            if (string.IsNullOrWhiteSpace(pincode) && string.IsNullOrWhiteSpace(searchString))
            {
                throw new SiffrumException(
                    ApiErrorTypeSM.InvalidInputData_NoLog,
                    "Either pincode or searchString must be provided");
            }

            var userIds = new HashSet<long>();

            // -----------------------------
            // Search by Pincode
            // -----------------------------
            if (!string.IsNullOrWhiteSpace(pincode))
            {
                var pincodeUsers = await _apiDbContext.UserAddress
                    .AsNoTracking()
                    .Where(x => x.Pincode == pincode)
                    .Select(x => x.UserId)
                    .Distinct()
                    .ToListAsync();

                foreach (var id in pincodeUsers)
                    userIds.Add(id);
            }

            // -----------------------------
            // Search by Name (Multi-word)
            // -----------------------------
            if (!string.IsNullOrWhiteSpace(searchString))
            {
                var words = searchString
                    .Trim()
                    .Split(' ', StringSplitOptions.RemoveEmptyEntries);

                IQueryable<UserDM> query = _apiDbContext.User
                    .AsNoTracking();

                foreach (var word in words)
                {
                    var lowerWord = word.ToLower();
                    query = query.Where(x => x.Name.ToLower().Contains(lowerWord));
                }

                var nameUsers = await query
                    .Select(x => x.Id)
                    .ToListAsync();

                foreach (var id in nameUsers)
                    userIds.Add(id);
            }

            var count = userIds.Count;

            return new IntResponseRoot(count, "Total Users");
        }

        #endregion Find By Pincode

        #region Generate Referal Code
        private async Task<string> GenerateUniqueReferralCode()
        {
            string code;
            do
            {
                code = GenerateShortUniqueKey();
            }
            while (await _apiDbContext.User.AnyAsync(u => u.ReferralCode == code));

            return code;
        }
        public static string GenerateShortUniqueKey()
        {
            var guidPart = Convert.ToBase64String(Guid.NewGuid().ToByteArray())
                .Replace("/", "")
                .Replace("+", "")
                .Replace("=", "");

            var timePart = DateTime.UtcNow.Ticks.ToString();

            var referalcode =  (guidPart + timePart)
                .Substring(0, 10)
                .ToUpper();
            return referalcode;
        }

        #endregion Generate Referal Code

        #region Seller Assignment

        public async Task<AssignedSellerResponseSM> FinalSellerAssigned(long userId, FinalSellerAssignedRequestSM request)
        {
            if (request.SellerId <= 0)
                throw new SiffrumException(ApiErrorTypeSM.InvalidInputData_NoLog, "SellerId is required");

            var seller = await _apiDbContext.Seller
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == request.SellerId && s.Status == SellerStatusDM.Active);

            if (seller == null)
                throw new SiffrumException(ApiErrorTypeSM.InvalidInputData_NoLog,
                    $"Seller with Id {request.SellerId} not found or inactive",
                    "Selected seller is not available. Please try again.");

            var user = await _apiDbContext.User.FindAsync(userId);
            if (user == null)
                throw new SiffrumException(ApiErrorTypeSM.Fatal_Log,
                    $"User with Id {userId} not found",
                    "User not found");

            user.AssignedSellerId = request.SellerId;
            user.AssignedStoreId = request.StoreId ?? request.SellerId;
            user.SellerAssignedAt = DateTime.UtcNow;
            user.UpdatedAt = DateTime.UtcNow;

            await _apiDbContext.SaveChangesAsync();

            return new AssignedSellerResponseSM
            {
                SellerId = user.AssignedSellerId,
                StoreId = user.AssignedStoreId,
                AssignedAt = user.SellerAssignedAt
            };
        }

        public async Task<AssignedSellerResponseSM> GetAssignedSeller(long userId)
        {
            var user = await _apiDbContext.User
                .AsNoTracking()
                .Where(u => u.Id == userId)
                .Select(u => new AssignedSellerResponseSM
                {
                    SellerId = u.AssignedSellerId,
                    StoreId = u.AssignedStoreId,
                    AssignedAt = u.SellerAssignedAt
                })
                .FirstOrDefaultAsync();

            return user;
        }

        public async Task<long?> GetAssignedSellerIdForUser(long userId)
        {
            return await _apiDbContext.User
                .AsNoTracking()
                .Where(u => u.Id == userId)
                .Select(u => u.AssignedSellerId)
                .FirstOrDefaultAsync();
        }

        #endregion Seller Assignment

    }
}

