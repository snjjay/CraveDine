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

    // A cuisine seeded for every test: CreateAsync requires at least one.
    private int _defaultCuisineId;

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

        // "Now" for deal-summary availability: 10 Nov 2026, 10:00.
        _service = new RestaurantService(
            _context,
            _mapper,
            _fileStorage,
            _currentUser.Object,
            new FixedTimeProvider(new DateTime(2026, 11, 10, 10, 0, 0)));

        var defaultCuisine = new Cuisine { Name = "Nepali" };
        _context.Cuisines.Add(defaultCuisine);
        _context.SaveChanges();
        _defaultCuisineId = defaultCuisine.Id;
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

    private CreateRestaurantDto BuildCreateDto(int ownerId, int areaId) => new()
    {
        CuisineIds = new List<int> { _defaultCuisineId },
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


    // =========================================================
    // GetAllAsync - deal summaries for restaurant cards
    // (clock fixed at 10 Nov 2026)
    // =========================================================

    private async Task<Restaurant> SeedRestaurantWithDealsAsync(params Deal[] deals)
    {
        var areaId = await SeedAreaAsync();

        var restaurant = new Restaurant
        {
            Name = "Momo House",
            OwnerId = 1,
            AreaId = areaId,
            IsActive = true
        };

        _context.Restaurants.Add(restaurant);
        await _context.SaveChangesAsync();

        foreach (var deal in deals)
        {
            deal.RestaurantId = restaurant.Id;
            _context.Deals.Add(deal);
        }

        await _context.SaveChangesAsync();

        return restaurant;
    }

    private static Deal SummaryDeal(
        decimal discount,
        OfferType offerType,
        DateOnly? startDate = null,
        DateOnly? endDate = null,
        bool isActive = true,
        int totalLimit = 0,
        int dailyLimit = 0)
    {
        return new Deal
        {
            Title = $"{discount}% deal",
            DiscountPercentage = discount,
            OfferType = offerType,
            StartDate = startDate ?? new DateOnly(2026, 11, 1),
            EndDate = endDate ?? new DateOnly(2026, 11, 30),
            StartTime = new TimeOnly(17, 0),
            EndTime = new TimeOnly(18, 0),
            MaximumGuests = 4,
            ReservationLimit = totalLimit,
            DailyRedemptionLimit = dailyLimit,
            IsActive = isActive
        };
    }

    private async Task SeedRedemptionAsync(Deal deal, DateOnly date, RedemptionStatus status)
    {
        _context.Redemptions.Add(new Redemption
        {
            DealId = deal.Id,
            UserId = 1,
            ArrivalDate = date,
            ArrivalTime = new TimeOnly(17, 0),
            GuestCount = 2,
            Status = status
        });

        await _context.SaveChangesAsync();
    }

    [TestMethod]
    public async Task GetAllAsync_ShouldIncludeOnlyActiveNotEndedDealSummaries_HighestDiscountFirst()
    {
        var dineIn = SummaryDeal(30, OfferType.DineIn);
        var takeaway = SummaryDeal(10, OfferType.Takeaway);
        var inactive = SummaryDeal(50, OfferType.DineIn, isActive: false);
        var ended = SummaryDeal(40, OfferType.DineIn, new DateOnly(2026, 10, 1), new DateOnly(2026, 10, 31));

        await SeedRestaurantWithDealsAsync(dineIn, takeaway, inactive, ended);

        var result = (await _service.GetAllAsync()).Single();

        result.DealSummaries.Select(s => s.Id).Should().Equal(dineIn.Id, takeaway.Id);

        var first = result.DealSummaries[0];
        first.DiscountPercentage.Should().Be(30);
        first.OfferType.Should().Be(OfferType.DineIn);
        first.StartTime.Should().Be(new TimeOnly(17, 0));
        first.EndTime.Should().Be(new TimeOnly(18, 0));
        first.AvailabilityDate.Should().Be(new DateOnly(2026, 11, 10));
        first.RemainingOffers.Should().BeNull();
        first.IsSoldOut.Should().BeFalse();

        // Existing list fields are unchanged (they count all active deals,
        // including ended ones; inactive deals are excluded).
        result.ActiveDeals.Should().Be(3);
        result.BestDiscount.Should().Be(40);
    }

    [TestMethod]
    public async Task GetAllAsync_DealSummaries_ShouldUseSameAvailabilityRulesAsDealList()
    {
        // Daily 2 with 1 active claim today -> 1 left; total 5 with 2
        // active claims -> 3 left; remaining = lower of the two (1).
        // Cancelled claims do not count.
        var today = new DateOnly(2026, 11, 10);
        var deal = SummaryDeal(25, OfferType.DineIn, totalLimit: 5, dailyLimit: 2);

        await SeedRestaurantWithDealsAsync(deal);

        await SeedRedemptionAsync(deal, today, RedemptionStatus.Redeemed);
        await SeedRedemptionAsync(deal, today.AddDays(1), RedemptionStatus.Completed);
        await SeedRedemptionAsync(deal, today, RedemptionStatus.Cancelled);

        var summary = (await _service.GetAllAsync()).Single().DealSummaries.Single();

        var expected = await DealCapacityCalculator.GetCapacityAsync(_context, deal, today);

        summary.RemainingOffers.Should().Be(1);
        summary.RemainingOffers.Should().Be(expected.Remaining);
        summary.IsSoldOut.Should().BeFalse();
    }

    [TestMethod]
    public async Task GetAllAsync_DealSummaries_ShouldMarkSoldOut_WhenTotalLimitIsUsedUp()
    {
        var deal = SummaryDeal(20, OfferType.Takeaway, totalLimit: 1);

        await SeedRestaurantWithDealsAsync(deal);

        await SeedRedemptionAsync(deal, new DateOnly(2026, 11, 12), RedemptionStatus.Redeemed);

        var summary = (await _service.GetAllAsync()).Single().DealSummaries.Single();

        summary.IsSoldOut.Should().BeTrue();
        summary.RemainingOffers.Should().Be(0);
    }

    [TestMethod]
    public async Task GetAllAsync_DealSummaries_ShouldUseStartDate_WhenDealHasNotStarted()
    {
        var startDate = new DateOnly(2026, 11, 20);
        var deal = SummaryDeal(15, OfferType.DineIn, startDate: startDate, dailyLimit: 3);

        await SeedRestaurantWithDealsAsync(deal);

        await SeedRedemptionAsync(deal, startDate, RedemptionStatus.Redeemed);

        var summary = (await _service.GetAllAsync()).Single().DealSummaries.Single();

        summary.AvailabilityDate.Should().Be(startDate);
        summary.RemainingOffers.Should().Be(2);
    }

    [TestMethod]
    public async Task GetAllAsync_ShouldReturnEmptyDealSummaries_WhenRestaurantHasNoRelevantDeals()
    {
        await SeedRestaurantWithDealsAsync(SummaryDeal(10, OfferType.DineIn, isActive: false));

        var result = (await _service.GetAllAsync()).Single();

        result.DealSummaries.Should().BeEmpty();
    }

    // ==========================================================
    // Area and Cuisine assignment (owner Create/Edit Restaurant)
    // ==========================================================

    private async Task<int> SeedCuisineAsync(string name)
    {
        var cuisine = new Cuisine { Name = name };

        _context.Cuisines.Add(cuisine);
        await _context.SaveChangesAsync();

        return cuisine.Id;
    }

    private async Task<int> SeedNamedAreaAsync(string name)
    {
        var area = new Area { Name = name };

        _context.Areas.Add(area);
        await _context.SaveChangesAsync();

        return area.Id;
    }

    // A restaurant with a stored logo, cover and menu, linked to the
    // given cuisines - like an existing restaurant in the database.
    private async Task<Restaurant> SeedRestaurantWithCuisinesAsync(int areaId, params int[] cuisineIds)
    {
        var restaurant = new Restaurant
        {
            Name = "Spice Kitchen",
            Description = "Nepali food",
            Address = "Thamel, Kathmandu",
            PhoneNumber = "0400000000",
            Email = "spice@test.com",
            OwnerId = 1,
            AreaId = areaId,
            CurrencyCode = "NPR",
            IsActive = true,
            LogoUrl = "/uploads/logos/spice.png",
            CoverImageUrl = "/uploads/covers/spice.jpg",
            MenuPdfUrl = "/uploads/menus/spice.pdf"
        };

        foreach (var cuisineId in cuisineIds)
            restaurant.RestaurantCuisines.Add(new RestaurantCuisine { CuisineId = cuisineId });

        _context.Restaurants.Add(restaurant);
        await _context.SaveChangesAsync();

        return restaurant;
    }

    private List<int> StoredCuisineIds(int restaurantId) =>
        _context.RestaurantCuisines
            .Where(rc => rc.RestaurantId == restaurantId)
            .Select(rc => rc.CuisineId)
            .OrderBy(id => id)
            .ToList();

    [TestMethod]
    public async Task CreateAsync_ShouldSaveSelectedCuisines_AndReturnTheirNamesAndIds()
    {
        var areaId = await SeedAreaAsync();
        var tibetanId = await SeedCuisineAsync("Tibetan");

        var dto = BuildCreateDto(1, areaId);
        dto.CuisineIds = new List<int> { _defaultCuisineId, tibetanId };

        var result = await _service.CreateAsync(dto);

        StoredCuisineIds(result.Id).Should().BeEquivalentTo(new[] { _defaultCuisineId, tibetanId });
        result.CuisineIds.Should().BeEquivalentTo(new[] { _defaultCuisineId, tibetanId });
        result.Cuisines.Should().BeEquivalentTo(new[] { "Nepali", "Tibetan" });
        result.AreaId.Should().Be(areaId);
        result.AreaName.Should().Be("Thamel");
    }

    [TestMethod]
    public async Task CreateAsync_ShouldThrowAndSaveNothing_WhenNoCuisineIsSelected()
    {
        var areaId = await SeedAreaAsync();

        var dto = BuildCreateDto(1, areaId);
        dto.CuisineIds = new List<int>();

        var act = () => _service.CreateAsync(dto);

        await act.Should().ThrowAsync<BusinessRuleException>()
            .WithMessage("Select at least one cuisine.");

        _context.Restaurants.Should().BeEmpty();
        _context.RestaurantOpeningHours.Should().BeEmpty();
    }

    [TestMethod]
    public async Task CreateAsync_ShouldThrowAndSaveNothing_WhenACuisineDoesNotExist()
    {
        var areaId = await SeedAreaAsync();

        var dto = BuildCreateDto(1, areaId);
        dto.CuisineIds = new List<int> { _defaultCuisineId, 999 };

        var act = () => _service.CreateAsync(dto);

        await act.Should().ThrowAsync<BusinessRuleException>()
            .WithMessage("*cuisine could not be found*999*");

        _context.Restaurants.Should().BeEmpty();
        _context.RestaurantCuisines.Should().BeEmpty();
    }

    [TestMethod]
    public async Task CreateAsync_ShouldThrow_WhenTheSameCuisineIsSelectedTwice()
    {
        var areaId = await SeedAreaAsync();

        var dto = BuildCreateDto(1, areaId);
        dto.CuisineIds = new List<int> { _defaultCuisineId, _defaultCuisineId };

        var act = () => _service.CreateAsync(dto);

        await act.Should().ThrowAsync<BusinessRuleException>()
            .WithMessage("Each cuisine can only be selected once.");

        _context.Restaurants.Should().BeEmpty();
    }

    [TestMethod]
    public async Task CreateAsync_ShouldThrowValidationError_NotDatabaseError_WhenAreaDoesNotExist()
    {
        var dto = BuildCreateDto(1, 999);

        var act = () => _service.CreateAsync(dto);

        await act.Should().ThrowAsync<BusinessRuleException>()
            .WithMessage("The selected area could not be found*");

        _context.Restaurants.Should().BeEmpty();
    }

    [TestMethod]
    public async Task UpdateAsync_ShouldReplaceCuisineSet_AddingNewAndRemovingDeselected()
    {
        var areaId = await SeedAreaAsync();
        var tibetanId = await SeedCuisineAsync("Tibetan");
        var cafeId = await SeedCuisineAsync("Cafe");

        var restaurant = await SeedRestaurantWithCuisinesAsync(areaId, _defaultCuisineId, tibetanId);

        var dto = BuildUpdateDto(restaurant, "NPR");
        dto.CuisineIds = new List<int> { tibetanId, cafeId };

        var result = await _service.UpdateAsync(restaurant.Id, dto);

        StoredCuisineIds(restaurant.Id).Should().BeEquivalentTo(new[] { tibetanId, cafeId });
        result!.CuisineIds.Should().BeEquivalentTo(new[] { tibetanId, cafeId });
        result.Cuisines.Should().BeEquivalentTo(new[] { "Tibetan", "Cafe" });
    }

    [TestMethod]
    public async Task UpdateAsync_ShouldKeepExistingCuisines_WhenCuisineIdsAreOmitted()
    {
        var areaId = await SeedAreaAsync();
        var tibetanId = await SeedCuisineAsync("Tibetan");

        var restaurant = await SeedRestaurantWithCuisinesAsync(areaId, _defaultCuisineId, tibetanId);

        var dto = BuildUpdateDto(restaurant, "NPR");
        dto.Name = "Spice Kitchen Thamel";
        dto.CuisineIds = null;

        var result = await _service.UpdateAsync(restaurant.Id, dto);

        result!.Name.Should().Be("Spice Kitchen Thamel");
        StoredCuisineIds(restaurant.Id).Should().BeEquivalentTo(new[] { _defaultCuisineId, tibetanId });
        result.CuisineIds.Should().BeEquivalentTo(new[] { _defaultCuisineId, tibetanId });
    }

    [TestMethod]
    public async Task UpdateAsync_ShouldRejectEmptyCuisineList_AndChangeNothing()
    {
        var areaId = await SeedAreaAsync();
        var restaurant = await SeedRestaurantWithCuisinesAsync(areaId, _defaultCuisineId);

        var dto = BuildUpdateDto(restaurant, "NPR");
        dto.Name = "Changed Name";
        dto.CuisineIds = new List<int>();

        var act = () => _service.UpdateAsync(restaurant.Id, dto);

        await act.Should().ThrowAsync<BusinessRuleException>()
            .WithMessage("Select at least one cuisine.");

        StoredCuisineIds(restaurant.Id).Should().BeEquivalentTo(new[] { _defaultCuisineId });
        _context.ChangeTracker.Clear();
        _context.Restaurants.Single().Name.Should().Be("Spice Kitchen");
    }

    [TestMethod]
    public async Task UpdateAsync_ShouldThrowAndKeepCuisines_WhenACuisineDoesNotExist()
    {
        var areaId = await SeedAreaAsync();
        var restaurant = await SeedRestaurantWithCuisinesAsync(areaId, _defaultCuisineId);

        var dto = BuildUpdateDto(restaurant, "NPR");
        dto.CuisineIds = new List<int> { 999 };

        var act = () => _service.UpdateAsync(restaurant.Id, dto);

        await act.Should().ThrowAsync<BusinessRuleException>()
            .WithMessage("*cuisine could not be found*999*");

        StoredCuisineIds(restaurant.Id).Should().BeEquivalentTo(new[] { _defaultCuisineId });
    }

    [TestMethod]
    public async Task UpdateAsync_ShouldChangeArea_AndReturnTheNewAreaName()
    {
        var thamelId = await SeedAreaAsync();
        var lalitpurId = await SeedNamedAreaAsync("Lalitpur");
        var restaurant = await SeedRestaurantWithCuisinesAsync(thamelId, _defaultCuisineId);

        var dto = BuildUpdateDto(restaurant, "NPR");
        dto.AreaId = lalitpurId;

        var result = await _service.UpdateAsync(restaurant.Id, dto);

        result!.AreaId.Should().Be(lalitpurId);
        result.AreaName.Should().Be("Lalitpur");
        _context.ChangeTracker.Clear();
        _context.Restaurants.Single().AreaId.Should().Be(lalitpurId);
    }

    [TestMethod]
    public async Task UpdateAsync_ShouldThrowAndKeepArea_WhenAreaDoesNotExist()
    {
        var areaId = await SeedAreaAsync();
        var restaurant = await SeedRestaurantWithCuisinesAsync(areaId, _defaultCuisineId);

        var dto = BuildUpdateDto(restaurant, "NPR");
        dto.AreaId = 999;

        var act = () => _service.UpdateAsync(restaurant.Id, dto);

        await act.Should().ThrowAsync<BusinessRuleException>()
            .WithMessage("The selected area could not be found*");

        _context.ChangeTracker.Clear();
        _context.Restaurants.Single().AreaId.Should().Be(areaId);
    }

    [TestMethod]
    public async Task UpdateAsync_ShouldNotEraseStoredLogo_WhenSavingUnrelatedDetails()
    {
        // -----------------------------
        // Regression: the owner edit form doesn't send logoUrl, so the
        // DTO's LogoUrl is blank. That used to overwrite the stored
        // logo on every profile save.
        // -----------------------------

        var areaId = await SeedAreaAsync();
        var restaurant = await SeedRestaurantWithCuisinesAsync(areaId, _defaultCuisineId);

        var dto = BuildUpdateDto(restaurant, "NPR");
        dto.LogoUrl = string.Empty;
        dto.Name = "Spice Kitchen Thamel";
        dto.PhoneNumber = "0411111111";

        var result = await _service.UpdateAsync(restaurant.Id, dto);

        result!.LogoUrl.Should().Be("/uploads/logos/spice.png");
        _context.ChangeTracker.Clear();
        var stored = _context.Restaurants.Single();
        stored.LogoUrl.Should().Be("/uploads/logos/spice.png");
        stored.CoverImageUrl.Should().Be("/uploads/covers/spice.jpg");
        stored.MenuPdfUrl.Should().Be("/uploads/menus/spice.pdf");
        stored.Name.Should().Be("Spice Kitchen Thamel");
        stored.PhoneNumber.Should().Be("0411111111");
    }

    [TestMethod]
    public async Task UpdateAsync_ShouldPreserveDealsOpeningHoursOwnerAndCurrency_WhenAreaAndCuisinesChange()
    {
        var thamelId = await SeedAreaAsync();
        var lalitpurId = await SeedNamedAreaAsync("Lalitpur");
        var tibetanId = await SeedCuisineAsync("Tibetan");
        var restaurant = await SeedRestaurantWithCuisinesAsync(thamelId, _defaultCuisineId);

        var deal = SummaryDeal(20, OfferType.DineIn);
        deal.RestaurantId = restaurant.Id;
        _context.Deals.Add(deal);
        _context.RestaurantOpeningHours.Add(new RestaurantOpeningHour
        {
            RestaurantId = restaurant.Id,
            DayOfWeek = DayOfWeek.Monday,
            OpenTime = new TimeOnly(10, 0),
            CloseTime = new TimeOnly(21, 0)
        });
        await _context.SaveChangesAsync();

        var dto = BuildUpdateDto(restaurant, "NPR");
        dto.AreaId = lalitpurId;
        dto.CuisineIds = new List<int> { tibetanId };

        await _service.UpdateAsync(restaurant.Id, dto);

        _context.ChangeTracker.Clear();
        var stored = _context.Restaurants.Single();
        stored.OwnerId.Should().Be(1);
        stored.CurrencyCode.Should().Be("NPR");
        stored.IsActive.Should().BeTrue();
        _context.Deals.Should().ContainSingle(d => d.RestaurantId == restaurant.Id && d.DiscountPercentage == 20);
        _context.RestaurantOpeningHours.Should().ContainSingle(h => h.RestaurantId == restaurant.Id);
    }

    [TestMethod]
    public async Task UpdateAsync_ShouldNotAffectOtherRestaurantsCuisines()
    {
        var areaId = await SeedAreaAsync();
        var tibetanId = await SeedCuisineAsync("Tibetan");

        var restaurant = await SeedRestaurantWithCuisinesAsync(areaId, _defaultCuisineId);
        var other = await SeedRestaurantWithCuisinesAsync(areaId, _defaultCuisineId, tibetanId);

        var dto = BuildUpdateDto(restaurant, "NPR");
        dto.CuisineIds = new List<int> { tibetanId };

        await _service.UpdateAsync(restaurant.Id, dto);

        StoredCuisineIds(restaurant.Id).Should().BeEquivalentTo(new[] { tibetanId });
        StoredCuisineIds(other.Id).Should().BeEquivalentTo(new[] { _defaultCuisineId, tibetanId });
    }

    [TestMethod]
    public async Task UpdateAsync_ShouldRejectNonOwner_AndKeepCuisines()
    {
        var areaId = await SeedAreaAsync();
        var tibetanId = await SeedCuisineAsync("Tibetan");
        var restaurant = await SeedRestaurantWithCuisinesAsync(areaId, _defaultCuisineId);

        _currentUser.Setup(x => x.IsAdmin).Returns(false);
        _currentUser.Setup(x => x.UserId).Returns(99);

        var dto = BuildUpdateDto(restaurant, "NPR");
        dto.CuisineIds = new List<int> { tibetanId };

        var act = () => _service.UpdateAsync(restaurant.Id, dto);

        await act.Should().ThrowAsync<BusinessRuleException>()
            .WithMessage("You are not authorized to update this restaurant.");

        StoredCuisineIds(restaurant.Id).Should().BeEquivalentTo(new[] { _defaultCuisineId });
    }

    [TestMethod]
    public async Task ReadMethods_ShouldIncludeCuisineIds_ForPrepopulatingEditForms()
    {
        var areaId = await SeedAreaAsync();
        var tibetanId = await SeedCuisineAsync("Tibetan");
        var restaurant = await SeedRestaurantWithCuisinesAsync(areaId, _defaultCuisineId, tibetanId);

        var expected = new[] { _defaultCuisineId, tibetanId };

        (await _service.GetByIdAsync(restaurant.Id))!.CuisineIds.Should().BeEquivalentTo(expected);
        (await _service.GetByOwnerIdAsync(1)).Single().CuisineIds.Should().BeEquivalentTo(expected);
        (await _service.GetAllAsync()).Single().CuisineIds.Should().BeEquivalentTo(expected);
    }

    [TestMethod]
    public async Task ReadMethods_ShouldExposeIsDemo_AndUpdateShouldNotChangeIt()
    {
        var areaId = await SeedAreaAsync();
        var demo = await SeedRestaurantWithCuisinesAsync(areaId, _defaultCuisineId);
        demo.IsDemo = true;
        var real = await SeedRestaurantWithCuisinesAsync(areaId, _defaultCuisineId);
        await _context.SaveChangesAsync();

        (await _service.GetByIdAsync(demo.Id))!.IsDemo.Should().BeTrue();
        (await _service.GetByIdAsync(real.Id))!.IsDemo.Should().BeFalse();
        (await _service.GetAllAsync()).Count(r => r.IsDemo).Should().Be(1);
        (await _service.GetByOwnerIdAsync(1)).Count(r => r.IsDemo).Should().Be(1);

        // Owner/admin edits can't switch the demo flag (not in the update request).
        var updated = await _service.UpdateAsync(demo.Id, BuildUpdateDto(demo, "NPR"));
        updated!.IsDemo.Should().BeTrue();
    }
}
