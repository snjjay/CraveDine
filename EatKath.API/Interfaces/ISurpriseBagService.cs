using EatKath.API.DTOs.SurpriseBag;

namespace EatKath.API.Interfaces
{
    public interface ISurpriseBagService
    {
        // Public (customer) views: only publicly visible bags.
        Task<IEnumerable<SurpriseBagDto>> GetPublicByRestaurantAsync(int restaurantId);

        // Visible bags across all active restaurants: reservable ones first,
        // then by pickup date and time. take is clamped to 1-100.
        Task<IEnumerable<SurpriseBagDto>> GetPublicAsync(int take);

        Task<SurpriseBagDto?> GetPublicByIdAsync(int id);

        // Owner/Admin management views: include inactive and past bags.
        Task<IEnumerable<OwnerSurpriseBagDto>> GetByOwnerAsync(int ownerId, int? restaurantId = null);

        Task<OwnerSurpriseBagDto?> GetManagedByIdAsync(int id);

        Task<OwnerSurpriseBagDto> CreateAsync(CreateSurpriseBagDto dto);

        Task<OwnerSurpriseBagDto> UpdateAsync(int id, UpdateSurpriseBagDto dto);

        // Soft delete: IsActive = false; existing reservations are kept.
        Task<bool> DeactivateAsync(int id);

        // Optional image (uploads or replaces it; returns its stored path).
        Task<string> UploadImageAsync(int id, IFormFile file);

        Task DeleteImageAsync(int id);
    }
}
