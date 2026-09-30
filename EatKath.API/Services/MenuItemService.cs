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

        _context.MenuItems.Remove(entity);

        await _context.SaveChangesAsync();

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

        var imagePath = await _fileStorage.SaveImageAsync(
            file,
            $"uploads/menuitems/{menuItemId}",
            "image");

        menuItem.ImageUrl = imagePath;

        await _context.SaveChangesAsync();

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

        await _fileStorage.DeleteFileAsync(menuItem.ImageUrl);

        menuItem.ImageUrl = string.Empty;

        await _context.SaveChangesAsync();
    }
}