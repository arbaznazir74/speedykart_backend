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
    public class ProductToppingProcess : SiffrumBalBase
    {
        private readonly ILoginUserDetail _loginUserDetail;

        public ProductToppingProcess(IMapper mapper, ApiDbContext apiDbContext,
            ILoginUserDetail loginUserDetail)
            : base(mapper, apiDbContext)
        {
            _loginUserDetail = loginUserDetail;
        }

        #region CREATE
        public async Task<BoolResponseRoot> AddToppingToProduct(ProductToppingSM objSM)
        {
            if (objSM == null)
                throw new SiffrumException(ApiErrorTypeSM.ModelError_NoLog, "Data is required");

            var product = await _apiDbContext.Product.FindAsync(objSM.ProductId);
            if (product == null)
                throw new SiffrumException(ApiErrorTypeSM.InvalidInputData_NoLog, "Product not found");

            var topping = await _apiDbContext.Topping.FindAsync(objSM.ToppingId);
            if (topping == null || topping.Status != StatusDM.Active)
                throw new SiffrumException(ApiErrorTypeSM.InvalidInputData_NoLog, "Topping not found or inactive");

            var exists = await _apiDbContext.ProductTopping
                .AsNoTracking()
                .AnyAsync(x => x.ProductId == objSM.ProductId && x.ToppingId == objSM.ToppingId);

            if (exists)
                throw new SiffrumException(ApiErrorTypeSM.InvalidInputData_NoLog,
                    "This topping is already assigned to this product");

            var dm = new ProductToppingDM
            {
                ProductId = objSM.ProductId,
                ToppingId = objSM.ToppingId,
                Price = objSM.Price,
                IsDefault = objSM.IsDefault,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = _loginUserDetail.LoginId
            };

            await _apiDbContext.ProductTopping.AddAsync(dm);

            if (await _apiDbContext.SaveChangesAsync() > 0)
                return new BoolResponseRoot(true, "Topping added to product successfully");

            throw new SiffrumException(ApiErrorTypeSM.Fatal_Log, "Failed to add topping to product");
        }
        #endregion

        #region READ
        public async Task<List<ProductToppingSM>> GetByProductId(long productId)
        {
            var list = await _apiDbContext.ProductTopping
                .AsNoTracking()
                .Include(x => x.Topping)
                .Include(x => x.Product).ThenInclude(p => p.Seller)
                .Where(x => x.ProductId == productId)
                .OrderBy(x => x.Topping.Name)
                .ToListAsync();

            return list.Select(x => new ProductToppingSM
            {
                Id = x.Id,
                ProductId = x.ProductId,
                ToppingId = x.ToppingId,
                ToppingName = x.Topping?.Name,
                ProductName = x.Product?.Name,
                SellerName = x.Product?.Seller?.StoreName ?? x.Product?.Seller?.Name,
                SellerId = x.Product?.SellerId ?? 0,
                Price = x.Price,
                IsDefault = x.IsDefault,
                CreatedBy = x.CreatedBy,
                CreatedAt = x.CreatedAt
            }).ToList();
        }
        public async Task<List<ProductToppingSM>> GetByToppingId(long toppingId)
        {
            var list = await _apiDbContext.ProductTopping
                .AsNoTracking()
                .Include(x => x.Product).ThenInclude(p => p.Seller)
                .Include(x => x.Topping)
                .Where(x => x.ToppingId == toppingId)
                .OrderBy(x => x.Product.Name)
                .ToListAsync();

            return list.Select(x => new ProductToppingSM
            {
                Id = x.Id,
                ProductId = x.ProductId,
                ToppingId = x.ToppingId,
                ToppingName = x.Topping?.Name,
                ProductName = x.Product?.Name,
                SellerName = x.Product?.Seller?.StoreName ?? x.Product?.Seller?.Name,
                SellerId = x.Product?.SellerId ?? 0,
                Price = x.Price,
                IsDefault = x.IsDefault,
                CreatedBy = x.CreatedBy,
                CreatedAt = x.CreatedAt
            }).ToList();
        }
        #endregion

        #region UPDATE
        public async Task<BoolResponseRoot> UpdateAsync(long id, ProductToppingSM objSM)
        {
            var dm = await _apiDbContext.ProductTopping.FirstOrDefaultAsync(x => x.Id == id);
            if (dm == null)
                throw new SiffrumException(ApiErrorTypeSM.InvalidInputData_NoLog, "Product topping not found");

            dm.Price = objSM.Price;
            dm.IsDefault = objSM.IsDefault;
            dm.UpdatedAt = DateTime.UtcNow;
            dm.UpdatedBy = _loginUserDetail.LoginId;

            await _apiDbContext.SaveChangesAsync();
            return new BoolResponseRoot(true, "Product topping updated");
        }
        #endregion

        #region DELETE
        public async Task<DeleteResponseRoot> RemoveFromProduct(long id)
        {
            var dm = await _apiDbContext.ProductTopping.FirstOrDefaultAsync(x => x.Id == id);
            if (dm == null)
                throw new SiffrumException(ApiErrorTypeSM.InvalidInputData_Log, "Product topping not found");

            _apiDbContext.ProductTopping.Remove(dm);
            await _apiDbContext.SaveChangesAsync();
            return new DeleteResponseRoot(true, "Topping removed from product");
        }
        #endregion
    }
}
