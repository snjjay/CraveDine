using AutoMapper;
using EatKath.API.Data;
using EatKath.API.DTOs.User;
using EatKath.API.Entities;
using EatKath.API.Enums;
using EatKath.API.Interfaces;
using EatKath.API.Services;
using EatKath.API.Tests.Helpers;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace EatKath.API.Tests.Services;

// -----------------------------
// DeleteAsync's dependency-record tests predate the Admin
// self-protection/last-active-admin safeguards added alongside the
// Admin User Management feature. CreateAsync and GetAllAsync/
// GetByIdAsync remain untested here.
// -----------------------------
[TestClass]
public class UserServiceTests
{
    private ApplicationDbContext _context = null!;
    private IMapper _mapper = null!;
    private Mock<ICurrentUserService> _currentUser = null!;
    private UserService _service = null!;

    [TestInitialize]
    public void Setup()
    {
        _context = TestDbContextFactory.Create();

        _mapper = MapperFactory.Create();

        _currentUser = new Mock<ICurrentUserService>();

        // A distinct default so the existing DeleteAsync tests below
        // (which never act on themselves) don't accidentally trip the
        // new self-delete guard.
        _currentUser.Setup(x => x.UserId).Returns(999);

        _service = new UserService(_context, _mapper, _currentUser.Object);
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

    private async Task<Role> SeedRoleAsync(string name)
    {
        var role = new Role { Name = name };

        _context.Roles.Add(role);
        await _context.SaveChangesAsync();

        return role;
    }

    private async Task<User> SeedUserWithRoleAsync(
        int roleId,
        string email,
        bool isActive = true)
    {
        var user = new User
        {
            FirstName = "Test",
            LastName = "User",
            Email = email,
            PasswordHash = "hash",
            PhoneNumber = "0400000000",
            RoleId = roleId,
            IsActive = isActive
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


    // -----------------------------
    // UpdateAsync / DeleteAsync - Admin self-protection and
    // last-active-admin safeguards
    // -----------------------------

    [TestMethod]
    public async Task UpdateAsync_ShouldSucceed_WhenAdminUpdatesAnotherUsersDetails()
    {
        var adminRole = await SeedRoleAsync("Admin");
        var customerRole = await SeedRoleAsync("Customer");

        var actingAdmin = await SeedUserWithRoleAsync(adminRole.Id, "admin1@test.com");
        var targetUser = await SeedUserWithRoleAsync(customerRole.Id, "customer1@test.com");

        _currentUser.Setup(x => x.UserId).Returns(actingAdmin.Id);

        var dto = new UpdateUserDto
        {
            FirstName = "Updated",
            LastName = targetUser.LastName,
            Email = targetUser.Email,
            PhoneNumber = targetUser.PhoneNumber,
            RoleId = customerRole.Id,
            IsActive = true
        };

        var result = await _service.UpdateAsync(targetUser.Id, dto);

        result.Should().NotBeNull();
        result!.FirstName.Should().Be("Updated");
    }

    [TestMethod]
    public async Task UpdateAsync_ShouldSucceed_WhenAdminChangesAnotherUsersRole()
    {
        var adminRole = await SeedRoleAsync("Admin");
        var ownerRole = await SeedRoleAsync("Owner");
        var customerRole = await SeedRoleAsync("Customer");

        var actingAdmin = await SeedUserWithRoleAsync(adminRole.Id, "admin1@test.com");
        var targetUser = await SeedUserWithRoleAsync(customerRole.Id, "customer1@test.com");

        _currentUser.Setup(x => x.UserId).Returns(actingAdmin.Id);

        var dto = new UpdateUserDto
        {
            FirstName = targetUser.FirstName,
            LastName = targetUser.LastName,
            Email = targetUser.Email,
            PhoneNumber = targetUser.PhoneNumber,
            RoleId = ownerRole.Id,
            IsActive = true
        };

        var result = await _service.UpdateAsync(targetUser.Id, dto);

        result.Should().NotBeNull();
        result!.RoleId.Should().Be(ownerRole.Id);
    }

    [TestMethod]
    public async Task UpdateAsync_ShouldSucceed_WhenAdminDeactivatesAnotherUser()
    {
        var adminRole = await SeedRoleAsync("Admin");
        var customerRole = await SeedRoleAsync("Customer");

        var actingAdmin = await SeedUserWithRoleAsync(adminRole.Id, "admin1@test.com");
        var targetUser = await SeedUserWithRoleAsync(customerRole.Id, "customer1@test.com");

        _currentUser.Setup(x => x.UserId).Returns(actingAdmin.Id);

        var dto = new UpdateUserDto
        {
            FirstName = targetUser.FirstName,
            LastName = targetUser.LastName,
            Email = targetUser.Email,
            PhoneNumber = targetUser.PhoneNumber,
            RoleId = customerRole.Id,
            IsActive = false
        };

        var result = await _service.UpdateAsync(targetUser.Id, dto);

        result.Should().NotBeNull();
        result!.IsActive.Should().BeFalse();
    }

    [TestMethod]
    public async Task UpdateAsync_ShouldThrowBusinessRuleException_WhenAdminDeactivatesThemselves()
    {
        var adminRole = await SeedRoleAsync("Admin");

        // Two active admins so this test isolates the SELF rule from
        // the separate last-active-admin rule.
        var actingAdmin = await SeedUserWithRoleAsync(adminRole.Id, "admin1@test.com");
        await SeedUserWithRoleAsync(adminRole.Id, "admin2@test.com");

        _currentUser.Setup(x => x.UserId).Returns(actingAdmin.Id);

        var dto = new UpdateUserDto
        {
            FirstName = actingAdmin.FirstName,
            LastName = actingAdmin.LastName,
            Email = actingAdmin.Email,
            PhoneNumber = actingAdmin.PhoneNumber,
            RoleId = adminRole.Id,
            IsActive = false
        };

        var ex = await Assert.ThrowsExceptionAsync<BusinessRuleException>(
            () => _service.UpdateAsync(actingAdmin.Id, dto));

        ex.Message.Should().Be("You cannot deactivate your own account.");

        _context.Users.Single(u => u.Id == actingAdmin.Id).IsActive.Should().BeTrue();
    }

    [TestMethod]
    public async Task UpdateAsync_ShouldThrowBusinessRuleException_WhenAdminChangesOwnRoleAwayFromAdmin()
    {
        var adminRole = await SeedRoleAsync("Admin");
        var customerRole = await SeedRoleAsync("Customer");

        // Two active admins so this test isolates the SELF rule from
        // the separate last-active-admin rule.
        var actingAdmin = await SeedUserWithRoleAsync(adminRole.Id, "admin1@test.com");
        await SeedUserWithRoleAsync(adminRole.Id, "admin2@test.com");

        _currentUser.Setup(x => x.UserId).Returns(actingAdmin.Id);

        var dto = new UpdateUserDto
        {
            FirstName = actingAdmin.FirstName,
            LastName = actingAdmin.LastName,
            Email = actingAdmin.Email,
            PhoneNumber = actingAdmin.PhoneNumber,
            RoleId = customerRole.Id,
            IsActive = true
        };

        var ex = await Assert.ThrowsExceptionAsync<BusinessRuleException>(
            () => _service.UpdateAsync(actingAdmin.Id, dto));

        ex.Message.Should().Be("You cannot change your own role away from Admin.");

        _context.Users.Single(u => u.Id == actingAdmin.Id).RoleId.Should().Be(adminRole.Id);
    }

    [TestMethod]
    public async Task DeleteAsync_ShouldThrowBusinessRuleException_WhenAdminDeletesThemselves()
    {
        var adminRole = await SeedRoleAsync("Admin");

        // Two active admins so this test isolates the SELF rule from
        // the separate last-active-admin rule.
        var actingAdmin = await SeedUserWithRoleAsync(adminRole.Id, "admin1@test.com");
        await SeedUserWithRoleAsync(adminRole.Id, "admin2@test.com");

        _currentUser.Setup(x => x.UserId).Returns(actingAdmin.Id);

        var ex = await Assert.ThrowsExceptionAsync<BusinessRuleException>(
            () => _service.DeleteAsync(actingAdmin.Id));

        ex.Message.Should().Be("You cannot delete your own account.");

        _context.Users.Count().Should().Be(2);
    }

    [TestMethod]
    public async Task UpdateAsync_ShouldThrowBusinessRuleException_WhenDeactivatingLastActiveAdmin()
    {
        var adminRole = await SeedRoleAsync("Admin");

        // Only one active admin exists (the target). The acting caller
        // is a different, non-admin user id - the service itself does
        // not re-check the caller's own role (the controller already
        // restricts this action to Admins), so this isolates the
        // last-active-admin rule from the self-protection rule.
        var lastAdmin = await SeedUserWithRoleAsync(adminRole.Id, "admin1@test.com");

        _currentUser.Setup(x => x.UserId).Returns(9999);

        var dto = new UpdateUserDto
        {
            FirstName = lastAdmin.FirstName,
            LastName = lastAdmin.LastName,
            Email = lastAdmin.Email,
            PhoneNumber = lastAdmin.PhoneNumber,
            RoleId = adminRole.Id,
            IsActive = false
        };

        var ex = await Assert.ThrowsExceptionAsync<BusinessRuleException>(
            () => _service.UpdateAsync(lastAdmin.Id, dto));

        ex.Message.Should().Be("Cannot deactivate the last remaining active Admin.");

        _context.Users.Single(u => u.Id == lastAdmin.Id).IsActive.Should().BeTrue();
    }

    [TestMethod]
    public async Task UpdateAsync_ShouldThrowBusinessRuleException_WhenDemotingLastActiveAdmin()
    {
        var adminRole = await SeedRoleAsync("Admin");
        var customerRole = await SeedRoleAsync("Customer");

        var lastAdmin = await SeedUserWithRoleAsync(adminRole.Id, "admin1@test.com");

        _currentUser.Setup(x => x.UserId).Returns(9999);

        var dto = new UpdateUserDto
        {
            FirstName = lastAdmin.FirstName,
            LastName = lastAdmin.LastName,
            Email = lastAdmin.Email,
            PhoneNumber = lastAdmin.PhoneNumber,
            RoleId = customerRole.Id,
            IsActive = true
        };

        var ex = await Assert.ThrowsExceptionAsync<BusinessRuleException>(
            () => _service.UpdateAsync(lastAdmin.Id, dto));

        ex.Message.Should().Be("Cannot change the role of the last remaining active Admin.");

        _context.Users.Single(u => u.Id == lastAdmin.Id).RoleId.Should().Be(adminRole.Id);
    }

    [TestMethod]
    public async Task DeleteAsync_ShouldThrowBusinessRuleException_WhenDeletingLastActiveAdmin()
    {
        var adminRole = await SeedRoleAsync("Admin");

        var lastAdmin = await SeedUserWithRoleAsync(adminRole.Id, "admin1@test.com");

        _currentUser.Setup(x => x.UserId).Returns(9999);

        var ex = await Assert.ThrowsExceptionAsync<BusinessRuleException>(
            () => _service.DeleteAsync(lastAdmin.Id));

        ex.Message.Should().Be("Cannot delete the last remaining active Admin.");

        _context.Users.Count().Should().Be(1);
    }

    [TestMethod]
    public async Task UpdateAsync_ShouldSucceed_WhenAnotherActiveAdminRemains()
    {
        var adminRole = await SeedRoleAsync("Admin");

        var remainingAdmin = await SeedUserWithRoleAsync(adminRole.Id, "admin1@test.com");
        var targetAdmin = await SeedUserWithRoleAsync(adminRole.Id, "admin2@test.com");

        // Acting as a different caller than the target - remainingAdmin
        // stays active, so deactivating targetAdmin is safe.
        _currentUser.Setup(x => x.UserId).Returns(remainingAdmin.Id);

        var dto = new UpdateUserDto
        {
            FirstName = targetAdmin.FirstName,
            LastName = targetAdmin.LastName,
            Email = targetAdmin.Email,
            PhoneNumber = targetAdmin.PhoneNumber,
            RoleId = adminRole.Id,
            IsActive = false
        };

        var result = await _service.UpdateAsync(targetAdmin.Id, dto);

        result.Should().NotBeNull();
        result!.IsActive.Should().BeFalse();
    }
}
