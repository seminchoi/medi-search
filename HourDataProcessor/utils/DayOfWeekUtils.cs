namespace HourDataProcessor.utils;

public static class DayOfWeekExtensions
{
    public static string GetDayPrefix(this DayOfWeek dayOfWeek)
    {
        return dayOfWeek switch
        {
            DayOfWeek.Monday => "Mon",
            DayOfWeek.Tuesday => "Tues",
            DayOfWeek.Wednesday => "Wed",
            DayOfWeek.Thursday => "Thurs",
            DayOfWeek.Friday => "Fri",
            DayOfWeek.Saturday => "Sat",
            DayOfWeek.Sunday => "Sun",
            _ => throw new ArgumentOutOfRangeException(nameof(dayOfWeek), dayOfWeek, "Invalid DayOfWeek value.")
        };
    }
}