namespace LogServer.Utils;

public static class DayOfWeekUtils
{
    public static (string startColumn, string endColumn) GetBusinessHourColumns(this DayOfWeek dayOfWeek)
    {
        return dayOfWeek switch
        {
            DayOfWeek.Monday => ("MonStart", "MonEnd"),
            DayOfWeek.Tuesday => ("TuesStart", "TuesEnd"),
            DayOfWeek.Wednesday => ("WedStart", "WedEnd"),
            DayOfWeek.Thursday => ("ThursStart", "ThursEnd"),
            DayOfWeek.Friday => ("FriStart", "FriEnd"),
            DayOfWeek.Saturday => ("SatStart", "SatEnd"),
            DayOfWeek.Sunday => ("SunStart", "SunEnd"),
            _ => throw new ArgumentException("Invalid day of week")
        };
    }
}