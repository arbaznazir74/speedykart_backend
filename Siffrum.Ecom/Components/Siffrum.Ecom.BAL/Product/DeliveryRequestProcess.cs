using AutoMapper;
using Google.Api.Gax;
using Microsoft.EntityFrameworkCore;
using Siffrum.Ecom.BAL.ExceptionHandler;
using Siffrum.Ecom.BAL.Foundation.Base;
using Siffrum.Ecom.DAL.Context;
using Siffrum.Ecom.DomainModels.Enums;
using Siffrum.Ecom.DomainModels.v1;
using Siffrum.Ecom.ServiceModels.Enums;
using Siffrum.Ecom.ServiceModels.Foundation.Base.CommonResponseRoot;
using Siffrum.Ecom.ServiceModels.Foundation.Base.Enums;
using Siffrum.Ecom.ServiceModels.Foundation.Base.Interfaces;
using Siffrum.Ecom.ServiceModels.v1;

namespace Siffrum.Ecom.BAL.Delivery
{
    public class DeliveryRequestProcess : SiffrumBalOdataBase<DeliveryRequestSM>
    {
        private readonly ILoginUserDetail _loginUserDetail;

        public DeliveryRequestProcess(
            IMapper mapper,
            ApiDbContext apiDbContext,
            ILoginUserDetail loginUserDetail)
            : base(mapper, apiDbContext)
        {
            _loginUserDetail = loginUserDetail;
        }

        #region OData
        public override async Task<IQueryable<DeliveryRequestSM>> GetServiceModelEntitiesForOdata()
        {
            IQueryable<DeliveryRequestDM> entitySet = _apiDbContext.DeliveryRequest
                .AsNoTracking();

            return await base.MapEntityAsToQuerable<DeliveryRequestDM, DeliveryRequestSM>(_mapper, entitySet);
        }
        #endregion

        #region CREATE
        public async Task<BoolResponseRoot> CreateAsync(DeliveryRequestSM objSM)
        {
            if (objSM == null)
                return new BoolResponseRoot(false, "Request data is required");

            if (string.IsNullOrWhiteSpace(objSM.Pincode))
                return new BoolResponseRoot(false, "Pincode is required");

            if (objSM.Latitude == 0 || objSM.Longitude == 0)
                return new BoolResponseRoot(false, "Please provide a valid location");

            // ✅ Check existing requests for this pincode
            var existingRequests = await _apiDbContext.DeliveryRequest
                .AsNoTracking()
                .Where(x => x.Pincode == objSM.Pincode)
                .ToListAsync();

            if (existingRequests.Any())
            {
                // 🔹 Same user cases
                var userRequest = existingRequests
                    .FirstOrDefault(x => x.UserId == objSM.UserId);

                if (userRequest != null)
                {
                    if (!userRequest.IsResolved)
                    {
                        return new BoolResponseRoot(false,
                            "You have already requested delivery for this location. Our team will review it soon.");
                    }

                    return new BoolResponseRoot(false,
                        "Your request for this location has already been reviewed.");
                }

                // 🔹 Other users already requested
                var hasPending = existingRequests.Any(x => !x.IsResolved);

                if (hasPending)
                {
                    return new BoolResponseRoot(true,
                        "A request for this location is already under review. We will notify you once it is available.");
                }

                return new BoolResponseRoot(true,
                    "This location has already been reviewed. We will update availability if service is enabled.");
            }

            // ✅ Create new request
            var dm = _mapper.Map<DeliveryRequestDM>(objSM);
            dm.CreatedAt = DateTime.UtcNow;
            dm.CreatedBy = _loginUserDetail.LoginId;
            dm.IsResolved = false;
            dm.AdminRemarks = "";

            await _apiDbContext.DeliveryRequest.AddAsync(dm);

            var result = await _apiDbContext.SaveChangesAsync();

            if (result > 0)
            {
                return new BoolResponseRoot(true,
                    "Your delivery request has been submitted successfully. We'll notify you once it's available in your area.");
            }

            return new BoolResponseRoot(false,
                "Something went wrong while submitting your request. Please try again.");
        }
        #endregion

        #region READ

        public async Task<DeliveryRequestSM?> GetByIdAsync(long id)
        {
            var dm = await _apiDbContext.DeliveryRequest
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id);

            return dm != null ? _mapper.Map<DeliveryRequestSM>(dm) : null;
        }

        public async Task<List<DeliveryRequestSM>> GetAll(int skip, int top)
        {
            var list = await _apiDbContext.DeliveryRequest
                .AsNoTracking()
                .OrderByDescending(x => x.CreatedAt)
                .Skip(skip)
                .Take(top)
                .ToListAsync();

            return _mapper.Map<List<DeliveryRequestSM>>(list);
        }
        public async Task<IntResponseRoot> GetAllCount()
        {
            var count = await _apiDbContext.DeliveryRequest.AsNoTracking().CountAsync();
            return new IntResponseRoot(count, "Total delivery requests");
        }

        public async Task<List<DeliveryRequestSM>> GetAllByPlatform(PlatformTypeSM platformType, int skip, int top)
        {
            var list = await _apiDbContext.DeliveryRequest
                .AsNoTracking()
                .Where(x=>x.Platform == (PlatformTypeDM) platformType)
                .OrderByDescending(x => x.CreatedAt)
                .Skip(skip)
                .Take(top)
                .ToListAsync();

            return _mapper.Map<List<DeliveryRequestSM>>(list);
        }
        public async Task<IntResponseRoot> GetAllByPlatformTypeCount(PlatformTypeSM platformType)
        {
            var count = await _apiDbContext.DeliveryRequest.AsNoTracking()
                .Where(x => x.Platform == (PlatformTypeDM)platformType)
                .CountAsync();
            return new IntResponseRoot(count, "Total delivery requests");
        }

        public async Task<List<DeliveryRequestSM>> GetByUser(long userId, int skip, int top)
        {
            var list = await _apiDbContext.DeliveryRequest
                .AsNoTracking()
                .Where(x => x.UserId == userId)
                .OrderByDescending(x => x.CreatedAt)
                .Skip(skip)
                .Take(top)
                .ToListAsync();

            return _mapper.Map<List<DeliveryRequestSM>>(list);
        }

        public async Task<IntResponseRoot> GetByUserCount(long userId)
        {
            var count = await _apiDbContext.DeliveryRequest
                .AsNoTracking()
                .Where(x => x.UserId == userId)
                .OrderByDescending(x => x.CreatedAt)
                .CountAsync();
            return new IntResponseRoot(count, "Total delivery requests");
        }

        #endregion

        #region UPDATE (Admin Resolve)

        public async Task<DeliveryRequestSM?> ResolveRequest(long id, string remarks, bool isApproved)
        {
            var dm = await _apiDbContext.DeliveryRequest
                .FirstOrDefaultAsync(x => x.Id == id);

            if (dm == null)
                throw new SiffrumException(
                    ApiErrorTypeSM.InvalidInputData_NoLog,
                    "Request not found"
                );

            if (dm.IsResolved)
                throw new SiffrumException(
                    ApiErrorTypeSM.InvalidInputData_NoLog,
                    "Request already resolved"
                );

            dm.IsResolved = true;
            dm.AdminRemarks = remarks;
            dm.UpdatedAt = DateTime.UtcNow;
            dm.UpdatedBy = _loginUserDetail.LoginId;

            await _apiDbContext.SaveChangesAsync();

            // 👉 Optional: If approved → add to DeliveryPlaces
            if (isApproved)
            {
                var deliveryPlace = new DeliveryPlacesDM
                {
                    SellerId = 0, // or assign logic
                    Pincode = dm.Pincode
                };

                await _apiDbContext.DeliveryPlaces.AddAsync(deliveryPlace);
                await _apiDbContext.SaveChangesAsync();
            }

            return _mapper.Map<DeliveryRequestSM>(dm);
        }

        #endregion

        #region DELETE

        public async Task<DeleteResponseRoot> DeleteAsync(long id)
        {
            var dm = await _apiDbContext.DeliveryRequest
                .FirstOrDefaultAsync(x => x.Id == id);

            if (dm == null)
                throw new SiffrumException(
                    ApiErrorTypeSM.InvalidInputData_Log,
                    "Request not found"
                );

            _apiDbContext.DeliveryRequest.Remove(dm);

            await _apiDbContext.SaveChangesAsync();

            return new DeleteResponseRoot(true, "Request deleted successfully");
        }

        #endregion

        #region SEARCH

        

        #endregion
    }
}