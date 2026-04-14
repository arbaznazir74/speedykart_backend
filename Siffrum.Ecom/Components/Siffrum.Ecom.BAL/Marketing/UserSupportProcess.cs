using AutoMapper;
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

namespace Siffrum.Ecom.BAL.Marketing
{
    public class UserSupportProcess : SiffrumBalOdataBase<UserSupportRequestSM>
    {
        private readonly ILoginUserDetail _loginUserDetail;

        public UserSupportProcess(
            IMapper mapper,
            ApiDbContext apiDbContext,
            ILoginUserDetail loginUserDetail)
            : base(mapper, apiDbContext)
        {
            _loginUserDetail = loginUserDetail;
        }

        #region ODATA
        public override async Task<IQueryable<UserSupportRequestSM>> GetServiceModelEntitiesForOdata()
        {
            var query = _apiDbContext.UserSupportRequest.AsNoTracking();
            return await base.MapEntityAsToQuerable<UserSupportRequestDM, UserSupportRequestSM>(_mapper, query);
        }
        #endregion

        #region CREATE
        public async Task<BoolResponseRoot> CreateAsync(UserSupportRequestSM objSM)
        {
            if (objSM == null)
                return new BoolResponseRoot(false, "Request data is required");

            if (string.IsNullOrWhiteSpace(objSM.Subject))
                return new BoolResponseRoot(false, "Subject is required");

            if (string.IsNullOrWhiteSpace(objSM.Message))
                return new BoolResponseRoot(false, "Message is required");

            var dm = _mapper.Map<UserSupportRequestDM>(objSM);

            dm.CreatedAt = DateTime.UtcNow;
            dm.CreatedBy = _loginUserDetail.LoginId;
            dm.IsResolved = false;

            await _apiDbContext.UserSupportRequest.AddAsync(dm);

            var result = await _apiDbContext.SaveChangesAsync();

            if (result > 0)
            {
                return new BoolResponseRoot(true,
                    "Your request has been submitted successfully. Our support team will contact you soon.");
            }

            return new BoolResponseRoot(false,
                "Failed to submit your request. Please try again.");
        }
        #endregion

        #region READ

        public async Task<UserSupportRequestSM?> GetByIdAsync(long id)
        {
            var dm = await _apiDbContext.UserSupportRequest
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id);

            return dm != null ? _mapper.Map<UserSupportRequestSM>(dm) : null;
        }

        public async Task<List<UserSupportRequestSM>> GetAll(int skip, int top)
        {
            var list = await _apiDbContext.UserSupportRequest
                .AsNoTracking()
                .OrderByDescending(x => x.CreatedAt)
                .Skip(skip)
                .Take(top)
                .ToListAsync();

            return _mapper.Map<List<UserSupportRequestSM>>(list);
        }

        public async Task<IntResponseRoot> GetAllCount()
        {
            var count = await _apiDbContext.UserSupportRequest
                .AsNoTracking()
                .CountAsync();

            return new IntResponseRoot(count, "Total support requests");
        }

        public async Task<List<UserSupportRequestSM>> GetByUser(long userId, int skip, int top)
        {
            var list = await _apiDbContext.UserSupportRequest
                .AsNoTracking()
                .Where(x => x.UserId == userId)
                .OrderByDescending(x => x.CreatedAt)
                .Skip(skip)
                .Take(top)
                .ToListAsync();

            return _mapper.Map<List<UserSupportRequestSM>>(list);
        }

        public async Task<IntResponseRoot> GetByUserCount(long userId)
        {
            var count = await _apiDbContext.UserSupportRequest
                .AsNoTracking()
                .Where(x => x.UserId == userId)
                .CountAsync();

            return new IntResponseRoot(count, "Total support requests");
        }

        #endregion

        #region UPDATE (Admin Resolve)

        public async Task<UserSupportRequestSM?> ResolveRequest(long id, string adminResponse)
        {
            var dm = await _apiDbContext.UserSupportRequest
                .FirstOrDefaultAsync(x => x.Id == id);

            if (dm == null)
                throw new SiffrumException(ApiErrorTypeSM.InvalidInputData_NoLog, "Request not found");

            if (dm.IsResolved)
                throw new SiffrumException(ApiErrorTypeSM.InvalidInputData_NoLog, "Request already resolved");

            dm.AdminResponse = adminResponse;
            dm.IsResolved = true;
            dm.UpdatedAt = DateTime.UtcNow;
            dm.UpdatedBy = _loginUserDetail.LoginId;

            await _apiDbContext.SaveChangesAsync();

            return _mapper.Map<UserSupportRequestSM>(dm);
        }

        #endregion

        #region DELETE

        public async Task<DeleteResponseRoot> DeleteAsync(long id)
        {
            var dm = await _apiDbContext.UserSupportRequest
                .FirstOrDefaultAsync(x => x.Id == id);

            if (dm == null)
                throw new SiffrumException(ApiErrorTypeSM.InvalidInputData_Log, "Request not found");

            _apiDbContext.UserSupportRequest.Remove(dm);

            await _apiDbContext.SaveChangesAsync();

            return new DeleteResponseRoot(true, "Request deleted successfully");
        }

        #endregion
    }

}
