using EatKath.API.DTOs.MenuItem;

namespace EatKath.API.Interfaces
{
    public interface IMenuItemService
    {
        Task<IEnumerable<MenuItemDto>> GetAllAsync();

        Task<MenuItemDto?> GetByIdAsync(int id);

        Task<IEnumerable<MenuItemDto>> GetByRestaurantAsync(int restaurantId);

        Task<IEnumerable<MenuItemDto>> GetByCategoryAsync(int categoryId);

        Task<MenuItemDto> CreateAsync(CreateMenuItemDto dto);

        Task<MenuItemDto?> UpdateAsync(int id, UpdateMenuItemDto dto);

        Task<bool> DeleteAsync(int id);
    }
}