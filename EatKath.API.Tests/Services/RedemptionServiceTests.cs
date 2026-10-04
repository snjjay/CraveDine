using AutoMapper;
using EatKath.API.Data;
using EatKath.API.DTOs.Redemption;
using EatKath.API.Entities;
using EatKath.API.Enums;
using EatKath.API.Interfaces;
using EatKath.API.Services;
using EatKath.API.Tests.Helpers;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;


namespace EatKath.API.Tests.Services;

[TestClass]
public class RedemptionServiceTests
{
    private ApplicationDbContext _context = null!;
    private IMapper _mapper = null!;
    private Mock<ICurrentUserService> _currentUser = null!;
    private RedemptionService _service = null!;
    private FixedTimeProvider _clock = null!;

    // "Now" for every test: 10 Nov 2026, 10:00.
    private static readonly DateOnly Today = new(2026, 11, 10);

    [TestInitialize]
    public void Setup()
    {
        _context = TestDbContextFactory.Create();

        _mapper = MapperFactory.Create();

        _currentUser = new Mock<ICurrentUserService>();

        _currentUser.Setup(x => x.UserId).Returns(1);

        // Default: an Admin caller, so existing tests that don't
        // care about ownership continue to behave as before.
        _currentUser.Setup(x => x.IsAdmin).Returns(true);

        _clock = new FixedTimeProvider(new DateTime(2026, 11, 10, 10, 0, 0));

        _service = new RedemptionService(
            _context,
            _currentUser.Object,
            _mapper,
            _clock);
    }

    [TestMethod]
    public async Task CompleteRedemptionAsync_ShouldCompleteRedemption_WhenRequestIsValid()
    {
        // -----------------------------
        // Arrange
        // Purpose:
        // Verify that a redeemed offer
        // can be completed successfully.
        // -----------------------------

        var user = new User
        {
            Id = 1,
            FirstName = "John",
            LastName = "Smith",
            Email = "john@test.com",
            PasswordHash = "Hash",
            PhoneNumber = "0400000000",
            RoleId = 1
        };

        var restaurant = new Restaurant
        {
            Name = "Spice Kitchen",
            IsActive = true
        };

        _context.Users.Add(user);
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

        var redemption = new Redemption
        {
            DealId = deal.Id,
            UserId = user.Id,
            Status = RedemptionStatus.Redeemed
        };

        _context.Redemptions.Add(redemption);
        await _context.SaveChangesAsync();

        var dto = new CompleteRedemptionDto
        {
            BillAmount = 100m
        };

        // -----------------------------
        // Act
        // -----------------------------

        var result = await _service.CompleteRedemptionAsync(redemption.Id, dto);

        // -----------------------------
        // Assert
        // -----------------------------

        result.Should().NotBeNull();

        var saved = _context.Redemptions.First();

        saved.Status.Should().Be(RedemptionStatus.Completed);
        saved.BillAmount.Should().Be(100m);
        saved.DiscountAmount.Should().Be(20m);
        saved.FinalAmount.Should().Be(80m);
    }

    [TestMethod]
    public async Task CompleteRedemptionAsync_ShouldThrowException_WhenRedemptionNotFound()
    {
        // -----------------------------
        // Arrange
        // Purpose:
        // Verify an exception is thrown
        // when the redemption does not exist.
        // -----------------------------

        var dto = new CompleteRedemptionDto
        {
            BillAmount = 100m
        };

        // -----------------------------
        // Act & Assert
        // -----------------------------

        var ex = await Assert.ThrowsExceptionAsync<NotFoundException>(
            () => _service.CompleteRedemptionAsync(999, dto));

        ex.Message.Should().Be("Redemption not found.");
    }


    [TestMethod]
    public async Task CompleteRedemptionAsync_ShouldThrowException_WhenRedemptionAlreadyCompleted()
    {
        // -----------------------------
        // Arrange
        // Purpose:
        // Verify a completed redemption
        // cannot be completed again.
        // -----------------------------

        var user = new User
        {
            Id = 1,
            FirstName = "John",
            LastName = "Smith",
            Email = "john@test.com",
            PasswordHash = "Hash",
            PhoneNumber = "0400000000",
            RoleId = 1
        };

        var restaurant = new Restaurant
        {
            Name = "Spice Kitchen",
            IsActive = true
        };

        _context.Users.Add(user);
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

        var redemption = new Redemption
        {
            DealId = deal.Id,
            UserId = user.Id,
            Status = RedemptionStatus.Completed
        };

        _context.Redemptions.Add(redemption);
        await _context.SaveChangesAsync();

        var dto = new CompleteRedemptionDto
        {
            BillAmount = 100m
        };

        // -----------------------------
        // Act & Assert
        // -----------------------------

        var ex = await Assert.ThrowsExceptionAsync<BusinessRuleException>(
            () => _service.CompleteRedemptionAsync(redemption.Id, dto));

        ex.Message.Should().Be("Redemption has already been completed.");
    }


    // -----------------------------
    // CompleteRedemptionAsync - ownership rules
    // -----------------------------

    [TestMethod]
    public async Task CompleteRedemptionAsync_ShouldSucceed_WhenOwnerOwnsRestaurant()
    {
        // -----------------------------
        // Arrange
        // -----------------------------

        _currentUser.Setup(x => x.IsAdmin).Returns(false);
        _currentUser.Setup(x => x.UserId).Returns(1);

        var user = new User
        {
            Id = 50,
            FirstName = "John",
            LastName = "Smith",
            Email = "john@test.com",
            PasswordHash = "Hash",
            PhoneNumber = "0400000000",
            RoleId = 1
        };

        var restaurant = new Restaurant
        {
            Name = "Spice Kitchen",
            OwnerId = 1,
            IsActive = true
        };

        _context.Users.Add(user);
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

        var redemption = new Redemption
        {
            DealId = deal.Id,
            UserId = user.Id,
            Status = RedemptionStatus.Redeemed
        };

        _context.Redemptions.Add(redemption);
        await _context.SaveChangesAsync();

        var dto = new CompleteRedemptionDto
        {
            BillAmount = 100m
        };

        // -----------------------------
        // Act
        // -----------------------------

        var result = await _service.CompleteRedemptionAsync(redemption.Id, dto);

        // -----------------------------
        // Assert
        // -----------------------------

        result.Should().NotBeNull();

        var saved = _context.Redemptions.First();

        saved.Status.Should().Be(RedemptionStatus.Completed);
        saved.BillAmount.Should().Be(100m);
    }

    [TestMethod]
    public async Task CompleteRedemptionAsync_ShouldThrowBusinessRuleException_WhenOwnerDoesNotOwnRestaurant()
    {
        // -----------------------------
        // Arrange
        // Purpose:
        // Verify the redemption (status and monetary
        // fields) remains unchanged when rejected.
        // -----------------------------

        _currentUser.Setup(x => x.IsAdmin).Returns(false);
        _currentUser.Setup(x => x.UserId).Returns(1);

        var user = new User
        {
            Id = 50,
            FirstName = "John",
            LastName = "Smith",
            Email = "john@test.com",
            PasswordHash = "Hash",
            PhoneNumber = "0400000000",
            RoleId = 1
        };

        var restaurant = new Restaurant
        {
            Name = "Spice Kitchen",
            OwnerId = 2,
            IsActive = true
        };

        _context.Users.Add(user);
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

        var redemption = new Redemption
        {
            DealId = deal.Id,
            UserId = user.Id,
            Status = RedemptionStatus.Redeemed
        };

        _context.Redemptions.Add(redemption);
        await _context.SaveChangesAsync();

        var dto = new CompleteRedemptionDto
        {
            BillAmount = 100m
        };

        // -----------------------------
        // Act & Assert
        // -----------------------------

        var ex = await Assert.ThrowsExceptionAsync<BusinessRuleException>(
            () => _service.CompleteRedemptionAsync(redemption.Id, dto));

        ex.Message.Should().Be("You are not authorized to complete redemptions for this restaurant.");

        var saved = _context.Redemptions.First();

        saved.Status.Should().Be(RedemptionStatus.Redeemed);
        saved.BillAmount.Should().BeNull();
        saved.DiscountAmount.Should().BeNull();
        saved.FinalAmount.Should().BeNull();
    }

    [TestMethod]
    public async Task CompleteRedemptionAsync_ShouldSucceed_WhenUserIsAdmin_EvenIfNotOwner()
    {
        // -----------------------------
        // Arrange
        // -----------------------------

        _currentUser.Setup(x => x.IsAdmin).Returns(true);
        _currentUser.Setup(x => x.UserId).Returns(999);

        var user = new User
        {
            Id = 50,
            FirstName = "John",
            LastName = "Smith",
            Email = "john@test.com",
            PasswordHash = "Hash",
            PhoneNumber = "0400000000",
            RoleId = 1
        };

        var restaurant = new Restaurant
        {
            Name = "Spice Kitchen",
            OwnerId = 2,
            IsActive = true
        };

        _context.Users.Add(user);
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

        var redemption = new Redemption
        {
            DealId = deal.Id,
            UserId = user.Id,
            Status = RedemptionStatus.Redeemed
        };

        _context.Redemptions.Add(redemption);
        await _context.SaveChangesAsync();

        var dto = new CompleteRedemptionDto
        {
            BillAmount = 100m
        };

        // -----------------------------
        // Act
        // -----------------------------

        var result = await _service.CompleteRedemptionAsync(redemption.Id, dto);

        // -----------------------------
        // Assert
        // -----------------------------

        result.Should().NotBeNull();

        var saved = _context.Redemptions.First();

        saved.Status.Should().Be(RedemptionStatus.Completed);
        saved.BillAmount.Should().Be(100m);
    }


    // -----------------------------
    // CompleteRedemptionAsync - reservation/redemption consistency
    // (Phase 3C: Redemption.ReservationId link)
    // -----------------------------

    [TestMethod]
    public async Task CompleteRedemptionAsync_ShouldRejectWhenLinkedReservationIsCancelled()
    {
        // -----------------------------
        // Arrange
        // Purpose:
        // A redemption auto-created from a reservation must not
        // be completable once that reservation has been cancelled.
        // -----------------------------

        var user = new User
        {
            Id = 50,
            FirstName = "Jane",
            LastName = "Diner",
            Email = "jane@test.com",
            PasswordHash = "Hash",
            PhoneNumber = "0400111222",
            RoleId = 1
        };

        var restaurant = new Restaurant
        {
            Name = "Spice Kitchen",
            IsActive = true
        };

        _context.Users.Add(user);
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

        var reservation = new Reservation
        {
            DealId = deal.Id,
            UserId = user.Id,
            CustomerName = "Jane Diner",
            PhoneNumber = "0400111222",
            Email = "jane@test.com",
            ReservationDate = DateOnly.FromDateTime(DateTime.Today),
            ReservationTime = new TimeOnly(12, 30),
            GuestCount = 2,
            Status = ReservationStatus.Cancelled
        };

        _context.Reservations.Add(reservation);
        await _context.SaveChangesAsync();

        var redemption = new Redemption
        {
            ReservationId = reservation.Id,
            DealId = deal.Id,
            UserId = user.Id,
            ArrivalDate = reservation.ReservationDate,
            ArrivalTime = reservation.ReservationTime,
            Status = RedemptionStatus.Redeemed
        };

        _context.Redemptions.Add(redemption);
        await _context.SaveChangesAsync();

        var dto = new CompleteRedemptionDto
        {
            BillAmount = 100m
        };

        // -----------------------------
        // Act & Assert
        // -----------------------------

        var ex = await Assert.ThrowsExceptionAsync<BusinessRuleException>(
            () => _service.CompleteRedemptionAsync(redemption.Id, dto));

        ex.Message.Should().Be("This redemption cannot be completed because the reservation is not pending.");

        var saved = _context.Redemptions.First();

        saved.Status.Should().Be(RedemptionStatus.Redeemed);
        saved.BillAmount.Should().BeNull();
    }

    [TestMethod]
    public async Task CompleteRedemptionAsync_ShouldNotChangeCancelledReservationToCompleted()
    {
        // -----------------------------
        // Arrange
        // Purpose:
        // Explicitly confirm the linked reservation's Cancelled
        // status is left untouched - it must never be silently
        // flipped to Completed as a side effect of a rejected
        // completion attempt.
        // -----------------------------

        var user = new User
        {
            Id = 50,
            FirstName = "Jane",
            LastName = "Diner",
            Email = "jane@test.com",
            PasswordHash = "Hash",
            PhoneNumber = "0400111222",
            RoleId = 1
        };

        var restaurant = new Restaurant
        {
            Name = "Spice Kitchen",
            IsActive = true
        };

        _context.Users.Add(user);
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

        var reservation = new Reservation
        {
            DealId = deal.Id,
            UserId = user.Id,
            CustomerName = "Jane Diner",
            PhoneNumber = "0400111222",
            Email = "jane@test.com",
            ReservationDate = DateOnly.FromDateTime(DateTime.Today),
            ReservationTime = new TimeOnly(12, 30),
            GuestCount = 2,
            Status = ReservationStatus.Cancelled
        };

        _context.Reservations.Add(reservation);
        await _context.SaveChangesAsync();

        var redemption = new Redemption
        {
            ReservationId = reservation.Id,
            DealId = deal.Id,
            UserId = user.Id,
            ArrivalDate = reservation.ReservationDate,
            ArrivalTime = reservation.ReservationTime,
            Status = RedemptionStatus.Redeemed
        };

        _context.Redemptions.Add(redemption);
        await _context.SaveChangesAsync();

        var dto = new CompleteRedemptionDto
        {
            BillAmount = 100m
        };

        // -----------------------------
        // Act & Assert
        // -----------------------------

        await Assert.ThrowsExceptionAsync<BusinessRuleException>(
            () => _service.CompleteRedemptionAsync(redemption.Id, dto));

        _context.Reservations.First().Status.Should().Be(ReservationStatus.Cancelled);
    }

    [TestMethod]
    public async Task CompleteRedemptionAsync_ShouldSucceed_WhenLinkedReservationIsPending()
    {
        // -----------------------------
        // Arrange
        // Purpose:
        // Active MVP lifecycle: Pending -> Completed is the only
        // active completion transition for a reservation-linked
        // redemption.
        // -----------------------------

        var user = new User
        {
            Id = 50,
            FirstName = "Jane",
            LastName = "Diner",
            Email = "jane@test.com",
            PasswordHash = "Hash",
            PhoneNumber = "0400111222",
            RoleId = 1
        };

        var restaurant = new Restaurant
        {
            Name = "Spice Kitchen",
            IsActive = true
        };

        _context.Users.Add(user);
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

        var reservation = new Reservation
        {
            DealId = deal.Id,
            UserId = user.Id,
            CustomerName = "Jane Diner",
            PhoneNumber = "0400111222",
            Email = "jane@test.com",
            ReservationDate = DateOnly.FromDateTime(DateTime.Today),
            ReservationTime = new TimeOnly(12, 30),
            GuestCount = 2,
            Status = ReservationStatus.Pending
        };

        _context.Reservations.Add(reservation);
        await _context.SaveChangesAsync();

        var redemption = new Redemption
        {
            ReservationId = reservation.Id,
            DealId = deal.Id,
            UserId = user.Id,
            ArrivalDate = reservation.ReservationDate,
            ArrivalTime = reservation.ReservationTime,
            Status = RedemptionStatus.Redeemed
        };

        _context.Redemptions.Add(redemption);
        await _context.SaveChangesAsync();

        var dto = new CompleteRedemptionDto
        {
            BillAmount = 100m
        };

        // -----------------------------
        // Act
        // -----------------------------

        var result = await _service.CompleteRedemptionAsync(redemption.Id, dto);

        // -----------------------------
        // Assert
        // -----------------------------

        result.Should().NotBeNull();

        _context.Redemptions.First().Status.Should().Be(RedemptionStatus.Completed);
        _context.Redemptions.First().BillAmount.Should().Be(100m);
        _context.Redemptions.First().DiscountAmount.Should().Be(20m);
        _context.Redemptions.First().FinalAmount.Should().Be(80m);
        _context.Reservations.First().Status.Should().Be(ReservationStatus.Completed);
    }

    [TestMethod]
    public async Task CompleteRedemptionAsync_ShouldRejectWhenLinkedReservationIsNoShow()
    {
        var user = new User
        {
            Id = 50,
            FirstName = "Jane",
            LastName = "Diner",
            Email = "jane@test.com",
            PasswordHash = "Hash",
            PhoneNumber = "0400111222",
            RoleId = 1
        };

        var restaurant = new Restaurant
        {
            Name = "Spice Kitchen",
            IsActive = true
        };

        _context.Users.Add(user);
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

        var reservation = new Reservation
        {
            DealId = deal.Id,
            UserId = user.Id,
            CustomerName = "Jane Diner",
            PhoneNumber = "0400111222",
            Email = "jane@test.com",
            ReservationDate = DateOnly.FromDateTime(DateTime.Today),
            ReservationTime = new TimeOnly(12, 30),
            GuestCount = 2,
            Status = ReservationStatus.NoShow
        };

        _context.Reservations.Add(reservation);
        await _context.SaveChangesAsync();

        var redemption = new Redemption
        {
            ReservationId = reservation.Id,
            DealId = deal.Id,
            UserId = user.Id,
            ArrivalDate = reservation.ReservationDate,
            ArrivalTime = reservation.ReservationTime,
            Status = RedemptionStatus.Redeemed
        };

        _context.Redemptions.Add(redemption);
        await _context.SaveChangesAsync();

        var dto = new CompleteRedemptionDto
        {
            BillAmount = 100m
        };

        var ex = await Assert.ThrowsExceptionAsync<BusinessRuleException>(
            () => _service.CompleteRedemptionAsync(redemption.Id, dto));

        ex.Message.Should().Be("This redemption cannot be completed because the reservation is not pending.");
        _context.Redemptions.First().Status.Should().Be(RedemptionStatus.Redeemed);
    }

    [TestMethod]
    public async Task CompleteRedemptionAsync_ShouldRejectWhenLinkedReservationIsRejected()
    {
        var user = new User
        {
            Id = 50,
            FirstName = "Jane",
            LastName = "Diner",
            Email = "jane@test.com",
            PasswordHash = "Hash",
            PhoneNumber = "0400111222",
            RoleId = 1
        };

        var restaurant = new Restaurant
        {
            Name = "Spice Kitchen",
            IsActive = true
        };

        _context.Users.Add(user);
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

        var reservation = new Reservation
        {
            DealId = deal.Id,
            UserId = user.Id,
            CustomerName = "Jane Diner",
            PhoneNumber = "0400111222",
            Email = "jane@test.com",
            ReservationDate = DateOnly.FromDateTime(DateTime.Today),
            ReservationTime = new TimeOnly(12, 30),
            GuestCount = 2,
            Status = ReservationStatus.Rejected
        };

        _context.Reservations.Add(reservation);
        await _context.SaveChangesAsync();

        var redemption = new Redemption
        {
            ReservationId = reservation.Id,
            DealId = deal.Id,
            UserId = user.Id,
            ArrivalDate = reservation.ReservationDate,
            ArrivalTime = reservation.ReservationTime,
            Status = RedemptionStatus.Redeemed
        };

        _context.Redemptions.Add(redemption);
        await _context.SaveChangesAsync();

        var dto = new CompleteRedemptionDto
        {
            BillAmount = 100m
        };

        var ex = await Assert.ThrowsExceptionAsync<BusinessRuleException>(
            () => _service.CompleteRedemptionAsync(redemption.Id, dto));

        ex.Message.Should().Be("This redemption cannot be completed because the reservation is not pending.");
        _context.Redemptions.First().Status.Should().Be(RedemptionStatus.Redeemed);
    }

    [TestMethod]
    public async Task CompleteRedemptionAsync_ShouldRejectWhenLinkedReservationIsConfirmed()
    {
        var user = new User
        {
            Id = 50,
            FirstName = "Jane",
            LastName = "Diner",
            Email = "jane@test.com",
            PasswordHash = "Hash",
            PhoneNumber = "0400111222",
            RoleId = 1
        };

        var restaurant = new Restaurant
        {
            Name = "Spice Kitchen",
            IsActive = true
        };

        _context.Users.Add(user);
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

        var reservation = new Reservation
        {
            DealId = deal.Id,
            UserId = user.Id,
            CustomerName = "Jane Diner",
            PhoneNumber = "0400111222",
            Email = "jane@test.com",
            ReservationDate = DateOnly.FromDateTime(DateTime.Today),
            ReservationTime = new TimeOnly(12, 30),
            GuestCount = 2,
            Status = ReservationStatus.Confirmed
        };

        _context.Reservations.Add(reservation);
        await _context.SaveChangesAsync();

        var redemption = new Redemption
        {
            ReservationId = reservation.Id,
            DealId = deal.Id,
            UserId = user.Id,
            ArrivalDate = reservation.ReservationDate,
            ArrivalTime = reservation.ReservationTime,
            Status = RedemptionStatus.Redeemed
        };

        _context.Redemptions.Add(redemption);
        await _context.SaveChangesAsync();

        var dto = new CompleteRedemptionDto
        {
            BillAmount = 100m
        };

        var ex = await Assert.ThrowsExceptionAsync<BusinessRuleException>(
            () => _service.CompleteRedemptionAsync(redemption.Id, dto));

        ex.Message.Should().Be("This redemption cannot be completed because the reservation is not pending.");
        _context.Redemptions.First().Status.Should().Be(RedemptionStatus.Redeemed);
    }

    [TestMethod]
    public async Task CompleteRedemptionAsync_ShouldRejectWhenLinkedReservationIsArrived()
    {
        var user = new User
        {
            Id = 50,
            FirstName = "Jane",
            LastName = "Diner",
            Email = "jane@test.com",
            PasswordHash = "Hash",
            PhoneNumber = "0400111222",
            RoleId = 1
        };

        var restaurant = new Restaurant
        {
            Name = "Spice Kitchen",
            IsActive = true
        };

        _context.Users.Add(user);
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

        var reservation = new Reservation
        {
            DealId = deal.Id,
            UserId = user.Id,
            CustomerName = "Jane Diner",
            PhoneNumber = "0400111222",
            Email = "jane@test.com",
            ReservationDate = DateOnly.FromDateTime(DateTime.Today),
            ReservationTime = new TimeOnly(12, 30),
            GuestCount = 2,
            Status = ReservationStatus.Arrived
        };

        _context.Reservations.Add(reservation);
        await _context.SaveChangesAsync();

        var redemption = new Redemption
        {
            ReservationId = reservation.Id,
            DealId = deal.Id,
            UserId = user.Id,
            ArrivalDate = reservation.ReservationDate,
            ArrivalTime = reservation.ReservationTime,
            Status = RedemptionStatus.Redeemed
        };

        _context.Redemptions.Add(redemption);
        await _context.SaveChangesAsync();

        var dto = new CompleteRedemptionDto
        {
            BillAmount = 100m
        };

        var ex = await Assert.ThrowsExceptionAsync<BusinessRuleException>(
            () => _service.CompleteRedemptionAsync(redemption.Id, dto));

        ex.Message.Should().Be("This redemption cannot be completed because the reservation is not pending.");
        _context.Redemptions.First().Status.Should().Be(RedemptionStatus.Redeemed);
    }

    [TestMethod]
    public async Task CompleteRedemptionAsync_ShouldStillWorkForLegacyRedemptionWithNoReservationId()
    {
        // -----------------------------
        // Arrange
        // Purpose:
        // A pre-existing/legacy redemption with no ReservationId (e.g.
        // seed data, or a row created before the link existed) must
        // still complete normally - ReservationId == null must not
        // become a hard requirement, and no reservation is guessed at.
        // -----------------------------

        var user = new User
        {
            Id = 50,
            FirstName = "Jane",
            LastName = "Diner",
            Email = "jane@test.com",
            PasswordHash = "Hash",
            PhoneNumber = "0400111222",
            RoleId = 1
        };

        var restaurant = new Restaurant
        {
            Name = "Spice Kitchen",
            IsActive = true
        };

        _context.Users.Add(user);
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

        var redemption = new Redemption
        {
            ReservationId = null,
            DealId = deal.Id,
            UserId = user.Id,
            ArrivalDate = DateOnly.FromDateTime(DateTime.Today),
            ArrivalTime = new TimeOnly(12, 30),
            Status = RedemptionStatus.Redeemed
        };

        _context.Redemptions.Add(redemption);
        await _context.SaveChangesAsync();

        var dto = new CompleteRedemptionDto
        {
            BillAmount = 100m
        };

        // -----------------------------
        // Act
        // -----------------------------

        var result = await _service.CompleteRedemptionAsync(redemption.Id, dto);

        // -----------------------------
        // Assert
        // -----------------------------

        result.Should().NotBeNull();

        var saved = _context.Redemptions.First();

        saved.Status.Should().Be(RedemptionStatus.Completed);
        saved.BillAmount.Should().Be(100m);
    }

    [TestMethod]
    public async Task CompleteRedemptionAsync_ShouldNotMatchUnrelatedReservationByFourFieldFallback_WhenReservationIdIsNull()
    {
        // -----------------------------
        // Arrange
        // Purpose:
        // Phase 5P removed the legacy four-field (UserId/DealId/
        // ArrivalDate/ArrivalTime) fallback match. A redemption with
        // ReservationId == null must complete without being blocked by
        // - or altering the status of - an unrelated reservation that
        // merely happens to share those four values.
        // -----------------------------

        var user = new User
        {
            Id = 50,
            FirstName = "Jane",
            LastName = "Diner",
            Email = "jane@test.com",
            PasswordHash = "Hash",
            PhoneNumber = "0400111222",
            RoleId = 1
        };

        var restaurant = new Restaurant
        {
            Name = "Spice Kitchen",
            IsActive = true
        };

        _context.Users.Add(user);
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

        var arrivalDate = DateOnly.FromDateTime(DateTime.Today);
        var arrivalTime = new TimeOnly(12, 30);

        // An unrelated, Cancelled reservation that shares the same
        // UserId/DealId/date/time as the redemption below, but has no
        // ReservationId link to it. Under the old four-field fallback,
        // this would have wrongly blocked completion (Cancelled match)
        // and could have been silently flipped to Completed.
        var unrelatedReservation = new Reservation
        {
            DealId = deal.Id,
            UserId = user.Id,
            CustomerName = "Jane Diner",
            PhoneNumber = "0400111222",
            Email = "jane@test.com",
            ReservationDate = arrivalDate,
            ReservationTime = arrivalTime,
            GuestCount = 2,
            Status = ReservationStatus.Cancelled
        };

        _context.Reservations.Add(unrelatedReservation);
        await _context.SaveChangesAsync();

        var redemption = new Redemption
        {
            ReservationId = null,
            DealId = deal.Id,
            UserId = user.Id,
            ArrivalDate = arrivalDate,
            ArrivalTime = arrivalTime,
            Status = RedemptionStatus.Redeemed
        };

        _context.Redemptions.Add(redemption);
        await _context.SaveChangesAsync();

        var dto = new CompleteRedemptionDto
        {
            BillAmount = 100m
        };

        // -----------------------------
        // Act
        // -----------------------------

        var result = await _service.CompleteRedemptionAsync(redemption.Id, dto);

        // -----------------------------
        // Assert
        // -----------------------------

        result.Should().NotBeNull();

        _context.Redemptions.First().Status.Should().Be(RedemptionStatus.Completed);

        // The unrelated reservation must be left exactly as it was -
        // no association was ever made with it.
        _context.Reservations.First().Status.Should().Be(ReservationStatus.Cancelled);
    }


    // -----------------------------
    // GetRestaurantRedemptionsAsync - ownership rules
    // -----------------------------

    [TestMethod]
    public async Task GetRestaurantRedemptionsAsync_ShouldReturnRedemptions_WhenOwnerOwnsRestaurant()
    {
        // -----------------------------
        // Arrange
        // -----------------------------

        _currentUser.Setup(x => x.IsAdmin).Returns(false);
        _currentUser.Setup(x => x.UserId).Returns(1);

        var user = new User
        {
            Id = 50,
            FirstName = "John",
            LastName = "Smith",
            Email = "john@test.com",
            PasswordHash = "Hash",
            PhoneNumber = "0400000000",
            RoleId = 1
        };

        var restaurant = new Restaurant
        {
            Name = "Spice Kitchen",
            OwnerId = 1,
            IsActive = true
        };

        _context.Users.Add(user);
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
            UserId = user.Id,
            Status = RedemptionStatus.Redeemed
        });

        await _context.SaveChangesAsync();

        // -----------------------------
        // Act
        // -----------------------------

        var result = await _service.GetRestaurantRedemptionsAsync(restaurant.Id);

        // -----------------------------
        // Assert
        // -----------------------------

        result.Should().HaveCount(1);
    }

    [TestMethod]
    public async Task GetRestaurantRedemptionsAsync_ShouldThrowBusinessRuleException_WhenOwnerDoesNotOwnRestaurant()
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
            () => _service.GetRestaurantRedemptionsAsync(restaurant.Id));

        ex.Message.Should().Be("You are not authorized to view redemptions for this restaurant.");
    }

    [TestMethod]
    public async Task GetRestaurantRedemptionsAsync_ShouldReturnRedemptions_WhenUserIsAdmin_EvenIfNotOwner()
    {
        // -----------------------------
        // Arrange
        // -----------------------------

        _currentUser.Setup(x => x.IsAdmin).Returns(true);
        _currentUser.Setup(x => x.UserId).Returns(999);

        var user = new User
        {
            Id = 50,
            FirstName = "John",
            LastName = "Smith",
            Email = "john@test.com",
            PasswordHash = "Hash",
            PhoneNumber = "0400000000",
            RoleId = 1
        };

        var restaurant = new Restaurant
        {
            Name = "Spice Kitchen",
            OwnerId = 2,
            IsActive = true
        };

        _context.Users.Add(user);
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
            UserId = user.Id,
            Status = RedemptionStatus.Redeemed
        });

        await _context.SaveChangesAsync();

        // -----------------------------
        // Act
        // -----------------------------

        var result = await _service.GetRestaurantRedemptionsAsync(restaurant.Id);

        // -----------------------------
        // Assert
        // -----------------------------

        result.Should().HaveCount(1);
    }

    [TestMethod]
    public async Task GetRestaurantRedemptionsAsync_ShouldReturnEmptyList_WhenRestaurantDoesNotExist()
    {
        // -----------------------------
        // Arrange
        // Purpose:
        // Preserve the existing behavior for a
        // nonexistent restaurantId - an empty list,
        // not an exception, regardless of caller role.
        // -----------------------------

        _currentUser.Setup(x => x.IsAdmin).Returns(false);
        _currentUser.Setup(x => x.UserId).Returns(1);

        // -----------------------------
        // Act
        // -----------------------------

        var result = await _service.GetRestaurantRedemptionsAsync(9999);

        // -----------------------------
        // Assert
        // -----------------------------

        result.Should().BeEmpty();
    }


    // -----------------------------
    // GetByIdAsync - authorization rules
    //
    // Unauthorized access returns null (not an
    // exception), so it is indistinguishable from
    // a nonexistent redemption.
    // -----------------------------

    private async Task<Redemption> SeedRedemptionAsync(
        int customerUserId,
        int restaurantOwnerId)
    {
        var user = new User
        {
            Id = customerUserId,
            FirstName = "Jane",
            LastName = "Diner",
            Email = $"customer{customerUserId}@test.com",
            PasswordHash = "Hash",
            PhoneNumber = "0400000000",
            RoleId = 1
        };

        var restaurant = new Restaurant
        {
            Name = "Spice Kitchen",
            OwnerId = restaurantOwnerId,
            IsActive = true
        };

        _context.Users.Add(user);
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

        var redemption = new Redemption
        {
            DealId = deal.Id,
            UserId = customerUserId,
            Status = RedemptionStatus.Redeemed
        };

        _context.Redemptions.Add(redemption);
        await _context.SaveChangesAsync();

        return redemption;
    }

    [TestMethod]
    public async Task GetByIdAsync_ShouldReturnRedemption_WhenCustomerOwnsIt()
    {
        // -----------------------------
        // Arrange
        // -----------------------------

        _currentUser.Setup(x => x.IsAdmin).Returns(false);
        _currentUser.Setup(x => x.UserId).Returns(10);

        var redemption = await SeedRedemptionAsync(
            customerUserId: 10,
            restaurantOwnerId: 99);

        // -----------------------------
        // Act
        // -----------------------------

        var result = await _service.GetByIdAsync(redemption.Id);

        // -----------------------------
        // Assert
        // -----------------------------

        result.Should().NotBeNull();
        result!.Id.Should().Be(redemption.Id);
    }

    [TestMethod]
    public async Task GetByIdAsync_ShouldReturnNull_WhenAnotherCustomerOwnsIt()
    {
        // -----------------------------
        // Arrange
        // -----------------------------

        _currentUser.Setup(x => x.IsAdmin).Returns(false);
        _currentUser.Setup(x => x.UserId).Returns(10);

        var redemption = await SeedRedemptionAsync(
            customerUserId: 20,
            restaurantOwnerId: 99);

        // -----------------------------
        // Act
        // -----------------------------

        var result = await _service.GetByIdAsync(redemption.Id);

        // -----------------------------
        // Assert
        // -----------------------------

        result.Should().BeNull();
    }

    [TestMethod]
    public async Task GetByIdAsync_ShouldReturnRedemption_WhenOwnerOwnsRestaurant()
    {
        // -----------------------------
        // Arrange
        // -----------------------------

        _currentUser.Setup(x => x.IsAdmin).Returns(false);
        _currentUser.Setup(x => x.UserId).Returns(1);

        var redemption = await SeedRedemptionAsync(
            customerUserId: 50,
            restaurantOwnerId: 1);

        // -----------------------------
        // Act
        // -----------------------------

        var result = await _service.GetByIdAsync(redemption.Id);

        // -----------------------------
        // Assert
        // -----------------------------

        result.Should().NotBeNull();
        result!.Id.Should().Be(redemption.Id);
    }

    [TestMethod]
    public async Task GetByIdAsync_ShouldReturnNull_WhenOwnerDoesNotOwnRestaurant()
    {
        // -----------------------------
        // Arrange
        // -----------------------------

        _currentUser.Setup(x => x.IsAdmin).Returns(false);
        _currentUser.Setup(x => x.UserId).Returns(1);

        var redemption = await SeedRedemptionAsync(
            customerUserId: 50,
            restaurantOwnerId: 2);

        // -----------------------------
        // Act
        // -----------------------------

        var result = await _service.GetByIdAsync(redemption.Id);

        // -----------------------------
        // Assert
        // -----------------------------

        result.Should().BeNull();
    }

    [TestMethod]
    public async Task GetByIdAsync_ShouldReturnRedemption_WhenUserIsAdmin()
    {
        // -----------------------------
        // Arrange
        // -----------------------------

        _currentUser.Setup(x => x.IsAdmin).Returns(true);
        _currentUser.Setup(x => x.UserId).Returns(999);

        var redemption = await SeedRedemptionAsync(
            customerUserId: 50,
            restaurantOwnerId: 2);

        // -----------------------------
        // Act
        // -----------------------------

        var result = await _service.GetByIdAsync(redemption.Id);

        // -----------------------------
        // Assert
        // -----------------------------

        result.Should().NotBeNull();
        result!.Id.Should().Be(redemption.Id);
    }

    [TestMethod]
    public async Task GetByIdAsync_ShouldReturnNull_WhenRedemptionDoesNotExist()
    {
        // -----------------------------
        // Arrange
        // Purpose: preserve existing not-found behavior.
        // -----------------------------

        // -----------------------------
        // Act
        // -----------------------------

        var result = await _service.GetByIdAsync(9999);

        // -----------------------------
        // Assert
        // -----------------------------

        result.Should().BeNull();
    }


    // -----------------------------
    // Currency resolution (restaurant-level CurrencyCode)
    // -----------------------------

    [TestMethod]
    public async Task GetMyHistoryAsync_ShouldResolveCurrencyCodeFromTheOwningRestaurant()
    {
        // -----------------------------
        // Arrange
        // Purpose:
        // Redemption carries no currency field of its own - confirm
        // GetMyHistoryAsync (used by MyRedemptionsPage) resolves it
        // correctly via Deal -> Restaurant.
        // -----------------------------

        var user = new User
        {
            Id = 50,
            FirstName = "Jane",
            LastName = "Diner",
            Email = "jane@test.com",
            PasswordHash = "Hash",
            PhoneNumber = "0400111222",
            RoleId = 1
        };

        var restaurant = new Restaurant
        {
            Name = "Spice Kitchen",
            CurrencyCode = "AUD",
            IsActive = true
        };

        _context.Users.Add(user);
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
            UserId = user.Id,
            Status = RedemptionStatus.Redeemed
        });

        await _context.SaveChangesAsync();

        _currentUser.Setup(x => x.UserId).Returns(user.Id);

        // -----------------------------
        // Act
        // -----------------------------

        var result = await _service.GetMyHistoryAsync();

        // -----------------------------
        // Assert
        // -----------------------------

        result.Should().ContainSingle()
            .Which.CurrencyCode.Should().Be("AUD");
    }

    [TestMethod]
    public async Task GetRestaurantRedemptionsAsync_ShouldResolveCurrencyCodeFromTheOwningRestaurant()
    {
        // -----------------------------
        // Arrange
        // -----------------------------

        _currentUser.Setup(x => x.IsAdmin).Returns(true);

        var user = new User
        {
            Id = 50,
            FirstName = "Jane",
            LastName = "Diner",
            Email = "jane@test.com",
            PasswordHash = "Hash",
            PhoneNumber = "0400111222",
            RoleId = 1
        };

        var restaurant = new Restaurant
        {
            Name = "Spice Kitchen",
            CurrencyCode = "GBP",
            IsActive = true
        };

        _context.Users.Add(user);
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
            UserId = user.Id,
            Status = RedemptionStatus.Redeemed
        });

        await _context.SaveChangesAsync();

        // -----------------------------
        // Act
        // -----------------------------

        var result = await _service.GetRestaurantRedemptionsAsync(restaurant.Id);

        // -----------------------------
        // Assert
        // -----------------------------

        result.Should().ContainSingle()
            .Which.CurrencyCode.Should().Be("GBP");
    }


    // =========================================================
    // RedeemAsync - walk-in offer claims (no Reservation)
    //
    // Clock is fixed at 10 Nov 2026 10:00. The seeded deal runs
    // 1-30 Nov 2026, arrival window 12:00-14:00, max 4 guests.
    // =========================================================

    private const int CustomerId = 1;
    private const int OwnerId = 99;

    private async Task<Deal> SeedWalkInDealAsync(
        int reservationLimit = 0,
        int dailyRedemptionLimit = 0,
        bool isActive = true,
        decimal discountPercentage = 25)
    {
        if (!_context.Users.Any(u => u.Id == CustomerId))
        {
            _context.Users.Add(new User
            {
                Id = CustomerId,
                FirstName = "Jane",
                LastName = "Diner",
                Email = "jane@test.com",
                PasswordHash = "Hash",
                PhoneNumber = "0400111222",
                RoleId = 3
            });
        }

        var restaurant = new Restaurant
        {
            Name = "Momo House",
            OwnerId = OwnerId,
            CurrencyCode = "AUD",
            IsActive = true
        };

        _context.Restaurants.Add(restaurant);
        await _context.SaveChangesAsync();

        var deal = new Deal
        {
            RestaurantId = restaurant.Id,
            Title = "25% Off - Dine In",
            DiscountPercentage = discountPercentage,
            OfferType = OfferType.DineIn,
            StartDate = new DateOnly(2026, 11, 1),
            EndDate = new DateOnly(2026, 11, 30),
            StartTime = new TimeOnly(12, 0),
            EndTime = new TimeOnly(14, 0),
            MaximumGuests = 4,
            ReservationLimit = reservationLimit,
            DailyRedemptionLimit = dailyRedemptionLimit,
            IsActive = isActive
        };

        _context.Deals.Add(deal);
        await _context.SaveChangesAsync();

        return deal;
    }

    private static CreateRedemptionDto Claim(
        Deal deal,
        DateOnly? date = null,
        TimeOnly? time = null,
        int guests = 2)
    {
        return new CreateRedemptionDto
        {
            DealId = deal.Id,
            ArrivalDate = date ?? Today,
            ArrivalTime = time ?? new TimeOnly(12, 30),
            GuestCount = guests
        };
    }

    // Adds a claim made by some other customer.
    private async Task SeedOtherClaimAsync(
        Deal deal,
        DateOnly date,
        RedemptionStatus status = RedemptionStatus.Redeemed)
    {
        var otherUserId = 1000 + _context.Users.Count();

        _context.Users.Add(new User
        {
            Id = otherUserId,
            FirstName = "Other",
            LastName = "Customer",
            Email = $"other{otherUserId}@test.com",
            PasswordHash = "Hash",
            RoleId = 3
        });

        _context.Redemptions.Add(new Redemption
        {
            DealId = deal.Id,
            UserId = otherUserId,
            ArrivalDate = date,
            ArrivalTime = new TimeOnly(12, 0),
            GuestCount = 2,
            Status = status
        });

        await _context.SaveChangesAsync();
    }

    [TestMethod]
    public async Task RedeemAsync_ShouldCreateRedemptionWithoutReservation_UsingAccountDetails()
    {
        // Arrange
        var deal = await SeedWalkInDealAsync();

        // Act
        var result = await _service.RedeemAsync(Claim(deal, guests: 3));

        // Assert
        var saved = _context.Redemptions.Single();

        saved.UserId.Should().Be(CustomerId);
        saved.ReservationId.Should().BeNull();
        saved.Status.Should().Be(RedemptionStatus.Redeemed);
        saved.ArrivalDate.Should().Be(Today);
        saved.ArrivalTime.Should().Be(new TimeOnly(12, 30));
        saved.GuestCount.Should().Be(3);
        saved.BillAmount.Should().BeNull();

        _context.Reservations.Should().BeEmpty();

        result.CustomerName.Should().Be("Jane Diner");
        result.CustomerPhone.Should().Be("0400111222");
        result.CustomerEmail.Should().Be("jane@test.com");
        result.RestaurantName.Should().Be("Momo House");
        result.CurrencyCode.Should().Be("AUD");
        result.ReservationId.Should().BeNull();
    }

    [TestMethod]
    public async Task RedeemAsync_ShouldThrowNotFound_WhenDealDoesNotExist()
    {
        var ex = await Assert.ThrowsExceptionAsync<NotFoundException>(
            () => _service.RedeemAsync(new CreateRedemptionDto
            {
                DealId = 12345,
                ArrivalDate = Today,
                ArrivalTime = new TimeOnly(12, 30),
                GuestCount = 2
            }));

        ex.Message.Should().Be("Deal not found.");
    }

    [TestMethod]
    public async Task RedeemAsync_ShouldReject_WhenDealIsInactive()
    {
        var deal = await SeedWalkInDealAsync(isActive: false);

        var ex = await Assert.ThrowsExceptionAsync<BusinessRuleException>(
            () => _service.RedeemAsync(Claim(deal)));

        ex.Message.Should().Be("Offer is inactive.");
        _context.Redemptions.Should().BeEmpty();
    }

    [TestMethod]
    public async Task RedeemAsync_ShouldReject_WhenArrivalDateIsInThePast()
    {
        // 5 Nov is inside the deal dates but before "today" (10 Nov).
        var deal = await SeedWalkInDealAsync();

        var ex = await Assert.ThrowsExceptionAsync<BusinessRuleException>(
            () => _service.RedeemAsync(Claim(deal, date: new DateOnly(2026, 11, 5))));

        ex.Message.Should().Be("Arrival date cannot be in the past.");
    }

    [TestMethod]
    public async Task RedeemAsync_ShouldReject_WhenArrivalTimeTodayHasPassed()
    {
        var deal = await SeedWalkInDealAsync();

        _clock.Now = new DateTimeOffset(2026, 11, 10, 13, 0, 0, TimeSpan.Zero);

        var ex = await Assert.ThrowsExceptionAsync<BusinessRuleException>(
            () => _service.RedeemAsync(Claim(deal, time: new TimeOnly(12, 30))));

        ex.Message.Should().Be("The selected arrival time has already passed.");
    }

    [TestMethod]
    public async Task RedeemAsync_ShouldAllow_LaterArrivalTimeToday()
    {
        var deal = await SeedWalkInDealAsync();

        _clock.Now = new DateTimeOffset(2026, 11, 10, 13, 0, 0, TimeSpan.Zero);

        await _service.RedeemAsync(Claim(deal, time: new TimeOnly(13, 30)));

        _context.Redemptions.Should().ContainSingle();
    }

    [TestMethod]
    public async Task RedeemAsync_ShouldReject_WhenArrivalDateIsOutsideDealDates()
    {
        var deal = await SeedWalkInDealAsync();

        var ex = await Assert.ThrowsExceptionAsync<BusinessRuleException>(
            () => _service.RedeemAsync(Claim(deal, date: new DateOnly(2026, 12, 1))));

        ex.Message.Should().Be("Offer is not available on the selected arrival date.");
    }

    [TestMethod]
    public async Task RedeemAsync_ShouldReject_WhenArrivalTimeIsOutsideWindow()
    {
        var deal = await SeedWalkInDealAsync();

        var ex = await Assert.ThrowsExceptionAsync<BusinessRuleException>(
            () => _service.RedeemAsync(Claim(deal, time: new TimeOnly(14, 15))));

        ex.Message.Should().Be("Arrival time must be within the offer time.");
    }

    [TestMethod]
    public async Task RedeemAsync_ShouldReject_WhenGuestsExceedDealMaximum()
    {
        var deal = await SeedWalkInDealAsync();

        var ex = await Assert.ThrowsExceptionAsync<BusinessRuleException>(
            () => _service.RedeemAsync(Claim(deal, guests: 5)));

        ex.Message.Should().Be("Maximum 4 guests allowed.");
    }

    [TestMethod]
    public async Task RedeemAsync_ShouldReject_WhenGuestCountIsZero()
    {
        var deal = await SeedWalkInDealAsync();

        var ex = await Assert.ThrowsExceptionAsync<BusinessRuleException>(
            () => _service.RedeemAsync(Claim(deal, guests: 0)));

        ex.Message.Should().Be("At least 1 guest is required.");
    }

    [TestMethod]
    public async Task RedeemAsync_ShouldReject_DuplicateActiveClaimForSameDealAndDate()
    {
        var deal = await SeedWalkInDealAsync();

        await _service.RedeemAsync(Claim(deal, time: new TimeOnly(12, 0)));

        // A different arrival time on the same date is still a duplicate.
        var ex = await Assert.ThrowsExceptionAsync<BusinessRuleException>(
            () => _service.RedeemAsync(Claim(deal, time: new TimeOnly(13, 0))));

        ex.Message.Should().Be("You have already redeemed this offer for the selected date.");
        _context.Redemptions.Should().ContainSingle();
    }

    [TestMethod]
    public async Task RedeemAsync_ShouldReject_WhenCompletedClaimExistsForSameDate()
    {
        var deal = await SeedWalkInDealAsync();

        _context.Redemptions.Add(new Redemption
        {
            DealId = deal.Id,
            UserId = CustomerId,
            ArrivalDate = Today,
            ArrivalTime = new TimeOnly(12, 0),
            GuestCount = 2,
            Status = RedemptionStatus.Completed
        });
        await _context.SaveChangesAsync();

        await Assert.ThrowsExceptionAsync<BusinessRuleException>(
            () => _service.RedeemAsync(Claim(deal)));
    }

    [TestMethod]
    public async Task RedeemAsync_ShouldAllow_SameDealOnDifferentDate()
    {
        var deal = await SeedWalkInDealAsync();

        await _service.RedeemAsync(Claim(deal, date: Today));
        await _service.RedeemAsync(Claim(deal, date: Today.AddDays(1)));

        _context.Redemptions.Should().HaveCount(2);
    }

    [TestMethod]
    public async Task RedeemAsync_ShouldAllow_NewClaimAfterPreviousClaimWasCancelled()
    {
        var deal = await SeedWalkInDealAsync();

        var first = await _service.RedeemAsync(Claim(deal));
        await _service.CancelMyRedemptionAsync(first.Id);

        await _service.RedeemAsync(Claim(deal));

        _context.Redemptions.Count(r => r.Status == RedemptionStatus.Redeemed)
            .Should().Be(1);
    }

    [TestMethod]
    public async Task RedeemAsync_ShouldReject_WhenTotalLimitIsReached()
    {
        // Total cap 2: one Redeemed + one Completed claim use it up;
        // the Cancelled claim does not count.
        var deal = await SeedWalkInDealAsync(reservationLimit: 2);

        await SeedOtherClaimAsync(deal, Today.AddDays(1), RedemptionStatus.Redeemed);
        await SeedOtherClaimAsync(deal, Today.AddDays(2), RedemptionStatus.Completed);
        await SeedOtherClaimAsync(deal, Today.AddDays(3), RedemptionStatus.Cancelled);

        var ex = await Assert.ThrowsExceptionAsync<BusinessRuleException>(
            () => _service.RedeemAsync(Claim(deal)));

        ex.Message.Should().Be("This offer has been fully redeemed.");
    }

    [TestMethod]
    public async Task RedeemAsync_ShouldAllow_WhenTotalLimitHasRemainingOffers()
    {
        var deal = await SeedWalkInDealAsync(reservationLimit: 2);

        await SeedOtherClaimAsync(deal, Today, RedemptionStatus.Redeemed);
        await SeedOtherClaimAsync(deal, Today, RedemptionStatus.Cancelled);

        await _service.RedeemAsync(Claim(deal));

        _context.Redemptions.Count(r => r.UserId == CustomerId).Should().Be(1);
    }

    [TestMethod]
    public async Task RedeemAsync_ShouldReject_WhenDailyLimitIsReached_ButAllowAnotherDate()
    {
        var deal = await SeedWalkInDealAsync(dailyRedemptionLimit: 1);

        await SeedOtherClaimAsync(deal, Today);

        var ex = await Assert.ThrowsExceptionAsync<BusinessRuleException>(
            () => _service.RedeemAsync(Claim(deal, date: Today)));

        ex.Message.Should().Be("No offers are left for the selected date.");

        await _service.RedeemAsync(Claim(deal, date: Today.AddDays(1)));

        _context.Redemptions.Count(r => r.UserId == CustomerId).Should().Be(1);
    }

    [TestMethod]
    public async Task RedeemAsync_ShouldNotLimitClaims_WhenLimitsAreZero()
    {
        var deal = await SeedWalkInDealAsync(reservationLimit: 0, dailyRedemptionLimit: 0);

        for (var i = 0; i < 5; i++)
            await SeedOtherClaimAsync(deal, Today);

        await _service.RedeemAsync(Claim(deal));

        _context.Redemptions.Should().HaveCount(6);
    }

    // -----------------------------
    // DealCapacityCalculator
    // -----------------------------

    [TestMethod]
    public async Task GetCapacityAsync_ShouldReturnLowerOfTotalAndDailyRemaining()
    {
        // Total 10 (3 used) -> 7 left; daily 4 (2 used today) -> 2 left.
        var deal = await SeedWalkInDealAsync(reservationLimit: 10, dailyRedemptionLimit: 4);

        await SeedOtherClaimAsync(deal, Today);
        await SeedOtherClaimAsync(deal, Today);
        await SeedOtherClaimAsync(deal, Today.AddDays(1));

        var capacity = await DealCapacityCalculator.GetCapacityAsync(_context, deal, Today);

        capacity.TotalRemaining.Should().Be(7);
        capacity.DailyRemaining.Should().Be(2);
        capacity.Remaining.Should().Be(2);
    }

    [TestMethod]
    public async Task GetCapacityAsync_ShouldReturnNull_WhenUnlimited()
    {
        var deal = await SeedWalkInDealAsync();

        var capacity = await DealCapacityCalculator.GetCapacityAsync(_context, deal, Today);

        capacity.Remaining.Should().BeNull();
    }

    // -----------------------------
    // Cancelling claims
    // -----------------------------

    [TestMethod]
    public async Task CancelMyRedemptionAsync_ShouldCancelOwnRedeemedClaim()
    {
        var deal = await SeedWalkInDealAsync();
        var claim = await _service.RedeemAsync(Claim(deal));

        var result = await _service.CancelMyRedemptionAsync(claim.Id);

        result.Status.Should().Be(RedemptionStatus.Cancelled);
        _context.Redemptions.Single().Status.Should().Be(RedemptionStatus.Cancelled);
    }

    [TestMethod]
    public async Task CancelMyRedemptionAsync_ShouldReject_WhenClaimBelongsToAnotherCustomer()
    {
        var deal = await SeedWalkInDealAsync();
        await SeedOtherClaimAsync(deal, Today);

        var otherClaim = _context.Redemptions.Single();

        var ex = await Assert.ThrowsExceptionAsync<BusinessRuleException>(
            () => _service.CancelMyRedemptionAsync(otherClaim.Id));

        ex.Message.Should().Be("You are not authorized to cancel this redemption.");
        otherClaim.Status.Should().Be(RedemptionStatus.Redeemed);
    }

    [TestMethod]
    public async Task CancelMyRedemptionAsync_ShouldReject_WhenClaimIsCompleted()
    {
        var deal = await SeedWalkInDealAsync();
        var claim = await _service.RedeemAsync(Claim(deal));

        await _service.CompleteRedemptionAsync(claim.Id, new CompleteRedemptionDto { BillAmount = 100m });

        var ex = await Assert.ThrowsExceptionAsync<BusinessRuleException>(
            () => _service.CancelMyRedemptionAsync(claim.Id));

        ex.Message.Should().Be("Only redeemed offers that have not been completed can be cancelled.");
    }

    [TestMethod]
    public async Task CancelMyRedemptionAsync_ShouldAlsoCancelPendingLinkedReservation()
    {
        var deal = await SeedWalkInDealAsync();

        var reservation = new Reservation
        {
            DealId = deal.Id,
            UserId = CustomerId,
            CustomerName = "Jane Diner",
            PhoneNumber = "0400111222",
            ReservationDate = Today,
            ReservationTime = new TimeOnly(12, 0),
            GuestCount = 2,
            Status = ReservationStatus.Pending
        };

        _context.Reservations.Add(reservation);
        await _context.SaveChangesAsync();

        var redemption = new Redemption
        {
            DealId = deal.Id,
            UserId = CustomerId,
            ReservationId = reservation.Id,
            ArrivalDate = Today,
            ArrivalTime = new TimeOnly(12, 0),
            GuestCount = 2,
            Status = RedemptionStatus.Redeemed
        };

        _context.Redemptions.Add(redemption);
        await _context.SaveChangesAsync();

        await _service.CancelMyRedemptionAsync(redemption.Id);

        redemption.Status.Should().Be(RedemptionStatus.Cancelled);
        reservation.Status.Should().Be(ReservationStatus.Cancelled);
    }

    [TestMethod]
    public async Task CancelRedemptionAsync_ShouldSucceed_WhenOwnerOwnsRestaurant()
    {
        var deal = await SeedWalkInDealAsync();
        var claim = await _service.RedeemAsync(Claim(deal));

        _currentUser.Setup(x => x.UserId).Returns(OwnerId);
        _currentUser.Setup(x => x.IsAdmin).Returns(false);

        var result = await _service.CancelRedemptionAsync(claim.Id);

        result.Status.Should().Be(RedemptionStatus.Cancelled);
    }

    [TestMethod]
    public async Task CancelRedemptionAsync_ShouldReject_WhenOwnerDoesNotOwnRestaurant()
    {
        var deal = await SeedWalkInDealAsync();
        var claim = await _service.RedeemAsync(Claim(deal));

        _currentUser.Setup(x => x.UserId).Returns(500);
        _currentUser.Setup(x => x.IsAdmin).Returns(false);

        var ex = await Assert.ThrowsExceptionAsync<BusinessRuleException>(
            () => _service.CancelRedemptionAsync(claim.Id));

        ex.Message.Should().Be("You are not authorized to cancel redemptions for this restaurant.");
    }

    // -----------------------------
    // Owner completes a walk-in claim
    // -----------------------------

    [TestMethod]
    public async Task CompleteRedemptionAsync_ShouldCompleteWalkInClaim_WithExistingDiscountCalculation()
    {
        var deal = await SeedWalkInDealAsync(discountPercentage: 25);
        var claim = await _service.RedeemAsync(Claim(deal));

        _currentUser.Setup(x => x.UserId).Returns(OwnerId);
        _currentUser.Setup(x => x.IsAdmin).Returns(false);

        var result = await _service.CompleteRedemptionAsync(
            claim.Id,
            new CompleteRedemptionDto { BillAmount = 80m });

        result.Status.Should().Be(RedemptionStatus.Completed);
        result.BillAmount.Should().Be(80m);
        result.DiscountAmount.Should().Be(20m);
        result.FinalAmount.Should().Be(60m);
        result.CurrencyCode.Should().Be("AUD");
    }

    [TestMethod]
    public async Task CompleteRedemptionAsync_ShouldReject_WhenClaimWasCancelled()
    {
        var deal = await SeedWalkInDealAsync();
        var claim = await _service.RedeemAsync(Claim(deal));
        await _service.CancelMyRedemptionAsync(claim.Id);

        var ex = await Assert.ThrowsExceptionAsync<BusinessRuleException>(
            () => _service.CompleteRedemptionAsync(
                claim.Id,
                new CompleteRedemptionDto { BillAmount = 80m }));

        ex.Message.Should().Be("This redemption has been cancelled or has expired and cannot be completed.");
    }
}
