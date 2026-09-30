using AutoMapper;
using EatKath.API.Data;
using EatKath.API.DTOs.RestaurantImage;
using EatKath.API.Entities;
using EatKath.API.Interfaces;
using EatKath.API.Services;
using EatKath.API.Tests.Helpers;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace EatKath.API.Tests.Services;

[TestClass]
public class RestaurantImageServiceTests
{
    private ApplicationDbContext _context = null!;
    private IMapper _mapper = null!;
    private Mock<IValidator<CreateRestaurantImageDto>> _createValidator = null!;
    private Mock<IValidator<UpdateRestaurantImageDto>> _updateValidator = null!;
    private Mock<ICurrentUserService> _currentUser = null!;
    private Mock<IWebHostEnvironment> _webHostEnvironment = null!;
    private FileStorageService _fileStorage = null!;
    private string _testRootPath = null!;
    private RestaurantImageService _service = null!;

    // -----------------------------
    // FileStorageService is a concrete class with no
    // virtual members and no interface, so it cannot be
    // mocked with Moq. It is constructed for real here,
    // backed by a real (temp) IWebHostEnvironment.WebRootPath,
    // so Delete/Upload tests perform genuine file I/O against
    // a throwaway directory that TestCleanup removes.
    // -----------------------------

    [TestInitialize]
    public void Setup()
    {
        _context = TestDbContextFactory.Create();

        _mapper = MapperFactory.Create();

        _createValidator = new Mock<IValidator<CreateRestaurantImageDto>>();
        _updateValidator = new Mock<IValidator<UpdateRestaurantImageDto>>();

        _createValidator
            .Setup(v => v.ValidateAsync(It.IsAny<CreateRestaurantImageDto>(), default))
            .ReturnsAsync(new ValidationResult());

        _updateValidator
            .Setup(v => v.ValidateAsync(It.IsAny<UpdateRestaurantImageDto>(), default))
            .ReturnsAsync(new ValidationResult());

        _currentUser = new Mock<ICurrentUserService>();

        // Default: an Admin caller, so tests that don't
        // care about ownership continue to behave predictably.
        _currentUser.Setup(x => x.UserId).Returns(1);
        _currentUser.Setup(x => x.IsAdmin).Returns(true);

        _testRootPath = Path.Combine(
            Path.GetTempPath(),
            "EatKathTests_RestaurantImage_" + Guid.NewGuid());

        _webHostEnvironment = new Mock<IWebHostEnvironment>();
        _webHostEnvironment.Setup(e => e.WebRootPath).Returns(_testRootPath);

        _fileStorage = new FileStorageService(_webHostEnvironment.Object);

        _service = new RestaurantImageService(
            _context,
            _mapper,
            _createValidator.Object,
            _updateValidator.Object,
            _fileStorage,
            _currentUser.Object);
    }

    [TestCleanup]
    public void Cleanup()
    {
        if (Directory.Exists(_testRootPath))
            Directory.Delete(_testRootPath, recursive: true);
    }

    private static CreateRestaurantImageDto BuildCreateDto(int restaurantId) => new()
    {
        RestaurantId = restaurantId,
        ImageUrl = "/uploads/restaurants/1/gallery/existing.png",
        IsLogo = false,
        DisplayOrder = 1
    };

    private static UpdateRestaurantImageDto BuildUpdateDto() => new()
    {
        ImageUrl = "/uploads/restaurants/1/gallery/updated.png",
        Caption = "Updated caption",
        DisplayOrder = 2,
        IsPrimary = true
    };

    private static Mock<IFormFile> BuildFormFile(string fileName = "photo.png")
    {
        var file = new Mock<IFormFile>();

        file.Setup(f => f.FileName).Returns(fileName);
        file.Setup(f => f.Length).Returns(4);

        file.Setup(f => f.CopyToAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        return file;
    }


    // -----------------------------
    // GetByRestaurantAsync
    // -----------------------------

    [TestMethod]
    public async Task GetByRestaurantAsync_ShouldReturnImagesForRestaurant()
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

        _context.RestaurantImages.Add(new RestaurantImage
        {
            RestaurantId = restaurantA.Id,
            ImageUrl = "/uploads/restaurants/a/1.png",
            DisplayOrder = 1
        });

        _context.RestaurantImages.Add(new RestaurantImage
        {
            RestaurantId = restaurantB.Id,
            ImageUrl = "/uploads/restaurants/b/1.png",
            DisplayOrder = 1
        });

        await _context.SaveChangesAsync();

        // -----------------------------
        // Act
        // -----------------------------

        var result = await _service.GetByRestaurantAsync(restaurantA.Id);

        // -----------------------------
        // Assert
        // -----------------------------

        result.Should().HaveCount(1);
        result.Single().RestaurantId.Should().Be(restaurantA.Id);
    }

    [TestMethod]
    public async Task GetByRestaurantAsync_ShouldReturnImagesOrderedByDisplayOrder()
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

        _context.RestaurantImages.Add(new RestaurantImage
        {
            RestaurantId = restaurant.Id,
            ImageUrl = "/uploads/restaurants/1/third.png",
            DisplayOrder = 3
        });

        _context.RestaurantImages.Add(new RestaurantImage
        {
            RestaurantId = restaurant.Id,
            ImageUrl = "/uploads/restaurants/1/first.png",
            DisplayOrder = 1
        });

        _context.RestaurantImages.Add(new RestaurantImage
        {
            RestaurantId = restaurant.Id,
            ImageUrl = "/uploads/restaurants/1/second.png",
            DisplayOrder = 2
        });

        await _context.SaveChangesAsync();

        // -----------------------------
        // Act
        // -----------------------------

        var result = (await _service.GetByRestaurantAsync(restaurant.Id)).ToList();

        // -----------------------------
        // Assert
        // -----------------------------

        result.Should().HaveCount(3);
        result[0].ImageUrl.Should().Be("/uploads/restaurants/1/first.png");
        result[1].ImageUrl.Should().Be("/uploads/restaurants/1/second.png");
        result[2].ImageUrl.Should().Be("/uploads/restaurants/1/third.png");
    }

    [TestMethod]
    public async Task GetByRestaurantAsync_ShouldReturnEmptyList_WhenRestaurantHasNoImages()
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

        // -----------------------------
        // Act
        // -----------------------------

        var result = await _service.GetByRestaurantAsync(restaurant.Id);

        // -----------------------------
        // Assert
        // -----------------------------

        result.Should().BeEmpty();
    }


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
        _context.RestaurantImages.Count().Should().Be(1);
    }

    [TestMethod]
    public async Task CreateAsync_ShouldThrowBusinessRuleException_WhenOwnerDoesNotOwnRestaurant()
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

        var dto = BuildCreateDto(restaurant.Id);

        // -----------------------------
        // Act & Assert
        // -----------------------------

        var ex = await Assert.ThrowsExceptionAsync<BusinessRuleException>(
            () => _service.CreateAsync(dto));

        ex.Message.Should().Be("You are not authorized to add images to this restaurant.");

        _context.RestaurantImages.Count().Should().Be(0);
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
        _context.RestaurantImages.Count().Should().Be(1);
    }

    [TestMethod]
    public async Task CreateAsync_ShouldThrowException_WhenRestaurantDoesNotExist()
    {
        // -----------------------------
        // Arrange
        // Purpose: preserve existing "not found" behavior.
        // -----------------------------

        var dto = BuildCreateDto(9999);

        // -----------------------------
        // Act & Assert
        // -----------------------------

        var ex = await Assert.ThrowsExceptionAsync<NotFoundException>(
            () => _service.CreateAsync(dto));

        ex.Message.Should().Be("Restaurant not found.");
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

        var entity = new RestaurantImage
        {
            RestaurantId = restaurant.Id,
            ImageUrl = "/uploads/restaurants/1/gallery/original.png",
            DisplayOrder = 1
        };

        _context.RestaurantImages.Add(entity);
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
        result!.Caption.Should().Be("Updated caption");
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

        var entity = new RestaurantImage
        {
            RestaurantId = restaurant.Id,
            ImageUrl = "/uploads/restaurants/1/gallery/original.png",
            DisplayOrder = 1
        };

        _context.RestaurantImages.Add(entity);
        await _context.SaveChangesAsync();

        var dto = BuildUpdateDto();

        // -----------------------------
        // Act & Assert
        // -----------------------------

        var ex = await Assert.ThrowsExceptionAsync<BusinessRuleException>(
            () => _service.UpdateAsync(entity.Id, dto));

        ex.Message.Should().Be("You are not authorized to modify images for this restaurant.");

        _context.RestaurantImages.First().Caption.Should().BeNull();
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

        var entity = new RestaurantImage
        {
            RestaurantId = restaurant.Id,
            ImageUrl = "/uploads/restaurants/1/gallery/original.png",
            DisplayOrder = 1
        };

        _context.RestaurantImages.Add(entity);
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
        result!.Caption.Should().Be("Updated caption");
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


    // -----------------------------
    // DeleteAsync
    // -----------------------------

    [TestMethod]
    public async Task DeleteAsync_ShouldSucceed_WhenOwnerOwnsRestaurant()
    {
        // -----------------------------
        // Arrange
        // Purpose:
        // Also verify the physical file is actually
        // removed for an authorized delete.
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

        var relativePath = "/uploads/restaurants/gallery/owned.png";
        var absolutePath = Path.Combine(_testRootPath, "uploads", "restaurants", "gallery", "owned.png");

        Directory.CreateDirectory(Path.GetDirectoryName(absolutePath)!);
        File.WriteAllBytes(absolutePath, new byte[] { 1, 2, 3 });

        var entity = new RestaurantImage
        {
            RestaurantId = restaurant.Id,
            ImageUrl = relativePath,
            DisplayOrder = 1
        };

        _context.RestaurantImages.Add(entity);
        await _context.SaveChangesAsync();

        // -----------------------------
        // Act
        // -----------------------------

        var result = await _service.DeleteAsync(entity.Id);

        // -----------------------------
        // Assert
        // -----------------------------

        result.Should().BeTrue();
        _context.RestaurantImages.Count().Should().Be(0);
        File.Exists(absolutePath).Should().BeFalse();
    }

    [TestMethod]
    public async Task DeleteAsync_ShouldThrowBusinessRuleException_WhenOwnerDoesNotOwnRestaurant()
    {
        // -----------------------------
        // Arrange
        // Purpose:
        // Verify the physical file is NOT touched when
        // the ownership check rejects the request - the
        // check runs before _fileStorage.DeleteFileAsync.
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

        var relativePath = "/uploads/restaurants/gallery/not-owned.png";
        var absolutePath = Path.Combine(_testRootPath, "uploads", "restaurants", "gallery", "not-owned.png");

        Directory.CreateDirectory(Path.GetDirectoryName(absolutePath)!);
        File.WriteAllBytes(absolutePath, new byte[] { 1, 2, 3 });

        var entity = new RestaurantImage
        {
            RestaurantId = restaurant.Id,
            ImageUrl = relativePath,
            DisplayOrder = 1
        };

        _context.RestaurantImages.Add(entity);
        await _context.SaveChangesAsync();

        // -----------------------------
        // Act & Assert
        // -----------------------------

        var ex = await Assert.ThrowsExceptionAsync<BusinessRuleException>(
            () => _service.DeleteAsync(entity.Id));

        ex.Message.Should().Be("You are not authorized to delete images from this restaurant.");

        _context.RestaurantImages.Count().Should().Be(1);
        File.Exists(absolutePath).Should().BeTrue();
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

        var entity = new RestaurantImage
        {
            RestaurantId = restaurant.Id,
            ImageUrl = "/uploads/restaurants/gallery/admin-delete.png",
            DisplayOrder = 1
        };

        _context.RestaurantImages.Add(entity);
        await _context.SaveChangesAsync();

        // -----------------------------
        // Act
        // -----------------------------

        var result = await _service.DeleteAsync(entity.Id);

        // -----------------------------
        // Assert
        // -----------------------------

        result.Should().BeTrue();
        _context.RestaurantImages.Count().Should().Be(0);
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


    // -----------------------------
    // UploadAsync
    // -----------------------------

    [TestMethod]
    public async Task UploadAsync_ShouldSucceed_WhenOwnerOwnsRestaurant()
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

        var file = BuildFormFile();

        // -----------------------------
        // Act
        // -----------------------------

        var result = await _service.UploadAsync(restaurant.Id, file.Object);

        // -----------------------------
        // Assert
        // -----------------------------

        result.Should().NotBeNull();
        _context.RestaurantImages.Count().Should().Be(1);

        var expectedFolder = Path.Combine(
            _testRootPath, "uploads", "restaurants", restaurant.Id.ToString(), "gallery");

        Directory.Exists(expectedFolder).Should().BeTrue();
        Directory.GetFiles(expectedFolder).Should().HaveCount(1);
    }

    [TestMethod]
    public async Task UploadAsync_ShouldThrowBusinessRuleException_WhenOwnerDoesNotOwnRestaurant()
    {
        // -----------------------------
        // Arrange
        // Purpose:
        // Verify no file is written to disk when the
        // ownership check rejects the request - the check
        // runs before _fileStorage.SaveImageAsync.
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

        var file = BuildFormFile();

        // -----------------------------
        // Act & Assert
        // -----------------------------

        var ex = await Assert.ThrowsExceptionAsync<BusinessRuleException>(
            () => _service.UploadAsync(restaurant.Id, file.Object));

        ex.Message.Should().Be("You are not authorized to upload images to this restaurant.");

        _context.RestaurantImages.Count().Should().Be(0);

        var expectedFolder = Path.Combine(
            _testRootPath, "uploads", "restaurants", restaurant.Id.ToString(), "gallery");

        Directory.Exists(expectedFolder).Should().BeFalse();
    }

    [TestMethod]
    public async Task UploadAsync_ShouldSucceed_WhenUserIsAdmin_EvenIfNotOwner()
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

        var file = BuildFormFile();

        // -----------------------------
        // Act
        // -----------------------------

        var result = await _service.UploadAsync(restaurant.Id, file.Object);

        // -----------------------------
        // Assert
        // -----------------------------

        result.Should().NotBeNull();
        _context.RestaurantImages.Count().Should().Be(1);
    }

    [TestMethod]
    public async Task UploadAsync_ShouldThrowException_WhenRestaurantDoesNotExist()
    {
        // -----------------------------
        // Arrange
        // Purpose: preserve existing "not found" behavior.
        // -----------------------------

        var file = BuildFormFile();

        // -----------------------------
        // Act & Assert
        // -----------------------------

        var ex = await Assert.ThrowsExceptionAsync<NotFoundException>(
            () => _service.UploadAsync(9999, file.Object));

        ex.Message.Should().Be("Restaurant not found.");
    }
}
