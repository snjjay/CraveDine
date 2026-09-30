using AutoMapper;
using EatKath.API.Data;
using EatKath.API.DTOs.MenuItem;
using EatKath.API.Entities;
using EatKath.API.Interfaces;
using EatKath.API.Services;
using EatKath.API.Tests.Helpers;
using FluentAssertions;
using FluentValidation;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace EatKath.API.Tests.Services;

[TestClass]
public class MenuItemServiceTests
{
    private ApplicationDbContext _context = null!;
    private IMapper _mapper = null!;
    private Mock<IValidator<CreateMenuItemDto>> _createValidator = null!;
    private Mock<IValidator<UpdateMenuItemDto>> _updateValidator = null!;
    private Mock<ICurrentUserService> _currentUser = null!;
    private Mock<IWebHostEnvironment> _webHostEnvironment = null!;
    private FileStorageService _fileStorage = null!;
    private string _testRootPath = null!;
    private MenuItemService _service = null!;

    // -----------------------------
    // FileStorageService is a concrete class with no
    // virtual members and no interface, so it cannot be
    // mocked with Moq. It is constructed for real here,
    // backed by a real (temp) IWebHostEnvironment.WebRootPath,
    // so Upload/Delete tests perform genuine file I/O against
    // a throwaway directory that TestCleanup removes.
    // -----------------------------

    [TestInitialize]
    public void Setup()
    {
        _context = TestDbContextFactory.Create();

        _mapper = MapperFactory.Create();

        _createValidator = new Mock<IValidator<CreateMenuItemDto>>();
        _updateValidator = new Mock<IValidator<UpdateMenuItemDto>>();

        _currentUser = new Mock<ICurrentUserService>();

        // Default: an Admin caller, so tests that don't
        // care about ownership continue to behave predictably.
        _currentUser.Setup(x => x.UserId).Returns(1);
        _currentUser.Setup(x => x.IsAdmin).Returns(true);

        _testRootPath = Path.Combine(
            Path.GetTempPath(),
            "EatKathTests_MenuItem_" + Guid.NewGuid());

        _webHostEnvironment = new Mock<IWebHostEnvironment>();
        _webHostEnvironment.Setup(e => e.WebRootPath).Returns(_testRootPath);

        _fileStorage = new FileStorageService(_webHostEnvironment.Object);

        _service = new MenuItemService(
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

    private async Task<MenuItem> SeedMenuItemAsync(
        int restaurantOwnerId,
        string? imageUrl = null)
    {
        var restaurant = new Restaurant
        {
            Name = "Spice Kitchen",
            OwnerId = restaurantOwnerId,
            IsActive = true
        };

        _context.Restaurants.Add(restaurant);
        await _context.SaveChangesAsync();

        var menuItem = new MenuItem
        {
            RestaurantId = restaurant.Id,
            MenuCategoryId = 1,
            Name = "Butter Chicken",
            Description = "Creamy tomato curry",
            Price = 15.99m,
            ImageUrl = imageUrl ?? string.Empty
        };

        _context.MenuItems.Add(menuItem);
        await _context.SaveChangesAsync();

        return menuItem;
    }

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
    // UploadImageAsync
    // -----------------------------

    [TestMethod]
    public async Task UploadImageAsync_ShouldSucceed_WhenOwnerOwnsRestaurant()
    {
        // -----------------------------
        // Arrange
        // -----------------------------

        _currentUser.Setup(x => x.IsAdmin).Returns(false);
        _currentUser.Setup(x => x.UserId).Returns(1);

        var menuItem = await SeedMenuItemAsync(restaurantOwnerId: 1);

        var file = BuildFormFile();

        // -----------------------------
        // Act
        // -----------------------------

        var result = await _service.UploadImageAsync(menuItem.Id, file.Object);

        // -----------------------------
        // Assert
        // -----------------------------

        result.Should().NotBeNullOrEmpty();
        _context.MenuItems.First().ImageUrl.Should().Be(result);

        var expectedFolder = Path.Combine(
            _testRootPath, "uploads", "menuitems", menuItem.Id.ToString());

        Directory.Exists(expectedFolder).Should().BeTrue();
        Directory.GetFiles(expectedFolder).Should().HaveCount(1);
    }

    [TestMethod]
    public async Task UploadImageAsync_ShouldThrowBusinessRuleException_WhenOwnerDoesNotOwnRestaurant()
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

        var menuItem = await SeedMenuItemAsync(restaurantOwnerId: 2);

        var file = BuildFormFile();

        // -----------------------------
        // Act & Assert
        // -----------------------------

        var ex = await Assert.ThrowsExceptionAsync<BusinessRuleException>(
            () => _service.UploadImageAsync(menuItem.Id, file.Object));

        ex.Message.Should().Be("You are not authorized to upload an image for this menu item.");

        _context.MenuItems.First().ImageUrl.Should().BeEmpty();

        var expectedFolder = Path.Combine(
            _testRootPath, "uploads", "menuitems", menuItem.Id.ToString());

        Directory.Exists(expectedFolder).Should().BeFalse();
    }

    [TestMethod]
    public async Task UploadImageAsync_ShouldSucceed_WhenUserIsAdmin_EvenIfNotOwner()
    {
        // -----------------------------
        // Arrange
        // -----------------------------

        _currentUser.Setup(x => x.IsAdmin).Returns(true);
        _currentUser.Setup(x => x.UserId).Returns(999);

        var menuItem = await SeedMenuItemAsync(restaurantOwnerId: 2);

        var file = BuildFormFile();

        // -----------------------------
        // Act
        // -----------------------------

        var result = await _service.UploadImageAsync(menuItem.Id, file.Object);

        // -----------------------------
        // Assert
        // -----------------------------

        result.Should().NotBeNullOrEmpty();
        _context.MenuItems.First().ImageUrl.Should().Be(result);
    }

    [TestMethod]
    public async Task UploadImageAsync_ShouldThrowException_WhenMenuItemDoesNotExist()
    {
        // -----------------------------
        // Arrange
        // Purpose: preserve existing not-found behavior.
        // -----------------------------

        var file = BuildFormFile();

        // -----------------------------
        // Act & Assert
        // -----------------------------

        var ex = await Assert.ThrowsExceptionAsync<NotFoundException>(
            () => _service.UploadImageAsync(9999, file.Object));

        ex.Message.Should().Be("Menu item not found.");
    }


    // -----------------------------
    // DeleteImageAsync
    // -----------------------------

    [TestMethod]
    public async Task DeleteImageAsync_ShouldSucceed_WhenOwnerOwnsRestaurant()
    {
        // -----------------------------
        // Arrange
        // Purpose:
        // Also verify the physical file is actually
        // removed for an authorized delete.
        // -----------------------------

        _currentUser.Setup(x => x.IsAdmin).Returns(false);
        _currentUser.Setup(x => x.UserId).Returns(1);

        var relativePath = "/uploads/menuitems/owned/image.png";
        var absolutePath = Path.Combine(_testRootPath, "uploads", "menuitems", "owned", "image.png");

        Directory.CreateDirectory(Path.GetDirectoryName(absolutePath)!);
        File.WriteAllBytes(absolutePath, new byte[] { 1, 2, 3 });

        var menuItem = await SeedMenuItemAsync(
            restaurantOwnerId: 1,
            imageUrl: relativePath);

        // -----------------------------
        // Act
        // -----------------------------

        await _service.DeleteImageAsync(menuItem.Id);

        // -----------------------------
        // Assert
        // -----------------------------

        _context.MenuItems.First().ImageUrl.Should().BeEmpty();
        File.Exists(absolutePath).Should().BeFalse();
    }

    [TestMethod]
    public async Task DeleteImageAsync_ShouldThrowBusinessRuleException_WhenOwnerDoesNotOwnRestaurant()
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

        var relativePath = "/uploads/menuitems/not-owned/image.png";
        var absolutePath = Path.Combine(_testRootPath, "uploads", "menuitems", "not-owned", "image.png");

        Directory.CreateDirectory(Path.GetDirectoryName(absolutePath)!);
        File.WriteAllBytes(absolutePath, new byte[] { 1, 2, 3 });

        var menuItem = await SeedMenuItemAsync(
            restaurantOwnerId: 2,
            imageUrl: relativePath);

        // -----------------------------
        // Act & Assert
        // -----------------------------

        var ex = await Assert.ThrowsExceptionAsync<BusinessRuleException>(
            () => _service.DeleteImageAsync(menuItem.Id));

        ex.Message.Should().Be("You are not authorized to delete the image for this menu item.");

        _context.MenuItems.First().ImageUrl.Should().Be(relativePath);
        File.Exists(absolutePath).Should().BeTrue();
    }

    [TestMethod]
    public async Task DeleteImageAsync_ShouldSucceed_WhenUserIsAdmin_EvenIfNotOwner()
    {
        // -----------------------------
        // Arrange
        // -----------------------------

        _currentUser.Setup(x => x.IsAdmin).Returns(true);
        _currentUser.Setup(x => x.UserId).Returns(999);

        var menuItem = await SeedMenuItemAsync(
            restaurantOwnerId: 2,
            imageUrl: "/uploads/menuitems/admin-delete/image.png");

        // -----------------------------
        // Act
        // -----------------------------

        await _service.DeleteImageAsync(menuItem.Id);

        // -----------------------------
        // Assert
        // -----------------------------

        _context.MenuItems.First().ImageUrl.Should().BeEmpty();
    }

    [TestMethod]
    public async Task DeleteImageAsync_ShouldThrowException_WhenMenuItemDoesNotExist()
    {
        // -----------------------------
        // Arrange
        // Purpose: preserve existing not-found behavior.
        // -----------------------------

        // -----------------------------
        // Act & Assert
        // -----------------------------

        var ex = await Assert.ThrowsExceptionAsync<NotFoundException>(
            () => _service.DeleteImageAsync(9999));

        ex.Message.Should().Be("Menu item not found.");
    }
}
