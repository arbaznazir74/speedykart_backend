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
using System.Data;
using System.CodeDom;

namespace Siffrum.Ecom.BAL.LoginUsers
{
    public class DeliveryBoyProcess : SiffrumBalOdataBase<DeliveryBoySM>
    {
        #region Properties
        

        private readonly ILoginUserDetail _loginUserDetail;
        private readonly IPasswordEncryptHelper _passwordEncryptHelper;
        private readonly EmailProcess _emailProcess;
        private readonly ImageProcess _imageProcess;

        #endregion Properties

        #region Constructor
        
        public DeliveryBoyProcess(IMapper mapper,ApiDbContext apiDbContext,ImageProcess imageProcess,
            ILoginUserDetail loginUserDetail, IPasswordEncryptHelper passwordEncryptHelper, EmailProcess emailProcess)
            : base(mapper, apiDbContext)
        {
            _loginUserDetail = loginUserDetail;
            _passwordEncryptHelper = passwordEncryptHelper;
            _emailProcess = emailProcess;
            _imageProcess = imageProcess;
        }

        #endregion Constructor

        #region OData
        public override async Task<IQueryable<DeliveryBoySM>> GetServiceModelEntitiesForOdata()
        {
            IQueryable<DeliveryBoyDM> entitySet = _apiDbContext.DeliveryBoy.AsNoTracking();
            return await base.MapEntityAsToQuerable<DeliveryBoyDM, DeliveryBoySM>(_mapper, entitySet);
        }
        #endregion

        #region CREATE
        public async Task<BoolResponseRoot> RegisterDeliveryBoy(long adminId, DeliveryBoySM objSM)
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
            if (string.IsNullOrEmpty(objSM.Mobile))
            {
                throw new SiffrumException(ApiErrorTypeSM.InvalidInputData_NoLog, "Mobile number is required", "Mobile number is required");
            }
            if (string.IsNullOrEmpty(objSM.DrivingLicense))
            {
                throw new SiffrumException(ApiErrorTypeSM.InvalidInputData_NoLog, "Driving license number is required", "Driving license number is required");
            }
            if (string.IsNullOrEmpty(objSM.AadhaarNumber))
            {
                throw new SiffrumException(ApiErrorTypeSM.InvalidInputData_NoLog, "Aadhaar number is required", "Aadhaar number is required");
            }
            if (string.IsNullOrEmpty(objSM.DrivingLicensePhoto))
            {
                throw new SiffrumException(ApiErrorTypeSM.InvalidInputData_NoLog, "Driving license photo is required", "Driving license photo is required");
            }
            if (string.IsNullOrEmpty(objSM.AadhaarPhoto))
            {
                throw new SiffrumException(ApiErrorTypeSM.InvalidInputData_NoLog, "Aadhaar photo is required", "Aadhaar photo is required");
            }
            if (string.IsNullOrEmpty(objSM.PassportPhoto))
            {
                throw new SiffrumException(ApiErrorTypeSM.InvalidInputData_NoLog, "Passport size photo is required", "Passport size photo is required");
            }
            var isEmailAvailable = await GetDeliveryBoyByEmail(objSM.Email);
            if (!isEmailAvailable)
            {
                throw new SiffrumException(
                    ApiErrorTypeSM.InvalidInputData_NoLog,
                    "Email already registered",
                    "An account with this email address already exists. Please use a different email or log in to your existing account."
                );
            }

            var isUsernameAvailable = await GetDeliveryBoyByUsername(objSM.Username);
            if (!isUsernameAvailable)
            {
                throw new SiffrumException(
                    ApiErrorTypeSM.InvalidInputData_NoLog,
                    "Username already taken",
                    "This username is already in use. Please choose a different username or log in to your existing account."
                );
            }
            var dm = _mapper.Map<DeliveryBoyDM>(objSM);
            var passwordHash = await _passwordEncryptHelper.ProtectAsync<string>(objSM.Password);
            dm.Password = passwordHash;
            dm.RoleType = RoleTypeDM.DeliveryBoy;
            dm.IsEmailConfirmed = true;
            dm.IsMobileConfirmed = true;
            dm.CreatedAt = DateTime.UtcNow;
            dm.Status = DeliveryBoyStatusDM.Active;
            dm.LoginStatus = LoginStatusDM.Enabled;
            dm.CreatedBy = _loginUserDetail.LoginId;
            dm.AdminId = adminId;

            const string docPath = "content/deliveryboys/documents";
            if (!string.IsNullOrEmpty(objSM.DrivingLicensePhoto))
                dm.DrivingLicensePhoto = await _imageProcess.SaveFromBase64(objSM.DrivingLicensePhoto, "jpg", docPath);
            if (!string.IsNullOrEmpty(objSM.AadhaarPhoto))
                dm.AadhaarPhoto = await _imageProcess.SaveFromBase64(objSM.AadhaarPhoto, "jpg", docPath);
            if (!string.IsNullOrEmpty(objSM.PassportPhoto))
                dm.PassportPhoto = await _imageProcess.SaveFromBase64(objSM.PassportPhoto, "jpg", docPath);

            await _apiDbContext.DeliveryBoy.AddAsync(dm);
            if(await _apiDbContext.SaveChangesAsync() > 0)
            {
                return new BoolResponseRoot(true, "Your account has been created Successfully");
            }
            throw new SiffrumException(ApiErrorTypeSM.Fatal_Log, "Something went wrong while creating account for delivery boy, please try again later");
            
        }

        public async Task<BoolResponseRoot> AssignDeviceTokenToDeliveryBoy(long deliveryBoyId, DeviceTokenSM sm)
        {
            if(sm == null || string.IsNullOrEmpty(sm.DeviceToken))
            {
                return null;
            }
            var dm = await _apiDbContext.DeliveryBoy.FindAsync(deliveryBoyId);
            if(dm == null)
            {
                throw new SiffrumException(ApiErrorTypeSM.Fatal_Log,
                    $"User tries to assign device token token to delivery boy with Id: {deliveryBoyId} who is not found", 
                    "User Details Not Found");
            }
            dm.FcmId = sm.DeviceToken;
            dm.UpdatedAt = DateTime.UtcNow;
            dm.UpdatedBy = _loginUserDetail.LoginId;
            if(await _apiDbContext.SaveChangesAsync() > 0)
            {
                return new BoolResponseRoot(true, "Device token updated successfully");
            }
            throw new SiffrumException(ApiErrorTypeSM.Fatal_Log,
                $"Something went worng while updating device token: {sm.DeviceToken} to delivery boy with Id: {deliveryBoyId}",
                "Something went wrong while updating device token");

        }
        #endregion

        #region READ

        public async Task<List<DeliveryBoySM>> GetAll(int skip, int top)
        {
            var dmIds = await _apiDbContext.DeliveryBoy.Select(x=>x.Id).Skip(skip).Take(top).ToListAsync();
            if(dmIds.Count > 0)
            {
                var response = new List<DeliveryBoySM>();

                foreach(var id in dmIds)
                {
                    var deliveryBoy = await GetByIdAsync(id);
                    if(deliveryBoy != null)
                    {
                        response.Add(deliveryBoy);
                    }
                }
                return response;
            }
            return new List<DeliveryBoySM>();
        }

        public async Task<IntResponseRoot> GetAllDeliveryBoysCount()
        {
            var count = await _apiDbContext.DeliveryBoy.CountAsync();
            return new IntResponseRoot(count, "Total Count of Delivery Boys");
            
        }

        public async Task<List<DeliveryBoySM>> GetAllByType(DeliveryBoyPaymentTypeSM type, int skip, int top)
        {
            var dmIds = await _apiDbContext.DeliveryBoy.Where(x=>x.PaymentType == (DeliveryBoyPaymentTypeDM)type).Select(x => x.Id).Skip(skip).Take(top).ToListAsync();
            if (dmIds.Count > 0)
            {
                var response = new List<DeliveryBoySM>();

                foreach (var id in dmIds)
                {
                    var deliveryBoy = await GetByIdAsync(id);
                    if (deliveryBoy != null)
                    {
                        response.Add(deliveryBoy);
                    }
                }
                return response;
            }
            return new List<DeliveryBoySM>();
        }

        public async Task<IntResponseRoot> GetAllDeliveryBoysByTypeCount(DeliveryBoyPaymentTypeSM type)
        {
            var count = await _apiDbContext.DeliveryBoy.Where(x => x.PaymentType == (DeliveryBoyPaymentTypeDM)type).CountAsync();
            return new IntResponseRoot(count, "Total Count of Delivery Boys");

        }

        public async Task<DeliveryBoySM?> GetByIdAsync(long id)
        {
            var dm = await _apiDbContext.DeliveryBoy
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id);
            string imagePath = null;
            if (dm != null)
            {
                var response = _mapper.Map<DeliveryBoySM>(dm);                
                response.Password = null;
                return response;
            }

            return null;
        }
        
        public async Task<DeliveryBoySM?> GetMineAccountDetails(long id)
        {
            var dm = await _apiDbContext.DeliveryBoy
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id);
            string imagePath = null;
            if (dm != null)
            {
                var activeResponse = await IsUserDeleted(dm);
                if(activeResponse.BoolResponse == false)
                {
                    throw new SiffrumException(ApiErrorTypeSM.InvalidToken_Log,
                        $"DeliveryBoy with Id: {dm.Id} is not active but tried to access details"
                        , activeResponse.ResponseMessage);
                }
                var response = _mapper.Map<DeliveryBoySM>(dm);                
                response.Password = null;

                // Enrich with seller name if seller-managed
                if (dm.SellerId.HasValue && dm.SellerId.Value > 0)
                {
                    var seller = await _apiDbContext.Seller
                        .AsNoTracking()
                        .FirstOrDefaultAsync(s => s.Id == dm.SellerId.Value);
                    response.SellerName = seller?.Name;
                }

                return response;
            }

            return null;
        }
        #endregion

        #region UPDATE
        public async Task<DeliveryBoySM?> UpdateAsync(long id, DeliveryBoySM objSM, bool IsSocialLogin = false, bool isUserUpdation = false)
        {
            if (objSM == null)
            {
                throw new SiffrumException(ApiErrorTypeSM.ModelError_NoLog, "Please provide details to update");
            }
            var dm = await _apiDbContext.DeliveryBoy
                .FirstOrDefaultAsync(x => x.Id == id);
            if (dm == null)
            {
                throw new SiffrumException(ApiErrorTypeSM.InvalidToken_Log, $"Delivery Boy tried to update but not found for the Id: {id}", "Delivery Boy not found.");
            }
            if (isUserUpdation)
            {
                var activeResponse = await IsUserDeleted(dm);
                if (activeResponse.BoolResponse == false)
                {
                    throw new SiffrumException(ApiErrorTypeSM.InvalidToken_Log,
                        $"Delivery Boy with Id: {dm.Id} is not active but tried to update details"
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
            string previousIdentityCard = null;
            string previousDriverLicense = null;
            if (objSM != null)
            {
                objSM.Id = id;
                if(isUserUpdation == true)
                {
                    objSM.Password = dm.Password;
                    objSM.RoleType = (RoleTypeSM)dm.RoleType;
                    objSM.Status = (DeliveryBoyStatusSM)dm.Status;
                    objSM.LoginStatus = (LoginStatusSM)dm.LoginStatus;
                    objSM.PaymentType = (DeliveryBoyPaymentTypeSM)dm.PaymentType;
                }
                else
                {
                    if (!string.IsNullOrEmpty(objSM.Password))
                    {
                        var passwordHash = await _passwordEncryptHelper.ProtectAsync<string>(objSM.Password);
                        objSM.Password = passwordHash;
                    }
                    else
                    {
                        objSM.Password = dm.Password;
                    }
                    objSM.RoleType = (RoleTypeSM)dm.RoleType;
                    objSM.Status = objSM.Status;
                    objSM.LoginStatus = objSM.LoginStatus;
                }
                objSM.AdminId = dm.AdminId;
                if(IsSocialLogin == true)
                {
                    objSM.IsEmailConfirmed = true;
                }
                else
                {
                    objSM.IsEmailConfirmed = dm.IsEmailConfirmed;
                }

                // DrivingLicense and NationalIdentityCard are now plain text fields (numbers), not base64 images
                // Only process photo fields if they contain new base64 data (not existing file paths)
                const string docPath = "content/deliveryboys/documents";

                if (!string.IsNullOrEmpty(objSM.DrivingLicensePhoto) && !objSM.DrivingLicensePhoto.Contains("/"))
                {
                    var path = await _imageProcess.SaveFromBase64(objSM.DrivingLicensePhoto, "jpg", docPath);
                    if (!string.IsNullOrEmpty(path)) objSM.DrivingLicensePhoto = path;
                }
                else
                {
                    objSM.DrivingLicensePhoto = dm.DrivingLicensePhoto;
                }

                if (!string.IsNullOrEmpty(objSM.AadhaarPhoto) && !objSM.AadhaarPhoto.Contains("/"))
                {
                    var path = await _imageProcess.SaveFromBase64(objSM.AadhaarPhoto, "jpg", docPath);
                    if (!string.IsNullOrEmpty(path)) objSM.AadhaarPhoto = path;
                }
                else
                {
                    objSM.AadhaarPhoto = dm.AadhaarPhoto;
                }

                if (!string.IsNullOrEmpty(objSM.PassportPhoto) && !objSM.PassportPhoto.Contains("/"))
                {
                    var path = await _imageProcess.SaveFromBase64(objSM.PassportPhoto, "jpg", docPath);
                    if (!string.IsNullOrEmpty(path)) objSM.PassportPhoto = path;
                }
                else
                {
                    objSM.PassportPhoto = dm.PassportPhoto;
                }

            }
        
            _mapper.Map(objSM, dm);

            dm.UpdatedAt = DateTime.UtcNow;
            dm.UpdatedBy = _loginUserDetail.LoginId;

            if (await _apiDbContext.SaveChangesAsync() > 0)
            {
                if (File.Exists(previousIdentityCard)) File.Delete(previousIdentityCard);
                if (File.Exists(previousDriverLicense)) File.Delete(previousDriverLicense);
                return await GetByIdAsync(id);
            }

            throw new SiffrumException(ApiErrorTypeSM.Fatal_Log, $"Error in updating delivery boy details with Id: {id}", "Something went wrong while updating profile details");
            
        }
        #endregion

        #region DELETE (SOFT DELETE)
        public async Task<DeleteResponseRoot> DeleteAsync(long id)
        {
            var dm = await _apiDbContext.DeliveryBoy
                .FirstOrDefaultAsync(x => x.Id == id);            
            if (dm == null)
            {
                throw new SiffrumException(ApiErrorTypeSM.InvalidInputData_Log, "User details not found");
            }
            var activeResponse = await IsUserDeleted(dm);
            if (activeResponse.BoolResponse == false)
            {
                throw new SiffrumException(ApiErrorTypeSM.InvalidToken_Log,
                    $"DeliveryBoy with Id: {dm.Id} is not active but tried to delete his account"
                    , activeResponse.ResponseMessage);
            }
            dm.Status = DeliveryBoyStatusDM.Removed;
            dm.UpdatedBy = _loginUserDetail.LoginId;
            var previousIdentityCard = dm.NationalIdentityCard;
            var previousDriverLicense = dm.DrivingLicense;
            if (await _apiDbContext.SaveChangesAsync() > 0) 
            {
                if (File.Exists(previousIdentityCard)) File.Delete(previousIdentityCard);
                if (File.Exists(previousDriverLicense)) File.Delete(previousDriverLicense);
                return new DeleteResponseRoot(true, "Your account deleted successfully");
            }
            throw new SiffrumException(ApiErrorTypeSM.Fatal_Log, $"Delivery Boy with Id:{id} is not getting deleted successfully", "Something went wrong while deleting your accunt, Please contact support team");
        }
        #endregion

        #region Admin - Assign to Seller

        public async Task<BoolResponseRoot> AssignToSeller(long deliveryBoyId, long sellerId)
        {
            var dm = await _apiDbContext.DeliveryBoy.FirstOrDefaultAsync(x => x.Id == deliveryBoyId);
            if (dm == null)
                throw new SiffrumException(ApiErrorTypeSM.InvalidInputData_Log, "Delivery boy not found");

            var seller = await _apiDbContext.Seller.AsNoTracking().FirstOrDefaultAsync(x => x.Id == sellerId);
            if (seller == null)
                throw new SiffrumException(ApiErrorTypeSM.InvalidInputData_Log, "Seller not found");

            dm.SellerId = sellerId;
            dm.UpdatedAt = DateTime.UtcNow;
            dm.UpdatedBy = _loginUserDetail.LoginId;

            if (await _apiDbContext.SaveChangesAsync() > 0)
                return new BoolResponseRoot(true, $"Delivery boy assigned to seller '{seller.StoreName ?? seller.Name}' successfully");

            throw new SiffrumException(ApiErrorTypeSM.Fatal_Log, "Failed to assign delivery boy to seller");
        }

        public async Task<BoolResponseRoot> UnassignFromSeller(long deliveryBoyId)
        {
            var dm = await _apiDbContext.DeliveryBoy.FirstOrDefaultAsync(x => x.Id == deliveryBoyId);
            if (dm == null)
                throw new SiffrumException(ApiErrorTypeSM.InvalidInputData_Log, "Delivery boy not found");

            dm.SellerId = null;
            dm.UpdatedAt = DateTime.UtcNow;
            dm.UpdatedBy = _loginUserDetail.LoginId;

            if (await _apiDbContext.SaveChangesAsync() > 0)
                return new BoolResponseRoot(true, "Delivery boy unassigned from seller");

            throw new SiffrumException(ApiErrorTypeSM.Fatal_Log, "Failed to unassign");
        }

        public async Task<BoolResponseRoot> ChangePaymentType(long deliveryBoyId, DeliveryBoyPaymentTypeSM paymentType)
        {
            var dm = await _apiDbContext.DeliveryBoy.FirstOrDefaultAsync(x => x.Id == deliveryBoyId);
            if (dm == null)
                throw new SiffrumException(ApiErrorTypeSM.InvalidInputData_Log, "Delivery boy not found");

            dm.PaymentType = (DeliveryBoyPaymentTypeDM)paymentType;
            dm.UpdatedAt = DateTime.UtcNow;
            dm.UpdatedBy = _loginUserDetail.LoginId;

            if (await _apiDbContext.SaveChangesAsync() > 0)
                return new BoolResponseRoot(true, $"Payment type changed to {paymentType}");

            throw new SiffrumException(ApiErrorTypeSM.Fatal_Log, "Failed to change payment type");
        }

        #endregion

        #region Toggle Availability (Online/Offline)

        public async Task<BoolResponseRoot> ToggleAvailability(long deliveryBoyId)
        {
            var dm = await _apiDbContext.DeliveryBoy.FirstOrDefaultAsync(x => x.Id == deliveryBoyId);
            if (dm == null)
                throw new SiffrumException(ApiErrorTypeSM.InvalidInputData_Log, "Delivery boy not found");

            dm.IsAvailable = dm.IsAvailable == 1 ? 0 : 1;
            dm.UpdatedAt = DateTime.UtcNow;

            if (await _apiDbContext.SaveChangesAsync() > 0)
                return new BoolResponseRoot(true, dm.IsAvailable == 1 ? "You are now Online. You will receive order notifications." : "You are now Offline. You will not receive order notifications.");

            throw new SiffrumException(ApiErrorTypeSM.Fatal_Log, "Failed to toggle availability");
        }

        public async Task<BoolResponseRoot> SetAvailability(long deliveryBoyId, bool isOnline)
        {
            var dm = await _apiDbContext.DeliveryBoy.FirstOrDefaultAsync(x => x.Id == deliveryBoyId);
            if (dm == null)
                throw new SiffrumException(ApiErrorTypeSM.InvalidInputData_Log, "Delivery boy not found");

            dm.IsAvailable = isOnline ? 1 : 0;
            dm.UpdatedAt = DateTime.UtcNow;

            if (await _apiDbContext.SaveChangesAsync() > 0)
                return new BoolResponseRoot(true, isOnline ? "You are now Online" : "You are now Offline");

            throw new SiffrumException(ApiErrorTypeSM.Fatal_Log, "Failed to set availability");
        }

        #endregion

        #region Delivery Boy Stats

        public async Task<DeliveryBoyStatsSM> GetDeliveryBoyStats(long deliveryBoyId)
        {
            var dm = await _apiDbContext.DeliveryBoy.AsNoTracking().FirstOrDefaultAsync(x => x.Id == deliveryBoyId);
            if (dm == null)
                throw new SiffrumException(ApiErrorTypeSM.InvalidInputData_Log, "Delivery boy not found");

            var deliveredCount = await _apiDbContext.Deliveries
                .AsNoTracking()
                .CountAsync(d => d.DeliveryBoyId == deliveryBoyId && d.Status == DomainModels.Enums.DeliveryStatusDM.Delivered);

            var assignedCount = await _apiDbContext.Deliveries
                .AsNoTracking()
                .CountAsync(d => d.DeliveryBoyId == deliveryBoyId && d.Status == DomainModels.Enums.DeliveryStatusDM.Assigned);

            var pickedUpCount = await _apiDbContext.Deliveries
                .AsNoTracking()
                .CountAsync(d => d.DeliveryBoyId == deliveryBoyId && d.Status == DomainModels.Enums.DeliveryStatusDM.PickedUp);

            var totalDeliveries = await _apiDbContext.Deliveries
                .AsNoTracking()
                .CountAsync(d => d.DeliveryBoyId == deliveryBoyId);

            var totalAmount = await _apiDbContext.Deliveries
                .AsNoTracking()
                .Where(d => d.DeliveryBoyId == deliveryBoyId && d.Status == DomainModels.Enums.DeliveryStatusDM.Delivered)
                .SumAsync(d => (decimal?)d.Amount) ?? 0;

            return new DeliveryBoyStatsSM
            {
                DeliveryBoyId = deliveryBoyId,
                DeliveryBoyName = dm.Name,
                TotalDeliveries = totalDeliveries,
                DeliveredCount = deliveredCount,
                AssignedCount = assignedCount,
                PickedUpCount = pickedUpCount,
                TotalDeliveredAmount = totalAmount,
                IsOnline = dm.IsAvailable == 1,
                PaymentType = ((DeliveryBoyPaymentTypeSM)dm.PaymentType).ToString(),
                SellerName = dm.SellerId.HasValue
                    ? (await _apiDbContext.Seller.AsNoTracking()
                        .Where(s => s.Id == dm.SellerId.Value)
                        .Select(s => s.Name)
                        .FirstOrDefaultAsync())
                    : null
            };
        }

        #endregion

        #region Email/LoginId checker

        public async Task<bool> GetDeliveryBoyByEmail(string email)
        {
            var existingDeliveryBoy = await _apiDbContext.DeliveryBoy.AsNoTracking().FirstOrDefaultAsync(x => x.Email == email);
            if(existingDeliveryBoy == null)
            {

                return true;
            }
            return false;
        }            
        
        public async Task<bool> GetDeliveryBoyByUsername(string username)
        {
            var existingDeliveryBoy = await _apiDbContext.DeliveryBoy.AsNoTracking().FirstOrDefaultAsync(x => x.Username == username);
            if(existingDeliveryBoy == null)
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
            var email = GetDeliveryBoyByEmail(objSM.Email);
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
                throw new SiffrumException(ApiErrorTypeSM.Fatal_Log, $"DeliveryBoy not found with email {objSM.Email}, tries to send email verification by logged in user with loginId {_loginUserDetail.LoginId}",$"DeliveryBoy not found with email '{objSM.Email}' found.");
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
                var user = await _apiDbContext.DeliveryBoy.Where(x => x.Email.ToUpper() == sm.Email.ToUpper()).FirstOrDefaultAsync();

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
                            $"DeliveryBoy with Id: {user.Id} is not active but tried to verify email"
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
                    throw new SiffrumException(ApiErrorTypeSM.Fatal_Log, $"No DeliveryBoy with email '{sm.Email}' found.", $"No DeliveryBoy with Email '{sm.Email}' found.");
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
            var user = await _apiDbContext.DeliveryBoy.AsNoTracking().Where(x=>x.Email == forgotPassword.UserName || x.Username == forgotPassword.UserName).FirstOrDefaultAsync();
            
            if(user != null)
            {
                var activeResponse = await IsUserDeleted(user);
                if (activeResponse.BoolResponse == false)
                {
                    throw new SiffrumException(ApiErrorTypeSM.InvalidToken_Log,
                        $"DeliveryBoy with Id: {user.Id} is not active but tried to send forgot password request"
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
                    throw new SiffrumException(ApiErrorTypeSM.Fatal_Log, $"No DeliveryBoy with username '{forgotPassword.UserName}' found.", $"No DeliveryBoy with username '{forgotPassword.UserName}' found.");
                }
                var user = (from c in _apiDbContext.DeliveryBoy
                            where c.Username.ToUpper() == forgotPassword.UserName.ToUpper()
                               || c.Email.ToUpper() == forgotPassword.UserName.ToUpper()
                            select new { DeliveryBoy = c }).FirstOrDefault();

                if (user != null)
                {
                    var activeResponse = await IsUserDeleted(user.DeliveryBoy);
                    if (activeResponse.BoolResponse == false)
                    {
                        throw new SiffrumException(ApiErrorTypeSM.InvalidToken_Log,
                            $"DeliveryBoy with Id: {user.DeliveryBoy.Id} is not active but tried update password"
                            , activeResponse.ResponseMessage);
                    }
                    string decrypt = "";
                    string newPassword = await _passwordEncryptHelper.ProtectAsync<string>(resetPasswordRequest.NewPassword);
                    if (string.Equals(user.DeliveryBoy.Password, newPassword))
                    {
                        throw new SiffrumException(ApiErrorTypeSM.Fatal_Log, $"Please don't use old password, use new one.", $"Please don't use old password, use new one.");
                    }
                    else
                    {
                        resetPasswordRequest.NewPassword = await _passwordEncryptHelper.ProtectAsync(resetPasswordRequest.NewPassword);
                        user.DeliveryBoy.Password = resetPasswordRequest.NewPassword;
                        await _apiDbContext.SaveChangesAsync();
                        return new BoolResponseRoot(true, "Password Updated Successfully");
                    }
                }
                else
                {
                    throw new SiffrumException(ApiErrorTypeSM.Fatal_Log, $"No DeliveryBoy with username '{forgotPassword.UserName}' found.", $"No DeliveryBoy with username '{forgotPassword.UserName}' found.");
                }
            }
            else
            {

                throw new SiffrumException(ApiErrorTypeSM.Fatal_Log, "Password can not be empty", "Password can not be empty");
            }
        }

        public async Task<BoolResponseRoot> ChangePassword(long DeliveryBoyId, UpdatePasswordRequestSM objSM)
        {
            if(string.IsNullOrEmpty(objSM.OldPassword) || string.IsNullOrEmpty(objSM.NewPassword))
            {
                throw new SiffrumException(ApiErrorTypeSM.InvalidInputData_NoLog, "Password is required, Please enter password.");
            }
            DeliveryBoyDM dm = await _apiDbContext.DeliveryBoy.FindAsync(DeliveryBoyId);
            if (dm != null) 
            {
                var activeResponse = await IsUserDeleted(dm);
                if (activeResponse.BoolResponse == false)
                {
                    throw new SiffrumException(ApiErrorTypeSM.InvalidToken_Log,
                        $"DeliveryBoy with Id: {dm.Id} is not active but tried to update password"
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
                        throw new SiffrumException(ApiErrorTypeSM.Fatal_Log, $"Password updation failed for DeliveryBoy with emailId: {dm.Email}",
                            "Something went wrong while updating your password, Please try again later");
                    }
                }
            }
            return new BoolResponseRoot(false, "DeliveryBoy details not found");
        }

        public async Task<BoolResponseRoot> SetPassord(long id, SetPasswordRequestSM objSM)
        {
            var dm = await _apiDbContext.DeliveryBoy.FindAsync(id);
            if (dm == null)
            {
                throw new SiffrumException(ApiErrorTypeSM.Fatal_Log, $"DeliveryBoy Not Found for Id:{id}",
                    "DeliveryBoy Not Found");
            }
            var activeResponse = await IsUserDeleted(dm);
            if (activeResponse.BoolResponse == false)
            {
                throw new SiffrumException(ApiErrorTypeSM.InvalidToken_Log,
                    $"DeliveryBoy with Id: {dm.Id} is not active but tried to set password"
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
            throw new SiffrumException(ApiErrorTypeSM.Fatal_Log, $"Set password has been failed for DeliveryBoyId: {id}, and Password: {objSM.Password}",
                   "Something went wrong while updating password");
        }

        #endregion Forgot/Update/Validate Password

        #region Handle Soft Deletion

        public async Task<BoolResponseRoot> IsUserDeleted(DeliveryBoyDM dm)
        {
            return dm.Status switch
            {
                DeliveryBoyStatusDM.Active =>
                    new BoolResponseRoot(true, "User is active"),

                DeliveryBoyStatusDM.Rejected =>
                    new BoolResponseRoot(false, "Your account has been rejected"),

                DeliveryBoyStatusDM.Deactivated =>
                    new BoolResponseRoot(false, "Your account is deactivated. Please contact support"),

                DeliveryBoyStatusDM.Removed =>
                    new BoolResponseRoot(false, "Your account has been removed"),

                DeliveryBoyStatusDM.Registered =>
                    new BoolResponseRoot(false, "Your account is registered but not active, contact support team"),

            };
        }
        #endregion Handle Soft Deletion

        #region Delivery Boy Pincodes

        public async Task<DeliveryBoyPincodesSM> AddDeliveryPincode(DeliveryBoyPincodesSM sm)
        {
            if(sm == null || string.IsNullOrEmpty(sm?.Pincode))
            {
                return null;
            }

            var exisitingPincode = await _apiDbContext.DeliveryBoyPincodes
                .Where(x=>x.DeliveryBoyId == sm.DeliveryBoyId && x.Pincode == sm.Pincode)
                .FirstOrDefaultAsync();
            if(exisitingPincode != null)
            {
                throw new SiffrumException(ApiErrorTypeSM.Fatal_Log, 
                    $"Delivery boy with id: {sm.DeliveryBoyId} has already associated pincode: {sm.Pincode} user tries to add same pincode",
                    "Pincode already associated with delivery boy");
            }
            var dm = _mapper.Map<DeliveryBoyPincodesDM>(sm);
            await _apiDbContext.DeliveryBoyPincodes.AddAsync(dm);
            if(await _apiDbContext.SaveChangesAsync() > 0)
            {
                return await GetDeliveryBoyPincodeByIdAsync(dm.Id);
            }
            throw new SiffrumException(ApiErrorTypeSM.Fatal_Log, 
                $"Something went wrong while associated pincode {sm.Pincode} to delivery boy with id: {sm.DeliveryBoyId}", 
                "Something went wrong while associating pincode to delivery boy");
        }

        public async Task<DeliveryBoyPincodesSM?> GetDeliveryBoyPincodeByIdAsync(long id)
        {
            var dm = await _apiDbContext.DeliveryBoyPincodes
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id);

            if (dm == null)
                return null;

            return _mapper.Map<DeliveryBoyPincodesSM>(dm);
        }

        public async Task<List<DeliveryBoyPincodesSM>> GetAllPincodesByDeliveryBoyIdAsync(long deliveryBoyId, int skip, int top)
        {
            var list = await _apiDbContext.DeliveryBoyPincodes
                .AsNoTracking()
                .Where(x => x.DeliveryBoyId == deliveryBoyId)
                .Skip(skip).Take(top)
                .ToListAsync();

            return _mapper.Map<List<DeliveryBoyPincodesSM>>(list);
        }

        public async Task<IntResponseRoot> GetAllPincodesByDeliveryBoyIdAsyncCount(long deliveryBoyId)
        {
            var count = await _apiDbContext.DeliveryBoyPincodes
                .AsNoTracking()
                .Where(x => x.DeliveryBoyId == deliveryBoyId)
                .CountAsync();

            return new IntResponseRoot(count, "Total Count of delivery boy pincodes");
        }

        public async Task<DeliveryBoyPincodesSM?> UpdateDeliveryBoyPincodeAsync(long id, DeliveryBoyPincodesSM sm)
        {
            var dm = await _apiDbContext.DeliveryBoyPincodes
                .FirstOrDefaultAsync(x => x.Id == id);

            if (dm == null)
                throw new SiffrumException(ApiErrorTypeSM.InvalidInputData_NoLog, "Pincode association not found");

            var exists = await _apiDbContext.DeliveryBoyPincodes
                .AnyAsync(x => x.DeliveryBoyId == sm.DeliveryBoyId
                            && x.Pincode == sm.Pincode
                            && x.Id != id);

            if (exists)
                throw new SiffrumException(ApiErrorTypeSM.InvalidInputData_NoLog,
                    "This pincode is already associated with the delivery boy");
           
            dm.Pincode = sm.Pincode;

            if (await _apiDbContext.SaveChangesAsync() > 0)
                return await GetDeliveryBoyPincodeByIdAsync(id);

            throw new SiffrumException(ApiErrorTypeSM.Fatal_Log,
                $"Failed to update pincode association with id {id}",
                "Failed to update pincode");
        }

        public async Task<DeleteResponseRoot> DeleteDeliveryBoyPincodeAsync(long id)
        {
            var dm = await _apiDbContext.DeliveryBoyPincodes
                .FirstOrDefaultAsync(x => x.Id == id);

            if (dm == null)
                throw new SiffrumException(ApiErrorTypeSM.InvalidInputData_NoLog, "Pincode association not found");

            _apiDbContext.DeliveryBoyPincodes.Remove(dm);

            if(await _apiDbContext.SaveChangesAsync() > 0)
            {
                return new DeleteResponseRoot(true, "Pincode associated with delivery boy deleted successfully");
            }
            throw new SiffrumException(ApiErrorTypeSM.Fatal_Log, $"Error in deleting delivery boy pincode with id: {id}",
                "SOmething went wrong while deleting delivery boy pincode association");
        }

        public async Task<List<DeliveryBoySM>> GetDeliveryBoyByPincodeAsync(string pincode, int skip, int top)
        {
            var deliveryBoyIds = await _apiDbContext.DeliveryBoyPincodes
                .AsNoTracking()
                .Where(x => x.Pincode == pincode)
                .Select(x => x.DeliveryBoyId)
                .Distinct()
                .Skip(skip).Take(top)
                .ToListAsync();
            var response = new List<DeliveryBoySM>();
            if(deliveryBoyIds.Count == 0)
            {
                return response;
            }
            foreach(var id in deliveryBoyIds)
            {
                var res = await GetByIdAsync(id);
                if(res != null)
                {
                    response.Add(res);
                }
            }
            return response;
        }

        public async Task<IntResponseRoot> GetDeliveryBoyByPincodeAsyncCount(string pincode)
        {
            var count = await _apiDbContext.DeliveryBoyPincodes
                .AsNoTracking()
                .Where(x => x.Pincode == pincode)
                .Select(x => x.DeliveryBoyId)
                .Distinct()
                .CountAsync();
            return new IntResponseRoot(count, "Total delivery boys");
        }

        #endregion Delivery Boy Pincodes

        #region Search

        public async Task<List<SearchResponseSM>> SearchDeliveryBoy(
          string searchText)
        {
            IQueryable<DeliveryBoyDM> query = _apiDbContext.DeliveryBoy
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
        
        public async Task<List<SearchResponseSM>> SearchDeliveryBoyByType(DeliveryBoyPaymentTypeSM? deliveryBoyType,
          string searchText)
        {
            IQueryable<DeliveryBoyDM> query = _apiDbContext.DeliveryBoy
                .AsNoTracking();
            if (deliveryBoyType.HasValue)
            {
                query = query.Where(x => x.PaymentType == (DeliveryBoyPaymentTypeDM)deliveryBoyType.Value);
            }
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

        #endregion Search
    }
}
