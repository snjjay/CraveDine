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

    public MenuItemService(
        ApplicationDbContext context,
        IMapper mapper,
        IValidator<CreateMenuItemDto> createValidator,
        IValidator<UpdateMenuItemDto> updateValidator,
        FileStorageService fileStorage)
    {
        _context = context;
        _mapper = mapper;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _fileStorage = fileStorage;
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

        var categoryExists = await _context.MenuCategories
            .AnyAsync(x => x.Id == dto.MenuCategoryId);

        if (!categoryExists)
            throw new BusinessRuleException("Menu category not found.");

        var entity = _mapper.Map<MenuItem>(dto);

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

        _mapper.Map(dto, entity);

        await _context.SaveChangesAsync();

        return _mapper.Map<MenuItemDto>(entity);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var entity = await _context.MenuItems.FindAsync(id);

        if (entity == null)
            return false;

        _context.MenuItems.Remove(entity);

        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<string> UploadImageAsync(int menuItemId, IFormFile file)
    {
        var menuItem = await _context.MenuItems.FindAsync(menuItemId);

        if (menuItem == null)
            throw new Exception("Menu item not found.");

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
            throw new Exception("Menu item not found.");

        await _fileStorage.DeleteFileAsync(menuItem.ImageUrl);

        menuItem.ImageUrl = string.Empty;

        await _context.SaveChangesAsync();
    }
}