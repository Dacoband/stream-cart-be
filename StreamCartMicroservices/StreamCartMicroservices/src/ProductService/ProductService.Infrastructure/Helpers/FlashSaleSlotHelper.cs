using System;
using System.Collections.Generic;
using System.Linq;

namespace ProductService.Application.Helpers
{
    public static class FlashSaleSlotHelper
    {
        public static readonly Dictionary<int, (TimeSpan Start, TimeSpan End)> SlotTimeRanges = new()
        {
            { 1, (new TimeSpan(0, 0, 0), new TimeSpan(2, 0, 0)) },    
            { 2, (new TimeSpan(2, 0, 0), new TimeSpan(6, 0, 0)) },    
            { 3, (new TimeSpan(6, 0, 0), new TimeSpan(9, 0, 0)) },    
            { 4, (new TimeSpan(9, 0, 0), new TimeSpan(17, 0, 0)) },   
            { 5, (new TimeSpan(17, 0, 0), new TimeSpan(19, 0, 0)) },
            { 6, (new TimeSpan(19, 0, 0), new TimeSpan(21, 0, 0)) },  
            { 7, (new TimeSpan(21, 0, 0), new TimeSpan(23, 59, 59)) } 
        };

        public static (DateTime Start, DateTime End) GetSlotTimeForDate(int slot, DateTime date)
        {
            if (!SlotTimeRanges.ContainsKey(slot))
                throw new ArgumentException($"Invalid slot number: {slot}");

            var timeRange = SlotTimeRanges[slot];
            var startDateTime = date.Date.Add(timeRange.Start);
            var endDateTime = date.Date.Add(timeRange.End);

            return (startDateTime, endDateTime);
        }

        public static List<int> GetAvailableSlotsForDate(DateTime date, List<int> occupiedSlots)
        {
            var allSlots = SlotTimeRanges.Keys.ToList();
            return allSlots.Except(occupiedSlots).ToList();
        }

        public static int? GetCurrentSlotForTime(DateTime dateTime)
        {
            var timeOfDay = dateTime.TimeOfDay;

            foreach (var slot in SlotTimeRanges)
            {
                if (timeOfDay >= slot.Value.Start && timeOfDay <= slot.Value.End)
                    return slot.Key;
            }

            return null; // Không trong slot nào
        }

        public static bool IsSlotValidForDate(int slot, DateTime date)
        {
            return SlotTimeRanges.ContainsKey(slot);
        }
    }
}