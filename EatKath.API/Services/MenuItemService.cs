using AutoMapper;
using EatKath.API.Data;
using EatKath.API.DTOs.MenuItem;
using EatKath.API.Entities;
using EatKath.API.Interfaces;
using FluentValidation;
using Microsoft.EntityFrameworkCore;


namespace EatKath.API.Services;

public class MenuItemService : IMenuItemService
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly IValidator<CreateMenuItemDto> _createValidator;
    private readonly IValidator<UpdateMenuItemDto> _updateValidator;
    private readonly ICurrentUserService _currentUser;
    public MenuItemService(
         ApplicationDbContext context,
         IMapper mapper,
         IValidator<CreateMenuItemDto> createValidator,
         IValidator<UpdateMenuItemDto> updateValidator)
    {
        _context = context;
        _mapper = mapper;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
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
}