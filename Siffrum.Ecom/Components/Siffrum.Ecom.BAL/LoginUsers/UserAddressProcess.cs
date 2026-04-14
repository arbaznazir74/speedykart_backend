using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Siffrum.Ecom.BAL.ExceptionHandler;
using Siffrum.Ecom.BAL.Foundation.Base;
using Siffrum.Ecom.DAL.Context;
using Siffrum.Ecom.DomainModels.v1;
using Siffrum.Ecom.ServiceModels.Foundation.Base.CommonResponseRoot;
using Siffrum.Ecom.ServiceModels.Foundation.Base.Enums;
using Siffrum.Ecom.ServiceModels.Foundation.Base.Interfaces;
using Siffrum.Ecom.ServiceModels.v1;

namespace Siffrum.Ecom.BAL.LoginUsers
{
    public class UserAddressProcess : SiffrumBalOdataBase<UserAddressSM>
    {
        private readonly ILoginUserDetail _loginUserDetail;

        public UserAddressProcess(
            IMapper mapper,
            ApiDbContext apiDbContext,
            ILoginUserDetail loginUserDetail)
            : base(mapper, apiDbContext)
        {
            _loginUserDetail = loginUserDetail;
        }

        #region ODATA
        public override async Task<IQueryable<UserAddressSM>> GetServiceModelEntitiesForOdata()
        {
            var entitySet = _apiDbContext.UserAddress.AsNoTracking();

            return await base.MapEntityAsToQuerable<UserAddressDM, UserAddressSM>(
                _mapper, entitySet);
        }
        #endregion

        #region CREATE
        public async Task<UserAddressSM> CreateAsync(long userId, UserAddressSM objSM)
        {
            if (objSM == null)
                throw new SiffrumException(ApiErrorTypeSM.ModelError_NoLog, "Address data required");
            if (!objSM.Latitude.HasValue || !objSM.Longitude.HasValue)
            {
                throw new SiffrumException(
                    ApiErrorTypeSM.InvalidInputData_NoLog,
                    "Please provide latitude and longitude");
            }
            var dm = _mapper.Map<UserAddressDM>(objSM);
            
            dm.UserId = userId;
            if (objSM.IsDefault == true)
            {                
                await ResetDefaultAddress(userId);
            }
                

            dm.CreatedAt = DateTime.UtcNow;
            dm.CreatedBy = _loginUserDetail.LoginId;

            await _apiDbContext.UserAddress.AddAsync(dm);
            await _apiDbContext.SaveChangesAsync();

            return await GetByIdAsync(userId, dm.Id);
        }
        #endregion

        #region READ

        public async Task<UserAddressSM?> GetByIdAsync(long userId, long id)
        {
            var dm = await _apiDbContext.UserAddress
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId);

            if (dm == null) return null;

            return _mapper.Map<UserAddressSM>(dm);
        }

        public async Task<UserAddressSM?> GetById( long id)
        {
            var dm = await _apiDbContext.UserAddress
                .FindAsync(id);

            if (dm == null) return null;

            return _mapper.Map<UserAddressSM>(dm);
        }

        public async Task<List<UserAddressSM>> GetAllAddresses(int skip, int top)
        {
            var dms = await _apiDbContext.UserAddress
                .AsNoTracking()
                .OrderByDescending(x => x.UserId)
                .ThenByDescending(x => x.IsDefault)
                .Skip(skip)
                .Take(top)
                .ToListAsync();

            return _mapper.Map<List<UserAddressSM>>(dms);
        }

        public async Task<UserAddressSM> GetDefaultAddress(long userId)
        {
            var dm = await _apiDbContext.UserAddress
                .AsNoTracking()
                .Where(x=>x.UserId == userId && x.IsDefault == true)                
                .FirstOrDefaultAsync();
            if(dm == null)
            {
                return null;
            }
            return _mapper.Map<UserAddressSM>(dm);
        }

        public async Task<IntResponseRoot> GetAllAddressesCount()
        {
            var count = await _apiDbContext.UserAddress
                .CountAsync();

            return new IntResponseRoot(count, "Total addresses");
        }

        public async Task<List<UserAddressSM>> GetAllMine(long userId, int skip, int top)
        {
            var ids = await _apiDbContext.UserAddress
                .AsNoTracking()
                .Where(x => x.UserId == userId)
                .OrderByDescending(x => x.IsDefault)
                .ThenByDescending(x => x.Id)
                .Skip(skip)
                .Take(top)
                .Select(x => x.Id)
                .ToListAsync();

            var list = new List<UserAddressSM>();

            foreach (var id in ids)
            {
                var item = await GetByIdAsync(userId, id);
                if (item != null)
                    list.Add(item);
            }

            return list;
        }

        public async Task<IntResponseRoot> GetAllMineCount(long userId)
        {
            var count = await _apiDbContext.UserAddress
                .CountAsync(x => x.UserId == userId);

            return new IntResponseRoot(count, "Total addresses");
        }

        #endregion

        #region UPDATE
        public async Task<UserAddressSM?> UpdateAsync(long userId, long id, UserAddressSM objSM)
        {
            var dm = await _apiDbContext.UserAddress
                .FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId);

            if (dm == null)
                throw new SiffrumException(ApiErrorTypeSM.InvalidInputData_NoLog, "Address not found");
            objSM.Id = id;
            objSM.UserId = dm.UserId;
            _mapper.Map(objSM, dm);

            if (objSM.IsDefault == true)
                await ResetDefaultAddress(userId);
            dm.UpdatedAt = DateTime.UtcNow;
            dm.UpdatedBy = _loginUserDetail.LoginId;

            await _apiDbContext.SaveChangesAsync();

            return await GetByIdAsync(userId, id);
        }
        #endregion

        #region DELETE
        public async Task<DeleteResponseRoot> DeleteMineAsync(long userId, long id)
        {
            var dm = await _apiDbContext.UserAddress
                .FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId);

            if (dm == null)
                throw new SiffrumException(ApiErrorTypeSM.InvalidInputData_Log, "Address not found");

            _apiDbContext.UserAddress.Remove(dm);
            await _apiDbContext.SaveChangesAsync();

            return new DeleteResponseRoot(true, "Address deleted successfully");
        }
        public async Task<DeleteResponseRoot> DeleteAsync(long id)
        {
            var dm = await _apiDbContext.UserAddress
                .FindAsync(id);

            if (dm == null)
                throw new SiffrumException(ApiErrorTypeSM.InvalidInputData_Log, "Address not found");

            _apiDbContext.UserAddress.Remove(dm);
            await _apiDbContext.SaveChangesAsync();

            return new DeleteResponseRoot(true, "Address deleted successfully");
        }
        #endregion

        #region HELPERS
        private async Task ResetDefaultAddress(long userId)
        {
            var list = await _apiDbContext.UserAddress
                .Where(x => x.UserId == userId && x.IsDefault == true)
                .ToListAsync();

            foreach (var item in list)
                item.IsDefault = false;
        }
        #endregion
    }
}
