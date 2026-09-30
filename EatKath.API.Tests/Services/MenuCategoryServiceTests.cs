using AutoMapper;
using EatKath.API.Data;
using EatKath.API.DTOs.MenuCategory;
using EatKath.API.Entities;
using EatKath.API.Exceptions;
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
public class MenuCategoryServiceTests
{
    private ApplicationDbContext _context = null!;
    private IMapper _mapper = null!;
    private Mock<IValidator<CreateMenuCategoryDto>> _createValidator = null!;
    private Mock<IValidator<UpdateMenuCategoryDto>> _updateValidator = null!;
    private Mock<ICurrentUserService> _currentUser = null!;
    private MenuCategoryService _service = null!;

    [TestInitialize]
    public void Setup()
    {
        _context = TestDbContextFactory.Create();

        _mapper = MapperFactory.Create();

        _createValidator = new Mock<IValidator<CreateMenuCategoryDto>>();
        _updateValidator = new Mock<IValidator<UpdateMenuCategoryDto>>();

        _createValidator
            .Setup(v => v.ValidateAsync(It.IsAny<CreateMenuCategoryDto>(), default))
            .ReturnsAsync(new ValidationResult());

        _updateValidator
            .Setup(v => v.ValidateAsync(It.IsAny<UpdateMenuCategoryDto>(), default))
            .ReturnsAsync(new ValidationResult());

        _currentUser = new Mock<ICurrentUserService>();

        // Default: an Admin caller, so tests that don't
        // care about ownership continue to behave predictably.
        _currentUser.Setup(x => x.UserId).Returns(1);
        _currentUser.Setup(x => x.IsAdmin).Returns(true);

        _service = new MenuCategoryService(
            _context,
            _mapper,
            _createValidator.Object,
            _updateValidator.Object,
            _currentUser.Object);
    }

    private static CreateMenuCategoryDto BuildCreateDto(
        int restaurantId,
        string name = "Starters",
        int displayOrder = 1) => new()
    {
        RestaurantId = restaurantId,
        Name = name,
        DisplayOrder = displayOrder
    };

    private static UpdateMenuCategoryDto BuildUpdateDto(
        string name = "Updated Category",
        int displayOrder = 2) => new()
    {
        Name = name,
        DisplayOrder = displayOrder
    };


    // -----------------------------
    // CreateAsync
    // -----------------------------

    [TestMethod]
    public async Task CreateAsync_ShouldSucceed_WhenOwnerOwnsRestaurant()
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

        var dto = BuildCreateDto(restaurant.Id);

        // -----------------------------
        // Act
        // -----------------------------

        var result = await _service.CreateAsync(dto);

        // -----------------------------
        // Assert
        // -----------------------------

        result.Should().NotBeNull();
        _context.MenuCategories.Count().Should().Be(1);
    }

    [TestMethod]
    public async Task CreateAsync_ShouldThrowBusinessRuleException_WhenRestaurantDoesNotExist()
    {
        // -----------------------------
        // Arrange
        // Purpose: preserve existing "not found" behavior.
        // -----------------------------

        var dto = BuildCreateDto(9999);

        // -----------------------------
        // Act & Assert
        // -----------------------------

        var ex = await Assert.ThrowsExceptionAsync<BusinessRuleException>(
            () => _service.CreateAsync(dto));

        ex.Message.Should().Be("Restaurant not found.");
    }

    [TestMethod]
    public async Task CreateAsync_ShouldThrowBusinessRuleException_WhenOwnerDoesNotOwnRestaurant()
    {
        // -----------------------------
        // Arrange
        // Purpose:
        // Verify the corrected, non-leaking message is used -
        // it must not expose OwnerId/UserId/IsAdmin.
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

        ex.Message.Should().Be("You are not authorized to modify this restaurant.");

        _context.MenuCategories.Count().Should().Be(0);
    }

    [TestMethod]
    public async Task CreateAsync_ShouldThrowDuplicateEntityException_WhenNameAlreadyExistsForRestaurant()
    {
        // -----------------------------
        // Arrange
        // Purpose:
        // Verify the existing case-insensitive, trimmed
        // duplicate-name comparison.
        // -----------------------------

        var restaurant = new Restaurant
        {
            Name = "Spice Kitchen",
            OwnerId = 1,
            IsActive = true
        };

        _context.Restaurants.Add(restaurant);
        await _context.SaveChangesAsync();

        _context.MenuCategories.Add(new MenuCategory
        {
            RestaurantId = restaurant.Id,
            Name = "Starters",
            DisplayOrder = 1
        });

        await _context.SaveChangesAsync();

        var dto = BuildCreateDto(restaurant.Id, name: "  STARTERS  ");

        // -----------------------------
        // Act & Assert
        // -----------------------------

        var ex = await Assert.ThrowsExceptionAsync<DuplicateEntityException>(
            () => _service.CreateAsync(dto));

        ex.Message.Should().Be("Menu category already exists.");

        _context.MenuCategories.Count().Should().Be(1);
    }

    [TestMethod]
    public async Task CreateAsync_ShouldSucceed_WhenUserIsAdmin_EvenIfNotOwner()
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

        var dto = BuildCreateDto(restaurant.Id);

        // -----------------------------
        // Act
        // -----------------------------

        var result = await _service.CreateAsync(dto);

        // -----------------------------
        // Assert
        // -----------------------------

        result.Should().NotBeNull();
        _context.MenuCategories.Count().Should().Be(1);
    }


    // -----------------------------
    // UpdateAsync
    // -----------------------------

    [TestMethod]
    public async Task UpdateAsync_ShouldSucceed_WhenOwnerOwnsRestaurant()
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

        var entity = new MenuCategory
        {
            RestaurantId = restaurant.Id,
            Name = "Starters",
            DisplayOrder = 1
        };

        _context.MenuCategories.Add(entity);
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
        result!.Name.Should().Be(dto.Name);
        result.DisplayOrder.Should().Be(dto.DisplayOrder);
    }

    [TestMethod]
    public async Task UpdateAsync_ShouldReturnNull_WhenEntityDoesNotExist()
    {
        // -----------------------------
        // Arrange
        // Purpose: preserve existing "not found" behavior.
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
    public async Task UpdateAsync_ShouldThrowBusinessRuleException_WhenOwnerDoesNotOwnRestaurant()
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

        var entity = new MenuCategory
        {
            RestaurantId = restaurant.Id,
            Name = "Starters",
            DisplayOrder = 1
        };

        _context.MenuCategories.Add(entity);
        await _context.SaveChangesAsync();

        var dto = BuildUpdateDto();

        // -----------------------------
        // Act & Assert
        // -----------------------------

        var ex = await Assert.ThrowsExceptionAsync<BusinessRuleException>(
            () => _service.UpdateAsync(entity.Id, dto));

        ex.Message.Should().Be("You are not authorized to modify this restaurant.");

        _context.MenuCategories.First().Name.Should().Be("Starters");
    }

    [TestMethod]
    public async Task UpdateAsync_ShouldThrowDuplicateEntityException_WhenNameAlreadyExistsForAnotherCategoryInSameRestaurant()
    {
        // -----------------------------
        // Arrange
        // Purpose:
        // Verify the update duplicate-check excludes the entity
        // being updated but still catches a collision (case-
        // insensitive, trimmed) with a different category.
        // -----------------------------

        var restaurant = new Restaurant
        {
            Name = "Spice Kitchen",
            OwnerId = 1,
            IsActive = true
        };

        _context.Restaurants.Add(restaurant);
        await _context.SaveChangesAsync();

        var existing = new MenuCategory
        {
            RestaurantId = restaurant.Id,
            Name = "Mains",
            DisplayOrder = 1
        };

        var toUpdate = new MenuCategory
        {
            RestaurantId = restaurant.Id,
            Name = "Starters",
            DisplayOrder = 2
        };

        _context.MenuCategories.Add(existing);
        _context.MenuCategories.Add(toUpdate);
        await _context.SaveChangesAsync();

        var dto = BuildUpdateDto(name: "  mains  ");

        // -----------------------------
        // Act & Assert
        // -----------------------------

        var ex = await Assert.ThrowsExceptionAsync<DuplicateEntityException>(
            () => _service.UpdateAsync(toUpdate.Id, dto));

        ex.Message.Should().Be("Menu category already exists.");

        _context.MenuCategories.First(c => c.Id == toUpdate.Id).Name.Should().Be("Starters");
    }

    [TestMethod]
    public async Task UpdateAsync_ShouldSucceed_WhenUserIsAdmin_EvenIfNotOwner()
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

        var entity = new MenuCategory
        {
            RestaurantId = restaurant.Id,
            Name = "Starters",
            DisplayOrder = 1
        };

        _context.MenuCategories.Add(entity);
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
        result!.Name.Should().Be(dto.Name);
    }


    // -----------------------------
    // DeleteAsync
    // -----------------------------

    [TestMethod]
    public async Task DeleteAsync_ShouldSucceed_WhenOwnerOwnsRestaurantAndHasNoMenuItems()
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

        var entity = new MenuCategory
        {
            RestaurantId = restaurant.Id,
            Name = "Starters",
            DisplayOrder = 1
        };

        _context.MenuCategories.Add(entity);
        await _context.SaveChangesAsync();

        // -----------------------------
        // Act
        // -----------------------------

        var result = await _service.DeleteAsync(entity.Id);

        // -----------------------------
        // Assert
        // -----------------------------

        result.Should().BeTrue();
        _context.MenuCategories.Count().Should().Be(0);
    }

    [TestMethod]
    public async Task DeleteAsync_ShouldReturnFalse_WhenEntityDoesNotExist()
    {
        // -----------------------------
        // Arrange
        // Purpose: preserve existing "not found" behavior.
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

        var entity = new MenuCategory
        {
            RestaurantId = restaurant.Id,
            Name = "Starters",
            DisplayOrder = 1
        };

        _context.MenuCategories.Add(entity);
        await _context.SaveChangesAsync();

        // -----------------------------
        // Act & Assert
        // -----------------------------

        var ex = await Assert.ThrowsExceptionAsync<BusinessRuleException>(
            () => _service.DeleteAsync(entity.Id));

        ex.Message.Should().Be("You are not authorized to delete this menu category.");

        _context.MenuCategories.Count().Should().Be(1);
    }

    [TestMethod]
    public async Task DeleteAsync_ShouldThrowBusinessRuleException_WhenCategoryHasMenuItems()
    {
        // -----------------------------
        // Arrange
        // -----------------------------

        var restaurant = new Restaurant
        {
            Name = "Spice Kitchen",
            OwnerId = 1,
            IsActive = true
        };

        _context.Restaurants.Add(restaurant);
        await _context.SaveChangesAsync();

        var entity = new MenuCategory
        {
            RestaurantId = restaurant.Id,
            Name = "Starters",
            DisplayOrder = 1
        };

        _context.MenuCategories.Add(entity);
        await _context.SaveChangesAsync();

        _context.MenuItems.Add(new MenuItem
        {
            RestaurantId = restaurant.Id,
            MenuCategoryId = entity.Id,
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
            () => _service.DeleteAsync(entity.Id));

        ex.Message.Should().Be("Cannot delete category because it contains menu items.");

        _context.MenuCategories.Count().Should().Be(1);
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

        var entity = new MenuCategory
        {
            RestaurantId = restaurant.Id,
            Name = "Starters",
            DisplayOrder = 1
        };

        _context.MenuCategories.Add(entity);
        await _context.SaveChangesAsync();

        // -----------------------------
        // Act
        // -----------------------------

        var result = await _service.DeleteAsync(entity.Id);

        // -----------------------------
        // Assert
        // -----------------------------

        result.Should().BeTrue();
        _context.MenuCategories.Count().Should().Be(0);
    }


    // -----------------------------
    // GetByRestaurantAsync
    // -----------------------------

    [TestMethod]
    public async Task GetByRestaurantAsync_ShouldReturnOnlyThatRestaurantsCategories_OrderedByDisplayOrder()
    {
        // -----------------------------
        // Arrange
        // -----------------------------

        var restaurantA = new Restaurant
        {
            Name = "Spice Kitchen",
            OwnerId = 1,
            IsActive = true
        };

        var restaurantB = new Restaurant
        {
            Name = "Ocean Grill",
            OwnerId = 2,
            IsActive = true
        };

        _context.Restaurants.Add(restaurantA);
        _context.Restaurants.Add(restaurantB);
        await _context.SaveChangesAsync();

        _context.MenuCategories.Add(new MenuCategory
        {
            RestaurantId = restaurantA.Id,
            Name = "Desserts",
            DisplayOrder = 3
        });

        _context.MenuCategories.Add(new MenuCategory
        {
            RestaurantId = restaurantA.Id,
            Name = "Starters",
            DisplayOrder = 1
        });

        _context.MenuCategories.Add(new MenuCategory
        {
            RestaurantId = restaurantA.Id,
            Name = "Mains",
            DisplayOrder = 2
        });

        _context.MenuCategories.Add(new MenuCategory
        {
            RestaurantId = restaurantB.Id,
            Name = "Drinks",
            DisplayOrder = 1
        });

        await _context.SaveChangesAsync();

        // -----------------------------
        // Act
        // -----------------------------

        var result = (await _service.GetByRestaurantAsync(restaurantA.Id)).ToList();

        // -----------------------------
        // Assert
        // -----------------------------

        result.Should().HaveCount(3);
        result.Should().OnlyContain(c => c.RestaurantId == restaurantA.Id);
        result[0].Name.Should().Be("Starters");
        result[1].Name.Should().Be("Mains");
        result[2].Name.Should().Be("Desserts");
    }
}
