using AutoMapper;
using EatKath.API.Data;
using EatKath.API.DTOs.MenuCategory;
using EatKath.API.Entities;
using EatKath.API.Exceptions;
using EatKath.API.Interfaces;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
namespace EatKath.API.Services;



public class MenuCategoryService : IMenuCategoryService
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly IValidator<CreateMenuCategoryDto> _createValidator;
    private readonly IValidator<UpdateMenuCategoryDto> _updateValidator;
    private readonly ICurrentUserService _currentUser;
    public MenuCategoryService(
    ApplicationDbContext context,
    IMapper mapper,
    IValidator<CreateMenuCategoryDto> createValidator,
    IValidator<UpdateMenuCategoryDto> updateValidator,
    ICurrentUserService currentUser)
    {
        _context = context;
        _mapper = mapper;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _currentUser = currentUser;
    }

    public async Task<IEnumerable<MenuCategoryDto>> GetAllAsync()
    {
        var categories = await _context.MenuCategories
            .OrderBy(x => x.DisplayOrder)
            .ToListAsync();

        return _mapper.Map<IEnumerable<MenuCategoryDto>>(categories);
    }

    public async Task<MenuCategoryDto?> GetByIdAsync(int id)
    {
        var category = await _context.MenuCategories.FindAsync(id);

        if (category == null)
            return null;

        return _mapper.Map<MenuCategoryDto>(category);
    }

    public async Task<IEnumerable<MenuCategoryDto>> GetByRestaurantAsync(int restaurantId)
    {
        var categories = await _context.MenuCategories
            .Where(x => x.RestaurantId == restaurantId)
            .OrderBy(x => x.DisplayOrder)
            .ToListAsync();

        return _mapper.Map<IEnumerable<MenuCategoryDto>>(categories);
    }

    public async Task<MenuCategoryDto> CreateAsync(CreateMenuCategoryDto dto)
    {
        var validation = await _createValidator.ValidateAsync(dto);

        if (!validation.IsValid)
            throw new ValidationException(validation.Errors);

        var restaurant = await _context.Restaurants
       .FirstOrDefaultAsync(r => r.Id == dto.RestaurantId);

        if (restaurant == null)
            throw new BusinessRuleException("Restaurant not found.");

        // Restaurant owners can only manage their own restaurant
        if (!_currentUser.IsAdmin &&
            restaurant.OwnerId != _currentUser.UserId)
        {
            throw new BusinessRuleException(
    $"Restaurant OwnerId = {restaurant.OwnerId}, Current UserId = {_currentUser.UserId}, IsAdmin = {_currentUser.IsAdmin}");
        }

        var exists = await _context.MenuCategories.AnyAsync(x =>
            x.RestaurantId == dto.RestaurantId &&
            x.Name.Trim().ToLower() == dto.Name.Trim().ToLower());

        if (exists)
            throw new DuplicateEntityException("Menu category already exists.");

        var entity = new MenuCategory
        {
            RestaurantId = dto.RestaurantId,
            Name = dto.Name,
            DisplayOrder = dto.DisplayOrder
        };

        _context.MenuCategories.Add(entity);

        await _context.SaveChangesAsync();

        return _mapper.Map<MenuCategoryDto>(entity);
    }

    public async Task<MenuCategoryDto?> UpdateAsync(int id, UpdateMenuCategoryDto dto)
    {
        var validation = await _updateValidator.ValidateAsync(dto);

        if (!validation.IsValid)
            throw new ValidationException(validation.Errors);

        var entity = await _context.MenuCategories.FindAsync(id);

        if (entity == null)
            return null;

        // Get the restaurant that owns this category
        var restaurant = await _context.Restaurants
            .FirstOrDefaultAsync(r => r.Id == entity.RestaurantId);

        if (restaurant == null)
            throw new BusinessRuleException("Restaurant not found.");

        // Admin can manage any restaurant.
        // Restaurant owners can only manage their own restaurant.
        Console.WriteLine($"Restaurant OwnerId: {restaurant.OwnerId}");
        Console.WriteLine($"Current UserId: {_currentUser.UserId}");
        Console.WriteLine($"Is Admin: {_currentUser.IsAdmin}");
        if (!_currentUser.IsAdmin &&
            restaurant.OwnerId != _currentUser.UserId)
        {
            throw new BusinessRuleException("You are not authorized to modify this restaurant.");
        }

        var exists = await _context.MenuCategories.AnyAsync(x =>
            x.Id != id &&
            x.RestaurantId == entity.RestaurantId &&
            x.Name.Trim().ToLower() == dto.Name.Trim().ToLower());

        if (exists)
            throw new DuplicateEntityException("Menu category already exists.");

        _mapper.Map(dto, entity);

        entity.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return _mapper.Map<MenuCategoryDto>(entity);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var entity = await _context.MenuCategories.FindAsync(id);

        if (entity == null)
            return false;

        // Get the restaurant that owns this category
        var restaurant = await _context.Restaurants
            .FirstOrDefaultAsync(r => r.Id == entity.RestaurantId);

        if (restaurant == null)
            throw new BusinessRuleException("Restaurant not found.");

        // Admin can manage any restaurant.
        // Restaurant owners can only manage their own restaurant.
        if (!_currentUser.IsAdmin &&
            restaurant.OwnerId != _currentUser.UserId)
        {
            throw new BusinessRuleException("You are not authorized to delete this menu category.");
        }

        var hasItems = await _context.MenuItems
            .AnyAsync(x => x.MenuCategoryId == id);

        if (hasItems)
            throw new BusinessRuleException("Cannot delete category because it contains menu items.");

        _context.MenuCategories.Remove(entity);

        await _context.SaveChangesAsync();

        return true;
    }
}