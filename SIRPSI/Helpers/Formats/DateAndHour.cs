namespace SIRPSI.Core.Helper
{
    public static class DateAndHour
    {
        //Cambia el formato de hora a la hora del servidor
        public static DateTimeOffset ToDateTimeZone(this DateTime date, int timeZone = -5)
        {
            var offsetLocal = TimeZoneInfo.Local.GetUtcOffset(DateTime.UtcNow);
            var utcDateTime = new DateTime(date.Year, date.Month, date.Day, date.Hour, date.Minute, date.Second, DateTimeKind.Local);
            var dateUTC = new DateTimeOffset(utcDateTime).ToOffset(TimeSpan.FromHours(0));
            dateUTC = dateUTC.ToOffset(TimeSpan.FromHours(0)).ToOffset(TimeSpan.FromHours(timeZone));
            return dateUTC;
        }
    }
}
