using System;

namespace OrderService.Infrastructure.Extensions
{
    public static class DateTimeExtensions
    {
        public static DateTime ToUtcSafe(this DateTime dateTime)
        {
            return dateTime.Kind switch
            {
                DateTimeKind.Utc => dateTime,
                DateTimeKind.Local => dateTime.ToUniversalTime(),
                DateTimeKind.Unspecified => DateTime.SpecifyKind(dateTime, DateTimeKind.Utc),
                _ => dateTime
            };
        }
    }
}