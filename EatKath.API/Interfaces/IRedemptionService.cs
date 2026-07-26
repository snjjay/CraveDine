using EatKath.API.DTOs.Redemption;

namespace EatKath.API.Interfaces
{
    public interface IRedemptionService
    {
        Task<RedemptionDto> RedeemAsync(CreateRedemptionDto dto);

        Task<IEnumerable<RedemptionDto>> GetMyHistoryAsync();

        Task<IEnumerable<RedemptionDto>> GetRestaurantRedemptionsAsync(int restaurantId);

        Task<RedemptionDto?> GetByIdAsync(int id);
    }
}