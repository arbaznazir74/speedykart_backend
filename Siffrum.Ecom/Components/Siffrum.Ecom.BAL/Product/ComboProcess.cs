using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
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

namespace Siffrum.Ecom.BAL.Product
{
    public class ComboProcess : SiffrumBalOdataBase<ComboProductSM>
    {
        private readonly ILoginUserDetail _loginUserDetail;
        private readonly ImageProcess _imageProcess;

        public ComboProcess(
            IMapper mapper,
            ApiDbContext apiDbContext, ImageProcess imageProcess,
            ILoginUserDetail loginUserDetail)
            : base(mapper, apiDbContext)
        {
            _loginUserDetail = loginUserDetail;
            _imageProcess = imageProcess;
        }

        #region ODATA
        public override async Task<IQueryable<ComboProductSM>> GetServiceModelEntitiesForOdata()
        {
            IQueryable<ComboProductDM> entitySet =
                _apiDbContext.ProductCombos.AsNoTracking();

            return await base.MapEntityAsToQuerable<ComboProductDM, ComboProductSM>(
                _mapper, entitySet);
        }
        #endregion

        #region CREATE
        public async Task<BoolResponseRoot> CreateAsync(ComboProductSM objSM)
        {
            if (objSM == null)
                throw new SiffrumException(
                    ApiErrorTypeSM.ModelError_NoLog,
                    "Combo product data is required");

            if (string.IsNullOrWhiteSpace(objSM.Name))
                throw new SiffrumException(
                    ApiErrorTypeSM.InvalidInputData_NoLog,
                    "Combo name is required");

            if (objSM.ProductIds == null)
                throw new SiffrumException(
                    ApiErrorTypeSM.InvalidInputData_NoLog,
                    "ProductIds are required");

            var dm = new ComboProductDM
            {
                Name = objSM.Name,
                Description = objSM.Description,
                IsInHotBox = objSM.IsInHotBox,
                TotalProducts = objSM.TotalProducts,
                BestFor = objSM.BestFor,
                ProductIds = JsonConvert.SerializeObject(objSM.ProductIds),
                JsonDetails = objSM.JsonDetails != null
                    ? JsonConvert.SerializeObject(objSM.JsonDetails)
                    : null,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = _loginUserDetail.LoginId
            };

            await _apiDbContext.ProductCombos.AddAsync(dm);

            if (await _apiDbContext.SaveChangesAsync() > 0)
                return new BoolResponseRoot(true, "Combo created successfully");

            throw new SiffrumException(
                ApiErrorTypeSM.Fatal_Log,
                "Failed to create combo");
        }
        #endregion

        #region Assign Combo To Category

        public async Task<List<ComboProductSM>> GetComboProductsInCategory(long categoryId, PlatformTypeSM platform, int count)
        {
            var response = new List<ComboProductSM>();
            var category = await _apiDbContext.Category.FindAsync(categoryId);
            if (category == null || category.Platform != (PlatformTypeDM)platform)
            {
                return response;
            }
            var comboCategories = await _apiDbContext.ComboCategory.AsNoTracking()
                .Where(x => x.CategoryId == categoryId).Select(x => x.ComboProductId).Take(count).ToListAsync();
            if(comboCategories.Count > 0)
            {
               foreach(var id in comboCategories)
                {
                    var res = await GetByIdAsync(id);
                    if(res != null)
                    {
                        response.Add(res);
                    }
                    
                }
                return response;
            }
            return response;
        }

        public async Task<ComboCategorySM> GetComboByComboProductCategoryId(long comboCategoryId)
        {
            var comboCategory = await _apiDbContext.ComboCategory.FindAsync(comboCategoryId);
            if (comboCategory == null)
            {
                throw new SiffrumException(ApiErrorTypeSM.InvalidInputData_NoLog, "Combo not found");
            }
            var combo = await _apiDbContext.ProductCombos.FirstOrDefaultAsync(x => x.Id == comboCategory.ComboProductId);
            if (combo == null)
            {
                throw new SiffrumException(ApiErrorTypeSM.InvalidInputData_NoLog, "Combo not found");

            }

            var category = await _apiDbContext.Category.FirstOrDefaultAsync(x => x.Id == comboCategory.CategoryId);
            if (category == null)
            {
                throw new SiffrumException(ApiErrorTypeSM.InvalidInputData_NoLog, "Category not found");
            }
            var sm = new ComboCategorySM()
            {
                Id = comboCategory.Id,
                CategoryId = category.Id,
                ComboProductId = combo.Id,
                CategoryName = category.Name,
                ComboName = combo.Name,

            };
            return sm;
        }

        public async Task<List<ComboProductSM>> GetComboByProductIds(List<long> productIds, int top)
        {
            var combos = await _apiDbContext.ProductCombos
                .AsNoTracking()
                .Where(c => c.IsInHotBox)
                .ToListAsync();

            var comboIds = combos
                .Where(c =>
                {
                    if (string.IsNullOrEmpty(c.ProductIds))
                        return false;

            var ids = JsonConvert.DeserializeObject<ProductIdsSM>(c.ProductIds);

            return ids?.ProductIds?.Any(pid => productIds.Contains(pid)) == true;
                })
                .Select(c => c.Id)
                .Take(top)
                .ToList();


            var response = new List<ComboProductSM>();
            if(comboIds.Count == 0)
            {
                return response;
            }

            foreach (var id in comboIds)
            {
                var res = await GetByIdAsync(id);
                if (res != null)
                {
                    response.Add(res);
                }
            }
            return response;
        }

        public async Task<DeleteResponseRoot> DeleleComboCategoryAsync(long comboCategoryId)
        {
            var comboCategory = await _apiDbContext.ComboCategory.FirstOrDefaultAsync(x => x.Id == comboCategoryId);
            if (comboCategory == null)
            {
                throw new SiffrumException(ApiErrorTypeSM.InvalidInputData_NoLog, "Combo not found");
            }
            _apiDbContext.ComboCategory.Remove(comboCategory);
            if (await _apiDbContext.SaveChangesAsync() > 0)
            {
                return new DeleteResponseRoot(true, "Combo category deleted successfully");
            }
            throw new SiffrumException(ApiErrorTypeSM.Fatal_Log, "Failed to delete combo category");
        }
        public async Task<ComboCategorySM> AssignComboToCategoryAsync(long comboId, long categoryId)
        {
            var combo = await _apiDbContext.ProductCombos.FirstOrDefaultAsync(x => x.Id == comboId);
            if (combo == null)
            {
                throw new SiffrumException(ApiErrorTypeSM.InvalidInputData_NoLog, "Combo not found");

            }

            var category = await _apiDbContext.Category.FirstOrDefaultAsync(x => x.Id == categoryId);
            if (category == null)
            {
                throw new SiffrumException(ApiErrorTypeSM.InvalidInputData_NoLog, "Category not found");
            }
            var existing = await _apiDbContext.ComboCategory.Where(x=>x.CategoryId == categoryId && x.ComboProductId == comboId).FirstOrDefaultAsync();
            if(existing != null)
            {
                return await GetComboByComboProductCategoryId(existing.Id);
            }
            var comboCategory = new ComboCategoryDM()
            {
                CategoryId = categoryId,
                ComboProductId = comboId,

            };
            await _apiDbContext.ComboCategory.AddAsync(comboCategory);
            if(await _apiDbContext.SaveChangesAsync() > 0)
            {
                return await GetComboByComboProductCategoryId(comboCategory.Id);
                //return new BoolResponseRoot(true, "Combo assigned to category successfully");
            }
            throw new SiffrumException(ApiErrorTypeSM.Fatal_Log, "Failed to assign combo to category");
        }

            #endregion Assign Combo To Category

        #region READ
        public async Task<ComboProductSM?> GetByIdAsync(long id)
        {
            var dm = await _apiDbContext.ProductCombos
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id);
            var cCategory = await _apiDbContext.ComboCategory.Where(x => x.ComboProductId == dm.Id).FirstOrDefaultAsync();
            long cId = 0;
            var cName = "";
            if (cCategory != null)
            {
                var category = await _apiDbContext.Category.FindAsync(cCategory.CategoryId);
                cId = category.Id;
                cName = category?.Name;
            }
            if (dm == null) return null;

            var sm = await MapDmToSm(dm);
            sm.CategoryId = cId;
            sm.CategoryName = cName;
            return sm;
        }

        public async Task<List<ComboProductSM>> GetAllAsync(int skip, int top)
        {
            var ids = await _apiDbContext.ProductCombos
                .AsNoTracking()
                .OrderByDescending(x => x.Id)
                .Skip(skip)
                .Take(top)
                .Select(x => x.Id)
                .ToListAsync();

            var response = new List<ComboProductSM>();

            foreach (var id in ids)
            {
                var combo = await GetByIdAsync(id);
                if (combo != null)
                    response.Add(combo);
            }

            return response;
        }

        public async Task<IntResponseRoot> GetCountAsync()
        {
            var count = await _apiDbContext.ProductCombos.CountAsync();
            return new IntResponseRoot(count, "Total combos");
        }
        #endregion

        #region UPDATE
        public async Task<ComboProductSM?> UpdateAsync(long id, ComboProductSM objSM)
        {
            if (objSM == null)
                throw new SiffrumException(
                    ApiErrorTypeSM.ModelError_NoLog,
                    "Combo product data is required");

            var dm = await _apiDbContext.ProductCombos
                .FirstOrDefaultAsync(x => x.Id == id);

            if (dm == null)
                throw new SiffrumException(
                    ApiErrorTypeSM.InvalidInputData_NoLog,
                    $"Combo with Id {id} not found");

            dm.Name = objSM.Name;
            dm.Description = objSM.Description;
            dm.IsInHotBox = objSM.IsInHotBox;            
            dm.BestFor = objSM.BestFor;

            if (objSM.ProductIds != null)
                dm.ProductIds = JsonConvert.SerializeObject(objSM.ProductIds);

            dm.JsonDetails = objSM.JsonDetails != null
                ? JsonConvert.SerializeObject(objSM.JsonDetails)
                : null;
            dm.TotalProducts = objSM.ProductIds?.ProductIds?.Count ?? 0;
            dm.UpdatedAt = DateTime.UtcNow;
            dm.UpdatedBy = _loginUserDetail.LoginId;

            if (await _apiDbContext.SaveChangesAsync() > 0)
                return await GetByIdAsync(id);

            throw new SiffrumException(
                ApiErrorTypeSM.Fatal_Log,
                "Failed to update combo");
        }
        #endregion

        #region DELETE
        public async Task<DeleteResponseRoot> DeleteAsync(long id)
        {
            var dm = await _apiDbContext.ProductCombos
                .FirstOrDefaultAsync(x => x.Id == id);

            if (dm == null)
                throw new SiffrumException(
                    ApiErrorTypeSM.InvalidInputData_Log,
                    "Combo not found");

            _apiDbContext.ProductCombos.Remove(dm);

            if (await _apiDbContext.SaveChangesAsync() > 0)
                return new DeleteResponseRoot(true, "Combo deleted successfully");

            throw new SiffrumException(
                ApiErrorTypeSM.Fatal_Log,
                "Failed to delete combo");
        }
        #endregion

        #region HELPERS        
        private async Task<ComboProductSM> MapDmToSm(ComboProductDM dm)
        {
            var sm = new ComboProductSM();
            sm.Id = dm.Id;
            sm.Name = dm.Name;
            sm.Description = dm.Description;
            sm.BestFor = dm.BestFor;
            sm.CreatedBy = dm.CreatedBy;
            sm.UpdatedBy = dm.UpdatedBy;
            sm.CreatedAt = dm.CreatedAt;
            sm.IsInHotBox = dm.IsInHotBox;
            sm.UpdatedAt = dm.UpdatedAt;

            sm.ProductIds = !string.IsNullOrEmpty(dm.ProductIds)
                ? JsonConvert.DeserializeObject<ProductIdsSM>(dm.ProductIds)
                : null;

            sm.JsonDetails = !string.IsNullOrEmpty(dm.JsonDetails)
                ? JsonConvert.DeserializeObject<ComboJsonDataSM>(dm.JsonDetails)
                : null;

            sm.ProductData = new List<ProductImageDataSM>();

            if (sm.ProductIds?.ProductIds != null && sm.ProductIds.ProductIds.Any())
            {
                // Step 1: Fetch from DB (NO Base64 conversion here)
                var products = await _apiDbContext.ProductVariant
                    .AsNoTracking()
                    .Where(p => sm.ProductIds.ProductIds.Contains(p.Id))
                    .Select(p => new
                    {
                        p.Id,
                        p.Name,
                        p.Image,
                        p.Price,
                    })
                    .ToListAsync();

                // Step 2: Convert Base64 after data is loaded
                foreach (var product in products)
                {
                    var cImg = await _imageProcess.ResolveImage(product.Image);
                    sm.ProductData.Add(new ProductImageDataSM
                    {
                        Id = product.Id,
                        Name = product.Name,
                        ImageBase64 = cImg.Base64,
                        NetworkImage = cImg.NetworkUrl
                    });
                }
                sm.Price = products.Sum(p => p.Price);
            }
            sm.TotalProducts = sm.ProductIds?.ProductIds?.Count ?? 0;

            return sm;
        }


        #endregion
    }
}
