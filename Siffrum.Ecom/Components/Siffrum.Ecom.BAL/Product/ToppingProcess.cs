using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Siffrum.Ecom.BAL.ExceptionHandler;
using Siffrum.Ecom.BAL.Foundation.Base;
using Siffrum.Ecom.DAL.Context;
using Siffrum.Ecom.DomainModels.Enums;
using Siffrum.Ecom.DomainModels.v1;
using Siffrum.Ecom.ServiceModels.Foundation.Base.CommonResponseRoot;
using Siffrum.Ecom.ServiceModels.Foundation.Base.Enums;
using Siffrum.Ecom.ServiceModels.Foundation.Base.Interfaces;
using Siffrum.Ecom.ServiceModels.v1;

namespace Siffrum.Ecom.BAL.Product
{
    public class ToppingProcess : SiffrumBalBase
    {
        private readonly ILoginUserDetail _loginUserDetail;

        public ToppingProcess(IMapper mapper, ApiDbContext apiDbContext,
            ILoginUserDetail loginUserDetail)
            : base(mapper, apiDbContext)
        {
            _loginUserDetail = loginUserDetail;
        }

        #region CREATE
        public async Task<BoolResponseRoot> CreateAsync(ToppingSM objSM)
        {
            if (objSM == null)
                throw new SiffrumException(ApiErrorTypeSM.ModelError_NoLog, "Topping data is required");

            if (string.IsNullOrWhiteSpace(objSM.Name))
                throw new SiffrumException(ApiErrorTypeSM.InvalidInputData_NoLog, "Topping name is required");

            var exists = await _apiDbContext.Topping
                .AsNoTracking()
                .AnyAsync(x => x.Name.ToLower() == objSM.Name.ToLower());

            if (exists)
                throw new SiffrumException(ApiErrorTypeSM.InvalidInputData_NoLog,
                    "Topping already exists", "A topping with this name already exists");

            var dm = _mapper.Map<ToppingDM>(objSM);
            dm.Status = StatusDM.Active;
            dm.CreatedAt = DateTime.UtcNow;
            dm.CreatedBy = _loginUserDetail.LoginId;

            await _apiDbContext.Topping.AddAsync(dm);

            if (await _apiDbContext.SaveChangesAsync() > 0)
                return new BoolResponseRoot(true, "Topping created successfully");

            throw new SiffrumException(ApiErrorTypeSM.Fatal_Log, "Failed to create topping");
        }
        #endregion

        #region READ
        public async Task<List<ToppingSM>> GetAll(int skip, int top)
        {
            var list = await _apiDbContext.Topping
                .AsNoTracking()
                .OrderBy(x => x.Name)
                .Skip(skip)
                .Take(top)
                .ToListAsync();

            return list.Select(x => _mapper.Map<ToppingSM>(x)).ToList();
        }

        public async Task<IntResponseRoot> GetCount()
        {
            var count = await _apiDbContext.Topping.AsNoTracking().CountAsync();
            return new IntResponseRoot(count, "Total Toppings");
        }

        public async Task<ToppingSM?> GetByIdAsync(long id)
        {
            var dm = await _apiDbContext.Topping
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id);

            if (dm == null) return null;
            return _mapper.Map<ToppingSM>(dm);
        }

        public async Task<List<ToppingSM>> GetAllActive()
        {
            var list = await _apiDbContext.Topping
                .AsNoTracking()
                .Where(x => x.Status == StatusDM.Active)
                .OrderBy(x => x.Name)
                .ToListAsync();

            return list.Select(x => _mapper.Map<ToppingSM>(x)).ToList();
        }
        #endregion

        #region UPDATE
        public async Task<ToppingSM?> UpdateAsync(long id, ToppingSM objSM)
        {
            if (objSM == null)
                throw new SiffrumException(ApiErrorTypeSM.ModelError_NoLog, "Topping data is required");

            var dm = await _apiDbContext.Topping.FirstOrDefaultAsync(x => x.Id == id);
            if (dm == null)
                throw new SiffrumException(ApiErrorTypeSM.InvalidInputData_NoLog,
                    $"Topping with Id: {id} not found", "Topping not found");

            if (!string.Equals(dm.Name, objSM.Name, StringComparison.OrdinalIgnoreCase))
            {
                var exists = await _apiDbContext.Topping
                    .AsNoTracking()
                    .AnyAsync(x => x.Id != id && x.Name.ToLower() == objSM.Name.ToLower());

                if (exists)
                    throw new SiffrumException(ApiErrorTypeSM.InvalidInputData_NoLog,
                        "Topping name already exists");
            }

            dm.Name = objSM.Name;
            dm.Price = objSM.Price;
            dm.Image = objSM.Image;
            dm.UpdatedAt = DateTime.UtcNow;
            dm.UpdatedBy = _loginUserDetail.LoginId;

            await _apiDbContext.SaveChangesAsync();
            return await GetByIdAsync(id);
        }

        public async Task<BoolResponseRoot> UpdateStatusAsync(long id, StatusDM status)
        {
            var dm = await _apiDbContext.Topping.FirstOrDefaultAsync(x => x.Id == id);
            if (dm == null)
                throw new SiffrumException(ApiErrorTypeSM.InvalidInputData_NoLog, "Topping not found");

            dm.Status = status;
            dm.UpdatedAt = DateTime.UtcNow;
            dm.UpdatedBy = _loginUserDetail.LoginId;

            await _apiDbContext.SaveChangesAsync();
            return new BoolResponseRoot(true, "Topping status updated");
        }
        #endregion

        #region DELETE
        public async Task<DeleteResponseRoot> DeleteAsync(long id)
        {
            var dm = await _apiDbContext.Topping.FirstOrDefaultAsync(x => x.Id == id);
            if (dm == null)
                throw new SiffrumException(ApiErrorTypeSM.InvalidInputData_Log, "Topping not found");

            var hasProducts = await _apiDbContext.ProductTopping
                .AsNoTracking()
                .AnyAsync(x => x.ToppingId == id);

            if (hasProducts)
                throw new SiffrumException(ApiErrorTypeSM.InvalidInputData_Log,
                    "Cannot delete topping because it is assigned to products");

            _apiDbContext.Topping.Remove(dm);
            await _apiDbContext.SaveChangesAsync();
            return new DeleteResponseRoot(true, "Topping deleted successfully");
        }
        #endregion

        #region SELLER SUGGEST
        public async Task<ToppingSM> SellerSuggestAsync(string name, long sellerId)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new SiffrumException(ApiErrorTypeSM.InvalidInputData_NoLog, "Topping name is required");

            var exists = await _apiDbContext.Topping
                .AsNoTracking()
                .AnyAsync(x => x.Name.ToLower() == name.ToLower().Trim());

            if (exists)
                throw new SiffrumException(ApiErrorTypeSM.InvalidInputData_NoLog,
                    "A topping with this name already exists");

            var dm = new ToppingDM
            {
                Name = name.Trim(),
                Status = StatusDM.Pending,
                SuggestedBySellerId = sellerId,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = _loginUserDetail.LoginId
            };

            await _apiDbContext.Topping.AddAsync(dm);

            if (await _apiDbContext.SaveChangesAsync() > 0)
            {
                return _mapper.Map<ToppingSM>(dm);
            }

            throw new SiffrumException(ApiErrorTypeSM.Fatal_Log, "Failed to suggest topping");
        }

        public async Task<List<ToppingSM>> GetPendingToppings()
        {
            var list = await _apiDbContext.Topping
                .AsNoTracking()
                .Where(x => x.Status == StatusDM.Pending)
                .OrderBy(x => x.CreatedAt)
                .ToListAsync();

            return list.Select(x => _mapper.Map<ToppingSM>(x)).ToList();
        }

        public async Task<BoolResponseRoot> ApproveToppingAsync(long id)
        {
            var dm = await _apiDbContext.Topping.FirstOrDefaultAsync(x => x.Id == id);
            if (dm == null)
                throw new SiffrumException(ApiErrorTypeSM.InvalidInputData_NoLog, "Topping not found");

            if (dm.Status != StatusDM.Pending)
                throw new SiffrumException(ApiErrorTypeSM.InvalidInputData_NoLog, "Topping is not pending approval");

            dm.Status = StatusDM.Active;
            dm.UpdatedAt = DateTime.UtcNow;
            dm.UpdatedBy = _loginUserDetail.LoginId;

            await _apiDbContext.SaveChangesAsync();
            return new BoolResponseRoot(true, "Topping approved successfully");
        }

        public async Task<BoolResponseRoot> RejectToppingAsync(long id)
        {
            var dm = await _apiDbContext.Topping.FirstOrDefaultAsync(x => x.Id == id);
            if (dm == null)
                throw new SiffrumException(ApiErrorTypeSM.InvalidInputData_NoLog, "Topping not found");

            if (dm.Status != StatusDM.Pending)
                throw new SiffrumException(ApiErrorTypeSM.InvalidInputData_NoLog, "Topping is not pending");

            _apiDbContext.Topping.Remove(dm);
            await _apiDbContext.SaveChangesAsync();
            return new BoolResponseRoot(true, "Topping rejected and removed");
        }

        public async Task<List<ToppingSM>> GetForSeller(long sellerId)
        {
            var list = await _apiDbContext.Topping
                .AsNoTracking()
                .Where(x => x.Status == StatusDM.Active
                    && (x.SuggestedBySellerId == null || x.SuggestedBySellerId == sellerId))
                .OrderBy(x => x.Name)
                .ToListAsync();

            return list.Select(x => _mapper.Map<ToppingSM>(x)).ToList();
        }

        public async Task<ToppingSM> CreateForSellerAsync(ToppingSM objSM, long sellerId)
        {
            if (objSM == null)
                throw new SiffrumException(ApiErrorTypeSM.ModelError_NoLog, "Topping data is required");

            if (string.IsNullOrWhiteSpace(objSM.Name))
                throw new SiffrumException(ApiErrorTypeSM.InvalidInputData_NoLog, "Topping name is required");

            // Check uniqueness among global toppings + this seller's own toppings
            var exists = await _apiDbContext.Topping
                .AsNoTracking()
                .AnyAsync(x => x.Name.ToLower() == objSM.Name.ToLower().Trim()
                    && (x.SuggestedBySellerId == null || x.SuggestedBySellerId == sellerId));

            if (exists)
                throw new SiffrumException(ApiErrorTypeSM.InvalidInputData_NoLog,
                    "A topping with this name already exists");

            var dm = new ToppingDM
            {
                Name = objSM.Name.Trim(),
                Price = objSM.Price,
                Image = objSM.Image,
                Status = StatusDM.Active,
                SuggestedBySellerId = sellerId,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = _loginUserDetail.LoginId
            };

            await _apiDbContext.Topping.AddAsync(dm);

            if (await _apiDbContext.SaveChangesAsync() > 0)
                return _mapper.Map<ToppingSM>(dm);

            throw new SiffrumException(ApiErrorTypeSM.Fatal_Log, "Failed to create topping");
        }

        public async Task<DeleteResponseRoot> DeleteForSellerAsync(long id, long sellerId)
        {
            var dm = await _apiDbContext.Topping
                .FirstOrDefaultAsync(x => x.Id == id && x.SuggestedBySellerId == sellerId);

            if (dm == null)
                throw new SiffrumException(ApiErrorTypeSM.InvalidInputData_Log,
                    "Topping not found or you don't have permission to delete it");

            // Remove product-topping associations first
            var ptList = await _apiDbContext.ProductTopping
                .Where(x => x.ToppingId == id)
                .ToListAsync();
            if (ptList.Any())
                _apiDbContext.ProductTopping.RemoveRange(ptList);

            _apiDbContext.Topping.Remove(dm);
            await _apiDbContext.SaveChangesAsync();

            return new DeleteResponseRoot(true, "Topping deleted successfully");
        }
        #endregion

        #region Search
        public async Task<List<SearchResponseSM>> SearchToppings(string searchText, int skip, int top)
        {
            if (string.IsNullOrWhiteSpace(searchText))
                return new List<SearchResponseSM>();

            var words = searchText.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);

            IQueryable<ToppingDM> query = _apiDbContext.Topping
                .AsNoTracking()
                .Where(x => x.Status == StatusDM.Active);

            foreach (var word in words)
            {
                query = query.Where(x => x.Name.ToLower().Contains(word.ToLower()));
            }

            return await query
                .OrderBy(x => x.Name)
                .Skip(skip)
                .Take(top)
                .Select(x => new SearchResponseSM { Id = x.Id, Title = x.Name })
                .ToListAsync();
        }
        #endregion
    }
}
