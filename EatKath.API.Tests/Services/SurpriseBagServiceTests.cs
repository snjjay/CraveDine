using EatKath.API.Data;
using EatKath.API.DTOs.SurpriseBag;
using EatKath.API.Entities;
using EatKath.API.Enums;
using EatKath.API.Interfaces;
using EatKath.API.Services;
using EatKath.API.Tests.Helpers;
using EatKath.API.Validators.SurpriseBag;
using FluentAssertions;
using FluentValidation;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace EatKath.API.Tests.Services;

// Surprise Bag listings. "Now" is Kathmandu business time: 12:00 on
// 10 Nov 2026 (06:15 UTC) unless a test sets another clock.
[TestClass]
public class SurpriseBagServiceTests
{
    private static readonly DateTime NoonInKathmanduUtc = new(2026, 11, 10, 6, 15, 0);
    private static readonly DateOnly Today = new(2026, 11, 10);

    // Smallest headers the image upload's content check recognises.
    private static readonly byte[] PngBytes =
        { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 0x00, 0x00, 0x00, 0x0D };

    private ApplicationDbContext _context = null!;
    private Mock<ICurrentUserService> _currentUser = null!;
    private SurpriseBagService _service = null!;

    // FileStorageService is a concrete class (no interface to mock), so the
    // image tests use it for real over a throwaway web root (as the
    // menu-item image tests do); Cleanup removes it.
    private string _testRootPath = null!;
    private FileStorageService _fileStorage = null!;

    [TestInitialize]
    public void Setup()
    {
        _context = TestDbContextFactory.Create();

        // Default caller: the owner (user 1) of the seeded restaurants.
        _currentUser = new Mock<ICurrentUserService>();
        _currentUser.Setup(x => x.UserId).Returns(1);
        _currentUser.Setup(x => x.IsAdmin).Returns(false);

        _testRootPath = Path.Combine(Path.GetTempPath(), "EatKathTests_SurpriseBag_" + Guid.NewGuid());

        var webHostEnvironment = new Mock<IWebHostEnvironment>();
        webHostEnvironment.Setup(e => e.WebRootPath).Returns(_testRootPath);

        _fileStorage = new FileStorageService(webHostEnvironment.Object);

        UseClock(NoonInKathmanduUtc);
    }

    [TestCleanup]
    public void Cleanup()
    {
        if (Directory.Exists(_testRootPath))
            Directory.Delete(_testRootPath, recursive: true);
    }

    private void UseClock(DateTime utc)
    {
        _service = new SurpriseBagService(
            _context,
            MapperFactory.Create(),
            _currentUser.Object,
            BusinessTimeProvider.CreateKathmandu(new FixedTimeProvider(utc)),
            new CreateSurpriseBagValidator(),
            new UpdateSurpriseBagValidator(),
            _fileStorage);
    }

    private async Task<Restaurant> SeedRestaurantAsync(int ownerId = 1, bool isActive = true, string currencyCode = "NPR")
    {
        var restaurant = new Restaurant
        {
            Name = "Himalayan Bakery",
            OwnerId = ownerId,
            IsActive = isActive,
            CurrencyCode = currencyCode
        };

        _context.Restaurants.Add(restaurant);
        await _context.SaveChangesAsync();

        return restaurant;
    }

    private async Task<SurpriseBag> SeedBagAsync(
        Restaurant restaurant,
        string title,
        DateOnly pickupDate,
        TimeOnly? start = null,
        TimeOnly? end = null,
        bool isActive = true,
        int quantityTotal = 5,
        int maxPerCustomer = 2)
    {
        var bag = new SurpriseBag
        {
            RestaurantId = restaurant.Id,
            Title = title,
            Category = SurpriseBagCategory.Bakery,
            Price = 250m,
            EstimatedValue = 750m,
            QuantityTotal = quantityTotal,
            MaxPerCustomer = maxPerCustomer,
            PickupDate = pickupDate,
            PickupStartTime = start ?? new TimeOnly(18, 0),
            PickupEndTime = end ?? new TimeOnly(19, 30),
            IsActive = isActive
        };

        _context.SurpriseBags.Add(bag);
        await _context.SaveChangesAsync();

        return bag;
    }

    private async Task<SurpriseBagReservation> SeedReservationAsync(
        SurpriseBag bag,
        int userId,
        int quantity,
        SurpriseBagReservationStatus status)
    {
        var reservation = new SurpriseBagReservation
        {
            SurpriseBagId = bag.Id,
            UserId = userId,
            Quantity = quantity,
            UnitPrice = bag.Price,
            ConfirmationCode = $"CODE{_context.SurpriseBagReservations.Count() + 1:D4}",
            Status = status
        };

        _context.SurpriseBagReservations.Add(reservation);
        await _context.SaveChangesAsync();

        return reservation;
    }

    private static CreateSurpriseBagDto ValidCreateDto(int restaurantId) => new()
    {
        RestaurantId = restaurantId,
        Title = "End-of-day bakery bag",
        Category = SurpriseBagCategory.Bakery,
        Description = "Breads and pastries",
        Conditions = "Contains gluten",
        Price = 250m,
        EstimatedValue = 750m,
        QuantityTotal = 5,
        MaxPerCustomer = 2,
        PickupDate = Today.AddDays(1),
        PickupStartTime = new TimeOnly(18, 0),
        PickupEndTime = new TimeOnly(19, 30)
    };

    private static UpdateSurpriseBagDto UpdateDtoFrom(SurpriseBag bag) => new()
    {
        Title = bag.Title,
        Category = bag.Category,
        Description = bag.Description,
        Conditions = bag.Conditions,
        Price = bag.Price,
        EstimatedValue = bag.EstimatedValue,
        QuantityTotal = bag.QuantityTotal,
        MaxPerCustomer = bag.MaxPerCustomer,
        PickupDate = bag.PickupDate,
        PickupStartTime = bag.PickupStartTime,
        PickupEndTime = bag.PickupEndTime,
        IsActive = bag.IsActive
    };

    // =========================================================
    // Public listings
    // =========================================================

    [TestMethod]
    public async Task GetPublicByRestaurantAsync_ShouldShowOnlyTodaysOpenAndUpcomingActiveBags()
    {
        var restaurant = await SeedRestaurantAsync();
        var other = await SeedRestaurantAsync(ownerId: 2);

        await SeedBagAsync(restaurant, "Yesterday", Today.AddDays(-1));
        await SeedBagAsync(restaurant, "Ended this morning", Today, new TimeOnly(9, 0), new TimeOnly(11, 30));
        await SeedBagAsync(restaurant, "Ends at noon", Today, new TimeOnly(11, 0), new TimeOnly(12, 0));
        await SeedBagAsync(restaurant, "Tomorrow", Today.AddDays(1), new TimeOnly(8, 0), new TimeOnly(9, 0));
        await SeedBagAsync(restaurant, "Open today", Today, new TimeOnly(11, 30), new TimeOnly(12, 30));
        await SeedBagAsync(restaurant, "Inactive", Today.AddDays(1), isActive: false);
        await SeedBagAsync(other, "Other restaurant", Today.AddDays(1));

        var result = await _service.GetPublicByRestaurantAsync(restaurant.Id);

        result.Select(b => b.Title).Should().Equal("Open today", "Tomorrow");
    }

    [TestMethod]
    public async Task GetPublicByRestaurantAsync_ShouldReturnNothing_WhenRestaurantIsInactive()
    {
        var restaurant = await SeedRestaurantAsync(isActive: false);
        await SeedBagAsync(restaurant, "Tomorrow", Today.AddDays(1));

        var result = await _service.GetPublicByRestaurantAsync(restaurant.Id);

        result.Should().BeEmpty();
    }

    // 18:30 UTC on 10 Nov is 00:15 on 11 Nov in Kathmandu: a bag for
    // 10 Nov 18:00-19:30 has ended there, although that window is "now" in UTC.
    [TestMethod]
    public async Task GetPublicByRestaurantAsync_ShouldUseKathmanduDate_AfterUtcRollover()
    {
        UseClock(new DateTime(2026, 11, 10, 18, 30, 0));

        var restaurant = await SeedRestaurantAsync();
        await SeedBagAsync(restaurant, "10 Nov evening", new DateOnly(2026, 11, 10));
        await SeedBagAsync(restaurant, "11 Nov morning", new DateOnly(2026, 11, 11), new TimeOnly(8, 0), new TimeOnly(9, 0));

        var result = await _service.GetPublicByRestaurantAsync(restaurant.Id);

        result.Select(b => b.Title).Should().Equal("11 Nov morning");
    }

    [TestMethod]
    public async Task RemainingQuantity_ShouldCountReservedAndCollected_Only()
    {
        var restaurant = await SeedRestaurantAsync();
        var bag = await SeedBagAsync(restaurant, "Tomorrow", Today.AddDays(1), quantityTotal: 10, maxPerCustomer: 5);

        await SeedReservationAsync(bag, 50, 2, SurpriseBagReservationStatus.Reserved);
        await SeedReservationAsync(bag, 51, 3, SurpriseBagReservationStatus.Collected);
        await SeedReservationAsync(bag, 52, 4, SurpriseBagReservationStatus.Cancelled);
        await SeedReservationAsync(bag, 53, 1, SurpriseBagReservationStatus.NoShow);

        var result = (await _service.GetPublicByRestaurantAsync(restaurant.Id)).Single();

        result.RemainingQuantity.Should().Be(5);
        result.IsSoldOut.Should().BeFalse();
    }

    [TestMethod]
    public async Task SoldOutBag_ShouldStayVisible_WithIsSoldOut()
    {
        var restaurant = await SeedRestaurantAsync();
        var bag = await SeedBagAsync(restaurant, "Tomorrow", Today.AddDays(1), quantityTotal: 3, maxPerCustomer: 3);

        await SeedReservationAsync(bag, 50, 2, SurpriseBagReservationStatus.Reserved);
        await SeedReservationAsync(bag, 51, 1, SurpriseBagReservationStatus.Collected);

        var listed = (await _service.GetPublicByRestaurantAsync(restaurant.Id)).Single();
        var single = await _service.GetPublicByIdAsync(bag.Id);

        listed.RemainingQuantity.Should().Be(0);
        listed.IsSoldOut.Should().BeTrue();
        single!.IsSoldOut.Should().BeTrue();
    }

    [TestMethod]
    public async Task GetPublicByIdAsync_ShouldReturnVisibleBagWithRestaurantDetails_OrNull()
    {
        var restaurant = await SeedRestaurantAsync(currencyCode: "AUD");
        var visible = await SeedBagAsync(restaurant, "Tomorrow", Today.AddDays(1));
        var inactive = await SeedBagAsync(restaurant, "Inactive", Today.AddDays(1), isActive: false);
        var ended = await SeedBagAsync(restaurant, "Ended", Today, new TimeOnly(9, 0), new TimeOnly(11, 0));

        var result = await _service.GetPublicByIdAsync(visible.Id);

        result!.RestaurantId.Should().Be(restaurant.Id);
        result.RestaurantName.Should().Be("Himalayan Bakery");
        result.CurrencyCode.Should().Be("AUD");
        result.Price.Should().Be(250m);
        result.EstimatedValue.Should().Be(750m);
        result.RemainingQuantity.Should().Be(5);

        (await _service.GetPublicByIdAsync(inactive.Id)).Should().BeNull();
        (await _service.GetPublicByIdAsync(ended.Id)).Should().BeNull();
        (await _service.GetPublicByIdAsync(9999)).Should().BeNull();
    }

    [TestMethod]
    public async Task GetPublicAsync_ShouldListVisibleBagsAcrossActiveRestaurants_SoonestPickupFirst()
    {
        var restaurant = await SeedRestaurantAsync();
        var other = await SeedRestaurantAsync(ownerId: 2);
        var inactiveRestaurant = await SeedRestaurantAsync(ownerId: 3, isActive: false);

        await SeedBagAsync(restaurant, "Yesterday", Today.AddDays(-1));
        await SeedBagAsync(restaurant, "Ended this morning", Today, new TimeOnly(9, 0), new TimeOnly(11, 30));
        await SeedBagAsync(other, "Ends at noon", Today, new TimeOnly(11, 0), new TimeOnly(12, 0));
        await SeedBagAsync(other, "Tomorrow morning", Today.AddDays(1), new TimeOnly(8, 0), new TimeOnly(9, 0));
        await SeedBagAsync(restaurant, "Tonight", Today);
        await SeedBagAsync(other, "Open today", Today, new TimeOnly(11, 30), new TimeOnly(12, 30));
        await SeedBagAsync(restaurant, "Inactive", Today.AddDays(1), isActive: false);
        await SeedBagAsync(inactiveRestaurant, "Inactive restaurant", Today.AddDays(1));

        var result = (await _service.GetPublicAsync(50)).ToList();

        result.Select(b => b.Title).Should().Equal("Open today", "Tonight", "Tomorrow morning");
        result[0].RestaurantId.Should().Be(other.Id);
        result[0].RestaurantName.Should().Be("Himalayan Bakery");
        result[0].RemainingQuantity.Should().Be(5);
    }

    [TestMethod]
    public async Task GetPublicAsync_ShouldListSoldOutBagsAfterReservableOnes()
    {
        var restaurant = await SeedRestaurantAsync();
        var soldOut = await SeedBagAsync(restaurant, "Sold out tonight", Today, quantityTotal: 2, maxPerCustomer: 2);
        await SeedBagAsync(restaurant, "Tomorrow", Today.AddDays(1));

        await SeedReservationAsync(soldOut, 50, 1, SurpriseBagReservationStatus.Reserved);
        await SeedReservationAsync(soldOut, 51, 1, SurpriseBagReservationStatus.Collected);

        var result = (await _service.GetPublicAsync(50)).ToList();

        result.Select(b => b.Title).Should().Equal("Tomorrow", "Sold out tonight");
        result[1].IsSoldOut.Should().BeTrue();
        result[1].RemainingQuantity.Should().Be(0);

        // The sold-out bag's pickup is sooner, but a single featured bag
        // must be one that can still be reserved.
        var first = (await _service.GetPublicAsync(1)).Single();

        first.Title.Should().Be("Tomorrow");
        first.IsSoldOut.Should().BeFalse();
    }

    // The anonymous listing returns the public DTO only: serialized as the
    // API does (Web defaults; Program.cs adds no JSON options) and by runtime
    // type, so an owner DTO subclass would show its extra fields here.
    [TestMethod]
    public async Task GetPublicAsync_ShouldExposeOnlyPublicSurpriseBagDtoFields()
    {
        var restaurant = await SeedRestaurantAsync();
        var bag = await SeedBagAsync(restaurant, "Tomorrow", Today.AddDays(1));
        await SeedReservationAsync(bag, 50, 1, SurpriseBagReservationStatus.Reserved);

        var result = (await _service.GetPublicAsync(50)).ToList();

        result.Should().ContainSingle();
        result.Should().AllBeOfType<SurpriseBagDto>();
        result[0].Should().NotBeOfType<OwnerSurpriseBagDto>();

        var options = new System.Text.Json.JsonSerializerOptions(System.Text.Json.JsonSerializerDefaults.Web);
        using var json = System.Text.Json.JsonDocument.Parse(
            System.Text.Json.JsonSerializer.Serialize<object>(result[0], options));
        var fields = json.RootElement.EnumerateObject().Select(p => p.Name).ToList();

        fields.Should().BeEquivalentTo(
            "id", "restaurantId", "restaurantName", "currencyCode", "title", "category",
            "description", "conditions", "price", "estimatedValue", "maxPerCustomer",
            "pickupDate", "pickupStartTime", "pickupEndTime", "imageUrl", "remainingQuantity", "isSoldOut");

        // Owner-only (OwnerSurpriseBagDto) and customer/reservation fields.
        fields.Should().NotContain(new[]
        {
            "isActive", "quantityTotal", "reservedQuantity", "collectedQuantity",
            "isPickupLocked", "createdAt", "updatedAt",
            "userId", "customerName", "customerEmail", "customerPhone", "confirmationCode"
        });
    }

    [TestMethod]
    public async Task GetPublicAsync_ShouldClampTake()
    {
        var restaurant = await SeedRestaurantAsync();

        for (var i = 0; i < SurpriseBagService.MaxPublicTake + 1; i++)
            await SeedBagAsync(restaurant, $"Bag {i}", Today.AddDays(1));

        (await _service.GetPublicAsync(2)).Should().HaveCount(2);
        (await _service.GetPublicAsync(0)).Should().HaveCount(1);
        (await _service.GetPublicAsync(-5)).Should().HaveCount(1);
        (await _service.GetPublicAsync(int.MaxValue)).Should().HaveCount(SurpriseBagService.MaxPublicTake);
    }

    [TestMethod]
    public async Task GetPublicAsync_ShouldReturnEmpty_WhenNoBagsAreVisible()
    {
        var restaurant = await SeedRestaurantAsync();
        await SeedBagAsync(restaurant, "Yesterday", Today.AddDays(-1));

        (await _service.GetPublicAsync(50)).Should().BeEmpty();
    }

    // =========================================================
    // Owner listings
    // =========================================================

    [TestMethod]
    public async Task GetByOwnerAsync_ShouldIncludeInactiveAndPastBags_WithCounts_ForOwnBagsOnly()
    {
        var mine = await SeedRestaurantAsync(ownerId: 1);
        var theirs = await SeedRestaurantAsync(ownerId: 2);

        var past = await SeedBagAsync(mine, "Past and inactive", Today.AddDays(-3), isActive: false);
        var upcoming = await SeedBagAsync(mine, "Upcoming", Today.AddDays(2), quantityTotal: 6);
        await SeedBagAsync(theirs, "Someone else's", Today.AddDays(1));

        await SeedReservationAsync(upcoming, 50, 2, SurpriseBagReservationStatus.Reserved);
        await SeedReservationAsync(upcoming, 51, 1, SurpriseBagReservationStatus.Collected);
        await SeedReservationAsync(upcoming, 52, 1, SurpriseBagReservationStatus.Cancelled);

        var result = (await _service.GetByOwnerAsync(1)).ToList();

        result.Select(b => b.Title).Should().Equal("Upcoming", "Past and inactive");

        var upcomingDto = result[0];
        upcomingDto.QuantityTotal.Should().Be(6);
        upcomingDto.ReservedQuantity.Should().Be(2);
        upcomingDto.CollectedQuantity.Should().Be(1);
        upcomingDto.RemainingQuantity.Should().Be(3);
        upcomingDto.IsPickupLocked.Should().BeTrue();
        upcomingDto.IsActive.Should().BeTrue();

        var pastDto = result[1];
        pastDto.Id.Should().Be(past.Id);
        pastDto.IsActive.Should().BeFalse();
        pastDto.IsPickupLocked.Should().BeFalse();
    }

    [TestMethod]
    public async Task GetByOwnerAsync_ShouldFilterByRestaurant()
    {
        var first = await SeedRestaurantAsync(ownerId: 1);
        var second = await SeedRestaurantAsync(ownerId: 1);

        await SeedBagAsync(first, "First's bag", Today.AddDays(1));
        await SeedBagAsync(second, "Second's bag", Today.AddDays(1));

        var result = await _service.GetByOwnerAsync(1, second.Id);

        result.Select(b => b.Title).Should().Equal("Second's bag");
    }

    [TestMethod]
    public async Task GetManagedByIdAsync_ShouldAllowOwnerAndAdmin_RejectOthers_AndReturnNullWhenMissing()
    {
        var restaurant = await SeedRestaurantAsync(ownerId: 1);
        var bag = await SeedBagAsync(restaurant, "Inactive", Today.AddDays(-1), isActive: false);

        (await _service.GetManagedByIdAsync(bag.Id))!.Title.Should().Be("Inactive");
        (await _service.GetManagedByIdAsync(9999)).Should().BeNull();

        _currentUser.Setup(x => x.UserId).Returns(2);

        var ex = await Assert.ThrowsExceptionAsync<BusinessRuleException>(
            () => _service.GetManagedByIdAsync(bag.Id));
        ex.Message.Should().Be("You are not authorized to view this Surprise Bag.");

        _currentUser.Setup(x => x.IsAdmin).Returns(true);

        (await _service.GetManagedByIdAsync(bag.Id)).Should().NotBeNull();
    }

    // =========================================================
    // CreateAsync
    // =========================================================

    [TestMethod]
    public async Task CreateAsync_ShouldSaveBag_AndReturnOwnerDto()
    {
        var restaurant = await SeedRestaurantAsync(currencyCode: "NPR");

        var result = await _service.CreateAsync(ValidCreateDto(restaurant.Id));

        result.Id.Should().BeGreaterThan(0);
        result.RestaurantName.Should().Be("Himalayan Bakery");
        result.CurrencyCode.Should().Be("NPR");
        result.IsActive.Should().BeTrue();
        result.QuantityTotal.Should().Be(5);
        result.RemainingQuantity.Should().Be(5);
        result.IsSoldOut.Should().BeFalse();
        result.IsPickupLocked.Should().BeFalse();

        var saved = _context.SurpriseBags.Single();
        saved.RestaurantId.Should().Be(restaurant.Id);
        saved.Title.Should().Be("End-of-day bakery bag");
        saved.PickupDate.Should().Be(Today.AddDays(1));
    }

    // The quantity sent is the quantity saved (no default is applied).
    [TestMethod]
    public async Task CreateAsync_ShouldSaveTheGivenQuantityTotal()
    {
        var restaurant = await SeedRestaurantAsync();
        var dto = ValidCreateDto(restaurant.Id);
        dto.QuantityTotal = 1;
        dto.MaxPerCustomer = 1;

        var result = await _service.CreateAsync(dto);

        result.QuantityTotal.Should().Be(1);
        result.ReservedQuantity.Should().Be(0);
        result.RemainingQuantity.Should().Be(1);
        result.IsSoldOut.Should().BeFalse();
        _context.SurpriseBags.Single().QuantityTotal.Should().Be(1);

        // One reservation takes the only bag.
        await SeedReservationAsync(_context.SurpriseBags.Single(), 50, 1, SurpriseBagReservationStatus.Reserved);

        var managed = await _service.GetManagedByIdAsync(result.Id);

        managed!.QuantityTotal.Should().Be(1);
        managed.ReservedQuantity.Should().Be(1);
        managed.RemainingQuantity.Should().Be(0);
        managed.IsSoldOut.Should().BeTrue();
    }

    [TestMethod]
    public async Task CreateAsync_ShouldThrowValidationException_WhenInvalid()
    {
        var restaurant = await SeedRestaurantAsync();
        var dto = ValidCreateDto(restaurant.Id);
        dto.EstimatedValue = dto.Price;

        await Assert.ThrowsExceptionAsync<ValidationException>(() => _service.CreateAsync(dto));

        _context.SurpriseBags.Should().BeEmpty();
    }

    [TestMethod]
    public async Task CreateAsync_ShouldRejectNonOwner_ButAllowAdmin()
    {
        var restaurant = await SeedRestaurantAsync(ownerId: 2);

        var ex = await Assert.ThrowsExceptionAsync<BusinessRuleException>(
            () => _service.CreateAsync(ValidCreateDto(restaurant.Id)));
        ex.Message.Should().Be("You are not authorized to add Surprise Bags to this restaurant.");

        _currentUser.Setup(x => x.IsAdmin).Returns(true);

        await _service.CreateAsync(ValidCreateDto(restaurant.Id));

        _context.SurpriseBags.Should().ContainSingle();
    }

    [TestMethod]
    public async Task CreateAsync_ShouldThrowNotFound_WhenRestaurantIsMissing()
    {
        await Assert.ThrowsExceptionAsync<NotFoundException>(
            () => _service.CreateAsync(ValidCreateDto(9999)));
    }

    [TestMethod]
    public async Task CreateAsync_ShouldReject_WhenPickupWindowHasEnded_ButAllowAnOpenWindowToday()
    {
        var restaurant = await SeedRestaurantAsync();

        var ended = ValidCreateDto(restaurant.Id);
        ended.PickupDate = Today;
        ended.PickupStartTime = new TimeOnly(10, 0);
        ended.PickupEndTime = new TimeOnly(12, 0); // ends exactly now

        var ex = await Assert.ThrowsExceptionAsync<BusinessRuleException>(() => _service.CreateAsync(ended));
        ex.Message.Should().Be("The pickup window has already ended.");

        var open = ValidCreateDto(restaurant.Id);
        open.PickupDate = Today;
        open.PickupStartTime = new TimeOnly(11, 0);
        open.PickupEndTime = new TimeOnly(12, 30);

        (await _service.CreateAsync(open)).PickupDate.Should().Be(Today);
    }

    // =========================================================
    // UpdateAsync
    // =========================================================

    [TestMethod]
    public async Task UpdateAsync_ShouldAllowPriceAndTextChanges_WhileReserved()
    {
        var restaurant = await SeedRestaurantAsync();
        var bag = await SeedBagAsync(restaurant, "Tomorrow", Today.AddDays(1));
        var reservation = await SeedReservationAsync(bag, 50, 1, SurpriseBagReservationStatus.Reserved);

        var dto = UpdateDtoFrom(bag);
        dto.Title = "Bigger bakery bag";
        dto.Price = 300m;
        dto.EstimatedValue = 900m;

        var result = await _service.UpdateAsync(bag.Id, dto);

        result.Title.Should().Be("Bigger bakery bag");
        result.Price.Should().Be(300m);
        result.IsPickupLocked.Should().BeTrue();
        bag.UpdatedAt.Should().NotBeNull();

        // The reservation keeps the price it was made at.
        reservation.UnitPrice.Should().Be(250m);
    }

    [TestMethod]
    [DataRow(SurpriseBagReservationStatus.Reserved)]
    [DataRow(SurpriseBagReservationStatus.Collected)]
    public async Task UpdateAsync_ShouldLockPickupWindow_WhileReservedOrCollected(SurpriseBagReservationStatus status)
    {
        var restaurant = await SeedRestaurantAsync();
        var bag = await SeedBagAsync(restaurant, "Tomorrow", Today.AddDays(1));
        await SeedReservationAsync(bag, 50, 1, status);

        var laterEnd = UpdateDtoFrom(bag);
        laterEnd.PickupEndTime = new TimeOnly(20, 0);

        var otherDay = UpdateDtoFrom(bag);
        otherDay.PickupDate = Today.AddDays(2);

        foreach (var dto in new[] { laterEnd, otherDay })
        {
            var ex = await Assert.ThrowsExceptionAsync<BusinessRuleException>(() => _service.UpdateAsync(bag.Id, dto));
            ex.Message.Should().Be("Pickup date and times cannot be changed once bags have been reserved or collected.");
        }

        bag.PickupDate.Should().Be(Today.AddDays(1));
        bag.PickupEndTime.Should().Be(new TimeOnly(19, 30));
    }

    [TestMethod]
    public async Task UpdateAsync_ShouldAllowPickupChange_WhenOnlyCancelledOrNoShowReservationsExist()
    {
        var restaurant = await SeedRestaurantAsync();
        var bag = await SeedBagAsync(restaurant, "Tomorrow", Today.AddDays(1));
        await SeedReservationAsync(bag, 50, 1, SurpriseBagReservationStatus.Cancelled);
        await SeedReservationAsync(bag, 51, 1, SurpriseBagReservationStatus.NoShow);

        var dto = UpdateDtoFrom(bag);
        dto.PickupStartTime = new TimeOnly(19, 0);
        dto.PickupEndTime = new TimeOnly(20, 30);

        var result = await _service.UpdateAsync(bag.Id, dto);

        result.PickupStartTime.Should().Be(new TimeOnly(19, 0));
        result.PickupEndTime.Should().Be(new TimeOnly(20, 30));
        result.IsPickupLocked.Should().BeFalse();
    }

    [TestMethod]
    public async Task UpdateAsync_ShouldRejectQuantityTotalBelowReservedPlusCollected_ButAllowEqual()
    {
        var restaurant = await SeedRestaurantAsync();
        var bag = await SeedBagAsync(restaurant, "Tomorrow", Today.AddDays(1), quantityTotal: 10, maxPerCustomer: 2);
        await SeedReservationAsync(bag, 50, 2, SurpriseBagReservationStatus.Reserved);
        await SeedReservationAsync(bag, 51, 1, SurpriseBagReservationStatus.Collected);
        await SeedReservationAsync(bag, 52, 2, SurpriseBagReservationStatus.Cancelled);

        var tooFew = UpdateDtoFrom(bag);
        tooFew.QuantityTotal = 2;

        var ex = await Assert.ThrowsExceptionAsync<BusinessRuleException>(() => _service.UpdateAsync(bag.Id, tooFew));
        ex.Message.Should().Be("Quantity cannot be less than the 3 bags already reserved or collected.");

        var exact = UpdateDtoFrom(bag);
        exact.QuantityTotal = 3;

        var result = await _service.UpdateAsync(bag.Id, exact);

        result.QuantityTotal.Should().Be(3);
        result.RemainingQuantity.Should().Be(0);
        result.IsSoldOut.Should().BeTrue();
    }

    [TestMethod]
    public async Task UpdateAsync_ShouldRejectMaxPerCustomerBelowLargestCustomerHolding_ButAllowEqual()
    {
        var restaurant = await SeedRestaurantAsync();
        var bag = await SeedBagAsync(restaurant, "Tomorrow", Today.AddDays(1), quantityTotal: 10, maxPerCustomer: 5);

        // Customer 50 holds 3 (1 reserved + 2 collected); customer 51 holds 1;
        // customer 52's cancelled 4 don't count.
        await SeedReservationAsync(bag, 50, 1, SurpriseBagReservationStatus.Reserved);
        await SeedReservationAsync(bag, 50, 2, SurpriseBagReservationStatus.Collected);
        await SeedReservationAsync(bag, 51, 1, SurpriseBagReservationStatus.Reserved);
        await SeedReservationAsync(bag, 52, 4, SurpriseBagReservationStatus.Cancelled);

        var tooLow = UpdateDtoFrom(bag);
        tooLow.MaxPerCustomer = 2;

        var ex = await Assert.ThrowsExceptionAsync<BusinessRuleException>(() => _service.UpdateAsync(bag.Id, tooLow));
        ex.Message.Should().Be("Max per customer cannot be less than the 3 bags one customer already holds.");

        var equal = UpdateDtoFrom(bag);
        equal.MaxPerCustomer = 3;

        (await _service.UpdateAsync(bag.Id, equal)).MaxPerCustomer.Should().Be(3);
    }

    [TestMethod]
    public async Task UpdateAsync_ShouldRejectNonOwner_AndThrowNotFound_WhenMissing()
    {
        var restaurant = await SeedRestaurantAsync(ownerId: 2);
        var bag = await SeedBagAsync(restaurant, "Tomorrow", Today.AddDays(1));

        var ex = await Assert.ThrowsExceptionAsync<BusinessRuleException>(
            () => _service.UpdateAsync(bag.Id, UpdateDtoFrom(bag)));
        ex.Message.Should().Be("You are not authorized to modify this Surprise Bag.");

        await Assert.ThrowsExceptionAsync<NotFoundException>(
            () => _service.UpdateAsync(9999, UpdateDtoFrom(bag)));
    }

    [TestMethod]
    public async Task UpdateAsync_ShouldThrowValidationException_WhenInvalid()
    {
        var restaurant = await SeedRestaurantAsync();
        var bag = await SeedBagAsync(restaurant, "Tomorrow", Today.AddDays(1));

        var dto = UpdateDtoFrom(bag);
        dto.MaxPerCustomer = dto.QuantityTotal + 1;

        await Assert.ThrowsExceptionAsync<ValidationException>(() => _service.UpdateAsync(bag.Id, dto));
    }

    [TestMethod]
    public async Task UpdateAsync_ShouldReactivateBag_ButRejectMovingTheWindowIntoThePast()
    {
        var restaurant = await SeedRestaurantAsync();
        var bag = await SeedBagAsync(restaurant, "Tomorrow", Today.AddDays(1), isActive: false);

        var reactivate = UpdateDtoFrom(bag);
        reactivate.IsActive = true;

        (await _service.UpdateAsync(bag.Id, reactivate)).IsActive.Should().BeTrue();
        (await _service.GetPublicByIdAsync(bag.Id)).Should().NotBeNull();

        var intoThePast = UpdateDtoFrom(bag);
        intoThePast.PickupDate = Today.AddDays(-1);

        var ex = await Assert.ThrowsExceptionAsync<BusinessRuleException>(() => _service.UpdateAsync(bag.Id, intoThePast));
        ex.Message.Should().Be("The pickup window has already ended.");
    }

    // A PUT that leaves IsActive out must not reactivate (or deactivate) a bag.
    [TestMethod]
    [DataRow(false)]
    [DataRow(true)]
    public async Task UpdateAsync_ShouldKeepTheCurrentState_WhenIsActiveIsOmitted(bool currentlyActive)
    {
        var restaurant = await SeedRestaurantAsync();
        var bag = await SeedBagAsync(restaurant, "Tomorrow", Today.AddDays(1), isActive: currentlyActive);

        var dto = UpdateDtoFrom(bag);
        dto.IsActive = null;
        dto.Title = "Renamed";

        var result = await _service.UpdateAsync(bag.Id, dto);

        result.Title.Should().Be("Renamed");
        result.IsActive.Should().Be(currentlyActive);
        bag.IsActive.Should().Be(currentlyActive);
    }

    [TestMethod]
    public async Task UpdateAsync_ShouldDeactivate_WhenIsActiveIsFalse()
    {
        var restaurant = await SeedRestaurantAsync();
        var bag = await SeedBagAsync(restaurant, "Tomorrow", Today.AddDays(1));

        var dto = UpdateDtoFrom(bag);
        dto.IsActive = false;

        (await _service.UpdateAsync(bag.Id, dto)).IsActive.Should().BeFalse();
        (await _service.GetPublicByIdAsync(bag.Id)).Should().BeNull();
    }

    // How ASP.NET Core binds the PUT body ([FromBody] uses the Web defaults;
    // Program.cs adds no JSON options): a missing isActive is null, not true.
    [TestMethod]
    public void UpdateSurpriseBagDto_ShouldBindAMissingIsActiveAsNull()
    {
        var options = new System.Text.Json.JsonSerializerOptions(System.Text.Json.JsonSerializerDefaults.Web);

        var withoutFlag = System.Text.Json.JsonSerializer.Deserialize<UpdateSurpriseBagDto>(
            """{"title":"Bag","price":250,"quantityTotal":5}""", options)!;
        var withFlag = System.Text.Json.JsonSerializer.Deserialize<UpdateSurpriseBagDto>(
            """{"title":"Bag","isActive":false}""", options)!;

        withoutFlag.IsActive.Should().BeNull();
        withFlag.IsActive.Should().BeFalse();
    }

    // =========================================================
    // Image (real file I/O under the throwaway web root)
    // =========================================================

    // A real FormFile over in-memory bytes (PNG by default), so the
    // content check and the file copy both run for real.
    private static IFormFile BuildFormFile(string fileName = "photo.png", byte[]? content = null)
    {
        var bytes = content ?? PngBytes;

        return new FormFile(new MemoryStream(bytes), 0, bytes.Length, "File", fileName);
    }

    private string ToAbsolutePath(string relativePath) =>
        Path.Combine(_testRootPath, relativePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));

    private string BagImageFolder(int bagId) =>
        Path.Combine(_testRootPath, "uploads", "surprisebags", bagId.ToString());

    // Creates a real file for a stored image path, as an earlier upload would have.
    private string CreateFileFor(string relativePath)
    {
        var absolutePath = ToAbsolutePath(relativePath);

        Directory.CreateDirectory(Path.GetDirectoryName(absolutePath)!);
        File.WriteAllBytes(absolutePath, PngBytes);

        return absolutePath;
    }

    [TestMethod]
    public async Task UploadImageAsync_ShouldSaveImageInTheBagFolder_AndExposeItPublicly()
    {
        var restaurant = await SeedRestaurantAsync();
        var bag = await SeedBagAsync(restaurant, "Tomorrow", Today.AddDays(1));

        var result = await _service.UploadImageAsync(bag.Id, BuildFormFile());

        result.Should().StartWith($"/uploads/surprisebags/{bag.Id}/image-").And.EndWith(".png");
        File.Exists(ToAbsolutePath(result)).Should().BeTrue();

        var saved = _context.SurpriseBags.Single();
        saved.ImageUrl.Should().Be(result);
        saved.UpdatedAt.Should().NotBeNull();

        (await _service.GetPublicByIdAsync(bag.Id))!.ImageUrl.Should().Be(result);
        (await _service.GetManagedByIdAsync(bag.Id))!.ImageUrl.Should().Be(result);
    }

    [TestMethod]
    public async Task UploadImageAsync_ShouldReplaceImage_AndDeleteThePreviousFile()
    {
        var restaurant = await SeedRestaurantAsync();
        var bag = await SeedBagAsync(restaurant, "Tomorrow", Today.AddDays(1));

        var first = await _service.UploadImageAsync(bag.Id, BuildFormFile("first.png"));
        var second = await _service.UploadImageAsync(bag.Id, BuildFormFile("second.png"));

        second.Should().NotBe(first);
        File.Exists(ToAbsolutePath(first)).Should().BeFalse();
        File.Exists(ToAbsolutePath(second)).Should().BeTrue();
        Directory.GetFiles(BagImageFolder(bag.Id)).Should().ContainSingle();
        _context.SurpriseBags.Single().ImageUrl.Should().Be(second);
    }

    // A rejected replacement must leave the current image and its file alone.
    [TestMethod]
    [DataRow("photo.gif", null, "Only JPG, PNG and WEBP images are allowed.")]
    [DataRow("photo.png", "text", "The selected file is not a valid JPG, PNG or WEBP image.")]
    [DataRow("photo.png", "empty", "The selected image is empty.")]
    [DataRow("photo.png", "oversized", "Images must be 5 MB or smaller.")]
    public async Task UploadImageAsync_ShouldRejectInvalidImages_AndKeepTheCurrentImage(
        string fileName, string? content, string message)
    {
        var restaurant = await SeedRestaurantAsync();
        var bag = await SeedBagAsync(restaurant, "Tomorrow", Today.AddDays(1));
        var current = await _service.UploadImageAsync(bag.Id, BuildFormFile());

        var bytes = content switch
        {
            "text" => "not an image at all"u8.ToArray(),
            "empty" => Array.Empty<byte>(),
            "oversized" => PngBytes.Concat(new byte[SurpriseBagService.MaxImageBytes]).ToArray(),
            _ => null
        };

        var ex = await Assert.ThrowsExceptionAsync<BusinessRuleException>(
            () => _service.UploadImageAsync(bag.Id, BuildFormFile(fileName, bytes)));

        ex.Message.Should().Be(message);
        _context.SurpriseBags.Single().ImageUrl.Should().Be(current);
        File.Exists(ToAbsolutePath(current)).Should().BeTrue();
        Directory.GetFiles(BagImageFolder(bag.Id)).Should().ContainSingle();
    }

    [TestMethod]
    public async Task UploadImageAsync_ShouldAcceptAFileExactlyAtTheSizeLimit()
    {
        var restaurant = await SeedRestaurantAsync();
        var bag = await SeedBagAsync(restaurant, "Tomorrow", Today.AddDays(1));
        var atLimit = PngBytes.Concat(new byte[SurpriseBagService.MaxImageBytes - PngBytes.Length]).ToArray();

        var result = await _service.UploadImageAsync(bag.Id, BuildFormFile("big.png", atLimit));

        File.Exists(ToAbsolutePath(result)).Should().BeTrue();
    }

    // Images are cosmetic: allowed for an inactive bag whose pickup details
    // are locked by a reservation.
    [TestMethod]
    public async Task UploadImageAsync_ShouldAllowInactiveAndPickupLockedBags()
    {
        var restaurant = await SeedRestaurantAsync();
        var bag = await SeedBagAsync(restaurant, "Tomorrow", Today.AddDays(1), isActive: false);
        await SeedReservationAsync(bag, 50, 1, SurpriseBagReservationStatus.Reserved);
        (await _service.GetManagedByIdAsync(bag.Id))!.IsPickupLocked.Should().BeTrue();

        var result = await _service.UploadImageAsync(bag.Id, BuildFormFile());

        var managed = await _service.GetManagedByIdAsync(bag.Id);
        managed!.ImageUrl.Should().Be(result);
        managed.IsActive.Should().BeFalse();
        managed.ReservedQuantity.Should().Be(1);
        _context.SurpriseBagReservations.Single().Status.Should().Be(SurpriseBagReservationStatus.Reserved);
    }

    [TestMethod]
    public async Task ImageChanges_ShouldRejectNonOwner_ButAllowAdmin()
    {
        var otherOwners = await SeedRestaurantAsync(ownerId: 2);
        var bag = await SeedBagAsync(otherOwners, "Tomorrow", Today.AddDays(1));

        var upload = await Assert.ThrowsExceptionAsync<BusinessRuleException>(
            () => _service.UploadImageAsync(bag.Id, BuildFormFile()));
        upload.Message.Should().Be("You are not authorized to change the image of this Surprise Bag.");
        Directory.Exists(BagImageFolder(bag.Id)).Should().BeFalse();

        var remove = await Assert.ThrowsExceptionAsync<BusinessRuleException>(
            () => _service.DeleteImageAsync(bag.Id));
        remove.Message.Should().Be("You are not authorized to remove the image of this Surprise Bag.");

        _currentUser.Setup(x => x.IsAdmin).Returns(true);

        var result = await _service.UploadImageAsync(bag.Id, BuildFormFile());
        _context.SurpriseBags.Single().ImageUrl.Should().Be(result);

        await _service.DeleteImageAsync(bag.Id);
        _context.SurpriseBags.Single().ImageUrl.Should().BeNull();
    }

    [TestMethod]
    public async Task ImageChanges_ShouldThrowNotFound_WhenBagIsMissing()
    {
        await Assert.ThrowsExceptionAsync<NotFoundException>(
            () => _service.UploadImageAsync(9999, BuildFormFile()));
        await Assert.ThrowsExceptionAsync<NotFoundException>(
            () => _service.DeleteImageAsync(9999));

        Directory.Exists(Path.Combine(_testRootPath, "uploads")).Should().BeFalse();
    }

    [TestMethod]
    public async Task DeleteImageAsync_ShouldClearTheImage_AndDeleteItsFile()
    {
        var restaurant = await SeedRestaurantAsync();
        var bag = await SeedBagAsync(restaurant, "Tomorrow", Today.AddDays(1));
        var imageUrl = await _service.UploadImageAsync(bag.Id, BuildFormFile());

        await _service.DeleteImageAsync(bag.Id);

        _context.SurpriseBags.Single().ImageUrl.Should().BeNull();
        File.Exists(ToAbsolutePath(imageUrl)).Should().BeFalse();
        (await _service.GetPublicByIdAsync(bag.Id))!.ImageUrl.Should().BeNull();
    }

    [TestMethod]
    public async Task DeleteImageAsync_ShouldDoNothing_WhenTheBagHasNoImage()
    {
        var restaurant = await SeedRestaurantAsync();
        var bag = await SeedBagAsync(restaurant, "Tomorrow", Today.AddDays(1));

        await _service.DeleteImageAsync(bag.Id);

        var saved = _context.SurpriseBags.Single();
        saved.ImageUrl.Should().BeNull();
        saved.UpdatedAt.Should().BeNull();
    }

    // Only files under /uploads/surprisebags/ are ever deleted.
    [TestMethod]
    [DataRow("/uploads/demo/bakery-01.jpg")]
    [DataRow("/uploads/surprisebags/../demo/bakery-01.jpg")]
    public async Task ImageChanges_ShouldNotDeleteFilesOutsideSurpriseBagUploads(string storedPath)
    {
        var restaurant = await SeedRestaurantAsync();
        var bag = await SeedBagAsync(restaurant, "Tomorrow", Today.AddDays(1));
        var otherFile = CreateFileFor("/uploads/demo/bakery-01.jpg");

        bag.ImageUrl = storedPath;
        await _context.SaveChangesAsync();

        await _service.UploadImageAsync(bag.Id, BuildFormFile());
        File.Exists(otherFile).Should().BeTrue();

        bag.ImageUrl = storedPath;
        await _context.SaveChangesAsync();

        await _service.DeleteImageAsync(bag.Id);
        File.Exists(otherFile).Should().BeTrue();
        _context.SurpriseBags.Single().ImageUrl.Should().BeNull();
    }

    // Editing or creating a bag's details never changes its image.
    [TestMethod]
    public async Task CreateAndUpdate_ShouldNotChangeTheImage()
    {
        var restaurant = await SeedRestaurantAsync();
        var created = await _service.CreateAsync(ValidCreateDto(restaurant.Id));
        created.ImageUrl.Should().BeNull();

        var imageUrl = await _service.UploadImageAsync(created.Id, BuildFormFile());
        var bag = _context.SurpriseBags.Single();
        var dto = UpdateDtoFrom(bag);
        dto.Title = "Renamed bag";

        var updated = await _service.UpdateAsync(bag.Id, dto);

        updated.Title.Should().Be("Renamed bag");
        updated.ImageUrl.Should().Be(imageUrl);
        File.Exists(ToAbsolutePath(imageUrl)).Should().BeTrue();
    }

    // =========================================================
    // DeactivateAsync
    // =========================================================

    [TestMethod]
    public async Task DeactivateAsync_ShouldHideBag_AndKeepReservations()
    {
        var restaurant = await SeedRestaurantAsync();
        var bag = await SeedBagAsync(restaurant, "Tomorrow", Today.AddDays(1));
        var reservation = await SeedReservationAsync(bag, 50, 1, SurpriseBagReservationStatus.Reserved);

        var result = await _service.DeactivateAsync(bag.Id);

        result.Should().BeTrue();
        bag.IsActive.Should().BeFalse();
        reservation.Status.Should().Be(SurpriseBagReservationStatus.Reserved);

        (await _service.GetPublicByRestaurantAsync(restaurant.Id)).Should().BeEmpty();

        var ownerView = (await _service.GetByOwnerAsync(1)).Single();
        ownerView.IsActive.Should().BeFalse();
        ownerView.ReservedQuantity.Should().Be(1);
    }

    [TestMethod]
    public async Task DeactivateAsync_ShouldRejectNonOwner_AndReturnFalse_WhenMissing()
    {
        var restaurant = await SeedRestaurantAsync(ownerId: 2);
        var bag = await SeedBagAsync(restaurant, "Tomorrow", Today.AddDays(1));

        var ex = await Assert.ThrowsExceptionAsync<BusinessRuleException>(() => _service.DeactivateAsync(bag.Id));
        ex.Message.Should().Be("You are not authorized to deactivate this Surprise Bag.");
        bag.IsActive.Should().BeTrue();

        (await _service.DeactivateAsync(9999)).Should().BeFalse();
    }
}
