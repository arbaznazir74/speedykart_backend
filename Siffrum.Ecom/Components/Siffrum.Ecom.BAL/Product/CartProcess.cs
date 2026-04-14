using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Siffrum.Ecom.BAL.ExceptionHandler;
using Siffrum.Ecom.BAL.Foundation.Base;
using Siffrum.Ecom.DAL.Context;
using Siffrum.Ecom.DomainModels.Enums;
using Siffrum.Ecom.DomainModels.v1;
using Siffrum.Ecom.ServiceModels.Enums;
using Siffrum.Ecom.ServiceModels.Foundation.Base.Enums;
using Siffrum.Ecom.ServiceModels.Foundation.Base.Interfaces;
using Siffrum.Ecom.ServiceModels.v1;

namespace Siffrum.Ecom.BAL.Product
{
    public class CartProcess : SiffrumBalOdataBase<CartSM>
    {
        private readonly ILoginUserDetail _loginUserDetail;
        private readonly ProductVariantProcess _productVariantProcess;

        public CartProcess(
            IMapper mapper,
            ApiDbContext apiDbContext,
            ProductVariantProcess productVariantProcess,
            ILoginUserDetail loginUserDetail)
            : base(mapper, apiDbContext)
        {
            _loginUserDetail = loginUserDetail;
            _productVariantProcess = productVariantProcess;
        }

        #region ODATA

        public override async Task<IQueryable<CartSM>> GetServiceModelEntitiesForOdata()
        {
            var entitySet = _apiDbContext.Carts.AsNoTracking();
            return await base.MapEntityAsToQuerable<CartDM, CartSM>(_mapper, entitySet);
        }

        #endregion

        #region GET OR CREATE CART

        public async Task<CartDM> GetOrCreateCartInternal(long userId, PlatformTypeDM platform)
        {
            var cart = await _apiDbContext.Carts
                .Include(x => x.Items)
                .FirstOrDefaultAsync(x => x.UserId == userId && x.PlatformType == platform);

            if (cart != null)
                return cart;

            cart = new CartDM
            {
                UserId = userId,
                PlatformType = platform,
                SubTotal = 0,
                TaxAmount = 0,
                DiscountAmount = 0,
                GrandTotal = 0,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = _loginUserDetail.LoginId,
                Items = new List<CartItemDM>()
            };

            await _apiDbContext.Carts.AddAsync(cart);

            try
            {
                await _apiDbContext.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                cart = await _apiDbContext.Carts
                    .Include(x => x.Items)
                    .FirstOrDefaultAsync(x => x.UserId == userId && x.PlatformType == platform);
            }

            return cart;
        }

        #endregion

        #region ADD TO CART

        public async Task<CombineCartSM> AddToCart(long userId, CartRequestSM req)
        {
            var productVariantId = req.ProductVariantId;
            var quantity = req.Quantity;
            var platformSM = req.PlatformType;

            if (quantity < 0)
                throw new SiffrumException(
                    ApiErrorTypeSM.InvalidInputData_NoLog,
                    "Quantity cannot be negative");

            var platform = (PlatformTypeDM)platformSM;

            var product = await _apiDbContext.ProductVariant
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == productVariantId);

            if (product == null)
                throw new SiffrumException(
                    ApiErrorTypeSM.InvalidInputData_NoLog,
                    "Product not found");

            if (product.PlatformType != platform)
                throw new SiffrumException(
                    ApiErrorTypeSM.InvalidInputData_NoLog,
                    "Product does not belong to selected platform");

            var cart = await GetOrCreateCartInternal(userId, platform);

            var existingItem = cart.Items
                .FirstOrDefault(x => x.ProductVariantId == productVariantId);

            // ── Resolve topping & addon prices from DB (server is source of truth) ──
            decimal toppingsTotal = 0;
            decimal addonsTotal = 0;
            string? toppingsJson = null;
            string? addonsJson = null;

            if (req.SelectedToppings != null && req.SelectedToppings.Any())
            {
                var toppingIds = req.SelectedToppings.Select(t => t.ToppingId).ToList();
                var dbToppings = await _apiDbContext.ProductTopping
                    .AsNoTracking()
                    .Where(pt => pt.ProductId == product.ProductId && toppingIds.Contains(pt.ToppingId))
                    .ToDictionaryAsync(pt => pt.ToppingId, pt => pt.Price);

                foreach (var t in req.SelectedToppings)
                {
                    if (dbToppings.TryGetValue(t.ToppingId, out var dbPrice))
                        t.Price = dbPrice;
                    // else: keep client price as fallback (topping might not be linked yet)
                }
                toppingsTotal = req.SelectedToppings.Sum(t => t.Price * t.Quantity);
                toppingsJson = JsonConvert.SerializeObject(req.SelectedToppings);
            }
            if (req.SelectedAddons != null && req.SelectedAddons.Any())
            {
                var addonProductIds = req.SelectedAddons.Select(a => a.AddonProductId).ToList();
                var dbAddonPrices = await _apiDbContext.ProductVariant
                    .AsNoTracking()
                    .Where(pv => addonProductIds.Contains(pv.Id))
                    .ToDictionaryAsync(pv => pv.Id, pv => pv.DiscountedPrice ?? pv.Price);

                foreach (var a in req.SelectedAddons)
                {
                    if (dbAddonPrices.TryGetValue(a.AddonProductId, out var dbPrice))
                        a.Price = dbPrice;
                }
                addonsTotal = req.SelectedAddons.Sum(a => a.Price * a.Quantity);
                addonsJson = JsonConvert.SerializeObject(req.SelectedAddons);
            }

            // If quantity = 0 → remove item
            if (quantity == 0)
            {
                if (existingItem != null)
                    cart.Items.Remove(existingItem);
            }
            else
            {
                if (product.TotalAllowedQuantity < quantity)
                    throw new SiffrumException(
                        ApiErrorTypeSM.InvalidInputData_NoLog,
                        "Stock exceeded");

                var unitPrice = product.DiscountedPrice ?? product.Price;
                var itemTotal = (quantity * unitPrice) + toppingsTotal + addonsTotal;

                if (existingItem != null)
                {
                    existingItem.Quantity = quantity;
                    existingItem.UnitPrice = unitPrice;
                    existingItem.TotalPrice = itemTotal;
                    existingItem.SelectedToppingsJson = toppingsJson;
                    existingItem.SelectedAddonsJson = addonsJson;
                    existingItem.ToppingsTotal = toppingsTotal;
                    existingItem.AddonsTotal = addonsTotal;
                }
                else
                {
                    cart.Items.Add(new CartItemDM
                    {
                        ProductVariantId = productVariantId,
                        Quantity = quantity,
                        UnitPrice = unitPrice,
                        TotalPrice = itemTotal,
                        SelectedToppingsJson = toppingsJson,
                        SelectedAddonsJson = addonsJson,
                        ToppingsTotal = toppingsTotal,
                        AddonsTotal = addonsTotal
                    });
                }
            }

            RecalculateCart(cart);

            await _apiDbContext.SaveChangesAsync();

            return await BuildCombineCartResponse(userId);
        }

        #endregion

        #region UPDATE QUANTITY

        public async Task<CombineCartSM> UpdateCartItemQuantity(
            long userId,
            long productVariantId,
            int quantity,
            PlatformTypeSM platformSM)
        {
            var platform = (PlatformTypeDM)platformSM;

            var cart = await GetOrCreateCartInternal(userId, platform);

            var item = cart.Items
                .FirstOrDefault(x => x.ProductVariantId == productVariantId);

            if (item == null)
                throw new SiffrumException(
                    ApiErrorTypeSM.InvalidInputData_NoLog,
                    "Cart item not found");

            var product = await _apiDbContext.ProductVariant
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == productVariantId);

            if (product == null)
                throw new SiffrumException(
                    ApiErrorTypeSM.InvalidInputData_NoLog,
                    "Product not found");

            if (quantity <= 0)
            {
                cart.Items.Remove(item);
            }
            else
            {
                if (product.TotalAllowedQuantity < quantity)
                    throw new SiffrumException(
                        ApiErrorTypeSM.InvalidInputData_NoLog,
                        "Stock exceeded");

                var currentUnitPrice = product.DiscountedPrice ?? product.Price;
                item.Quantity = quantity;
                item.UnitPrice = currentUnitPrice;
                item.TotalPrice = (quantity * currentUnitPrice) + item.ToppingsTotal + item.AddonsTotal;
            }

            RecalculateCart(cart);

            await _apiDbContext.SaveChangesAsync();

            return await BuildCombineCartResponse(userId);
        }

        #endregion

        #region REMOVE ITEM

        public async Task<CombineCartSM> RemoveFromCart(
            long userId,
            long productVariantId,
            PlatformTypeSM platformSM)
        {
            var platform = (PlatformTypeDM)platformSM;

            var cart = await GetOrCreateCartInternal(userId, platform);

            var item = cart.Items
                .FirstOrDefault(x => x.ProductVariantId == productVariantId);

            if (item != null)
                cart.Items.Remove(item);

            RecalculateCart(cart);

            await _apiDbContext.SaveChangesAsync();

            return await BuildCombineCartResponse(userId);
        }

        #endregion

        #region CLEAR CART

        public async Task<CombineCartSM> ClearCart(
            long userId,
            PlatformTypeSM platformSM)
        {
            var platform = (PlatformTypeDM)platformSM;

            var cart = await GetOrCreateCartInternal(userId, platform);

            _apiDbContext.CartItems.RemoveRange(cart.Items);

            cart.SubTotal = 0;
            cart.TaxAmount = 0;
            cart.DiscountAmount = 0;
            cart.GrandTotal = 0;

            await _apiDbContext.SaveChangesAsync();

            return await BuildCombineCartResponse(userId);
        }

        #endregion

        #region PRIVATE HELPERS

        private void RecalculateCart(CartDM cart)
        {
            cart.SubTotal = cart.Items.Sum(x => x.TotalPrice);

            cart.TaxAmount = Math.Round(cart.SubTotal * 0.05m, 2);

            cart.DiscountAmount = 0;

            cart.GrandTotal = cart.SubTotal + cart.TaxAmount;

            cart.UpdatedAt = DateTime.UtcNow;
            cart.UpdatedBy = _loginUserDetail.LoginId;
        }

        public async Task<CombineCartSM> BuildCombineCartResponse(long userId)
        {
            // 1️⃣ Fetch carts with items
            var carts = await _apiDbContext.Carts
                .AsNoTracking()
                .Include(x => x.Items)
                .Where(x => x.UserId == userId)
                .ToListAsync();

            if (!carts.Any())
            {
                return new CombineCartSM
                {
                    HotBoxCart = null,
                    SpeedyMartCart = null,
                    HotBoxCartItems = new List<HotBoxCartItemSM>(),
                    SpeedyMartCartItems = new List<SpeedyMartCartItemSM>()
                };
            }

            var hotBoxCart = carts.FirstOrDefault(x => x.PlatformType == PlatformTypeDM.HotBox);
            var speedyMartCart = carts.FirstOrDefault(x => x.PlatformType == PlatformTypeDM.SpeedyMart);

            var response = new CombineCartSM
            {
                HotBoxCart = hotBoxCart != null ? _mapper.Map<CartSM>(hotBoxCart) : null,
                SpeedyMartCart = speedyMartCart != null ? _mapper.Map<CartSM>(speedyMartCart) : null,
                HotBoxCartItems = new List<HotBoxCartItemSM>(),
                SpeedyMartCartItems = new List<SpeedyMartCartItemSM>()
            };

            // ==============================
            // 🔥 HOTBOX SECTION
            // ==============================
            if (hotBoxCart?.Items != null && hotBoxCart.Items.Any())
            {
                var hotBoxProductIds = hotBoxCart.Items
                    .Select(x => x.ProductVariantId)
                    .Distinct()
                    .ToList();

                var hotBoxProducts = await _productVariantProcess
                    .GetHotBoxProductsByBanner(hotBoxProductIds);

                var productDictionary = hotBoxProducts
                    .ToDictionary(x => x.Id);

                foreach (var item in hotBoxCart.Items)
                {
                    productDictionary.TryGetValue(item.ProductVariantId, out var productDetails);

                    response.HotBoxCartItems.Add(new HotBoxCartItemSM
                    {
                        Id = item.Id,
                        CartId = item.CartId,
                        ProductVariantId = item.ProductVariantId,
                        HotBoxProductDetails = productDetails,
                        Quantity = item.Quantity,
                        UnitPrice = item.UnitPrice,
                        TotalPrice = item.TotalPrice,
                        SelectedToppings = string.IsNullOrEmpty(item.SelectedToppingsJson)
                            ? null
                            : JsonConvert.DeserializeObject<List<SelectedToppingItem>>(item.SelectedToppingsJson),
                        SelectedAddons = string.IsNullOrEmpty(item.SelectedAddonsJson)
                            ? null
                            : JsonConvert.DeserializeObject<List<SelectedAddonItem>>(item.SelectedAddonsJson),
                        ToppingsTotal = item.ToppingsTotal,
                        AddonsTotal = item.AddonsTotal
                    });
                }
            }

            // ==============================
            // 🛒 SPEEDYMART SECTION
            // ==============================
            if (speedyMartCart?.Items != null && speedyMartCart.Items.Any())
            {
                var speedyProductIds = speedyMartCart.Items
                    .Select(x => x.ProductVariantId)
                    .Distinct()
                    .ToList();

                var speedyProducts = await _productVariantProcess
                    .GetSpeedyMartProductsByBanner(speedyProductIds);

                var productDictionary = speedyProducts
                    .ToDictionary(x => x.Id);

                foreach (var item in speedyMartCart.Items)
                {
                    productDictionary.TryGetValue(item.ProductVariantId, out var productDetails);

                    response.SpeedyMartCartItems.Add(new SpeedyMartCartItemSM
                    {
                        Id = item.Id,
                        CartId = item.CartId,
                        ProductVariantId = item.ProductVariantId,
                        SpeedyMartProductDetails = productDetails, // safe null
                        Quantity = item.Quantity,                  // ✅ FROM CART ITEM
                        UnitPrice = item.UnitPrice,                // ✅ FROM CART ITEM
                        TotalPrice = item.TotalPrice               // ✅ FROM CART ITEM
                    });
                }
            }

            return response;
        }

        #endregion
    }
}
/*
using Siffrum.Ecom.BAL.Foundation.Base;
using Siffrum.Ecom.DAL.Context;
using Siffrum.Ecom.DomainModels.Enums;
using Siffrum.Ecom.DomainModels.v1;
using Siffrum.Ecom.ServiceModels.Enums;
using Siffrum.Ecom.ServiceModels.Foundation.Base.Enums;
using Siffrum.Ecom.ServiceModels.Foundation.Base.Interfaces;
using Siffrum.Ecom.ServiceModels.v1;

namespace Siffrum.Ecom.BAL.Product
{
    public class CartProcess : SiffrumBalOdataBase<CartSM>
    {
        private readonly ILoginUserDetail _loginUserDetail;
        private readonly ProductVariantProcess _productVariantProcess;

        public CartProcess(
            IMapper mapper,
            ApiDbContext apiDbContext,
            ProductVariantProcess productVariantProcess,
            ILoginUserDetail loginUserDetail)
            : base(mapper, apiDbContext)
        {
            _loginUserDetail = loginUserDetail;
            _productVariantProcess = productVariantProcess;
        }

        #region ODATA

        public override async Task<IQueryable<CartSM>> GetServiceModelEntitiesForOdata()
        {
            var entitySet = _apiDbContext.Carts.AsNoTracking();
            return await base.MapEntityAsToQuerable<CartDM, CartSM>(_mapper, entitySet);
        }

        #endregion

        #region GET OR CREATE CART

        public async Task<CartDM> GetOrCreateCartInternal(long userId, PlatformTypeDM platform)
        {
            var cart = await _apiDbContext.Carts
                .Include(x => x.Items)
                .FirstOrDefaultAsync(x =>
                    x.UserId == userId &&
                    x.PlatformType == platform);

            if (cart != null)
                return cart;

            cart = new CartDM
            {
                UserId = userId,
                PlatformType = platform,
                SubTotal = 0,
                TaxAmount = 0,
                DiscountAmount = 0,
                GrandTotal = 0,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = _loginUserDetail.LoginId,
                Items = new List<CartItemDM>()
            };

            await _apiDbContext.Carts.AddAsync(cart);
            await _apiDbContext.SaveChangesAsync();

            return cart;
        }

        #endregion

        #region ADD TO CART

        public async Task<CombineCartSM> AddToCart(
            long userId,
            long productVariantId,
            int quantity,
            PlatformTypeSM platformsm)
        {
            var platform = (PlatformTypeDM)platformsm;
            if (quantity <= 0)
                throw new SiffrumException(
                    ApiErrorTypeSM.InvalidInputData_NoLog,
                    "Quantity must be greater than zero");

            using var transaction = await _apiDbContext.Database.BeginTransactionAsync();

            try
            {
                var cart = await GetOrCreateCartInternal(userId, platform);

                var product = await _apiDbContext.ProductVariant
                    .FirstOrDefaultAsync(x => x.Id == productVariantId);

                if (product == null)
                    throw new SiffrumException(
                        ApiErrorTypeSM.InvalidInputData_NoLog,
                        "Product not found");

                if (product.PlatformType != platform)
                    throw new SiffrumException(
                        ApiErrorTypeSM.InvalidInputData_NoLog,
                        "Product does not belong to selected platform");

                if (product.TotalAllowedQuantity < quantity)
                    throw new SiffrumException(
                        ApiErrorTypeSM.InvalidInputData_NoLog,
                        "Insufficient stock");

                var unitPrice = product.DiscountedPrice ?? product.Price;

                var existingItem = cart.Items
                    .FirstOrDefault(x => x.ProductVariantId == productVariantId);

                if (existingItem != null)
                {
                    existingItem.Quantity += quantity;

                    if (product.TotalAllowedQuantity < existingItem.Quantity)
                        throw new SiffrumException(
                            ApiErrorTypeSM.InvalidInputData_NoLog,
                            "Stock exceeded");

                    existingItem.TotalPrice = existingItem.Quantity * unitPrice;
                }
                else
                {
                    cart.Items.Add(new CartItemDM
                    {
                        ProductVariantId = productVariantId,
                        Quantity = quantity,
                        UnitPrice = unitPrice,
                        TotalPrice = quantity * unitPrice
                    });
                }

                await RecalculateCart(cart);

                await _apiDbContext.SaveChangesAsync();
                await transaction.CommitAsync();

                return await BuildCombineCartResponse(userId);
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        #endregion

        #region UPDATE QUANTITY

        public async Task<CombineCartSM> UpdateCartItemQuantity(
            long userId,
            long productVariantId,
            int quantity,
            PlatformTypeSM platformSM)
        {
            var platform = (PlatformTypeDM)platformSM;
            var cart = await GetOrCreateCartInternal(userId, platform);

            var item = cart.Items
                .FirstOrDefault(x => x.ProductVariantId == productVariantId);

            if (item == null)
                throw new SiffrumException(
                    ApiErrorTypeSM.InvalidInputData_NoLog,
                    "Cart item not found");

            if (quantity <= 0)
            {
                cart.Items.Remove(item);
            }
            else
            {
                item.Quantity = quantity;
                item.TotalPrice = quantity * item.UnitPrice;
            }

            await RecalculateCart(cart);
            await _apiDbContext.SaveChangesAsync();

            return await BuildCombineCartResponse(userId);
        }

        #endregion

        #region REMOVE ITEM

        public async Task<CombineCartSM> RemoveFromCart(
            long userId,
            long productVariantId,
            PlatformTypeSM platformSM)
        {
            var platform = (PlatformTypeDM)platformSM;
            var cart = await GetOrCreateCartInternal(userId, platform);

            var item = cart.Items
                .FirstOrDefault(x => x.ProductVariantId == productVariantId);

            if (item != null)
                cart.Items.Remove(item);

            await RecalculateCart(cart);
            await _apiDbContext.SaveChangesAsync();

            return await BuildCombineCartResponse(userId);
        }

        #endregion

        #region CLEAR CART

        public async Task<CombineCartSM> ClearCart(
            long userId,
            PlatformTypeSM platformSM)
        {
            var platform = (PlatformTypeDM)platformSM;
            var cart = await GetOrCreateCartInternal(userId, platform);

            _apiDbContext.CartItems.RemoveRange(cart.Items);

            cart.SubTotal = 0;
            cart.TaxAmount = 0;
            cart.DiscountAmount = 0;
            cart.GrandTotal = 0;

            await _apiDbContext.SaveChangesAsync();

            return await BuildCombineCartResponse(userId);
        }

        #endregion

        #region PRIVATE HELPERS

        private async Task RecalculateCart(CartDM cart)
        {
            cart.SubTotal = cart.Items.Sum(x => x.TotalPrice);
            cart.TaxAmount = Math.Round(cart.SubTotal * 0.05m, 2); //Handle Tax here
            cart.DiscountAmount = 0;
            cart.GrandTotal = cart.SubTotal + cart.TaxAmount;
            
            cart.UpdatedAt = DateTime.UtcNow;
            cart.UpdatedBy = _loginUserDetail.LoginId;

            await Task.CompletedTask;
        }

        public async Task<CombineCartSM> BuildCombineCartResponse(long userId)
        {
            // 1️⃣ Fetch carts with items
            var carts = await _apiDbContext.Carts
                .AsNoTracking()
                .Include(x => x.Items)
                .Where(x => x.UserId == userId)
                .ToListAsync();

            if (!carts.Any())
            {
                return new CombineCartSM
                {
                    HotBoxCart = null,
                    SpeedyMartCart = null,
                    HotBoxCartItems = new List<HotBoxCartItemSM>(),
                    SpeedyMartCartItems = new List<SpeedyMartCartItemSM>()
                };
            }

            var hotBoxCart = carts.FirstOrDefault(x => x.PlatformType == PlatformTypeDM.HotBox);
            var speedyMartCart = carts.FirstOrDefault(x => x.PlatformType == PlatformTypeDM.SpeedyMart);

            var response = new CombineCartSM
            {
                HotBoxCart = hotBoxCart != null ? _mapper.Map<CartSM>(hotBoxCart) : null,
                SpeedyMartCart = speedyMartCart != null ? _mapper.Map<CartSM>(speedyMartCart) : null,
                HotBoxCartItems = new List<HotBoxCartItemSM>(),
                SpeedyMartCartItems = new List<SpeedyMartCartItemSM>()
            };

            // ==============================
            // 🔥 HOTBOX SECTION
            // ==============================
            if (hotBoxCart?.Items != null && hotBoxCart.Items.Any())
            {
                var hotBoxProductIds = hotBoxCart.Items
                    .Select(x => x.ProductVariantId)
                    .Distinct()
                    .ToList();

                var hotBoxProducts = await _productVariantProcess
                    .GetHotBoxProductsByBanner(hotBoxProductIds);

                var productDictionary = hotBoxProducts
                    .ToDictionary(x => x.Id);

                foreach (var item in hotBoxCart.Items)
                {
                    productDictionary.TryGetValue(item.ProductVariantId, out var productDetails);

                    response.HotBoxCartItems.Add(new HotBoxCartItemSM
                    {
                        Id = item.Id,
                        CartId = item.CartId,
                        ProductVariantId = item.ProductVariantId,
                        HotBoxProductDetails = productDetails,
                        Quantity = item.Quantity,
                        UnitPrice = item.UnitPrice,
                        TotalPrice = item.TotalPrice,
                        SelectedToppings = string.IsNullOrEmpty(item.SelectedToppingsJson)
                            ? null
                            : JsonConvert.DeserializeObject<List<SelectedToppingItem>>(item.SelectedToppingsJson),
                        SelectedAddons = string.IsNullOrEmpty(item.SelectedAddonsJson)
                            ? null
                            : JsonConvert.DeserializeObject<List<SelectedAddonItem>>(item.SelectedAddonsJson),
                        ToppingsTotal = item.ToppingsTotal,
                        AddonsTotal = item.AddonsTotal
                    });
                }
            }

            // ==============================
            // 🛒 SPEEDYMART SECTION
            // ==============================
            if (speedyMartCart?.Items != null && speedyMartCart.Items.Any())
            {
                var speedyProductIds = speedyMartCart.Items
                    .Select(x => x.ProductVariantId)
                    .Distinct()
                    .ToList();

                var speedyProducts = await _productVariantProcess
                    .GetSpeedyMartProductsByBanner(speedyProductIds);

                var productDictionary = speedyProducts
                    .ToDictionary(x => x.Id);

                foreach (var item in speedyMartCart.Items)
                {
                    productDictionary.TryGetValue(item.ProductVariantId, out var productDetails);

                    response.SpeedyMartCartItems.Add(new SpeedyMartCartItemSM
                    {
                        Id = item.Id,
                        CartId = item.CartId,
                        ProductVariantId = item.ProductVariantId,
                        SpeedyMartProductDetails = productDetails, // safe null
                        Quantity = item.Quantity,                  // ✅ FROM CART ITEM
                        UnitPrice = item.UnitPrice,                // ✅ FROM CART ITEM
                        TotalPrice = item.TotalPrice               // ✅ FROM CART ITEM
                    });
                }
            }

            return response;
        }

        #endregion
    }
}*/