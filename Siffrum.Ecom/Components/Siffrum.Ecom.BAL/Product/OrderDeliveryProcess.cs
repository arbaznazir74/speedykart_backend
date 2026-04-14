using AutoMapper; 
using Microsoft.EntityFrameworkCore;
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

namespace Siffrum.Ecom.BAL.Product
{
    public class OrderDeliveryProcess : SiffrumBalBase
    {
        #region Properties

        private readonly ILoginUserDetail _loginUserDetail;
        private readonly OrderProcess _orderProcess;
        private readonly UserAddressProcess _userAddressProcess;
        private readonly NotificationProcess _notificationProcess;

        #endregion

        #region Constructor

        public OrderDeliveryProcess(
            OrderProcess orderProcess, NotificationProcess notificationProcess,
            UserAddressProcess userAddressProcess,
            IMapper mapper,
            ApiDbContext apiDbContext,
            ILoginUserDetail loginUserDetail)
            : base(mapper, apiDbContext)
        {
            _loginUserDetail = loginUserDetail;
            _orderProcess = orderProcess;
            _userAddressProcess = userAddressProcess;
            _notificationProcess = notificationProcess;
        }

        #endregion

        #region Delivery Methods

        #region Assign Delivery

        public async Task<DeliverySM> AssignDelivery(DeliverySM request)
        {
            await using var transaction = await _apiDbContext.Database.BeginTransactionAsync();

            try
            {
                var order = await _apiDbContext.Order
                    .FirstOrDefaultAsync(x => x.Id == request.OrderId);

                if (order == null)
                    throw new SiffrumException(ApiErrorTypeSM.InvalidInputData_Log,
                        $"Order with Id: {request.OrderId} not found",
                        "Order not found");

                /*if (
                    order.OrderStatus == OrderStatusDM.Delivered || order.OrderStatus == OrderStatusDM.Cancelled || order.OrderStatus == OrderStatusDM.Assigned)
                {
                    throw new SiffrumException(ApiErrorTypeSM.InvalidInputData_Log,
                        $"Order {order.Id} cannot be assigned. Current status: {order.OrderStatus}",
                        "Order cannot be assigned for delivery");
                }*/

                var deliveryBoy = await _apiDbContext.DeliveryBoy.AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Id == request.DeliveryBoyId);

                if (deliveryBoy == null)
                    throw new SiffrumException(ApiErrorTypeSM.InvalidInputData_Log,
                        $"Delivery boy with Id {request.DeliveryBoyId} not found",
                        "Delivery boy not found");

                var existingDelivery = await _apiDbContext.Deliveries
                    .FirstOrDefaultAsync(x => x.OrderId == request.OrderId);

                if (existingDelivery != null)
                {
                    if (existingDelivery.DeliveryBoyId == request.DeliveryBoyId)
                        return await GetDeliveryById(existingDelivery.Id);

                    throw new SiffrumException(ApiErrorTypeSM.InvalidInputData_Log,
                        $"Order already assigned to delivery boy {existingDelivery.DeliveryBoyId}",
                        "Delivery already assigned");
                }

                var userDefaultAddress = await _userAddressProcess.GetDefaultAddress(order.UserId);

                if (userDefaultAddress == null)
                    throw new SiffrumException(ApiErrorTypeSM.InvalidInputData_Log,
                        $"User {order.UserId} has no default address",
                        "User order address not found");

                var dm = _mapper.Map<DeliveryDM>(request);
                dm.DeliveryBoyId = request.DeliveryBoyId;
                dm.DeliveryBoy = null;
                dm.Status = DeliveryStatusDM.Assigned;
                dm.AssignedAt = DateTime.UtcNow;
                dm.CreatedAt = DateTime.UtcNow;
                dm.CreatedBy = _loginUserDetail.LoginId;
                dm.PaymentMode = (PaymentModeDM)order.PaymentMode;
                dm.ExpectedDeliveryDate = order.ExpectedDeliveryDate;
                dm.EndLat = userDefaultAddress.Latitude;
                dm.EndLong = userDefaultAddress.Longitude;
                dm.Amount = order.Amount;

                order.OrderStatus = OrderStatusDM.Assigned;

                await _apiDbContext.Deliveries.AddAsync(dm);

                await _apiDbContext.SaveChangesAsync();

                await AddStatusHistory(new DeliveryStatusHistorySM
                {
                    DeliveryId = dm.Id,
                    Status = DeliveryStatusSM.Assigned
                });

                await _apiDbContext.SaveChangesAsync();

                await transaction.CommitAsync();

                // Notify delivery boy
                if (!string.IsNullOrEmpty(deliveryBoy.FcmId))
                {
                    var msg = new SendNotificationMessageSM()
                    {
                        Title = "New Delivery Assigned",
                        Message = "A new order has been assigned to you. Please review the details and proceed with the delivery."
                    };
                    await _notificationProcess.SendPushNotificationByPlayerId(deliveryBoy.FcmId, msg);
                }

                // Notify user about delivery assignment
                try
                {
                    var user = await _apiDbContext.User.AsNoTracking()
                        .FirstOrDefaultAsync(u => u.Id == order.UserId);
                    if (user != null && !string.IsNullOrEmpty(user.FcmId))
                    {
                        await _notificationProcess.SendPushNotificationToUser(
                            new SendNotificationMessageSM
                            {
                                Title = "Delivery Boy Assigned",
                                Message = $"A delivery boy has been assigned to your order #{order.OrderNumber ?? order.Id.ToString()}."
                            }, user.FcmId);
                    }
                }
                catch { }

                // Notify seller about delivery assignment
                try
                {
                    if (order.SellerId.HasValue)
                    {
                        var seller = await _apiDbContext.Seller.AsNoTracking()
                            .FirstOrDefaultAsync(s => s.Id == order.SellerId.Value);
                        if (seller != null && !string.IsNullOrEmpty(seller.FcmId))
                        {
                            await _notificationProcess.SendPushNotificationByPlayerId(seller.FcmId,
                                new SendNotificationMessageSM
                                {
                                    Title = "Delivery Boy Assigned",
                                    Message = $"Delivery boy {deliveryBoy.Name} has been assigned to order #{order.OrderNumber ?? order.Id.ToString()}."
                                });
                        }
                    }
                }
                catch { }

                return await GetDeliveryById(dm.Id);
            }
            catch (Exception ex)
            {
                
                    await transaction.RollbackAsync();
                    throw new SiffrumException(ApiErrorTypeSM.Fatal_Log, $"Error in assigning order Message: {ex.Message}, InnerException: {ex.InnerException}, StackTrace: {ex.StackTrace}",
                        "Something went wrong while assigning delivery");
                
            }
        }

        public async Task<DeliverySM> AcceptOrder(DeliverySM request)
        {
            await using var transaction = await _apiDbContext.Database.BeginTransactionAsync();

            try
            {
                var order = await _apiDbContext.Order
                    .FirstOrDefaultAsync(x => x.Id == request.OrderId);

                if (order == null)
                    throw new SiffrumException(ApiErrorTypeSM.InvalidInputData_Log,
                        $"Order with Id: {request.OrderId} not found",
                        "Order not found");

                var existingDelivery = await _apiDbContext.Deliveries
                    .FirstOrDefaultAsync(x => x.OrderId == request.OrderId);

                if (existingDelivery != null)
                {
                    if (existingDelivery.DeliveryBoyId == request.DeliveryBoyId)
                        return await GetDeliveryById(existingDelivery.Id);

                    throw new SiffrumException(ApiErrorTypeSM.InvalidInputData_Log,
                        $"Order already assigned to delivery boy {existingDelivery.DeliveryBoyId}",
                        "This order has already been claimed by another rider");
                }

                if (order.OrderStatus != OrderStatusDM.Created &&
                    order.OrderStatus != OrderStatusDM.Processing &&
                    order.OrderStatus != OrderStatusDM.SellerAccepted)
                {
                    throw new SiffrumException(ApiErrorTypeSM.InvalidInputData_Log,
                        $"Order {order.Id} cannot be assigned. Current status: {order.OrderStatus}",
                        "Order cannot be assigned for delivery");
                }

                var deliveryBoy = await _apiDbContext.DeliveryBoy
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Id == request.DeliveryBoyId);

                if (deliveryBoy == null)
                    throw new SiffrumException(ApiErrorTypeSM.InvalidInputData_Log,
                        $"Delivery boy with Id {request.DeliveryBoyId} not found",
                        "Delivery boy not found");

                // Seller-managed delivery boys can only accept their seller's orders
                if (deliveryBoy.SellerId.HasValue && deliveryBoy.SellerId.Value > 0)
                {
                    if (order.SellerId != deliveryBoy.SellerId.Value)
                        throw new SiffrumException(ApiErrorTypeSM.InvalidInputData_Log,
                            $"Delivery boy {deliveryBoy.Id} (seller {deliveryBoy.SellerId}) cannot accept order {order.Id} (seller {order.SellerId})",
                            "You can only accept orders from your assigned seller");
                }

                var userDefaultAddress = await _userAddressProcess.GetById((long)order.AddressId);

                if (userDefaultAddress == null)
                    throw new SiffrumException(ApiErrorTypeSM.InvalidInputData_Log,
                        $"User {order.UserId} has no default address",
                        "User order address not found");

                var dm = _mapper.Map<DeliveryDM>(request);
                dm.DeliveryBoyId = request.DeliveryBoyId;
                dm.DeliveryBoy = null;
                dm.Status = DeliveryStatusDM.Assigned;
                dm.AssignedAt = DateTime.UtcNow;
                dm.CreatedAt = DateTime.UtcNow;
                dm.CreatedBy = _loginUserDetail.LoginId;
                dm.PaymentMode = (PaymentModeDM)order.PaymentMode;
                dm.ExpectedDeliveryDate = order.ExpectedDeliveryDate;
                dm.EndLat = userDefaultAddress.Latitude;
                dm.EndLong = userDefaultAddress.Longitude;
                dm.Amount = order.Amount;

                order.OrderStatus = OrderStatusDM.Assigned;

                await _apiDbContext.Deliveries.AddAsync(dm);

                await _apiDbContext.SaveChangesAsync();

                await AddStatusHistory(new DeliveryStatusHistorySM
                {
                    DeliveryId = dm.Id,
                    Status = DeliveryStatusSM.Assigned
                });

                // Mark delivery boy as busy
                deliveryBoy = await _apiDbContext.DeliveryBoy
                    .FirstOrDefaultAsync(x => x.Id == request.DeliveryBoyId);
                if (deliveryBoy != null)
                {
                    deliveryBoy.IsAvailable = 0;
                }

                await _apiDbContext.SaveChangesAsync();

                await transaction.CommitAsync();

                // Notify customer that delivery boy is assigned
                try
                {
                    if (order != null)
                    {
                        var user = await _apiDbContext.User.AsNoTracking()
                            .FirstOrDefaultAsync(u => u.Id == order.UserId);
                        if (user != null && !string.IsNullOrEmpty(user.FcmId))
                        {
                            await _notificationProcess.SendPushNotificationToUser(
                                new SendNotificationMessageSM
                                {
                                    Title = "Delivery Boy Assigned",
                                    Message = "A delivery boy has been assigned to your order and will pick it up soon!"
                                }, user.FcmId);
                        }

                        // Notify seller
                        if (order.SellerId.HasValue && order.SellerId.Value > 0)
                        {
                            var seller = await _apiDbContext.Seller.AsNoTracking()
                                .FirstOrDefaultAsync(s => s.Id == order.SellerId.Value);
                            if (seller != null && !string.IsNullOrEmpty(seller.FcmId))
                            {
                                await _notificationProcess.SendPushNotificationByPlayerId(seller.FcmId,
                                    new SendNotificationMessageSM
                                    {
                                        Title = "Delivery Boy Assigned",
                                        Message = $"Order ({order.OrderNumber}) has been accepted by delivery boy."
                                    });
                            }
                        }
                    }
                }
                catch { }

                return await GetDeliveryById(dm.Id);
            }
            catch (SiffrumException)
            {
                await transaction.RollbackAsync();
                throw;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                throw new SiffrumException(ApiErrorTypeSM.Fatal_Log,$"Error accepting order {request.OrderId} by delivery boy {request.DeliveryBoyId}: {ex.Message}, Inner: {ex.InnerException?.Message}",
                    "Something went wrong while accepting order, please try again");
            }
        }

        #endregion

        #region Get Delivery By Id

        public async Task<DeliverySM> GetDeliveryById(long id)
        {
            var dm = await _apiDbContext.Deliveries.FindAsync(id);
            if(dm == null)
            {
                return null;
            }
            var deliveryBoy = await _apiDbContext.DeliveryBoy.FindAsync(dm.DeliveryBoyId);
            if(deliveryBoy == null)
            {
                throw new SiffrumException(ApiErrorTypeSM.Fatal_Log, $"Delivery boy with id:{dm.DeliveryBoyId} is not found for delivery process", "Delivery boy not found");
            }
            var sm = _mapper.Map<DeliverySM>(dm);
            sm.DeliveryBoyName = deliveryBoy?.Name;
            sm.DeliveryBoyMobie = deliveryBoy?.Mobile;
            return sm;
        }

        public async Task<DeliverySM> GetDeliveryByOrderIdAndDeliveryBoyId(long deliveryBoyId, long id)
        {
            var dm = await _apiDbContext.Deliveries
                .FirstOrDefaultAsync(x => x.OrderId == id && x.DeliveryBoyId == deliveryBoyId);

            if (dm == null)
                throw new SiffrumException(ApiErrorTypeSM.InvalidInputData_Log,
                    $"Order with Id: {id} is not assigned to delivery boy with Id: {deliveryBoyId}",
                    "Orders delivery details not found");

            return _mapper.Map<DeliverySM>(dm);
        }

        #endregion

        #region Update Delivery Status

        public async Task<List<DeliveryStatusHistorySM>> UpdateDeliveryStatus(long deliveryId, DeliveryStatusDM status)
        {
            await using var transaction = await _apiDbContext.Database.BeginTransactionAsync();

            try
            {
                var delivery = await _apiDbContext.Deliveries
                    .FirstOrDefaultAsync(x => x.Id == deliveryId);

                if (delivery == null)
                    throw new SiffrumException(ApiErrorTypeSM.InvalidInputData_Log,
                        $"Delivery with Id: {deliveryId} not found",
                        "Delivery details not found");

                if (delivery.Status == DeliveryStatusDM.Delivered)
                    throw new SiffrumException(ApiErrorTypeSM.InvalidInputData_Log,
                        $"Delivery with Id: {deliveryId} already delivered",
                        "Your order is already delivered");

                delivery.Status = status;
                delivery.UpdatedAt = DateTime.UtcNow;

                if (status == DeliveryStatusDM.Delivered)
                    delivery.DeliveredAt = DateTime.UtcNow;

                {
                    var orderDetails = await _apiDbContext.Order
                        .Include(x => x.Invoices)
                        .FirstOrDefaultAsync(x => x.Id == delivery.OrderId);

                    if (orderDetails != null)
                    {
                        var orderStatus = status switch
                        {
                            DeliveryStatusDM.PickedUp => OrderStatusDM.PickedUp,
                            DeliveryStatusDM.OutForDelivery => OrderStatusDM.OutForDelivery,
                            DeliveryStatusDM.Delivered => OrderStatusDM.Delivered,
                            DeliveryStatusDM.Cancelled => OrderStatusDM.Cancelled,
                            DeliveryStatusDM.Failed => OrderStatusDM.Failed,
                            _ => orderDetails.OrderStatus
                        };

                        orderDetails.OrderStatus = orderStatus;
                        orderDetails.UpdatedAt = DateTime.UtcNow;

                        foreach (var invoice in orderDetails.Invoices)
                        {
                            invoice.OrderStatus = orderStatus;
                            invoice.UpdatedAt = DateTime.UtcNow;
                        }
                    }
                }

                await AddStatusHistory(new DeliveryStatusHistorySM
                {
                    DeliveryId = deliveryId,
                    Status = (DeliveryStatusSM)status
                });

                // Release delivery boy after terminal statuses
                if (status == DeliveryStatusDM.Delivered ||
                    status == DeliveryStatusDM.Cancelled ||
                    status == DeliveryStatusDM.Failed)
                {
                    var dboy = await _apiDbContext.DeliveryBoy
                        .FirstOrDefaultAsync(d => d.Id == delivery.DeliveryBoyId);
                    if (dboy != null)
                    {
                        dboy.IsAvailable = 1;
                    }
                }

                // Mark delivery boy as busy when picked up
                if (status == DeliveryStatusDM.PickedUp || status == DeliveryStatusDM.OutForDelivery)
                {
                    var dboy = await _apiDbContext.DeliveryBoy
                        .FirstOrDefaultAsync(d => d.Id == delivery.DeliveryBoyId);
                    if (dboy != null)
                    {
                        dboy.IsAvailable = 0;
                    }
                }

                // Calculate commission for commission-based delivery boys
                if (status == DeliveryStatusDM.Delivered)
                {
                    var dboy = await _apiDbContext.DeliveryBoy.AsNoTracking()
                        .FirstOrDefaultAsync(d => d.Id == delivery.DeliveryBoyId);

                    if (dboy != null && dboy.PaymentType == DeliveryBoyPaymentTypeDM.CommissionBased)
                    {
                        // Calculate distance from start/end coordinates
                        double distanceKm = 0;
                        if (delivery.StartLat.HasValue && delivery.StartLong.HasValue &&
                            delivery.EndLat.HasValue && delivery.EndLong.HasValue)
                        {
                            distanceKm = CalculateDistanceInKm(
                                delivery.StartLat.Value, delivery.StartLong.Value,
                                delivery.EndLat.Value, delivery.EndLong.Value);
                        }
                        delivery.DistanceInKm = distanceKm;

                        // Get order to resolve seller and delivery charge
                        var orderForCommission = await _apiDbContext.Order.AsNoTracking()
                            .FirstOrDefaultAsync(o => o.Id == delivery.OrderId);

                        if (orderForCommission != null)
                        {
                            var (isFreeDelivery, commissionPerKm) = await ResolveCommissionSettings(orderForCommission.SellerId);

                            if (isFreeDelivery)
                            {
                                delivery.Commission = commissionPerKm * (decimal)distanceKm;
                            }
                            else
                            {
                                delivery.Commission = orderForCommission.DeliveryCharge;
                            }
                        }
                    }
                }

                await _apiDbContext.SaveChangesAsync();
                await transaction.CommitAsync();

                // Notify customer and seller about status change
                try
                {
                    var order = await _apiDbContext.Order.AsNoTracking()
                        .FirstOrDefaultAsync(o => o.Id == delivery.OrderId);
                    if (order != null)
                    {
                        var user = await _apiDbContext.User.AsNoTracking()
                            .FirstOrDefaultAsync(u => u.Id == order.UserId);

                        var statusMessage = status switch
                        {
                            DeliveryStatusDM.PickedUp => "Your order has been picked up by the delivery boy and is on the way!",
                            DeliveryStatusDM.OutForDelivery => "Your order is out for delivery and will reach you soon!",
                            DeliveryStatusDM.Delivered => "Your order has been delivered. Enjoy!",
                            DeliveryStatusDM.Cancelled => "Your delivery has been cancelled.",
                            DeliveryStatusDM.Failed => "Delivery failed. Please contact support.",
                            _ => $"Delivery status updated to {status}."
                        };

                        // Notify customer
                        if (user != null && !string.IsNullOrEmpty(user.FcmId))
                        {
                            var title = status switch
                            {
                                DeliveryStatusDM.PickedUp => "Order Picked Up 📦",
                                DeliveryStatusDM.OutForDelivery => "Out for Delivery 🚀",
                                DeliveryStatusDM.Delivered => "Order Delivered ✅",
                                DeliveryStatusDM.Cancelled => "Delivery Cancelled ❌",
                                DeliveryStatusDM.Failed => "Delivery Failed ⚠️",
                                _ => "Order Update"
                            };
                            await _notificationProcess.SendPushNotificationToUser(
                                new SendNotificationMessageSM
                                {
                                    Title = title,
                                    Message = statusMessage
                                }, user.FcmId);
                        }

                        // Notify seller
                        if (order.SellerId.HasValue && order.SellerId.Value > 0)
                        {
                            var seller = await _apiDbContext.Seller.AsNoTracking()
                                .FirstOrDefaultAsync(s => s.Id == order.SellerId.Value);
                            if (seller != null && !string.IsNullOrEmpty(seller.FcmId))
                            {
                                var sellerMsg = status switch
                                {
                                    DeliveryStatusDM.PickedUp => $"Order ({order.OrderNumber}) picked up by delivery boy.",
                                    DeliveryStatusDM.Delivered => $"Order ({order.OrderNumber}) has been delivered successfully.",
                                    _ => $"Order ({order.OrderNumber}) delivery status: {status}."
                                };
                                await _notificationProcess.SendPushNotificationByPlayerId(seller.FcmId,
                                    new SendNotificationMessageSM
                                    {
                                        Title = "Delivery Update",
                                        Message = sellerMsg
                                    });
                            }
                        }
                    }
                }
                catch { }

                var history = await _apiDbContext.DeliveryStatusHistory
                    .Where(x => x.DeliveryId == deliveryId)
                    .OrderByDescending(x => x.CreatedAt)
                    .ToListAsync();

                return _mapper.Map<List<DeliveryStatusHistorySM>>(history);
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<List<DeliveryStatusHistorySM>> UpdateDeliveryStatusByOrder(long orderId, long deliveryBoyId, DeliveryStatusDM status)
        {
            var delivery = await _apiDbContext.Deliveries
                .FirstOrDefaultAsync(d => d.OrderId == orderId && d.DeliveryBoyId == deliveryBoyId);

            if (delivery == null)
                throw new SiffrumException(ApiErrorTypeSM.InvalidInputData_Log,
                    $"No delivery found for order {orderId} assigned to delivery boy {deliveryBoyId}",
                    "Delivery not found for this order");

            return await UpdateDeliveryStatus(delivery.Id, status);
        }

        #endregion

        #region Delivered product

        public async Task<BoolResponseRoot> DeliverOrder(long orderId, long deliveryBoyId)
        {
            await using var transaction = await _apiDbContext.Database.BeginTransactionAsync();

            try
            {
                // 🔹 Get Order
                var order = await _apiDbContext.Order
                    .Include(x => x.Invoices)
                    .FirstOrDefaultAsync(x => x.Id == orderId);

                if (order == null)
                    throw new SiffrumException(ApiErrorTypeSM.InvalidInputData_Log,
                        $"Order with Id: {orderId} not found",
                        "Order not found");

                // 🔹 Get Delivery Boy
                var deliveryBoy = await _apiDbContext.DeliveryBoy
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Id == deliveryBoyId);

                if (deliveryBoy == null)
                    throw new SiffrumException(ApiErrorTypeSM.InvalidInputData_Log,
                        $"Delivery boy with Id {deliveryBoyId} not found",
                        "Delivery boy not found");

                // 🔹 Check Assignment
                var existingDelivery = await _apiDbContext.Deliveries
                    .FirstOrDefaultAsync(x => x.OrderId == orderId && x.DeliveryBoyId == deliveryBoyId);

                if (existingDelivery == null)
                    throw new SiffrumException(ApiErrorTypeSM.InvalidInputData_Log,
                        $"Order {orderId} is not assigned to delivery boy {deliveryBoyId}",
                        "This order has not been assigned to you");

                // 🔒 Prevent invalid states
                if (existingDelivery.Status == DeliveryStatusDM.Delivered)
                    throw new SiffrumException(ApiErrorTypeSM.InvalidInputData_Log,
                        $"Order {orderId} already delivered",
                        "Order already delivered");

                if (existingDelivery.Status == DeliveryStatusDM.Cancelled ||
                    existingDelivery.Status == DeliveryStatusDM.Failed)
                    throw new SiffrumException(ApiErrorTypeSM.InvalidInputData_Log,
                        $"Order {orderId} cannot be delivered from {existingDelivery.Status}",
                        "Invalid delivery state");

                // 🔹 Update Delivery
                existingDelivery.Status = DeliveryStatusDM.Delivered;
                existingDelivery.DeliveredAt = DateTime.UtcNow;
                existingDelivery.UpdatedAt = DateTime.UtcNow;

                // 🔹 Add Status History
                await AddStatusHistory(new DeliveryStatusHistorySM
                {
                    DeliveryId = existingDelivery.Id,
                    Status = DeliveryStatusSM.Delivered
                });

                // 🔹 COD Payment Handling (Full Payment)
                order.PaidAmount = order.Amount;
                order.DueAmount = 0;
                order.PaymentStatus = PaymentStatusDM.Paid;
                order.OrderStatus = OrderStatusDM.Delivered;
                order.UpdatedAt = DateTime.UtcNow;
                order.UpdatedBy = _loginUserDetail.LoginId;

                // 🔹 Update Invoices
                foreach (var invoice in order.Invoices)
                {
                    invoice.PaymentStatus = PaymentStatusDM.Paid;
                    invoice.OrderStatus = OrderStatusDM.Delivered;
                    invoice.UpdatedAt = DateTime.UtcNow;
                }

                // 🔹 Commission Logic for commission-based delivery boys
                if (deliveryBoy.PaymentType == DeliveryBoyPaymentTypeDM.CommissionBased)
                {
                    double distanceKm = 0;
                    if (existingDelivery.StartLat.HasValue && existingDelivery.StartLong.HasValue &&
                        existingDelivery.EndLat.HasValue && existingDelivery.EndLong.HasValue)
                    {
                        distanceKm = CalculateDistanceInKm(
                            existingDelivery.StartLat.Value, existingDelivery.StartLong.Value,
                            existingDelivery.EndLat.Value, existingDelivery.EndLong.Value);
                    }
                    existingDelivery.DistanceInKm = distanceKm;

                    var (isFreeDelivery, commissionPerKm) = await ResolveCommissionSettings(order.SellerId);

                    if (isFreeDelivery)
                    {
                        // Free delivery: commission = commissionPerKm × distance
                        existingDelivery.Commission = commissionPerKm * (decimal)distanceKm;
                    }
                    else
                    {
                        // Normal delivery: commission = order's delivery charge
                        existingDelivery.Commission = order.DeliveryCharge;
                    }
                }

                // 🔹 Transaction record
                    var creditAmount = new DeliveryBoyOrderTransactionsDM
                    {
                        Amount = order.Amount,
                        DeliveryBoyId = deliveryBoyId,
                        OrderId = orderId,
                        PaymentType = DeliveryOrderPaymentTypeDM.OrderReceivedAmount,
                        TransactionDate = DateTime.UtcNow,
                        CreatedAt = DateTime.UtcNow,
                        CreatedBy = _loginUserDetail.LoginId
                    };

                    await _apiDbContext.DeliveryBoyOrderTransactions.AddAsync(creditAmount);

                // Release delivery boy so they can accept new orders
                var dBoyTracked = await _apiDbContext.DeliveryBoy
                    .FirstOrDefaultAsync(x => x.Id == deliveryBoyId);
                if (dBoyTracked != null)
                {
                    dBoyTracked.IsAvailable = 1;
                }

                await _apiDbContext.SaveChangesAsync();
                await transaction.CommitAsync();

                // Notify customer
                try
                {
                    var user = await _apiDbContext.User.AsNoTracking()
                        .FirstOrDefaultAsync(u => u.Id == order.UserId);
                    if (user != null && !string.IsNullOrEmpty(user.FcmId))
                    {
                        var isCod = order.PaymentMode == PaymentModeDM.CashOnDelivery;
                        await _notificationProcess.SendPushNotificationToUser(
                            new SendNotificationMessageSM
                            {
                                Title = "Order Delivered!",
                                Message = isCod
                                    ? "Your order has been delivered and cash has been collected. Thank you!"
                                    : "Your order has been delivered successfully. Enjoy!"
                            }, user.FcmId);
                    }
                }
                catch { }

                // Notify seller
                try
                {
                    if (order.SellerId.HasValue && order.SellerId.Value > 0)
                    {
                        var seller = await _apiDbContext.Seller.AsNoTracking()
                            .FirstOrDefaultAsync(s => s.Id == order.SellerId.Value);
                        if (seller != null && !string.IsNullOrEmpty(seller.FcmId))
                        {
                            var isCod = order.PaymentMode == PaymentModeDM.CashOnDelivery;
                            var msg = isCod
                                ? $"Order ({order.OrderNumber}) delivered. COD amount {order.Amount:C} collected by delivery boy."
                                : $"Order ({order.OrderNumber}) has been delivered successfully.";
                            await _notificationProcess.SendPushNotificationByPlayerId(seller.FcmId,
                                new SendNotificationMessageSM
                                {
                                    Title = "Order Delivered",
                                    Message = msg
                                });
                        }
                    }
                }
                catch { }

                return new BoolResponseRoot(true, "Order delivered successfully");
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
                    $"Error while delivering order with Id:{orderId}, Message: {ex?.Message}, InnerException:{ex?.InnerException?.Message}, StackTrace:{ex?.StackTrace}",
                    "Something went wrong while delivering the order");
            }
        }
        #endregion Delivered product

        #endregion

        #region Delivery Status

        private async Task AddStatusHistory(DeliveryStatusHistorySM sm)
        {
            var history = new DeliveryStatusHistoryDM
            {
                DeliveryId = sm.DeliveryId,
                Status = (DeliveryStatusDM)sm.Status,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = _loginUserDetail.LoginId
            };

            await _apiDbContext.DeliveryStatusHistory.AddAsync(history);
        }

        public async Task<List<DeliveryStatusHistorySM>> GetDeliveryStatuses(long orderId, long userId)
        {
            var existingOrder = await _orderProcess.GetByIdAsync(orderId);

            if (existingOrder == null || existingOrder.UserId != userId)
                throw new SiffrumException(ApiErrorTypeSM.InvalidInputData_Log,
                    $"Order with Id: {orderId} not found for the user with UserId: {userId}",
                    "Order not found");

            var deliveryDetails = await _apiDbContext.Deliveries
                .FirstOrDefaultAsync(x => x.OrderId == orderId);

            if (deliveryDetails == null)
                return new List<DeliveryStatusHistorySM>();
            
            var history = await _apiDbContext.DeliveryStatusHistory
                .Where(x => x.DeliveryId == deliveryDetails.Id)
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();
            var result = _mapper.Map<List<DeliveryStatusHistorySM>>(history);

            return result;
        }

        public async Task<DeliveryBoySM> GetDeliveryBoyDetails(long orderId, long userId)
        {
            var existingOrder = await _orderProcess.GetByIdAsync(orderId);

            if (existingOrder == null)
                throw new SiffrumException(ApiErrorTypeSM.InvalidInputData_Log,
                    $"Order with Id: {orderId} not found for the user with UserId: {userId}",
                    "Order not found");

            var deliveryDetails = await _apiDbContext.Deliveries
                .FirstOrDefaultAsync(x => x.OrderId == orderId);

            if (deliveryDetails == null)
            {
                return null;
            }

            var dBoy = await _apiDbContext.DeliveryBoy.FindAsync(deliveryDetails.DeliveryBoyId);
            if (dBoy == null)
            {
                return null;
            }

            return _mapper.Map<DeliveryBoySM>(dBoy);
        }

        public async Task<List<DeliveryStatusHistorySM>> GetDeliveryStatusesOfOrderId(long orderId)
        {
            var deliveryDetails = await _apiDbContext.Deliveries
                .FirstOrDefaultAsync(x => x.OrderId == orderId);

            if (deliveryDetails == null)
                return new List<DeliveryStatusHistorySM>();

            var history = await _apiDbContext.DeliveryStatusHistory
                .Where(x => x.DeliveryId == deliveryDetails.Id)
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();

            return _mapper.Map<List<DeliveryStatusHistorySM>>(history);
        }

        #endregion

        #region Tracking Location

        public async Task<DeliveryTrackingSM> UpdateLocation(DeliveryTrackingSM sm)
        {
            if (sm == null)
                throw new SiffrumException(ApiErrorTypeSM.ModelError_NoLog,
                    "Delivery tracking data is required");

            var delivery = await _apiDbContext.Deliveries
                .FirstOrDefaultAsync(x => x.Id == sm.DeliveryId);

            if (delivery == null)
                throw new SiffrumException(ApiErrorTypeSM.InvalidInputData_Log,
                    $"Delivery with Id {sm.DeliveryId} not found");

            // Prevent location updates after completion
            if (delivery.Status == DeliveryStatusDM.Delivered)
                throw new SiffrumException(ApiErrorTypeSM.InvalidInputData_Log,
                    $"Delivery {sm.DeliveryId} is already delivered",
                    "Cannot update location after delivery is completed");

            if (delivery.Status == DeliveryStatusDM.Cancelled ||
                delivery.Status == DeliveryStatusDM.Failed)
                throw new SiffrumException(ApiErrorTypeSM.InvalidInputData_Log,
                    $"Delivery {sm.DeliveryId} is no longer active. Current status: {delivery.Status}",
                    "Cannot update location for inactive delivery");

            var dm = _mapper.Map<DeliveryTrackingDM>(sm);

            dm.CreatedAt = DateTime.UtcNow;
            dm.CreatedBy = _loginUserDetail.LoginId;

            await _apiDbContext.DeliveryTracking.AddAsync(dm);

            await _apiDbContext.SaveChangesAsync();

            return await GetTrackingDataById(dm.Id);
        }
        
        public async Task<DeliveryTrackingSM> GetLatestLocation(long deliveryId)
        {
            var dm = await _apiDbContext.DeliveryTracking
                .Where(x => x.DeliveryId == deliveryId)
                .OrderByDescending(x => x.CreatedAt)
                .FirstOrDefaultAsync();

            if (dm == null)
                return null;
            return _mapper.Map<DeliveryTrackingSM>(dm);
        }

        public async Task<List<DeliveryTrackingSM>> GetUsersOrderTracking(long userId, long orderId)
        {
            var existingOrder = await _orderProcess.GetByIdAsync(orderId);

            if (existingOrder == null || existingOrder.UserId != userId)
                throw new SiffrumException(ApiErrorTypeSM.InvalidInputData_Log,
                    $"Order with Id: {orderId} not found for the user with UserId: {userId}",
                    "Order not found");
            
            var delivery = await _apiDbContext.Deliveries
                .Where(x => x.OrderId == orderId)
                .FirstOrDefaultAsync();

            if (delivery == null)
                return new List<DeliveryTrackingSM>();
            
            var tracking = await _apiDbContext.DeliveryTracking
                .Where(x => x.DeliveryId == delivery.Id)
                .OrderBy(x => x.CreatedAt)
                .ToListAsync();
            var result = _mapper.Map<List<DeliveryTrackingSM>>(tracking);

            return result;
            //return _mapper.Map<List<DeliveryTrackingSM>>(tracking);
        }

        public async Task<DeliveryTrackingSM> GetTrackingDataById(long id)
        {
            var dm = await _apiDbContext.DeliveryTracking.FindAsync(id);
            return _mapper.Map<DeliveryTrackingSM>(dm);
        }

        #endregion

        #region Delivery Boy Orders

        public async Task<List<DeliverySM>> GetDeliveryBoyOrders(long deliveryBoyId, DeliveryBoyOrderRequestSM sm, int skip, int top)
        {
            var query = _apiDbContext.Deliveries
                .Where(x => x.DeliveryBoyId == deliveryBoyId);

            if (sm.DeliveryStatus.HasValue)
                query = query.Where(x => x.Status == (DeliveryStatusDM)sm.DeliveryStatus.Value);

            if (sm.StartDate.HasValue && sm.EndDate.HasValue)
                query = query.Where(x => x.AssignedAt >= sm.StartDate.Value &&
                                         x.AssignedAt <= sm.EndDate.Value);

            var dms = await query
                .OrderByDescending(x => x.AssignedAt)
                .Skip(skip)
                .Take(top)
                .ToListAsync();

            return _mapper.Map<List<DeliverySM>>(dms);
        }


        public async Task<IntResponseRoot> GetDeliveryBoyOrdersCount(long deliveryBoyId, DeliveryBoyOrderRequestSM sm)
        {
            var query = _apiDbContext.Deliveries
                .Where(x => x.DeliveryBoyId == deliveryBoyId);

            if (sm.DeliveryStatus.HasValue)
                query = query.Where(x => x.Status == (DeliveryStatusDM)sm.DeliveryStatus.Value);

            if (sm.StartDate.HasValue && sm.EndDate.HasValue)
                query = query.Where(x => x.AssignedAt >= sm.StartDate.Value &&
                                         x.AssignedAt <= sm.EndDate.Value);

            var count = await query.CountAsync();

            return new IntResponseRoot(count, "Total Orders");
        }

        public async Task<List<DeliverySM>> GetAllDeliveryBoyOrders(long deliveryBoyId, int skip, int top)
        {
            var query = _apiDbContext.Deliveries
                .Where(x => x.DeliveryBoyId == deliveryBoyId);

            var dms = await query
                .OrderByDescending(x => x.AssignedAt)
                .Skip(skip)
                .Take(top)
                .ToListAsync();

            return _mapper.Map<List<DeliverySM>>(dms);
        }

        public async Task<IntResponseRoot> GetAllDeliveryBoyOrdersCount(long deliveryBoyId)
        {
            var count = await _apiDbContext.Deliveries
                .Where(x => x.DeliveryBoyId == deliveryBoyId)
                .CountAsync();

            return new IntResponseRoot(count, "Total Orders");
        }

        public async Task<List<OrderSM>> GetAvailableOrdersForDeliveryBoy(long deliveryBoyId, int skip, int top)
        {
            var dboy = await _apiDbContext.DeliveryBoy
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.Id == deliveryBoyId);

            if (dboy == null || !dboy.SellerId.HasValue)
                return new List<OrderSM>();

            var assignedOrderIds = _apiDbContext.Deliveries
                .Select(d => d.OrderId);

            var orders = await _apiDbContext.Order
                .Where(o => o.SellerId == dboy.SellerId.Value
                    && o.OrderStatus == DomainModels.Enums.OrderStatusDM.SellerAccepted
                    && !assignedOrderIds.Contains(o.Id))
                .OrderByDescending(o => o.CreatedAt)
                .Skip(skip)
                .Take(top)
                .ToListAsync();

            var result = new List<OrderSM>();
            foreach (var order in orders)
            {
                var sm = _mapper.Map<OrderSM>(order);
                result.Add(sm);
            }
            return result;
        }

        public async Task<IntResponseRoot> GetAvailableOrdersCountForDeliveryBoy(long deliveryBoyId)
        {
            var dboy = await _apiDbContext.DeliveryBoy
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.Id == deliveryBoyId);

            if (dboy == null || !dboy.SellerId.HasValue)
                return new IntResponseRoot(0, "No available orders");

            var assignedOrderIds = _apiDbContext.Deliveries
                .Select(d => d.OrderId);

            var count = await _apiDbContext.Order
                .Where(o => o.SellerId == dboy.SellerId.Value
                    && o.OrderStatus == DomainModels.Enums.OrderStatusDM.SellerAccepted
                    && !assignedOrderIds.Contains(o.Id))
                .CountAsync();

            return new IntResponseRoot(count, "Available Orders");
        }

        public async Task<OrderSM> GetDeliveryBoyOrderById(long deliveryBoyId, long orderId)
        {
            var isOrderAssigned = await IsOrderAssignedToDeliveryBoy(deliveryBoyId, orderId);

            if (isOrderAssigned)
                return await _orderProcess.GetByIdAsync(orderId);

            throw new SiffrumException(ApiErrorTypeSM.InvalidInputData_Log,
                $"Order with Id: {orderId} is not assigned to delivery boy with Id: {deliveryBoyId}");
        }

        public async Task<List<OrderItemSM>> GetDeliveryBoyOrderItemByOrderId(long deliveryBoyId, long orderId)
        {
            var isOrderAssigned = await IsOrderAssignedToDeliveryBoy(deliveryBoyId, orderId);

            if (isOrderAssigned)
                return await _orderProcess.GetOrdersItemsByOrderId(orderId);

            throw new SiffrumException(ApiErrorTypeSM.InvalidInputData_Log,
                $"Order with Id: {orderId} is not assigned to delivery boy with Id: {deliveryBoyId}");
        }

        public async Task<bool> IsOrderAssignedToDeliveryBoy(long deliveryBoyId, long orderId)
        {
            return await _apiDbContext.Deliveries
                .AnyAsync(x => x.DeliveryBoyId == deliveryBoyId && x.OrderId == orderId);
        }

        public async Task<DeliveryBoyOrderDetailsSM> OrderDetailsForDeliveryBoy(long deliveryBoyId, long orderId)
        {
            var order = await GetDeliveryBoyOrderById(deliveryBoyId, orderId);
            var orderItems = await GetDeliveryBoyOrderItemByOrderId(deliveryBoyId, orderId);
            var deliveryDetails = await GetDeliveryByOrderIdAndDeliveryBoyId(deliveryBoyId, orderId);
            var address = await _userAddressProcess.GetById((long)order.AddressId);

            var mobileNumber = new[] { address?.Mobile, address?.AlternateMobile }
                .FirstOrDefault(x => !string.IsNullOrWhiteSpace(x));

            if (string.IsNullOrWhiteSpace(mobileNumber))
            {
                mobileNumber = await _apiDbContext.User
                    .Where(x => x.Id == order.UserId)
                    .Select(x => x.Mobile)
                    .FirstOrDefaultAsync();
            }

            return new DeliveryBoyOrderDetailsSM
            {
                OrderDetails = order,
                OrderItems = orderItems,
                DeliveryDetails = deliveryDetails,
                UserMobile = mobileNumber
            };
        }

        #endregion

        #region Delivery Boy Earnings

        public async Task<DeliveryBoyEarningSM> GetDeliveryBoyEarningsByTip(long deliveryBoyId)
        {
            var dBoy = await _apiDbContext.DeliveryBoy.FindAsync(deliveryBoyId);
            if (dBoy == null)
            {
                throw new SiffrumException(ApiErrorTypeSM.Fatal_Log, $"Delivery boy with id: {deliveryBoyId} not found for getting Tip amount earnings", "Delivery Boy Not Found");
            }
            if (dBoy.PaymentType == DeliveryBoyPaymentTypeDM.CommissionBased)
            {
                return new DeliveryBoyEarningSM()
                {
                    TotalEarnings = 0
                };
            }
            var orderIds = await _apiDbContext.Deliveries
                .Where(x => x.DeliveryBoyId == deliveryBoyId).Select(x=>x.OrderId).Distinct().ToListAsync();
            if (!orderIds.Any())
            {
                return new DeliveryBoyEarningSM()
                {
                    TotalEarnings = 0
                };
            }
           var deliveredOrders = await _apiDbContext.Order.Where(x => orderIds.Contains(x.Id) && x.OrderStatus == OrderStatusDM.Delivered).ToListAsync();
            var totalEarnings = deliveredOrders.Sum(x => x.TipAmount);
            return new DeliveryBoyEarningSM()
            {
                TotalEarnings = totalEarnings
            };
        }

        public async Task<DeliveryBoyEarningSM> GetDeliveryBoyEarningsByDeliveryCharges(long deliveryBoyId)
        {
            var dBoy = await _apiDbContext.DeliveryBoy.FindAsync(deliveryBoyId);
            if (dBoy == null)
            {
                throw new SiffrumException(ApiErrorTypeSM.Fatal_Log, $"Delivery boy with id: {deliveryBoyId} not found for getting Tip amount earnings", "Delivery Boy Not Found");
            }
            if (dBoy.PaymentType == DeliveryBoyPaymentTypeDM.Salaried)
            {
                return new DeliveryBoyEarningSM()
                {
                    TotalEarnings = 0
                };
            }
            var orderIds = await _apiDbContext.Deliveries
                .Where(x => x.DeliveryBoyId == deliveryBoyId).Select(x => x.OrderId).Distinct().ToListAsync();
            if (!orderIds.Any())
            {
                return new DeliveryBoyEarningSM()
                {
                    TotalEarnings = 0
                };
            }
            var deliveredOrders = await _apiDbContext.Order.Where(x => orderIds.Contains(x.Id) && x.OrderStatus == OrderStatusDM.Delivered).ToListAsync();
            var totalEarnings = deliveredOrders.Sum(x => x.DeliveryCharge);
            return new DeliveryBoyEarningSM()
            {
                TotalEarnings = totalEarnings
            };
        }

        #endregion Delivery Boy Earnings

        #region Seller - Order Deliveries

        public async Task<List<SellerOrderDeliverySM>> GetSellerOrderDeliveries(long sellerId, int skip, int top)
        {
            var sellerOrders = await _apiDbContext.Order
                .AsNoTracking()
                .Where(o => o.SellerId == sellerId)
                .OrderByDescending(o => o.CreatedAt)
                .Skip(skip).Take(top)
                .ToListAsync();

            var result = new List<SellerOrderDeliverySM>();

            foreach (var order in sellerOrders)
            {
                var delivery = await _apiDbContext.Deliveries
                    .AsNoTracking()
                    .FirstOrDefaultAsync(d => d.OrderId == order.Id);

                var item = new SellerOrderDeliverySM
                {
                    OrderId = order.Id,
                    OrderNumber = order.OrderNumber,
                    OrderStatus = ((OrderStatusSM)order.OrderStatus).ToString(),
                    PaymentMode = ((PaymentModeSM)order.PaymentMode).ToString(),
                    Amount = order.Amount,
                    OrderDate = order.CreatedAt
                };

                if (delivery != null)
                {
                    var dBoy = await _apiDbContext.DeliveryBoy.AsNoTracking()
                        .FirstOrDefaultAsync(d => d.Id == delivery.DeliveryBoyId);

                    item.DeliveryStatus = ((DeliveryStatusSM)delivery.Status).ToString();
                    item.DeliveryBoyId = delivery.DeliveryBoyId;
                    item.DeliveryBoyName = dBoy?.Name;
                    item.DeliveryBoyMobile = dBoy?.Mobile;
                    item.AssignedAt = delivery.AssignedAt;
                    item.DeliveredAt = delivery.DeliveredAt;
                    item.Commission = delivery.Commission;
                    item.DistanceInKm = delivery.DistanceInKm;
                    item.IsCodCollected = order.PaymentMode == PaymentModeDM.CashOnDelivery
                        && delivery.Status == DeliveryStatusDM.Delivered;
                }
                else
                {
                    item.DeliveryStatus = "Not Assigned";
                }

                var user = await _apiDbContext.User.AsNoTracking()
                    .FirstOrDefaultAsync(u => u.Id == order.UserId);
                item.CustomerName = user?.Name ?? user?.Mobile;

                result.Add(item);
            }

            return result;
        }

        public async Task<IntResponseRoot> GetSellerOrderDeliveriesCount(long sellerId)
        {
            var count = await _apiDbContext.Order
                .AsNoTracking()
                .CountAsync(o => o.SellerId == sellerId);
            return new IntResponseRoot(count, "Total orders");
        }

        #endregion

        #region Admin - Full Order Lifecycle

        public async Task<AdminOrderLifecycleSM> GetOrderLifecycle(long orderId)
        {
            var order = await _apiDbContext.Order
                .AsNoTracking()
                .Include(o => o.Invoices)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null)
                throw new SiffrumException(ApiErrorTypeSM.InvalidInputData_NoLog, "Order not found");

            var user = await _apiDbContext.User.AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == order.UserId);

            var seller = order.SellerId.HasValue
                ? await _apiDbContext.Seller.AsNoTracking()
                    .FirstOrDefaultAsync(s => s.Id == order.SellerId.Value)
                : null;

            var delivery = await _apiDbContext.Deliveries.AsNoTracking()
                .FirstOrDefaultAsync(d => d.OrderId == orderId);

            DeliveryBoyDM dBoy = null;
            List<DeliveryStatusHistorySM> statusHistory = new();
            DeliveryTrackingSM latestTracking = null;

            if (delivery != null)
            {
                dBoy = await _apiDbContext.DeliveryBoy.AsNoTracking()
                    .FirstOrDefaultAsync(d => d.Id == delivery.DeliveryBoyId);

                var historyDms = await _apiDbContext.DeliveryStatusHistory
                    .AsNoTracking()
                    .Where(h => h.DeliveryId == delivery.Id)
                    .OrderBy(h => h.CreatedAt)
                    .ToListAsync();
                statusHistory = _mapper.Map<List<DeliveryStatusHistorySM>>(historyDms);

                latestTracking = await GetLatestLocation(delivery.Id);
            }

            var orderItems = await _apiDbContext.OrderItem.AsNoTracking()
                .Where(i => i.OrderId == orderId).ToListAsync();

            return new AdminOrderLifecycleSM
            {
                OrderId = order.Id,
                OrderNumber = order.OrderNumber,
                OrderStatus = ((OrderStatusSM)order.OrderStatus).ToString(),
                PaymentMode = ((PaymentModeSM)order.PaymentMode).ToString(),
                PaymentStatus = ((PaymentStatusSM)order.PaymentStatus).ToString(),
                Amount = order.Amount,
                OrderDate = order.CreatedAt,

                CustomerName = user?.Name ?? user?.Mobile,
                CustomerMobile = user?.Mobile,

                SellerName = seller?.StoreName ?? seller?.Name,

                DeliveryStatus = delivery != null ? ((DeliveryStatusSM)delivery.Status).ToString() : "Not Assigned",
                DeliveryBoyName = dBoy?.Name,
                DeliveryBoyMobile = dBoy?.Mobile,
                AssignedAt = delivery?.AssignedAt,
                DeliveredAt = delivery?.DeliveredAt,

                IsCod = order.PaymentMode == PaymentModeDM.CashOnDelivery,
                CodCollected = order.PaymentMode == PaymentModeDM.CashOnDelivery
                    && delivery?.Status == DeliveryStatusDM.Delivered,

                DeliveryStatusHistory = statusHistory,
                LatestTracking = latestTracking,
                TotalItems = orderItems.Count
            };
        }

        #endregion

        #region Helpers

        private static double CalculateDistanceInKm(double lat1, double lon1, double lat2, double lon2)
        {
            const double R = 6371; // Earth radius in km
            var dLat = (lat2 - lat1) * Math.PI / 180;
            var dLon = (lon2 - lon1) * Math.PI / 180;
            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Cos(lat1 * Math.PI / 180) * Math.Cos(lat2 * Math.PI / 180) *
                    Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return Math.Round(R * c, 2);
        }

        /// <summary>
        /// Resolve CommissionPerKm with cascading priority: Seller > Admin.
        /// Also checks IsFreeDelivery: seller can turn it off for their store.
        /// Returns (isFreeDelivery, commissionPerKm)
        /// </summary>
        private async Task<(bool isFreeDelivery, decimal commissionPerKm)> ResolveCommissionSettings(long? sellerId)
        {
            // 1. Load admin (global) defaults
            bool globalFreeDelivery = false;
            decimal globalCommPerKm = 0;
            var globalSettings = await _apiDbContext.Settings.AsNoTracking().FirstOrDefaultAsync();
            if (globalSettings != null && !string.IsNullOrEmpty(globalSettings.JsonData))
            {
                var gJson = Newtonsoft.Json.JsonConvert.DeserializeObject<SettingsSM>(globalSettings.JsonData);
                globalFreeDelivery = gJson?.IsFreeDelivery ?? false;
                globalCommPerKm = gJson?.CommissionPerKm ?? 0;
            }

            // 2. Load seller overrides (if seller exists)
            if (sellerId.HasValue && sellerId.Value > 0)
            {
                var sellerSettingsDm = await _apiDbContext.SellerSettings.AsNoTracking()
                    .FirstOrDefaultAsync(s => s.SellerId == sellerId.Value);
                if (sellerSettingsDm != null && !string.IsNullOrEmpty(sellerSettingsDm.JsonData))
                {
                    var sJson = Newtonsoft.Json.JsonConvert.DeserializeObject<SellerSettingsJson>(sellerSettingsDm.JsonData);
                    if (sJson != null)
                    {
                        // Seller can turn OFF free delivery for their store
                        bool sellerFreeDelivery = sJson.IsFreeDelivery;
                        // Seller commission overrides admin if set (> 0)
                        decimal sellerCommPerKm = sJson.CommissionPerKm;

                        return (
                            isFreeDelivery: globalFreeDelivery && sellerFreeDelivery,
                            commissionPerKm: sellerCommPerKm > 0 ? sellerCommPerKm : globalCommPerKm
                        );
                    }
                }
            }

            return (globalFreeDelivery, globalCommPerKm);
        }

        #endregion
    }
}
