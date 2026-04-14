using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Siffrum.Ecom.BAL.Base.ImageProcess;
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
    public class ProductImagesProcess : SiffrumBalOdataBase<ProductImagesSM>
    {
        private readonly ILoginUserDetail _loginUserDetail;
        private readonly ImageProcess _imageProcess;

        public ProductImagesProcess(
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
        public override async Task<IQueryable<ProductImagesSM>> GetServiceModelEntitiesForOdata()
        {
            IQueryable<ProductImagesDM> entitySet = _apiDbContext.ProductImages.AsNoTracking();
            return await base.MapEntityAsToQuerable<ProductImagesDM, ProductImagesSM>(_mapper, entitySet);
        }
        #endregion

        #region Get Product Images

        public async Task<List<ProductImagesSM>> GetAllProductImages(int skip, int top)
        {
            var ids = await _apiDbContext.ProductImages.AsNoTracking()
                .Skip(skip).Take(top)
                .Select(x => x.Id).ToListAsync();
            if (ids.Count > 0)
            {
                var response = new List<ProductImagesSM>();
                foreach (var id in ids)
                {
                    var image = await GetById(id);
                    if (image != null)
                    {
                        response.Add(image);
                    }
                }
                return response;
            }
            return new List<ProductImagesSM>();
        }

        public async Task<IntResponseRoot> GetAllProductImagesCount()
        {
            var count = await _apiDbContext.ProductImages.AsNoTracking().CountAsync();

            return new IntResponseRoot(count, "Total Product Images");
        }

        public async Task<List<ProductImagesSM>> GetProductImages(long productVariantId)
        {
            var ids = await _apiDbContext.ProductImages.AsNoTracking()
                .Where(x => x.ProductVariantId == productVariantId).Select(x => x.Id).ToListAsync();
            if(ids.Count > 0)
            {
                var response = new List<ProductImagesSM>();
                foreach (var id in ids)
                {
                    var image = await GetById(id);
                    if (image != null)
                    {
                        response.Add(image);
                    }
                }
                return response;
            }
            return new List<ProductImagesSM>();
        }

        #region Get Product Image By Id

        public async Task<ProductImagesSM> GetById(long id)
        {
            var dm = await _apiDbContext.ProductImages.AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id);
            if (dm != null)
            {
                var sm = _mapper.Map<ProductImagesSM>(dm);
                if (!string.IsNullOrEmpty(dm.Image))
                {
                    var imageBase64 = await _imageProcess.ConvertToBase64(dm.Image);
                    if (!string.IsNullOrEmpty(imageBase64))
                    {
                        sm.ImageBase64 = imageBase64;
                    }
                    else
                    {
                        sm.ImageBase64 = null;
                    }
                }
                return sm;
            }
            return null;
        }

        #endregion Get Product Image By Id

        #endregion Get Product Images

        #region Add Product Images
               

        public async Task<List<ProductImagesSM>> AddMultipleProductImagesAsync(
            long productVariantId, List<ProductImagesSM> images)
        {
            if (images == null || !images.Any())
            {
                throw new SiffrumException(
                    ApiErrorTypeSM.ModelError_NoLog,
                    "Product images are required");
            }

            // ✅ Validate Product Variant Exists
            var variantExists = await _apiDbContext.ProductVariant
                .AnyAsync(x => x.Id == productVariantId);

            if (!variantExists)
            {
                throw new SiffrumException(
                    ApiErrorTypeSM.InvalidInputData_NoLog,
                    "Product variant not found");
            }
            var dmList = new List<ProductImagesDM>();

            foreach (var img in images)
            {
                if (string.IsNullOrWhiteSpace(img.ImageBase64))
                    continue; // Skip invalid entries


                // ✅ Save Image
                var imgPath = await _imageProcess.SaveFromBase64(
                    img.ImageBase64,
                    "jpg",
                    "wwwroot/content/product");

                // ✅ Map SM → DM
                var dm = _mapper.Map<ProductImagesDM>(img);

                dm.ProductVariantId = productVariantId;
                dm.Image = imgPath;
                dm.CreatedAt = DateTime.UtcNow;
                dm.CreatedBy = _loginUserDetail.LoginId;

                dmList.Add(dm);
            }

            if (!dmList.Any())
            {
                throw new SiffrumException(
                    ApiErrorTypeSM.InvalidInputData_NoLog,
                    "No valid images found");
            }

            await _apiDbContext.ProductImages.AddRangeAsync(dmList);

            if (await _apiDbContext.SaveChangesAsync() > 0)
            {
                var response = await GetProductImages(productVariantId);
                return response;
            }

            throw new SiffrumException(
                ApiErrorTypeSM.Fatal_Log,
                "Failed to add product images");
        }


        #endregion Add Product Images

        #region Update Product Image

        public async Task<ProductImagesSM> UpdateProductImage(long id, ProductImagesSM objSM)
        {
            if (objSM == null)
            {
                throw new SiffrumException(ApiErrorTypeSM.ModelError_NoLog, "Product image data is required");

            }
            var dm = await _apiDbContext.ProductImages.FindAsync(id);
            if (dm == null) 
            {
                throw new SiffrumException(ApiErrorTypeSM.Fatal_Log,$"User tries to update image for Id:{id}, but image with this id is not present", "Product image not found");
            }
            string oldImagePath = null;
            if (!string.IsNullOrWhiteSpace(objSM.ImageBase64))
            {
                var imgPath = await _imageProcess.SaveFromBase64(objSM.ImageBase64, "jpg", "wwwroot/content/product");
                oldImagePath = dm.Image;
                dm.Image = imgPath;

            }
            dm.UpdatedAt = DateTime.UtcNow;
            dm.UpdatedBy = _loginUserDetail.LoginId;
            _apiDbContext.ProductImages.Update(dm);
            if (await _apiDbContext.SaveChangesAsync() > 0)
            {
                if (File.Exists(oldImagePath)) File.Delete(oldImagePath);
                return await GetById(id);
            }
            throw new SiffrumException(ApiErrorTypeSM.Fatal_Log, "Failed to update product image");
        }

        #endregion Update Product Image

        #region Delete Product Images

        public async Task<DeleteResponseRoot> DeleteProductImage(long id)
        {
            var dm = await _apiDbContext.ProductImages
               .FirstOrDefaultAsync(x => x.Id == id);
            if (dm != null)
            {
                string productImagePath = null;
                if (!string.IsNullOrEmpty(dm.Image))
                {
                    productImagePath = dm.Image;
                }
                _apiDbContext.ProductImages.Remove(dm);
                if (await _apiDbContext.SaveChangesAsync() > 0)
                {
                    if (File.Exists(productImagePath)) File.Delete(productImagePath);
                    return new DeleteResponseRoot(true, "Product image deleted successfully");
                }
            }
            throw new SiffrumException(ApiErrorTypeSM.Fatal_Log, "Product Image not found");
        }

        #endregion Delete Product Images
    }
}
