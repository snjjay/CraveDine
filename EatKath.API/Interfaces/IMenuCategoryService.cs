using EatKath.API.DTOs.MenuCategory;

namespace EatKath.API.Interfaces
{
    public interface IMenuCategoryService
    {
        Task<IEnumerable<MenuCategoryDto>> GetAllAsync();

        Task<MenuCategoryDto?> GetByIdAsync(int id);

        Task<IEnumerable<MenuCategoryDto>> GetByRestaurantAsync(int restaurantId);

        Task<MenuCategoryDto> CreateAsync(CreateMenuCategoryDto dto);

        Task<MenuCategoryDto?> UpdateAsync(int id, UpdateMenuCategoryDto dto);

        Task<bool> DeleteAsync(int id);
    }
}