using EatKath.API.Data;
using EatKath.API.DTOs.SurpriseBagReservation;
using EatKath.API.Entities;
using EatKath.API.Enums;
using EatKath.API.Interfaces;
using EatKath.API.Services;
using EatKath.API.Tests.Helpers;
using EatKath.API.Validators.SurpriseBagReservation;
using FluentAssertions;
using FluentValidation;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace EatKath.API.Tests.Services;

// Customer Surprise Bag reservations. "Now" is Kathmandu business time:
// 12:00 on 10 Nov 2026 (06:15 UTC) unless a test sets another clock.
// (Row locking and SQL Server concurrency are verified against LocalDB
// separately; the InMemory provider has neither.)
[TestClass]
public class SurpriseBagReservationServiceTests
{
    private static readonly DateTime NoonInKathmanduUtc = new(2026, 11, 10, 6, 15, 0);
    private static readonly DateOnly Today = new(2026, 11, 10);

    private ApplicationDbContext _context = null!;
    private Mock<ICurrentUserService> _currentUser = null!;
    private QueueCodeGenerator _codes = null!;
    private SurpriseBagReservationService _service = null!;

    private User _owner = null!;
    private User _otherOwner = null!;
    private User _customer = null!;
    private User _otherCustomer = null!;
    private Restaurant _restaurant = null!;
    private Restaurant _otherRestaurant = null!;

    // Hands out queued codes first, then random ones.
    private sealed class QueueCodeGenerator : IConfirmationCodeGenerator
    {
        private readonly ConfirmationCodeGenerator _random = new();

        public Queue<string> Next { get; } = new();

        public bool AlwaysRepeat { get; set; }

        public string Generate()
        {
            if (AlwaysRepeat && Next.Count > 0)
                return Next.Peek();

            return Next.Count > 0 ? Next.Dequeue() : _random.Generate();
        }
    }

    [TestInitialize]
    public async Task Setup()
    {
        _context = TestDbContextFactory.Create();
        _currentUser = new Mock<ICurrentUserService>();
        _codes = new QueueCodeGenerator();

        _owner = NewUser("Ram", "Owner");
        _otherOwner = NewUser("Sita", "Owner");
        _customer = NewUser("Asha", "Shrestha");
        _otherCustomer = NewUser("Bikash", "Gurung");
        _context.Users.AddRange(_owner, _otherOwner, _customer, _otherCustomer);
        await _context.SaveChangesAsync();

        _restaurant = NewRestaurant(_owner, "Himalayan Bakery");
        _otherRestaurant = NewRestaurant(_otherOwner, "Thamel Kitchen");
        _context.Restaurants.AddRange(_restaurant, _otherRestaurant);
        await _context.SaveChangesAsync();

        ActAs(_customer);
        UseClock(NoonInKathmanduUtc);
    }

    private static User NewUser(string first, string last) => new()
    {
        FirstName = first,
        LastName = last,
        Email = $"{first.ToLowerInvariant()}@example.com",
        PhoneNumber = "9800000000",
        PasswordHash = "x"
    };

    private static Restaurant NewRestaurant(User owner, string name) => new()
    {
        OwnerId = owner.Id,
        Name = name,
        Address = "Thamel, Kathmandu",
        PhoneNumber = "01-4000000",
        CurrencyCode = "NPR",
        IsActive = true
    };

    private void ActAs(User user, bool isAdmin = false)
    {
        _currentUser.Setup(x => x.UserId).Returns(user.Id);
        _currentUser.Setup(x => x.IsAdmin).Returns(isAdmin);
    }

    private void ActAsAdmin()
    {
        _currentUser.Setup(x => x.UserId).Returns(999);
        _currentUser.Setup(x => x.IsAdmin).Returns(true);
    }

    private void UseClock(DateTime utc)
    {
        _service = new SurpriseBagReservationService(
            _context,
            MapperFactory.Create(),
            _currentUser.Object,
            BusinessTimeProvider.CreateKathmandu(new FixedTimeProvider(utc)),
            _codes,
            new CreateSurpriseBagReservationValidator(),
            new CollectSurpriseBagReservationValidator());
    }

    private async Task<SurpriseBag> SeedBagAsync(
        DateOnly? pickupDate = null,
        TimeOnly? start = null,
        TimeOnly? end = null,
        int quantityTotal = 5,
        int maxPerCustomer = 3,
        bool isActive = true,
        Restaurant? restaurant = null,
        decimal price = 250m)
    {
        var bag = new SurpriseBag
        {
            RestaurantId = (restaurant ?? _restaurant).Id,
            Title = "End-of-day bakery bag",
            Category = SurpriseBagCategory.Bakery,
            Price = price,
            EstimatedValue = 750m,
            QuantityTotal = quantityTotal,
            MaxPerCustomer = maxPerCustomer,
            PickupDate = pickupDate ?? Today.AddDays(1),
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
        User customer,
        int quantity,
        SurpriseBagReservationStatus status,
        string? code = null)
    {
        var reservation = new SurpriseBagReservation
        {
            SurpriseBagId = bag.Id,
            UserId = customer.Id,
            Quantity = quantity,
            UnitPrice = bag.Price,
            ConfirmationCode = code ?? new ConfirmationCodeGenerator().Generate(),
            Status = status
        };

        _context.SurpriseBagReservations.Add(reservation);
        await _context.SaveChangesAsync();

        return reservation;
    }

    private async Task<int> RemainingAsync(SurpriseBag bag) =>
        (await SurpriseBagAvailability.GetUsageAsync(_context, bag.Id)).RemainingOf(bag.QuantityTotal);

    private Task<SurpriseBagReservationDto> ReserveAsync(SurpriseBag bag, int quantity = 1) =>
        _service.CreateAsync(new CreateSurpriseBagReservationDto { SurpriseBagId = bag.Id, Quantity = quantity });

    private static async Task<string> RejectedAsync(Func<Task> action)
    {
        var ex = await Assert.ThrowsExceptionAsync<BusinessRuleException>(action);
        return ex.Message;
    }

    // =========================================================
    // CreateAsync
    // =========================================================

    [TestMethod]
    public async Task CreateAsync_ShouldReserve_WithSnapshotPrice_CodeAndPayAtPickupTotal()
    {
        var bag = await SeedBagAsync(price: 250.50m);

        var result = await ReserveAsync(bag, 2);

        result.Status.Should().Be(SurpriseBagReservationStatus.Reserved);
        result.Quantity.Should().Be(2);
        result.UnitPrice.Should().Be(250.50m);
        result.TotalPrice.Should().Be(501.00m);
        result.CurrencyCode.Should().Be("NPR");
        result.BagTitle.Should().Be("End-of-day bakery bag");
        result.RestaurantName.Should().Be("Himalayan Bakery");
        result.PickupDate.Should().Be(Today.AddDays(1));
        result.ConfirmationCode.Should().MatchRegex("^[ABCDEFGHJKMNPQRSTUVWXYZ23456789]{8}$");
        result.CanCancel.Should().BeTrue();

        var saved = _context.SurpriseBagReservations.Single();
        saved.UserId.Should().Be(_customer.Id);
        (await RemainingAsync(bag)).Should().Be(3);
    }

    [TestMethod]
    public async Task CreateAsync_ShouldThrowNotFound_WhenBagIsMissing()
    {
        await Assert.ThrowsExceptionAsync<NotFoundException>(
            () => _service.CreateAsync(new CreateSurpriseBagReservationDto { SurpriseBagId = 9999, Quantity = 1 }));
    }

    [TestMethod]
    public async Task CreateAsync_ShouldThrowValidationException_WhenQuantityIsInvalid()
    {
        var bag = await SeedBagAsync();

        await Assert.ThrowsExceptionAsync<ValidationException>(() => ReserveAsync(bag, 0));

        _context.SurpriseBagReservations.Should().BeEmpty();
    }

    [TestMethod]
    public async Task CreateAsync_ShouldReject_InactiveBag_OrInactiveRestaurant()
    {
        var inactiveBag = await SeedBagAsync(isActive: false);
        (await RejectedAsync(() => ReserveAsync(inactiveBag))).Should().Be("This Surprise Bag is not available.");

        var bag = await SeedBagAsync();
        _restaurant.IsActive = false;
        await _context.SaveChangesAsync();
        (await RejectedAsync(() => ReserveAsync(bag))).Should().Be("This Surprise Bag is not available.");
    }

    [TestMethod]
    public async Task CreateAsync_ShouldAllowReservingUntilThePickupWindowEnds()
    {
        var ended = await SeedBagAsync(Today, new TimeOnly(10, 0), new TimeOnly(12, 0)); // ends exactly now
        var inProgress = await SeedBagAsync(Today, new TimeOnly(11, 30), new TimeOnly(12, 30));

        (await RejectedAsync(() => ReserveAsync(ended))).Should().Be("The pickup window for this Surprise Bag has ended.");

        var result = await ReserveAsync(inProgress);
        result.Status.Should().Be(SurpriseBagReservationStatus.Reserved);
        result.CanCancel.Should().BeFalse(); // the window has started
    }

    // 18:30 UTC on 10 Nov is 00:15 on 11 Nov in Kathmandu.
    [TestMethod]
    public async Task CreateAsync_ShouldUseKathmanduDate_AfterUtcRollover()
    {
        UseClock(new DateTime(2026, 11, 10, 18, 30, 0));
        var bag = await SeedBagAsync(new DateOnly(2026, 11, 10), new TimeOnly(18, 0), new TimeOnly(19, 30));

        (await RejectedAsync(() => ReserveAsync(bag))).Should().Be("The pickup window for this Surprise Bag has ended.");
    }

    [TestMethod]
    public async Task CreateAsync_ShouldRejectDuplicateActiveReservation_ButAllowAfterCancelling()
    {
        var bag = await SeedBagAsync();
        var first = await ReserveAsync(bag);

        (await RejectedAsync(() => ReserveAsync(bag))).Should().Be("You already have a reservation for this Surprise Bag.");

        await _service.CancelMineAsync(first.Id);

        (await ReserveAsync(bag)).Status.Should().Be(SurpriseBagReservationStatus.Reserved);
        _context.SurpriseBagReservations.Count(r => r.Status == SurpriseBagReservationStatus.Reserved).Should().Be(1);
    }

    [TestMethod]
    public async Task CreateAsync_ShouldEnforceMaxPerCustomer_AcrossReservedAndCollected()
    {
        var bag = await SeedBagAsync(quantityTotal: 10, maxPerCustomer: 3);

        (await RejectedAsync(() => ReserveAsync(bag, 4))).Should().Be("You can reserve at most 3 of this Surprise Bag.");

        await SeedReservationAsync(bag, _customer, 2, SurpriseBagReservationStatus.Collected);

        (await RejectedAsync(() => ReserveAsync(bag, 2)))
            .Should().Be("You can reserve at most 3 of this Surprise Bag (you already have 2).");

        (await ReserveAsync(bag, 1)).Quantity.Should().Be(1);
    }

    [TestMethod]
    public async Task CreateAsync_ShouldNotCountCancelledOrNoShow_TowardsTheCustomerLimit()
    {
        var bag = await SeedBagAsync(quantityTotal: 10, maxPerCustomer: 2);
        await SeedReservationAsync(bag, _customer, 2, SurpriseBagReservationStatus.Cancelled);
        await SeedReservationAsync(bag, _customer, 2, SurpriseBagReservationStatus.NoShow);

        (await ReserveAsync(bag, 2)).Quantity.Should().Be(2);
    }

    [TestMethod]
    public async Task CreateAsync_ShouldRejectWhenNotEnoughStock_AndWhenSoldOut()
    {
        var bag = await SeedBagAsync(quantityTotal: 3, maxPerCustomer: 3);
        await SeedReservationAsync(bag, _otherCustomer, 1, SurpriseBagReservationStatus.Reserved);
        await SeedReservationAsync(bag, _otherOwner, 1, SurpriseBagReservationStatus.Collected);

        (await RejectedAsync(() => ReserveAsync(bag, 2))).Should().Be("Only 1 left.");

        await ReserveAsync(bag, 1);

        ActAs(_owner); // a different customer id with nothing reserved
        (await RejectedAsync(() => ReserveAsync(bag, 1))).Should().Be("This Surprise Bag is sold out.");
    }

    [TestMethod]
    public async Task CreateAsync_ShouldReleaseStock_FromCancelledAndNoShowReservations()
    {
        var bag = await SeedBagAsync(quantityTotal: 2, maxPerCustomer: 2);
        await SeedReservationAsync(bag, _otherCustomer, 2, SurpriseBagReservationStatus.Cancelled);
        await SeedReservationAsync(bag, _otherOwner, 1, SurpriseBagReservationStatus.NoShow);

        (await ReserveAsync(bag, 2)).Quantity.Should().Be(2);
        (await RemainingAsync(bag)).Should().Be(0);
    }

    [TestMethod]
    public async Task CreateAsync_ShouldRetryWithANewCode_WhenTheGeneratedCodeAlreadyExists()
    {
        var bag = await SeedBagAsync();
        await SeedReservationAsync(bag, _otherCustomer, 1, SurpriseBagReservationStatus.Collected, "AAAAAAAA");

        _codes.Next.Enqueue("AAAAAAAA");
        _codes.Next.Enqueue("BBBBBBBB");

        (await ReserveAsync(bag)).ConfirmationCode.Should().Be("BBBBBBBB");
    }

    [TestMethod]
    public async Task CreateAsync_ShouldGiveUpSafely_AfterBoundedCodeAttempts()
    {
        var bag = await SeedBagAsync();
        await SeedReservationAsync(bag, _otherCustomer, 1, SurpriseBagReservationStatus.Collected, "AAAAAAAA");

        _codes.Next.Enqueue("AAAAAAAA");
        _codes.AlwaysRepeat = true;

        (await RejectedAsync(() => ReserveAsync(bag)))
            .Should().Be("We couldn't create your reservation right now. Please try again.");

        _context.SurpriseBagReservations.Count().Should().Be(1);
    }

    // =========================================================
    // GetMyAsync / GetByIdAsync
    // =========================================================

    [TestMethod]
    public async Task GetMyAsync_ShouldKeepHistory_WithReservedPrice_AfterTheBagChangesOrExpires()
    {
        var current = await SeedBagAsync(price: 250m);
        var past = await SeedBagAsync(Today.AddDays(-2), price: 100m);

        var mine = await ReserveAsync(current, 1);
        await SeedReservationAsync(past, _customer, 1, SurpriseBagReservationStatus.Collected);
        await SeedReservationAsync(current, _otherCustomer, 1, SurpriseBagReservationStatus.Reserved);

        // The owner later changes the price and deactivates the bag.
        current.Price = 400m;
        current.IsActive = false;
        await _context.SaveChangesAsync();

        var result = (await _service.GetMyAsync()).ToList();

        result.Should().HaveCount(2);
        result.Should().OnlyContain(r => r.RestaurantName == "Himalayan Bakery");

        var currentDto = result.Single(r => r.Id == mine.Id);
        currentDto.UnitPrice.Should().Be(250m);
        currentDto.TotalPrice.Should().Be(250m);
        currentDto.CanCancel.Should().BeTrue();

        var pastDto = result.Single(r => r.SurpriseBagId == past.Id);
        pastDto.Status.Should().Be(SurpriseBagReservationStatus.Collected);
        pastDto.UnitPrice.Should().Be(100m);
        pastDto.CanCancel.Should().BeFalse();
    }

    [TestMethod]
    public async Task GetByIdAsync_ShouldShowCustomerView_ToTheCustomer_AndOwnerView_ToOwnerAndAdmin_Only()
    {
        var bag = await SeedBagAsync();
        var reservation = await ReserveAsync(bag);

        var asCustomer = await _service.GetByIdAsync(reservation.Id);
        asCustomer.Should().NotBeNull();
        asCustomer.Should().NotBeOfType<OwnerSurpriseBagReservationDto>();

        ActAs(_owner);
        var asOwner = (await _service.GetByIdAsync(reservation.Id)).Should().BeOfType<OwnerSurpriseBagReservationDto>().Subject;
        asOwner.CustomerName.Should().Be("Asha Shrestha");
        asOwner.CustomerEmail.Should().Be("asha@example.com");
        asOwner.UserId.Should().Be(_customer.Id);

        ActAsAdmin();
        (await _service.GetByIdAsync(reservation.Id)).Should().BeOfType<OwnerSurpriseBagReservationDto>();

        ActAs(_otherCustomer);
        (await _service.GetByIdAsync(reservation.Id)).Should().BeNull();

        ActAs(_otherOwner);
        (await _service.GetByIdAsync(reservation.Id)).Should().BeNull();

        (await _service.GetByIdAsync(9999)).Should().BeNull();
    }

    // =========================================================
    // CancelMineAsync
    // =========================================================

    [TestMethod]
    public async Task CancelMineAsync_ShouldCancelBeforeThePickupWindowStarts_AndReleaseStock()
    {
        var bag = await SeedBagAsync(quantityTotal: 2, maxPerCustomer: 2);
        var reservation = await ReserveAsync(bag, 2);

        var result = await _service.CancelMineAsync(reservation.Id);

        result.Status.Should().Be(SurpriseBagReservationStatus.Cancelled);
        result.CancelledAt.Should().NotBeNull();
        result.CanCancel.Should().BeFalse();
        (await RemainingAsync(bag)).Should().Be(2);
    }

    [TestMethod]
    public async Task CancelMineAsync_ShouldReject_OnceThePickupWindowHasStarted()
    {
        var bag = await SeedBagAsync(Today, new TimeOnly(11, 30), new TimeOnly(12, 30));
        var reservation = await SeedReservationAsync(bag, _customer, 1, SurpriseBagReservationStatus.Reserved);

        (await RejectedAsync(() => _service.CancelMineAsync(reservation.Id)))
            .Should().Be("Reservations can only be cancelled before the pickup window starts.");

        reservation.Status.Should().Be(SurpriseBagReservationStatus.Reserved);
    }

    [TestMethod]
    [DataRow(SurpriseBagReservationStatus.Collected, "This reservation has already been collected.")]
    [DataRow(SurpriseBagReservationStatus.Cancelled, "This reservation has already been cancelled.")]
    [DataRow(SurpriseBagReservationStatus.NoShow, "This reservation has already been marked as a no-show.")]
    public async Task CancelMineAsync_ShouldRejectReservationsThatAreNotReserved(
        SurpriseBagReservationStatus status, string message)
    {
        var bag = await SeedBagAsync();
        var reservation = await SeedReservationAsync(bag, _customer, 1, status);

        (await RejectedAsync(() => _service.CancelMineAsync(reservation.Id))).Should().Be(message);
    }

    [TestMethod]
    public async Task CancelMineAsync_ShouldReturnNotFound_ForAnotherCustomersOrMissingReservation()
    {
        var bag = await SeedBagAsync();
        var theirs = await SeedReservationAsync(bag, _otherCustomer, 1, SurpriseBagReservationStatus.Reserved);

        await Assert.ThrowsExceptionAsync<NotFoundException>(() => _service.CancelMineAsync(theirs.Id));
        await Assert.ThrowsExceptionAsync<NotFoundException>(() => _service.CancelMineAsync(9999));

        theirs.Status.Should().Be(SurpriseBagReservationStatus.Reserved);
    }

    // =========================================================
    // Owner: list and cancel
    // =========================================================

    [TestMethod]
    public async Task GetForRestaurantAsync_ShouldListOnlyThatRestaurantsReservations_WithFilters()
    {
        var tomorrow = await SeedBagAsync(Today.AddDays(1));
        var later = await SeedBagAsync(Today.AddDays(3));
        var elsewhere = await SeedBagAsync(restaurant: _otherRestaurant);

        await SeedReservationAsync(tomorrow, _customer, 1, SurpriseBagReservationStatus.Reserved);
        await SeedReservationAsync(tomorrow, _otherCustomer, 2, SurpriseBagReservationStatus.Cancelled);
        await SeedReservationAsync(later, _customer, 1, SurpriseBagReservationStatus.Reserved);
        await SeedReservationAsync(elsewhere, _customer, 1, SurpriseBagReservationStatus.Reserved);

        ActAs(_owner);

        var all = (await _service.GetForRestaurantAsync(_restaurant.Id)).ToList();
        all.Should().HaveCount(3);
        all.Should().OnlyContain(r => r.RestaurantId == _restaurant.Id);
        all.First().PickupDate.Should().Be(Today.AddDays(3)); // latest pickup first
        all.Should().Contain(r => r.CustomerName == "Bikash Gurung" && r.Quantity == 2);

        (await _service.GetForRestaurantAsync(_restaurant.Id, pickupDate: Today.AddDays(1))).Should().HaveCount(2);
        (await _service.GetForRestaurantAsync(_restaurant.Id, status: SurpriseBagReservationStatus.Cancelled))
            .Single().CustomerName.Should().Be("Bikash Gurung");
    }

    [TestMethod]
    public async Task GetForRestaurantAsync_ShouldReturnNotFound_ForAnotherOwner_ButAllowAdmin()
    {
        var bag = await SeedBagAsync();
        await SeedReservationAsync(bag, _customer, 1, SurpriseBagReservationStatus.Reserved);

        ActAs(_otherOwner);
        await Assert.ThrowsExceptionAsync<NotFoundException>(() => _service.GetForRestaurantAsync(_restaurant.Id));
        await Assert.ThrowsExceptionAsync<NotFoundException>(() => _service.GetForRestaurantAsync(9999));

        ActAsAdmin();
        (await _service.GetForRestaurantAsync(_restaurant.Id)).Should().ContainSingle();
    }

    [TestMethod]
    public async Task CancelAsync_ShouldLetTheOwnerCancelAReservedBooking_EvenDuringTheWindow()
    {
        var bag = await SeedBagAsync(Today, new TimeOnly(11, 30), new TimeOnly(12, 30), quantityTotal: 2);
        var reservation = await SeedReservationAsync(bag, _customer, 1, SurpriseBagReservationStatus.Reserved);

        ActAs(_owner);
        var result = await _service.CancelAsync(reservation.Id);

        result.Status.Should().Be(SurpriseBagReservationStatus.Cancelled);
        result.CancelledAt.Should().NotBeNull();
        result.CustomerName.Should().Be("Asha Shrestha");
        (await RemainingAsync(bag)).Should().Be(2);

        (await RejectedAsync(() => _service.CancelAsync(reservation.Id)))
            .Should().Be("This reservation has already been cancelled.");
    }

    [TestMethod]
    public async Task CancelAsync_ShouldReturnNotFound_ForAnotherRestaurant_ButAllowAdmin()
    {
        var bag = await SeedBagAsync();
        var reservation = await SeedReservationAsync(bag, _customer, 1, SurpriseBagReservationStatus.Reserved);

        ActAs(_otherOwner);
        await Assert.ThrowsExceptionAsync<NotFoundException>(() => _service.CancelAsync(reservation.Id));
        reservation.Status.Should().Be(SurpriseBagReservationStatus.Reserved);

        ActAsAdmin();
        (await _service.CancelAsync(reservation.Id)).Status.Should().Be(SurpriseBagReservationStatus.Cancelled);
    }

    // =========================================================
    // CollectAsync
    // =========================================================

    [TestMethod]
    public async Task CollectAsync_ShouldCollectOnThePickupDate_WithTheCodeInAnyCase()
    {
        var bag = await SeedBagAsync(Today, new TimeOnly(18, 0), new TimeOnly(19, 30));
        var reservation = await SeedReservationAsync(bag, _customer, 2, SurpriseBagReservationStatus.Reserved, "K7M2Q9XA");

        ActAs(_owner);
        var result = await _service.CollectAsync(new CollectSurpriseBagReservationDto { ConfirmationCode = "  k7m2q9xa " });

        result.Id.Should().Be(reservation.Id);
        result.Status.Should().Be(SurpriseBagReservationStatus.Collected);
        result.CollectedAt.Should().NotBeNull();
        result.TotalPrice.Should().Be(500m);
        result.CustomerName.Should().Be("Asha Shrestha");

        (await RejectedAsync(() => _service.CollectAsync(new CollectSurpriseBagReservationDto { ConfirmationCode = "K7M2Q9XA" })))
            .Should().Be("This reservation has already been collected.");
    }

    [TestMethod]
    public async Task CollectAsync_ShouldNotRevealCodesOfOtherRestaurants()
    {
        var bag = await SeedBagAsync(Today, restaurant: _otherRestaurant);
        var reservation = await SeedReservationAsync(bag, _customer, 1, SurpriseBagReservationStatus.Reserved, "K7M2Q9XA");

        ActAs(_owner);

        var otherRestaurants = await Assert.ThrowsExceptionAsync<NotFoundException>(
            () => _service.CollectAsync(new CollectSurpriseBagReservationDto { ConfirmationCode = "K7M2Q9XA" }));
        var unknown = await Assert.ThrowsExceptionAsync<NotFoundException>(
            () => _service.CollectAsync(new CollectSurpriseBagReservationDto { ConfirmationCode = "ZZZZZZZZ" }));

        otherRestaurants.Message.Should().Be(unknown.Message);
        reservation.Status.Should().Be(SurpriseBagReservationStatus.Reserved);

        ActAsAdmin();
        (await _service.CollectAsync(new CollectSurpriseBagReservationDto { ConfirmationCode = "K7M2Q9XA" }))
            .Status.Should().Be(SurpriseBagReservationStatus.Collected);
    }

    [TestMethod]
    public async Task CollectAsync_ShouldRejectBeforeOrAfterThePickupDate()
    {
        var tomorrow = await SeedBagAsync(Today.AddDays(1));
        var yesterday = await SeedBagAsync(Today.AddDays(-1));
        await SeedReservationAsync(tomorrow, _customer, 1, SurpriseBagReservationStatus.Reserved, "TOMORROW");
        await SeedReservationAsync(yesterday, _customer, 1, SurpriseBagReservationStatus.Reserved, "YESTERDA");

        ActAs(_owner);

        (await RejectedAsync(() => _service.CollectAsync(new CollectSurpriseBagReservationDto { ConfirmationCode = "TOMORROW" })))
            .Should().Be("This reservation can only be collected on its pickup date (2026-11-11).");
        (await RejectedAsync(() => _service.CollectAsync(new CollectSurpriseBagReservationDto { ConfirmationCode = "YESTERDA" })))
            .Should().Be("The pickup date for this reservation has passed. Mark it as a no-show instead.");
    }

    [TestMethod]
    public async Task CollectAsync_ShouldRejectACancelledReservation_AndAnEmptyCode()
    {
        var bag = await SeedBagAsync(Today);
        await SeedReservationAsync(bag, _customer, 1, SurpriseBagReservationStatus.Cancelled, "K7M2Q9XA");

        ActAs(_owner);

        (await RejectedAsync(() => _service.CollectAsync(new CollectSurpriseBagReservationDto { ConfirmationCode = "K7M2Q9XA" })))
            .Should().Be("This reservation has already been cancelled.");
        await Assert.ThrowsExceptionAsync<ValidationException>(
            () => _service.CollectAsync(new CollectSurpriseBagReservationDto { ConfirmationCode = " " }));
    }

    // =========================================================
    // MarkNoShowAsync
    // =========================================================

    [TestMethod]
    public async Task MarkNoShowAsync_ShouldOnlyWorkAfterThePickupWindowEnds_AndReleaseStock()
    {
        var running = await SeedBagAsync(Today, new TimeOnly(11, 30), new TimeOnly(12, 30), quantityTotal: 2);
        var ended = await SeedBagAsync(Today, new TimeOnly(9, 0), new TimeOnly(11, 0), quantityTotal: 2);
        var notYetOver = await SeedReservationAsync(running, _customer, 1, SurpriseBagReservationStatus.Reserved);
        var over = await SeedReservationAsync(ended, _customer, 2, SurpriseBagReservationStatus.Reserved);

        ActAs(_owner);

        (await RejectedAsync(() => _service.MarkNoShowAsync(notYetOver.Id)))
            .Should().Be("A reservation can only be marked as a no-show after the pickup window ends.");

        var result = await _service.MarkNoShowAsync(over.Id);
        result.Status.Should().Be(SurpriseBagReservationStatus.NoShow);
        (await RemainingAsync(ended)).Should().Be(2);

        (await RejectedAsync(() => _service.MarkNoShowAsync(over.Id)))
            .Should().Be("This reservation has already been marked as a no-show.");
    }

    [TestMethod]
    public async Task MarkNoShowAsync_ShouldReturnNotFound_ForAnotherRestaurant()
    {
        var bag = await SeedBagAsync(Today.AddDays(-1));
        var reservation = await SeedReservationAsync(bag, _customer, 1, SurpriseBagReservationStatus.Reserved);

        ActAs(_otherOwner);
        await Assert.ThrowsExceptionAsync<NotFoundException>(() => _service.MarkNoShowAsync(reservation.Id));
        await Assert.ThrowsExceptionAsync<NotFoundException>(() => _service.MarkNoShowAsync(9999));

        reservation.Status.Should().Be(SurpriseBagReservationStatus.Reserved);
    }
}
