using AutoMapper;
using EatKath.API.Data;
using EatKath.API.Entities;
using EatKath.API.Enums;
using EatKath.API.Services;
using EatKath.API.Tests.Helpers;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EatKath.API.Tests.Services;

// -----------------------------
// Scoped strictly to DeleteAsync for this phase - CreateAsync,
// UpdateAsync, GetAllAsync and GetByIdAsync are intentionally
// left untested here.
// -----------------------------
[TestClass]
public class UserServiceTests
{
    private ApplicationDbContext _context = null!;
    private IMapper _mapper = null!;
    private UserService _service = null!;

    [TestInitialize]
    public void Setup()
    {
        _context = TestDbContextFactory.Create();

        _mapper = MapperFactory.Create();

        _service = new UserService(_context, _mapper);
    }

    private async Task<User> SeedUserAsync()
    {
        var user = new User
        {
            FirstName = "Jane",
            LastName = "Diner",
            Email = "jane.diner@test.com",
            PasswordHash = "hash",
            PhoneNumber = "0400111222",
            RoleId = 1,
            IsActive = true
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return user;
    }


    // -----------------------------
    // DeleteAsync
    // -----------------------------

    [TestMethod]
    public async Task DeleteAsync_ShouldReturnFalse_WhenUserDoesNotExist()
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
    public async Task DeleteAsync_ShouldSucceed_WhenUserHasNoDependentRecords()
    {
        // -----------------------------
        // Arrange
        // -----------------------------

        var user = await SeedUserAsync();

        // -----------------------------
        // Act
        // -----------------------------

        var result = await _service.DeleteAsync(user.Id);

        // -----------------------------
        // Assert
        // -----------------------------

        result.Should().BeTrue();
        _context.Users.Count().Should().Be(0);
    }

    [TestMethod]
    public async Task DeleteAsync_ShouldThrowBusinessRuleException_WhenUserHasReservations()
    {
        // -----------------------------
        // Arrange
        // -----------------------------

        var user = await SeedUserAsync();

        _context.Reservations.Add(new Reservation
        {
            DealId = 1,
            UserId = user.Id,
            CustomerName = "Jane Diner",
            PhoneNumber = "0400111222",
            ReservationDate = DateOnly.FromDateTime(DateTime.Today),
            ReservationTime = new TimeOnly(12, 30),
            GuestCount = 2,
            Status = ReservationStatus.Pending
        });

        await _context.SaveChangesAsync();

        // -----------------------------
        // Act & Assert
        // -----------------------------

        var ex = await Assert.ThrowsExceptionAsync<BusinessRuleException>(
            () => _service.DeleteAsync(user.Id));

        ex.Message.Should().Be("Cannot delete this user because they have existing reservations, redemptions, or restaurants.");

        _context.Users.Count().Should().Be(1);
    }

    [TestMethod]
    public async Task DeleteAsync_ShouldThrowBusinessRuleException_WhenUserHasRedemptions()
    {
        // -----------------------------
        // Arrange
        // -----------------------------

        var user = await SeedUserAsync();

        _context.Redemptions.Add(new Redemption
        {
            DealId = 1,
            UserId = user.Id,
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
            () => _service.DeleteAsync(user.Id));

        ex.Message.Should().Be("Cannot delete this user because they have existing reservations, redemptions, or restaurants.");

        _context.Users.Count().Should().Be(1);
    }

    [TestMethod]
    public async Task DeleteAsync_ShouldThrowBusinessRuleException_WhenUserOwnsRestaurant()
    {
        // -----------------------------
        // Arrange
        // -----------------------------

        var user = await SeedUserAsync();

        _context.Restaurants.Add(new Restaurant
        {
            Name = "Spice Kitchen",
            OwnerId = user.Id,
            IsActive = true
        });

        await _context.SaveChangesAsync();

        // -----------------------------
        // Act & Assert
        // -----------------------------

        var ex = await Assert.ThrowsExceptionAsync<BusinessRuleException>(
            () => _service.DeleteAsync(user.Id));

        ex.Message.Should().Be("Cannot delete this user because they have existing reservations, redemptions, or restaurants.");

        _context.Users.Count().Should().Be(1);
    }
}
