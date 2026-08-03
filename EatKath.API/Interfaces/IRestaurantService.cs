using EatKath.API.DTOs.Restaurant;
using Microsoft.AspNetCore.Http;

namespace EatKath.API.Services.Interfaces
{
    public interface IRestaurantService
    {
        Task<IEnumerable<RestaurantDto>> GetAllAsync();

        Task<RestaurantDto?> GetByIdAsync(int id);

        Task<RestaurantDto> CreateAsync(CreateRestaurantDto dto);

        Task<RestaurantDto?> UpdateAsync(int id, UpdateRestaurantDto dto);

        Task<bool> DeleteAsync(int id);

        // Images
        Task<string> UploadLogoAsync(int restaurantId, IFormFile file);

        Task<string> UploadCoverAsync(int restaurantId, IFormFile file);

        Task<string> UploadMenuPdfAsync(int restaurantId, IFormFile file);


        Task DeleteLogoAsync(int restaurantId);

        Task DeleteCoverAsync(int restaurantId);

        Task DeleteMenuPdfAsync(int restaurantId);

        Task<RestaurantDto?> GetByOwnerIdAsync(int ownerId);
    }
}