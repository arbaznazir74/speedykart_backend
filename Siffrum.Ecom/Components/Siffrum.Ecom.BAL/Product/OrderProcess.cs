using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Siffrum.Ecom.BAL.Base.ImageProcess;
using Siffrum.Ecom.BAL.Base.OneSignal;
using Siffrum.Ecom.BAL.ExceptionHandler;
using Siffrum.Ecom.BAL.Foundation.Base;
using Siffrum.Ecom.BAL.LoginUsers;
using Siffrum.Ecom.DAL.Context;
using Siffrum.Ecom.DomainModels.Enums;
using Siffrum.Ecom.DomainModels.v1;
using Siffrum.Ecom.ServiceModels.AppUser.Login;
using Siffrum.Ecom.ServiceModels.Enums;
using Siffrum.Ecom.ServiceModels.Foundation.Base.CommonResponseRoot;
using Siffrum.Ecom.ServiceModels.Foundation.Base.Enums;
using Siffrum.Ecom.ServiceModels.Foundation.Base.Interfaces;
using Siffrum.Ecom.ServiceModels.v1;
using System.Drawing;
using System.Text.Json;

namespace Siffrum.Ecom.BAL.Product
{
    public class OrderProcess : SiffrumBalOdataBase<OrderSM>
    {
        private readonly ILoginUserDetail _loginUserDetail;
        private readonly UserAddressProcess _userAddressProcess;
        private readonly NotificationProcess _notificationProcess;
        private readonly ImageProcess _imageProcess;
        private readonly StoreHoursProcess _storeHoursProcess;
        public OrderProcess(IMapper mapper, ApiDbContext apiDbContext,UserAddressProcess userAddressProcess,
            NotificationProcess notificationProcess, ImageProcess imageProcess,
            ILoginUserDetail loginUserDetail, StoreHoursProcess storeHoursProcess)
            : base(mapper, apiDbContext)
        {
            _loginUserDetail = loginUserDetail;
            _imageProcess = imageProcess;
            _userAddressProcess = userAddressProcess;
            _notificationProcess = notificationProcess;
            _storeHoursProcess = storeHoursProcess;
        }

        #region OData
        public override async Task<IQueryable<OrderSM>> GetServiceModelEntitiesForOdata()
        {
            IQueryable<OrderDM> entitySet = _apiDbContext.Order
                .AsNoTracking();

            return await base.MapEntityAsToQuerable<OrderDM, OrderSM>(_mapper, entitySet);
        }
        #endregion

        #region USER - CREATE ORDER       
            

        public async Task<OrderSM> CreateOrderAsync(
    OrderSM orderSM,
    List<OrderItemSM> itemSMs)
        {
            if (itemSMs == null || !itemSMs.Any())
                throw new SiffrumException(
                    ApiErrorTypeSM.InvalidInputData_NoLog,
                    "Order must contain at least one item");
            if (!orderSM.SellerId.HasValue || orderSM.SellerId.Value <= 0)
            {
                var assignedSellerId = await _apiDbContext.User
                    .AsNoTracking()
                    .Where(u => u.Id == orderSM.UserId)
                    .Select(u => u.AssignedSellerId)
                    .FirstOrDefaultAsync();

                if (assignedSellerId.HasValue && assignedSellerId.Value > 0)
                    orderSM.SellerId = assignedSellerId.Value;
            }

            var defaultAddress = await _userAddressProcess.GetDefaultAddress(orderSM.UserId);
            if (defaultAddress == null)
            {
                throw new SiffrumException(ApiErrorTypeSM.Fatal_Log, $"User with Id: {orderSM.UserId} has no default address"
                    , "Please set a default address before placing an order");
            }

            // 🔹 Store hours validation
            if (orderSM.SellerId.HasValue && orderSM.SellerId.Value > 0)
            {
                var availability = await _storeHoursProcess.CheckStoreAvailability(orderSM.SellerId.Value);
                if (!availability.IsOpen)
                    throw new SiffrumException(ApiErrorTypeSM.InvalidInputData_NoLog, availability.Message);
            }

            await using var transaction = await _apiDbContext.Database.BeginTransactionAsync();

            OrderDM orderDM;

            try
            {

                orderDM = _mapper.Map<OrderDM>(orderSM);

                orderDM.OrderNumber = await GenerateUniqueOrderNumber();
                orderDM.TransactionId = await GenerateUniqueTransactionId();
                orderDM.Receipt = $"Receipt#{Guid.NewGuid():N}";
                orderDM.PaymentStatus = PaymentStatusDM.Pending;
                orderDM.OrderStatus = OrderStatusDM.Created;
                orderDM.PaidAmount = 0;
                orderDM.RazorpayOrderId = null;
                orderDM.RazorpayPaymentId = null;
                orderDM.AddressId = defaultAddress.Id;
                orderDM.SellerId = orderSM.SellerId;
                // ✅ Use frontend amount directly
                orderDM.DueAmount = orderDM.Amount;

                await _apiDbContext.Order.AddAsync(orderDM);
                await _apiDbContext.SaveChangesAsync();

                var orderItemDMs = _mapper.Map<List<OrderItemDM>>(itemSMs);

                // Serialize toppings & addons from the request lists into JSON strings for DB
                for (int i = 0; i < orderItemDMs.Count; i++)
                {
                    var src = itemSMs[i];
                    if (src.SelectedToppings != null && src.SelectedToppings.Any())
                        orderItemDMs[i].SelectedToppings = JsonSerializer.Serialize(src.SelectedToppings, _jsonOptions);
                    if (src.SelectedAddons != null && src.SelectedAddons.Any())
                        orderItemDMs[i].SelectedAddons = JsonSerializer.Serialize(src.SelectedAddons, _jsonOptions);
                }

                // 🔹 Stock validation & deduction
                var variantIds = orderItemDMs.Select(x => x.ProductVariantId).Distinct().ToList();
                var variants = await _apiDbContext.ProductVariant
                    .Where(v => variantIds.Contains(v.Id))
                    .ToDictionaryAsync(v => v.Id);

                foreach (var item in orderItemDMs)
                {
                    item.OrderId = orderDM.Id;
                    item.TotalPrice = item.UnitPrice * item.Quantity;

                    if (!variants.TryGetValue(item.ProductVariantId, out var variant))
                        throw new SiffrumException(ApiErrorTypeSM.InvalidInputData_NoLog,
                            "Product not found");

                    if (variant.Stock.HasValue && variant.Stock.Value < item.Quantity)
                        throw new SiffrumException(ApiErrorTypeSM.InvalidInputData_NoLog,
                            $"Only {(int)variant.Stock.Value} in stock for this product");

                    if (variant.Stock.HasValue)
                    {
                        variant.Stock -= item.Quantity;
                        variant.UpdatedAt = DateTime.UtcNow;
                    }
                }

                await _apiDbContext.OrderItem.AddRangeAsync(orderItemDMs);

                var invoice = new InvoiceDM
                {
                    TransactionId = orderDM.TransactionId,
                    InvoiceDate = DateTime.UtcNow,
                    OrderId = orderDM.Id,
                    Currency = orderDM.Currency,
                    Amount = orderDM.Amount, // ✅ frontend amount
                    PaymentStatus = PaymentStatusDM.Pending,
                    OrderStatus = OrderStatusDM.Created
                };

                await _apiDbContext.Invoice.AddAsync(invoice);

                await _apiDbContext.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch (SiffrumException)
            {
                await transaction.RollbackAsync();
                throw;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();

                throw new SiffrumException(
                    ApiErrorTypeSM.Fatal_Log,
                    $"Message: {ex.Message}, InnerException: {ex.InnerException?.Message}",
                    "Failed to create order. Please try again later.");
            }

            // ✅ Notifications (after commit) — notify seller only; delivery boys notified after seller accepts
            try
            {
                var adminNotification = new SendNotificationMessageSM()
                {
                    Title = "New Order Received",
                    Message = $"A new order ({orderDM.OrderNumber}) has been placed. Please review and assign it."
                };
                await _notificationProcess.SendBulkPushNotificationToAdmins(adminNotification);

                if (orderDM.SellerId.HasValue && orderDM.SellerId.Value > 0)
                {
                    var sellerNotification = new SendNotificationMessageSM()
                    {
                        UserIds = new List<long> { orderDM.SellerId.Value },
                        Title = "New Order Received",
                        Message = $"You have received a new order ({orderDM.OrderNumber}). Please accept or reject it."
                    };
                    await _notificationProcess.SendBulkPushNotificationToSellerForOrder(sellerNotification);
                }
            }
            catch
            {
                // Don't fail order if notification fails
            }

            return await GetOrderSM(orderDM);
        }

        #endregion

        #region ORDER MANAGEMENT

        public async Task<List<OrderSM>> GetSellerOrders(long sellerId, int skip, int top)
        {
            var orders = await _apiDbContext.Order
                .AsNoTracking()
                .Where(x => x.SellerId == sellerId)
                .OrderByDescending(x => x.Id)
                .Skip(skip)
                .Take(top)
                .ToListAsync();

            var orderList = new List<OrderSM>();
            foreach (var order in orders)
            {
                var orderSM = await GetOrderSM(order);
                orderList.Add(orderSM);
            }
            return orderList;
        }

        public async Task<IntResponseRoot> GetSellerOrdersCount(long sellerId)
        {
            var count = await _apiDbContext.Order
                .AsNoTracking()
                .CountAsync(x => x.SellerId == sellerId);
            return new IntResponseRoot(count, "Total seller orders");
        }

        public async Task<OrderSM> SellerAcceptOrder(long orderId, long sellerId, int preparationTimeInMinutes = 0)
        {
            var order = await _apiDbContext.Order
                .FirstOrDefaultAsync(x => x.Id == orderId && x.SellerId == sellerId);

            if (order == null)
                throw new SiffrumException(ApiErrorTypeSM.InvalidInputData_NoLog,
                    "Order not found or does not belong to this seller");

            if (order.OrderStatus != OrderStatusDM.Created && order.OrderStatus != OrderStatusDM.Processing)
                throw new SiffrumException(ApiErrorTypeSM.InvalidInputData_NoLog,
                    $"Order cannot be accepted. Current status: {order.OrderStatus}");

            order.OrderStatus = OrderStatusDM.SellerAccepted;
            order.PreparationTimeInMinutes = preparationTimeInMinutes > 0 ? preparationTimeInMinutes : 0;
            order.SellerAcceptedAt = DateTime.UtcNow;
            order.UpdatedAt = DateTime.UtcNow;
            await _apiDbContext.SaveChangesAsync();

            // Notify user
            try
            {
                var user = await _apiDbContext.User.AsNoTracking()
                    .FirstOrDefaultAsync(u => u.Id == order.UserId);
                if (user != null && !string.IsNullOrEmpty(user.FcmId))
                {
                    var prepMsg = preparationTimeInMinutes > 0
                        ? $"Estimated preparation time: {preparationTimeInMinutes} minutes."
                        : "";
                    var userNotification = new SendNotificationMessageSM
                    {
                        Title = "Order Accepted ✅",
                        Message = $"Your order #{order.OrderNumber ?? order.Id.ToString()} has been accepted and is being prepared! {prepMsg}"
                    };
                    await _notificationProcess.SendPushNotificationToUser(userNotification, user.FcmId);
                }
            }
            catch { }

            // Notify seller's own delivery boys
            try
            {
                if (order.SellerId.HasValue && order.SellerId.Value > 0)
                {
                    var dboyNotification = new SendNotificationMessageSM
                    {
                        Title = "New Order Available",
                        Message = $"Order ({order.OrderNumber ?? order.Id.ToString()}) is ready for pickup. Accept now!",
                        AdditionalData = new Dictionary<string, string>
                        {
                            { "orderId", order.Id.ToString() },
                            { "orderNumber", order.OrderNumber ?? "" },
                            { "type", "new_order" }
                        }
                    };
                    await _notificationProcess.SendPushNotificationToSellerDeliveryBoys(
                        dboyNotification, order.SellerId.Value);
                }
            }
            catch { }

            return await GetOrderSM(order);
        }

        public async Task<OrderSM> SellerCancelOrder(long orderId, long sellerId)
        {
            var order = await _apiDbContext.Order
                .FirstOrDefaultAsync(x => x.Id == orderId && x.SellerId == sellerId);

            if (order == null)
                throw new SiffrumException(ApiErrorTypeSM.InvalidInputData_NoLog,
                    "Order not found or does not belong to this seller");

            if (order.OrderStatus == OrderStatusDM.Delivered
                || order.OrderStatus == OrderStatusDM.Cancelled
                || order.OrderStatus == OrderStatusDM.CancelledBySeller)
                throw new SiffrumException(ApiErrorTypeSM.InvalidInputData_NoLog,
                    $"Order cannot be cancelled. Current status: {order.OrderStatus}");

            order.OrderStatus = OrderStatusDM.CancelledBySeller;
            order.UpdatedAt = DateTime.UtcNow;

            // 🔹 Restore stock
            await RestoreStockForOrder(order.Id);

            if (order.PaymentMode == PaymentModeDM.Online && order.PaymentStatus == PaymentStatusDM.Paid)
            {
                order.PaymentStatus = PaymentStatusDM.RefundInitiated;
            }
            else
            {
                order.PaymentStatus = PaymentStatusDM.Cancelled;
            }

            await _apiDbContext.SaveChangesAsync();

            // Notify user about cancellation
            try
            {
                var user = await _apiDbContext.User.AsNoTracking()
                    .FirstOrDefaultAsync(u => u.Id == order.UserId);
                if (user != null && !string.IsNullOrEmpty(user.FcmId))
                {
                    var cancelMsg = order.PaymentMode == PaymentModeDM.Online && order.PaymentStatus == PaymentStatusDM.RefundInitiated
                        ? $"Your order ({order.OrderNumber ?? order.Id.ToString()}) has been cancelled by the seller. Your refund will be initiated shortly."
                        : $"Your order ({order.OrderNumber ?? order.Id.ToString()}) has been cancelled by the seller. No payment is required.";

                    var userNotification = new SendNotificationMessageSM
                    {
                        Title = "Order Cancelled by Seller",
                        Message = cancelMsg
                    };
                    await _notificationProcess.SendPushNotificationToUser(userNotification, user.FcmId);
                }
            }
            catch { }

            return await GetOrderSM(order);
        }

        #endregion

        #region USER - GET MY ORDERS

        public async Task<List<OrderSM>> GetMyOrdersAsync(
            long userId,
            int skip,
            int top)
        {
            var orders = await _apiDbContext.Order
                .AsNoTracking()
                .Where(x => x.UserId == userId)
                .OrderByDescending(x => x.Id)
                .Skip(skip)
                .Take(top)
                .ToListAsync();
            var orderList = new List<OrderSM>();
            foreach(var order in orders)
            {
                var orderSM = await GetOrderSM(order);
                orderList.Add(orderSM);
            }
            return orderList;
        }       

        public async Task<IntResponseRoot> GetMyOrdersCountAsync(
           long userId)
        {
            var count = await _apiDbContext.Order
                .AsNoTracking()
                .Where(x => x.UserId == userId)
                .CountAsync();

            return new IntResponseRoot(count, "Total Orders");
        }

        public async Task<BoolResponseRoot> IsMyFirstOrderApplicable(
          long userId)
        {
            var user = await _apiDbContext.User.FindAsync(userId);
            if(user == null || user?.Status == StatusDM.Inactive)
            {
                throw new SiffrumException(ApiErrorTypeSM.InvalidInputData_NoLog,$"User with Id: {userId} not found or checking delivery applicability", "User not found");
            }
            if (string.IsNullOrEmpty(user.FriendsCode))
            {
                return new BoolResponseRoot(false, "User friends referral code not found");
            }
            var count = await _apiDbContext.Order
                .AsNoTracking()
                .Where(x => x.UserId == userId && x.PaymentStatus == PaymentStatusDM.Paid)
                .CountAsync();
            if(count > 0)
            {
                return new BoolResponseRoot(false, $"Users has already {count} orders");

            }

            return new BoolResponseRoot(true, "Users First Order");
        }

        public async Task<List<OrderSM>> GetUserOrdersByOrderTye(PaymentStatusSM paymentStatus,
            long userId,
            int skip,
            int top)
        {
            var orders = await _apiDbContext.Order
                .AsNoTracking()
                .Where(x => x.UserId == userId && x.OrderStatus == (OrderStatusDM)paymentStatus)
                .OrderByDescending(x => x.CreatedAt)
                .Skip(skip)
                .Take(top)
                .ToListAsync();

            var orderList = new List<OrderSM>();
            foreach (var order in orders)
            {
                var orderSM = await GetOrderSM(order);
                orderList.Add(orderSM);
            }
            return orderList;
        }
        public async Task<IntResponseRoot> GetUserOrdersByOrderTyeCount(PaymentStatusSM paymentStatus,
            long userId)
        {
            var count = await _apiDbContext.Order
                .AsNoTracking()
                .Where(x => x.UserId == userId && x.OrderStatus == (OrderStatusDM)paymentStatus)                
                .CountAsync();

            return new IntResponseRoot(count, "Total User Orders");
        }

        public async Task<List<OrderSM>> SearchOrder(
            long? id,
            PaymentStatusSM? paymentStatus,
            OrderStatusSM? orderStatus,
            int skip,int top)
        {
            IQueryable<OrderDM> query = _apiDbContext.Order
                .AsNoTracking();

            // Filter by Id
            if (id.HasValue && id.Value > 0)
            {
                query = query.Where(x => x.Id == id.Value);
            }

            // Filter by PaymentStatus
            if (paymentStatus.HasValue)
            {
                query = query.Where(x => x.PaymentStatus == (PaymentStatusDM)paymentStatus.Value);
            }

            // Filter by OrderStatus
            if (orderStatus.HasValue)
            {
                query = query.Where(x => x.OrderStatus == (OrderStatusDM)orderStatus.Value);
            }

            var orders = await query
                .OrderByDescending(x => x.CreatedAt)
                .Skip(skip)
                .Take(top)
                .ToListAsync();

            var orderList = new List<OrderSM>();

            foreach (var order in orders)
            {
                var orderSM = await GetOrderSM(order);
                orderList.Add(orderSM);
            }

            return orderList;
        }

        public async Task<IntResponseRoot> SearchOrderCount(
           long? id,
           PaymentStatusSM? paymentStatus,
           OrderStatusSM? orderStatus)
        {
            IQueryable<OrderDM> query = _apiDbContext.Order
                .AsNoTracking();

            // Filter by Id
            if (id.HasValue && id.Value > 0)
            {
                query = query.Where(x => x.Id == id.Value);
            }

            // Filter by PaymentStatus
            if (paymentStatus.HasValue)
            {
                query = query.Where(x => x.PaymentStatus == (PaymentStatusDM)paymentStatus.Value);
            }

            // Filter by OrderStatus
            if (orderStatus.HasValue)
            {
                query = query.Where(x => x.OrderStatus == (OrderStatusDM)orderStatus.Value);
            }

            var count = await query
                .CountAsync();
            return new IntResponseRoot(count, "Total Count");
        }

        public async Task<List<OrderItemSM>> GetOrdersItemsAsync(
            long userId,
            long orderId,
            bool isSuperAdmin,
            int skip,
            int top)
        {
            var query = _apiDbContext.Order
                .AsNoTracking().AsQueryable();
            if (!isSuperAdmin)
            {
                query = query.Where(query => query.UserId == userId);
            }
            var order = await query.FirstOrDefaultAsync(x => x.Id == orderId);
            if(order == null)
            {
                throw new SiffrumException(
                    ApiErrorTypeSM.Fatal_Log,$"User tried to access order with different user or order not found for Id: {orderId}",
                    "Order not found");
            }
            var orderItems = await _apiDbContext.OrderItem.Where(x => x.OrderId == orderId)
                .Skip(skip).Take(top)
                .ToListAsync();
            var orderItemList = new List<OrderItemSM>();
            foreach (var item in orderItems)
            {
                var orderItem = await GetOrderItemSM(item);
                orderItemList.Add(orderItem);
            }
            return orderItemList;

        }

        public async Task<IntResponseRoot> GetOrdersItemsCountAsync(
           long userId,
            long orderId,
            bool isSuperAdmin)
        {
            var query = _apiDbContext.Order
                .AsNoTracking().AsQueryable();
            if (!isSuperAdmin)
            {
                query = query.Where(query => query.UserId == userId);
            }
            var order = await query.FirstOrDefaultAsync(x => x.Id == orderId);
            if (order == null)
            {
                throw new SiffrumException(
                    ApiErrorTypeSM.Fatal_Log, $"User tried to access order with different user or order not found for Id: {orderId}",
                    "Order not found");
            }
            var count = await _apiDbContext.OrderItem.Where(x => x.OrderId == orderId)
                .CountAsync();
            return new IntResponseRoot(count, "Total Orders");
        }
        public async Task<OrderSM> GetOrderByOrderId(
            long orderId)
        {
            var order = await _apiDbContext.Order.FindAsync(orderId);
            if(order == null)
            {
                return null;
            }

            return await GetOrderSM(order);
        }
        public async Task<List<OrderItemSM>> GetOrdersItemByOrderId(
            long orderId)
        {            
            var orderItems = await _apiDbContext.OrderItem.Where(x => x.OrderId == orderId)
                .ToListAsync();

            var orderItemList = new List<OrderItemSM>();
            foreach (var item in orderItems)
            {
                var orderItem = await GetOrderItemSM(item);
                orderItemList.Add(orderItem);
            }
            return orderItemList;
        }

        public async Task<OrderItemSM> GetOrdersItemByOrderItemId(
            long orderItemId)
        {
            var orderItem = await _apiDbContext.OrderItem.FindAsync(orderItemId);

            return await GetOrderItemSM(orderItem);
        }

        public async Task<OrderItemExtendedDetailsSM> GetOrdersItemByOrderItemIdWithStatusDetails(
    long orderItemId)
        {
            var orderItem = await _apiDbContext.OrderItem
                .AsNoTracking()
                .Include(x => x.Order)
                .FirstOrDefaultAsync(x => x.Id == orderItemId);

            if (orderItem == null)
            {
                throw new SiffrumException(
                    ApiErrorTypeSM.InvalidInputData_Log,
                    $"OrderItem with Id {orderItemId} not found",
                    "Order item not found");
            }
            var response = new OrderItemExtendedDetailsSM();

            var sm = await GetOrderItemSM(orderItem);

            response.OrderItem = sm;

            // ✅ Get statuses from Order table
            response.OrderStatus = (OrderStatusSM)orderItem.Order?.OrderStatus;
            response.PaymentStatus = (PaymentStatusSM)orderItem.Order?.PaymentStatus;

            return response;
        }



        public async Task<List<OrderItemSM>> GetOrdersItemsByOrderId(
            long orderId)
        {
            var order = await _apiDbContext.Order.FindAsync(orderId);
            
            if (order == null)
            {
                throw new SiffrumException(
                    ApiErrorTypeSM.Fatal_Log, $"Order not found for Id: {orderId}",
                    "Order not found");
            }
            var orderItems = await _apiDbContext.OrderItem.Where(x => x.OrderId == orderId)
                .ToListAsync();

            var orderItemList = new List<OrderItemSM>();
            foreach (var item in orderItems)
            {
                var orderItem = await GetOrderItemSM(item);
                orderItemList.Add(orderItem);
            }
            return orderItemList;
        }
        public async Task<List<OrderItemSM>> GetSellerOrdersItemsAsync(
            long sellerId,
            int skip,
            int top)
        {
            
            var orderItems = await _apiDbContext.OrderItem.Where(x => x.ProductVariant.Product.SellerId == sellerId)
                .OrderBy(x => x.OrderId)
                .Skip(skip).Take(top)
                .ToListAsync();

            var orderItemList = new List<OrderItemSM>();
            foreach (var item in orderItems)
            {
                var orderItem = await GetOrderItemSM(item);
                orderItemList.Add(orderItem);
            }
            return orderItemList;
        }

        public async Task<IntResponseRoot> GetSellerOrdersItemsCountAsync(
           long sellerId)
        {

            var count = await _apiDbContext.OrderItem
                .Where(x => x.ProductVariant.Product.SellerId == sellerId)
                .CountAsync();
            return new IntResponseRoot(count, "Total Orders");
        }


        public async Task<OrderSM> GetMyOrderByIdAsync(long id, long userId)
        {
            var order = await _apiDbContext.Order
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId);

            if (order == null)
                throw new SiffrumException(
                    ApiErrorTypeSM.InvalidInputData_NoLog,
                    "Order not found");

            return await GetOrderSM(order);
        }

        #endregion

        #region ADMIN - GET ALL

        public async Task<List<OrderSM>> GetAllAsync(int skip, int top)
        {
            var orders = await _apiDbContext.Order
                .AsNoTracking()
                .OrderByDescending(x => x.Id)
                .Skip(skip)
                .Take(top)
                .ToListAsync();

            var orderList = new List<OrderSM>();
            foreach (var order in orders)
            {
                var orderSM = await GetOrderSM(order);
                orderList.Add(orderSM);
            }
            return orderList;
        }

        public async Task<IntResponseRoot> GetCountAsync()
        {
            var count = await _apiDbContext.Order.CountAsync();
            return new IntResponseRoot(count, "Total Orders");
        }

        public async Task<List<OrderSM>> GetAllByOrderStatusAsync(OrderStatusSM status, int skip, int top)
        {
            var orders = await _apiDbContext.Order
                .AsNoTracking()
                .Where(x=>x.OrderStatus == (OrderStatusDM)status)
                .OrderByDescending(x => x.Id)
                .Skip(skip)
                .Take(top)
                .ToListAsync();

            var orderList = new List<OrderSM>();
            foreach (var order in orders)
            {
                var orderSM = await GetOrderSM(order);
                orderList.Add(orderSM);
            }
            return orderList;
        }

        public async Task<IntResponseRoot> GetByOrderStatusCountAsync(OrderStatusSM status)
        {
            var count = await _apiDbContext.Order
                .Where(x => x.OrderStatus == (OrderStatusDM)status)
                .CountAsync();
            return new IntResponseRoot(count, "Total Orders");
        }

        public async Task<List<OrderSM>> GetAllByPaymentStatusAsync(PaymentStatusSM status, int skip, int top)
        {
            var orders = await _apiDbContext.Order
                .AsNoTracking()
                .Where(x => x.PaymentStatus == (PaymentStatusDM)status)
                .OrderByDescending(x => x.Id)
                .Skip(skip)
                .Take(top)
                .ToListAsync();

            var orderList = new List<OrderSM>();
            foreach (var order in orders)
            {
                var orderSM = await GetOrderSM(order);
                orderList.Add(orderSM);
            }
            return orderList;
        }

        public async Task<IntResponseRoot> GetByPaymentStatusCountAsync(PaymentStatusSM status)
        {
            var count = await _apiDbContext.Order
                .Where(x => x.PaymentStatus == (PaymentStatusDM)status)
                .CountAsync();
            return new IntResponseRoot(count, "Total Orders");
        }

        public async Task<OrderSM> GetByIdAsync(long id)
        {
            var order = await _apiDbContext.Order
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id);

            if (order == null)
            {
                return null;
            }

            return await GetOrderSM(order);
        }

        #endregion

        #region UPDATE STATUS (ADMIN)

        public async Task<BoolResponseRoot> UpdateOrderStatusAsync(
            long id,
            OrderStatusSM status)
        {
            var order = await _apiDbContext.Order
                .FirstOrDefaultAsync(x => x.Id == id);

            if (order == null)
                throw new SiffrumException(
                    ApiErrorTypeSM.InvalidInputData_NoLog,
                    "Order not found");

            order.OrderStatus = (OrderStatusDM)status;
            order.UpdatedAt = DateTime.UtcNow;
            order.UpdatedBy = _loginUserDetail.LoginId;
            await _apiDbContext.SaveChangesAsync();

            return new BoolResponseRoot(true, "Order status updated");
        }

        public async Task<BoolResponseRoot> UpdatePaymentStatusAsync(
            long id,
            PaymentStatusSM status)
        {
            var order = await _apiDbContext.Order
                .FirstOrDefaultAsync(x => x.Id == id);

            if (order == null)
                throw new SiffrumException(
                    ApiErrorTypeSM.InvalidInputData_NoLog,
                    "Order not found");

            order.PaymentStatus = (PaymentStatusDM)status;

            if (status == PaymentStatusSM.Paid)
            {
                order.PaidAmount = order.Amount;
                order.DueAmount = 0;
            }
            order.UpdatedAt = DateTime.UtcNow;
            order.UpdatedBy = _loginUserDetail.LoginId;
            await _apiDbContext.SaveChangesAsync();

            return new BoolResponseRoot(true, "Payment status updated");
        }

        #endregion

        #region USER - CANCEL

        public async Task<BoolResponseRoot> CancelOrderAsync(
            long id,
            long userId)
        {
            var order = await _apiDbContext.Order
                .FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId);

            if (order == null)
                throw new SiffrumException(
                    ApiErrorTypeSM.InvalidInputData_NoLog,
                    "Order not found");

            if (order.OrderStatus != OrderStatusDM.Created)
                throw new SiffrumException(
                    ApiErrorTypeSM.InvalidInputData_NoLog,
                    "Order cannot be cancelled at this stage");

            order.OrderStatus = OrderStatusDM.Cancelled;
            order.UpdatedAt = DateTime.UtcNow;
            order.UpdatedBy = _loginUserDetail.LoginId;

            // 🔹 Restore stock
            await RestoreStockForOrder(order.Id);

            await _apiDbContext.SaveChangesAsync();

            return new BoolResponseRoot(true, "Order cancelled successfully");
        }

        #endregion

        #region DELETE (ADMIN)

        public async Task<DeleteResponseRoot> DeleteAsync(long id)
        {
            var order = await _apiDbContext.Order
                .Include(x => x.OrderItems)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (order == null)
                throw new SiffrumException(
                    ApiErrorTypeSM.InvalidInputData_NoLog,
                    "Order not found");

            _apiDbContext.OrderItem.RemoveRange(order.OrderItems);
            _apiDbContext.Order.Remove(order);

            await _apiDbContext.SaveChangesAsync();

            return new DeleteResponseRoot(true, "Order deleted successfully");
        }

        #endregion

        #region Product Availability

        public async Task<BoolResponseRoot> IsProductOrderPossible(long userId, long productVariantId)
        {
            var userDefaultAddress = await _userAddressProcess.GetDefaultAddress(userId);

            if (userDefaultAddress == null )
            {
                throw new SiffrumException(
                    ApiErrorTypeSM.Fatal_Log,
                    $"User with Id: {userId} has no default address",
                    "Please update the default address. Address is required for order");
            }
            var sellerId = await _apiDbContext.ProductVariant
                .AsNoTracking()
                .Where(x => x.Id == productVariantId && x.Status == ProductStatusDM.Active)
                .Select(x => x.Product.SellerId)
                .FirstOrDefaultAsync();

            if (sellerId == 0)
            {
                throw new SiffrumException(
                    ApiErrorTypeSM.InvalidInputData_Log,
                    $"User with Id: {userId} tried to acces product with Id: {productVariantId} which is not found",
                    $"ProductVariant not found");
            }

            return new BoolResponseRoot(true, "Delivery available at your location");
        }

        #endregion Product Availability

        #region Order Address

        public async Task<UserAddressSM> GetOrderAddress(long orderId)
        {
            var order = await _apiDbContext.Order.FindAsync(orderId);
            if (order == null)
            {
                throw new SiffrumException(ApiErrorTypeSM.Fatal_Log, $"Order with Id: {orderId} not found or getting address", "Order not found");
            }
            var address = await _userAddressProcess.GetById((long)order.AddressId);
            return address;
        }

        #endregion Order Address

        #region Get OrderSM

        public async Task<OrderSM> GetOrderSM(OrderDM dm)
        {
            var user = _apiDbContext.User.Where(x => x.Id == dm.UserId).FirstOrDefault();

            string displayName = !string.IsNullOrWhiteSpace(user?.Name)
                ? user.Name
                : !string.IsNullOrWhiteSpace(user?.Username)
                    ? user.Username
                    : user?.Mobile;

            var sm = _mapper.Map<OrderSM>(dm);
            sm.CustomerName = displayName;

            if (dm.PreparationTimeInMinutes > 0 && dm.SellerAcceptedAt.HasValue)
            {
                var deadline = dm.SellerAcceptedAt.Value.AddMinutes(dm.PreparationTimeInMinutes);
                var now = DateTime.UtcNow;

                var delivery = await _apiDbContext.Deliveries
                    .AsNoTracking()
                    .FirstOrDefaultAsync(d => d.OrderId == dm.Id);

                var isPickedUp = false;
                if (delivery != null)
                {
                    isPickedUp = await _apiDbContext.DeliveryStatusHistory
                        .AsNoTracking()
                        .AnyAsync(h => h.DeliveryId == delivery.Id
                            && h.Status == DomainModels.Enums.DeliveryStatusDM.PickedUp);
                }

                if (isPickedUp)
                {
                    sm.PreparationStatus = "PickedUp";
                    sm.PreparationStatusMessage = "Order picked up by delivery partner!";
                }
                else if (now < deadline)
                {
                    sm.PreparationStatus = "Preparing";
                    var remaining = (int)(deadline - now).TotalMinutes;
                    sm.PreparationStatusMessage = $"Your order is being prepared. Estimated {remaining} min remaining.";
                }
                else
                {
                    sm.PreparationStatus = "DeliveryBoyLate";
                    sm.PreparationStatusMessage = "Delivery boy may be running late due to some issues.";
                }
            }

            return sm;
        }

        public async Task<OrderItemSM> GetOrderItemSM(OrderItemDM dm)
        {
            var order = await _apiDbContext.Order.FindAsync(dm.OrderId);
            var user = _apiDbContext.User.AsNoTracking().Where(x => x.Id == order.UserId).FirstOrDefault();

            string displayName = !string.IsNullOrWhiteSpace(user?.Name)
                ? user.Name
                : !string.IsNullOrWhiteSpace(user?.Username)
                    ? user.Username
                    : user?.Mobile;
            var variant = await _apiDbContext.ProductVariant.AsNoTracking().Where(x=>x.Id == dm.ProductVariantId).FirstOrDefaultAsync();
            var sm = _mapper.Map<OrderItemSM>(dm);
            if(variant != null && !string.IsNullOrEmpty(variant.Image))
            {
                var oImg = await _imageProcess.ResolveImage(variant.Image);
                sm.ProductImage = oImg.Base64;
                sm.NetworkProductImage = oImg.NetworkUrl;
            }
            sm.CustomerName = displayName;
            sm.ProductName = variant?.Name;
            sm.OrderStatus = (OrderStatusSM)order.OrderStatus;
            sm.PaymentStatus = (PaymentStatusSM)order.PaymentStatus;
            sm.PaymentMode = (PaymentModeSM)order.PaymentMode;

            // Deserialize toppings & addons from DB JSON strings back to lists
            if (!string.IsNullOrEmpty(dm.SelectedToppings))
            {
                try { sm.SelectedToppings = JsonSerializer.Deserialize<List<SelectedToppingItem>>(dm.SelectedToppings, _jsonOptions); }
                catch { sm.SelectedToppings = null; }
            }
            if (!string.IsNullOrEmpty(dm.SelectedAddons))
            {
                try { sm.SelectedAddons = JsonSerializer.Deserialize<List<SelectedAddonItem>>(dm.SelectedAddons, _jsonOptions); }
                catch { sm.SelectedAddons = null; }
            }

            return sm;
        }

        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public async Task<OrderItemDetailSM> GetOrderItemDetailSM(OrderItemDM dm)
        {
            var order = await _apiDbContext.Order.FindAsync(dm.OrderId);
            var user = _apiDbContext.User.AsNoTracking().Where(x => x.Id == order.UserId).FirstOrDefault();

            string displayName = !string.IsNullOrWhiteSpace(user?.Name)
                ? user.Name
                : !string.IsNullOrWhiteSpace(user?.Username)
                    ? user.Username
                    : user?.Mobile;

            var variant = await _apiDbContext.ProductVariant
                .AsNoTracking()
                .Include(v => v.Product)
                .Where(x => x.Id == dm.ProductVariantId)
                .FirstOrDefaultAsync();

            string imageBase64 = null;
            string networkVariantImage = null;
            if (variant != null && !string.IsNullOrEmpty(variant.Image))
            {
                var oImg = await _imageProcess.ResolveImage(variant.Image);
                imageBase64 = oImg.Base64;
                networkVariantImage = oImg.NetworkUrl;
            }

            // Parse toppings
            List<OrderToppingDetailSM>? toppings = null;
            if (!string.IsNullOrEmpty(dm.SelectedToppings))
            {
                try
                {
                    toppings = JsonSerializer.Deserialize<List<OrderToppingDetailSM>>(dm.SelectedToppings, _jsonOptions);
                }
                catch { toppings = null; }
            }

            // Parse addons
            List<OrderAddonDetailSM>? addons = null;
            if (!string.IsNullOrEmpty(dm.SelectedAddons))
            {
                try
                {
                    addons = JsonSerializer.Deserialize<List<OrderAddonDetailSM>>(dm.SelectedAddons, _jsonOptions);
                }
                catch { addons = null; }
            }

            return new OrderItemDetailSM
            {
                Id = dm.Id,
                OrderId = dm.OrderId,
                CustomerName = displayName,
                ProductId = variant?.ProductId ?? 0,
                BaseProductName = variant?.Product?.Name,
                ProductVariantId = dm.ProductVariantId,
                VariantName = variant?.Name,
                VariantImageBase64 = imageBase64,
                NetworkVariantImage = networkVariantImage,
                Indicator = variant?.Indicator.ToString(),
                Quantity = dm.Quantity,
                UnitPrice = dm.UnitPrice,
                TotalPrice = dm.TotalPrice,
                OrderStatus = (OrderStatusSM)order.OrderStatus,
                PaymentStatus = (PaymentStatusSM)order.PaymentStatus,
                PaymentMode = (PaymentModeSM)order.PaymentMode,
                Toppings = toppings,
                Addons = addons
            };
        }

        public async Task<List<OrderItemDetailSM>> GetOrderItemsDetailAsync(
            long userId,
            long orderId,
            bool isSuperAdmin,
            int skip,
            int top)
        {
            var query = _apiDbContext.Order.AsNoTracking().AsQueryable();
            if (!isSuperAdmin)
            {
                query = query.Where(q => q.UserId == userId);
            }
            var order = await query.FirstOrDefaultAsync(x => x.Id == orderId);
            if (order == null)
            {
                throw new SiffrumException(
                    ApiErrorTypeSM.Fatal_Log, $"User tried to access order with different user or order not found for Id: {orderId}",
                    "Order not found");
            }
            var orderItems = await _apiDbContext.OrderItem.Where(x => x.OrderId == orderId)
                .Skip(skip).Take(top)
                .ToListAsync();
            var result = new List<OrderItemDetailSM>();
            foreach (var item in orderItems)
            {
                result.Add(await GetOrderItemDetailSM(item));
            }
            return result;
        }

        #endregion Get OrderSM

        #region Get Delivery Charges

        public async Task<DeliveryChargeResponseSM> GetDeliveryCharges(long userId)
        {
            var defualtAddress = await _userAddressProcess.GetDefaultAddress(userId);
            if(defualtAddress == null)
            {
                throw new SiffrumException(ApiErrorTypeSM.Fatal_Log, $"User with Id: {userId} has no default address",
                    "Please update the default address. Address is required for order");
            }
            var deliveryCharges = await _apiDbContext.DeliveryPlaces
                .AsNoTracking()
                .Where(x => x.Status == StatusDM.Active)
                .Select(x => x.DeliveryCharges)
                .FirstOrDefaultAsync();
            return new DeliveryChargeResponseSM()
            {
                DeliveryCharge = deliveryCharges
            };
        }

        #endregion Get Delivery Charges

        #region Invoice

        public async Task<OrderInvoiceSM> GenerateInvoice(long orderId, long userId, bool isSuperAdmin)
        {
            var query = _apiDbContext.Order.AsNoTracking().AsQueryable();
            if (!isSuperAdmin)
            {
                query = query.Where(o => o.UserId == userId);
            }
            var order = await query.FirstOrDefaultAsync(o => o.Id == orderId);
            if (order == null)
            {
                throw new SiffrumException(
                    ApiErrorTypeSM.Fatal_Log,
                    $"Order not found for Id: {orderId}",
                    "Order not found");
            }

            // Seller
            InvoiceSellerSM sellerInfo = null;
            if (order.SellerId.HasValue && order.SellerId.Value > 0)
            {
                var seller = await _apiDbContext.Seller.AsNoTracking()
                    .FirstOrDefaultAsync(s => s.Id == order.SellerId.Value);
                if (seller != null)
                {
                    sellerInfo = new InvoiceSellerSM
                    {
                        Id = seller.Id,
                        Name = seller.Name,
                        StoreName = seller.StoreName,
                        Email = seller.Email,
                        Mobile = seller.Mobile,
                        Address = seller.FormattedAddress ?? seller.Street,
                        City = seller.City,
                        State = seller.State,
                        Country = seller.Country,
                        FssaiLicNo = seller.FssaiLicNo,
                        TaxName = seller.TaxName,
                        TaxNumber = seller.TaxNumber
                    };
                }
            }

            // Customer
            var user = await _apiDbContext.User.AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == order.UserId);
            var customerInfo = new InvoiceCustomerSM
            {
                Id = user?.Id ?? 0,
                Name = !string.IsNullOrWhiteSpace(user?.Name) ? user.Name
                     : !string.IsNullOrWhiteSpace(user?.Username) ? user.Username
                     : user?.Mobile,
                Mobile = user?.Mobile,
                Email = user?.Email
            };

            // Delivery address
            InvoiceAddressSM addressInfo = null;
            if (order.AddressId.HasValue && order.AddressId.Value > 0)
            {
                var addr = await _userAddressProcess.GetById(order.AddressId.Value);
                if (addr != null)
                {
                    addressInfo = new InvoiceAddressSM
                    {
                        Name = addr.Name,
                        Mobile = addr.Mobile,
                        Address = addr.Address,
                        Landmark = addr.Landmark,
                        Area = addr.Area,
                        Pincode = addr.Pincode,
                        City = addr.City,
                        State = addr.State,
                        Country = addr.Country
                    };
                }
            }

            // Order items
            var orderItems = await _apiDbContext.OrderItem
                .Where(x => x.OrderId == orderId)
                .ToListAsync();

            var invoiceItems = new List<InvoiceItemSM>();
            decimal subtotal = 0;

            foreach (var item in orderItems)
            {
                var variant = await _apiDbContext.ProductVariant
                    .AsNoTracking()
                    .Include(v => v.Product)
                    .FirstOrDefaultAsync(v => v.Id == item.ProductVariantId);

                // Parse toppings
                List<OrderToppingDetailSM> toppings = null;
                decimal toppingsTotal = 0;
                if (!string.IsNullOrEmpty(item.SelectedToppings))
                {
                    try
                    {
                        toppings = JsonSerializer.Deserialize<List<OrderToppingDetailSM>>(item.SelectedToppings, _jsonOptions);
                        if (toppings != null)
                            toppingsTotal = toppings.Sum(t => t.Price * t.Quantity);
                    }
                    catch { }
                }

                // Parse addons
                List<OrderAddonDetailSM> addons = null;
                decimal addonsTotal = 0;
                if (!string.IsNullOrEmpty(item.SelectedAddons))
                {
                    try
                    {
                        addons = JsonSerializer.Deserialize<List<OrderAddonDetailSM>>(item.SelectedAddons, _jsonOptions);
                        if (addons != null)
                            addonsTotal = addons.Sum(a => a.Price * a.Quantity);
                    }
                    catch { }
                }

                var lineTotal = item.TotalPrice + toppingsTotal + addonsTotal;
                subtotal += lineTotal;

                invoiceItems.Add(new InvoiceItemSM
                {
                    ProductVariantId = item.ProductVariantId,
                    BaseProductName = variant?.Product?.Name,
                    VariantName = variant?.Name,
                    Indicator = variant?.Indicator.ToString(),
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice,
                    TotalPrice = item.TotalPrice,
                    Toppings = toppings,
                    ToppingsTotal = toppingsTotal,
                    Addons = addons,
                    AddonsTotal = addonsTotal,
                    LineTotal = lineTotal
                });
            }

            return new OrderInvoiceSM
            {
                InvoiceNumber = $"INV-{order.OrderNumber}",
                InvoiceDate = order.CreatedAt ?? DateTime.UtcNow,
                OrderId = order.Id,
                OrderNumber = order.OrderNumber,
                Seller = sellerInfo,
                Customer = customerInfo,
                DeliveryAddress = addressInfo,
                Items = invoiceItems,
                Subtotal = subtotal,
                DeliveryCharge = order.DeliveryCharge,
                PlatformCharge = order.PlatormCharge,
                CutleryCharge = order.CutlaryCharge,
                LowCartFeeCharge = order.LowCartFeeCharge,
                TipAmount = order.TipAmount,
                TotalAmount = order.Amount,
                Currency = order.Currency,
                PaymentMode = ((PaymentModeSM)order.PaymentMode).ToString(),
                PaymentStatus = ((PaymentStatusSM)order.PaymentStatus).ToString(),
                OrderStatus = ((OrderStatusSM)order.OrderStatus).ToString()
            };
        }

        #endregion Invoice

        #region Order Number Generator

        private static readonly char[] _orderNumberChars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789".ToCharArray();
        private static readonly Random _rng = new();

        private async Task<string> GenerateUniqueOrderNumber()
        {
            for (int attempt = 0; attempt < 10; attempt++)
            {
                var chars = new char[8];
                lock (_rng)
                {
                    for (int i = 0; i < 8; i++)
                        chars[i] = _orderNumberChars[_rng.Next(_orderNumberChars.Length)];
                }
                var code = new string(chars);
                var exists = await _apiDbContext.Order.AnyAsync(o => o.OrderNumber == code);
                if (!exists) return code;
            }
            // Fallback: timestamp-based
            return DateTime.UtcNow.ToString("yyMMddHH") + Guid.NewGuid().ToString("N")[..4].ToUpper();
        }

        #endregion Order Number Generator

        #region Transaction Id Generator

        private async Task<long> GenerateUniqueTransactionId()
        {
            for (int attempt = 0; attempt < 10; attempt++)
            {
                // 13-digit numeric: timestamp prefix (7) + random suffix (6)
                var tsPart = DateTime.UtcNow.Ticks % 10_000_000;
                long randomPart;
                lock (_rng)
                {
                    randomPart = _rng.Next(100_000, 999_999);
                }
                var txnId = tsPart * 1_000_000 + randomPart;
                var exists = await _apiDbContext.Order.AnyAsync(o => o.TransactionId == txnId);
                if (!exists) return txnId;
            }
            // Fallback: full ticks-based (guaranteed unique in practice)
            return Math.Abs(DateTime.UtcNow.Ticks + Environment.TickCount64);
        }

        #endregion Transaction Id Generator

        #region Seller Earnings

        public async Task<SellerEarningsSM> GetSellerEarningsAsync(long sellerId)
        {
            var today = DateTime.UtcNow.Date;
            var monthStart = new DateTime(today.Year, today.Month, 1, 0, 0, 0, DateTimeKind.Utc);

            var todayEarnings = await _apiDbContext.OrderItem
                .Where(x =>
                    x.ProductVariant.Product.SellerId == sellerId &&
                    x.Order.PaymentStatus == PaymentStatusDM.Paid &&
                    x.Order.CreatedAt >= today)
                .SumAsync(x => (decimal?)x.TotalPrice) ?? 0;

            var todayOrders = await _apiDbContext.OrderItem
                .Where(x =>
                    x.ProductVariant.Product.SellerId == sellerId &&
                    x.Order.CreatedAt >= today)
                .Select(x => x.OrderId)
                .Distinct()
                .CountAsync();

            var monthEarnings = await _apiDbContext.OrderItem
                .Where(x =>
                    x.ProductVariant.Product.SellerId == sellerId &&
                    x.Order.PaymentStatus == PaymentStatusDM.Paid &&
                    x.Order.CreatedAt >= monthStart)
                .SumAsync(x => (decimal?)x.TotalPrice) ?? 0;

            var monthOrders = await _apiDbContext.OrderItem
                .Where(x =>
                    x.ProductVariant.Product.SellerId == sellerId &&
                    x.Order.CreatedAt >= monthStart)
                .Select(x => x.OrderId)
                .Distinct()
                .CountAsync();

            return new SellerEarningsSM
            {
                TodayEarnings = todayEarnings,
                MonthEarnings = monthEarnings,
                TodayOrders = todayOrders,
                MonthOrders = monthOrders
            };
        }

        #endregion Seller Earnings

        #region Stock Helpers

        private async Task RestoreStockForOrder(long orderId)
        {
            var orderItems = await _apiDbContext.OrderItem
                .Where(x => x.OrderId == orderId)
                .ToListAsync();

            var variantIds = orderItems.Select(x => x.ProductVariantId).Distinct().ToList();
            var variants = await _apiDbContext.ProductVariant
                .Where(v => variantIds.Contains(v.Id))
                .ToDictionaryAsync(v => v.Id);

            foreach (var item in orderItems)
            {
                if (variants.TryGetValue(item.ProductVariantId, out var variant) && variant.Stock.HasValue)
                {
                    variant.Stock += item.Quantity;
                    variant.UpdatedAt = DateTime.UtcNow;
                }
            }
        }

        #endregion Stock Helpers
    }
}
