using AutoMapper;
using EatKath.API.Data;
using EatKath.API.DTOs.Redemption;
using EatKath.API.Entities;
using EatKath.API.Enums;
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
public class RedemptionServiceTests
{
    private ApplicationDbContext _context = null!;
    private IMapper _mapper = null!;
    private Mock<ICurrentUserService> _currentUser = null!;
    private Mock<IValidator<CreateRedemptionDto>> _validator = null!;
    private RedemptionService _service = null!;

    [TestInitialize]
    public void Setup()
    {
        _context = TestDbContextFactory.Create();

        _mapper = MapperFactory.Create();

        _currentUser = new Mock<ICurrentUserService>();
        _validator = new Mock<IValidator<CreateRedemptionDto>>();

        _currentUser.Setup(x => x.UserId).Returns(1);

        _service = new RedemptionService(
            _context,
            _currentUser.Object,
            _mapper,
            _validator.Object);
    }

    [TestMethod]
    public async Task RedeemAsync_ShouldCreateRedemption_WhenRequestIsValid()
    {
        // -----------------------------
        // Arrange
        // Purpose:
        // Verify that a customer can
        // successfully redeem an offer.
        // -----------------------------

        _validator
            .Setup(v => v.ValidateAsync(It.IsAny<CreateRedemptionDto>(), default))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult());

        var restaurant = new Restaurant
        {
            Name = "Spice Kitchen",
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

        var user = new User
        {
            Id = 1,
            FirstName = "John",
            LastName = "Smith",
            Email = "john@test.com",
            PasswordHash = "TestPasswordHash",
            PhoneNumber = "0400000000",
            RoleId = 1,
            IsActive = true
        };

        _context.Users.Add(user);

        await _context.SaveChangesAsync();

        var dto = new CreateRedemptionDto
        {
            DealId = deal.Id,
            ArrivalDate = DateOnly.FromDateTime(DateTime.Today),
            ArrivalTime = new TimeOnly(12, 30),
            GuestCount = 2
        };

        // -----------------------------
        // Act
        // -----------------------------

        var result = await _service.RedeemAsync(dto);

        // -----------------------------
        // Assert
        // -----------------------------

        result.Should().NotBeNull();

        _context.Redemptions.Count().Should().Be(1);

        _context.Redemptions.First().GuestCount.Should().Be(2);
    }


    [TestMethod]
    public async Task RedeemAsync_ShouldThrowException_WhenOfferNotFound()
    {
        // -----------------------------
        // Arrange
        // Purpose:
        // Verify an exception is thrown
        // when the Deal does not exist.
        // -----------------------------

        _validator
            .Setup(v => v.ValidateAsync(It.IsAny<CreateRedemptionDto>(), default))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult());

        var dto = new CreateRedemptionDto
        {
            DealId = 999,
            ArrivalDate = DateOnly.FromDateTime(DateTime.Today),
            ArrivalTime = new TimeOnly(12, 30),
            GuestCount = 2
        };

        // -----------------------------
        // Act & Assert
        // -----------------------------

        var ex = await Assert.ThrowsExceptionAsync<Exception>(
            () => _service.RedeemAsync(dto));

        ex.Message.Should().Be("Offer not found.");
    }

    [TestMethod]
    public async Task RedeemAsync_ShouldThrowException_WhenOfferIsInactive()
    {
        // -----------------------------
        // Arrange
        // Purpose:
        // Verify that redemption is not
        // allowed when the offer is inactive.
        // -----------------------------

        _validator
            .Setup(v => v.ValidateAsync(It.IsAny<CreateRedemptionDto>(), default))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult());

        var restaurant = new Restaurant
        {
            Name = "Spice Kitchen",
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

            // Only change from the success test
            IsActive = false
        };

        _context.Deals.Add(deal);

        var user = new User
        {
            Id = 1,
            FirstName = "John",
            LastName = "Smith",
            Email = "john@test.com",
            PasswordHash = "Hash",
            PhoneNumber = "0400000000",
            RoleId = 1,
            IsActive = true
        };

        _context.Users.Add(user);

        await _context.SaveChangesAsync();

        var dto = new CreateRedemptionDto
        {
            DealId = deal.Id,
            ArrivalDate = DateOnly.FromDateTime(DateTime.Today),
            ArrivalTime = new TimeOnly(12, 30),
            GuestCount = 2
        };

        // -----------------------------
        // Act & Assert
        // -----------------------------

        var ex = await Assert.ThrowsExceptionAsync<Exception>(
            () => _service.RedeemAsync(dto));

        ex.Message.Should().Be("Offer is inactive.");
    }


    [TestMethod]
    public async Task RedeemAsync_ShouldThrowException_WhenArrivalDateIsOutsideOfferPeriod()
    {
        // -----------------------------
        // Arrange
        // Purpose:
        // Verify that redemption is not
        // allowed when the arrival date
        // is outside the offer period.
        // -----------------------------

        _validator
            .Setup(v => v.ValidateAsync(It.IsAny<CreateRedemptionDto>(), default))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult());

        var restaurant = new Restaurant
        {
            Name = "Spice Kitchen",
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
            EndDate = DateOnly.FromDateTime(DateTime.Today.AddDays(5)),
            StartTime = new TimeOnly(12, 0),
            EndTime = new TimeOnly(15, 0),
            MaximumGuests = 4,
            DailyRedemptionLimit = 100,
            IsActive = true
        };

        _context.Deals.Add(deal);

        _context.Users.Add(new User
        {
            Id = 1,
            FirstName = "John",
            LastName = "Smith",
            Email = "john@test.com",
            PasswordHash = "Hash",
            PhoneNumber = "0400000000",
            RoleId = 1,
            IsActive = true
        });

        await _context.SaveChangesAsync();

        var dto = new CreateRedemptionDto
        {
            DealId = deal.Id,

            // Outside offer period
            ArrivalDate = DateOnly.FromDateTime(DateTime.Today.AddDays(10)),

            ArrivalTime = new TimeOnly(12, 30),
            GuestCount = 2
        };

        // -----------------------------
        // Act & Assert
        // -----------------------------

        var ex = await Assert.ThrowsExceptionAsync<Exception>(
            () => _service.RedeemAsync(dto));

        ex.Message.Should().Be("Offer is not available on the selected arrival date.");
    }
    [TestMethod]
    public async Task RedeemAsync_ShouldThrowException_WhenArrivalTimeIsOutsideOfferTime()
    {
        // -----------------------------
        // Arrange
        // Purpose:
        // Verify redemption is rejected
        // when arrival time is outside
        // the offer time window.
        // -----------------------------

        _validator
            .Setup(v => v.ValidateAsync(It.IsAny<CreateRedemptionDto>(), default))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult());

        var restaurant = new Restaurant
        {
            Name = "Spice Kitchen",
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

        _context.Users.Add(new User
        {
            Id = 1,
            FirstName = "John",
            LastName = "Smith",
            Email = "john@test.com",
            PasswordHash = "Hash",
            PhoneNumber = "0400000000",
            RoleId = 1,
            IsActive = true
        });

        await _context.SaveChangesAsync();

        var dto = new CreateRedemptionDto
        {
            DealId = deal.Id,
            ArrivalDate = DateOnly.FromDateTime(DateTime.Today),

            // Outside offer time
            ArrivalTime = new TimeOnly(16, 0),

            GuestCount = 2
        };

        // -----------------------------
        // Act & Assert
        // -----------------------------

        var ex = await Assert.ThrowsExceptionAsync<Exception>(
            () => _service.RedeemAsync(dto));

        ex.Message.Should().Be("Arrival time must be within the offer time.");
    }


    [TestMethod]
    public async Task RedeemAsync_ShouldThrowException_WhenRestaurantIsInactive()
    {
        // -----------------------------
        // Arrange
        // Purpose:
        // Verify redemption is rejected
        // when the restaurant is inactive.
        // -----------------------------

        _validator
            .Setup(v => v.ValidateAsync(It.IsAny<CreateRedemptionDto>(), default))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult());

        var restaurant = new Restaurant
        {
            Name = "Spice Kitchen",
            IsActive = false
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

        _context.Users.Add(new User
        {
            Id = 1,
            FirstName = "John",
            LastName = "Smith",
            Email = "john@test.com",
            PasswordHash = "Hash",
            PhoneNumber = "0400000000",
            RoleId = 1,
            IsActive = true
        });

        await _context.SaveChangesAsync();

        var dto = new CreateRedemptionDto
        {
            DealId = deal.Id,
            ArrivalDate = DateOnly.FromDateTime(DateTime.Today),
            ArrivalTime = new TimeOnly(12, 30),
            GuestCount = 2
        };

        // -----------------------------
        // Act & Assert
        // -----------------------------

        var ex = await Assert.ThrowsExceptionAsync<Exception>(
            () => _service.RedeemAsync(dto));

        ex.Message.Should().Be("Restaurant is inactive.");
    }

    [TestMethod]
    public async Task RedeemAsync_ShouldThrowException_WhenGuestLimitExceeded()
    {
        // -----------------------------
        // Arrange
        // Purpose:
        // Verify redemption is rejected
        // when the guest count exceeds
        // the maximum allowed.
        // -----------------------------

        _validator
            .Setup(v => v.ValidateAsync(It.IsAny<CreateRedemptionDto>(), default))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult());

        var restaurant = new Restaurant
        {
            Name = "Spice Kitchen",
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

        _context.Users.Add(new User
        {
            Id = 1,
            FirstName = "John",
            LastName = "Smith",
            Email = "john@test.com",
            PasswordHash = "Hash",
            PhoneNumber = "0400000000",
            RoleId = 1,
            IsActive = true
        });

        await _context.SaveChangesAsync();

        var dto = new CreateRedemptionDto
        {
            DealId = deal.Id,
            ArrivalDate = DateOnly.FromDateTime(DateTime.Today),
            ArrivalTime = new TimeOnly(12, 30),

            // Exceeds MaximumGuests = 4
            GuestCount = 5
        };

        // -----------------------------
        // Act & Assert
        // -----------------------------

        var ex = await Assert.ThrowsExceptionAsync<Exception>(
            () => _service.RedeemAsync(dto));

        ex.Message.Should().Be("Maximum 4 guests allowed.");
    }

    [TestMethod]
    public async Task RedeemAsync_ShouldThrowValidationException_WhenValidationFails()
    {
        // -----------------------------
        // Arrange
        // Purpose:
        // Verify redemption is rejected
        // when request validation fails.
        // -----------------------------

        var dto = new CreateRedemptionDto
        {
            DealId = 1,
            ArrivalDate = DateOnly.FromDateTime(DateTime.Today),
            ArrivalTime = new TimeOnly(12, 30),
            GuestCount = 2
        };

        var failures = new List<ValidationFailure>
    {
        new ValidationFailure("GuestCount", "Guest count is invalid.")
    };

        _validator
            .Setup(v => v.ValidateAsync(dto, default))
            .ReturnsAsync(new ValidationResult(failures));

        // -----------------------------
        // Act & Assert
        // -----------------------------

        await Assert.ThrowsExceptionAsync<ValidationException>(
            () => _service.RedeemAsync(dto));

        _context.Redemptions.Count().Should().Be(0);
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

        var ex = await Assert.ThrowsExceptionAsync<Exception>(
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

        var ex = await Assert.ThrowsExceptionAsync<Exception>(
            () => _service.CompleteRedemptionAsync(redemption.Id, dto));

        ex.Message.Should().Be("Redemption has already been completed.");
    }


}