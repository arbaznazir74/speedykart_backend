using AutoMapper;
using Google.Api.Gax;
using Microsoft.EntityFrameworkCore;
using Siffrum.Ecom.BAL.Base.ImageProcess;
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
    public class PromotionalContentProcess : SiffrumBalOdataBase<PromotionalContentSM>
    {
        private readonly ImageProcess _imageProcess;
        private readonly ILoginUserDetail _loginUserDetail;

        public PromotionalContentProcess(
            ApiDbContext apiDbContext,
            IMapper mapper,
            ImageProcess imageProcess,
            ILoginUserDetail loginUserDetail)
            : base(mapper, apiDbContext)
        {
            _imageProcess = imageProcess;
            _loginUserDetail = loginUserDetail;
        }

        #region ODATA
        public override async Task<IQueryable<PromotionalContentSM>> GetServiceModelEntitiesForOdata()
        {
            var entitySet = _apiDbContext.PromotionalContents
                .AsNoTracking();

            return await base.MapEntityAsToQuerable<
                PromotionalContentDM,
                PromotionalContentSM>(_mapper, entitySet);
        }
        #endregion

        #region CREATE
        public async Task<BoolResponseRoot> CreateAsync(PromotionalContentSM objSM)
        {
            if (objSM == null)
                throw new SiffrumException(
                    ApiErrorTypeSM.ModelError_NoLog,
                    "Offer data is required"
                );

            if (string.IsNullOrWhiteSpace(objSM.Title))
                throw new SiffrumException(
                    ApiErrorTypeSM.InvalidInputData_NoLog,
                    "Title is required"
                );


            bool exists = await _apiDbContext.PromotionalContents
                .AnyAsync(x => x.Title.ToLower() == objSM.Title.ToLower());

            if (exists)
                throw new SiffrumException(
                    ApiErrorTypeSM.InvalidInputData_NoLog,
                    "Offer already exists"
                );

            var dm = _mapper.Map<PromotionalContentDM>(objSM);
            if (!string.IsNullOrEmpty(objSM.IconBase64))
            {
                if(objSM.ExtensionType == null)
                {
                    throw new SiffrumException(
                        ApiErrorTypeSM.InvalidInputData_NoLog,
                        "ExtensionType is required when icon is uploaded"
                    );
                }
                var path = await _imageProcess.SaveFromBase64(
                    objSM.IconBase64,
                    objSM.ExtensionType.ToString().ToLower(),
                    "wwwroot/content/icons"
                );
                if (!string.IsNullOrEmpty(path))
                {
                    dm.IconPath = path;
                }
            }
            dm.CreatedAt = DateTime.UtcNow;
            dm.CreatedBy = _loginUserDetail.LoginId;

            await _apiDbContext.PromotionalContents.AddAsync(dm);

            if (await _apiDbContext.SaveChangesAsync() > 0)
                return new BoolResponseRoot(true, "Promotional content created successfully");

            throw new SiffrumException(
                ApiErrorTypeSM.Fatal_Log,
                "Failed to promotional content"
            );
        }
        #endregion

        #region READ
        public async Task<PromotionalContentSM?> GetByIdAsync(long id)
        {
            var dm = await _apiDbContext.PromotionalContents.FindAsync(id);
            if (dm == null) return null;

            var sm = _mapper.Map<PromotionalContentSM>(dm);

            if (!string.IsNullOrEmpty(dm.IconPath))
            {
                var pImg = await _imageProcess.ResolveImage(dm.IconPath);
                sm.IconBase64 = pImg.Base64;
                sm.NetworkIcon = pImg.NetworkUrl;
            }

            return sm;
        }

        public async Task<List<PromotionalContentSM>> GetAll(int skip, int top)
        {
            var ids = await _apiDbContext.PromotionalContents
                .AsNoTracking()
                .Skip(skip)
                .Take(top)
                .Select(x => x.Id)
                .ToListAsync();

            var response = new List<PromotionalContentSM>();
            foreach (var id in ids)
            {
                var offer = await GetByIdAsync(id);
                if (offer != null)
                    response.Add(offer);
            }
            return response;
        }

        public async Task<IntResponseRoot> GetCount()
        {
            var count = await _apiDbContext.PromotionalContents
                .CountAsync();
            return new IntResponseRoot(count, "Total Promotional contents");
        }

        public async Task<List<PromotionalContentSM>> GetAllByPlatform(PlatformTypeSM platform, int skip, int top)
        {
            var ids = await _apiDbContext.PromotionalContents
                .Where(x=>x.PlatformType == (PlatformTypeDM)platform)
                .AsNoTracking()
                .Skip(skip)
                .Take(top)
                .Select(x => x.Id)
                .ToListAsync();

            var response = new List<PromotionalContentSM>();
            foreach (var id in ids)
            {
                var offer = await GetByIdAsync(id);
                if (offer != null)
                    response.Add(offer);
            }
            return response;
        }

        public async Task<IntResponseRoot> GetAllCountByPlatform(PlatformTypeSM platform)
        {
            var count = await _apiDbContext.PromotionalContents
                .Where(x => x.PlatformType == (PlatformTypeDM)platform)
                .CountAsync();
            return new IntResponseRoot(count, "Total Promotional contents");
        }

        public async Task<List<PromotionalContentSM>> GetAllByDisplayLocation(PromotionDisplayLocationSM displayLocation, PlatformTypeSM platform, int skip, int top)
        {
            var ids = await _apiDbContext.PromotionalContents
                .AsNoTracking()
                .Where(x=>x.DisplayLocation == (PromotionDisplayLocationDM)displayLocation && x.PlatformType == (PlatformTypeDM)platform)
                .Skip(skip)
                .Take(top)
                .Select(x => x.Id)
                .ToListAsync();

            var response = new List<PromotionalContentSM>();
            foreach (var id in ids)
            {
                var offer = await GetByIdAsync(id);
                if (offer != null)
                    response.Add(offer);
            }
            return response;
        }

        public async Task<IntResponseRoot> GetAllByDisplayLocationCount(PromotionDisplayLocationSM displayLocation, PlatformTypeSM platform)
        {
            var count = await _apiDbContext.PromotionalContents
                .Where(x => x.DisplayLocation == (PromotionDisplayLocationDM)displayLocation && x.PlatformType == (PlatformTypeDM)platform)
                .CountAsync();
            return new IntResponseRoot(count, $"Total {displayLocation.ToString()} contents");
        }

        #endregion

        #region UPDATE
        public async Task<PromotionalContentSM?> UpdateAsync(long id, PromotionalContentSM objSM)
        {
            var dm = await _apiDbContext.PromotionalContents
                .FirstOrDefaultAsync(x => x.Id == id);

            if (dm == null)
                throw new SiffrumException(
                    ApiErrorTypeSM.InvalidInputData_NoLog,
                    "Promotion Content not found"
                );

            string oldImage = null;

            _mapper.Map(objSM, dm);

            if (!string.IsNullOrEmpty(objSM.IconBase64))
            {
                var newPath = await _imageProcess.SaveFromBase64(
                    objSM.IconBase64,
                    objSM.ExtensionType.ToString().ToLower(),
                    "wwwroot/icons"
                );
                if (!string.IsNullOrEmpty(newPath))
                {
                    oldImage = dm.IconPath;
                    dm.IconPath = newPath;
                }
                
            }
            dm.Id = id;
            dm.UpdatedAt = DateTime.UtcNow;
            dm.UpdatedBy = _loginUserDetail.LoginId;

            if (await _apiDbContext.SaveChangesAsync() > 0)
            {
                if (!string.IsNullOrEmpty(oldImage) && File.Exists(oldImage))
                    File.Delete(oldImage);

                return await GetByIdAsync(id);
            }

            throw new SiffrumException(
                ApiErrorTypeSM.Fatal_Log,
                "Failed to update Promotional content"
            );
        }
        #endregion

        #region DELETE
        public async Task<DeleteResponseRoot> DeleteAsync(long id)
        {
            var dm = await _apiDbContext.PromotionalContents
                .FirstOrDefaultAsync(x => x.Id == id);

            if (dm == null)
                throw new SiffrumException(
                    ApiErrorTypeSM.InvalidInputData_Log,
                    "Offer not found"
                );

            string oldImage = dm.IconPath;

            _apiDbContext.PromotionalContents.Remove(dm);

            if (await _apiDbContext.SaveChangesAsync() > 0)
            {
                if (!string.IsNullOrEmpty(oldImage) && File.Exists(oldImage))
                    File.Delete(oldImage);

                return new DeleteResponseRoot(true, "Promotional content deleted successfully");
            }

            throw new SiffrumException(
                ApiErrorTypeSM.Fatal_Log,
                "Failed to delete Promotional content"
            );
        }
        #endregion
    }
}
