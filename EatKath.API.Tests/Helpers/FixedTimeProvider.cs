namespace EatKath.API.Tests.Helpers
{
    // A clock frozen at a chosen moment (in UTC, which is also used as
    // the local time zone) so date/time business rules are testable.
    public class FixedTimeProvider : TimeProvider
    {
        public DateTimeOffset Now { get; set; }

        public FixedTimeProvider(DateTime localNow)
        {
            Now = new DateTimeOffset(localNow, TimeSpan.Zero);
        }

        public override DateTimeOffset GetUtcNow() => Now;

        public override TimeZoneInfo LocalTimeZone => TimeZoneInfo.Utc;
    }
}
