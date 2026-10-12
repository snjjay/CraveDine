using EatKath.API.DTOs.SurpriseBag;

namespace EatKath.API.Interfaces
{
    public interface ISurpriseBagService
    {
        // Public (customer) views: only publicly visible bags.
        Task<IEnumerable<SurpriseBagDto>> GetPublicByRestaurantAsync(int restaurantId);

        Task<SurpriseBagDto?> GetPublicByIdAsync(int id);

        // Owner/Admin management views: include inactive and past bags.
        Task<IEnumerable<OwnerSurpriseBagDto>> GetByOwnerAsync(int ownerId, int? restaurantId = null);

        Task<OwnerSurpriseBagDto?> GetManagedByIdAsync(int id);

        Task<OwnerSurpriseBagDto> CreateAsync(CreateSurpriseBagDto dto);

        Task<OwnerSurpriseBagDto> UpdateAsync(int id, UpdateSurpriseBagDto dto);

        // Soft delete: IsActive = false; existing reservations are kept.
        Task<bool> DeactivateAsync(int id);
    }
}
