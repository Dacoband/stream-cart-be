using System;
using System.Collections.Generic;
using System.Linq;

namespace ProductService.Application.Helpers
{
    public static class FlashSaleSlotHelper
    {
        public static readonly Dictionary<int, (TimeSpan Start, TimeSpan End)> SlotTimeRanges = new()
        {
            { 1, (new TimeSpan(0, 0, 0), new TimeSpan(2, 0, 0)) },      // 00:00 - 02:00
            { 2, (new TimeSpan(2, 0, 0), new TimeSpan(6, 0, 0)) },      // 02:00 - 06:00
            { 3, (new TimeSpan(6, 0, 0), new TimeSpan(9, 0, 0)) },      // 06:00 - 09:00
            { 4, (new TimeSpan(9, 0, 0), new TimeSpan(14, 0, 0)) },     // 09:00 - 14:00
            { 5, (new TimeSpan(14, 0, 0), new TimeSpan(17, 0, 0)) },    // 14:00 - 17:00 
            { 6, (new TimeSpan(17, 0, 0), new TimeSpan(19, 0, 0)) },    // 17:00 - 19:00
            { 7, (new TimeSpan(19, 0, 0), new TimeSpan(21, 0, 0)) },    // 19:00 - 21:00
            { 8, (new TimeSpan(21, 0, 0), new TimeSpan(23, 59, 59)) }   // 21:00 - 23:59 
        };

        public static (DateTime Start, DateTime End) GetSlotTimeForDate(int slot, DateTime date)
        {
            if (!SlotTimeRanges.ContainsKey(slot))
                throw new ArgumentException($"Invalid slot number: {slot}. Valid slots are 1-8");

            var timeRange = SlotTimeRanges[slot];
            var seAsiaTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
            var startLocal = date.Date.Add(timeRange.Start);
            var endLocal = date.Date.Add(timeRange.End);
            var startUtc = TimeZoneInfo.ConvertTimeToUtc(startLocal, seAsiaTimeZone);
            var endUtc = TimeZoneInfo.ConvertTimeToUtc(endLocal, seAsiaTimeZone);

            return (startUtc, endUtc);
        }

        public static List<int> GetAvailableSlotsForDate(DateTime date, List<int> occupiedSlots)
        {
            var allSlots = SlotTimeRanges.Keys.ToList();
            return allSlots.Except(occupiedSlots).ToList();
        }

        public static int? GetCurrentSlotForTime(DateTime dateTime)
        {
            var seAsiaTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
            var localTime = dateTime.Kind == DateTimeKind.Utc
                ? TimeZoneInfo.ConvertTimeFromUtc(dateTime, seAsiaTimeZone)
                : dateTime;

            var timeOfDay = localTime.TimeOfDay;

            foreach (var slot in SlotTimeRanges)
            {
                if (timeOfDay >= slot.Value.Start && timeOfDay <= slot.Value.End)
                    return slot.Key;
            }

            return null;
        }

        public static bool IsSlotValidForDate(int slot, DateTime date)
        {
            return SlotTimeRanges.ContainsKey(slot);
        }
    }
}