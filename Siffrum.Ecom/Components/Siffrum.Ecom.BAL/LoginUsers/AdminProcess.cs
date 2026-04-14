using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Siffrum.Ecom.BAL.Base.Email;
using Siffrum.Ecom.BAL.Base.ImageProcess;
using Siffrum.Ecom.BAL.ExceptionHandler;
using Siffrum.Ecom.BAL.Foundation.Base;
using Siffrum.Ecom.DAL.Context;
using Siffrum.Ecom.DomainModels.v1;
using Siffrum.Ecom.ServiceModels.AppUser;
using Siffrum.Ecom.ServiceModels.Enums;
using Siffrum.Ecom.ServiceModels.Foundation.Base.CommonResponseRoot;
using Siffrum.Ecom.ServiceModels.Foundation.Base.Enums;
using Siffrum.Ecom.ServiceModels.Foundation.Base.Interfaces;
using Siffrum.Ecom.ServiceModels.v1;
using Siffrum.Ecom.ServiceModels.v1.General;
using System.Text;
using Siffrum.Ecom.DomainModels.Enums;

namespace Siffrum.Ecom.BAL.LoginUsers
{
    public class AdminProcess : SiffrumBalOdataBase<AdminSM>
    {
        private readonly ILoginUserDetail _loginUserDetail;
        private readonly IPasswordEncryptHelper _passwordEncryptHelper;
        private readonly EmailProcess _emailProcess;
        private readonly ImageProcess _imageProcess;

        public AdminProcess(IMapper mapper,ApiDbContext apiDbContext,ImageProcess imageProcess,
            ILoginUserDetail loginUserDetail, IPasswordEncryptHelper passwordEncryptHelper, EmailProcess emailProcess)
            : base(mapper, apiDbContext)
        {
            _loginUserDetail = loginUserDetail;
            _passwordEncryptHelper = passwordEncryptHelper;
            _emailProcess = emailProcess;
            _imageProcess = imageProcess;
        }

        #region OData
        public override async Task<IQueryable<AdminSM>> GetServiceModelEntitiesForOdata()
        {
            IQueryable<AdminDM> entitySet = _apiDbContext.Admin.AsNoTracking();
            return await base.MapEntityAsToQuerable<AdminDM, AdminSM>(_mapper, entitySet);
        }
        #endregion

        #region CREATE
        public async Task<BoolResponseRoot> RegisterAdmin(AdminSM objSM)
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
            var isEmailAvailable = await GetAdminByEmail(objSM.Email);
            if (!isEmailAvailable)
            {
                throw new SiffrumException(
                    ApiErrorTypeSM.InvalidInputData_NoLog,
                    "Email already registered",
                    "An account with this email address already exists. Please use a different email or log in to your existing account."
                );
            }

            var isUsernameAvailable = await GetAdminByUsername(objSM.Username);
            if (!isUsernameAvailable)
            {
                throw new SiffrumException(
                    ApiErrorTypeSM.InvalidInputData_NoLog,
                    "Username already taken",
                    "This username is already in use. Please choose a different username or log in to your existing account."
                );
            }
            var dm = _mapper.Map<AdminDM>(objSM);
            var passwordHash = await _passwordEncryptHelper.ProtectAsync<string>(objSM.Password);
            dm.Password = passwordHash;
            if (objSM.RoleType == RoleTypeSM.SuperAdmin || objSM.RoleType == RoleTypeSM.SystemAdmin) 
            {
                dm.RoleType = (RoleTypeDM)objSM.RoleType;
            }
            else
            {
                return new BoolResponseRoot(false, "Role type is wrong, Please check and try again...");
            }            
            dm.CreatedAt = DateTime.UtcNow;
            dm.Status = StatusDM.Active;
            dm.LoginStatus = LoginStatusDM.Enabled;
            dm.CreatedBy = _loginUserDetail.LoginId;

            await _apiDbContext.Admin.AddAsync(dm);
            if(await _apiDbContext.SaveChangesAsync() > 0)
            {
                return new BoolResponseRoot(true, "Your account created Successfully, Now you can Login in to your account");
            }
            throw new SiffrumException(ApiErrorTypeSM.Fatal_Log, "Something went wrong while creating account for admin.");
           
        }

        public async Task<BoolResponseRoot> AssignDeviceTokenToDeliveryBoy(long adminId, DeviceTokenSM sm)
        {
            if (sm == null || string.IsNullOrEmpty(sm.DeviceToken))
            {
                return null;
            }
            var dm = await _apiDbContext.Admin.FindAsync(adminId);
            if (dm == null)
            {
                throw new SiffrumException(ApiErrorTypeSM.Fatal_Log,
                    $"User tries to assign device token token to Admin with Id: {adminId} who is not found",
                    "Admin Details Not Found");
            }
            dm.FcmId = sm.DeviceToken;
            dm.UpdatedAt = DateTime.UtcNow;
            dm.UpdatedBy = _loginUserDetail.LoginId;
            if (await _apiDbContext.SaveChangesAsync() > 0)
            {
                return new BoolResponseRoot(true, "Device token updated successfully");
            }
            throw new SiffrumException(ApiErrorTypeSM.Fatal_Log,
                $"Something went worng while updating device token: {sm.DeviceToken} to Admin with Id: {adminId}",
                "Something went wrong while updating device token");

        }

        #endregion

        #region READ

        public async Task<List<AdminSM>> GetAll(int skip, int top)
        {
            var adminIds = await _apiDbContext.Admin.Select(x=>x.Id).Skip(skip).Take(top).ToListAsync();
            if(adminIds.Count > 0)
            {
                var response = new List<AdminSM>();

                foreach(var id in adminIds)
                {
                    var admin = await GetByIdAsync(id);
                    if(admin != null)
                    {
                        response.Add(admin);
                    }
                }
                return response;
            }
            return new List<AdminSM>();
        }

        public async Task<IntResponseRoot> GetAllAdminsCount()
        {
            var count = await _apiDbContext.Admin.CountAsync();
            return new IntResponseRoot(count, "Total Count of admins");
            
        }

        public async Task<AdminSM?> GetByIdAsync(long id)
        {
            var dm = await _apiDbContext.Admin
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id);
            if (dm != null)
            {
                var response = _mapper.Map<AdminSM>(dm);                
                response.Password = null;
                return response;
            }

            return null;
        }
        #endregion

        #region UPDATE
        public async Task<AdminSM?> UpdateAsync(long id, AdminSM objSM)
        {
            if (objSM == null)
            {
                throw new SiffrumException(ApiErrorTypeSM.ModelError_NoLog, "Please provide details to update");
            }
            var dm = await _apiDbContext.Admin
                .FirstOrDefaultAsync(x => x.Id == id);
            if (dm == null)
            {
                throw new SiffrumException(ApiErrorTypeSM.InvalidToken_Log, $"Admin tried to update but not found for the Id: {id}", "Admin not found.");
            }

            if (objSM.Email != dm.Email)
            {
                throw new SiffrumException(ApiErrorTypeSM.InvalidInputData_NoLog, "The email address cannot be updated, Please contact support, if you need assistence");
            }
            if (objSM.Username != dm.Username)
            {
                throw new SiffrumException(ApiErrorTypeSM.InvalidInputData_NoLog, "Username cannot be updated, Please contact support, if you need assistence");
            }
            if (objSM != null)
            {
                objSM.Id = id;
                objSM.Password = dm.Password;
                objSM.RoleType = (RoleTypeSM)dm.RoleType;
                objSM.Status = (StatusSM)dm.Status;
                objSM.LoginStatus = (LoginStatusSM)dm.LoginStatus;
            }
        
            _mapper.Map(objSM, dm);

            dm.UpdatedAt = DateTime.UtcNow;
            dm.UpdatedBy = _loginUserDetail.LoginId;

            if (await _apiDbContext.SaveChangesAsync() > 0)
            {               
                return await GetByIdAsync(id);
            }

            throw new SiffrumException(ApiErrorTypeSM.Fatal_Log, $"Error in updating admin details with Id: {id}", "Something went wrong while updating profile details");
            
        }
        #endregion

        #region DELETE (SOFT DELETE)
        public async Task<DeleteResponseRoot> DeleteAsync(long id)
        {
            var dm = await _apiDbContext.Admin
                .FirstOrDefaultAsync(x => x.Id == id);

            if (dm == null)
            {
                throw new SiffrumException(ApiErrorTypeSM.InvalidInputData_Log, "User details not found");
            }
            if (dm.Status == StatusDM.Inactive)
            {
                throw new SiffrumException(ApiErrorTypeSM.InvalidToken_Log,
                    $"Admin with Id {id} is inactive or already deleted but trying to delete the data again",
                    "Admin details not found");
            }
            dm.Status = StatusDM.Inactive;
            dm.UpdatedBy = _loginUserDetail.LoginId;
            if (await _apiDbContext.SaveChangesAsync() > 0) 
            {
                return new DeleteResponseRoot(true, "Your account deleted successfully");
            }
            throw new SiffrumException(ApiErrorTypeSM.Fatal_Log, $"Admin with Id:{id} is not getting deleted successfully", "Something went wrong while deleting your accunt, Please contact service team");
        }
        #endregion

        #region Email/LoginId checker

        public async Task<bool> GetAdminByEmail(string email)
        {
            var existingAdmin = await _apiDbContext.Admin.AsNoTracking().FirstOrDefaultAsync(x => x.Email == email);
            if(existingAdmin == null)
            {
                return true;
            }
            return false;
        }      
        
        
        public async Task<bool> GetAdminByUsername(string username)
        {
            var existingAdmin = await _apiDbContext.Admin.AsNoTracking().FirstOrDefaultAsync(x => x.Username == username);
            if(existingAdmin == null)
            {
                return true;
            }
            return false;
        }

        #endregion Email/LoginId checker

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


            var baseUrl = "https://speedykart.org";
            var link = $"{baseUrl}/resetpassword?authCode={encodedAuthCode}";

            if (string.IsNullOrEmpty(forgotPassword.UserName))
                throw new SiffrumException(ApiErrorTypeSM.ModelError_NoLog, "User Name cannot be empty.");
            //Todo: Handle deleted users
            var user = await _apiDbContext.Admin.AsNoTracking().Where(x=>x.Email == forgotPassword.UserName || x.Username == forgotPassword.UserName).FirstOrDefaultAsync();

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
                    throw new SiffrumException(ApiErrorTypeSM.Fatal_Log, $"No Admin with username '{forgotPassword.UserName}' found.", $"No Admin with username '{forgotPassword.UserName}' found.");
                }
                var user = (from c in _apiDbContext.Admin
                            where c.Username.ToUpper() == forgotPassword.UserName.ToUpper()
                               || c.Email.ToUpper() == forgotPassword.UserName.ToUpper()
                            select new { Admin = c }).FirstOrDefault();

                if (user != null)
                {
                    string decrypt = "";
                    string newPassword = await _passwordEncryptHelper.ProtectAsync<string>(resetPasswordRequest.NewPassword);
                    if (string.Equals(user.Admin.Password, newPassword))
                    {
                        throw new SiffrumException(ApiErrorTypeSM.Fatal_Log, $"Please don't use old password, use new one.", $"Please don't use old password, use new one.");
                    }
                    else
                    {
                        resetPasswordRequest.NewPassword = await _passwordEncryptHelper.ProtectAsync(resetPasswordRequest.NewPassword);
                        user.Admin.Password = resetPasswordRequest.NewPassword;
                        await _apiDbContext.SaveChangesAsync();
                        return new BoolResponseRoot(true, "Password Updated Successfully");
                    }
                }
                else
                {
                    throw new SiffrumException(ApiErrorTypeSM.Fatal_Log, $"No Admin with username '{forgotPassword.UserName}' found.", $"No Admin with username '{forgotPassword.UserName}' found.");
                }
            }
            else
            {

                throw new SiffrumException(ApiErrorTypeSM.Fatal_Log, "Password can not be empty", "Password can not be empty");
            }
        }

        public async Task<BoolResponseRoot> ChangePassword(long adminId, UpdatePasswordRequestSM objSM)
        {
            if(string.IsNullOrEmpty(objSM.OldPassword) || string.IsNullOrEmpty(objSM.NewPassword))
            {
                throw new SiffrumException(ApiErrorTypeSM.InvalidInputData_NoLog, "Password is required, Please enter password.");
            }
            AdminDM dm = await _apiDbContext.Admin.FindAsync(adminId);
            if (dm != null) 
            {
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
                        throw new SiffrumException(ApiErrorTypeSM.Fatal_Log, $"Password updation failed for Admin with emailId: {dm.Email}",
                            "Something went wrong while updating your password, Please try again later");
                    }
                }
            }
            return new BoolResponseRoot(false, "Admin details not found");
        }

        public async Task<BoolResponseRoot> SetPassord(long id, SetPasswordRequestSM objSM)
        {
            var dm = await _apiDbContext.Admin.FindAsync(id);
            if (dm == null)
            {
                throw new SiffrumException(ApiErrorTypeSM.Fatal_Log, $"Admin Not Found for Id:{id}",
                    "Admin Not Found");
            }
            if (!string.IsNullOrEmpty(dm.Password))
            {
                return new BoolResponseRoot(false, "Password is already present, cannot set password");
            }
            var passwordHash = await _passwordEncryptHelper.ProtectAsync<string>(objSM.Password);
            dm.Password = passwordHash;
            dm.UpdatedAt = DateTime.UtcNow;
            dm.UpdatedBy = _loginUserDetail.LoginId;
            if (await _apiDbContext.SaveChangesAsync() > 0)
            {
                return new BoolResponseRoot(true, "Password has been set successfully");
            }
            throw new SiffrumException(ApiErrorTypeSM.Fatal_Log, $"Set password has been failed for AdminId: {id}, and Password: {objSM.Password}",
                   "Something went wrong while updating password");
        }

        public async Task<BoolResponseRoot> ForceResetPassword(long adminId, string newPassword)
        {
            if (string.IsNullOrWhiteSpace(newPassword))
                throw new SiffrumException(ApiErrorTypeSM.InvalidInputData_NoLog, "New password cannot be empty");

            var dm = await _apiDbContext.Admin.FindAsync(adminId);
            if (dm == null)
                throw new SiffrumException(ApiErrorTypeSM.InvalidInputData_NoLog, "Admin not found");

            if (dm.RoleType == DomainModels.Enums.RoleTypeDM.SuperAdmin)
                throw new SiffrumException(ApiErrorTypeSM.InvalidInputData_NoLog, "Cannot reset SuperAdmin password from here");

            var passwordHash = await _passwordEncryptHelper.ProtectAsync<string>(newPassword);
            dm.Password = passwordHash;
            dm.UpdatedAt = DateTime.UtcNow;
            dm.UpdatedBy = _loginUserDetail.LoginId;

            if (await _apiDbContext.SaveChangesAsync() > 0)
                return new BoolResponseRoot(true, "Password has been reset successfully");

            throw new SiffrumException(ApiErrorTypeSM.Fatal_Log,
                $"Force reset password failed for AdminId: {adminId}",
                "Something went wrong while resetting password");
        }

        #endregion Forgot/Update/Validate Password
    }
}
