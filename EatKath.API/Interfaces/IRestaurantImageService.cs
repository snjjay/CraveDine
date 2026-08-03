using EatKath.API.DTOs.RestaurantImage;
using Microsoft.AspNetCore.Http;

namespace EatKath.API.Interfaces
{
    public interface IRestaurantImageService
    {
        Task<IEnumerable<RestaurantImageDto>> GetAllAsync();

        Task<RestaurantImageDto?> GetByIdAsync(int id);

        Task<IEnumerable<RestaurantImageDto>> GetByRestaurantAsync(int restaurantId);

        Task<RestaurantImageDto> CreateAsync(CreateRestaurantImageDto dto);

        Task<RestaurantImageDto?> UpdateAsync(int id, UpdateRestaurantImageDto dto);

        Task<bool> DeleteAsync(int id);

        // NEW
        Task<RestaurantImageDto> UploadAsync(int restaurantId, IFormFile file);
    }
}