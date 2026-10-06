using AutoMapper;
using EatKath.API.Data;
using EatKath.API.DTOs.MenuItem;
using EatKath.API.Entities;
using EatKath.API.Interfaces;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace EatKath.API.Services;

public class MenuItemService : IMenuItemService
{
    // Largest accepted menu-item image upload (the owner page checks the
    // same limit before uploading).
    public const long MaxImageBytes = 5 * 1024 * 1024;

    private const string UploadFolderPrefix = "/uploads/menuitems/";

    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly IValidator<CreateMenuItemDto> _createValidator;
    private readonly IValidator<UpdateMenuItemDto> _updateValidator;
    private readonly FileStorageService _fileStorage;
    private readonly ICurrentUserService _currentUser;

    public MenuItemService(
        ApplicationDbContext context,
        IMapper mapper,
        IValidator<CreateMenuItemDto> createValidator,
        IValidator<UpdateMenuItemDto> updateValidator,
        FileStorageService fileStorage,
        ICurrentUserService currentUser)
    {
        _context = context;
        _mapper = mapper;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _fileStorage = fileStorage;
        _currentUser = currentUser;
    }

    public async Task<IEnumerable<MenuItemDto>> GetAllAsync()
    {
        var items = await _context.MenuItems
            .OrderBy(x => x.Name)
            .ToListAsync();

        return _mapper.Map<IEnumerable<MenuItemDto>>(items);
    }

    public async Task<MenuItemDto?> GetByIdAsync(int id)
    {
        var item = await _context.MenuItems.FindAsync(id);

        if (item == null)
            return null;

        return _mapper.Map<MenuItemDto>(item);
    }

    public async Task<IEnumerable<MenuItemDto>> GetByRestaurantAsync(int restaurantId)
    {
        var items = await _context.MenuItems
            .Where(x => x.RestaurantId == restaurantId)
            .OrderBy(x => x.Name)
            .ToListAsync();

        return _mapper.Map<IEnumerable<MenuItemDto>>(items);
    }

    public async Task<IEnumerable<MenuItemDto>> GetByCategoryAsync(int categoryId)
    {
        var items = await _context.MenuItems
            .Where(x => x.MenuCategoryId == categoryId)
            .OrderBy(x => x.Name)
            .ToListAsync();

        return _mapper.Map<IEnumerable<MenuItemDto>>(items);
    }

    public async Task<MenuItemDto> CreateAsync(CreateMenuItemDto dto)
    {
        var validation = await _createValidator.ValidateAsync(dto);

        if (!validation.IsValid)
            throw new ValidationException(validation.Errors);

        var category = await _context.MenuCategories
            .FirstOrDefaultAsync(x => x.Id == dto.MenuCategoryId);

        if (category == null)
            throw new BusinessRuleException("Menu category not found.");

        var restaurant = await _context.Restaurants
            .FirstOrDefaultAsync(r => r.Id == category.RestaurantId);

        if (restaurant == null)
            throw new BusinessRuleException("Restaurant not found.");

        if (!_currentUser.IsAdmin &&
            restaurant.OwnerId != _currentUser.UserId)
        {
            throw new BusinessRuleException("You are not authorized to add menu items to this restaurant.");
        }

        var entity = _mapper.Map<MenuItem>(dto);

        entity.RestaurantId = category.RestaurantId;

        _context.MenuItems.Add(entity);

        await _context.SaveChangesAsync();

        return _mapper.Map<MenuItemDto>(entity);
    }

    public async Task<MenuItemDto?> UpdateAsync(int id, UpdateMenuItemDto dto)
    {
        var validation = await _updateValidator.ValidateAsync(dto);

        if (!validation.IsValid)
            throw new ValidationException(validation.Errors);

        var entity = await _context.MenuItems.FindAsync(id);

        if (entity == null)
            return null;

        var restaurant = await _context.Restaurants
            .FirstOrDefaultAsync(r => r.Id == entity.RestaurantId);

        if (restaurant == null)
            throw new BusinessRuleException("Restaurant not found.");

        if (!_currentUser.IsAdmin &&
            restaurant.OwnerId != _currentUser.UserId)
        {
            throw new BusinessRuleException("You are not authorized to modify this menu item.");
        }

        if (dto.MenuCategoryId != entity.MenuCategoryId)
        {
            var newCategory = await _context.MenuCategories
                .FirstOrDefaultAsync(x => x.Id == dto.MenuCategoryId);

            if (newCategory == null)
                throw new BusinessRuleException("Menu category not found.");

            if (newCategory.RestaurantId != entity.RestaurantId)
                throw new BusinessRuleException("Menu category does not belong to this restaurant.");
        }

        _mapper.Map(dto, entity);

        await _context.SaveChangesAsync();

        return _mapper.Map<MenuItemDto>(entity);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var entity = await _context.MenuItems.FindAsync(id);

        if (entity == null)
            return false;

        var restaurant = await _context.Restaurants
            .FirstOrDefaultAsync(r => r.Id == entity.RestaurantId);

        if (restaurant == null)
            throw new BusinessRuleException("Restaurant not found.");

        if (!_currentUser.IsAdmin &&
            restaurant.OwnerId != _currentUser.UserId)
        {
            throw new BusinessRuleException("You are not authorized to delete this menu item.");
        }

        var imageUrl = entity.ImageUrl;

        _context.MenuItems.Remove(entity);

        await _context.SaveChangesAsync();

        await DeleteUploadedImageAsync(imageUrl);

        return true;
    }

    public async Task<string> UploadImageAsync(int menuItemId, IFormFile file)
    {
        var menuItem = await _context.MenuItems.FindAsync(menuItemId);

        if (menuItem == null)
            throw new NotFoundException("Menu item not found.");

        var restaurant = await _context.Restaurants
            .FirstOrDefaultAsync(r => r.Id == menuItem.RestaurantId);

        if (restaurant == null)
            throw new BusinessRuleException("Restaurant not found.");

        if (!_currentUser.IsAdmin &&
            restaurant.OwnerId != _currentUser.UserId)
        {
            throw new BusinessRuleException("You are not authorized to upload an image for this menu item.");
        }

        var previousImageUrl = menuItem.ImageUrl;

        var imagePath = await _fileStorage.SaveValidatedImageAsync(
            file,
            $"uploads/menuitems/{menuItemId}",
            MaxImageBytes);

        menuItem.ImageUrl = imagePath;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch
        {
            // Not saved: the new file would be orphaned.
            await DeleteUploadedImageAsync(imagePath);
            throw;
        }

        // Only after the new image is saved: the replaced file is no
        // longer referenced.
        await DeleteUploadedImageAsync(previousImageUrl);

        return imagePath;
    }


    public async Task DeleteImageAsync(int menuItemId)
    {
        var menuItem = await _context.MenuItems.FindAsync(menuItemId);

        if (menuItem == null)
            throw new NotFoundException("Menu item not found.");

        var restaurant = await _context.Restaurants
            .FirstOrDefaultAsync(r => r.Id == menuItem.RestaurantId);

        if (restaurant == null)
            throw new BusinessRuleException("Restaurant not found.");

        if (!_currentUser.IsAdmin &&
            restaurant.OwnerId != _currentUser.UserId)
        {
            throw new BusinessRuleException("You are not authorized to delete the image for this menu item.");
        }

        await DeleteUploadedImageAsync(menuItem.ImageUrl);

        menuItem.ImageUrl = null;

        await _context.SaveChangesAsync();
    }

    // Deletes a menu-item image file uploaded through this service. Any
    // other value (empty, or an external URL from old seed data) is not
    // a file under uploads/menuitems and is left alone.
    private async Task DeleteUploadedImageAsync(string? imageUrl)
    {
        if (string.IsNullOrEmpty(imageUrl) ||
            !imageUrl.StartsWith(UploadFolderPrefix, StringComparison.OrdinalIgnoreCase) ||
            imageUrl.Contains(".."))
        {
            return;
        }

        await _fileStorage.DeleteFileAsync(imageUrl);
    }
}