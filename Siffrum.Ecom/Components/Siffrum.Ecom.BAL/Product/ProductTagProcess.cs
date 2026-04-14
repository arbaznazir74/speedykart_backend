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
using System.Net;

namespace Siffrum.Ecom.BAL.Product
{
    public class ProductTagProcess : SiffrumBalOdataBase<TagSM>
    {
        private readonly ILoginUserDetail _loginUserDetail;
        private readonly ProductVariantProcess _productVariantProcess;
        public ProductTagProcess(IMapper mapper, ApiDbContext apiDbContext, ILoginUserDetail loginUserDetail, ProductVariantProcess productVariantProcess)
            :base(mapper, apiDbContext)
        {
            _loginUserDetail = loginUserDetail;
            _productVariantProcess = productVariantProcess;
        }

        #region OData
        public override async Task<IQueryable<TagSM>> GetServiceModelEntitiesForOdata()
        {
            IQueryable<TagDM> entitySet = _apiDbContext.Tag.AsNoTracking();
            return await base.MapEntityAsToQuerable<TagDM, TagSM>(_mapper, entitySet);
        }
        #endregion

        #region Tag Methods

        #region Get

        #region Get All and Count

        public async Task<List<TagSM>> GetAllTags(int skip, int top)
        {
            var dms = await _apiDbContext.Tag.AsNoTracking()
                .Skip(skip).Take(top)
                .ToListAsync();
            var sms = _mapper.Map<List<TagSM>>(dms);
            return sms;
        }

        public async Task<IntResponseRoot> GetAllTagsCount()
        {
            var count = await _apiDbContext.Tag.AsNoTracking()
                .CountAsync();
            return new IntResponseRoot(count, "Total Tags");
        }

        public async Task<List<TagSM>> GetAllTagsWithProducts(
            PlatformTypeSM platform,
            int skip,
            int top)
        {
            var dms = await _apiDbContext.Tag
                .Where(tag => tag.ProductTags
                    .Any(pt => pt.ProductVariant.PlatformType == (PlatformTypeDM)platform))
                .Skip(skip)
                .Take(top)
                .ToListAsync();

            var sms = _mapper.Map<List<TagSM>>(dms);
            return sms;
        }

        public async Task<IntResponseRoot> GetAllTagsWithProductsCount(PlatformTypeSM platform)
        {
            var count = await _apiDbContext.Tag
                .Where(tag => tag.ProductTags
                    .Any(pt => pt.ProductVariant.PlatformType == (PlatformTypeDM)platform))
                .CountAsync();
            return new IntResponseRoot(count, "Total Tags");
        }

        #endregion Get All

        #region Get By Id

        public async Task<TagSM> GetTagById(long id)
        {
            var dm = await _apiDbContext.Tag.FindAsync(id);
            if (dm != null)
            {
                var sm = _mapper.Map<TagSM>(dm);
                return sm;
            }
            return null;
        }

        #endregion Get By Id

        #endregion Get

        #region Add Tag

        public async Task<TagSM> AddTag(TagSM objSM)
        {
            if (objSM == null)
            {
                throw new SiffrumException(ApiErrorTypeSM.ModelError_NoLog, "Tag data is required");
            }
            var dm = _mapper.Map<TagDM>(objSM);
            dm.UpdatedBy = _loginUserDetail.LoginId;
            dm.CreatedAt = DateTime.UtcNow;
            _apiDbContext.Tag.Add(dm);
            if (await _apiDbContext.SaveChangesAsync() > 0)
            {
                return await GetTagById(dm.Id);
            }
            throw new SiffrumException(ApiErrorTypeSM.Fatal_Log, "Failed to add tag");
        }

        #endregion Add Tag

        #region Update Tag

        public async Task<TagSM> UpdateTag(TagSM objSM)
        {
            if (objSM == null)
            {
                throw new SiffrumException(ApiErrorTypeSM.ModelError_NoLog, "Tag data is required");
            }
            var dm = _mapper.Map<TagDM>(objSM);
            dm.UpdatedBy = _loginUserDetail.LoginId;
            dm.UpdatedAt = DateTime.UtcNow;
             _apiDbContext.Tag.Update(dm);
            if (await _apiDbContext.SaveChangesAsync() > 0)
            {
                return await GetTagById(dm.Id);
            }
            throw new SiffrumException(ApiErrorTypeSM.Fatal_Log, "Failed to update tag");
        }

        #endregion Update Tag

        #region Delete Tag

        public async Task<DeleteResponseRoot> DeleteTag(long id)
        {
            var dm = await _apiDbContext.Tag.FirstOrDefaultAsync(x => x.Id == id);
            if (dm == null)
            {
                throw new SiffrumException(ApiErrorTypeSM.InvalidInputData_Log, "Tag not found");
            }

            if (_loginUserDetail.UserType == RoleTypeSM.Seller
                && !string.Equals(dm.CreatedBy, _loginUserDetail.LoginId, StringComparison.OrdinalIgnoreCase))
            {
                throw new SiffrumException(ApiErrorTypeSM.InvalidInputData_NoLog, "You can only delete tags you created");
            }

            _apiDbContext.Tag.Remove(dm);

            if (await _apiDbContext.SaveChangesAsync() > 0)
            {
                return new DeleteResponseRoot(true, "Tag deleted successfully");
            }
            throw new SiffrumException(ApiErrorTypeSM.Fatal_Log,$"Tag with id {id} failed to delete, ", "Failed to delete tag");
        }

        #endregion Delete Tag

        #endregion Tag Methods

        #region Product Tag Methods

        #region Get Product Tag Product Variant Id      

        public async Task<List<ProductTagSM>> GetByProductVariantId(long productVariantId)
        {
            var dms = await _apiDbContext.ProductTag.AsNoTracking()
                .Where(x => x.ProductVariantId == productVariantId).ToListAsync();
            if(dms.Count == 0)
            {
                return new List<ProductTagSM>();
            }
            var response = new List<ProductTagSM>();
            foreach (var dm in dms)
            {
                var sm = _mapper.Map<ProductTagSM>(dm);
                var existingTag = await GetTagById(dm.TagId);
                if(existingTag != null)
                {
                    sm.Name = existingTag.Name;
                }                               
                response.Add(sm);
            }
            return response;
        }

        #region Get By Id
        public async Task<ProductTagSM> GetByProductTagId(long id)
        {
            var dm = await _apiDbContext.ProductTag.FindAsync(id);
            if (dm != null)
            {
                var sm = _mapper.Map<ProductTagSM>(dm);
                var existingTag = await GetTagById(dm.TagId);
                if (existingTag != null)
                {
                    sm.Name = existingTag.Name;
                }
                return sm;
            }
            return null;
        }

        #endregion Get By Id


        #endregion Get Product Tag Product Variant Id  

        #region Add Product Tag

        public async Task<ProductTagSM> AddProductTag(ProductTagSM objSM)
        {
            if (objSM == null)
                throw new SiffrumException(ApiErrorTypeSM.ModelError_NoLog, "Product tag data is required");

            var product = await _apiDbContext.ProductVariant
                .FirstOrDefaultAsync(x => x.Id == objSM.ProductVariantId);

            if (product == null)
                throw new SiffrumException(ApiErrorTypeSM.InvalidInputData_NoLog, "Product not found");

            var tag = await _apiDbContext.Tag
                .FirstOrDefaultAsync(x => x.Id == objSM.TagId);

            if (tag == null)
                throw new SiffrumException(ApiErrorTypeSM.InvalidInputData_NoLog, "Tag not found");

            var exists = await _apiDbContext.ProductTag
       .AnyAsync(x => x.ProductVariantId == objSM.ProductVariantId
                   && x.TagId == objSM.TagId);

            if (exists)
                throw new SiffrumException(ApiErrorTypeSM.InvalidInputData_NoLog,
                    "Product already has this tag");

            var dm = new ProductTagDM
            {
                ProductVariantId = product.Id,
                TagId = tag.Id
            };

            _apiDbContext.ProductTag.Add(dm);
            await _apiDbContext.SaveChangesAsync();

            return await GetByProductTagId(dm.Id);
        }

        #endregion Add Product Tag

        #region Delete Product Tag

        public async Task<DeleteResponseRoot> DeleteProductTag(long id)
        {
            var dm = await _apiDbContext.ProductTag.FindAsync(id);
            if (dm != null)
            {
                _apiDbContext.ProductTag.Remove(dm);
                if (await _apiDbContext.SaveChangesAsync() > 0)
                {
                    return new DeleteResponseRoot(true, "Product Tag deleted successfully");
                }
            }
            throw new SiffrumException(ApiErrorTypeSM.Fatal_Log, "Product Tag Not found");
        }

        #endregion Delete Product Tag

        #region Products In Tags

        public async Task<List<UserHotBoxProductSM>> GetHotBoxProductsInTag(
            long tagId, int skip, int top)
        {
            var existingTag = await _apiDbContext.Tag.FindAsync(tagId);
            if (existingTag == null)
            {
                throw new SiffrumException(
                    ApiErrorTypeSM.Fatal_Log,
                    $"User tries to fetch tag products on TagId: {tagId} which is not found",
                    "Tag not found");
            }

            var productVariants = await _apiDbContext.ProductTag
                .Where(pt => pt.TagId == tagId &&
                             pt.ProductVariant.PlatformType == PlatformTypeDM.HotBox)
                .Select(pt => pt.ProductVariant)
                .OrderBy(pt => pt.ViewCount)
                .Select(i=>i.Id)
                .Skip(skip)
                .Take(top)
                .ToListAsync();
            var products = await _productVariantProcess.GetHotBoxProductsByBanner(productVariants);
            

            return products;
        }

        public async Task<IntResponseRoot> GetHotBoxProductsInTagCount(
            long tagId)
        {
            var existingTag = await _apiDbContext.Tag.FindAsync(tagId);
            if (existingTag == null)
            {
                throw new SiffrumException(
                    ApiErrorTypeSM.Fatal_Log,
                    $"User tries to fetch tag products on TagId: {tagId} which is not found",
                    "Tag not found");
            }

            var productVariants = await _apiDbContext.ProductTag
                .Where(pt => pt.TagId == tagId &&
                             pt.ProductVariant.PlatformType == PlatformTypeDM.HotBox)
                .Select(pt => pt.ProductVariant)
                .OrderBy(pt => pt.ViewCount)
                .CountAsync();
            return new  IntResponseRoot(productVariants, "Total Products");
        }

        public async Task<List<UserSpeedyMartProductSM>> GetSpeedyMartProductsInTag(
            long tagId, int skip, int top)
        {
            var existingTag = await _apiDbContext.Tag.FindAsync(tagId);
            if (existingTag == null)
            {
                throw new SiffrumException(
                    ApiErrorTypeSM.Fatal_Log,
                    $"User tries to fetch tag products on TagId: {tagId} which is not found",
                    "Tag not found");
            }

            var productVariants = await _apiDbContext.ProductTag
                .Where(pt => pt.TagId == tagId &&
                             pt.ProductVariant.PlatformType == PlatformTypeDM.SpeedyMart)
                .Select(pt => pt.ProductVariant)
                .OrderBy(pt => pt.ViewCount)
                .Select(i => i.Id)
                .Skip(skip)
                .Take(top)
                .ToListAsync();
            var products = await _productVariantProcess.GetSpeedyMartProductsByBanner(productVariants);


            return products;
        }

        public async Task<IntResponseRoot> GetSpeedyMartProductsInTagCount(
            long tagId)
        {
            var existingTag = await _apiDbContext.Tag.FindAsync(tagId);
            if (existingTag == null)
            {
                throw new SiffrumException(
                    ApiErrorTypeSM.Fatal_Log,
                    $"User tries to fetch tag products on TagId: {tagId} which is not found",
                    "Tag not found");
            }

            var productVariants = await _apiDbContext.ProductTag
                .Where(pt => pt.TagId == tagId &&
                             pt.ProductVariant.PlatformType == PlatformTypeDM.SpeedyMart)
                .Select(pt => pt.ProductVariant)
                .OrderBy(pt => pt.ViewCount)
                .CountAsync();
            return new IntResponseRoot(productVariants, "Total Products");
        }

        #endregion Products In Tags

        #endregion Product Tag Methods

    }
}
