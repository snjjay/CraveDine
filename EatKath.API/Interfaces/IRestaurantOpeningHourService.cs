using EatKath.API.DTOs.RestaurantOpeningHour;

namespace EatKath.API.Interfaces
{
    public interface IRestaurantOpeningHourService
    {
        Task<IEnumerable<RestaurantOpeningHourDto>> GetAllAsync();

        Task<RestaurantOpeningHourDto?> GetByIdAsync(int id);

        Task<IEnumerable<RestaurantOpeningHourDto>> GetByRestaurantAsync(int restaurantId);

        Task<RestaurantOpeningHourDto> CreateAsync(CreateRestaurantOpeningHourDto dto);

        Task<RestaurantOpeningHourDto?> UpdateAsync(int id, UpdateRestaurantOpeningHourDto dto);

        Task<bool> DeleteAsync(int id);
    }
}