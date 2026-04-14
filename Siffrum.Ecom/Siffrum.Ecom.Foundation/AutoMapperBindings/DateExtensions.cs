using System.Globalization;

namespace Siffrum.Ecom.Foundation.AutoMapperBindings
{
    public static class DateExtensions
    {
        public static string ConvertToIsoDateTimeString(this DateTime dateTime)
        {
            return dateTime.ToString("s", CultureInfo.InvariantCulture);
        }

        public static DateTime ConvertToDateTimeFromIsoString(this string isoDateTime)
        {
            return DateTime.Parse(isoDateTime, CultureInfo.InvariantCulture);
        }

        public static DateTime? ConvertToNullableDateTimeFromIsoString(this string isoDateTime)
        {
            if (string.IsNullOrWhiteSpace(isoDateTime))
            {
                return null;
            }

            return DateTime.Parse(isoDateTime, CultureInfo.InvariantCulture);
        }

        public static DateTime ConvertFromUTCToTimezoneById(this DateTime dateTime, string timeZoneId)
        {
            return TimeZoneInfo.ConvertTimeToUtc(dateTime, TimeZoneInfo.FindSystemTimeZoneById(timeZoneId));
        }

        public static DateTime ConvertToUTCFromTimezoneById(this DateTime dateTime, string timeZoneId)
        {
            return TimeZoneInfo.ConvertTimeFromUtc(dateTime, TimeZoneInfo.FindSystemTimeZoneById(timeZoneId));
        }

        public static DateTime ConvertFromUTCToSystemTimezone(this DateTime dateTime)
        {
            return TimeZoneInfo.ConvertTimeFromUtc(dateTime, TimeZoneInfo.Local);
        }

        public static DateTime ConvertToUTCFromSystemTimezone(this DateTime dateTime)
        {
            return TimeZoneInfo.ConvertTimeToUtc(dateTime, TimeZoneInfo.Local);
        }

        public static DateTime? ConvertDateTimeToSystemTimezone(this DateTime? dateTime)
        {
            return TimeZoneInfo.Local.ConvertDateTimeToTimezone(dateTime);
        }

        public static DateTime? ConvertDateTimeToTimezoneById(this DateTime? dateTime, string timeZoneId)
        {
            return TimeZoneInfo.FindSystemTimeZoneById(timeZoneId).ConvertDateTimeToTimezone(dateTime);
        }

        public static DateTime? ConvertDateTimeToTimezone(this TimeZoneInfo timeZoneInfo, DateTime? dateTime)
        {
            if (timeZoneInfo != null && dateTime.HasValue)
            {
                return TimeZoneInfo.ConvertTime(dateTime.Value, timeZoneInfo);
            }

            return null;
        }
    }
}
