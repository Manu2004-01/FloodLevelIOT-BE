using System;

namespace WebAPI.Helpers
{
    public static class ScheduleValidator
    {
        public static string? ValidateScheduleDates(string? scheduleType, DateTime? startDate, DateTime? endDate)
        {
            var type = scheduleType?.Trim();
            if (string.IsNullOrWhiteSpace(type))
                return "ScheduleType là bắt buộc và phải là Weekly, Monthly hoặc Quarterly.";

            var typeLower = type.ToLowerInvariant();
            var isValidType = typeLower == "weekly" || typeLower == "monthly" || typeLower == "quarterly";
            if (!isValidType)
                return "ScheduleType phải là Weekly, Monthly hoặc Quarterly.";

            if (!startDate.HasValue || !endDate.HasValue)
                return "StartDate và EndDate là bắt buộc.";

            var start = startDate.Value.Date;
            var end = endDate.Value.Date;

            if (end <= start)
                return "EndDate phải sau StartDate.";

            if (typeLower == "weekly")
            {
                var daysDiff = (end - start).TotalDays;
                if (daysDiff < 7)
                    return "Với ScheduleType là Weekly, khoảng cách giữa EndDate và StartDate phải đúng 1 tuần (7 ngày). Hiện tại chưa đủ 1 tuần.";
                if (daysDiff > 7)
                    return "Với ScheduleType là Weekly, khoảng cách giữa EndDate và StartDate phải đúng 1 tuần (7 ngày). Hiện tại vượt quá 1 tuần.";
                return null;
            }

            if (typeLower == "monthly")
            {
                var expectedEnd = start.AddMonths(1);
                if (end != expectedEnd)
                    return "Với ScheduleType là Monthly, EndDate phải đúng 1 tháng sau StartDate (ví dụ: StartDate 15/01 thì EndDate phải là 15/02).";
                return null;
            }

            // quarterly
            var expectedEndQuarter = start.AddMonths(3);
            if (end != expectedEndQuarter)
                return "Với ScheduleType là Quarterly, EndDate phải đúng 3 tháng (1 quý) sau StartDate (ví dụ: StartDate 15/01 thì EndDate phải là 15/04).";
            return null;
        }
    }
}
