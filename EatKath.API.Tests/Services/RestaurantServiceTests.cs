using AutoMapper;
using EatKath.API.Data;
using EatKath.API.DTOs.Restaurant;
using EatKath.API.Entities;
using EatKath.API.Enums;
using EatKath.API.Interfaces;
using EatKath.API.Mappings;
using EatKath.API.Services;
using EatKath.API.Tests.Helpers;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace EatKath.API.Tests.Services;

[TestClass]
public class RestaurantServiceTests
{
    private ApplicationDbContext _context = null!;
    private IMapper _mapper = null!;
    private Mock<ICurrentUserService> _currentUser = null!;
    private FileStorageService _fileStorage = null!;
    private RestaurantService _service = null!;

    // -----------------------------
    // Restaurant <-> RestaurantDto mappings live in
    // RestaurantProfile, a separate AutoMapper profile
    // from MappingProfile (the one the shared MapperFactory
    // helper registers). CreateAsync needs RestaurantProfile,
    // so the mapper is built locally here rather than via
    // MapperFactory, without touching any shared test helper.
    // -----------------------------

    [TestInitialize]
    public void Setup()
    {
        _context = TestDbContextFactory.Create();

        var config = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<MappingProfile>();
            cfg.AddProfile<RestaurantProfile>();
        });

        _mapper = config.CreateMapper();

        _currentUser = new Mock<ICurrentUserService>();

        // Default: an Admin caller, so tests that don't
        // care about ownership continue to behave predictably.
        _currentUser.Setup(x => x.UserId).Returns(1);
        _currentUser.Setup(x => x.IsAdmin).Returns(true);

        // CreateAsync never touches file storage - a real
        // FileStorageService is only constructed here to
        // satisfy the constructor.
        var webHostEnvironment = new Mock<IWebHostEnvironment>();
        webHostEnvironment.Setup(e => e.WebRootPath).Returns(Path.GetTempPath());
        _fileStorage = new FileStorageService(webHostEnvironment.Object);

        _service = new RestaurantService(
            _context,
            _mapper,
            _fileStorage,
            _currentUser.Object);
    }

    private async Task<int> SeedAreaAsync()
    {
        var area = new Area
        {
            Name = "Thamel"
        };

        _context.Areas.Add(area);
        await _context.SaveChangesAsync();

        return area.Id;
    }

    private static CreateRestaurantDto BuildCreateDto(int ownerId, int areaId) => new()
    {
        OwnerId = ownerId,
        Name = "Spice Kitchen",
        Description = "Nepali cuisine",
        Address = "Thamel, Kathmandu",
        AreaId = areaId,
        PhoneNumber = "0400000000",
        Email = "spice@test.com",
        Website = "",
        LogoUrl = "",
        IsActive = true
    };


    [TestMethod]
    public async Task CreateAsync_ShouldForceOwnerIdToCurrentUser_WhenOwnerSuppliesDifferentOwnerId()
    {
        // -----------------------------
        // Arrange
        // Purpose:
        // A non-admin Owner must not be able to plant
        // a restaurant under another account's OwnerId.
        // -----------------------------

        _currentUser.Setup(x => x.IsAdmin).Returns(false);
        _currentUser.Setup(x => x.UserId).Returns(10);

        var areaId = await SeedAreaAsync();

        var dto = BuildCreateDto(ownerId: 999, areaId: areaId);

        // -----------------------------
        // Act
        // -----------------------------

        var result = await _service.CreateAsync(dto);

        // -----------------------------
        // Assert
        // -----------------------------

        result.Should().NotBeNull();

        var saved = _context.Restaurants.First();

        saved.OwnerId.Should().Be(10);
        saved.Name.Should().Be("Spice Kitchen");
    }

    [TestMethod]
    public async Task CreateAsync_ShouldKeepOwnerIdAsCurrentUser_WhenOwnerSuppliesMatchingOwnerId()
    {
        // -----------------------------
        // Arrange
        // Purpose:
        // A non-admin Owner creating a restaurant with
        // their own id in the DTO should be unaffected.
        // -----------------------------

        _currentUser.Setup(x => x.IsAdmin).Returns(false);
        _currentUser.Setup(x => x.UserId).Returns(10);

        var areaId = await SeedAreaAsync();

        var dto = BuildCreateDto(ownerId: 10, areaId: areaId);

        // -----------------------------
        // Act
        // -----------------------------

        var result = await _service.CreateAsync(dto);

        // -----------------------------
        // Assert
        // -----------------------------

        result.Should().NotBeNull();

        var saved = _context.Restaurants.First();

        saved.OwnerId.Should().Be(10);
    }

    [TestMethod]
    public async Task CreateAsync_ShouldPreserveSuppliedOwnerId_WhenUserIsAdmin()
    {
        // -----------------------------
        // Arrange
        // Purpose:
        // Admin must retain the ability to create a
        // restaurant on behalf of a specific owner.
        // -----------------------------

        _currentUser.Setup(x => x.IsAdmin).Returns(true);
        _currentUser.Setup(x => x.UserId).Returns(1);

        var areaId = await SeedAreaAsync();

        var dto = BuildCreateDto(ownerId: 999, areaId: areaId);

        // -----------------------------
        // Act
        // -----------------------------

        var result = await _service.CreateAsync(dto);

        // -----------------------------
        // Assert
        // -----------------------------

        result.Should().NotBeNull();

        var saved = _context.Restaurants.First();

        saved.OwnerId.Should().Be(999);
        saved.Name.Should().Be("Spice Kitchen");
    }


    // -----------------------------
    // DeleteAsync
    // -----------------------------

    [TestMethod]
    public async Task DeleteAsync_ShouldReturnFalse_WhenRestaurantDoesNotExist()
    {
        // -----------------------------
        // Act
        // -----------------------------

        var result = await _service.DeleteAsync(9999);

        // -----------------------------
        // Assert
        // -----------------------------

        result.Should().BeFalse();
    }

    [TestMethod]
    public async Task DeleteAsync_ShouldThrowBusinessRuleException_WhenOwnerDoesNotOwnRestaurant()
    {
        // -----------------------------
        // Arrange
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

        // -----------------------------
        // Act & Assert
        // -----------------------------

        var ex = await Assert.ThrowsExceptionAsync<BusinessRuleException>(
            () => _service.DeleteAsync(restaurant.Id));

        ex.Message.Should().Be("You are not authorized to delete this restaurant.");

        _context.Restaurants.Count().Should().Be(1);
    }

    [TestMethod]
    public async Task DeleteAsync_ShouldSucceed_WhenRestaurantHasNoDependentRecords()
    {
        // -----------------------------
        // Arrange
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

        // -----------------------------
        // Act
        // -----------------------------

        var result = await _service.DeleteAsync(restaurant.Id);

        // -----------------------------
        // Assert
        // -----------------------------

        result.Should().BeTrue();
        _context.Restaurants.Count().Should().Be(0);
    }

    [TestMethod]
    public async Task DeleteAsync_ShouldThrowBusinessRuleException_WhenRestaurantHasMenuItems()
    {
        // -----------------------------
        // Arrange
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

        _context.MenuItems.Add(new MenuItem
        {
            RestaurantId = restaurant.Id,
            MenuCategoryId = 1,
            Name = "Spring Rolls",
            Description = "Crispy rolls",
            Price = 8.99m,
            ImageUrl = string.Empty
        });

        await _context.SaveChangesAsync();

        // -----------------------------
        // Act & Assert
        // -----------------------------

        var ex = await Assert.ThrowsExceptionAsync<BusinessRuleException>(
            () => _service.DeleteAsync(restaurant.Id));

        ex.Message.Should().Be("Cannot delete this restaurant because it has existing menu items or redeemed deals.");

        _context.Restaurants.Count().Should().Be(1);
    }

    [TestMethod]
    public async Task DeleteAsync_ShouldThrowBusinessRuleException_WhenRestaurantHasRedeemedDeals()
    {
        // -----------------------------
        // Arrange
        // Purpose:
        // Verify the Redemption -> Deal -> Restaurant chain is
        // checked correctly via a query (not by assuming the
        // Deal/Restaurant navigation is already loaded).
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

        _context.Redemptions.Add(new Redemption
        {
            DealId = deal.Id,
            UserId = 50,
            ArrivalDate = DateOnly.FromDateTime(DateTime.Today),
            ArrivalTime = new TimeOnly(12, 30),
            GuestCount = 2,
            Status = RedemptionStatus.Redeemed
        });

        await _context.SaveChangesAsync();

        // -----------------------------
        // Act & Assert
        // -----------------------------

        var ex = await Assert.ThrowsExceptionAsync<BusinessRuleException>(
            () => _service.DeleteAsync(restaurant.Id));

        ex.Message.Should().Be("Cannot delete this restaurant because it has existing menu items or redeemed deals.");

        _context.Restaurants.Count().Should().Be(1);
    }

    [TestMethod]
    public async Task DeleteAsync_ShouldSucceed_WhenUserIsAdmin_EvenIfNotOwner()
    {
        // -----------------------------
        // Arrange
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

        // -----------------------------
        // Act
        // -----------------------------

        var result = await _service.DeleteAsync(restaurant.Id);

        // -----------------------------
        // Assert
        // -----------------------------

        result.Should().BeTrue();
        _context.Restaurants.Count().Should().Be(0);
    }
}
