using AutoMapper;
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

namespace Siffrum.Ecom.BAL.Product
{
    public class ProductRatingProcess : SiffrumBalOdataBase<ProductRatingSM>
    {
        private readonly ILoginUserDetail _loginUserDetail;
        private readonly ImageProcess  _imageProcess;
        public ProductRatingProcess(IMapper mapper, ApiDbContext apiDbContext, ILoginUserDetail loginUserDetail, ImageProcess imageProcess)
            :base(mapper, apiDbContext)
        {
            _loginUserDetail = loginUserDetail;
            _imageProcess = imageProcess;
        }

        #region OData
        public override async Task<IQueryable<ProductRatingSM>> GetServiceModelEntitiesForOdata()
        {
            IQueryable<ProductRatingDM> entitySet = _apiDbContext.ProductRating.AsNoTracking();
            return await base.MapEntityAsToQuerable<ProductRatingDM, ProductRatingSM>(_mapper, entitySet);
        }
        #endregion

        #region Get All and Counts

        #region All

        public async Task<List<ProductRatingSM>> GetAll(int skip, int top)
        {
            var dms = await _apiDbContext.ProductRating.AsNoTracking().Skip(skip).Take(top).ToListAsync();
            var sms = _mapper.Map<List<ProductRatingSM>>(dms);
            return sms;
        }

        public async Task<IntResponseRoot> GetAllCount()
        {
            var count = await _apiDbContext.ProductRating.AsNoTracking().CountAsync();
            return new IntResponseRoot(count, "Total Product Ratings");
        }
        #endregion All

        #region Product Ratings

        public async Task<List<ProductRatingSM>> GetAllProductRatings(long productVariantId, int skip, int top)
        {
            var ids = await _apiDbContext.ProductRating.AsNoTracking()
                .Where(x => x.ProductVariantId == productVariantId && x.Status == StatusDM.Active)
                .Skip(skip).Take(top)                
                .Select(x => x.Id)
                .ToListAsync();
            var response = new List<ProductRatingSM>();
            if(ids.Count == 0)
            {
                return response;
            }
            foreach (var id in ids)
            {
                var res = await GetProductRatingById(id);
                if (res != null)
                {
                    response.Add(res);
                }
            }
            return response;
        }

        public async Task<IntResponseRoot> GetAllProductRatingsCount(long productVariantId)
        {
            var count = await _apiDbContext.ProductRating.AsNoTracking()
                .Where(x=>x.ProductVariantId == productVariantId && x.Status == StatusDM.Active)
                .CountAsync();
            return new IntResponseRoot(count, "Total Product Ratings");
        }

        #endregion Product Ratings

        #endregion  Get All and Counts

        #region Get By Id

        public async Task<ProductRatingSM> GetProductRatingById(long id) 
        {
            var dm = await _apiDbContext.ProductRating.AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id);
            var role = _loginUserDetail.UserType;
            if(role == RoleTypeSM.User || role == RoleTypeSM.Seller)
            {
                if(dm.Status != StatusDM.Active)
                {
                    return null;
                }
            }
            var sm = _mapper.Map<ProductRatingSM>(dm);
            var usersName = await _apiDbContext.User.AsNoTracking().Where(x => x.Id == dm.UserId).Select(x => x.Name).FirstOrDefaultAsync();
            if (string.IsNullOrEmpty(usersName))
            {
                usersName = "Anonymous";
            }
            sm.UserName = usersName;
            var images = await _apiDbContext.RatingImages.AsNoTracking().Where(x => x.ProductRatingId == dm.Id).ToListAsync();
            var imageBase64List = new List<string>();
            if(images.Count > 0)
            {
                foreach(var image in images)
                {
                    var base64 = await _imageProcess.ConvertToBase64(image.Image);
                    if (!string.IsNullOrEmpty(base64))
                    {
                        imageBase64List.Add(base64);
                    }
                }
            }
            sm.Images = imageBase64List;
            return sm;
        }      

        #endregion Get By Id

        #region Add

        public async Task<ProductRatingSM> AddProductRating(ProductRatingSM request)
        {
            using var transaction = await _apiDbContext.Database.BeginTransactionAsync();

            try
            {
                // 🔹 Validate product variant
                var variantExists = await _apiDbContext.ProductVariant
                    .AnyAsync(x => x.Id == request.ProductVariantId && x.Status == ProductStatusDM.Active);

                if (!variantExists)
                {
                    throw new SiffrumException(
                        ApiErrorTypeSM.InvalidInputData_Log,
                        $"User tried to rate non-active product variant with Id: {request.ProductVariantId}",
                        "Product not found"
                    );
                }
                var alreadyRated = await _apiDbContext.ProductRating
                    .AnyAsync(x =>
                        x.ProductVariantId == request.ProductVariantId &&
                        x.UserId == request.UserId);

                if (alreadyRated)
                {
                    throw new SiffrumException(
                        ApiErrorTypeSM.InvalidInputData_Log,
                        "You have already rated this product, Thank you for your feedback"
                    );
                }


                // 🔹 Create rating
                var rating = new ProductRatingDM
                {
                    ProductVariantId = request.ProductVariantId,
                    UserId = request.UserId,
                    Rate = request.Rate,
                    Review = request.Review,
                    Status = StatusDM.Active,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = _loginUserDetail.LoginId
                };

                _apiDbContext.ProductRating.Add(rating);
                await _apiDbContext.SaveChangesAsync();

                // 🔹 Save images
                if (request.Images != null && request.Images.Count > 0)
                {
                    foreach (var base64 in request.Images)
                    {
                        var imagePath = await _imageProcess.SaveFromBase64(
                            base64,
                            "jpg",
                            @"wwwroot/content/ratings"
                        );

                        if (!string.IsNullOrEmpty(imagePath))
                        {
                            _apiDbContext.RatingImages.Add(new RatingImagesDM
                            {
                                ProductRatingId = rating.Id,
                                Image = imagePath
                            });
                        }
                    }

                    await _apiDbContext.SaveChangesAsync();
                }

                await transaction.CommitAsync();
                return await GetProductRatingById(rating.Id);
            }
            catch(Exception ex)
            {
                await transaction.RollbackAsync();
                throw new SiffrumException(ApiErrorTypeSM.Fatal_Log, 
                    $"Error in adding product rating by User with UserId:{request.UserId} to ProductVariantId:{request.ProductVariantId}, Reason: {ex.Message}, Inner Exception: {ex.InnerException}, Stack Trace: {ex.StackTrace}"
                    ,"Something went wrong while adding product rating. Please try again.");
            }
        }


        #endregion Add

        #region Update Rating Status

        public async Task<BoolResponseRoot> UpdateProductRatingStatus(long id, StatusSM status)
        {
            var existingRating = await _apiDbContext.ProductRating.FindAsync(id);
            if (existingRating == null)
            {
                throw new SiffrumException(ApiErrorTypeSM.InvalidInputData_Log, "Product Rating not found");
            }
            if(existingRating.Status == (StatusDM)status)
            {
                return new BoolResponseRoot(false, $"Product Rating Status already updated to {status.ToString()}");
            }
            existingRating.Status = (StatusDM)status;
            existingRating.UpdatedAt = DateTime.UtcNow;
            existingRating.UpdatedBy = _loginUserDetail.LoginId;
            if(await _apiDbContext.SaveChangesAsync()>0)
            {
                return new BoolResponseRoot(true, "Product Rating Status updated successfully");
            }
            throw new SiffrumException(ApiErrorTypeSM.Fatal_Log, $"Product Rating with Id:{id} status updation failed to {status.ToString()}","Failed to update product rating");
        }

        #endregion Update

        #region Delete Rating

        public async Task<DeleteResponseRoot> DeleteProductRating(long id)
        {
            using var transaction = await _apiDbContext.Database.BeginTransactionAsync();

            try
            {
                var rating = await _apiDbContext.ProductRating
                    .FirstOrDefaultAsync(x => x.Id == id);

                if (rating == null)
                {
                    throw new SiffrumException(
                        ApiErrorTypeSM.InvalidInputData_Log,
                        "Product rating not found"
                    );
                }

                var images = await _apiDbContext.RatingImages
                    .Where(x => x.ProductRatingId == id)
                    .ToListAsync();

                var imagePaths = images
                    .Where(x => !string.IsNullOrEmpty(x.Image))
                    .Select(x => x.Image!)
                    .ToList();

                // 🔹 Remove DB records
                _apiDbContext.RatingImages.RemoveRange(images);
                _apiDbContext.ProductRating.Remove(rating);

                await _apiDbContext.SaveChangesAsync();
                await transaction.CommitAsync();

                // 🔹 Delete physical files AFTER commit
                foreach (var path in imagePaths)
                {
                    if (File.Exists(path))
                        File.Delete(path);                   
                }

                return new DeleteResponseRoot(true, "Product rating deleted successfully");
            }
            catch(Exception ex)
            {
                await transaction.RollbackAsync();
                throw new SiffrumException(ApiErrorTypeSM.Fatal_Log, $"Failed to delete product rating: {ex.Message}, InnerException: {ex.InnerException}, StackTrace: {ex.StackTrace}", "Failed to delete product rating");
            }
        }


        #endregion Delete Rating

    }
}
