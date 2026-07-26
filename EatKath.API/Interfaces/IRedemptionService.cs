using EatKath.API.DTOs.Redemption;

namespace EatKath.API.Interfaces
{
    public interface IRedemptionService
    {
        // Customer redeems a deal/offer
        Task<RedemptionDto> RedeemAsync(CreateRedemptionDto dto);

        // Restaurant completes redemption after payment
        Task<RedemptionDto> CompleteRedemptionAsync(
            int redemptionId,
            CompleteRedemptionDto dto);

        // Customer redemption history
        Task<IEnumerable<RedemptionDto>> GetMyHistoryAsync();

        // Restaurant redemption history
        Task<IEnumerable<RedemptionDto>> GetRestaurantRedemptionsAsync(int restaurantId);

        // Single redemption
        Task<RedemptionDto?> GetByIdAsync(int id);
    }
}