using EatKath.API.Entities;
using EatKath.API.Interfaces;
using EatKath.API.Services;
using EatKath.API.Tests.Helpers;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace EatKath.API.Tests.Services;

// The business clock: UTC unchanged, "local" time in Kathmandu (UTC+05:45),
// so "today" rolls over at 18:15 UTC - not at midnight UTC.
[TestClass]
public class BusinessTimeProviderTests
{
    private static BusinessTimeProvider KathmanduClockAt(DateTime utc)
    {
        return BusinessTimeProvider.CreateKathmandu(new FixedTimeProvider(utc));
    }

    [TestMethod]
    public void CreateKathmandu_ShouldUseUtcPlus0545_WithoutDaylightSaving()
    {
        var zone = BusinessTimeProvider.CreateKathmandu().LocalTimeZone;

        zone.Id.Should().BeOneOf("Asia/Kathmandu", "Nepal Standard Time");
        zone.BaseUtcOffset.Should().Be(new TimeSpan(5, 45, 0));
        zone.SupportsDaylightSavingTime.Should().BeFalse();
    }

    [TestMethod]
    public void GetLocalNow_ShouldBeNextKathmanduDate_LateInTheUtcEvening()
    {
        // 20:30 UTC on 12 Oct = 02:15 on 13 Oct in Kathmandu.
        var local = KathmanduClockAt(new DateTime(2026, 10, 12, 20, 30, 0)).GetLocalNow();

        local.Offset.Should().Be(new TimeSpan(5, 45, 0));
        local.DateTime.Should().Be(new DateTime(2026, 10, 13, 2, 15, 0));

        // How the services derive "today".
        DateOnly.FromDateTime(local.DateTime).Should().Be(new DateOnly(2026, 10, 13));
    }

    [TestMethod]
    [DataRow(18, 14, 59, 12, 23, 59, 59)]
    [DataRow(18, 15, 0, 13, 0, 0, 0)]
    public void GetLocalNow_ShouldRollOverAt1815Utc(
        int utcHour, int utcMinute, int utcSecond,
        int localDay, int localHour, int localMinute, int localSecond)
    {
        var local = KathmanduClockAt(
            new DateTime(2026, 10, 12, utcHour, utcMinute, utcSecond)).GetLocalNow();

        local.DateTime.Should().Be(
            new DateTime(2026, 10, localDay, localHour, localMinute, localSecond));
    }

    [TestMethod]
    public void GetUtcNow_ShouldStayUtc()
    {
        var utc = new DateTime(2026, 10, 12, 20, 30, 0);

        var now = KathmanduClockAt(utc).GetUtcNow();

        now.Offset.Should().Be(TimeSpan.Zero);
        now.UtcDateTime.Should().Be(utc);
    }

    [TestMethod]
    public void ResolveKathmanduTimeZone_ShouldFallBackToFixedUtcPlus0545_WhenNoIdIsKnown()
    {
        var zone = BusinessTimeProvider.ResolveKathmanduTimeZone(new[] { "Not/A_Real_Zone" });

        zone.Id.Should().Be("Asia/Kathmandu");
        zone.BaseUtcOffset.Should().Be(new TimeSpan(5, 45, 0));
        zone.SupportsDaylightSavingTime.Should().BeFalse();
    }

    [TestMethod]
    public void ResolveKathmanduTimeZone_SystemZoneAndFallback_ShouldAgree()
    {
        var system = BusinessTimeProvider.ResolveKathmanduTimeZone();
        var fallback = BusinessTimeProvider.ResolveKathmanduTimeZone(new[] { "Not/A_Real_Zone" });

        foreach (var utc in new[]
        {
            new DateTime(2026, 1, 15, 0, 0, 0, DateTimeKind.Utc),
            new DateTime(2026, 7, 15, 12, 0, 0, DateTimeKind.Utc),
            new DateTime(2026, 10, 12, 18, 15, 0, DateTimeKind.Utc)
        })
        {
            system.GetUtcOffset(utc).Should().Be(fallback.GetUtcOffset(utc));
        }
    }

    // The correction reaching existing date rules: a deal ending on
    // 10 Nov has ended once it is 11 Nov in Kathmandu, even though it is
    // still 10 Nov in UTC.
    [TestMethod]
    [DataRow(18, 0, false)] // 23:45 on 10 Nov in Kathmandu - still running
    [DataRow(20, 0, true)]  // 01:45 on 11 Nov in Kathmandu - ended
    public async Task DealService_ShouldJudgeDealEndInKathmanduTime(int utcHour, int utcMinute, bool expectEnded)
    {
        var context = TestDbContextFactory.Create();

        var restaurant = new Restaurant { Name = "Momo House", IsActive = true };
        context.Restaurants.Add(restaurant);
        await context.SaveChangesAsync();

        var deal = new Deal
        {
            RestaurantId = restaurant.Id,
            Title = "25% Off - Dine In",
            DiscountPercentage = 25,
            StartDate = new DateOnly(2026, 11, 1),
            EndDate = new DateOnly(2026, 11, 10),
            StartTime = new TimeOnly(12, 0),
            EndTime = new TimeOnly(14, 0),
            MaximumGuests = 4,
            IsActive = true
        };
        context.Deals.Add(deal);
        await context.SaveChangesAsync();

        var service = new DealService(
            context,
            MapperFactory.Create(),
            new Mock<IHttpContextAccessor>().Object,
            new Mock<ICurrentUserService>().Object,
            KathmanduClockAt(new DateTime(2026, 11, 10, utcHour, utcMinute, 0)));

        var result = (await service.GetByRestaurantAsync(restaurant.Id)).Single();

        if (expectEnded)
        {
            result.RemainingOffers.Should().Be(0);
            result.AvailabilityDate.Should().BeNull();
        }
        else
        {
            result.RemainingOffers.Should().BeNull(); // unlimited
            result.AvailabilityDate.Should().Be(new DateOnly(2026, 11, 10));
        }
    }
}
