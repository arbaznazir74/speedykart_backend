using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Siffrum.Ecom.BAL.Foundation.Base;
using Siffrum.Ecom.DAL.Context;
using Siffrum.Ecom.DomainModels.Enums;
using Siffrum.Ecom.ServiceModels.v1;
using Siffrum.Ecom.ServiceModels.v1.Dashboard.AdminDashboard;

namespace Siffrum.Ecom.BAL.LoginUsers
{
    public class AdminDashboardProcess : SiffrumBalBase
    {

        public AdminDashboardProcess(IMapper mapper,ApiDbContext apiDbContext)
        : base(mapper, apiDbContext)
        {
        }

        public async Task<AdminDashboardResponseSM> GetDashboardAsync(DateTime? date = null)
        {
            var today = DateTime.UtcNow.Date;
            var selectedDay = date.HasValue ? date.Value.Date : today;
            var dayStart = selectedDay.AddHours(-5).AddMinutes(-30);
            var dayEnd = dayStart.AddDays(1);
            var firstDayOfMonth = new DateTime(selectedDay.Year, selectedDay.Month, 1, 0, 0, 0, DateTimeKind.Utc);
            var weekStart = selectedDay.AddDays(-7);

            var response = new AdminDashboardResponseSM();

            #region PLATFORM KPIs

            var monthlyOrders = await _apiDbContext.Order
                .Where(o => o.CreatedAt >= firstDayOfMonth && o.PaymentStatus == PaymentStatusDM.Paid)
                .ToListAsync();

            response.PlatformKpis.GmvThisMonth = monthlyOrders.Sum(x => x.Amount);

            response.PlatformKpis.PlatformCommissionEarned =
                await _apiDbContext.OrderItem
                    .Where(x => x.Order.CreatedAt >= firstDayOfMonth &&
                                x.Order.PaymentStatus == PaymentStatusDM.Paid)
                    .SumAsync(x =>
                        (x.UnitPrice * x.Quantity) *
                        (x.ProductVariant.Product.Seller.Commission / 100));
            
            response.PlatformKpis.TotalOrdersToday =
                await _apiDbContext.Order
                    .Where(x => x.CreatedAt >= dayStart && x.CreatedAt < dayEnd)
                    .Select(x => x.Id)
                    .Distinct()
                    .CountAsync();

            response.PlatformKpis.ActiveVendors =
                await _apiDbContext.Seller
                    .Where(x => x.Status == SellerStatusDM.Active && x.DeletedAt == null)
                    .CountAsync();

            response.PlatformKpis.NewVendorsThisWeek =
                await _apiDbContext.Seller
                    .Where(x => x.CreatedAt >= weekStart)
                    .CountAsync();

            response.PlatformKpis.PendingRefundRequests =
                await _apiDbContext.Order
                    .Where(x => x.PaymentStatus == PaymentStatusDM.Pending)
                    .CountAsync();

            #endregion

            #region REVENUE ANALYTICS (30 DAYS)

            var last30Days = today.AddDays(-30);

            var dailyRevenue = await _apiDbContext.Order
                .Where(x => x.CreatedAt >= last30Days && x.PaymentStatus == PaymentStatusDM.Paid)
                .GroupBy(x => x.CreatedAt.Value.Date)
                .Select(g => new DailyRevenueSM
                {
                    Date = g.Key,
                    Amount = (double)g.Sum(x => x.Amount),
                    OrderCount = g.Count()
                })
                .OrderBy(x => x.Date)
                .ToListAsync();

            response.RevenueAnalytics.CommissionTrend = dailyRevenue;

            var dailyTotals = await _apiDbContext.Order
                .Where(x => x.CreatedAt >= last30Days)
                .GroupBy(x => x.CreatedAt.Value.Date)
                .Select(g => new
                {
                    Date = g.Key,
                    TotalOrders = g.Count(),
                    RefundedOrders = g.Count(o => o.OrderStatus == OrderStatusDM.Returned || o.OrderStatus == OrderStatusDM.Cancelled)
                })
                .OrderBy(x => x.Date)
                .ToListAsync();

            response.RevenueAnalytics.RefundRatioOverlay = dailyTotals
                .Select(d => new RefundRatioSM
                {
                    Date = d.Date,
                    RefundRatioPercentage = d.TotalOrders > 0
                        ? Math.Round((double)d.RefundedOrders / d.TotalOrders * 100, 2)
                        : 0
                }).ToList();

            #endregion

            #region VENDOR HEALTH

            var allVendorStats = await _apiDbContext.OrderItem
                .Where(x => x.Order.PaymentStatus == PaymentStatusDM.Paid)
                .GroupBy(x => new
                {
                    x.ProductVariant.Product.SellerId,
                    x.ProductVariant.Product.Seller.StoreName
                })
                .Select(g => new TopVendorSM
                {
                    SellerId = g.Key.SellerId,
                    StoreName = g.Key.StoreName,
                    TotalSales = g.Sum(x => x.UnitPrice * x.Quantity)
                })
                .OrderByDescending(x => x.TotalSales)
                .ToListAsync();

            response.VendorHealth.TopVendorsBySales = allVendorStats.Take(5).ToList();

            var vendorRefundStats = await _apiDbContext.OrderItem
                .GroupBy(x => new
                {
                    x.ProductVariant.Product.SellerId,
                    x.ProductVariant.Product.Seller.StoreName
                })
                .Select(g => new
                {
                    SellerId = g.Key.SellerId,
                    StoreName = g.Key.StoreName,
                    TotalOrders = g.Select(x => x.OrderId).Distinct().Count(),
                    RefundedOrders = g.Where(x =>
                        x.Order.OrderStatus == OrderStatusDM.Returned ||
                        x.Order.OrderStatus == OrderStatusDM.Cancelled)
                        .Select(x => x.OrderId).Distinct().Count(),
                    TotalSales = g.Sum(x => x.UnitPrice * x.Quantity)
                })
                .ToListAsync();

            response.VendorHealth.VendorsWithHighRefundRate = vendorRefundStats
                .Where(v => v.TotalOrders > 0 && ((double)v.RefundedOrders / v.TotalOrders * 100) > 10)
                .Select(v => new TopVendorSM
                {
                    SellerId = v.SellerId,
                    StoreName = v.StoreName,
                    TotalSales = v.TotalSales,
                    RefundRate = Math.Round((double)v.RefundedOrders / v.TotalOrders * 100, 1)
                })
                .OrderByDescending(x => x.RefundRate)
                .Take(5)
                .ToList();

            response.VendorHealth.DeactivatedSellers =
                await _apiDbContext.Seller
                    .Where(x => x.Status == SellerStatusDM.Deactivated)
                    .CountAsync();

            #endregion

            #region ORDER HEALTH

            response.OrderHealth.OrdersByStatus =
                await _apiDbContext.Order
                    .GroupBy(x => x.OrderStatus)
                    .Select(g => new OrderStatusCountSM
                    {
                        Status = g.Key.ToString(),
                        Count = g.Count()
                    }).ToListAsync();

            response.OrderHealth.FailedPayments =
                await _apiDbContext.Order
                    .Where(x => x.PaymentStatus == PaymentStatusDM.Failed)
                    .CountAsync();

            response.OrderHealth.PendingPayments =
                await _apiDbContext.Order
                    .Where(x => x.PaymentStatus == PaymentStatusDM.Pending)
                    .CountAsync();

            #endregion

            #region PAYMENT MODE DISTRIBUTION

            var paymentModes = await _apiDbContext.Order
                .Where(x => x.CreatedAt >= firstDayOfMonth)
                .GroupBy(x => x.PaymentMode)
                .Select(g => new PaymentModeCountSM
                {
                    Mode = g.Key.ToString(),
                    Count = g.Count(),
                    Amount = g.Sum(x => x.Amount)
                })
                .ToListAsync();

            response.PaymentModeDistribution = paymentModes;

            #endregion

            #region HOURLY ORDER DISTRIBUTION (last 7 days)

            var last7Days = today.AddDays(-7);

            var hourlyOrders = await _apiDbContext.Order
                .Where(x => x.CreatedAt >= last7Days)
                .GroupBy(x => x.CreatedAt.Value.Hour)
                .Select(g => new HourlyOrderSM
                {
                    Hour = g.Key,
                    Count = g.Count()
                })
                .OrderBy(x => x.Hour)
                .ToListAsync();

            response.HourlyOrderDistribution = hourlyOrders;

            #endregion

            #region CATEGORY REVENUE (this month)

            var categoryRevenue = await _apiDbContext.OrderItem
                .Where(x => x.Order.CreatedAt >= firstDayOfMonth &&
                            x.Order.PaymentStatus == PaymentStatusDM.Paid)
                .GroupBy(x => x.ProductVariant.Product.Category.Name)
                .Select(g => new CategoryRevenueSM
                {
                    Category = g.Key ?? "Uncategorized",
                    Revenue = g.Sum(x => x.TotalPrice),
                    OrderCount = g.Select(x => x.OrderId).Distinct().Count()
                })
                .OrderByDescending(x => x.Revenue)
                .Take(10)
                .ToListAsync();

            response.CategoryRevenue = categoryRevenue;

            #endregion

            return response;
        }
    }
}
