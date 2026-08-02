using EatKath.API.Data;
using EatKath.API.Entities;
using EatKath.API.Enums;


namespace EatKath.API.Data.Seeders;

public static class RedemptionSeeder
{
    public static async Task SeedAsync(ApplicationDbContext context)
    {
        if (context.Redemptions.Any())
            return;

        var users = context.Users.ToList();
        var deals = context.Deals.ToList();

        if (!users.Any() || !deals.Any())
            return;

        var redemptions = new List<Redemption>
{
    new()
    {
        UserId = users[0].Id,
        DealId = deals[0].Id,
        ArrivalDate = DateOnly.FromDateTime(DateTime.Today.AddDays(-10)),
        ArrivalTime = new TimeOnly(18, 30),
        GuestCount = 2,
        BillAmount = 2500,
        DiscountAmount = 750,
        FinalAmount = 1750,
        Status = RedemptionStatus.Completed,
        RedeemedAt = DateTime.UtcNow.AddDays(-10),
        CompletedAt = DateTime.UtcNow.AddDays(-10).AddHours(2)
    },

    new()
    {
        UserId = users[1].Id,
        DealId = deals[1].Id,
        ArrivalDate = DateOnly.FromDateTime(DateTime.Today.AddDays(-5)),
        ArrivalTime = new TimeOnly(19, 00),
        GuestCount = 4,
        BillAmount = 5200,
        DiscountAmount = 1560,
        FinalAmount = 3640,
        Status = RedemptionStatus.Completed,
        RedeemedAt = DateTime.UtcNow.AddDays(-5),
        CompletedAt = DateTime.UtcNow.AddDays(-5).AddHours(2)
    },

    new()
    {
        UserId = users[2].Id,
        DealId = deals[2].Id,
        ArrivalDate = DateOnly.FromDateTime(DateTime.Today.AddDays(-2)),
        ArrivalTime = new TimeOnly(18, 45),
        GuestCount = 3,
        BillAmount = 3600,
        DiscountAmount = 1080,
        FinalAmount = 2520,
        Status = RedemptionStatus.Cancelled,
        RedeemedAt = DateTime.UtcNow.AddDays(-2)
    },

    new()
    {
        UserId = users[0].Id,
        DealId = deals[3].Id,
        ArrivalDate = DateOnly.FromDateTime(DateTime.Today.AddDays(1)),
        ArrivalTime = new TimeOnly(20, 00),
        GuestCount = 2,
        Status = RedemptionStatus.Redeemed,
        RedeemedAt = DateTime.UtcNow
    },

    new()
    {
        UserId = users[1].Id,
        DealId = deals[4].Id,
        ArrivalDate = DateOnly.FromDateTime(DateTime.Today.AddDays(-1)),
        ArrivalTime = new TimeOnly(17, 30),
        GuestCount = 5,
        Status = RedemptionStatus.Expired,
        RedeemedAt = DateTime.UtcNow.AddDays(-1)
    }
};

        await context.Redemptions.AddRangeAsync(redemptions);
        await context.SaveChangesAsync();

       
    }
}