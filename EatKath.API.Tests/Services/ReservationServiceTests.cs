using AutoMapper;
using EatKath.API.Data;
using EatKath.API.DTOs.Reservation;
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
public class ReservationServiceTests
{
    private ApplicationDbContext _context = null!;
    private IMapper _mapper = null!;
    private Mock<ICurrentUserService> _currentUser = null!;
    private ReservationService _service = null!;

    [TestInitialize]
    public void Setup()
    {
        _context = TestDbContextFactory.Create();

        _mapper = MapperFactory.Create();

        _currentUser = new Mock<ICurrentUserService>();

        // Default: an Admin caller, so tests that don't
        // care about ownership continue to behave predictably.
        _currentUser.Setup(x => x.UserId).Returns(1);
        _currentUser.Setup(x => x.IsAdmin).Returns(true);

        _service = new ReservationService(
            _context,
            _mapper,
            _currentUser.Object);
    }

    // -----------------------------
    // Seeds a Restaurant (owned by restaurantOwnerId) -> Deal -> Reservation
    // graph, mirroring how a real reservation is structured
    // (Reservation has no direct RestaurantId; it's reached via Deal).
    // -----------------------------
    private async Task<Reservation> SeedReservationAsync(
        int restaurantOwnerId,
        string status = ReservationStatus.Pending,
        int customerUserId = 50)
    {
        var restaurant = new Restaurant
        {
            Name = "Spice Kitchen",
            OwnerId = restaurantOwnerId,
            IsActive = true
        };

        _context.Restaurants.Add(restaurant);
        await _context.SaveChangesAsync();

        var deal = new Deal
        {
            RestaurantId = restaurant.Id,
            Title = "20% Lunch",
            DiscountPercentage = 20,
            StartDate = DateOnly.FromDateTime(DateTime.Today),
            EndDate = DateOnly.FromDateTime(DateTime.Today.AddDays(30)),
            StartTime = new TimeOnly(12, 0),
            EndTime = new TimeOnly(15, 0),
            MaximumGuests = 4,
            DailyRedemptionLimit = 100,
            IsActive = true
        };

        _context.Deals.Add(deal);
        await _context.SaveChangesAsync();

        var reservation = new Reservation
        {
            DealId = deal.Id,
            UserId = customerUserId,
            CustomerName = "Jane Diner",
            PhoneNumber = "0400111222",
            Email = "jane@test.com",
            ReservationDate = DateOnly.FromDateTime(DateTime.Today),
            ReservationTime = new TimeOnly(12, 30),
            GuestCount = 2,
            Status = status
        };

        _context.Reservations.Add(reservation);
        await _context.SaveChangesAsync();

        return reservation;
    }

    private void SetOwner(int userId) =>
        SetCurrentUser(userId, isAdmin: false);

    private void SetAdmin(int userId = 999) =>
        SetCurrentUser(userId, isAdmin: true);

    private void SetCurrentUser(int userId, bool isAdmin)
    {
        _currentUser.Setup(x => x.UserId).Returns(userId);
        _currentUser.Setup(x => x.IsAdmin).Returns(isAdmin);
    }


    // =====================================
    // Confirm
    // =====================================

    [TestMethod]
    public async Task ConfirmReservationAsync_ShouldThrowBusinessRuleException_WhenOwnerOwnsRestaurant()
    {
        SetOwner(1);
        var reservation = await SeedReservationAsync(restaurantOwnerId: 1);

        var ex = await Assert.ThrowsExceptionAsync<BusinessRuleException>(
            () => _service.ConfirmReservationAsync(reservation.Id));

        ex.Message.Should().Be("Reservation confirmation is not part of the active reservation workflow.");
        _context.Reservations.First().Status.Should().Be(ReservationStatus.Pending);
    }

    [TestMethod]
    public async Task ConfirmReservationAsync_ShouldThrowBusinessRuleException_WhenOwnerDoesNotOwnRestaurant()
    {
        SetOwner(1);
        var reservation = await SeedReservationAsync(restaurantOwnerId: 2);

        var ex = await Assert.ThrowsExceptionAsync<BusinessRuleException>(
            () => _service.ConfirmReservationAsync(reservation.Id));

        ex.Message.Should().Be("You are not authorized to confirm this reservation.");
        _context.Reservations.First().Status.Should().Be(ReservationStatus.Pending);
    }

    [TestMethod]
    public async Task ConfirmReservationAsync_ShouldThrowBusinessRuleException_WhenUserIsAdmin_EvenIfNotOwner()
    {
        SetAdmin();
        var reservation = await SeedReservationAsync(restaurantOwnerId: 2);

        var ex = await Assert.ThrowsExceptionAsync<BusinessRuleException>(
            () => _service.ConfirmReservationAsync(reservation.Id));

        ex.Message.Should().Be("Reservation confirmation is not part of the active reservation workflow.");
        _context.Reservations.First().Status.Should().Be(ReservationStatus.Pending);
    }

    [TestMethod]
    public async Task ConfirmReservationAsync_ShouldReturnFalse_WhenReservationDoesNotExist()
    {
        var result = await _service.ConfirmReservationAsync(9999);

        result.Should().BeFalse();
    }


    // =====================================
    // Reject
    // =====================================

    [TestMethod]
    public async Task RejectReservationAsync_ShouldThrowBusinessRuleException_WhenOwnerOwnsRestaurant()
    {
        SetOwner(1);
        var reservation = await SeedReservationAsync(restaurantOwnerId: 1);

        var ex = await Assert.ThrowsExceptionAsync<BusinessRuleException>(
            () => _service.RejectReservationAsync(reservation.Id));

        ex.Message.Should().Be("Reservation rejection is not part of the active reservation workflow.");
        _context.Reservations.First().Status.Should().Be(ReservationStatus.Pending);
    }

    [TestMethod]
    public async Task RejectReservationAsync_ShouldThrowBusinessRuleException_WhenOwnerDoesNotOwnRestaurant()
    {
        SetOwner(1);
        var reservation = await SeedReservationAsync(restaurantOwnerId: 2);

        var ex = await Assert.ThrowsExceptionAsync<BusinessRuleException>(
            () => _service.RejectReservationAsync(reservation.Id));

        ex.Message.Should().Be("You are not authorized to reject this reservation.");
        _context.Reservations.First().Status.Should().Be(ReservationStatus.Pending);
    }

    [TestMethod]
    public async Task RejectReservationAsync_ShouldThrowBusinessRuleException_WhenUserIsAdmin_EvenIfNotOwner()
    {
        SetAdmin();
        var reservation = await SeedReservationAsync(restaurantOwnerId: 2);

        var ex = await Assert.ThrowsExceptionAsync<BusinessRuleException>(
            () => _service.RejectReservationAsync(reservation.Id));

        ex.Message.Should().Be("Reservation rejection is not part of the active reservation workflow.");
        _context.Reservations.First().Status.Should().Be(ReservationStatus.Pending);
    }

    [TestMethod]
    public async Task RejectReservationAsync_ShouldReturnFalse_WhenReservationDoesNotExist()
    {
        var result = await _service.RejectReservationAsync(9999);

        result.Should().BeFalse();
    }


    // =====================================
    // Arrive
    // =====================================

    [TestMethod]
    public async Task ArriveReservationAsync_ShouldThrowBusinessRuleException_WhenOwnerOwnsRestaurant()
    {
        SetOwner(1);
        var reservation = await SeedReservationAsync(restaurantOwnerId: 1);

        var ex = await Assert.ThrowsExceptionAsync<BusinessRuleException>(
            () => _service.ArriveReservationAsync(reservation.Id));

        ex.Message.Should().Be("Arrival status is not part of the active reservation workflow.");
        _context.Reservations.First().Status.Should().Be(ReservationStatus.Pending);
    }

    [TestMethod]
    public async Task ArriveReservationAsync_ShouldThrowBusinessRuleException_WhenOwnerDoesNotOwnRestaurant()
    {
        SetOwner(1);
        var reservation = await SeedReservationAsync(restaurantOwnerId: 2);

        var ex = await Assert.ThrowsExceptionAsync<BusinessRuleException>(
            () => _service.ArriveReservationAsync(reservation.Id));

        ex.Message.Should().Be("You are not authorized to mark this reservation as arrived.");
        _context.Reservations.First().Status.Should().Be(ReservationStatus.Pending);
    }

    [TestMethod]
    public async Task ArriveReservationAsync_ShouldThrowBusinessRuleException_WhenUserIsAdmin_EvenIfNotOwner()
    {
        SetAdmin();
        var reservation = await SeedReservationAsync(restaurantOwnerId: 2);

        var ex = await Assert.ThrowsExceptionAsync<BusinessRuleException>(
            () => _service.ArriveReservationAsync(reservation.Id));

        ex.Message.Should().Be("Arrival status is not part of the active reservation workflow.");
        _context.Reservations.First().Status.Should().Be(ReservationStatus.Pending);
    }

    [TestMethod]
    public async Task ArriveReservationAsync_ShouldReturnFalse_WhenReservationDoesNotExist()
    {
        var result = await _service.ArriveReservationAsync(9999);

        result.Should().BeFalse();
    }


    // =====================================
    // Complete
    // =====================================

    [TestMethod]
    public async Task CompleteReservationAsync_ShouldSucceed_WhenOwnerOwnsRestaurant()
    {
        SetOwner(1);
        var reservation = await SeedReservationAsync(restaurantOwnerId: 1);

        var result = await _service.CompleteReservationAsync(reservation.Id);

        result.Should().BeTrue();
        _context.Reservations.First().Status.Should().Be(ReservationStatus.Completed);
    }

    [TestMethod]
    public async Task CompleteReservationAsync_ShouldThrowBusinessRuleException_WhenOwnerDoesNotOwnRestaurant()
    {
        SetOwner(1);
        var reservation = await SeedReservationAsync(restaurantOwnerId: 2);

        var ex = await Assert.ThrowsExceptionAsync<BusinessRuleException>(
            () => _service.CompleteReservationAsync(reservation.Id));

        ex.Message.Should().Be("You are not authorized to complete this reservation.");
        _context.Reservations.First().Status.Should().Be(ReservationStatus.Pending);
    }

    [TestMethod]
    public async Task CompleteReservationAsync_ShouldSucceed_WhenUserIsAdmin_EvenIfNotOwner()
    {
        SetAdmin();
        var reservation = await SeedReservationAsync(restaurantOwnerId: 2);

        var result = await _service.CompleteReservationAsync(reservation.Id);

        result.Should().BeTrue();
        _context.Reservations.First().Status.Should().Be(ReservationStatus.Completed);
    }

    [TestMethod]
    public async Task CompleteReservationAsync_ShouldReturnFalse_WhenReservationDoesNotExist()
    {
        var result = await _service.CompleteReservationAsync(9999);

        result.Should().BeFalse();
    }

    [TestMethod]
    public async Task CompleteReservationAsync_ShouldThrowBusinessRuleException_WhenStatusIsCompleted()
    {
        SetOwner(1);
        var reservation = await SeedReservationAsync(restaurantOwnerId: 1, status: ReservationStatus.Completed);

        var ex = await Assert.ThrowsExceptionAsync<BusinessRuleException>(
            () => _service.CompleteReservationAsync(reservation.Id));

        ex.Message.Should().Be("This reservation cannot be completed because it is not pending.");
        _context.Reservations.First().Status.Should().Be(ReservationStatus.Completed);
    }

    [TestMethod]
    public async Task CompleteReservationAsync_ShouldThrowBusinessRuleException_WhenStatusIsNoShow()
    {
        SetOwner(1);
        var reservation = await SeedReservationAsync(restaurantOwnerId: 1, status: ReservationStatus.NoShow);

        var ex = await Assert.ThrowsExceptionAsync<BusinessRuleException>(
            () => _service.CompleteReservationAsync(reservation.Id));

        ex.Message.Should().Be("This reservation cannot be completed because it is not pending.");
        _context.Reservations.First().Status.Should().Be(ReservationStatus.NoShow);
    }

    [TestMethod]
    public async Task CompleteReservationAsync_ShouldThrowBusinessRuleException_WhenStatusIsCancelled()
    {
        SetOwner(1);
        var reservation = await SeedReservationAsync(restaurantOwnerId: 1, status: ReservationStatus.Cancelled);

        var ex = await Assert.ThrowsExceptionAsync<BusinessRuleException>(
            () => _service.CompleteReservationAsync(reservation.Id));

        ex.Message.Should().Be("This reservation cannot be completed because it is not pending.");
        _context.Reservations.First().Status.Should().Be(ReservationStatus.Cancelled);
    }

    [TestMethod]
    public async Task CompleteReservationAsync_ShouldThrowBusinessRuleException_WhenStatusIsRejected()
    {
        SetOwner(1);
        var reservation = await SeedReservationAsync(restaurantOwnerId: 1, status: ReservationStatus.Rejected);

        var ex = await Assert.ThrowsExceptionAsync<BusinessRuleException>(
            () => _service.CompleteReservationAsync(reservation.Id));

        ex.Message.Should().Be("This reservation cannot be completed because it is not pending.");
        _context.Reservations.First().Status.Should().Be(ReservationStatus.Rejected);
    }

    [TestMethod]
    public async Task CompleteReservationAsync_ShouldThrowBusinessRuleException_WhenStatusIsConfirmed()
    {
        SetOwner(1);
        var reservation = await SeedReservationAsync(restaurantOwnerId: 1, status: ReservationStatus.Confirmed);

        var ex = await Assert.ThrowsExceptionAsync<BusinessRuleException>(
            () => _service.CompleteReservationAsync(reservation.Id));

        ex.Message.Should().Be("This reservation cannot be completed because it is not pending.");
        _context.Reservations.First().Status.Should().Be(ReservationStatus.Confirmed);
    }

    [TestMethod]
    public async Task CompleteReservationAsync_ShouldThrowBusinessRuleException_WhenStatusIsArrived()
    {
        SetOwner(1);
        var reservation = await SeedReservationAsync(restaurantOwnerId: 1, status: ReservationStatus.Arrived);

        var ex = await Assert.ThrowsExceptionAsync<BusinessRuleException>(
            () => _service.CompleteReservationAsync(reservation.Id));

        ex.Message.Should().Be("This reservation cannot be completed because it is not pending.");
        _context.Reservations.First().Status.Should().Be(ReservationStatus.Arrived);
    }

    [TestMethod]
    public async Task CompleteReservationAsync_ShouldThrowBusinessRuleException_WhenLinkedRedemptionExists()
    {
        // -----------------------------
        // Phase 5Q: completing a redemption requires a bill amount,
        // which this endpoint does not collect. Rather than complete
        // the reservation while leaving its redemption stuck at
        // Redeemed (the Phase 5O/5P inconsistency), or invent bill/
        // discount/final amount data, this must be blocked - the
        // owner must use the redemption completion flow instead.
        // Neither the reservation nor the redemption may change.
        // -----------------------------

        SetOwner(1);
        var reservation = await SeedReservationAsync(restaurantOwnerId: 1);

        var redemption = new Redemption
        {
            ReservationId = reservation.Id,
            DealId = reservation.DealId,
            UserId = reservation.UserId,
            ArrivalDate = reservation.ReservationDate,
            ArrivalTime = reservation.ReservationTime,
            Status = RedemptionStatus.Redeemed
        };

        _context.Redemptions.Add(redemption);
        await _context.SaveChangesAsync();

        var ex = await Assert.ThrowsExceptionAsync<BusinessRuleException>(
            () => _service.CompleteReservationAsync(reservation.Id));

        ex.Message.Should().Be("This reservation must be completed by completing its redemption, which requires a bill amount.");

        _context.Reservations.First().Status.Should().Be(ReservationStatus.Pending);
        _context.Redemptions.First().Status.Should().Be(RedemptionStatus.Redeemed);
    }

    [TestMethod]
    public async Task CompleteReservationAsync_ShouldSucceed_WhenNoLinkedRedemptionExists()
    {
        // Confirms CompleteReservationAsync still works normally (does
        // not crash or behave differently) when there is no linked
        // redemption to reason about - e.g. legacy data.
        SetOwner(1);
        var reservation = await SeedReservationAsync(restaurantOwnerId: 1);

        var result = await _service.CompleteReservationAsync(reservation.Id);

        result.Should().BeTrue();
        _context.Reservations.First().Status.Should().Be(ReservationStatus.Completed);
        _context.Redemptions.Should().BeEmpty();
    }


    // =====================================
    // NoShow
    // =====================================

    [TestMethod]
    public async Task NoShowReservationAsync_ShouldSucceed_WhenOwnerOwnsRestaurant()
    {
        SetOwner(1);
        var reservation = await SeedReservationAsync(restaurantOwnerId: 1);

        var result = await _service.NoShowReservationAsync(reservation.Id);

        result.Should().BeTrue();
        _context.Reservations.First().Status.Should().Be(ReservationStatus.NoShow);
    }

    [TestMethod]
    public async Task NoShowReservationAsync_ShouldThrowBusinessRuleException_WhenOwnerDoesNotOwnRestaurant()
    {
        SetOwner(1);
        var reservation = await SeedReservationAsync(restaurantOwnerId: 2);

        var ex = await Assert.ThrowsExceptionAsync<BusinessRuleException>(
            () => _service.NoShowReservationAsync(reservation.Id));

        ex.Message.Should().Be("You are not authorized to mark this reservation as a no-show.");
        _context.Reservations.First().Status.Should().Be(ReservationStatus.Pending);
    }

    [TestMethod]
    public async Task NoShowReservationAsync_ShouldSucceed_WhenUserIsAdmin_EvenIfNotOwner()
    {
        SetAdmin();
        var reservation = await SeedReservationAsync(restaurantOwnerId: 2);

        var result = await _service.NoShowReservationAsync(reservation.Id);

        result.Should().BeTrue();
        _context.Reservations.First().Status.Should().Be(ReservationStatus.NoShow);
    }

    [TestMethod]
    public async Task NoShowReservationAsync_ShouldReturnFalse_WhenReservationDoesNotExist()
    {
        var result = await _service.NoShowReservationAsync(9999);

        result.Should().BeFalse();
    }

    [TestMethod]
    public async Task NoShowReservationAsync_ShouldThrowBusinessRuleException_WhenStatusIsCompleted()
    {
        SetOwner(1);
        var reservation = await SeedReservationAsync(restaurantOwnerId: 1, status: ReservationStatus.Completed);

        var ex = await Assert.ThrowsExceptionAsync<BusinessRuleException>(
            () => _service.NoShowReservationAsync(reservation.Id));

        ex.Message.Should().Be("This reservation cannot be marked as no-show because it is not pending.");
        _context.Reservations.First().Status.Should().Be(ReservationStatus.Completed);
    }

    [TestMethod]
    public async Task NoShowReservationAsync_ShouldThrowBusinessRuleException_WhenStatusIsCancelled()
    {
        SetOwner(1);
        var reservation = await SeedReservationAsync(restaurantOwnerId: 1, status: ReservationStatus.Cancelled);

        var ex = await Assert.ThrowsExceptionAsync<BusinessRuleException>(
            () => _service.NoShowReservationAsync(reservation.Id));

        ex.Message.Should().Be("This reservation cannot be marked as no-show because it is not pending.");
        _context.Reservations.First().Status.Should().Be(ReservationStatus.Cancelled);
    }

    [TestMethod]
    public async Task NoShowReservationAsync_ShouldThrowBusinessRuleException_WhenStatusIsAlreadyNoShow()
    {
        SetOwner(1);
        var reservation = await SeedReservationAsync(restaurantOwnerId: 1, status: ReservationStatus.NoShow);

        var ex = await Assert.ThrowsExceptionAsync<BusinessRuleException>(
            () => _service.NoShowReservationAsync(reservation.Id));

        ex.Message.Should().Be("This reservation cannot be marked as no-show because it is not pending.");
        _context.Reservations.First().Status.Should().Be(ReservationStatus.NoShow);
    }

    [TestMethod]
    public async Task NoShowReservationAsync_ShouldSetLinkedRedemptionToCancelled()
    {
        // -----------------------------
        // Phase 5Q regression test: before this fix, NoShowReservationAsync
        // left the linked Redemption stuck at Redeemed forever, since it
        // could never subsequently be completed (its reservation is no
        // longer Pending) or cancelled by any other endpoint.
        // -----------------------------

        SetOwner(1);
        var reservation = await SeedReservationAsync(restaurantOwnerId: 1);

        var redemption = new Redemption
        {
            ReservationId = reservation.Id,
            DealId = reservation.DealId,
            UserId = reservation.UserId,
            ArrivalDate = reservation.ReservationDate,
            ArrivalTime = reservation.ReservationTime,
            Status = RedemptionStatus.Redeemed
        };

        _context.Redemptions.Add(redemption);
        await _context.SaveChangesAsync();

        var result = await _service.NoShowReservationAsync(reservation.Id);

        result.Should().BeTrue();
        _context.Reservations.First().Status.Should().Be(ReservationStatus.NoShow);
        _context.Redemptions.First().Status.Should().Be(RedemptionStatus.Cancelled);
    }

    [TestMethod]
    public async Task NoShowReservationAsync_ShouldSucceedWhenNoLinkedRedemptionExists()
    {
        SetOwner(1);
        var reservation = await SeedReservationAsync(restaurantOwnerId: 1);

        var result = await _service.NoShowReservationAsync(reservation.Id);

        result.Should().BeTrue();
        _context.Reservations.First().Status.Should().Be(ReservationStatus.NoShow);
        _context.Redemptions.Should().BeEmpty();
    }


    // =====================================
    // Cancel
    // =====================================

    [TestMethod]
    public async Task CancelReservationAsync_ShouldSucceed_WhenOwnerOwnsRestaurant()
    {
        SetOwner(1);
        var reservation = await SeedReservationAsync(restaurantOwnerId: 1);

        var result = await _service.CancelReservationAsync(reservation.Id);

        result.Should().BeTrue();
        _context.Reservations.First().Status.Should().Be(ReservationStatus.Cancelled);
    }

    [TestMethod]
    public async Task CancelReservationAsync_ShouldThrowBusinessRuleException_WhenOwnerDoesNotOwnRestaurant()
    {
        SetOwner(1);
        var reservation = await SeedReservationAsync(restaurantOwnerId: 2);

        var ex = await Assert.ThrowsExceptionAsync<BusinessRuleException>(
            () => _service.CancelReservationAsync(reservation.Id));

        ex.Message.Should().Be("You are not authorized to cancel this reservation.");
        _context.Reservations.First().Status.Should().Be(ReservationStatus.Pending);
    }

    [TestMethod]
    public async Task CancelReservationAsync_ShouldSucceed_WhenUserIsAdmin_EvenIfNotOwner()
    {
        SetAdmin();
        var reservation = await SeedReservationAsync(restaurantOwnerId: 2);

        var result = await _service.CancelReservationAsync(reservation.Id);

        result.Should().BeTrue();
        _context.Reservations.First().Status.Should().Be(ReservationStatus.Cancelled);
    }

    [TestMethod]
    public async Task CancelReservationAsync_ShouldReturnFalse_WhenReservationDoesNotExist()
    {
        var result = await _service.CancelReservationAsync(9999);

        result.Should().BeFalse();
    }

    [TestMethod]
    public async Task CancelReservationAsync_ShouldThrowBusinessRuleException_WhenStatusIsCompleted()
    {
        SetOwner(1);
        var reservation = await SeedReservationAsync(restaurantOwnerId: 1, status: ReservationStatus.Completed);

        var ex = await Assert.ThrowsExceptionAsync<BusinessRuleException>(
            () => _service.CancelReservationAsync(reservation.Id));

        ex.Message.Should().Be("This reservation cannot be cancelled because it is not pending.");
        _context.Reservations.First().Status.Should().Be(ReservationStatus.Completed);
    }

    [TestMethod]
    public async Task CancelReservationAsync_ShouldThrowBusinessRuleException_WhenStatusIsNoShow()
    {
        SetOwner(1);
        var reservation = await SeedReservationAsync(restaurantOwnerId: 1, status: ReservationStatus.NoShow);

        var ex = await Assert.ThrowsExceptionAsync<BusinessRuleException>(
            () => _service.CancelReservationAsync(reservation.Id));

        ex.Message.Should().Be("This reservation cannot be cancelled because it is not pending.");
        _context.Reservations.First().Status.Should().Be(ReservationStatus.NoShow);
    }

    [TestMethod]
    public async Task CancelReservationAsync_ShouldThrowBusinessRuleException_WhenStatusIsAlreadyCancelled()
    {
        SetOwner(1);
        var reservation = await SeedReservationAsync(restaurantOwnerId: 1, status: ReservationStatus.Cancelled);

        var ex = await Assert.ThrowsExceptionAsync<BusinessRuleException>(
            () => _service.CancelReservationAsync(reservation.Id));

        ex.Message.Should().Be("This reservation cannot be cancelled because it is not pending.");
        _context.Reservations.First().Status.Should().Be(ReservationStatus.Cancelled);
    }

    [TestMethod]
    public async Task CancelReservationAsync_ShouldThrowBusinessRuleException_WhenStatusIsConfirmed()
    {
        SetOwner(1);
        var reservation = await SeedReservationAsync(restaurantOwnerId: 1, status: ReservationStatus.Confirmed);

        var ex = await Assert.ThrowsExceptionAsync<BusinessRuleException>(
            () => _service.CancelReservationAsync(reservation.Id));

        ex.Message.Should().Be("This reservation cannot be cancelled because it is not pending.");
        _context.Reservations.First().Status.Should().Be(ReservationStatus.Confirmed);
    }

    [TestMethod]
    public async Task CancelReservationAsync_ShouldThrowBusinessRuleException_WhenStatusIsArrived()
    {
        SetOwner(1);
        var reservation = await SeedReservationAsync(restaurantOwnerId: 1, status: ReservationStatus.Arrived);

        var ex = await Assert.ThrowsExceptionAsync<BusinessRuleException>(
            () => _service.CancelReservationAsync(reservation.Id));

        ex.Message.Should().Be("This reservation cannot be cancelled because it is not pending.");
        _context.Reservations.First().Status.Should().Be(ReservationStatus.Arrived);
    }

    [TestMethod]
    public async Task CancelReservationAsync_ShouldNotChangeCompletedLinkedRedemption_WhenStatusIsCompleted()
    {
        // -----------------------------
        // Arrange
        // Purpose:
        // A Completed reservation is rejected before the redemption
        // is ever touched, so a completed sale's financial record
        // must remain exactly as it was.
        // -----------------------------

        SetOwner(1);
        var reservation = await SeedReservationAsync(restaurantOwnerId: 1, status: ReservationStatus.Completed);

        var redemption = new Redemption
        {
            ReservationId = reservation.Id,
            DealId = reservation.DealId,
            UserId = reservation.UserId,
            ArrivalDate = reservation.ReservationDate,
            ArrivalTime = reservation.ReservationTime,
            BillAmount = 100m,
            DiscountAmount = 20m,
            FinalAmount = 80m,
            Status = RedemptionStatus.Completed
        };

        _context.Redemptions.Add(redemption);
        await _context.SaveChangesAsync();

        // -----------------------------
        // Act & Assert
        // -----------------------------

        await Assert.ThrowsExceptionAsync<BusinessRuleException>(
            () => _service.CancelReservationAsync(reservation.Id));

        _context.Redemptions.First().Status.Should().Be(RedemptionStatus.Completed);
        _context.Redemptions.First().BillAmount.Should().Be(100m);
    }


    // =====================================
    // CancelMyReservationAsync (customer self-service -
    // authorizes on reservation.UserId, not restaurant
    // ownership; does not use EnsureOwnership at all)
    // =====================================

    [TestMethod]
    public async Task CancelMyReservationAsync_ShouldSucceed_WhenStatusIsPending()
    {
        var reservation = await SeedReservationAsync(
            restaurantOwnerId: 1,
            status: ReservationStatus.Pending,
            customerUserId: 50);

        var result = await _service.CancelMyReservationAsync(reservation.Id, userId: 50);

        result.Should().BeTrue();
        _context.Reservations.First().Status.Should().Be(ReservationStatus.Cancelled);
    }

    [TestMethod]
    public async Task CancelMyReservationAsync_ShouldThrowBusinessRuleException_WhenStatusIsConfirmed()
    {
        // -----------------------------
        // Confirmed is no longer an active workflow state under
        // the MVP lifecycle (Pending -> Completed/NoShow/Cancelled
        // only), so it must no longer be customer-cancellable.
        // -----------------------------

        var reservation = await SeedReservationAsync(
            restaurantOwnerId: 1,
            status: ReservationStatus.Confirmed,
            customerUserId: 50);

        var ex = await Assert.ThrowsExceptionAsync<BusinessRuleException>(
            () => _service.CancelMyReservationAsync(reservation.Id, userId: 50));

        ex.Message.Should().Be("This reservation cannot be cancelled.");
        _context.Reservations.First().Status.Should().Be(ReservationStatus.Confirmed);
    }

    [TestMethod]
    public async Task CancelMyReservationAsync_ShouldThrowNotFoundException_WhenReservationDoesNotExist()
    {
        var ex = await Assert.ThrowsExceptionAsync<NotFoundException>(
            () => _service.CancelMyReservationAsync(9999, userId: 50));

        ex.Message.Should().Be("Reservation not found.");
    }

    [TestMethod]
    public async Task CancelMyReservationAsync_ShouldThrowBusinessRuleException_WhenReservationBelongsToAnotherCustomer()
    {
        var reservation = await SeedReservationAsync(
            restaurantOwnerId: 1,
            status: ReservationStatus.Pending,
            customerUserId: 50);

        var ex = await Assert.ThrowsExceptionAsync<BusinessRuleException>(
            () => _service.CancelMyReservationAsync(reservation.Id, userId: 99));

        ex.Message.Should().Be("You are not authorized to cancel this reservation.");
        _context.Reservations.First().Status.Should().Be(ReservationStatus.Pending);
    }

    [TestMethod]
    public async Task CancelMyReservationAsync_ShouldThrowBusinessRuleException_WhenStatusIsArrived()
    {
        var reservation = await SeedReservationAsync(
            restaurantOwnerId: 1,
            status: ReservationStatus.Arrived,
            customerUserId: 50);

        var ex = await Assert.ThrowsExceptionAsync<BusinessRuleException>(
            () => _service.CancelMyReservationAsync(reservation.Id, userId: 50));

        ex.Message.Should().Be("This reservation cannot be cancelled.");
        _context.Reservations.First().Status.Should().Be(ReservationStatus.Arrived);
    }

    [TestMethod]
    public async Task CancelMyReservationAsync_ShouldThrowBusinessRuleException_WhenStatusIsCompleted()
    {
        var reservation = await SeedReservationAsync(
            restaurantOwnerId: 1,
            status: ReservationStatus.Completed,
            customerUserId: 50);

        var ex = await Assert.ThrowsExceptionAsync<BusinessRuleException>(
            () => _service.CancelMyReservationAsync(reservation.Id, userId: 50));

        ex.Message.Should().Be("This reservation cannot be cancelled.");
        _context.Reservations.First().Status.Should().Be(ReservationStatus.Completed);
    }

    [TestMethod]
    public async Task CancelMyReservationAsync_ShouldNotChangeCompletedLinkedRedemption_WhenStatusIsCompleted()
    {
        // -----------------------------
        // Arrange
        // Purpose:
        // Same guarantee as the owner-side cancellation path -
        // a Completed reservation is rejected before the redemption
        // is ever touched, so a completed sale's financial record
        // must remain exactly as it was.
        // -----------------------------

        var reservation = await SeedReservationAsync(
            restaurantOwnerId: 1,
            status: ReservationStatus.Completed,
            customerUserId: 50);

        var redemption = new Redemption
        {
            ReservationId = reservation.Id,
            DealId = reservation.DealId,
            UserId = reservation.UserId,
            ArrivalDate = reservation.ReservationDate,
            ArrivalTime = reservation.ReservationTime,
            BillAmount = 100m,
            DiscountAmount = 20m,
            FinalAmount = 80m,
            Status = RedemptionStatus.Completed
        };

        _context.Redemptions.Add(redemption);
        await _context.SaveChangesAsync();

        // -----------------------------
        // Act & Assert
        // -----------------------------

        await Assert.ThrowsExceptionAsync<BusinessRuleException>(
            () => _service.CancelMyReservationAsync(reservation.Id, userId: 50));

        _context.Redemptions.First().Status.Should().Be(RedemptionStatus.Completed);
        _context.Redemptions.First().BillAmount.Should().Be(100m);
    }

    [TestMethod]
    public async Task CancelMyReservationAsync_ShouldThrowBusinessRuleException_WhenStatusIsAlreadyCancelled()
    {
        var reservation = await SeedReservationAsync(
            restaurantOwnerId: 1,
            status: ReservationStatus.Cancelled,
            customerUserId: 50);

        var ex = await Assert.ThrowsExceptionAsync<BusinessRuleException>(
            () => _service.CancelMyReservationAsync(reservation.Id, userId: 50));

        ex.Message.Should().Be("This reservation cannot be cancelled.");
        _context.Reservations.First().Status.Should().Be(ReservationStatus.Cancelled);
    }

    [TestMethod]
    public async Task CancelMyReservationAsync_ShouldThrowBusinessRuleException_WhenStatusIsRejected()
    {
        var reservation = await SeedReservationAsync(
            restaurantOwnerId: 1,
            status: ReservationStatus.Rejected,
            customerUserId: 50);

        var ex = await Assert.ThrowsExceptionAsync<BusinessRuleException>(
            () => _service.CancelMyReservationAsync(reservation.Id, userId: 50));

        ex.Message.Should().Be("This reservation cannot be cancelled.");
        _context.Reservations.First().Status.Should().Be(ReservationStatus.Rejected);
    }

    [TestMethod]
    public async Task CancelMyReservationAsync_ShouldThrowBusinessRuleException_WhenStatusIsNoShow()
    {
        var reservation = await SeedReservationAsync(
            restaurantOwnerId: 1,
            status: ReservationStatus.NoShow,
            customerUserId: 50);

        var ex = await Assert.ThrowsExceptionAsync<BusinessRuleException>(
            () => _service.CancelMyReservationAsync(reservation.Id, userId: 50));

        ex.Message.Should().Be("This reservation cannot be cancelled.");
        _context.Reservations.First().Status.Should().Be(ReservationStatus.NoShow);
    }


    // =====================================
    // Reservation/Redemption consistency
    // (Phase 3C: Redemption.ReservationId link)
    // =====================================

    [TestMethod]
    public async Task CreateAsync_ShouldSetReservationIdOnAutoCreatedRedemption()
    {
        var deal = await SeedDealAsync();

        var dto = BuildCreateReservationDto(deal.Id);

        var result = await _service.CreateAsync(dto);

        var redemption = _context.Redemptions.Single();

        redemption.ReservationId.Should().Be(result.Id);
    }

    [TestMethod]
    public async Task CancelMyReservationAsync_ShouldSetLinkedRedemptionToCancelled()
    {
        var reservation = await SeedReservationAsync(
            restaurantOwnerId: 1,
            status: ReservationStatus.Pending,
            customerUserId: 50);

        var redemption = new Redemption
        {
            ReservationId = reservation.Id,
            DealId = reservation.DealId,
            UserId = reservation.UserId,
            ArrivalDate = reservation.ReservationDate,
            ArrivalTime = reservation.ReservationTime,
            Status = RedemptionStatus.Redeemed
        };

        _context.Redemptions.Add(redemption);
        await _context.SaveChangesAsync();

        var result = await _service.CancelMyReservationAsync(reservation.Id, userId: 50);

        result.Should().BeTrue();
        _context.Reservations.First().Status.Should().Be(ReservationStatus.Cancelled);
        _context.Redemptions.First().Status.Should().Be(RedemptionStatus.Cancelled);
    }

    [TestMethod]
    public async Task CancelMyReservationAsync_ShouldNotAffectAnotherRedemption()
    {
        // -----------------------------
        // Regression scenario: cancel reservation A, then create a
        // new reservation B for the same customer/deal/date/time.
        // Redemption A and Redemption B end up with identical
        // UserId+DealId+ArrivalDate+ArrivalTime - proving the old
        // four-field matching approach could not safely distinguish
        // them. Only the redemption actually linked via
        // ReservationId to the cancelled reservation must change.
        // -----------------------------

        SetCurrentUser(50, isAdmin: false);

        var deal = await SeedDealAsync();

        var dtoA = BuildCreateReservationDto(deal.Id);
        var reservationA = await _service.CreateAsync(dtoA);

        await _service.CancelMyReservationAsync(reservationA.Id, userId: 50);

        var dtoB = BuildCreateReservationDto(deal.Id);
        var reservationB = await _service.CreateAsync(dtoB);

        var redemptionA = _context.Redemptions
            .Single(r => r.ReservationId == reservationA.Id);

        var redemptionB = _context.Redemptions
            .Single(r => r.ReservationId == reservationB.Id);

        redemptionA.Status.Should().Be(RedemptionStatus.Cancelled);
        redemptionB.Status.Should().Be(RedemptionStatus.Redeemed);
    }

    [TestMethod]
    public async Task CancelMyReservationAsync_ShouldSucceedWhenNoLinkedRedemptionExists()
    {
        var reservation = await SeedReservationAsync(
            restaurantOwnerId: 1,
            status: ReservationStatus.Pending,
            customerUserId: 50);

        var result = await _service.CancelMyReservationAsync(reservation.Id, userId: 50);

        result.Should().BeTrue();
        _context.Reservations.First().Status.Should().Be(ReservationStatus.Cancelled);
        _context.Redemptions.Should().BeEmpty();
    }

    [TestMethod]
    public async Task CancelReservationAsync_ShouldSetLinkedRedemptionToCancelled()
    {
        SetOwner(1);

        var reservation = await SeedReservationAsync(restaurantOwnerId: 1);

        var redemption = new Redemption
        {
            ReservationId = reservation.Id,
            DealId = reservation.DealId,
            UserId = reservation.UserId,
            ArrivalDate = reservation.ReservationDate,
            ArrivalTime = reservation.ReservationTime,
            Status = RedemptionStatus.Redeemed
        };

        _context.Redemptions.Add(redemption);
        await _context.SaveChangesAsync();

        var result = await _service.CancelReservationAsync(reservation.Id);

        result.Should().BeTrue();
        _context.Reservations.First().Status.Should().Be(ReservationStatus.Cancelled);
        _context.Redemptions.First().Status.Should().Be(RedemptionStatus.Cancelled);
    }

    [TestMethod]
    public async Task CancelReservationAsync_ShouldSucceedWhenNoLinkedRedemptionExists()
    {
        SetOwner(1);

        var reservation = await SeedReservationAsync(restaurantOwnerId: 1);

        var result = await _service.CancelReservationAsync(reservation.Id);

        result.Should().BeTrue();
        _context.Reservations.First().Status.Should().Be(ReservationStatus.Cancelled);
        _context.Redemptions.Should().BeEmpty();
    }

    [TestMethod]
    public async Task GetOwnerReservationsAsync_ShouldResolveRedemptionIdUsingReservationIdLink()
    {
        var reservation = await SeedReservationAsync(restaurantOwnerId: 1);

        var redemption = new Redemption
        {
            ReservationId = reservation.Id,
            DealId = reservation.DealId,
            UserId = reservation.UserId,
            ArrivalDate = reservation.ReservationDate,
            ArrivalTime = reservation.ReservationTime,
            Status = RedemptionStatus.Redeemed
        };

        _context.Redemptions.Add(redemption);
        await _context.SaveChangesAsync();

        var result = await _service.GetOwnerReservationsAsync(ownerId: 1);

        result.Single(r => r.Id == reservation.Id).RedemptionId
            .Should().Be(redemption.Id);
    }

    [TestMethod]
    public async Task GetOwnerReservationsAsync_ShouldNotInventAssociationViaFourFieldFallback()
    {
        // -----------------------------
        // Phase 5P removed the legacy four-field (UserId/DealId/
        // ArrivalDate/ArrivalTime) fallback match. A reservation with
        // no redemption linked by ReservationId must report
        // RedemptionId = null, even if an unrelated redemption exists
        // that happens to share those four values.
        // -----------------------------

        var reservation = await SeedReservationAsync(restaurantOwnerId: 1);

        var unrelatedRedemption = new Redemption
        {
            ReservationId = null,
            DealId = reservation.DealId,
            UserId = reservation.UserId,
            ArrivalDate = reservation.ReservationDate,
            ArrivalTime = reservation.ReservationTime,
            Status = RedemptionStatus.Redeemed
        };

        _context.Redemptions.Add(unrelatedRedemption);
        await _context.SaveChangesAsync();

        var result = await _service.GetOwnerReservationsAsync(ownerId: 1);

        result.Single(r => r.Id == reservation.Id).RedemptionId
            .Should().BeNull();
    }


    // =====================================
    // Delete (service-level rule only -
    // the controller keeps this Admin-only;
    // not tested/changed here)
    // =====================================

    [TestMethod]
    public async Task DeleteAsync_ShouldSucceed_WhenOwnerOwnsRestaurant()
    {
        SetOwner(1);
        var reservation = await SeedReservationAsync(restaurantOwnerId: 1);

        var result = await _service.DeleteAsync(reservation.Id);

        result.Should().BeTrue();
        _context.Reservations.Count().Should().Be(0);
    }

    [TestMethod]
    public async Task DeleteAsync_ShouldThrowBusinessRuleException_WhenOwnerDoesNotOwnRestaurant()
    {
        SetOwner(1);
        var reservation = await SeedReservationAsync(restaurantOwnerId: 2);

        var ex = await Assert.ThrowsExceptionAsync<BusinessRuleException>(
            () => _service.DeleteAsync(reservation.Id));

        ex.Message.Should().Be("You are not authorized to delete this reservation.");
        _context.Reservations.Count().Should().Be(1);
    }

    [TestMethod]
    public async Task DeleteAsync_ShouldSucceed_WhenUserIsAdmin_EvenIfNotOwner()
    {
        SetAdmin();
        var reservation = await SeedReservationAsync(restaurantOwnerId: 2);

        var result = await _service.DeleteAsync(reservation.Id);

        result.Should().BeTrue();
        _context.Reservations.Count().Should().Be(0);
    }

    [TestMethod]
    public async Task DeleteAsync_ShouldReturnFalse_WhenReservationDoesNotExist()
    {
        var result = await _service.DeleteAsync(9999);

        result.Should().BeFalse();
    }


    // =====================================
    // GetMyReservationsAsync - DealTitle/RestaurantName mapping
    // (Phase 3D-B: ReservationDto now exposes deal/restaurant
    // identity via ProjectTo, with no explicit Include needed)
    // =====================================

    [TestMethod]
    public async Task GetMyReservationsAsync_ShouldIncludeDealTitleAndRestaurantName()
    {
        var reservation = await SeedReservationAsync(
            restaurantOwnerId: 1,
            customerUserId: 50);

        var results = await _service.GetMyReservationsAsync(50);

        var result = results.Single();

        result.Id.Should().Be(reservation.Id);
        result.DealTitle.Should().Be("20% Lunch");
        result.RestaurantName.Should().Be("Spice Kitchen");
    }


    // =====================================
    // GetById - authorization rules
    //
    // Unauthorized access returns null (not an
    // exception), so it is indistinguishable from
    // a nonexistent reservation.
    // =====================================

    [TestMethod]
    public async Task GetByIdAsync_ShouldReturnReservation_WhenCustomerOwnsIt()
    {
        SetCurrentUser(10, isAdmin: false);

        var reservation = await SeedReservationAsync(
            restaurantOwnerId: 99,
            customerUserId: 10);

        var result = await _service.GetByIdAsync(reservation.Id);

        result.Should().NotBeNull();
        result!.Id.Should().Be(reservation.Id);
    }

    [TestMethod]
    public async Task GetByIdAsync_ShouldReturnNull_WhenAnotherCustomerOwnsIt()
    {
        SetCurrentUser(10, isAdmin: false);

        var reservation = await SeedReservationAsync(
            restaurantOwnerId: 99,
            customerUserId: 20);

        var result = await _service.GetByIdAsync(reservation.Id);

        result.Should().BeNull();
    }

    [TestMethod]
    public async Task GetByIdAsync_ShouldReturnReservation_WhenOwnerOwnsRestaurant()
    {
        SetOwner(1);

        var reservation = await SeedReservationAsync(
            restaurantOwnerId: 1,
            customerUserId: 50);

        var result = await _service.GetByIdAsync(reservation.Id);

        result.Should().NotBeNull();
        result!.Id.Should().Be(reservation.Id);
    }

    [TestMethod]
    public async Task GetByIdAsync_ShouldReturnNull_WhenOwnerDoesNotOwnRestaurant()
    {
        SetOwner(1);

        var reservation = await SeedReservationAsync(
            restaurantOwnerId: 2,
            customerUserId: 50);

        var result = await _service.GetByIdAsync(reservation.Id);

        result.Should().BeNull();
    }

    [TestMethod]
    public async Task GetByIdAsync_ShouldReturnReservation_WhenUserIsAdmin()
    {
        SetCurrentUser(999, isAdmin: true);

        var reservation = await SeedReservationAsync(
            restaurantOwnerId: 2,
            customerUserId: 50);

        var result = await _service.GetByIdAsync(reservation.Id);

        result.Should().NotBeNull();
        result!.Id.Should().Be(reservation.Id);
    }

    [TestMethod]
    public async Task GetByIdAsync_ShouldReturnNull_WhenReservationDoesNotExist()
    {
        var result = await _service.GetByIdAsync(9999);

        result.Should().BeNull();
    }

    // -----------------------------
    // CreateAsync
    // -----------------------------

    // -----------------------------
    // Seeds a Restaurant -> Deal graph (no Reservation) so
    // CreateAsync can be exercised directly against a deal
    // with caller-controlled availability characteristics.
    // -----------------------------
    private async Task<Deal> SeedDealAsync(
        bool dealIsActive = true,
        bool restaurantIsActive = true,
        int reservationLimit = 0,
        int maximumGuests = 4)
    {
        var restaurant = new Restaurant
        {
            Name = "Spice Kitchen",
            OwnerId = 1,
            IsActive = restaurantIsActive
        };

        _context.Restaurants.Add(restaurant);
        await _context.SaveChangesAsync();

        var deal = new Deal
        {
            RestaurantId = restaurant.Id,
            Title = "20% Lunch",
            DiscountPercentage = 20,
            StartDate = DateOnly.FromDateTime(DateTime.Today),
            EndDate = DateOnly.FromDateTime(DateTime.Today.AddDays(30)),
            StartTime = new TimeOnly(12, 0),
            EndTime = new TimeOnly(15, 0),
            MaximumGuests = maximumGuests,
            ReservationLimit = reservationLimit,
            DailyRedemptionLimit = 100,
            IsActive = dealIsActive
        };

        _context.Deals.Add(deal);
        await _context.SaveChangesAsync();

        return deal;
    }

    private static CreateReservationDto BuildCreateReservationDto(
        int dealId,
        DateOnly? reservationDate = null,
        TimeOnly? reservationTime = null,
        int guestCount = 2) => new()
    {
        DealId = dealId,
        CustomerName = "Jane Diner",
        PhoneNumber = "0400111222",
        Email = "jane@test.com",
        ReservationDate = reservationDate ?? DateOnly.FromDateTime(DateTime.Today),
        ReservationTime = reservationTime ?? new TimeOnly(12, 30),
        GuestCount = guestCount
    };

    [TestMethod]
    public async Task CreateAsync_ShouldThrowNotFoundException_WhenDealDoesNotExist()
    {
        var dto = BuildCreateReservationDto(dealId: 9999);

        var ex = await Assert.ThrowsExceptionAsync<NotFoundException>(
            () => _service.CreateAsync(dto));

        ex.Message.Should().Be("Deal not found.");
    }

    [TestMethod]
    public async Task CreateAsync_ShouldThrowBusinessRuleException_WhenDealIsFullyBooked()
    {
        var deal = await SeedDealAsync(reservationLimit: 1);

        var existing = new Reservation
        {
            DealId = deal.Id,
            UserId = 50,
            CustomerName = "Existing Diner",
            PhoneNumber = "0400111222",
            Email = "existing@test.com",
            ReservationDate = DateOnly.FromDateTime(DateTime.Today),
            ReservationTime = new TimeOnly(12, 30),
            GuestCount = 2,
            Status = ReservationStatus.Pending
        };

        _context.Reservations.Add(existing);
        await _context.SaveChangesAsync();

        var dto = BuildCreateReservationDto(deal.Id);

        var ex = await Assert.ThrowsExceptionAsync<BusinessRuleException>(
            () => _service.CreateAsync(dto));

        ex.Message.Should().Be("This deal is fully booked.");
    }

    [TestMethod]
    public async Task CreateAsync_ShouldThrowBusinessRuleException_WhenDealIsInactive()
    {
        var deal = await SeedDealAsync(dealIsActive: false);

        var dto = BuildCreateReservationDto(deal.Id);

        var ex = await Assert.ThrowsExceptionAsync<BusinessRuleException>(
            () => _service.CreateAsync(dto));

        ex.Message.Should().Be("Offer is inactive.");
    }

    [TestMethod]
    public async Task CreateAsync_ShouldThrowBusinessRuleException_WhenReservationDateIsBeforeDealStartDate()
    {
        var deal = await SeedDealAsync();

        var dto = BuildCreateReservationDto(
            deal.Id,
            reservationDate: deal.StartDate.AddDays(-1));

        var ex = await Assert.ThrowsExceptionAsync<BusinessRuleException>(
            () => _service.CreateAsync(dto));

        ex.Message.Should().Be("Offer is not available on the selected arrival date.");
    }

    [TestMethod]
    public async Task CreateAsync_ShouldThrowBusinessRuleException_WhenReservationDateIsAfterDealEndDate()
    {
        var deal = await SeedDealAsync();

        var dto = BuildCreateReservationDto(
            deal.Id,
            reservationDate: deal.EndDate.AddDays(1));

        var ex = await Assert.ThrowsExceptionAsync<BusinessRuleException>(
            () => _service.CreateAsync(dto));

        ex.Message.Should().Be("Offer is not available on the selected arrival date.");
    }

    [TestMethod]
    public async Task CreateAsync_ShouldThrowBusinessRuleException_WhenReservationTimeIsBeforeDealStartTime()
    {
        var deal = await SeedDealAsync();

        var dto = BuildCreateReservationDto(
            deal.Id,
            reservationTime: deal.StartTime.AddHours(-1));

        var ex = await Assert.ThrowsExceptionAsync<BusinessRuleException>(
            () => _service.CreateAsync(dto));

        ex.Message.Should().Be("Arrival time must be within the offer time.");
    }

    [TestMethod]
    public async Task CreateAsync_ShouldThrowBusinessRuleException_WhenReservationTimeIsAfterDealEndTime()
    {
        var deal = await SeedDealAsync();

        var dto = BuildCreateReservationDto(
            deal.Id,
            reservationTime: deal.EndTime.AddHours(1));

        var ex = await Assert.ThrowsExceptionAsync<BusinessRuleException>(
            () => _service.CreateAsync(dto));

        ex.Message.Should().Be("Arrival time must be within the offer time.");
    }

    [TestMethod]
    public async Task CreateAsync_ShouldThrowBusinessRuleException_WhenRestaurantIsInactive()
    {
        var deal = await SeedDealAsync(restaurantIsActive: false);

        var dto = BuildCreateReservationDto(deal.Id);

        var ex = await Assert.ThrowsExceptionAsync<BusinessRuleException>(
            () => _service.CreateAsync(dto));

        ex.Message.Should().Be("Restaurant is inactive.");
    }

    [TestMethod]
    public async Task CreateAsync_ShouldThrowBusinessRuleException_WhenGuestCountExceedsMaximum()
    {
        var deal = await SeedDealAsync(maximumGuests: 4);

        var dto = BuildCreateReservationDto(deal.Id, guestCount: 5);

        var ex = await Assert.ThrowsExceptionAsync<BusinessRuleException>(
            () => _service.CreateAsync(dto));

        ex.Message.Should().Be("Maximum 4 guests allowed.");
    }

    [TestMethod]
    public async Task CreateAsync_ShouldCreateReservationAndRedemption_WhenRequestIsValid()
    {
        SetCurrentUser(50, isAdmin: false);

        var deal = await SeedDealAsync();

        var dto = BuildCreateReservationDto(deal.Id);

        var result = await _service.CreateAsync(dto);

        result.Should().NotBeNull();

        var reservation = _context.Reservations.Single();

        reservation.DealId.Should().Be(deal.Id);
        reservation.UserId.Should().Be(50);
        reservation.CustomerName.Should().Be(dto.CustomerName);
        reservation.PhoneNumber.Should().Be(dto.PhoneNumber);
        reservation.Email.Should().Be(dto.Email);
        reservation.ReservationDate.Should().Be(dto.ReservationDate);
        reservation.ReservationTime.Should().Be(dto.ReservationTime);
        reservation.GuestCount.Should().Be(dto.GuestCount);
        reservation.Status.Should().Be(ReservationStatus.Pending);

        var redemption = _context.Redemptions.Single();

        redemption.DealId.Should().Be(deal.Id);
        redemption.UserId.Should().Be(50);
        redemption.ArrivalDate.Should().Be(dto.ReservationDate);
        redemption.ArrivalTime.Should().Be(dto.ReservationTime);
        redemption.GuestCount.Should().Be(dto.GuestCount);
        redemption.Status.Should().Be(RedemptionStatus.Redeemed);
    }


    // =====================================
    // CreateAsync - duplicate reservation policy
    // (Phase 5D, Option B: one active reservation per
    // customer per deal/date/time; Cancelled/Rejected/NoShow
    // don't count, guest count is irrelevant to the check)
    // =====================================

    [TestMethod]
    public async Task CreateAsync_ShouldRejectDuplicateActiveReservationForSameCustomerAndSlot()
    {
        SetCurrentUser(50, isAdmin: false);

        var deal = await SeedDealAsync();

        var dto = BuildCreateReservationDto(deal.Id);

        await _service.CreateAsync(dto);

        var ex = await Assert.ThrowsExceptionAsync<BusinessRuleException>(
            () => _service.CreateAsync(dto));

        ex.Message.Should().Be("You already have a reservation for this deal at this date and time.");
    }

    [TestMethod]
    public async Task CreateAsync_ShouldAllowSameCustomerToRebookAfterCancellation()
    {
        SetCurrentUser(50, isAdmin: false);

        var deal = await SeedDealAsync();

        var dto = BuildCreateReservationDto(deal.Id);

        var first = await _service.CreateAsync(dto);

        await _service.CancelMyReservationAsync(first.Id, userId: 50);

        var second = await _service.CreateAsync(dto);

        second.Should().NotBeNull();

        var reservations = _context.Reservations.ToList();

        reservations.Should().HaveCount(2);
        reservations.Single(r => r.Id == first.Id).Status.Should().Be(ReservationStatus.Cancelled);
        reservations.Single(r => r.Id == second.Id).Status.Should().Be(ReservationStatus.Pending);

        var redemptions = _context.Redemptions.ToList();

        redemptions.Should().HaveCount(2);
        redemptions.Single(r => r.ReservationId == first.Id).Status.Should().Be(RedemptionStatus.Cancelled);
        redemptions.Single(r => r.ReservationId == second.Id).Status.Should().Be(RedemptionStatus.Redeemed);
    }

    [TestMethod]
    public async Task CreateAsync_ShouldAllowSameCustomerToRebookAfterNoShow()
    {
        var deal = await SeedDealAsync();

        SetCurrentUser(50, isAdmin: false);
        var dto = BuildCreateReservationDto(deal.Id);
        var first = await _service.CreateAsync(dto);

        SetAdmin();
        await _service.NoShowReservationAsync(first.Id);

        SetCurrentUser(50, isAdmin: false);
        var second = await _service.CreateAsync(dto);

        second.Should().NotBeNull();

        var reservations = _context.Reservations.ToList();

        reservations.Should().HaveCount(2);
        reservations.Single(r => r.Id == first.Id).Status.Should().Be(ReservationStatus.NoShow);
        reservations.Single(r => r.Id == second.Id).Status.Should().Be(ReservationStatus.Pending);
    }

    [TestMethod]
    public async Task CreateAsync_ShouldAllowSameCustomerToRebookAfterRejection()
    {
        // Rejected is no longer reachable via RejectReservationAsync (the
        // workflow was disabled in Phase 5N), but legacy data could still
        // carry this status, so the new active-only uniqueness rule must
        // still exclude it and allow rebooking. Seeded directly since the
        // service itself can no longer produce this status.
        SetCurrentUser(50, isAdmin: false);

        var deal = await SeedDealAsync();

        var dto = BuildCreateReservationDto(deal.Id);

        _context.Reservations.Add(new Reservation
        {
            DealId = deal.Id,
            UserId = 50,
            CustomerName = dto.CustomerName,
            PhoneNumber = dto.PhoneNumber,
            Email = dto.Email,
            ReservationDate = dto.ReservationDate,
            ReservationTime = dto.ReservationTime,
            GuestCount = dto.GuestCount,
            Status = ReservationStatus.Rejected
        });

        await _context.SaveChangesAsync();

        var result = await _service.CreateAsync(dto);

        result.Should().NotBeNull();
        _context.Reservations.Count().Should().Be(2);
    }

    [TestMethod]
    public async Task CreateAsync_ShouldAllowDifferentCustomerForSameDealAndSlot()
    {
        var deal = await SeedDealAsync();

        var dto = BuildCreateReservationDto(deal.Id);

        SetCurrentUser(50, isAdmin: false);
        var resultA = await _service.CreateAsync(dto);

        SetCurrentUser(60, isAdmin: false);
        var resultB = await _service.CreateAsync(dto);

        resultA.Should().NotBeNull();
        resultB.Should().NotBeNull();

        _context.Reservations.Count().Should().Be(2);
    }

    [TestMethod]
    public async Task CreateAsync_ShouldRejectDuplicateRegardlessOfGuestCount()
    {
        SetCurrentUser(50, isAdmin: false);

        var deal = await SeedDealAsync(maximumGuests: 6);

        var dtoA = BuildCreateReservationDto(deal.Id, guestCount: 2);

        await _service.CreateAsync(dtoA);

        var dtoB = BuildCreateReservationDto(deal.Id, guestCount: 4);

        var ex = await Assert.ThrowsExceptionAsync<BusinessRuleException>(
            () => _service.CreateAsync(dtoB));

        ex.Message.Should().Be("You already have a reservation for this deal at this date and time.");
    }

    [TestMethod]
    public async Task CreateAsync_ShouldAllowSameCustomerForDifferentTime()
    {
        SetCurrentUser(50, isAdmin: false);

        var deal = await SeedDealAsync();

        var dtoX = BuildCreateReservationDto(
            deal.Id,
            reservationTime: new TimeOnly(12, 30));

        var dtoY = BuildCreateReservationDto(
            deal.Id,
            reservationTime: new TimeOnly(13, 30));

        var resultX = await _service.CreateAsync(dtoX);
        var resultY = await _service.CreateAsync(dtoY);

        resultX.Should().NotBeNull();
        resultY.Should().NotBeNull();

        _context.Reservations.Count().Should().Be(2);
    }
}
