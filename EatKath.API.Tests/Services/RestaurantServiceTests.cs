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
    // CreateAsync - default opening hours
    // -----------------------------

    [TestMethod]
    public async Task CreateAsync_ShouldCreateExactlySevenOpeningHours()
    {
        // -----------------------------
        // Arrange
        // -----------------------------

        var areaId = await SeedAreaAsync();

        var dto = BuildCreateDto(ownerId: 1, areaId: areaId);

        // -----------------------------
        // Act
        // -----------------------------

        var result = await _service.CreateAsync(dto);

        // -----------------------------
        // Assert
        // -----------------------------

        _context.RestaurantOpeningHours
            .Where(h => h.RestaurantId == result.Id)
            .Should().HaveCount(7);
    }

    [TestMethod]
    public async Task CreateAsync_ShouldCreateOpeningHoursForAllSevenDaysExactlyOnce()
    {
        // -----------------------------
        // Arrange
        // -----------------------------

        var areaId = await SeedAreaAsync();

        var dto = BuildCreateDto(ownerId: 1, areaId: areaId);

        // -----------------------------
        // Act
        // -----------------------------

        var result = await _service.CreateAsync(dto);

        // -----------------------------
        // Assert
        // -----------------------------

        var days = _context.RestaurantOpeningHours
            .Where(h => h.RestaurantId == result.Id)
            .Select(h => h.DayOfWeek)
            .ToList();

        days.Should().BeEquivalentTo(new[]
        {
            DayOfWeek.Sunday,
            DayOfWeek.Monday,
            DayOfWeek.Tuesday,
            DayOfWeek.Wednesday,
            DayOfWeek.Thursday,
            DayOfWeek.Friday,
            DayOfWeek.Saturday
        });

        // None of the default rows start out closed, and the
        // generic (non-bakery/non-cafe) schedule from
        // RestaurantOpeningHourSeeder.cs is reused exactly.
        var saturday = _context.RestaurantOpeningHours
            .Single(h => h.RestaurantId == result.Id && h.DayOfWeek == DayOfWeek.Saturday);

        saturday.IsClosed.Should().BeFalse();
        saturday.OpenTime.Should().Be(new TimeOnly(10, 0));
        saturday.CloseTime.Should().Be(new TimeOnly(22, 0));

        var sunday = _context.RestaurantOpeningHours
            .Single(h => h.RestaurantId == result.Id && h.DayOfWeek == DayOfWeek.Sunday);

        sunday.OpenTime.Should().Be(new TimeOnly(11, 0));
        sunday.CloseTime.Should().Be(new TimeOnly(20, 0));

        var monday = _context.RestaurantOpeningHours
            .Single(h => h.RestaurantId == result.Id && h.DayOfWeek == DayOfWeek.Monday);

        monday.OpenTime.Should().Be(new TimeOnly(10, 0));
        monday.CloseTime.Should().Be(new TimeOnly(21, 0));
    }

    [TestMethod]
    public async Task CreateAsync_ShouldLinkOpeningHoursToTheNewlyCreatedRestaurant()
    {
        // -----------------------------
        // Arrange
        // -----------------------------

        var areaId = await SeedAreaAsync();

        var dto = BuildCreateDto(ownerId: 1, areaId: areaId);

        // -----------------------------
        // Act
        // -----------------------------

        var result = await _service.CreateAsync(dto);

        // -----------------------------
        // Assert
        // -----------------------------

        var openingHours = _context.RestaurantOpeningHours.ToList();

        openingHours.Should().OnlyContain(h => h.RestaurantId == result.Id);
    }

    [TestMethod]
    public async Task CreateAsync_ShouldNotAffectExistingRestaurantsOpeningHours()
    {
        // -----------------------------
        // Arrange
        // Purpose:
        // Creating a new restaurant must not touch or duplicate the
        // opening hours already belonging to a different, pre-existing
        // restaurant.
        // -----------------------------

        var existingRestaurant = new Restaurant
        {
            Name = "Existing Kitchen",
            OwnerId = 1,
            IsActive = true
        };

        _context.Restaurants.Add(existingRestaurant);
        await _context.SaveChangesAsync();

        var existingOpeningHour = new RestaurantOpeningHour
        {
            RestaurantId = existingRestaurant.Id,
            DayOfWeek = DayOfWeek.Monday,
            OpenTime = new TimeOnly(9, 0),
            CloseTime = new TimeOnly(17, 0),
            IsClosed = false
        };

        _context.RestaurantOpeningHours.Add(existingOpeningHour);
        await _context.SaveChangesAsync();

        var areaId = await SeedAreaAsync();

        var dto = BuildCreateDto(ownerId: 1, areaId: areaId);

        // -----------------------------
        // Act
        // -----------------------------

        await _service.CreateAsync(dto);

        // -----------------------------
        // Assert
        // -----------------------------

        var existingRestaurantHours = _context.RestaurantOpeningHours
            .Where(h => h.RestaurantId == existingRestaurant.Id)
            .ToList();

        existingRestaurantHours.Should().HaveCount(1);

        existingRestaurantHours.Single().OpenTime.Should().Be(new TimeOnly(9, 0));
        existingRestaurantHours.Single().CloseTime.Should().Be(new TimeOnly(17, 0));
    }


    // -----------------------------
    // CreateAsync - CurrencyCode
    // -----------------------------

    [TestMethod]
    public async Task CreateAsync_ShouldDefaultCurrencyCodeToNpr_WhenNotExplicitlySelected()
    {
        // -----------------------------
        // Arrange
        // -----------------------------

        var areaId = await SeedAreaAsync();

        var dto = BuildCreateDto(ownerId: 1, areaId: areaId);

        // -----------------------------
        // Act
        // -----------------------------

        var result = await _service.CreateAsync(dto);

        // -----------------------------
        // Assert
        // -----------------------------

        result.CurrencyCode.Should().Be("NPR");
        _context.Restaurants.First().CurrencyCode.Should().Be("NPR");
    }

    [TestMethod]
    public async Task CreateAsync_ShouldUseExplicitlySelectedCurrency()
    {
        // -----------------------------
        // Arrange
        // -----------------------------

        var areaId = await SeedAreaAsync();

        var dto = BuildCreateDto(ownerId: 1, areaId: areaId);
        dto.CurrencyCode = "AUD";

        // -----------------------------
        // Act
        // -----------------------------

        var result = await _service.CreateAsync(dto);

        // -----------------------------
        // Assert
        // -----------------------------

        result.CurrencyCode.Should().Be("AUD");
        _context.Restaurants.First().CurrencyCode.Should().Be("AUD");
    }

    [TestMethod]
    public async Task CreateAsync_ShouldThrowBusinessRuleException_WhenCurrencyCodeIsNotSupported()
    {
        // -----------------------------
        // Arrange
        // -----------------------------

        var areaId = await SeedAreaAsync();

        var dto = BuildCreateDto(ownerId: 1, areaId: areaId);
        dto.CurrencyCode = "XYZ";

        // -----------------------------
        // Act & Assert
        // -----------------------------

        var ex = await Assert.ThrowsExceptionAsync<BusinessRuleException>(
            () => _service.CreateAsync(dto));

        ex.Message.Should().Be("'XYZ' is not a supported currency.");

        _context.Restaurants.Should().BeEmpty();
    }


    // -----------------------------
    // UpdateAsync - CurrencyCode
    // -----------------------------

    private static UpdateRestaurantDto BuildUpdateDto(Restaurant restaurant, string currencyCode) => new()
    {
        Name = restaurant.Name,
        Description = restaurant.Description,
        Address = restaurant.Address,
        AreaId = restaurant.AreaId,
        PhoneNumber = restaurant.PhoneNumber,
        Email = restaurant.Email,
        Website = restaurant.Website,
        LogoUrl = restaurant.LogoUrl,
        CurrencyCode = currencyCode,
        IsActive = restaurant.IsActive
    };

    [TestMethod]
    public async Task UpdateAsync_ShouldAllowChangingCurrency_WhenNoMonetaryTransactionsExist()
    {
        // -----------------------------
        // Arrange
        // -----------------------------

        var areaId = await SeedAreaAsync();

        var restaurant = new Restaurant
        {
            Name = "Spice Kitchen",
            OwnerId = 1,
            AreaId = areaId,
            CurrencyCode = "NPR",
            IsActive = true
        };

        _context.Restaurants.Add(restaurant);
        await _context.SaveChangesAsync();

        var dto = BuildUpdateDto(restaurant, "AUD");

        // -----------------------------
        // Act
        // -----------------------------

        var result = await _service.UpdateAsync(restaurant.Id, dto);

        // -----------------------------
        // Assert
        // -----------------------------

        result.Should().NotBeNull();
        result!.CurrencyCode.Should().Be("AUD");
        _context.Restaurants.First().CurrencyCode.Should().Be("AUD");
    }

    [TestMethod]
    public async Task UpdateAsync_ShouldThrowBusinessRuleException_WhenCurrencyCodeIsNotSupported()
    {
        // -----------------------------
        // Arrange
        // -----------------------------

        var areaId = await SeedAreaAsync();

        var restaurant = new Restaurant
        {
            Name = "Spice Kitchen",
            OwnerId = 1,
            AreaId = areaId,
            CurrencyCode = "NPR",
            IsActive = true
        };

        _context.Restaurants.Add(restaurant);
        await _context.SaveChangesAsync();

        var dto = BuildUpdateDto(restaurant, "ZZZ");

        // -----------------------------
        // Act & Assert
        // -----------------------------

        var ex = await Assert.ThrowsExceptionAsync<BusinessRuleException>(
            () => _service.UpdateAsync(restaurant.Id, dto));

        ex.Message.Should().Be("'ZZZ' is not a supported currency.");

        _context.Restaurants.First().CurrencyCode.Should().Be("NPR");
    }

    [TestMethod]
    public async Task UpdateAsync_ShouldThrowBusinessRuleException_WhenChangingCurrencyWithExistingMonetaryTransactions()
    {
        // -----------------------------
        // Arrange
        // Purpose:
        // Once a real monetary amount has been recorded (a completed
        // Redemption with BillAmount set), changing the restaurant's
        // currency must be blocked - it would silently relabel that
        // historical amount under a different currency.
        // -----------------------------

        var areaId = await SeedAreaAsync();

        var restaurant = new Restaurant
        {
            Name = "Spice Kitchen",
            OwnerId = 1,
            AreaId = areaId,
            CurrencyCode = "NPR",
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
            Status = RedemptionStatus.Completed,
            BillAmount = 1000m,
            DiscountAmount = 200m,
            FinalAmount = 800m
        });

        await _context.SaveChangesAsync();

        var dto = BuildUpdateDto(restaurant, "AUD");

        // -----------------------------
        // Act & Assert
        // -----------------------------

        var ex = await Assert.ThrowsExceptionAsync<BusinessRuleException>(
            () => _service.UpdateAsync(restaurant.Id, dto));

        ex.Message.Should().Be("Cannot change currency because this restaurant already has completed transactions with recorded monetary amounts.");

        _context.Restaurants.First().CurrencyCode.Should().Be("NPR");
    }

    [TestMethod]
    public async Task UpdateAsync_ShouldAllowUpdate_WhenCurrencyUnchangedDespiteExistingMonetaryTransactions()
    {
        // -----------------------------
        // Arrange
        // Purpose:
        // The historical-transaction guard only blocks an actual
        // currency CHANGE - updating other fields while keeping the
        // same currency must still work normally.
        // -----------------------------

        var areaId = await SeedAreaAsync();

        var restaurant = new Restaurant
        {
            Name = "Spice Kitchen",
            OwnerId = 1,
            AreaId = areaId,
            CurrencyCode = "NPR",
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
            Status = RedemptionStatus.Completed,
            BillAmount = 1000m,
            DiscountAmount = 200m,
            FinalAmount = 800m
        });

        await _context.SaveChangesAsync();

        var dto = BuildUpdateDto(restaurant, "NPR");
        dto.Name = "Spice Kitchen Renamed";

        // -----------------------------
        // Act
        // -----------------------------

        var result = await _service.UpdateAsync(restaurant.Id, dto);

        // -----------------------------
        // Assert
        // -----------------------------

        result.Should().NotBeNull();
        result!.Name.Should().Be("Spice Kitchen Renamed");
        result.CurrencyCode.Should().Be("NPR");
    }


    // -----------------------------
    // GetByIdAsync - CurrencyCode
    // -----------------------------

    [TestMethod]
    public async Task GetByIdAsync_ShouldIncludeCurrencyCode()
    {
        // -----------------------------
        // Arrange
        // -----------------------------

        var areaId = await SeedAreaAsync();

        var restaurant = new Restaurant
        {
            Name = "Spice Kitchen",
            OwnerId = 1,
            AreaId = areaId,
            CurrencyCode = "USD",
            IsActive = true
        };

        _context.Restaurants.Add(restaurant);
        await _context.SaveChangesAsync();

        // -----------------------------
        // Act
        // -----------------------------

        var result = await _service.GetByIdAsync(restaurant.Id);

        // -----------------------------
        // Assert
        // -----------------------------

        result.Should().NotBeNull();
        result!.CurrencyCode.Should().Be("USD");
    }


    // -----------------------------
    // GetByIdAsync - opening hours (customer-facing details response)
    // -----------------------------

    [TestMethod]
    public async Task GetByIdAsync_ShouldIncludeOpeningHours_OrderedByDayOfWeek()
    {
        // -----------------------------
        // Arrange
        // -----------------------------

        var areaId = await SeedAreaAsync();

        var restaurant = new Restaurant
        {
            Name = "Spice Kitchen",
            OwnerId = 1,
            AreaId = areaId,
            IsActive = true
        };

        _context.Restaurants.Add(restaurant);
        await _context.SaveChangesAsync();

        // Inserted out of day-of-week order, to prove the service
        // orders the response rather than returning insertion order.
        _context.RestaurantOpeningHours.AddRange(
            new RestaurantOpeningHour
            {
                RestaurantId = restaurant.Id,
                DayOfWeek = DayOfWeek.Friday,
                OpenTime = new TimeOnly(10, 0),
                CloseTime = new TimeOnly(22, 0),
                IsClosed = false
            },
            new RestaurantOpeningHour
            {
                RestaurantId = restaurant.Id,
                DayOfWeek = DayOfWeek.Sunday,
                OpenTime = new TimeOnly(11, 0),
                CloseTime = new TimeOnly(20, 0),
                IsClosed = false
            },
            new RestaurantOpeningHour
            {
                RestaurantId = restaurant.Id,
                DayOfWeek = DayOfWeek.Monday,
                OpenTime = new TimeOnly(0, 0),
                CloseTime = new TimeOnly(0, 0),
                IsClosed = true
            });

        await _context.SaveChangesAsync();

        // -----------------------------
        // Act
        // -----------------------------

        var result = await _service.GetByIdAsync(restaurant.Id);

        // -----------------------------
        // Assert
        // -----------------------------

        result.Should().NotBeNull();

        result!.OpeningHours.Should().HaveCount(3);

        result.OpeningHours
            .Select(h => h.DayOfWeek)
            .Should().ContainInOrder(
                DayOfWeek.Sunday,
                DayOfWeek.Monday,
                DayOfWeek.Friday);

        var monday = result.OpeningHours.Single(h => h.DayOfWeek == DayOfWeek.Monday);
        monday.IsClosed.Should().BeTrue();

        var friday = result.OpeningHours.Single(h => h.DayOfWeek == DayOfWeek.Friday);
        friday.IsClosed.Should().BeFalse();
        friday.OpenTime.Should().Be(new TimeOnly(10, 0));
        friday.CloseTime.Should().Be(new TimeOnly(22, 0));
    }

    [TestMethod]
    public async Task GetByIdAsync_ShouldReturnEmptyOpeningHours_WhenNoneExist()
    {
        // -----------------------------
        // Arrange
        // Purpose:
        // A restaurant with no opening-hour rows yet must not crash
        // GetByIdAsync - the customer details page must be able to
        // handle this gracefully.
        // -----------------------------

        var areaId = await SeedAreaAsync();

        var restaurant = new Restaurant
        {
            Name = "Spice Kitchen",
            OwnerId = 1,
            AreaId = areaId,
            IsActive = true
        };

        _context.Restaurants.Add(restaurant);
        await _context.SaveChangesAsync();

        // -----------------------------
        // Act
        // -----------------------------

        var result = await _service.GetByIdAsync(restaurant.Id);

        // -----------------------------
        // Assert
        // -----------------------------

        result.Should().NotBeNull();
        result!.OpeningHours.Should().NotBeNull();
        result.OpeningHours.Should().BeEmpty();
    }

    [TestMethod]
    public async Task GetByIdAsync_ShouldOnlyIncludeOpeningHoursForTheRequestedRestaurant()
    {
        // -----------------------------
        // Arrange
        // Purpose:
        // Confirms the customer-facing response for one restaurant
        // never leaks another restaurant's opening hours.
        // -----------------------------

        var areaId = await SeedAreaAsync();

        var restaurantA = new Restaurant
        {
            Name = "Spice Kitchen",
            OwnerId = 1,
            AreaId = areaId,
            IsActive = true
        };

        var restaurantB = new Restaurant
        {
            Name = "Momo House",
            OwnerId = 1,
            AreaId = areaId,
            IsActive = true
        };

        _context.Restaurants.AddRange(restaurantA, restaurantB);
        await _context.SaveChangesAsync();

        _context.RestaurantOpeningHours.AddRange(
            new RestaurantOpeningHour
            {
                RestaurantId = restaurantA.Id,
                DayOfWeek = DayOfWeek.Monday,
                OpenTime = new TimeOnly(10, 0),
                CloseTime = new TimeOnly(21, 0),
                IsClosed = false
            },
            new RestaurantOpeningHour
            {
                RestaurantId = restaurantB.Id,
                DayOfWeek = DayOfWeek.Tuesday,
                OpenTime = new TimeOnly(9, 0),
                CloseTime = new TimeOnly(18, 0),
                IsClosed = false
            });

        await _context.SaveChangesAsync();

        // -----------------------------
        // Act
        // -----------------------------

        var result = await _service.GetByIdAsync(restaurantA.Id);

        // -----------------------------
        // Assert
        // -----------------------------

        result.Should().NotBeNull();
        result!.OpeningHours.Should().HaveCount(1);
        result.OpeningHours.Single().RestaurantId.Should().Be(restaurantA.Id);
        result.OpeningHours.Single().DayOfWeek.Should().Be(DayOfWeek.Monday);
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
