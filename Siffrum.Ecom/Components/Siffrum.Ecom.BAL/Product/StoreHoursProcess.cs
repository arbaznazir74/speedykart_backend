using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Siffrum.Ecom.BAL.ExceptionHandler;
using Siffrum.Ecom.BAL.Foundation.Base;
using Siffrum.Ecom.DAL.Context;
using Siffrum.Ecom.DomainModels.v1;
using Siffrum.Ecom.ServiceModels.Foundation.Base.Enums;
using Siffrum.Ecom.ServiceModels.v1;

namespace Siffrum.Ecom.BAL.Product
{
    public class StoreHoursProcess : SiffrumBalBase
    {
        public StoreHoursProcess(IMapper mapper, ApiDbContext apiDbContext)
            : base(mapper, apiDbContext)
        {
        }

        #region GET

        public async Task<List<StoreHoursSM>> GetStoreHours(long sellerId)
        {
            var hours = await _apiDbContext.StoreHours
                .AsNoTracking()
                .Where(x => x.SellerId == sellerId)
                .OrderBy(x => x.DayOfWeek)
                .ToListAsync();

            return _mapper.Map<List<StoreHoursSM>>(hours);
        }

        #endregion

        #region UPSERT (Seller sets weekly schedule)

        public async Task<List<StoreHoursSM>> UpsertStoreHours(long sellerId, List<StoreHoursSM> hoursList)
        {
            if (hoursList == null || !hoursList.Any())
                throw new SiffrumException(ApiErrorTypeSM.InvalidInputData_NoLog,
                    "Please provide at least one day's schedule");

            foreach (var h in hoursList)
            {
                if (h.DayOfWeek < 0 || h.DayOfWeek > 6)
                    throw new SiffrumException(ApiErrorTypeSM.InvalidInputData_NoLog,
                        "DayOfWeek must be between 0 (Sunday) and 6 (Saturday)");

                if (!h.IsClosed && (h.OpenTime == null || h.CloseTime == null))
                    throw new SiffrumException(ApiErrorTypeSM.InvalidInputData_NoLog,
                        $"Open and Close times are required for open days (day {h.DayOfWeek})");
            }

            var existing = await _apiDbContext.StoreHours
                .Where(x => x.SellerId == sellerId)
                .ToListAsync();

            foreach (var h in hoursList)
            {
                var match = existing.FirstOrDefault(x => x.DayOfWeek == h.DayOfWeek);
                if (match != null)
                {
                    match.OpenTime = h.IsClosed ? null : h.OpenTime;
                    match.CloseTime = h.IsClosed ? null : h.CloseTime;
                    match.IsClosed = h.IsClosed;
                    match.UpdatedAt = DateTime.UtcNow;
                }
                else
                {
                    var dm = new StoreHoursDM
                    {
                        SellerId = sellerId,
                        DayOfWeek = h.DayOfWeek,
                        OpenTime = h.IsClosed ? null : h.OpenTime,
                        CloseTime = h.IsClosed ? null : h.CloseTime,
                        IsClosed = h.IsClosed,
                        CreatedAt = DateTime.UtcNow
                    };
                    await _apiDbContext.StoreHours.AddAsync(dm);
                }
            }

            await _apiDbContext.SaveChangesAsync();
            return await GetStoreHours(sellerId);
        }

        #endregion

        #region STORE AVAILABILITY CHECK (for Flutter app & order validation)

        public async Task<StoreAvailabilitySM> CheckStoreAvailability(long sellerId, string timezone = "Asia/Kolkata")
        {
            var now = TimeZoneInfo.ConvertTimeFromUtc(
                DateTime.UtcNow,
                TimeZoneInfo.FindSystemTimeZoneById(timezone));

            var todayDow = (short)now.DayOfWeek; // .NET DayOfWeek: Sunday=0
            var currentTime = now.TimeOfDay;

            var hours = await _apiDbContext.StoreHours
                .AsNoTracking()
                .Where(x => x.SellerId == sellerId)
                .ToListAsync();

            // If no hours set, store is always open
            if (!hours.Any())
                return new StoreAvailabilitySM { IsOpen = true, Message = "Store is open" };

            var today = hours.FirstOrDefault(x => x.DayOfWeek == todayDow);

            // Today is closed or not configured
            if (today == null || today.IsClosed)
            {
                var nextOpen = FindNextOpenSlot(hours, todayDow);
                return new StoreAvailabilitySM
                {
                    IsOpen = false,
                    Message = nextOpen != null
                        ? $"Store is closed today. Opens {nextOpen.Value.dayName} at {FormatTime(nextOpen.Value.openTime)}"
                        : "Store is currently closed"
                };
            }

            // 24-hour open: openTime=00:00 and closeTime=23:59
            var is24Hour = today.OpenTime.HasValue && today.CloseTime.HasValue
                && today.OpenTime.Value == TimeSpan.Zero
                && today.CloseTime.Value >= new TimeSpan(23, 59, 0);

            if (is24Hour)
            {
                return new StoreAvailabilitySM
                {
                    IsOpen = true,
                    Message = "Store is open 24 hours today"
                };
            }

            // Before opening
            if (today.OpenTime.HasValue && currentTime < today.OpenTime.Value)
            {
                return new StoreAvailabilitySM
                {
                    IsOpen = false,
                    Message = $"Store opens today at {FormatTime(today.OpenTime.Value)}",
                    OpensAt = today.OpenTime
                };
            }

            // After closing
            if (today.CloseTime.HasValue && currentTime > today.CloseTime.Value)
            {
                var nextOpen = FindNextOpenSlot(hours, todayDow);
                return new StoreAvailabilitySM
                {
                    IsOpen = false,
                    Message = nextOpen != null
                        ? $"Store is closed for today. Opens {nextOpen.Value.dayName} at {FormatTime(nextOpen.Value.openTime)}"
                        : "Store is currently closed"
                };
            }

            // Store is open
            return new StoreAvailabilitySM
            {
                IsOpen = true,
                Message = $"Store is open until {FormatTime(today.CloseTime!.Value)}",
                ClosesAt = today.CloseTime
            };
        }

        #endregion

        #region Helpers

        private (string dayName, TimeSpan openTime)? FindNextOpenSlot(
            List<StoreHoursDM> hours, short currentDow)
        {
            for (int i = 1; i <= 7; i++)
            {
                var nextDow = (short)((currentDow + i) % 7);
                var slot = hours.FirstOrDefault(x => x.DayOfWeek == nextDow);
                if (slot != null && !slot.IsClosed && slot.OpenTime.HasValue)
                {
                    var dayName = ((System.DayOfWeek)nextDow).ToString();
                    if (i == 1) dayName = "tomorrow";
                    return (dayName, slot.OpenTime.Value);
                }
            }
            return null;
        }

        private static string FormatTime(TimeSpan ts)
        {
            var dt = DateTime.Today.Add(ts);
            return dt.ToString("h:mm tt");
        }

        #endregion
    }

    public class StoreAvailabilitySM
    {
        public bool IsOpen { get; set; }
        public string Message { get; set; }
        public TimeSpan? OpensAt { get; set; }
        public TimeSpan? ClosesAt { get; set; }
    }
}
