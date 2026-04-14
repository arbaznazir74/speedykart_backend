using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Siffrum.Ecom.BAL.Base.Email;
using Siffrum.Ecom.BAL.Base.ImageProcess;
using Siffrum.Ecom.BAL.ExceptionHandler;
using Siffrum.Ecom.BAL.Foundation.Base;
using Siffrum.Ecom.DAL.Context;
using Siffrum.Ecom.DomainModels.v1;
using Siffrum.Ecom.ServiceModels.AppUser;
using Siffrum.Ecom.ServiceModels.AppUser.Login;
using Siffrum.Ecom.ServiceModels.Enums;
using Siffrum.Ecom.ServiceModels.Foundation.Base.CommonResponseRoot;
using Siffrum.Ecom.ServiceModels.Foundation.Base.Enums;
using Siffrum.Ecom.ServiceModels.Foundation.Base.Interfaces;
using Siffrum.Ecom.ServiceModels.v1;
using Siffrum.Ecom.ServiceModels.v1.General;
using System.Text;
using Siffrum.Ecom.DomainModels.Enums;
using Siffrum.Ecom.BAL.Base.OneSignal;

namespace Siffrum.Ecom.BAL.LoginUsers
{
    public class SellerProcess : SiffrumBalOdataBase<SellerSM>
    {
        private readonly ILoginUserDetail _loginUserDetail;
        private readonly IPasswordEncryptHelper _passwordEncryptHelper;
        private readonly EmailProcess _emailProcess;
        private readonly ImageProcess _imageProcess;
        private readonly NotificationProcess _notificationProcess;

        public SellerProcess(IMapper mapper,ApiDbContext apiDbContext,ImageProcess imageProcess,
            ILoginUserDetail loginUserDetail, IPasswordEncryptHelper passwordEncryptHelper, EmailProcess emailProcess,
            NotificationProcess notificationProcess)
            : base(mapper, apiDbContext)
        {
            _loginUserDetail = loginUserDetail;
            _passwordEncryptHelper = passwordEncryptHelper;
            _emailProcess = emailProcess;
            _imageProcess = imageProcess;
            _notificationProcess = notificationProcess;
        }

        #region OData
        public override async Task<IQueryable<SellerSM>> GetServiceModelEntitiesForOdata()
        {
            IQueryable<SellerDM> entitySet = _apiDbContext.Seller.AsNoTracking();
            return await base.MapEntityAsToQuerable<SellerDM, SellerSM>(_mapper, entitySet);
        }
        #endregion

        #region CREATE
        public async Task<BoolResponseRoot> RegisterSeller(SellerSM objSM, bool isAdminCreated = false)
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
            if(!isAdminCreated && (objSM.Latitude == null || objSM.Longitude == null))
            {
                throw new SiffrumException(ApiErrorTypeSM.InvalidInputData_NoLog, 
                    $"Latitude and Longitude does not have value for seller with email {objSM.Email} and trying to register",
                    "Please provide Latitude and Longitude");
            }
            var isEmailAvailable = await GetSellerByEmail(objSM.Email);
            if (!isEmailAvailable)
            {
                throw new SiffrumException(
                    ApiErrorTypeSM.InvalidInputData_NoLog,
                    "Email already registered",
                    "An account with this email address already exists. Please use a different email or log in to your existing account."
                );
            }

            var isUsernameAvailable = await GetSellerByUsername(objSM.Username);
            if (!isUsernameAvailable)
            {
                throw new SiffrumException(
                    ApiErrorTypeSM.InvalidInputData_NoLog,
                    "Username already taken",
                    "This username is already in use. Please choose a different username or log in to your existing account."
                );
            }
            
            var isMobileAvailable = await GetSellerByMobile(objSM?.Mobile);
            if (!isMobileAvailable)
            {
                throw new SiffrumException(
                    ApiErrorTypeSM.InvalidInputData_NoLog,
                    $"Mobile number {objSM?.Mobile} already taken",
                    "This mobile number is already in use. Please choose a different mobile number or log in to your existing account."
                );
            }
            var dm = _mapper.Map<SellerDM>(objSM);
            var passwordHash = await _passwordEncryptHelper.ProtectAsync<string>(objSM.Password);
            dm.Password = passwordHash;
            dm.RoleType = RoleTypeDM.Seller;
            dm.CreatedAt = DateTime.UtcNow;
            dm.DeletedAt = null;

            if (isAdminCreated)
            {
                dm.Status = SellerStatusDM.Active;
                dm.LoginStatus = LoginStatusDM.Enabled;
                dm.IsEmailConfirmed = true;
                dm.IsMobileConfirmed = true;
                dm.CreatedBy = "admin";
                dm.AdminId = _loginUserDetail?.DbRecordId;
            }
            else
            {
                dm.Status = SellerStatusDM.Pending;
                dm.LoginStatus = LoginStatusDM.Disabled;
                dm.IsEmailConfirmed = false;
                dm.IsMobileConfirmed = false;
                dm.CreatedBy = "self-signup";
                dm.AdminId = null;
            }

            await _apiDbContext.Seller.AddAsync(dm);
            if(await _apiDbContext.SaveChangesAsync() > 0)
            {
                // Send email verification link (non-blocking)
                try
                {
                    var link = "https://speedykart.org/verify-email";
                    var emailRequest = new EmailSM()
                    {
                        Email = objSM.Email,
                    };
                    await SendEmailVerificationLink(emailRequest, link);
                }
                catch { }

                // Notify admins about new seller signup
                try
                {
                    var adminNotification = new SendNotificationMessageSM
                    {
                        Title = "New Seller Registration",
                        Message = $"A new seller '{objSM.StoreName ?? objSM.Name}' has signed up and is awaiting approval."
                    };
                    await _notificationProcess.SendBulkPushNotificationToAdmins(adminNotification);
                }
                catch { }
            }

            return new BoolResponseRoot(true, "Your account has been created successfully! You will be notified once approved by the admin.");
        }
        public async Task<BoolResponseRoot> AssignDeviceTokenToDeliveryBoy(long sellerId, DeviceTokenSM sm)
        {
            if (sm == null || string.IsNullOrEmpty(sm.DeviceToken))
            {
                return null;
            }
            var dm = await _apiDbContext.Seller.FindAsync(sellerId);
            if (dm == null)
            {
                throw new SiffrumException(ApiErrorTypeSM.Fatal_Log,
                    $"User tries to assign device token token to Seller with Id: {sellerId} who is not found",
                    "Seller Details Not Found");
            }
            dm.FcmId = sm.DeviceToken;
            dm.UpdatedAt = DateTime.UtcNow;
            dm.UpdatedBy = _loginUserDetail.LoginId;
            if (await _apiDbContext.SaveChangesAsync() > 0)
            {
                return new BoolResponseRoot(true, "Device token updated successfully");
            }
            throw new SiffrumException(ApiErrorTypeSM.Fatal_Log,
                $"Something went worng while updating device token: {sm.DeviceToken} to Seller with Id: {sellerId}",
                "Something went wrong while updating device token");

        }
        public async Task<SellerSM> RegisterSocialUser(SellerSM objSM)
        {
            if (objSM == null)
            {
                throw new SiffrumException(ApiErrorTypeSM.InvalidInputData_NoLog, "Please provide details for Sign Up", "Please provide details for registration");
            }

            if (string.IsNullOrEmpty(objSM.Email))
            {
                throw new SiffrumException(ApiErrorTypeSM.InvalidInputData_NoLog, "Please provide EmailId ", "Please provide EmailId");
            }
            var dm = _mapper.Map<SellerDM>(objSM);
            dm.Password = null;
            dm.Username = null;
            dm.RoleType = RoleTypeDM.Seller;
            dm.IsEmailConfirmed = true;
            dm.IsMobileConfirmed = false;
            dm.CreatedAt = DateTime.UtcNow;
            dm.Status = SellerStatusDM.Active;
            dm.LoginStatus = LoginStatusDM.Enabled;
            dm.DeletedAt = null;
            dm.CreatedBy = _loginUserDetail.LoginId;

            await _apiDbContext.Seller.AddAsync(dm);
            if (await _apiDbContext.SaveChangesAsync() > 0)
            {
                return await GetByIdAsync(dm.Id);
            }
            throw new SiffrumException(ApiErrorTypeSM.Fatal_Log,
                $"Error in creating seller details using social login",
                "Something went wrong while creating your account, Please contact support team");
        }
        #endregion

        #region READ

        public async Task<List<SellerSM>> GetAll(int skip, int top)
        {
            var sellerIds = await _apiDbContext.Seller.Select(x=>x.Id).Skip(skip).Take(top).ToListAsync();
            if(sellerIds.Count > 0)
            {
                var response = new List<SellerSM>();

                foreach(var id in sellerIds)
                {
                    var seller = await GetByIdAsync(id);
                    if(seller != null)
                    {
                        response.Add(seller);
                    }
                }
                return response;
            }
            return new List<SellerSM>();
        }

        public async Task<List<StoreSM>> GetStoresForUser()
        {
            var stores = await _apiDbContext.Seller
                .AsNoTracking()
                .Where(s => s.Status == SellerStatusDM.Active
                         && s.Latitude != null && s.Longitude != null)
                .Select(s => new
                {
                    s.Id,
                    s.Latitude,
                    s.Longitude,
                    s.Status,
                    SettingsJson = _apiDbContext.SellerSettings
                        .Where(ss => ss.SellerId == s.Id)
                        .Select(ss => ss.JsonData)
                        .FirstOrDefault()
                })
                .ToListAsync();

            return stores.Select(s => new StoreSM
            {
                Id = s.Id,
                SellerId = s.Id,
                Latitude = s.Latitude,
                Longitude = s.Longitude,
                Status = (SellerStatusSM)s.Status,
                SellerSettingsJson = string.IsNullOrEmpty(s.SettingsJson)
                    ? null
                    : Newtonsoft.Json.JsonConvert.DeserializeObject<SellerSettingsJson>(s.SettingsJson)
            }).ToList();
        }

        public async Task<IntResponseRoot> GetAllSellersCount()
        {
            var count = await _apiDbContext.Seller.CountAsync();
            return new IntResponseRoot(count, "Total Count of sellers");
            
        }


        public async Task<List<SearchResponseSM>> SearchSellers(
          string searchText)
        {
            IQueryable<SellerDM> query = _apiDbContext.Seller
                .AsNoTracking();

            if (string.IsNullOrWhiteSpace(searchText))
            {
                return await query
                .OrderBy(x => x.Id)
                .Select(x => new SearchResponseSM
                {
                    Id = x.Id,
                    Title = x.Name
                })
                .ToListAsync();
            }

            var words = searchText
                .Trim()
                .Split(' ', StringSplitOptions.RemoveEmptyEntries);

            
            // Multi-word search using LIKE (Better performance)
            foreach (var word in words)
            {
                query = query.Where(x => x.Name.ToLower().Contains(word.ToLower()));
            }

            return await query
                .OrderBy(x => x.Name)
                .Select(x => new SearchResponseSM
                {
                    Id = x.Id,
                    Title = x.Name
                })
                .ToListAsync();
        }

        public async Task<SellerSM?> GetByIdAsync(long id)
        {
            var dm = await _apiDbContext.Seller
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id);
            string imagePath = null;
            if (dm != null)
            {
                var response = _mapper.Map<SellerSM>(dm);
                if (!string.IsNullOrEmpty(response.Logo))
                {
                    var logoImg = await _imageProcess.ResolveImage(response.Logo);
                    response.Logo = logoImg.Base64;
                    response.NetworkLogo = logoImg.NetworkUrl;
                }
                if (!string.IsNullOrEmpty(response.NationalIdentityCard))
                {
                    var cardImg = await _imageProcess.ResolveImage(response.NationalIdentityCard);
                    response.NationalIdentityCard = cardImg.Base64;
                    response.NetworkNationalIdentityCard = cardImg.NetworkUrl;
                }
                if (!string.IsNullOrEmpty(response.AddressProof))
                {
                    var proofImg = await _imageProcess.ResolveImage(response.AddressProof);
                    response.AddressProof = proofImg.Base64;
                    response.NetworkAddressProof = proofImg.NetworkUrl;
                }
                response.Password = null;
                return response;
            }

            return null;
        }
        
        public async Task<SellerSM?> GetMineAccountDetails(long id)
        {
            var dm = await _apiDbContext.Seller
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id);
            string imagePath = null;
            if (dm != null)
            {
                var activeResponse = await IsUserDeleted(dm);
                if(activeResponse.BoolResponse == false)
                {
                    throw new SiffrumException(ApiErrorTypeSM.InvalidToken_Log,
                        $"Seller with Id: {dm.Id} is not active but tried to access details"
                        , activeResponse.ResponseMessage);
                }
                var response = _mapper.Map<SellerSM>(dm);
                if (!string.IsNullOrEmpty(response.Logo))
                {
                    var logoImg = await _imageProcess.ResolveImage(response.Logo);
                    response.Logo = logoImg.Base64;
                    response.NetworkLogo = logoImg.NetworkUrl;
                }
                if (!string.IsNullOrEmpty(response.NationalIdentityCard))
                {
                    var cardImg = await _imageProcess.ResolveImage(response.NationalIdentityCard);
                    response.NationalIdentityCard = cardImg.Base64;
                    response.NetworkNationalIdentityCard = cardImg.NetworkUrl;
                }
                if (!string.IsNullOrEmpty(response.AddressProof))
                {
                    var proofImg = await _imageProcess.ResolveImage(response.AddressProof);
                    response.AddressProof = proofImg.Base64;
                    response.NetworkAddressProof = proofImg.NetworkUrl;
                }
                response.Password = null;
                return response;
            }

            return null;
        }
        #endregion

        #region UPDATE
        public async Task<SellerSM?> UpdateAsync(long id, SellerSM objSM, bool IsSocialLogin = false, bool isUserUpdation = false)
        {
            if (objSM == null)
            {
                throw new SiffrumException(ApiErrorTypeSM.ModelError_NoLog, "Please provide details to update");
            }
            var dm = await _apiDbContext.Seller
                .FirstOrDefaultAsync(x => x.Id == id);
            if (dm == null)
            {
                throw new SiffrumException(ApiErrorTypeSM.InvalidToken_Log, $"Seller tried to update but not found for the Id: {id}", "Seller not found.");
            }
            if (isUserUpdation)
            {
                var activeResponse = await IsUserDeleted(dm);
                if (activeResponse.BoolResponse == false)
                {
                    throw new SiffrumException(ApiErrorTypeSM.InvalidToken_Log,
                        $"Seller with Id: {dm.Id} is not active but tried to update details"
                        , activeResponse.ResponseMessage);
                }
            }
            if (objSM.Email != dm.Email)
            {
                throw new SiffrumException(ApiErrorTypeSM.InvalidInputData_NoLog, "The email address cannot be updated, Please contact support team, if you need assistence");
            }
            if (objSM.Username != dm.Username)
            {
                throw new SiffrumException(ApiErrorTypeSM.InvalidInputData_NoLog, "Username cannot be updated, Please contact support team, if you need assistence");
            }

            string previousLogo = null;
            string previousIdentityCard = null;
            string previousAddProof = null;
            if (objSM != null)
            {
                objSM.Id = id;
                objSM.Password = dm.Password;
                objSM.RoleType = (RoleTypeSM)dm.RoleType;
                objSM.Status = (SellerStatusSM)dm.Status;
                objSM.LoginStatus = (LoginStatusSM)dm.LoginStatus;
                objSM.AdminId = dm.AdminId;
                if(IsSocialLogin == true)
                {
                    objSM.IsEmailConfirmed = true;
                }
                else
                {
                    objSM.IsEmailConfirmed = dm.IsEmailConfirmed;
                }
                if (!string.IsNullOrEmpty(objSM.Logo))
                {
                    var imagePath = await _imageProcess.SaveFromBase64(objSM.Logo, "jpg", "wwwroot/content/loginusers/logo");
                    if (!string.IsNullOrEmpty(imagePath))
                    {
                        previousLogo = dm.Logo;
                        objSM.Logo = imagePath;
                    }                        
                }

                if (!string.IsNullOrEmpty(objSM.NationalIdentityCard))
                {
                    var idPath = await _imageProcess.SaveFromBase64(objSM.NationalIdentityCard, "jpg", "wwwroot/content/loginusers/cards");
                    if (!string.IsNullOrEmpty(idPath))
                    {
                        previousIdentityCard = dm.Logo;
                        objSM.NationalIdentityCard = idPath;
                    }                        
                }
                
                if (!string.IsNullOrEmpty(objSM.AddressProof))
                {
                    var addPath = await _imageProcess.SaveFromBase64(objSM.AddressProof, "jpg", "wwwroot/content/loginusers/cards");
                    if (!string.IsNullOrEmpty(addPath))
                    {
                        previousAddProof = dm.AddressProof;
                        objSM.AddressProof = addPath;
                    }                        
                }
            }
        
            _mapper.Map(objSM, dm);

            dm.UpdatedAt = DateTime.UtcNow;
            dm.UpdatedBy = _loginUserDetail.LoginId;

            if (await _apiDbContext.SaveChangesAsync() > 0)
            {
                if (File.Exists(previousLogo)) File.Delete(previousLogo);
                if (File.Exists(previousIdentityCard)) File.Delete(previousIdentityCard);
                if (File.Exists(previousAddProof)) File.Delete(previousAddProof);
                return await GetByIdAsync(id);
            }

            throw new SiffrumException(ApiErrorTypeSM.Fatal_Log, $"Error in updating seller details with Id: {id}", "Something went wrong while updating profile details");
            
        }
        #endregion

        #region Approve / Reject Seller

        public async Task<BoolResponseRoot> ApproveSeller(long sellerId)
        {
            var dm = await _apiDbContext.Seller.FirstOrDefaultAsync(x => x.Id == sellerId);
            if (dm == null)
                throw new SiffrumException(ApiErrorTypeSM.InvalidInputData_Log, "Seller not found");

            if (dm.Status == SellerStatusDM.Active)
                return new BoolResponseRoot(false, "Seller is already active");

            dm.Status = SellerStatusDM.Active;
            dm.LoginStatus = LoginStatusDM.Enabled;
            dm.IsEmailConfirmed = true;
            dm.UpdatedAt = DateTime.UtcNow;
            dm.UpdatedBy = _loginUserDetail.LoginId;

            if (await _apiDbContext.SaveChangesAsync() > 0)
            {
                // Send approval email
                if (!string.IsNullOrEmpty(dm.Email))
                {
                    try
                    {
                        var subject = "Your Seller Account Has Been Approved!";
                        var body = $@"
                        <html>
                          <body style='font-family: Arial, sans-serif; background-color: #f4f4f4; padding: 20px;'>
                            <div style='max-width: 600px; margin: auto; background: #ffffff; border-radius: 8px; overflow: hidden; box-shadow: 0 0 10px rgba(0,0,0,0.1);'>
                              <div style='background-color: #28a745; padding: 20px; text-align: center; color: #ffffff;'>
                                <h1 style='margin: 0; font-size: 24px;'>Account Approved</h1>
                              </div>
                              <div style='padding: 30px;'>
                                <p>Hello {dm.Name ?? dm.StoreName},</p>
                                <p>Great news! Your seller account on <strong>SpeedyCart</strong> has been approved by the admin.</p>
                                <p>You can now log in and start managing your store, products, and orders.</p>
                                <p style='text-align: center; margin: 30px 0;'>
                                  <a href='https://speedykart.org/login' style='display: inline-block; padding: 12px 25px; font-size: 16px; color: #ffffff; background-color: #28a745; text-decoration: none; border-radius: 5px;'>
                                    Log In Now
                                  </a>
                                </p>
                                <p>Warm regards,<br>Team SpeedyCart</p>
                              </div>
                            </div>
                          </body>
                        </html>";
                        _emailProcess.SendEmail(dm.Email, subject, body);
                    }
                    catch { }
                }
                return new BoolResponseRoot(true, "Seller approved successfully");
            }
            throw new SiffrumException(ApiErrorTypeSM.Fatal_Log, $"Failed to approve seller with Id: {sellerId}", "Something went wrong while approving the seller");
        }

        public async Task<BoolResponseRoot> RejectSeller(long sellerId, string? reason)
        {
            var dm = await _apiDbContext.Seller.FirstOrDefaultAsync(x => x.Id == sellerId);
            if (dm == null)
                throw new SiffrumException(ApiErrorTypeSM.InvalidInputData_Log, "Seller not found");

            if (dm.Status == SellerStatusDM.Rejected)
                return new BoolResponseRoot(false, "Seller is already rejected");

            dm.Status = SellerStatusDM.Rejected;
            dm.LoginStatus = LoginStatusDM.Disabled;
            dm.Remark = reason;
            dm.UpdatedAt = DateTime.UtcNow;
            dm.UpdatedBy = _loginUserDetail.LoginId;

            if (await _apiDbContext.SaveChangesAsync() > 0)
            {
                // Send rejection email
                if (!string.IsNullOrEmpty(dm.Email))
                {
                    try
                    {
                        var reasonText = string.IsNullOrEmpty(reason) ? "No specific reason was provided." : reason;
                        var subject = "Your Seller Account Application Update";
                        var body = $@"
                        <html>
                          <body style='font-family: Arial, sans-serif; background-color: #f4f4f4; padding: 20px;'>
                            <div style='max-width: 600px; margin: auto; background: #ffffff; border-radius: 8px; overflow: hidden; box-shadow: 0 0 10px rgba(0,0,0,0.1);'>
                              <div style='background-color: #dc3545; padding: 20px; text-align: center; color: #ffffff;'>
                                <h1 style='margin: 0; font-size: 24px;'>Application Not Approved</h1>
                              </div>
                              <div style='padding: 30px;'>
                                <p>Hello {dm.Name ?? dm.StoreName},</p>
                                <p>We regret to inform you that your seller account application on <strong>SpeedyCart</strong> has not been approved at this time.</p>
                                <p><strong>Reason:</strong> {reasonText}</p>
                                <p>If you believe this is an error or would like to re-apply with updated information, please contact our support team.</p>
                                <p>Warm regards,<br>Team SpeedyCart</p>
                              </div>
                            </div>
                          </body>
                        </html>";
                        _emailProcess.SendEmail(dm.Email, subject, body);
                    }
                    catch { }
                }
                return new BoolResponseRoot(true, "Seller rejected successfully");
            }
            throw new SiffrumException(ApiErrorTypeSM.Fatal_Log, $"Failed to reject seller with Id: {sellerId}", "Something went wrong while rejecting the seller");
        }

        public async Task<List<SellerSM>> GetPendingSellers(int skip, int top)
        {
            var sellerIds = await _apiDbContext.Seller
                .Where(x => x.Status == SellerStatusDM.Pending)
                .OrderByDescending(x => x.CreatedAt)
                .Select(x => x.Id)
                .Skip(skip).Take(top)
                .ToListAsync();

            var response = new List<SellerSM>();
            foreach (var id in sellerIds)
            {
                var seller = await GetByIdAsync(id);
                if (seller != null) response.Add(seller);
            }
            return response;
        }

        public async Task<IntResponseRoot> GetPendingSellersCount()
        {
            var count = await _apiDbContext.Seller.CountAsync(x => x.Status == SellerStatusDM.Pending);
            return new IntResponseRoot(count, "Total pending sellers");
        }

        #endregion Approve / Reject Seller

        #region Store Location Change

        public async Task<BoolResponseRoot> RequestLocationChange(long sellerId, decimal latitude, decimal longitude)
        {
            var dm = await _apiDbContext.Seller.FirstOrDefaultAsync(x => x.Id == sellerId);
            if (dm == null)
                throw new SiffrumException(ApiErrorTypeSM.InvalidInputData_Log, "Seller not found");

            if (dm.LocationStatus == 1)
                return new BoolResponseRoot(false, "You already have a pending location change request. Please wait for admin approval.");

            dm.PendingLatitude = latitude;
            dm.PendingLongitude = longitude;
            dm.LocationStatus = 1; // Pending
            dm.UpdatedAt = DateTime.UtcNow;

            if (await _apiDbContext.SaveChangesAsync() > 0)
            {
                try
                {
                    var adminNotification = new SendNotificationMessageSM
                    {
                        Title = "Location Change Request",
                        Message = $"Seller '{dm.StoreName ?? dm.Name}' has requested a store location change."
                    };
                    await _notificationProcess.SendBulkPushNotificationToAdmins(adminNotification);
                }
                catch { }
                return new BoolResponseRoot(true, "Location change request submitted. You will be notified once approved by admin.");
            }
            throw new SiffrumException(ApiErrorTypeSM.Fatal_Log, "Failed to submit location change request");
        }

        public async Task<BoolResponseRoot> ApproveLocationChange(long sellerId)
        {
            var dm = await _apiDbContext.Seller.FirstOrDefaultAsync(x => x.Id == sellerId);
            if (dm == null)
                throw new SiffrumException(ApiErrorTypeSM.InvalidInputData_Log, "Seller not found");

            if (dm.LocationStatus != 1)
                return new BoolResponseRoot(false, "No pending location change request for this seller");

            dm.Latitude = dm.PendingLatitude;
            dm.Longitude = dm.PendingLongitude;
            dm.PendingLatitude = null;
            dm.PendingLongitude = null;
            dm.LocationStatus = 2; // Approved
            dm.UpdatedAt = DateTime.UtcNow;
            dm.UpdatedBy = _loginUserDetail.LoginId;

            if (await _apiDbContext.SaveChangesAsync() > 0)
                return new BoolResponseRoot(true, "Location change approved");

            throw new SiffrumException(ApiErrorTypeSM.Fatal_Log, "Failed to approve location change");
        }

        public async Task<BoolResponseRoot> RejectLocationChange(long sellerId)
        {
            var dm = await _apiDbContext.Seller.FirstOrDefaultAsync(x => x.Id == sellerId);
            if (dm == null)
                throw new SiffrumException(ApiErrorTypeSM.InvalidInputData_Log, "Seller not found");

            if (dm.LocationStatus != 1)
                return new BoolResponseRoot(false, "No pending location change request for this seller");

            dm.PendingLatitude = null;
            dm.PendingLongitude = null;
            dm.LocationStatus = 3; // Rejected
            dm.UpdatedAt = DateTime.UtcNow;
            dm.UpdatedBy = _loginUserDetail.LoginId;

            if (await _apiDbContext.SaveChangesAsync() > 0)
                return new BoolResponseRoot(true, "Location change rejected");

            throw new SiffrumException(ApiErrorTypeSM.Fatal_Log, "Failed to reject location change");
        }

        public async Task<List<SellerSM>> GetSellersWithPendingLocation(int skip, int top)
        {
            var sellerIds = await _apiDbContext.Seller
                .Where(x => x.LocationStatus == 1)
                .OrderByDescending(x => x.UpdatedAt)
                .Select(x => x.Id)
                .Skip(skip).Take(top)
                .ToListAsync();

            var response = new List<SellerSM>();
            foreach (var id in sellerIds)
            {
                var seller = await GetByIdAsync(id);
                if (seller != null) response.Add(seller);
            }
            return response;
        }

        public async Task<IntResponseRoot> GetSellersWithPendingLocationCount()
        {
            var count = await _apiDbContext.Seller.CountAsync(x => x.LocationStatus == 1);
            return new IntResponseRoot(count, "Total pending location requests");
        }

        public async Task<BoolResponseRoot> AdminSetLocation(long sellerId, decimal latitude, decimal longitude)
        {
            var dm = await _apiDbContext.Seller.FirstOrDefaultAsync(x => x.Id == sellerId);
            if (dm == null)
                throw new SiffrumException(ApiErrorTypeSM.InvalidInputData_Log, "Seller not found");

            dm.Latitude = latitude;
            dm.Longitude = longitude;
            dm.PendingLatitude = null;
            dm.PendingLongitude = null;
            dm.LocationStatus = 0;
            dm.UpdatedAt = DateTime.UtcNow;
            dm.UpdatedBy = _loginUserDetail.LoginId;

            if (await _apiDbContext.SaveChangesAsync() > 0)
                return new BoolResponseRoot(true, "Seller location updated successfully");

            throw new SiffrumException(ApiErrorTypeSM.Fatal_Log, "Failed to update seller location");
        }

        #endregion Store Location Change

        #region DELETE (SOFT DELETE)
        public async Task<DeleteResponseRoot> DeleteAsync(long id)
        {
            var dm = await _apiDbContext.Seller
                .FirstOrDefaultAsync(x => x.Id == id);            
            if (dm == null)
            {
                throw new SiffrumException(ApiErrorTypeSM.InvalidInputData_Log, "User details not found");
            }
            var activeResponse = await IsUserDeleted(dm);
            if (activeResponse.BoolResponse == false)
            {
                throw new SiffrumException(ApiErrorTypeSM.InvalidToken_Log,
                    $"Seller with Id: {dm.Id} is not active but tried to delete his account"
                    , activeResponse.ResponseMessage);
            }
            dm.Status = DomainModels.Enums.SellerStatusDM.Removed;
            dm.DeletedAt = DateTime.UtcNow;
            dm.UpdatedBy = _loginUserDetail.LoginId;
            var previousLogo = dm.Logo;
            var previousIdentityCard = dm.NationalIdentityCard;
            var previousAddProof = dm.AddressProof;
            if (await _apiDbContext.SaveChangesAsync() > 0) 
            {
                if (File.Exists(previousLogo)) File.Delete(previousLogo);
                if (File.Exists(previousIdentityCard)) File.Delete(previousIdentityCard);
                if (File.Exists(previousAddProof)) File.Delete(previousAddProof);
                return new DeleteResponseRoot(true, "Your account deleted successfully");
            }
            throw new SiffrumException(ApiErrorTypeSM.Fatal_Log, $"Seller with Id:{id} is not getting deleted successfully", "Something went wrong while deleting your accunt, Please contact support team");
        }
        #endregion

        #region Email/LoginId checker

        public async Task<bool> GetSellerByEmail(string email)
        {
            var existingSeller = await _apiDbContext.Seller.AsNoTracking().FirstOrDefaultAsync(x => x.Email == email);
            if(existingSeller == null)
            {

                return true;
            }
            return false;
        }

        public async Task<SellerSM> GetSellerDetailsByEmail(string email)
        {
            var existingSeller = await _apiDbContext.Seller.AsNoTracking().FirstOrDefaultAsync(x => x.Email == email);
            if (existingSeller != null)
            {

                return _mapper.Map<SellerSM>(existingSeller);
            }
            return null;
        }

        public async Task<bool> GetSellerByUsername(string username)
        {
            var existingSeller = await _apiDbContext.Seller.AsNoTracking().FirstOrDefaultAsync(x => x.Username == username);
            if(existingSeller == null)
            {
                return true;
            }
            return false;
        }
        public async Task<bool> GetSellerByMobile(string mobile)
        {
            var existingSeller = await _apiDbContext.Seller.AsNoTracking().FirstOrDefaultAsync(x => x.Mobile == mobile);
            if(existingSeller == null)
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
            var email = GetSellerByEmail(objSM.Email);
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
                throw new SiffrumException(ApiErrorTypeSM.Fatal_Log, $"Seller not found with email {objSM.Email}, tries to send email verification by logged in user with loginId {_loginUserDetail.LoginId}",$"Seller not found with email '{objSM.Email}' found.");
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
                var user = await _apiDbContext.Seller.Where(x => x.Email.ToUpper() == sm.Email.ToUpper()).FirstOrDefaultAsync();

                if (user != null)
                {
                    if (user.IsEmailConfirmed == true)
                    {
                        return new BoolResponseRoot(false, "Your Email is already Verified, Login to your Account now");
                    }
                    var activeResponse = await IsUserDeleted(user);
                    if (activeResponse.BoolResponse == false)
                    {
                        throw new SiffrumException(ApiErrorTypeSM.InvalidToken_Log,
                            $"Seller with Id: {user.Id} is not active but tried to verify email"
                            , activeResponse.ResponseMessage);
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
                    throw new SiffrumException(ApiErrorTypeSM.Fatal_Log, $"No Seller with email '{sm.Email}' found.", $"No Seller with Email '{sm.Email}' found.");
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


            var baseUrl = "https://speedykart.org";
            var link = $"{baseUrl}/resetpassword?authCode={encodedAuthCode}";

            if (string.IsNullOrEmpty(forgotPassword.UserName))
                throw new SiffrumException(ApiErrorTypeSM.ModelError_NoLog, "User Name cannot be empty.");
            //Todo: Handle deleted users
            var user = await _apiDbContext.Seller.AsNoTracking().Where(x=>x.Email == forgotPassword.UserName || x.Username == forgotPassword.UserName).FirstOrDefaultAsync();
            
            if(user != null)
            {
                var activeResponse = await IsUserDeleted(user);
                if (activeResponse.BoolResponse == false)
                {
                    throw new SiffrumException(ApiErrorTypeSM.InvalidToken_Log,
                        $"Seller with Id: {user.Id} is not active but tried to send forgot password request"
                        , activeResponse.ResponseMessage);
                }
            }
            else
            {
                throw new SiffrumException(ApiErrorTypeSM.Fatal_Log, $"No User with username '{forgotPassword.UserName}' found.");
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
                    throw new SiffrumException(ApiErrorTypeSM.Fatal_Log, $"No Seller with username '{forgotPassword.UserName}' found.", $"No Seller with username '{forgotPassword.UserName}' found.");
                }
                var user = (from c in _apiDbContext.Seller
                            where c.Username.ToUpper() == forgotPassword.UserName.ToUpper()
                               || c.Email.ToUpper() == forgotPassword.UserName.ToUpper()
                            select new { Seller = c }).FirstOrDefault();

                if (user != null)
                {
                    var activeResponse = await IsUserDeleted(user.Seller);
                    if (activeResponse.BoolResponse == false)
                    {
                        throw new SiffrumException(ApiErrorTypeSM.InvalidToken_Log,
                            $"Seller with Id: {user.Seller.Id} is not active but tried update password"
                            , activeResponse.ResponseMessage);
                    }
                    string decrypt = "";
                    string newPassword = await _passwordEncryptHelper.ProtectAsync<string>(resetPasswordRequest.NewPassword);
                    if (string.Equals(user.Seller.Password, newPassword))
                    {
                        throw new SiffrumException(ApiErrorTypeSM.Fatal_Log, $"Please don't use old password, use new one.", $"Please don't use old password, use new one.");
                    }
                    else
                    {
                        resetPasswordRequest.NewPassword = await _passwordEncryptHelper.ProtectAsync(resetPasswordRequest.NewPassword);
                        user.Seller.Password = resetPasswordRequest.NewPassword;
                        await _apiDbContext.SaveChangesAsync();
                        return new BoolResponseRoot(true, "Password Updated Successfully");
                    }
                }
                else
                {
                    throw new SiffrumException(ApiErrorTypeSM.Fatal_Log, $"No Seller with username '{forgotPassword.UserName}' found.", $"No Seller with username '{forgotPassword.UserName}' found.");
                }
            }
            else
            {

                throw new SiffrumException(ApiErrorTypeSM.Fatal_Log, "Password can not be empty", "Password can not be empty");
            }
        }

        public async Task<BoolResponseRoot> ChangePassword(long sellerId, UpdatePasswordRequestSM objSM)
        {
            if(string.IsNullOrEmpty(objSM.OldPassword) || string.IsNullOrEmpty(objSM.NewPassword))
            {
                throw new SiffrumException(ApiErrorTypeSM.InvalidInputData_NoLog, "Password is required, Please enter password.");
            }
            SellerDM dm = await _apiDbContext.Seller.FindAsync(sellerId);
            if (dm != null) 
            {
                var activeResponse = await IsUserDeleted(dm);
                if (activeResponse.BoolResponse == false)
                {
                    throw new SiffrumException(ApiErrorTypeSM.InvalidToken_Log,
                        $"Seller with Id: {dm.Id} is not active but tried to update password"
                        , activeResponse.ResponseMessage);
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
                        throw new SiffrumException(ApiErrorTypeSM.Fatal_Log, $"Password updation failed for seller with emailId: {dm.Email}",
                            "Something went wrong while updating your password, Please try again later");
                    }
                }
            }
            return new BoolResponseRoot(false, "Seller details not found");
        }

        public async Task<BoolResponseRoot> SetPassord(long id, SetPasswordRequestSM objSM)
        {
            var dm = await _apiDbContext.Seller.FindAsync(id);
            if (dm == null)
            {
                throw new SiffrumException(ApiErrorTypeSM.Fatal_Log, $"Seller Not Found for Id:{id}",
                    "Seller Not Found");
            }
            var activeResponse = await IsUserDeleted(dm);
            if (activeResponse.BoolResponse == false)
            {
                throw new SiffrumException(ApiErrorTypeSM.InvalidToken_Log,
                    $"Seller with Id: {dm.Id} is not active but tried to set password"
                    , activeResponse.ResponseMessage);
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
            throw new SiffrumException(ApiErrorTypeSM.Fatal_Log, $"Set password has been failed for SellerId: {id}, and Password: {objSM.Password}",
                   "Something went wrong while updating password");
        }

        #endregion Forgot/Update/Validate Password

        #region Handle Soft Deletion

        public async Task<BoolResponseRoot> IsUserDeleted(SellerDM dm)
        {
            return dm.Status switch
            {
                SellerStatusDM.Active =>
                    new BoolResponseRoot(true, "User is active"),

                SellerStatusDM.Rejected =>
                    new BoolResponseRoot(false, "Your account has been rejected"),

                SellerStatusDM.Deactivated =>
                    new BoolResponseRoot(false, "Your account is deactivated. Please contact support"),

                SellerStatusDM.Removed =>
                    new BoolResponseRoot(false, "Your account has been removed"),

                SellerStatusDM.Blocked =>
                    new BoolResponseRoot(false, "Your account is blocked due to policy violations"),

                SellerStatusDM.Unknown or _ =>
                    new BoolResponseRoot(false, "User status is not valid")
            };
        }
        #endregion Handle Soft Deletion
    }
}
