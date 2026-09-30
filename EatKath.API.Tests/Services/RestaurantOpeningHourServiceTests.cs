using AutoMapper;
using EatKath.API.Data;
using EatKath.API.DTOs.RestaurantOpeningHour;
using EatKath.API.Entities;
using EatKath.API.Interfaces;
using EatKath.API.Services;
using EatKath.API.Tests.Helpers;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace EatKath.API.Tests.Services;

[TestClass]
public class RestaurantOpeningHourServiceTests
{
    private ApplicationDbContext _context = null!;
    private IMapper _mapper = null!;
    private Mock<IValidator<CreateRestaurantOpeningHourDto>> _createValidator = null!;
    private Mock<IValidator<UpdateRestaurantOpeningHourDto>> _updateValidator = null!;
    private Mock<ICurrentUserService> _currentUser = null!;
    private RestaurantOpeningHourService _service = null!;

    [TestInitialize]
    public void Setup()
    {
        _context = TestDbContextFactory.Create();

        _mapper = MapperFactory.Create();

        _createValidator = new Mock<IValidator<CreateRestaurantOpeningHourDto>>();
        _updateValidator = new Mock<IValidator<UpdateRestaurantOpeningHourDto>>();

        _createValidator
            .Setup(v => v.ValidateAsync(It.IsAny<CreateRestaurantOpeningHourDto>(), default))
            .ReturnsAsync(new ValidationResult());

        _updateValidator
            .Setup(v => v.ValidateAsync(It.IsAny<UpdateRestaurantOpeningHourDto>(), default))
            .ReturnsAsync(new ValidationResult());

        _currentUser = new Mock<ICurrentUserService>();

        // Default: an Admin caller, so tests that don't
        // care about ownership continue to behave predictably.
        _currentUser.Setup(x => x.UserId).Returns(1);
        _currentUser.Setup(x => x.IsAdmin).Returns(true);

        _service = new RestaurantOpeningHourService(
            _context,
            _mapper,
            _createValidator.Object,
            _updateValidator.Object,
            _currentUser.Object);
    }

    private static CreateRestaurantOpeningHourDto BuildCreateDto(int restaurantId) => new()
    {
        RestaurantId = restaurantId,
        DayOfWeek = DayOfWeek.Monday,
        OpenTime = new TimeOnly(9, 0),
        CloseTime = new TimeOnly(17, 0),
        IsClosed = false
    };

    private static UpdateRestaurantOpeningHourDto BuildUpdateDto() => new()
    {
        DayOfWeek = DayOfWeek.Tuesday,
        OpenTime = new TimeOnly(10, 0),
        CloseTime = new TimeOnly(18, 0),
        IsClosed = false
    };


    // -----------------------------
    // CreateAsync
    // -----------------------------

    [TestMethod]
    public async Task CreateAsync_ShouldSucceed_WhenOwnerOwnsRestaurant()
    {
        // -----------------------------
        // Arrange
        // Purpose:
        // An Owner who owns the restaurant should be
        // allowed to add opening hours for it.
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

        var dto = BuildCreateDto(restaurant.Id);

        // -----------------------------
        // Act
        // -----------------------------

        var result = await _service.CreateAsync(dto);

        // -----------------------------
        // Assert
        // -----------------------------

        result.Should().NotBeNull();
        _context.RestaurantOpeningHours.Count().Should().Be(1);
    }

    [TestMethod]
    public async Task CreateAsync_ShouldThrowBusinessRuleException_WhenOwnerDoesNotOwnRestaurant()
    {
        // -----------------------------
        // Arrange
        // Purpose:
        // An Owner must not be able to add opening
        // hours to a restaurant they don't own.
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

        var dto = BuildCreateDto(restaurant.Id);

        // -----------------------------
        // Act & Assert
        // -----------------------------

        var ex = await Assert.ThrowsExceptionAsync<BusinessRuleException>(
            () => _service.CreateAsync(dto));

        ex.Message.Should().Be("You are not authorized to add opening hours for this restaurant.");

        _context.RestaurantOpeningHours.Count().Should().Be(0);
    }

    [TestMethod]
    public async Task CreateAsync_ShouldSucceed_WhenUserIsAdmin_EvenIfNotOwner()
    {
        // -----------------------------
        // Arrange
        // Purpose:
        // An Admin should be able to add opening
        // hours to any restaurant.
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

        var dto = BuildCreateDto(restaurant.Id);

        // -----------------------------
        // Act
        // -----------------------------

        var result = await _service.CreateAsync(dto);

        // -----------------------------
        // Assert
        // -----------------------------

        result.Should().NotBeNull();
        _context.RestaurantOpeningHours.Count().Should().Be(1);
    }

    [TestMethod]
    public async Task CreateAsync_ShouldThrowException_WhenRestaurantDoesNotExist()
    {
        // -----------------------------
        // Arrange
        // Purpose:
        // Verify the existing "restaurant not found"
        // behavior is preserved.
        // -----------------------------

        var dto = BuildCreateDto(9999);

        // -----------------------------
        // Act & Assert
        // -----------------------------

        var ex = await Assert.ThrowsExceptionAsync<NotFoundException>(
            () => _service.CreateAsync(dto));

        ex.Message.Should().Be("Restaurant not found.");
    }

    [TestMethod]
    public async Task CreateAsync_ShouldThrowValidationException_WhenValidationFails()
    {
        // -----------------------------
        // Arrange
        // Purpose:
        // Verify validation failures are still
        // surfaced before any ownership check runs.
        // -----------------------------

        var dto = BuildCreateDto(1);

        var failures = new List<ValidationFailure>
        {
            new ValidationFailure("DayOfWeek", "Day of week is invalid.")
        };

        _createValidator
            .Setup(v => v.ValidateAsync(dto, default))
            .ReturnsAsync(new ValidationResult(failures));

        // -----------------------------
        // Act & Assert
        // -----------------------------

        await Assert.ThrowsExceptionAsync<ValidationException>(
            () => _service.CreateAsync(dto));

        _context.RestaurantOpeningHours.Count().Should().Be(0);
    }


    // -----------------------------
    // UpdateAsync
    // -----------------------------

    [TestMethod]
    public async Task UpdateAsync_ShouldSucceed_WhenOwnerOwnsRestaurant()
    {
        // -----------------------------
        // Arrange
        // Purpose:
        // An Owner who owns the opening hour's restaurant
        // should be allowed to update it.
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

        var entity = new RestaurantOpeningHour
        {
            RestaurantId = restaurant.Id,
            DayOfWeek = DayOfWeek.Monday,
            OpenTime = new TimeOnly(9, 0),
            CloseTime = new TimeOnly(17, 0),
            IsClosed = false
        };

        _context.RestaurantOpeningHours.Add(entity);
        await _context.SaveChangesAsync();

        var dto = BuildUpdateDto();

        // -----------------------------
        // Act
        // -----------------------------

        var result = await _service.UpdateAsync(entity.Id, dto);

        // -----------------------------
        // Assert
        // -----------------------------

        result.Should().NotBeNull();
        result!.DayOfWeek.Should().Be(DayOfWeek.Tuesday);
    }

    [TestMethod]
    public async Task UpdateAsync_ShouldThrowBusinessRuleException_WhenOwnerDoesNotOwnRestaurant()
    {
        // -----------------------------
        // Arrange
        // Purpose:
        // An Owner must not be able to update opening
        // hours belonging to another owner's restaurant.
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

        var entity = new RestaurantOpeningHour
        {
            RestaurantId = restaurant.Id,
            DayOfWeek = DayOfWeek.Monday,
            OpenTime = new TimeOnly(9, 0),
            CloseTime = new TimeOnly(17, 0),
            IsClosed = false
        };

        _context.RestaurantOpeningHours.Add(entity);
        await _context.SaveChangesAsync();

        var dto = BuildUpdateDto();

        // -----------------------------
        // Act & Assert
        // -----------------------------

        var ex = await Assert.ThrowsExceptionAsync<BusinessRuleException>(
            () => _service.UpdateAsync(entity.Id, dto));

        ex.Message.Should().Be("You are not authorized to modify this restaurant's opening hours.");

        _context.RestaurantOpeningHours.First().DayOfWeek.Should().Be(DayOfWeek.Monday);
    }

    [TestMethod]
    public async Task UpdateAsync_ShouldSucceed_WhenUserIsAdmin_EvenIfNotOwner()
    {
        // -----------------------------
        // Arrange
        // Purpose:
        // An Admin should be able to update opening
        // hours for any restaurant.
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

        var entity = new RestaurantOpeningHour
        {
            RestaurantId = restaurant.Id,
            DayOfWeek = DayOfWeek.Monday,
            OpenTime = new TimeOnly(9, 0),
            CloseTime = new TimeOnly(17, 0),
            IsClosed = false
        };

        _context.RestaurantOpeningHours.Add(entity);
        await _context.SaveChangesAsync();

        var dto = BuildUpdateDto();

        // -----------------------------
        // Act
        // -----------------------------

        var result = await _service.UpdateAsync(entity.Id, dto);

        // -----------------------------
        // Assert
        // -----------------------------

        result.Should().NotBeNull();
        result!.DayOfWeek.Should().Be(DayOfWeek.Tuesday);
    }

    [TestMethod]
    public async Task UpdateAsync_ShouldReturnNull_WhenEntityDoesNotExist()
    {
        // -----------------------------
        // Arrange
        // Purpose:
        // Verify the existing not-found behavior
        // (return null) is preserved.
        // -----------------------------

        var dto = BuildUpdateDto();

        // -----------------------------
        // Act
        // -----------------------------

        var result = await _service.UpdateAsync(9999, dto);

        // -----------------------------
        // Assert
        // -----------------------------

        result.Should().BeNull();
    }

    [TestMethod]
    public async Task UpdateAsync_ShouldThrowValidationException_WhenValidationFails()
    {
        // -----------------------------
        // Arrange
        // Purpose:
        // Verify validation failures are still
        // surfaced before any ownership check runs.
        // -----------------------------

        var dto = BuildUpdateDto();

        var failures = new List<ValidationFailure>
        {
            new ValidationFailure("OpenTime", "Open time is invalid.")
        };

        _updateValidator
            .Setup(v => v.ValidateAsync(dto, default))
            .ReturnsAsync(new ValidationResult(failures));

        // -----------------------------
        // Act & Assert
        // -----------------------------

        await Assert.ThrowsExceptionAsync<ValidationException>(
            () => _service.UpdateAsync(1, dto));
    }


    // -----------------------------
    // DeleteAsync
    // -----------------------------

    [TestMethod]
    public async Task DeleteAsync_ShouldSucceed_WhenOwnerOwnsRestaurant()
    {
        // -----------------------------
        // Arrange
        // Purpose:
        // An Owner who owns the opening hour's restaurant
        // should be allowed to delete it.
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

        var entity = new RestaurantOpeningHour
        {
            RestaurantId = restaurant.Id,
            DayOfWeek = DayOfWeek.Monday,
            OpenTime = new TimeOnly(9, 0),
            CloseTime = new TimeOnly(17, 0),
            IsClosed = false
        };

        _context.RestaurantOpeningHours.Add(entity);
        await _context.SaveChangesAsync();

        // -----------------------------
        // Act
        // -----------------------------

        var result = await _service.DeleteAsync(entity.Id);

        // -----------------------------
        // Assert
        // -----------------------------

        result.Should().BeTrue();
        _context.RestaurantOpeningHours.Count().Should().Be(0);
    }

    [TestMethod]
    public async Task DeleteAsync_ShouldThrowBusinessRuleException_WhenOwnerDoesNotOwnRestaurant()
    {
        // -----------------------------
        // Arrange
        // Purpose:
        // An Owner must not be able to delete opening
        // hours belonging to another owner's restaurant.
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

        var entity = new RestaurantOpeningHour
        {
            RestaurantId = restaurant.Id,
            DayOfWeek = DayOfWeek.Monday,
            OpenTime = new TimeOnly(9, 0),
            CloseTime = new TimeOnly(17, 0),
            IsClosed = false
        };

        _context.RestaurantOpeningHours.Add(entity);
        await _context.SaveChangesAsync();

        // -----------------------------
        // Act & Assert
        // -----------------------------

        var ex = await Assert.ThrowsExceptionAsync<BusinessRuleException>(
            () => _service.DeleteAsync(entity.Id));

        ex.Message.Should().Be("You are not authorized to delete this restaurant's opening hours.");

        _context.RestaurantOpeningHours.Count().Should().Be(1);
    }

    [TestMethod]
    public async Task DeleteAsync_ShouldSucceed_WhenUserIsAdmin_EvenIfNotOwner()
    {
        // -----------------------------
        // Arrange
        // Purpose:
        // An Admin should be able to delete opening
        // hours for any restaurant.
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

        var entity = new RestaurantOpeningHour
        {
            RestaurantId = restaurant.Id,
            DayOfWeek = DayOfWeek.Monday,
            OpenTime = new TimeOnly(9, 0),
            CloseTime = new TimeOnly(17, 0),
            IsClosed = false
        };

        _context.RestaurantOpeningHours.Add(entity);
        await _context.SaveChangesAsync();

        // -----------------------------
        // Act
        // -----------------------------

        var result = await _service.DeleteAsync(entity.Id);

        // -----------------------------
        // Assert
        // -----------------------------

        result.Should().BeTrue();
        _context.RestaurantOpeningHours.Count().Should().Be(0);
    }

    [TestMethod]
    public async Task DeleteAsync_ShouldReturnFalse_WhenEntityDoesNotExist()
    {
        // -----------------------------
        // Arrange
        // Purpose:
        // Verify the existing not-found behavior
        // (return false) is preserved.
        // -----------------------------

        // -----------------------------
        // Act
        // -----------------------------

        var result = await _service.DeleteAsync(9999);

        // -----------------------------
        // Assert
        // -----------------------------

        result.Should().BeFalse();
    }
}
