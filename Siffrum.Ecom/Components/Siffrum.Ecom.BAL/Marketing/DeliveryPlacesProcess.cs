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
    public class DeliveryPlacesProcess : SiffrumBalOdataBase<DeliveryPlacesSM>
    {
        private readonly ILoginUserDetail _loginUserDetail;

        public DeliveryPlacesProcess( ApiDbContext apiDbContext, IMapper mapper,ILoginUserDetail loginUserDetail)
            : base(mapper, apiDbContext)
        {
            _loginUserDetail = loginUserDetail;
        }

        #region ODATA
        public override async Task<IQueryable<DeliveryPlacesSM>> GetServiceModelEntitiesForOdata()
        {
            var entitySet = _apiDbContext.DeliveryPlaces.AsNoTracking();

            return await base.MapEntityAsToQuerable<
                DeliveryPlacesDM,
                DeliveryPlacesSM>(_mapper, entitySet);
        }
        #endregion

        #region CREATE
        public async Task<BoolResponseRoot> CreateAsync(DeliveryPlacesSM objSM)
        {
            if (objSM == null)
                throw new SiffrumException(
                    ApiErrorTypeSM.ModelError_NoLog,
                    "Delivery place data is required"
                );

            var exists = await _apiDbContext.DeliveryPlaces
                .Where(x => x.Pincode.ToLower() == objSM.Pincode.ToLower()).FirstOrDefaultAsync();

            if (exists != null)
                throw new SiffrumException(
                    ApiErrorTypeSM.InvalidInputData_NoLog,
                   $"Place with PINCODE already exists with seller Id: {exists.SellerId}"
                );

            var dm = _mapper.Map<DeliveryPlacesDM>(objSM);

            dm.CreatedAt = DateTime.UtcNow;
            dm.CreatedBy = _loginUserDetail.LoginId;

            await _apiDbContext.DeliveryPlaces.AddAsync(dm);

            if (await _apiDbContext.SaveChangesAsync() > 0)
                return new BoolResponseRoot(true, "Place created successfully");

            throw new SiffrumException(
                ApiErrorTypeSM.Fatal_Log,
                "Failed to create add new place"
            );
        }
        #endregion

        #region READ
        public async Task<DeliveryPlacesSM?> GetByIdAsync(long id)
        {
            var dm = await _apiDbContext.DeliveryPlaces.FindAsync(id);
            if (dm == null) return null;

            var sm = _mapper.Map<DeliveryPlacesSM>(dm);
            return sm;
        }

        public async Task<List<DeliveryPlacesSM>> GetAll(int skip, int top)
        {
            var ids = await _apiDbContext.DeliveryPlaces
                .AsNoTracking()
                .Skip(skip)
                .Take(top)
                .Select(x => x.Id)
                .ToListAsync();

            var response = new List<DeliveryPlacesSM>();
            foreach (var id in ids)
            {
                var place = await GetByIdAsync(id);
                if (place != null)
                    response.Add(place);
            }
            return response;
        }
        public async Task<IntResponseRoot> GetCount()
        {
            var count = await _apiDbContext.DeliveryPlaces.CountAsync();
            return new IntResponseRoot(count, "Total Places");
        }
        public async Task<List<DeliveryPlacesSM>> GetAllForUser(int skip, int top)
        {
            var ids = await _apiDbContext.DeliveryPlaces
                .AsNoTracking()
                .Where(x=>x.Status == StatusDM.Active)
                .Skip(skip)
                .Take(top)
                .Select(x => x.Id)
                .ToListAsync();

            var response = new List<DeliveryPlacesSM>();
            foreach (var id in ids)
            {
                var place = await GetByIdAsync(id);
                if (place != null)
                    response.Add(place);
            }
            return response;
        }

        public async Task<IntResponseRoot> GetCountForUser()
        {
            var count = await _apiDbContext.DeliveryPlaces
                .Where(x => x.Status == StatusDM.Active)
                .CountAsync();
            return new IntResponseRoot(count, "Total Places");
        }

        public async Task<List<DeliveryPlacesSM>> GetAllSellerPlaces(long sellerId, int skip, int top)
        {
            var ids = await _apiDbContext.DeliveryPlaces
                .AsNoTracking()
                .Where(x=>x.SellerId == sellerId)
                .Skip(skip)
                .Take(top)
                .Select(x => x.Id)
                .ToListAsync();

            var response = new List<DeliveryPlacesSM>();
            foreach (var id in ids)
            {
                var place = await GetByIdAsync(id);
                if (place != null)
                    response.Add(place);
            }
            return response;
        }

        public async Task<IntResponseRoot> GetSellerPlacesCount(long sellerId)
        {
            var count = await _apiDbContext.DeliveryPlaces
                .Where(x => x.SellerId == sellerId)
                .CountAsync();
            return new IntResponseRoot(count, "Total Places");
        }

        #endregion

        #region UPDATE
        public async Task<DeliveryPlacesSM?> UpdateAsync(long id, DeliveryPlacesSM objSM)
        {
            var dm = await _apiDbContext.DeliveryPlaces
                .FirstOrDefaultAsync(x => x.Id == id);

            if (dm == null)
                throw new SiffrumException(
                    ApiErrorTypeSM.InvalidInputData_NoLog,
                    "Place not found"
                );
            var existingPlaceWithPincode = await _apiDbContext.DeliveryPlaces.Where(x=>x.Pincode.ToLower() == objSM.Pincode.ToLower() && x.Id != id).FirstOrDefaultAsync();
            if (existingPlaceWithPincode != null)
                throw new SiffrumException(
                    ApiErrorTypeSM.InvalidInputData_NoLog,
                    "Place with PINCODE already exists"
                );
            _mapper.Map(objSM, dm);
            dm.Id = id;
            dm.UpdatedAt = DateTime.UtcNow;
            dm.UpdatedBy = _loginUserDetail.LoginId;

            if (await _apiDbContext.SaveChangesAsync() > 0)
            {    
                return await GetByIdAsync(id);
            }

            throw new SiffrumException(
                ApiErrorTypeSM.Fatal_Log,
                "Failed to update place details"
            );
        }

        public async Task<DeliveryPlacesSM?> UpdateMineAsync(long sellerId, long id, DeliveryPlacesSM objSM)
        {
            var dm = await _apiDbContext.DeliveryPlaces
                .FirstOrDefaultAsync(x => x.Id == id && x.SellerId == sellerId);

            if (dm == null)
                throw new SiffrumException(
                    ApiErrorTypeSM.InvalidInputData_NoLog,
                    "Place not found"
                );
            var existingPlaceWithPincode = await _apiDbContext.DeliveryPlaces.Where(x => x.Pincode.ToLower() == objSM.Pincode.ToLower() && x.Id != id).FirstOrDefaultAsync();
            if (existingPlaceWithPincode != null)
                throw new SiffrumException(
                    ApiErrorTypeSM.InvalidInputData_NoLog,
                    "Place with PINCODE already exists"
                );
            _mapper.Map(objSM, dm);
            dm.Id = id;
            dm.SellerId = sellerId;
            dm.UpdatedAt = DateTime.UtcNow;
            dm.UpdatedBy = _loginUserDetail.LoginId;

            if (await _apiDbContext.SaveChangesAsync() > 0)
            {
                return await GetByIdAsync(id);
            }

            throw new SiffrumException(
                ApiErrorTypeSM.Fatal_Log,
                "Failed to update place details"
            );
        }

        public async Task<BoolResponseRoot?> UpdateStatusAsync(long id, StatusSM status)
        {
            var dm = await _apiDbContext.DeliveryPlaces
                .FirstOrDefaultAsync(x => x.Id == id);

            if (dm == null)
                throw new SiffrumException(
                    ApiErrorTypeSM.InvalidInputData_NoLog,
                    "Place not found"
                );
            if(dm.Status == (StatusDM)status)
            {
                return new BoolResponseRoot(false, "Place status already updated");
            }
            dm.Status = (StatusDM)status;
            if (await _apiDbContext.SaveChangesAsync() > 0)
            {
                return new BoolResponseRoot(true, "Place status updated successfully");
            }

            throw new SiffrumException(
                ApiErrorTypeSM.Fatal_Log,
                "Failed to update place details"
            );
        }

        public async Task<BoolResponseRoot?> UpdateMineStatusAsync(long sellerId,long id, StatusSM status)
        {
            var dm = await _apiDbContext.DeliveryPlaces
                .FirstOrDefaultAsync(x => x.Id == id && x.SellerId == sellerId);

            if (dm == null)
                throw new SiffrumException(
                    ApiErrorTypeSM.InvalidInputData_NoLog,
                    "Place not found"
                );
            if (dm.Status == (StatusDM)status)
            {
                return new BoolResponseRoot(false, "Place status already updated");
            }
            dm.Status = (StatusDM)status;
            if (await _apiDbContext.SaveChangesAsync() > 0)
            {
                return new BoolResponseRoot(true, "Place status updated successfully");
            }

            throw new SiffrumException(
                ApiErrorTypeSM.Fatal_Log,
                "Failed to update place details"
            );
        }
        #endregion

        #region DELETE
        public async Task<DeleteResponseRoot> DeleteAsync(long id)
        {
            var dm = await _apiDbContext.DeliveryPlaces
                .FirstOrDefaultAsync(x => x.Id == id);

            if (dm == null)
                throw new SiffrumException(
                    ApiErrorTypeSM.InvalidInputData_Log,
                    "Place not found"
                );


            _apiDbContext.DeliveryPlaces.Remove(dm);

            if (await _apiDbContext.SaveChangesAsync() > 0)
            {
                return new DeleteResponseRoot(true, "Place deleted successfully");
            }

            throw new SiffrumException(
                ApiErrorTypeSM.Fatal_Log,
                "Failed to delete place details"
            );
        }

        public async Task<DeleteResponseRoot> DeleteMineAsync(long sellerId, long id)
        {
            var dm = await _apiDbContext.DeliveryPlaces
                .FirstOrDefaultAsync(x => x.Id == id && x.SellerId == sellerId);

            if (dm == null)
                throw new SiffrumException(
                    ApiErrorTypeSM.InvalidInputData_Log,
                    "Place not found"
                );


            _apiDbContext.DeliveryPlaces.Remove(dm);

            if (await _apiDbContext.SaveChangesAsync() > 0)
            {
                return new DeleteResponseRoot(true, "Place deleted successfully");
            }

            throw new SiffrumException(
                ApiErrorTypeSM.Fatal_Log,
                "Failed to delete place details"
            );
        }

        #endregion
    }
}
