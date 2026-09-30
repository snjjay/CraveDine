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

        _service = new RedemptionService(
            _context,
            _currentUser.Object,
            _mapper);
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
}