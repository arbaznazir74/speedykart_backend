using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Siffrum.Ecom.BAL.Foundation.Base;
using Siffrum.Ecom.BAL.Base.ImageProcess;
using Siffrum.Ecom.BAL.ExceptionHandler;
using Siffrum.Ecom.DAL.Context;
using Siffrum.Ecom.DomainModels.Enums;
using Siffrum.Ecom.DomainModels.v1;
using Siffrum.Ecom.ServiceModels.Foundation.Base.CommonResponseRoot;
using Siffrum.Ecom.ServiceModels.Foundation.Base.Enums;
using Siffrum.Ecom.ServiceModels.Foundation.Base.Interfaces;
using Siffrum.Ecom.ServiceModels.v1;
using Siffrum.Ecom.ServiceModels.Enums;

namespace Siffrum.Ecom.BAL.Product
{
    public class BrandProcess : SiffrumBalOdataBase<BrandSM>
    {
        private readonly ILoginUserDetail _loginUserDetail;
        private readonly ImageProcess _imageProcess;

        public BrandProcess(
            IMapper mapper,
            ApiDbContext apiDbContext,
            ImageProcess imageProcess,
            ILoginUserDetail loginUserDetail)
            : base(mapper, apiDbContext)
        {
            _loginUserDetail = loginUserDetail;
            _imageProcess = imageProcess;
        }

        #region OData
        public override async Task<IQueryable<BrandSM>> GetServiceModelEntitiesForOdata()
        {
            IQueryable<BrandDM> entitySet = _apiDbContext.Brand
                .Where(x => x.Status == StatusDM.Active)
                .AsNoTracking();

            return await base.MapEntityAsToQuerable<BrandDM, BrandSM>(_mapper, entitySet);
        }
        #endregion

        #region CREATE
        public async Task<BoolResponseRoot> CreateAsync(BrandSM objSM)
        {
            if (objSM == null)
                throw new SiffrumException(ApiErrorTypeSM.ModelError_NoLog, "Brand data is required");

            if (string.IsNullOrWhiteSpace(objSM.Name))
                throw new SiffrumException(ApiErrorTypeSM.InvalidInputData_NoLog, "Brand name is required");

            var isNameAvailable = await IsBrandNameAvailable(objSM.Name);
            if (!isNameAvailable)
            {
                throw new SiffrumException(
                    ApiErrorTypeSM.InvalidInputData_NoLog,
                    "Brand already exists",
                    "A brand with this name already exists"
                );
            }

            var dm = _mapper.Map<BrandDM>(objSM);

            if (!string.IsNullOrWhiteSpace(objSM.Image))
            {
                dm.Image = await _imageProcess.SaveFromBase64(objSM.Image, "jpg", "wwwroot/content/brands");
            }

            dm.Status = (StatusDM)objSM.Status;
            dm.CreatedAt = DateTime.UtcNow;
            dm.CreatedBy = _loginUserDetail.LoginId;

            await _apiDbContext.Brand.AddAsync(dm);

            if (await _apiDbContext.SaveChangesAsync() > 0)
            {
                return new BoolResponseRoot(true, "Brand created successfully");
            }

            throw new SiffrumException(ApiErrorTypeSM.Fatal_Log, "Failed to create brand");
        }
        #endregion

        #region READ
        public async Task<List<BrandSM>> GetAll(int skip, int top, string role)
        {
            IQueryable<BrandDM> query = _apiDbContext.Brand.AsNoTracking();

            if (role == "User" || role == "Seller")
            {
                query = query.Where(x => x.Status == StatusDM.Active);
            }

            var ids = await query
                .Skip(skip)
                .Take(top)  
                .Select(x => x.Id)
                .ToListAsync();


            var response = new List<BrandSM>();

            foreach (var id in ids)
            {
                var brand = await GetByIdAsync(id, role);
                if (brand != null)
                    response.Add(brand);
            }

            return response;
        }

        public async Task<IntResponseRoot> GetCount(string role)
        {
            IQueryable<BrandDM> query = _apiDbContext.Brand.AsNoTracking();

            if (role == "User" || role == "Seller")
            {
                query = query.Where(x => x.Status == StatusDM.Active);
            }

            var count = await query
                .CountAsync();

            return new IntResponseRoot(count, "Total brands");
        }

        public async Task<BrandSM?> GetByIdAsync(long id , string role)
        {
            var dm = await _apiDbContext.Brand
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id);

            if (dm != null)
            {
                if (role == "User" || role == "Seller")
                {
                    if (dm.Status != StatusDM.Active)
                    {
                        return null;
                    }
                }
                var sm = _mapper.Map<BrandSM>(dm);
                if (!string.IsNullOrEmpty(dm.Image))
                {
                    var bImg = await _imageProcess.ResolveImage(dm.Image);
                    sm.Image = bImg.Base64;
                    sm.NetworkImage = bImg.NetworkUrl;
                }
                return sm;

            }
            else
            {
                return null;
            }
            
        }

        public async Task<List<SearchResponseSM>> SearchBrands(
           string searchText,
           int skip,
           int top)
        {
            if (string.IsNullOrWhiteSpace(searchText))
                return new List<SearchResponseSM>();

            var words = searchText
                .Trim()
                .Split(' ', StringSplitOptions.RemoveEmptyEntries);

            IQueryable<BrandDM> query = _apiDbContext.Brand
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
        #endregion

        #region UPDATE
        public async Task<BrandSM?> UpdateAsync(long id, BrandSM objSM)
        {
            if (objSM == null)
                throw new SiffrumException(ApiErrorTypeSM.ModelError_NoLog, "Brand data is required");

            var dm = await _apiDbContext.Brand.FirstOrDefaultAsync(x => x.Id == id);
            
            if(dm == null)
            {
                throw new SiffrumException(ApiErrorTypeSM.InvalidInputData_NoLog,$"Brand with Id: {id} not found", "Brand not found");
            }
            if (!string.Equals(dm.Name, objSM.Name, StringComparison.OrdinalIgnoreCase))
            {
                var exists = await _apiDbContext.Brand
                    .AsNoTracking()
                    .AnyAsync(x => x.Name.ToLower() == objSM.Name.ToLower() && x.Id != id);

                if (exists)
                {
                    throw new SiffrumException(
                        ApiErrorTypeSM.InvalidInputData_NoLog,
                        "Brand name already exists"
                    );
                }
            }

            var previousImage = dm.Image;
            if (!string.IsNullOrWhiteSpace(objSM.Image))
            { 
                dm.Image = await _imageProcess.SaveFromBase64(objSM.Image, "jpg", "wwwroot/content/brands");
            }

            dm.Name = objSM.Name;
            dm.Status = (StatusDM)objSM.Status;
            dm.UpdatedAt = DateTime.UtcNow;
            dm.UpdatedBy = _loginUserDetail.LoginId;

            if (await _apiDbContext.SaveChangesAsync() > 0)
            {
                if (File.Exists(previousImage)) File.Delete(previousImage);
                return await GetByIdAsync(id,"SystemAdmin");
            }

            throw new SiffrumException(ApiErrorTypeSM.Fatal_Log, "Failed to update brand");
        }

        public async Task<BoolResponseRoot> UpdateStatusAsync(long id, StatusSM status)
        {
            var dm = await _apiDbContext.Brand.FirstOrDefaultAsync(x => x.Id == id);

            if (dm == null)
                throw new SiffrumException(ApiErrorTypeSM.InvalidInputData_Log, "Brand not found");

            if (dm.Status == (StatusDM)status)
                return new BoolResponseRoot(false, "Brand status is already updated");

            dm.Status = (StatusDM)status;
            dm.UpdatedAt = DateTime.UtcNow;
            dm.UpdatedBy = _loginUserDetail.LoginId;

            if (await _apiDbContext.SaveChangesAsync() > 0)
            {
                return new BoolResponseRoot(true, "Brand status updated successfully");
            }

            throw new SiffrumException(ApiErrorTypeSM.Fatal_Log, "Failed to update brand status");
        }
        #endregion

        #region DELETE (SOFT DELETE)

        public async Task<DeleteResponseRoot> DeleteAsync(long id)
        {
            var dm = await _apiDbContext.Brand.FirstOrDefaultAsync(x => x.Id == id);
            if (dm == null)
            {
                throw new SiffrumException(ApiErrorTypeSM.InvalidInputData_Log, "Brand not found");
            }
            
            string brandImage = null;
            if (!string.IsNullOrEmpty(dm.Image))
            {
                brandImage = dm.Image;
            }
             _apiDbContext.Brand.Remove(dm);
            if (await _apiDbContext.SaveChangesAsync() > 0)
            {
                if (File.Exists(brandImage)) File.Delete(brandImage);
                return new DeleteResponseRoot(true, "Brand deleted successfully");
            }

            throw new SiffrumException(ApiErrorTypeSM.Fatal_Log, "Failed to delete brand");
        }
        #endregion

        #region Helpers
        private async Task<bool> IsBrandNameAvailable(string name)
        {
            return !await _apiDbContext.Brand
                .AsNoTracking()
                .AnyAsync(x =>
                    x.Name.ToLower() == name.ToLower()
                );
        }
        #endregion
    }
}
