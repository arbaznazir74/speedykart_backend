using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Siffrum.Ecom.BAL.ExceptionHandler;
using Siffrum.Ecom.BAL.Foundation.Base;
using Siffrum.Ecom.DAL.Context;
using Siffrum.Ecom.DomainModels.Enums;
using Siffrum.Ecom.DomainModels.v1;
using Siffrum.Ecom.ServiceModels.Enums;
using Siffrum.Ecom.ServiceModels.Foundation.Base.CommonResponseRoot;
using Siffrum.Ecom.ServiceModels.Foundation.Base.Enums;
using Siffrum.Ecom.ServiceModels.v1;

namespace Siffrum.Ecom.BAL.Product
{
    public class OrderComplaintProcess : SiffrumBalBase
    {
        public OrderComplaintProcess(IMapper mapper, ApiDbContext apiDbContext)
            : base(mapper, apiDbContext) { }

        #region User - Submit Complaint

        public async Task<OrderComplaintSM> SubmitComplaint(long userId, OrderComplaintSM sm)
        {
            var order = await _apiDbContext.Order
                .AsNoTracking()
                .FirstOrDefaultAsync(o => o.Id == sm.OrderId && o.UserId == userId);

            if (order == null)
                throw new SiffrumException(ApiErrorTypeSM.InvalidInputData_NoLog,
                    "Order not found or does not belong to you");

            var dm = _mapper.Map<OrderComplaintDM>(sm);
            dm.UserId = userId;
            dm.SellerId = order.SellerId;
            dm.Status = ComplaintStatusDM.Open;
            dm.CreatedAt = DateTime.UtcNow;

            await _apiDbContext.OrderComplaint.AddAsync(dm);
            await _apiDbContext.SaveChangesAsync();

            return _mapper.Map<OrderComplaintSM>(dm);
        }

        #endregion

        #region User - My Complaints

        public async Task<List<OrderComplaintSM>> GetMyComplaints(long userId, int skip, int top)
        {
            var list = await _apiDbContext.OrderComplaint
                .AsNoTracking()
                .Where(c => c.UserId == userId)
                .OrderByDescending(c => c.CreatedAt)
                .Skip(skip).Take(top)
                .ToListAsync();

            return _mapper.Map<List<OrderComplaintSM>>(list);
        }

        public async Task<IntResponseRoot> GetMyComplaintsCount(long userId)
        {
            var count = await _apiDbContext.OrderComplaint
                .AsNoTracking()
                .CountAsync(c => c.UserId == userId);
            return new IntResponseRoot(count, "Total Complaints");
        }

        #endregion

        #region Seller - Get Complaints

        public async Task<List<OrderComplaintDetailSM>> GetSellerComplaints(long sellerId, int skip, int top)
        {
            var complaints = await _apiDbContext.OrderComplaint
                .AsNoTracking()
                .Where(c => c.SellerId == sellerId)
                .OrderByDescending(c => c.CreatedAt)
                .Skip(skip).Take(top)
                .ToListAsync();

            var result = new List<OrderComplaintDetailSM>();
            foreach (var c in complaints)
            {
                result.Add(await BuildComplaintDetail(c));
            }
            return result;
        }

        public async Task<IntResponseRoot> GetSellerComplaintsCount(long sellerId)
        {
            var count = await _apiDbContext.OrderComplaint
                .AsNoTracking()
                .CountAsync(c => c.SellerId == sellerId);
            return new IntResponseRoot(count, "Total Complaints");
        }

        public async Task<OrderComplaintDetailSM> GetSellerComplaintById(long sellerId, long complaintId)
        {
            var complaint = await _apiDbContext.OrderComplaint
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == complaintId && c.SellerId == sellerId);

            if (complaint == null)
                throw new SiffrumException(ApiErrorTypeSM.InvalidInputData_NoLog,
                    "Complaint not found");

            return await BuildComplaintDetail(complaint);
        }

        #endregion

        #region Seller - Reply / Update Status

        public async Task<OrderComplaintSM> ReplyToComplaint(long sellerId, long complaintId, string reply, ComplaintStatusSM status)
        {
            var complaint = await _apiDbContext.OrderComplaint
                .FirstOrDefaultAsync(c => c.Id == complaintId && c.SellerId == sellerId);

            if (complaint == null)
                throw new SiffrumException(ApiErrorTypeSM.InvalidInputData_NoLog,
                    "Complaint not found");

            complaint.SellerReply = reply;
            complaint.Status = (ComplaintStatusDM)status;
            complaint.RepliedAt = DateTime.UtcNow;
            complaint.UpdatedAt = DateTime.UtcNow;

            await _apiDbContext.SaveChangesAsync();
            return _mapper.Map<OrderComplaintSM>(complaint);
        }

        #endregion

        #region Admin - Get All Complaints

        public async Task<List<OrderComplaintDetailSM>> GetAllComplaints(int skip, int top)
        {
            var complaints = await _apiDbContext.OrderComplaint
                .AsNoTracking()
                .OrderByDescending(c => c.CreatedAt)
                .Skip(skip).Take(top)
                .ToListAsync();

            var result = new List<OrderComplaintDetailSM>();
            foreach (var c in complaints)
            {
                result.Add(await BuildComplaintDetail(c));
            }
            return result;
        }

        public async Task<IntResponseRoot> GetAllComplaintsCount()
        {
            var count = await _apiDbContext.OrderComplaint.AsNoTracking().CountAsync();
            return new IntResponseRoot(count, "Total Complaints");
        }

        #endregion

        #region Private Helpers

        private async Task<OrderComplaintDetailSM> BuildComplaintDetail(OrderComplaintDM c)
        {
            var order = await _apiDbContext.Order
                .AsNoTracking()
                .FirstOrDefaultAsync(o => o.Id == c.OrderId);

            // Customer
            var user = await _apiDbContext.User
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == c.UserId);

            string customerName = !string.IsNullOrWhiteSpace(user?.Name)
                ? user.Name
                : !string.IsNullOrWhiteSpace(user?.Username)
                    ? user.Username
                    : user?.Mobile;

            // Delivery address
            OrderComplaintAddressSM? addressInfo = null;
            if (order?.AddressId != null)
            {
                var addr = await _apiDbContext.UserAddress
                    .AsNoTracking()
                    .FirstOrDefaultAsync(a => a.Id == order.AddressId);
                if (addr != null)
                {
                    addressInfo = new OrderComplaintAddressSM
                    {
                        Name = addr.Name,
                        Mobile = addr.Mobile,
                        Address = addr.Address,
                        Landmark = addr.Landmark,
                        Pincode = addr.Pincode,
                        City = addr.City,
                        State = addr.State
                    };
                }
            }

            // Delivery boy
            OrderComplaintDeliveryBoySM? deliveryBoyInfo = null;
            var delivery = await _apiDbContext.Deliveries
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.OrderId == c.OrderId);
            if (delivery != null)
            {
                var dBoy = await _apiDbContext.DeliveryBoy
                    .AsNoTracking()
                    .FirstOrDefaultAsync(db => db.Id == delivery.DeliveryBoyId);
                deliveryBoyInfo = new OrderComplaintDeliveryBoySM
                {
                    Id = delivery.DeliveryBoyId,
                    Name = dBoy?.Name,
                    Mobile = dBoy?.Mobile,
                    Email = dBoy?.Email,
                    DeliveryStatus = delivery.Status.ToString(),
                    AssignedAt = delivery.AssignedAt,
                    DeliveredAt = delivery.DeliveredAt
                };
            }

            // Order items
            var orderItems = await _apiDbContext.OrderItem
                .AsNoTracking()
                .Where(oi => oi.OrderId == c.OrderId)
                .ToListAsync();

            var items = new List<OrderComplaintItemSM>();
            foreach (var oi in orderItems)
            {
                var variant = await _apiDbContext.ProductVariant
                    .AsNoTracking()
                    .Include(v => v.Product)
                    .FirstOrDefaultAsync(v => v.Id == oi.ProductVariantId);

                items.Add(new OrderComplaintItemSM
                {
                    ProductName = variant?.Product?.Name,
                    VariantName = variant?.Name,
                    Indicator = variant?.Indicator.ToString(),
                    Quantity = oi.Quantity,
                    UnitPrice = oi.UnitPrice,
                    TotalPrice = oi.TotalPrice
                });
            }

            return new OrderComplaintDetailSM
            {
                Id = c.Id,
                Email = c.Email,
                Message = c.Message,
                Status = ((ComplaintStatusSM)c.Status).ToString(),
                SellerReply = c.SellerReply,
                RepliedAt = c.RepliedAt,
                CreatedAt = c.CreatedAt,
                OrderId = c.OrderId,
                OrderNumber = order?.OrderNumber ?? "",
                OrderAmount = order?.Amount ?? 0,
                OrderStatus = order != null ? ((OrderStatusSM)order.OrderStatus).ToString() : "",
                PaymentStatus = order != null ? ((PaymentStatusSM)order.PaymentStatus).ToString() : "",
                PaymentMode = order != null ? ((PaymentModeSM)order.PaymentMode).ToString() : "",
                DeliveryCharge = order?.DeliveryCharge ?? 0,
                PlatformCharge = order?.PlatormCharge ?? 0,
                CutleryCharge = order?.CutlaryCharge ?? 0,
                LowCartFeeCharge = order?.LowCartFeeCharge ?? 0,
                TipAmount = order?.TipAmount ?? 0,
                OrderDate = order?.CreatedAt,
                UserId = c.UserId,
                CustomerName = customerName,
                CustomerMobile = user?.Mobile,
                CustomerEmail = user?.Email,
                DeliveryAddress = addressInfo,
                DeliveryBoy = deliveryBoyInfo,
                Items = items
            };
        }

        #endregion
    }
}
