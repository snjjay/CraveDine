namespace EatKath.API.Services
{
    // The app's clock (registered as TimeProvider in Program.cs).
    //
    // UTC comes from the system clock unchanged, so stored timestamps stay
    // UTC. "Local" time is the business time zone, Kathmandu (UTC+05:45),
    // instead of the server's own zone (UTC on Azure): every GetLocalNow()
    // caller - deal availability, redemption date/time rules, card
    // summaries - judges "today" and "now" as the restaurants see them.
    public sealed class BusinessTimeProvider : TimeProvider
    {
        public const string KathmanduTimeZoneId = "Asia/Kathmandu";

        // Nepal has used UTC+05:45 with no daylight saving since 1986.
        public static readonly TimeSpan KathmanduUtcOffset = new(5, 45, 0);

        // IANA id first (Linux, and Windows with ICU), then the Windows id.
        // .NET 8 converts between the two where it can, so either works on
        // a normal Azure App Service.
        private static readonly string[] KathmanduTimeZoneIds =
        {
            KathmanduTimeZoneId,
            "Nepal Standard Time"
        };

        private readonly TimeProvider _utcSource;

        private readonly TimeZoneInfo _timeZone;

        public BusinessTimeProvider(TimeZoneInfo timeZone, TimeProvider? utcSource = null)
        {
            _timeZone = timeZone;
            _utcSource = utcSource ?? System;
        }

        public static BusinessTimeProvider CreateKathmandu(TimeProvider? utcSource = null)
        {
            return new BusinessTimeProvider(ResolveKathmanduTimeZone(), utcSource);
        }

        // The system's Kathmandu zone, or - if the OS has no time zone data
        // for it (e.g. a container without tzdata/ICU) - an equivalent fixed
        // UTC+05:45 zone, so the business clock never silently becomes UTC.
        public static TimeZoneInfo ResolveKathmanduTimeZone(IEnumerable<string>? candidateIds = null)
        {
            foreach (var id in candidateIds ?? KathmanduTimeZoneIds)
            {
                try
                {
                    return TimeZoneInfo.FindSystemTimeZoneById(id);
                }
                catch (Exception ex) when (ex is TimeZoneNotFoundException or InvalidTimeZoneException)
                {
                    // Try the next id.
                }
            }

            return TimeZoneInfo.CreateCustomTimeZone(
                KathmanduTimeZoneId,
                KathmanduUtcOffset,
                "Nepal Time",
                "Nepal Time");
        }

        public override TimeZoneInfo LocalTimeZone => _timeZone;

        public override DateTimeOffset GetUtcNow() => _utcSource.GetUtcNow();

        public override long GetTimestamp() => _utcSource.GetTimestamp();

        public override long TimestampFrequency => _utcSource.TimestampFrequency;

        public override ITimer CreateTimer(TimerCallback callback, object? state, TimeSpan dueTime, TimeSpan period)
            => _utcSource.CreateTimer(callback, state, dueTime, period);
    }
}
