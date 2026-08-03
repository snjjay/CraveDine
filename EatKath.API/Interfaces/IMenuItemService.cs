using EatKath.API.DTOs.MenuItem;
using Microsoft.AspNetCore.Http;

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

        Task<string> UploadImageAsync(int menuItemId, IFormFile file);

        Task DeleteImageAsync(int menuItemId);
    }
}