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

    // Smallest headers the upload's content check recognises.
    private static readonly byte[] PngBytes =
        { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 0x00, 0x00, 0x00, 0x0D };

    private static readonly byte[] JpegBytes =
        { 0xFF, 0xD8, 0xFF, 0xE0, 0x00, 0x10, 0x4A, 0x46, 0x49, 0x46, 0x00, 0x01 };

    private static readonly byte[] WebpBytes =
        { 0x52, 0x49, 0x46, 0x46, 0x24, 0x00, 0x00, 0x00, 0x57, 0x45, 0x42, 0x50 };

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
            ImageUrl = imageUrl
        };

        _context.MenuItems.Add(menuItem);
        await _context.SaveChangesAsync();

        return menuItem;
    }

    // A real FormFile over in-memory bytes (PNG by default), so the
    // upload's content check and the file copy both run for real.
    private static IFormFile BuildFormFile(
        string fileName = "photo.png",
        byte[]? content = null)
    {
        var bytes = content ?? PngBytes;

        return new FormFile(new MemoryStream(bytes), 0, bytes.Length, "File", fileName);
    }

    private string MenuItemFolder(int menuItemId) =>
        Path.Combine(_testRootPath, "uploads", "menuitems", menuItemId.ToString());

    private string ToAbsolutePath(string relativePath) =>
        Path.Combine(_testRootPath, relativePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));

    // Creates a real file for a stored image path, as an earlier
    // upload would have.
    private string CreateFileFor(string relativePath)
    {
        var absolutePath = ToAbsolutePath(relativePath);

        Directory.CreateDirectory(Path.GetDirectoryName(absolutePath)!);
        File.WriteAllBytes(absolutePath, PngBytes);

        return absolutePath;
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

        var result = await _service.UploadImageAsync(menuItem.Id, file);

        // -----------------------------
        // Assert
        // Purpose:
        // The image is stored under a unique name in the
        // menu item's folder, with the uploaded content.
        // -----------------------------

        result.Should().MatchRegex($"^/uploads/menuitems/{menuItem.Id}/image-[0-9a-f]{{32}}\\.png$");
        _context.MenuItems.First().ImageUrl.Should().Be(result);

        var files = Directory.GetFiles(MenuItemFolder(menuItem.Id));

        files.Should().HaveCount(1);
        File.ReadAllBytes(files[0]).Should().Equal(PngBytes);
    }

    [TestMethod]
    [DataRow("photo.jpg", "jpeg")]
    [DataRow("photo.jpeg", "jpeg")]
    [DataRow("photo.webp", "webp")]
    [DataRow("PHOTO.PNG", "png")]
    public async Task UploadImageAsync_ShouldAccept_JpgPngAndWebp(string fileName, string kind)
    {
        // -----------------------------
        // Arrange
        // -----------------------------

        var menuItem = await SeedMenuItemAsync(restaurantOwnerId: 1);

        var content = kind switch
        {
            "jpeg" => JpegBytes,
            "webp" => WebpBytes,
            _ => PngBytes
        };

        // -----------------------------
        // Act
        // -----------------------------

        var result = await _service.UploadImageAsync(menuItem.Id, BuildFormFile(fileName, content));

        // -----------------------------
        // Assert
        // -----------------------------

        result.Should().EndWith(Path.GetExtension(fileName));
        File.Exists(ToAbsolutePath(result)).Should().BeTrue();
    }

    [TestMethod]
    public async Task UploadImageAsync_ShouldReplaceImage_AndDeletePreviousFile()
    {
        // -----------------------------
        // Arrange
        // -----------------------------

        _currentUser.Setup(x => x.IsAdmin).Returns(false);
        _currentUser.Setup(x => x.UserId).Returns(1);

        var menuItem = await SeedMenuItemAsync(restaurantOwnerId: 1);

        var first = await _service.UploadImageAsync(menuItem.Id, BuildFormFile("first.png"));

        // -----------------------------
        // Act
        // -----------------------------

        var second = await _service.UploadImageAsync(
            menuItem.Id,
            BuildFormFile("second.jpg", JpegBytes));

        // -----------------------------
        // Assert
        // Purpose:
        // A new URL (so browsers don't show the cached old
        // image), and the replaced file is gone - only the
        // new file remains in the folder.
        // -----------------------------

        second.Should().NotBe(first);
        _context.MenuItems.First().ImageUrl.Should().Be(second);

        File.Exists(ToAbsolutePath(first)).Should().BeFalse();
        File.Exists(ToAbsolutePath(second)).Should().BeTrue();
        Directory.GetFiles(MenuItemFolder(menuItem.Id)).Should().HaveCount(1);
    }

    [TestMethod]
    public async Task UploadImageAsync_ShouldReplaceExternalUrl_WithoutTouchingAnyFile()
    {
        // -----------------------------
        // Arrange
        // Purpose:
        // Old seed data used external picsum URLs; replacing
        // one must not try to delete anything.
        // -----------------------------

        var menuItem = await SeedMenuItemAsync(
            restaurantOwnerId: 1,
            imageUrl: "https://picsum.photos/seed/1-1-1/600/600");

        // -----------------------------
        // Act
        // -----------------------------

        var result = await _service.UploadImageAsync(menuItem.Id, BuildFormFile());

        // -----------------------------
        // Assert
        // -----------------------------

        _context.MenuItems.First().ImageUrl.Should().Be(result);
        File.Exists(ToAbsolutePath(result)).Should().BeTrue();
    }

    [TestMethod]
    public async Task UploadImageAsync_ShouldNotDeleteFilesOutsideMenuItemUploads()
    {
        // -----------------------------
        // Arrange
        // Purpose:
        // Only files under /uploads/menuitems/ are ever
        // cleaned up - e.g. a restaurant photo path stored
        // on a menu item is left on disk.
        // -----------------------------

        var restaurantPhoto = "/uploads/restaurants/7/cover.png";
        var restaurantPhotoPath = CreateFileFor(restaurantPhoto);

        var menuItem = await SeedMenuItemAsync(
            restaurantOwnerId: 1,
            imageUrl: restaurantPhoto);

        // -----------------------------
        // Act
        // -----------------------------

        await _service.UploadImageAsync(menuItem.Id, BuildFormFile());

        // -----------------------------
        // Assert
        // -----------------------------

        File.Exists(restaurantPhotoPath).Should().BeTrue();
    }

    [TestMethod]
    public async Task UploadImageAsync_ShouldThrowBusinessRuleException_WhenOwnerDoesNotOwnRestaurant()
    {
        // -----------------------------
        // Arrange
        // Purpose:
        // Verify no file is written to disk when the
        // ownership check rejects the request - the check
        // runs before the file is saved.
        // -----------------------------

        _currentUser.Setup(x => x.IsAdmin).Returns(false);
        _currentUser.Setup(x => x.UserId).Returns(1);

        var existingImage = "/uploads/menuitems/not-owned/image-existing.png";
        var existingPath = CreateFileFor(existingImage);

        var menuItem = await SeedMenuItemAsync(
            restaurantOwnerId: 2,
            imageUrl: existingImage);

        // -----------------------------
        // Act & Assert
        // -----------------------------

        var ex = await Assert.ThrowsExceptionAsync<BusinessRuleException>(
            () => _service.UploadImageAsync(menuItem.Id, BuildFormFile()));

        ex.Message.Should().Be("You are not authorized to upload an image for this menu item.");

        _context.MenuItems.First().ImageUrl.Should().Be(existingImage);
        File.Exists(existingPath).Should().BeTrue();
        Directory.Exists(MenuItemFolder(menuItem.Id)).Should().BeFalse();
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

        // -----------------------------
        // Act
        // -----------------------------

        var result = await _service.UploadImageAsync(menuItem.Id, BuildFormFile());

        // -----------------------------
        // Assert
        // -----------------------------

        result.Should().NotBeNullOrEmpty();
        _context.MenuItems.First().ImageUrl.Should().Be(result);
    }

    [TestMethod]
    [DataRow("photo.gif")]
    [DataRow("photo.svg")]
    [DataRow("menu.pdf")]
    [DataRow("photo")]
    public async Task UploadImageAsync_ShouldReject_UnsupportedExtension(string fileName)
    {
        // -----------------------------
        // Arrange
        // -----------------------------

        var existingImage = "/uploads/menuitems/1/image-existing.png";
        var menuItem = await SeedMenuItemAsync(restaurantOwnerId: 1, imageUrl: existingImage);

        // -----------------------------
        // Act & Assert
        // -----------------------------

        var ex = await Assert.ThrowsExceptionAsync<BusinessRuleException>(
            () => _service.UploadImageAsync(menuItem.Id, BuildFormFile(fileName)));

        ex.Message.Should().Be("Only JPG, PNG and WEBP images are allowed.");

        _context.MenuItems.First().ImageUrl.Should().Be(existingImage);
        Directory.Exists(MenuItemFolder(menuItem.Id)).Should().BeFalse();
    }

    [TestMethod]
    public async Task UploadImageAsync_ShouldReject_FileWhoseContentIsNotAnImage()
    {
        // -----------------------------
        // Arrange
        // Purpose:
        // A text file renamed to .png is rejected - the
        // extension alone is not trusted.
        // -----------------------------

        var menuItem = await SeedMenuItemAsync(restaurantOwnerId: 1);

        var fake = BuildFormFile("photo.png", "<script>alert(1)</script>"u8.ToArray());

        // -----------------------------
        // Act & Assert
        // -----------------------------

        var ex = await Assert.ThrowsExceptionAsync<BusinessRuleException>(
            () => _service.UploadImageAsync(menuItem.Id, fake));

        ex.Message.Should().Be("The selected file is not a valid JPG, PNG or WEBP image.");

        _context.MenuItems.First().ImageUrl.Should().BeNull();
        Directory.Exists(MenuItemFolder(menuItem.Id)).Should().BeFalse();
    }

    [TestMethod]
    public async Task UploadImageAsync_ShouldReject_EmptyFile()
    {
        // -----------------------------
        // Arrange
        // -----------------------------

        var menuItem = await SeedMenuItemAsync(restaurantOwnerId: 1);

        // -----------------------------
        // Act & Assert
        // -----------------------------

        var ex = await Assert.ThrowsExceptionAsync<BusinessRuleException>(
            () => _service.UploadImageAsync(menuItem.Id, BuildFormFile("photo.png", Array.Empty<byte>())));

        ex.Message.Should().Be("The selected image is empty.");
        Directory.Exists(MenuItemFolder(menuItem.Id)).Should().BeFalse();
    }

    [TestMethod]
    public async Task UploadImageAsync_ShouldReject_FileLargerThanLimit()
    {
        // -----------------------------
        // Arrange
        // -----------------------------

        var existingImage = "/uploads/menuitems/1/image-existing.png";
        var menuItem = await SeedMenuItemAsync(restaurantOwnerId: 1, imageUrl: existingImage);

        var oversized = new byte[MenuItemService.MaxImageBytes + 1];
        PngBytes.CopyTo(oversized, 0);

        // -----------------------------
        // Act & Assert
        // -----------------------------

        var ex = await Assert.ThrowsExceptionAsync<BusinessRuleException>(
            () => _service.UploadImageAsync(menuItem.Id, BuildFormFile("big.png", oversized)));

        ex.Message.Should().Be("Images must be 5 MB or smaller.");

        _context.MenuItems.First().ImageUrl.Should().Be(existingImage);
        Directory.Exists(MenuItemFolder(menuItem.Id)).Should().BeFalse();
    }

    [TestMethod]
    public async Task UploadImageAsync_ShouldAccept_FileExactlyAtLimit()
    {
        // -----------------------------
        // Arrange
        // -----------------------------

        var menuItem = await SeedMenuItemAsync(restaurantOwnerId: 1);

        var atLimit = new byte[MenuItemService.MaxImageBytes];
        PngBytes.CopyTo(atLimit, 0);

        // -----------------------------
        // Act
        // -----------------------------

        var result = await _service.UploadImageAsync(menuItem.Id, BuildFormFile("big.png", atLimit));

        // -----------------------------
        // Assert
        // -----------------------------

        new FileInfo(ToAbsolutePath(result)).Length.Should().Be(MenuItemService.MaxImageBytes);
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
            () => _service.UploadImageAsync(9999, file));

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
        // removed and the stored value becomes NULL.
        // -----------------------------

        _currentUser.Setup(x => x.IsAdmin).Returns(false);
        _currentUser.Setup(x => x.UserId).Returns(1);

        var relativePath = "/uploads/menuitems/owned/image.png";
        var absolutePath = CreateFileFor(relativePath);

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

        _context.MenuItems.First().ImageUrl.Should().BeNull();
        File.Exists(absolutePath).Should().BeFalse();
    }

    [TestMethod]
    public async Task DeleteImageAsync_ShouldRemoveImageUploadedThroughService()
    {
        // -----------------------------
        // Arrange
        // -----------------------------

        _currentUser.Setup(x => x.IsAdmin).Returns(false);
        _currentUser.Setup(x => x.UserId).Returns(1);

        var menuItem = await SeedMenuItemAsync(restaurantOwnerId: 1);

        var uploaded = await _service.UploadImageAsync(menuItem.Id, BuildFormFile());

        // -----------------------------
        // Act
        // -----------------------------

        await _service.DeleteImageAsync(menuItem.Id);

        // -----------------------------
        // Assert
        // -----------------------------

        _context.MenuItems.First().ImageUrl.Should().BeNull();
        File.Exists(ToAbsolutePath(uploaded)).Should().BeFalse();
        Directory.GetFiles(MenuItemFolder(menuItem.Id)).Should().BeEmpty();
    }

    [TestMethod]
    public async Task DeleteImageAsync_ShouldClearExternalUrl_ToNull()
    {
        // -----------------------------
        // Arrange
        // -----------------------------

        var menuItem = await SeedMenuItemAsync(
            restaurantOwnerId: 1,
            imageUrl: "https://picsum.photos/seed/1-1-1/600/600");

        // -----------------------------
        // Act
        // -----------------------------

        await _service.DeleteImageAsync(menuItem.Id);

        // -----------------------------
        // Assert
        // -----------------------------

        _context.MenuItems.First().ImageUrl.Should().BeNull();
    }

    [TestMethod]
    public async Task DeleteImageAsync_ShouldThrowBusinessRuleException_WhenOwnerDoesNotOwnRestaurant()
    {
        // -----------------------------
        // Arrange
        // Purpose:
        // Verify the physical file is NOT touched when
        // the ownership check rejects the request - the
        // check runs before the file is deleted.
        // -----------------------------

        _currentUser.Setup(x => x.IsAdmin).Returns(false);
        _currentUser.Setup(x => x.UserId).Returns(1);

        var relativePath = "/uploads/menuitems/not-owned/image.png";
        var absolutePath = CreateFileFor(relativePath);

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

        var relativePath = "/uploads/menuitems/admin-delete/image.png";
        var absolutePath = CreateFileFor(relativePath);

        var menuItem = await SeedMenuItemAsync(
            restaurantOwnerId: 2,
            imageUrl: relativePath);

        // -----------------------------
        // Act
        // -----------------------------

        await _service.DeleteImageAsync(menuItem.Id);

        // -----------------------------
        // Assert
        // -----------------------------

        _context.MenuItems.First().ImageUrl.Should().BeNull();
        File.Exists(absolutePath).Should().BeFalse();
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


    // -----------------------------
    // DeleteAsync (image cleanup)
    // -----------------------------

    [TestMethod]
    public async Task DeleteAsync_ShouldDeleteMenuItemImageFile()
    {
        // -----------------------------
        // Arrange
        // -----------------------------

        _currentUser.Setup(x => x.IsAdmin).Returns(false);
        _currentUser.Setup(x => x.UserId).Returns(1);

        var menuItem = await SeedMenuItemAsync(restaurantOwnerId: 1);

        var uploaded = await _service.UploadImageAsync(menuItem.Id, BuildFormFile());

        // -----------------------------
        // Act
        // -----------------------------

        var result = await _service.DeleteAsync(menuItem.Id);

        // -----------------------------
        // Assert
        // -----------------------------

        result.Should().BeTrue();
        _context.MenuItems.Should().BeEmpty();
        File.Exists(ToAbsolutePath(uploaded)).Should().BeFalse();
    }

    [TestMethod]
    public async Task DeleteAsync_ShouldSucceed_WhenMenuItemHasNoImage()
    {
        // -----------------------------
        // Arrange
        // -----------------------------

        var menuItem = await SeedMenuItemAsync(restaurantOwnerId: 1);

        // -----------------------------
        // Act
        // -----------------------------

        var result = await _service.DeleteAsync(menuItem.Id);

        // -----------------------------
        // Assert
        // -----------------------------

        result.Should().BeTrue();
        _context.MenuItems.Should().BeEmpty();
    }

    [TestMethod]
    public async Task DeleteAsync_ShouldKeepItemAndImage_WhenOwnerDoesNotOwnRestaurant()
    {
        // -----------------------------
        // Arrange
        // -----------------------------

        _currentUser.Setup(x => x.IsAdmin).Returns(false);
        _currentUser.Setup(x => x.UserId).Returns(1);

        var relativePath = "/uploads/menuitems/not-owned/image.png";
        var absolutePath = CreateFileFor(relativePath);

        var menuItem = await SeedMenuItemAsync(
            restaurantOwnerId: 2,
            imageUrl: relativePath);

        // -----------------------------
        // Act & Assert
        // -----------------------------

        var ex = await Assert.ThrowsExceptionAsync<BusinessRuleException>(
            () => _service.DeleteAsync(menuItem.Id));

        ex.Message.Should().Be("You are not authorized to delete this menu item.");

        _context.MenuItems.Should().HaveCount(1);
        File.Exists(absolutePath).Should().BeTrue();
    }
}
