using AutoMapper;
using EatKath.API.Data;
using EatKath.API.DTOs.Deal;
using EatKath.API.Entities;
using EatKath.API.Enums;
using EatKath.API.Interfaces;
using EatKath.API.Services;
using EatKath.API.Tests.Helpers;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace EatKath.API.Tests.Services;

[TestClass]
public class DealServiceTests
{
    private ApplicationDbContext _context = null!;
    private IMapper _mapper = null!;
    private Mock<IHttpContextAccessor> _httpContextAccessor = null!;
    private Mock<ICurrentUserService> _currentUser = null!;
    private DealService _service = null!;

    [TestInitialize]
    public void Setup()
    {
        _context = TestDbContextFactory.Create();

        _mapper = MapperFactory.Create();

        _httpContextAccessor = new Mock<IHttpContextAccessor>();

        _currentUser = new Mock<ICurrentUserService>();

        // Default: an Admin caller, so existing tests that don't
        // care about ownership continue to behave as before.
        _currentUser.Setup(x => x.UserId).Returns(1);
        _currentUser.Setup(x => x.IsAdmin).Returns(true);

        // "Now" for availability tests: 10 Nov 2026, 10:00.
        _service = new DealService(
            _context,
            _mapper,
            _httpContextAccessor.Object,
            _currentUser.Object,
            new FixedTimeProvider(new DateTime(2026, 11, 10, 10, 0, 0)));
    }


    [TestMethod]
    public async Task CreateAsync_ShouldCreateDeal_WhenRestaurantExists()
    {
        // -----------------------------
        // Arrange
        // Purpose:
        // Verify that a Deal is created
        // successfully when the Restaurant exists.
        // -----------------------------

        var restaurant = new Restaurant
        {
            Name = "Spice Kitchen",
            IsActive = true
        };

        _context.Restaurants.Add(restaurant);
        await _context.SaveChangesAsync();

        var dto = new CreateDealDto
        {
            RestaurantId = restaurant.Id,
            Title = "20% Lunch Discount",
            Description = "Lunch Special",
            DiscountPercentage = 20,
            OfferType = OfferType.DineIn,
            PromoImageUrl = "",
            TermsAndConditions = "",
            StartDate = DateOnly.FromDateTime(DateTime.Today),
            EndDate = DateOnly.FromDateTime(DateTime.Today.AddDays(30)),
            StartTime = new TimeOnly(12, 0),
            EndTime = new TimeOnly(15, 0),
            MaximumGuests = 4,
            ReservationLimit = 5,
            DailyRedemptionLimit = 100,
            IsActive = true
        };

        // -----------------------------
        // Act
        // -----------------------------

        var result = await _service.CreateAsync(dto);

        // -----------------------------
        // Assert
        // -----------------------------

        result.Should().NotBeNull();
        result.Title.Should().Be("20% Lunch Discount");
        result.ReservationLimit.Should().Be(5);

        _context.Deals.Count().Should().Be(1);
        _context.Deals.First().Title.Should().Be("20% Lunch Discount");
        _context.Deals.First().ReservationLimit.Should().Be(5);
    }


    [TestMethod]
    public async Task CreateAsync_ShouldThrowException_WhenRestaurantDoesNotExist()
    {
        // -----------------------------
        // Arrange
        // Purpose:
        // Verify that CreateAsync throws
        // an exception when the supplied
        // RestaurantId does not exist.
        // -----------------------------

        var dto = new CreateDealDto
        {
            RestaurantId = 9999,
            Title = "20% Lunch Discount",
            Description = "Lunch Special",
            DiscountPercentage = 20,
            OfferType = OfferType.DineIn,
            PromoImageUrl = "",
            TermsAndConditions = "",
            StartDate = DateOnly.FromDateTime(DateTime.Today),
            EndDate = DateOnly.FromDateTime(DateTime.Today.AddDays(30)),
            StartTime = new TimeOnly(12, 0),
            EndTime = new TimeOnly(15, 0),
            MaximumGuests = 4,
            DailyRedemptionLimit = 100,
            IsActive = true
        };

        // -----------------------------
        // Act & Assert
        // Purpose:
        // RestaurantId doesn't exist,
        // therefore CreateAsync should fail.
        // -----------------------------

        var ex = await Assert.ThrowsExceptionAsync<NotFoundException>(
            () => _service.CreateAsync(dto));

        ex.Message.Should().Be("Restaurant not found.");
    }


    // -----------------------------
    // Ownership rules
    // -----------------------------

    private static CreateDealDto BuildCreateDealDto(int restaurantId) => new()
    {
        RestaurantId = restaurantId,
        Title = "20% Lunch Discount",
        Description = "Lunch Special",
        DiscountPercentage = 20,
        OfferType = OfferType.DineIn,
        PromoImageUrl = "",
        TermsAndConditions = "",
        StartDate = DateOnly.FromDateTime(DateTime.Today),
        EndDate = DateOnly.FromDateTime(DateTime.Today.AddDays(30)),
        StartTime = new TimeOnly(12, 0),
        EndTime = new TimeOnly(15, 0),
        MaximumGuests = 4,
        ReservationLimit = 5,
        DailyRedemptionLimit = 100,
        IsActive = true
    };

    private static UpdateDealDto BuildUpdateDealDto() => new()
    {
        Title = "Updated Title",
        Description = "Updated Description",
        DiscountPercentage = 25,
        OfferType = OfferType.DineIn,
        PromoImageUrl = "",
        TermsAndConditions = "",
        StartDate = DateOnly.FromDateTime(DateTime.Today),
        EndDate = DateOnly.FromDateTime(DateTime.Today.AddDays(30)),
        StartTime = new TimeOnly(12, 0),
        EndTime = new TimeOnly(15, 0),
        MaximumGuests = 4,
        ReservationLimit = 5,
        DailyRedemptionLimit = 100,
        IsActive = true
    };

    [TestMethod]
    public async Task CreateAsync_ShouldSucceed_WhenOwnerOwnsRestaurant()
    {
        // -----------------------------
        // Arrange
        // Purpose:
        // An Owner who owns the restaurant
        // should be allowed to create a deal for it.
        // -----------------------------

        _currentUser.Setup(x => x.IsAdmin).Returns(false);
        _currentUser.Setup(x => x.UserId).Returns(1);

        var restaurant = new Restaurant
        {
            Name = "Spice Kitchen",
            OwnerId = 1,
            IsActive = true
        };

        _context.Restaurants.Add(restaurant);
        await _context.SaveChangesAsync();

        var dto = BuildCreateDealDto(restaurant.Id);

        // -----------------------------
        // Act
        // -----------------------------

        var result = await _service.CreateAsync(dto);

        // -----------------------------
        // Assert
        // -----------------------------

        result.Should().NotBeNull();
        _context.Deals.Count().Should().Be(1);
    }

    [TestMethod]
    public async Task CreateAsync_ShouldThrowBusinessRuleException_WhenOwnerDoesNotOwnRestaurant()
    {
        // -----------------------------
        // Arrange
        // Purpose:
        // An Owner must not be able to create
        // a deal for a restaurant they don't own.
        // -----------------------------

        _currentUser.Setup(x => x.IsAdmin).Returns(false);
        _currentUser.Setup(x => x.UserId).Returns(1);

        var restaurant = new Restaurant
        {
            Name = "Spice Kitchen",
            OwnerId = 2,
            IsActive = true
        };

        _context.Restaurants.Add(restaurant);
        await _context.SaveChangesAsync();

        var dto = BuildCreateDealDto(restaurant.Id);

        // -----------------------------
        // Act & Assert
        // -----------------------------

        await Assert.ThrowsExceptionAsync<BusinessRuleException>(
            () => _service.CreateAsync(dto));

        _context.Deals.Count().Should().Be(0);
    }

    [TestMethod]
    public async Task UpdateAsync_ShouldSucceed_WhenOwnerOwnsRestaurant()
    {
        // -----------------------------
        // Arrange
        // Purpose:
        // An Owner who owns the deal's restaurant
        // should be allowed to update it.
        // -----------------------------

        _currentUser.Setup(x => x.IsAdmin).Returns(false);
        _currentUser.Setup(x => x.UserId).Returns(1);

        var restaurant = new Restaurant
        {
            Name = "Spice Kitchen",
            OwnerId = 1,
            IsActive = true
        };

        _context.Restaurants.Add(restaurant);
        await _context.SaveChangesAsync();

        var deal = new Deal
        {
            RestaurantId = restaurant.Id,
            Title = "20% Lunch",
            DiscountPercentage = 20,
            IsActive = true
        };

        _context.Deals.Add(deal);
        await _context.SaveChangesAsync();

        var dto = BuildUpdateDealDto();

        // -----------------------------
        // Act
        // -----------------------------

        var result = await _service.UpdateAsync(deal.Id, dto);

        // -----------------------------
        // Assert
        // -----------------------------

        result.Should().NotBeNull();
        result.Title.Should().Be("Updated Title");
    }

    [TestMethod]
    public async Task UpdateAsync_ShouldThrowBusinessRuleException_WhenOwnerDoesNotOwnDeal()
    {
        // -----------------------------
        // Arrange
        // Purpose:
        // An Owner must not be able to update
        // a deal belonging to another owner's restaurant.
        // -----------------------------

        _currentUser.Setup(x => x.IsAdmin).Returns(false);
        _currentUser.Setup(x => x.UserId).Returns(1);

        var restaurant = new Restaurant
        {
            Name = "Spice Kitchen",
            OwnerId = 2,
            IsActive = true
        };

        _context.Restaurants.Add(restaurant);
        await _context.SaveChangesAsync();

        var deal = new Deal
        {
            RestaurantId = restaurant.Id,
            Title = "20% Lunch",
            DiscountPercentage = 20,
            IsActive = true
        };

        _context.Deals.Add(deal);
        await _context.SaveChangesAsync();

        var dto = BuildUpdateDealDto();

        // -----------------------------
        // Act & Assert
        // -----------------------------

        await Assert.ThrowsExceptionAsync<BusinessRuleException>(
            () => _service.UpdateAsync(deal.Id, dto));

        _context.Deals.First().Title.Should().Be("20% Lunch");
    }

    [TestMethod]
    public async Task UpdateAsync_ShouldSucceed_WhenUserIsAdmin_EvenIfNotOwner()
    {
        // -----------------------------
        // Arrange
        // Purpose:
        // An Admin should be able to update
        // any restaurant's deal.
        // -----------------------------

        _currentUser.Setup(x => x.IsAdmin).Returns(true);
        _currentUser.Setup(x => x.UserId).Returns(999);

        var restaurant = new Restaurant
        {
            Name = "Spice Kitchen",
            OwnerId = 2,
            IsActive = true
        };

        _context.Restaurants.Add(restaurant);
        await _context.SaveChangesAsync();

        var deal = new Deal
        {
            RestaurantId = restaurant.Id,
            Title = "20% Lunch",
            DiscountPercentage = 20,
            IsActive = true
        };

        _context.Deals.Add(deal);
        await _context.SaveChangesAsync();

        var dto = BuildUpdateDealDto();

        // -----------------------------
        // Act
        // -----------------------------

        var result = await _service.UpdateAsync(deal.Id, dto);

        // -----------------------------
        // Assert
        // -----------------------------

        result.Should().NotBeNull();
        result.Title.Should().Be("Updated Title");
    }

    [TestMethod]
    public async Task DeleteAsync_ShouldSucceed_WhenOwnerOwnsRestaurant()
    {
        // -----------------------------
        // Arrange
        // Purpose:
        // An Owner who owns the deal's restaurant
        // should be allowed to delete (soft-delete) it.
        // -----------------------------

        _currentUser.Setup(x => x.IsAdmin).Returns(false);
        _currentUser.Setup(x => x.UserId).Returns(1);

        var restaurant = new Restaurant
        {
            Name = "Spice Kitchen",
            OwnerId = 1,
            IsActive = true
        };

        _context.Restaurants.Add(restaurant);
        await _context.SaveChangesAsync();

        var deal = new Deal
        {
            RestaurantId = restaurant.Id,
            Title = "20% Lunch",
            DiscountPercentage = 20,
            IsActive = true
        };

        _context.Deals.Add(deal);
        await _context.SaveChangesAsync();

        // -----------------------------
        // Act
        // -----------------------------

        var result = await _service.DeleteAsync(deal.Id);

        // -----------------------------
        // Assert
        // -----------------------------

        result.Should().BeTrue();
        _context.Deals.First().IsActive.Should().BeFalse();
    }

    [TestMethod]
    public async Task DeleteAsync_ShouldThrowBusinessRuleException_WhenOwnerDoesNotOwnDeal()
    {
        // -----------------------------
        // Arrange
        // Purpose:
        // An Owner must not be able to delete
        // a deal belonging to another owner's restaurant.
        // -----------------------------

        _currentUser.Setup(x => x.IsAdmin).Returns(false);
        _currentUser.Setup(x => x.UserId).Returns(1);

        var restaurant = new Restaurant
        {
            Name = "Spice Kitchen",
            OwnerId = 2,
            IsActive = true
        };

        _context.Restaurants.Add(restaurant);
        await _context.SaveChangesAsync();

        var deal = new Deal
        {
            RestaurantId = restaurant.Id,
            Title = "20% Lunch",
            DiscountPercentage = 20,
            IsActive = true
        };

        _context.Deals.Add(deal);
        await _context.SaveChangesAsync();

        // -----------------------------
        // Act & Assert
        // -----------------------------

        await Assert.ThrowsExceptionAsync<BusinessRuleException>(
            () => _service.DeleteAsync(deal.Id));

        _context.Deals.First().IsActive.Should().BeTrue();
    }

    [TestMethod]
    public async Task DeleteAsync_ShouldSucceed_WhenUserIsAdmin_EvenIfNotOwner()
    {
        // -----------------------------
        // Arrange
        // Purpose:
        // An Admin should be able to delete
        // any restaurant's deal.
        // -----------------------------

        _currentUser.Setup(x => x.IsAdmin).Returns(true);
        _currentUser.Setup(x => x.UserId).Returns(999);

        var restaurant = new Restaurant
        {
            Name = "Spice Kitchen",
            OwnerId = 2,
            IsActive = true
        };

        _context.Restaurants.Add(restaurant);
        await _context.SaveChangesAsync();

        var deal = new Deal
        {
            RestaurantId = restaurant.Id,
            Title = "20% Lunch",
            DiscountPercentage = 20,
            IsActive = true
        };

        _context.Deals.Add(deal);
        await _context.SaveChangesAsync();

        // -----------------------------
        // Act
        // -----------------------------

        var result = await _service.DeleteAsync(deal.Id);

        // -----------------------------
        // Assert
        // -----------------------------

        result.Should().BeTrue();
        _context.Deals.First().IsActive.Should().BeFalse();
    }

    // =========================================================
    // GetByRestaurantAsync - walk-in offer availability
    // (clock fixed at 10 Nov 2026)
    // =========================================================

    private async Task<Deal> SeedAvailabilityDealAsync(
        DateOnly startDate,
        DateOnly endDate,
        int reservationLimit,
        int dailyRedemptionLimit)
    {
        var restaurant = new Restaurant { Name = "Momo House", IsActive = true };

        _context.Restaurants.Add(restaurant);
        await _context.SaveChangesAsync();

        var deal = new Deal
        {
            RestaurantId = restaurant.Id,
            Title = "25% Off - Dine In",
            DiscountPercentage = 25,
            StartDate = startDate,
            EndDate = endDate,
            StartTime = new TimeOnly(12, 0),
            EndTime = new TimeOnly(14, 0),
            MaximumGuests = 4,
            ReservationLimit = reservationLimit,
            DailyRedemptionLimit = dailyRedemptionLimit,
            IsActive = true
        };

        _context.Deals.Add(deal);
        await _context.SaveChangesAsync();

        return deal;
    }

    private async Task SeedClaimAsync(Deal deal, DateOnly date, RedemptionStatus status)
    {
        _context.Redemptions.Add(new Redemption
        {
            DealId = deal.Id,
            UserId = 1,
            ArrivalDate = date,
            ArrivalTime = new TimeOnly(12, 0),
            GuestCount = 2,
            Status = status
        });

        await _context.SaveChangesAsync();
    }

    [TestMethod]
    public async Task GetByRestaurantAsync_ShouldReturnLowerOfTotalAndDailyRemaining_ForToday()
    {
        // Total 10 with 3 active claims -> 7; daily 3 with 2 active
        // claims today -> 1. Cancelled claims do not count.
        var today = new DateOnly(2026, 11, 10);
        var deal = await SeedAvailabilityDealAsync(
            new DateOnly(2026, 11, 1), new DateOnly(2026, 11, 30), 10, 3);

        await SeedClaimAsync(deal, today, RedemptionStatus.Redeemed);
        await SeedClaimAsync(deal, today, RedemptionStatus.Completed);
        await SeedClaimAsync(deal, today, RedemptionStatus.Cancelled);
        await SeedClaimAsync(deal, today.AddDays(1), RedemptionStatus.Redeemed);

        var result = (await _service.GetByRestaurantAsync(deal.RestaurantId)).Single();

        result.AvailabilityDate.Should().Be(today);
        result.RemainingOffers.Should().Be(1);
        result.IsSoldOut.Should().BeFalse();
    }

    [TestMethod]
    public async Task GetByRestaurantAsync_ShouldBeSoldOut_OnlyWhenTotalLimitIsUsedUp()
    {
        var today = new DateOnly(2026, 11, 10);

        // Daily limit used up today, total still available -> not sold out.
        var dailyFull = await SeedAvailabilityDealAsync(
            new DateOnly(2026, 11, 1), new DateOnly(2026, 11, 30), 5, 1);
        await SeedClaimAsync(dailyFull, today, RedemptionStatus.Redeemed);

        var dailyResult = (await _service.GetByRestaurantAsync(dailyFull.RestaurantId)).Single();

        dailyResult.RemainingOffers.Should().Be(0);
        dailyResult.IsSoldOut.Should().BeFalse();

        // Total limit used up -> sold out.
        var totalFull = await SeedAvailabilityDealAsync(
            new DateOnly(2026, 11, 1), new DateOnly(2026, 11, 30), 1, 0);
        await SeedClaimAsync(totalFull, today.AddDays(2), RedemptionStatus.Completed);

        var totalResult = (await _service.GetByRestaurantAsync(totalFull.RestaurantId)).Single();

        totalResult.RemainingOffers.Should().Be(0);
        totalResult.IsSoldOut.Should().BeTrue();
    }

    [TestMethod]
    public async Task GetByRestaurantAsync_ShouldReturnNullRemaining_WhenUnlimited()
    {
        var deal = await SeedAvailabilityDealAsync(
            new DateOnly(2026, 11, 1), new DateOnly(2026, 11, 30), 0, 0);

        var result = (await _service.GetByRestaurantAsync(deal.RestaurantId)).Single();

        result.RemainingOffers.Should().BeNull();
        result.AvailabilityDate.Should().Be(new DateOnly(2026, 11, 10));
    }

    [TestMethod]
    public async Task GetByRestaurantAsync_ShouldUseStartDate_WhenDealHasNotStarted()
    {
        var startDate = new DateOnly(2026, 11, 20);
        var deal = await SeedAvailabilityDealAsync(startDate, new DateOnly(2026, 11, 30), 0, 5);

        await SeedClaimAsync(deal, startDate, RedemptionStatus.Redeemed);

        var result = (await _service.GetByRestaurantAsync(deal.RestaurantId)).Single();

        result.AvailabilityDate.Should().Be(startDate);
        result.RemainingOffers.Should().Be(4);
    }

    [TestMethod]
    public async Task GetByRestaurantAsync_ShouldReturnZeroRemaining_WhenDealHasEnded()
    {
        var deal = await SeedAvailabilityDealAsync(
            new DateOnly(2026, 10, 1), new DateOnly(2026, 10, 31), 0, 0);

        var result = (await _service.GetByRestaurantAsync(deal.RestaurantId)).Single();

        result.RemainingOffers.Should().Be(0);
        result.AvailabilityDate.Should().BeNull();
    }
}