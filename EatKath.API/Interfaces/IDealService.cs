using EatKath.API.DTOs.Deal;

namespace EatKath.API.Interfaces
{
    public interface IDealService
    {
        Task<IEnumerable<DealDto>> GetAllAsync();

        Task<DealDto?> GetByIdAsync(int id);

        Task<DealDto> CreateAsync(CreateDealDto dto);

        Task<DealDto> UpdateAsync(int id, UpdateDealDto dto);

        Task<bool> DeleteAsync(int id);

        Task<IEnumerable<DealDto>> GetByRestaurantAsync(int restaurantId);


        Task<IEnumerable<DealDto>> GetByOwnerAsync(int ownerId);
    }
}