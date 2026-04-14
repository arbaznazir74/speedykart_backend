using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Siffrum.Ecom.BAL.Base.Email;
using Siffrum.Ecom.BAL.Base.ImageProcess;
using Siffrum.Ecom.BAL.Foundation.Base;
using Siffrum.Ecom.DAL.Context;
using Siffrum.Ecom.DomainModels.Enums;
using Siffrum.Ecom.ServiceModels.Foundation.Base.Interfaces;
using Siffrum.Ecom.ServiceModels.v1.Dashboard.SellerDashboard;

namespace Siffrum.Ecom.BAL.LoginUsers
{
    public class SellerDashboardProcess : SiffrumBalBase
    {

        public SellerDashboardProcess(IMapper mapper,ApiDbContext apiDbContext)
            : base(mapper, apiDbContext)
        {
        }

        public async Task<SellerDashboardSM> GetSellerDashboard(long sellerId)
        {
            return new SellerDashboardSM
            {
                Kpis = await GetKpis(sellerId),
                SalesGraph = await GetSalesGraph(sellerId),
                Orders = await GetOrderSnapshot(sellerId),
                Products = await GetProductSnapshot(sellerId),
                Financial = await GetFinancialSnapshot(sellerId),
                Customers = await GetCustomerInsights(sellerId)
            };
        }

        private async Task<SellerKpiSM> GetKpis(long sellerId)
        {
            var today = DateTime.UtcNow.Date;

            var todayRevenue = await _apiDbContext.OrderItem
                .Where(x =>
                    x.ProductVariant.Product.SellerId == sellerId &&
                    x.Order.PaymentStatus == PaymentStatusDM.Paid &&
                    x.Order.CreatedAt >= today)
                .SumAsync(x => (decimal?)x.TotalPrice) ?? 0;

            var ordersToday = await _apiDbContext.OrderItem
                .Where(x =>
                    x.ProductVariant.Product.SellerId == sellerId &&
                    x.Order.CreatedAt >= today)
                .Select(x => x.OrderId)
                .Distinct()
                .CountAsync();

            var pendingOrders = await _apiDbContext.OrderItem
                .Where(x =>
                    x.ProductVariant.Product.SellerId == sellerId &&
                    x.Order.OrderStatus == OrderStatusDM.Created)
                .Select(x => x.OrderId)
                .Distinct()
                .CountAsync();

            var lowStock = await _apiDbContext.ProductVariant
                .Where(x =>
                    x.Product.SellerId == sellerId &&
                    x.Stock.HasValue &&
                    x.Stock < 5)
                .CountAsync();

            var rating = await _apiDbContext.ProductRating
                .Where(x => x.ProductVariant.Product.SellerId == sellerId)
                .AverageAsync(x => (double?)x.Rate) ?? 0;

            return new SellerKpiSM
            {
                TodayRevenue = todayRevenue,
                OrdersToday = ordersToday,
                PendingOrders = pendingOrders,
                LowStockCount = lowStock,
                PendingPayoutAmount = todayRevenue,
                StoreRating = rating
            };
        }

        private async Task<SellerSalesGraphSM> GetSalesGraph(long sellerId)
        {
            var startDate = DateTime.UtcNow.Date.AddDays(-6);

            var data = await _apiDbContext.OrderItem
                .Where(x =>
                    x.ProductVariant.Product.SellerId == sellerId &&
                    x.Order.PaymentStatus == PaymentStatusDM.Paid &&
                    x.Order.CreatedAt >= startDate)
                .GroupBy(x => x.Order.CreatedAt.Value.Date)
                .Select(g => new SalesGraphPointSM
                {
                    Date = g.Key,
                    Revenue = g.Sum(x => x.TotalPrice),
                    OrderCount = g.Select(x => x.OrderId).Distinct().Count()
                })
                .ToListAsync();

            return new SellerSalesGraphSM
            {
                Data = data,
                GrowthPercentage = 0
            };
        }

        private async Task<SellerOrderSnapshotSM> GetOrderSnapshot(long sellerId)
        {
            var query = _apiDbContext.OrderItem
                .Where(x => x.ProductVariant.Product.SellerId == sellerId);

            return new SellerOrderSnapshotSM
            {
                Pending = await query.Where(x => x.Order.OrderStatus == OrderStatusDM.Created)
                                     .Select(x => x.OrderId).Distinct().CountAsync(),

                Processing = await query.Where(x => x.Order.OrderStatus == OrderStatusDM.Processing)
                                        .Select(x => x.OrderId).Distinct().CountAsync(),

                Shipped = await query.Where(x => x.Order.OrderStatus == OrderStatusDM.Shipped)
                                     .Select(x => x.OrderId).Distinct().CountAsync(),

                Delivered = await query.Where(x => x.Order.OrderStatus == OrderStatusDM.Delivered)
                                       .Select(x => x.OrderId).Distinct().CountAsync(),

                Returned = await query.Where(x => x.Order.OrderStatus == OrderStatusDM.Returned)
                                      .Select(x => x.OrderId).Distinct().CountAsync()
            };
        }

        private async Task<SellerProductSnapshotSM> GetProductSnapshot(long sellerId)
        {
            var totalActive = await _apiDbContext.ProductVariant
                .Where(x =>
                    x.Product.SellerId == sellerId &&
                    x.Status == ProductStatusDM.Active)
                .CountAsync();

            var outOfStock = await _apiDbContext.ProductVariant
                .Where(x =>
                    x.Product.SellerId == sellerId &&
                    x.Stock <= 0)
                .CountAsync();

            var rejected = await _apiDbContext.ProductVariant
                .Where(x =>
                    x.Product.SellerId == sellerId &&
                    x.Status == ProductStatusDM.Rejected)
                .CountAsync();

            var topProducts = await _apiDbContext.OrderItem
                .Where(x =>
                    x.ProductVariant.Product.SellerId == sellerId &&
                    x.Order.PaymentStatus == PaymentStatusDM.Paid)
                .GroupBy(x => new
                {
                    x.ProductVariantId,
                    x.ProductVariant.Name
                })
                .Select(g => new TopSellingProductSM
                {
                    ProductVariantId = g.Key.ProductVariantId,
                    Name = g.Key.Name,
                    QuantitySold = g.Sum(x => x.Quantity)
                })
                .OrderByDescending(x => x.QuantitySold)
                .Take(5)
                .ToListAsync();

            return new SellerProductSnapshotSM
            {
                TotalActiveProducts = totalActive,
                OutOfStock = outOfStock,
                RejectedProducts = rejected,
                TopSellingProducts = topProducts
            };
        }

        private async Task<SellerFinancialSnapshotSM> GetFinancialSnapshot(long sellerId)
        {
            var seller = await _apiDbContext.Seller.FindAsync(sellerId);

            var lockedBalance = await _apiDbContext.OrderItem
                .Where(x =>
                    x.ProductVariant.Product.SellerId == sellerId &&
                    x.Order.PaymentStatus == PaymentStatusDM.Paid &&
                    x.Order.OrderStatus != OrderStatusDM.Delivered)
                .SumAsync(x => (decimal?)x.TotalPrice) ?? 0;

            return new SellerFinancialSnapshotSM
            {
                AvailableBalance = (decimal)(seller?.Balance ?? 0),
                LockedBalance = lockedBalance,
                CommissionPaid = 0,
                UpcomingPayoutDate = DateTime.UtcNow.AddDays(7)
            };
        }

        private async Task<SellerCustomerInsightsSM> GetCustomerInsights(long sellerId)
        {
            var sellerOrders = _apiDbContext.OrderItem
                .Where(x => x.ProductVariant.Product.SellerId == sellerId);

            var totalCustomers = await sellerOrders
                .Select(x => x.Order.UserId)
                .Distinct()
                .CountAsync();

            var repeatCustomers = await sellerOrders
                .GroupBy(x => x.Order.UserId)
                .Where(g => g.Select(x => x.OrderId).Distinct().Count() > 1)
                .CountAsync();

            var avgOrderValue = await sellerOrders
                .Select(x => x.OrderId)
                .Distinct()
                .CountAsync() == 0
                ? 0
                : await sellerOrders.SumAsync(x => (decimal?)x.TotalPrice) /
                  await sellerOrders.Select(x => x.OrderId).Distinct().CountAsync();

            var bestCategory = await sellerOrders
                .GroupBy(x => x.ProductVariant.Product.Category.Name)
                .Select(g => new
                {
                    Category = g.Key,
                    Revenue = g.Sum(x => x.TotalPrice)
                })
                .OrderByDescending(x => x.Revenue)
                .Select(x => x.Category)
                .FirstOrDefaultAsync();

            return new SellerCustomerInsightsSM
            {
                RepeatCustomerPercentage = totalCustomers == 0
                    ? 0
                    : (double)repeatCustomers / totalCustomers * 100,

                AverageOrderValue = avgOrderValue ?? 0,
                BestPerformingCategory = bestCategory
            };
        }
    }
}
