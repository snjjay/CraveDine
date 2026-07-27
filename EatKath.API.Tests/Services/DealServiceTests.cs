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
    private DealService _service = null!;

    [TestInitialize]
    public void Setup()
    {
        _context = TestDbContextFactory.Create();

        _mapper = MapperFactory.Create();

        _httpContextAccessor = new Mock<IHttpContextAccessor>();

        _service = new DealService(
            _context,
            _mapper,
            _httpContextAccessor.Object);
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

        _context.Deals.Count().Should().Be(1);
        _context.Deals.First().Title.Should().Be("20% Lunch Discount");
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

        var ex = await Assert.ThrowsExceptionAsync<Exception>(
            () => _service.CreateAsync(dto));

        ex.Message.Should().Be("Restaurant not found.");
    }
}