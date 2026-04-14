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

namespace Siffrum.Ecom.BAL.Product
{
    public class UnitProcess : SiffrumBalOdataBase<UnitSM>
    {
        private readonly ILoginUserDetail _loginUserDetail;

        public UnitProcess(IMapper mapper, ApiDbContext apiDbContext,
            ILoginUserDetail loginUserDetail)
            : base(mapper, apiDbContext)
        {
            _loginUserDetail = loginUserDetail;
        }

        #region OData
        public override async Task<IQueryable<UnitSM>> GetServiceModelEntitiesForOdata()
        {
            IQueryable<UnitDM> entitySet = _apiDbContext.Unit
                .AsNoTracking();

            return await base.MapEntityAsToQuerable<UnitDM, UnitSM>(_mapper, entitySet);
        }
        #endregion

        #region CREATE
        public async Task<BoolResponseRoot> CreateAsync(UnitSM objSM)
        {
            
            if (objSM == null)
                throw new SiffrumException(ApiErrorTypeSM.ModelError_NoLog, "Unit data is required");

            if (string.IsNullOrWhiteSpace(objSM.Name))
                throw new SiffrumException(ApiErrorTypeSM.InvalidInputData_NoLog, "Unit name is required");

            var isNameAvailable = await IsUnitNameAvailable(objSM.Name);

            if (!isNameAvailable)
            {
                throw new SiffrumException(
                    ApiErrorTypeSM.InvalidInputData_NoLog,
                    "Unit already exists",
                    "A Unit with this name already exists"
                );
            }
            if (objSM.ParentId.HasValue && objSM.ParentId.Value > 0)
            {
                var existingParent = await _apiDbContext.Unit.FindAsync(objSM.ParentId.Value);
                if (existingParent == null)
                {
                    throw new SiffrumException(
                        ApiErrorTypeSM.InvalidInputData_NoLog,
                        "Parent unit not found"
                    );
                }
            }
            else
            {
                objSM.ParentId = null;
            }
            var dm = _mapper.Map<UnitDM>(objSM);
            dm.CreatedAt = DateTime.UtcNow;
            dm.CreatedBy = _loginUserDetail.LoginId;

            await _apiDbContext.Unit.AddAsync(dm);

            if (await _apiDbContext.SaveChangesAsync() > 0)
            {
                return new BoolResponseRoot(true, "Unit created successfully");
            }

            throw new SiffrumException(ApiErrorTypeSM.Fatal_Log, "Failed to create unit");
        }
        #endregion

        #region READ
        public async Task<List<UnitSM>> GetAll(int skip, int top)
        {
            var ids = await _apiDbContext.Unit.AsNoTracking()
                .Skip(skip)
                .Take(top)  
                .Select(x => x.Id)
                .ToListAsync();
            var response = new List<UnitSM>();

            foreach (var id in ids)
            {
                var unit = await GetByIdAsync(id);
                if (unit != null)
                    response.Add(unit);
            }

            return response;
        }

        public async Task<List<UnitSM>> GetAllByParentId(long parentId, int skip, int top)
        {
            var ids = await _apiDbContext.Unit.AsNoTracking()
                .Where(x => x.ParentId == parentId)
                .Skip(skip)
                .Take(top)
                .Select(x => x.Id)
                .ToListAsync();
            var response = new List<UnitSM>();

            foreach (var id in ids)
            {
                var unit = await GetByIdAsync(id);
                if (unit != null)
                    response.Add(unit);
            }

            return response;
        }

        public async Task<IntResponseRoot> GetCount()
        {
            var count = await _apiDbContext.Unit.AsNoTracking().CountAsync();

            return new IntResponseRoot(count, "Total Units");
        }

        public async Task<IntResponseRoot> GetByParentCount(long parentId)
        {
            var count = await _apiDbContext.Unit.AsNoTracking().Where(x=> x.ParentId == parentId).CountAsync();

            return new IntResponseRoot(count, "Total Child Units");
        }


        public async Task<UnitSM?> GetByIdAsync(long id)
        {
            var dm = await _apiDbContext.Unit
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id);
            if(dm != null)
            {
                var sm = _mapper.Map<UnitSM>(dm);
                return sm;
            }

            return null;    
        }
        #endregion

        #region UPDATE
        public async Task<UnitSM?> UpdateAsync(long id, UnitSM objSM)
        {
            if (objSM == null)
                throw new SiffrumException(
                    ApiErrorTypeSM.ModelError_NoLog,
                    "Unit data is required"
                );

            var dm = await _apiDbContext.Unit
                .FirstOrDefaultAsync(x => x.Id == id);

            if (dm == null)
                throw new SiffrumException(
                    ApiErrorTypeSM.InvalidInputData_NoLog,
                    $"Unit with Id: {id} not found",
                    "Unit not found"
                );

            // 🔒 Name uniqueness (index-friendly)
            if (!string.Equals(dm.Name, objSM.Name, StringComparison.OrdinalIgnoreCase))
            {
                var exists = await _apiDbContext.Unit
                    .AsNoTracking()
                    .AnyAsync(x => x.Id != id && x.Name == objSM.Name);

                if (exists)
                    throw new SiffrumException(
                        ApiErrorTypeSM.InvalidInputData_NoLog,
                        "Unit name already exists"
                    );
            }

            // 🔒 Parent validation (only if changed)
            if (dm.ParentId != objSM.ParentId)
            {
                if (objSM.ParentId == id)
                    throw new SiffrumException(
                        ApiErrorTypeSM.InvalidInputData_NoLog,
                        "Unit cannot be its own parent"
                    );

                if (objSM.ParentId.HasValue)
                {
                    var parent = await _apiDbContext.Unit
                        .AsNoTracking()
                        .FirstOrDefaultAsync(x => x.Id == objSM.ParentId);

                    if (parent == null)
                        throw new SiffrumException(
                            ApiErrorTypeSM.InvalidInputData_NoLog,
                            "Parent unit not found"
                        );

                    if (parent.ParentId.HasValue)
                        throw new SiffrumException(
                            ApiErrorTypeSM.InvalidInputData_NoLog,
                            "Only top-level units can be parent"
                        );
                }
            }

            // 🔑 Preserve immutable fields
            objSM.Id = dm.Id;

            // ✅ Map into tracked entity
            _mapper.Map(objSM, dm);

            // Audit
            dm.UpdatedAt = DateTime.UtcNow;
            dm.UpdatedBy = _loginUserDetail.LoginId;

            await _apiDbContext.SaveChangesAsync();

            return await GetByIdAsync(id);
        }


        #endregion

        #region Search
        public async Task<List<SearchResponseSM>> SearchUnits(
           string searchText,
           int skip,
           int top)
        {
            if (string.IsNullOrWhiteSpace(searchText))
                return new List<SearchResponseSM>();

            var words = searchText
                .Trim()
                .Split(' ', StringSplitOptions.RemoveEmptyEntries);

            IQueryable<UnitDM> query = _apiDbContext.Unit
                .AsNoTracking();

            // Multi-word search using LIKE (Better performance)
            foreach (var word in words)
            {
                query = query.Where(x => x.Name.ToLower().Contains(word.ToLower()));
            }

            return await query
                .OrderBy(x => x.Name)
                .Skip(skip)
                .Take(top)
                .Select(x => new SearchResponseSM
                {
                    Id = x.Id,
                    Title = x.Name
                })
                .ToListAsync();
        }
        #endregion Search


        #region DELETE (SOFT DELETE)
        public async Task<DeleteResponseRoot> DeleteAsync(long id)
        {
            var dm = await _apiDbContext.Unit
                .FirstOrDefaultAsync(x => x.Id == id);

            if (dm == null)
                throw new SiffrumException(
                    ApiErrorTypeSM.InvalidInputData_Log,
                    "Unit not found"
                );

            // 🔒 Check for child units
            var hasChildren = await _apiDbContext.Unit
                .AsNoTracking()
                .AnyAsync(x => x.ParentId == id);

            if (hasChildren)
                throw new SiffrumException(
                    ApiErrorTypeSM.InvalidInputData_Log,
                    "Cannot delete unit because it has child units"
                );

            _apiDbContext.Unit.Remove(dm);

            await _apiDbContext.SaveChangesAsync();

            return new DeleteResponseRoot(true, "Unit deleted successfully");
        }

        #endregion

        #region SELLER
        public async Task<BoolResponseRoot> CreateForSellerAsync(UnitSM objSM, long sellerId)
        {
            if (objSM == null)
                throw new SiffrumException(ApiErrorTypeSM.ModelError_NoLog, "Unit data is required");

            if (string.IsNullOrWhiteSpace(objSM.Name))
                throw new SiffrumException(ApiErrorTypeSM.InvalidInputData_NoLog, "Unit name is required");

            // Check uniqueness among global units + this seller's own units
            var exists = await _apiDbContext.Unit
                .AsNoTracking()
                .AnyAsync(x => x.Name.ToLower() == objSM.Name.ToLower().Trim()
                    && (x.SellerId == null || x.SellerId == sellerId));

            if (exists)
                throw new SiffrumException(ApiErrorTypeSM.InvalidInputData_NoLog,
                    "Unit already exists", "A unit with this name already exists");

            var dm = _mapper.Map<UnitDM>(objSM);
            dm.SellerId = sellerId;
            dm.CreatedAt = DateTime.UtcNow;
            dm.CreatedBy = _loginUserDetail.LoginId;

            await _apiDbContext.Unit.AddAsync(dm);

            if (await _apiDbContext.SaveChangesAsync() > 0)
                return new BoolResponseRoot(true, "Unit created successfully");

            throw new SiffrumException(ApiErrorTypeSM.Fatal_Log, "Failed to create unit");
        }

        public async Task<List<UnitSM>> GetForSeller(long sellerId, int skip, int top)
        {
            var list = await _apiDbContext.Unit
                .AsNoTracking()
                .Where(x => x.SellerId == null || x.SellerId == sellerId)
                .OrderBy(x => x.Name)
                .Skip(skip)
                .Take(top)
                .ToListAsync();

            return list.Select(x => _mapper.Map<UnitSM>(x)).ToList();
        }

        public async Task<DeleteResponseRoot> DeleteForSellerAsync(long id, long sellerId)
        {
            var dm = await _apiDbContext.Unit
                .FirstOrDefaultAsync(x => x.Id == id && x.SellerId == sellerId);

            if (dm == null)
                throw new SiffrumException(ApiErrorTypeSM.InvalidInputData_Log,
                    "Unit not found or you don't have permission to delete it");

            var hasChildren = await _apiDbContext.Unit
                .AsNoTracking()
                .AnyAsync(x => x.ParentId == id);

            if (hasChildren)
                throw new SiffrumException(ApiErrorTypeSM.InvalidInputData_Log,
                    "Cannot delete unit because it has child units");

            _apiDbContext.Unit.Remove(dm);
            await _apiDbContext.SaveChangesAsync();

            return new DeleteResponseRoot(true, "Unit deleted successfully");
        }
        #endregion

        #region Helpers
        private async Task<bool> IsUnitNameAvailable(string name)
        {
            return !await _apiDbContext.Unit
                .AsNoTracking()
                .AnyAsync(x =>
                    x.Name.ToLower() == name.ToLower()
                );
        }
        #endregion
    }
}
