using EatKath.API.DTOs.SurpriseBagReservation;
using EatKath.API.Enums;

namespace EatKath.API.Interfaces
{
    public interface ISurpriseBagReservationService
    {
        // Customer (the signed-in user).
        Task<SurpriseBagReservationDto> CreateAsync(CreateSurpriseBagReservationDto dto);

        Task<IEnumerable<SurpriseBagReservationDto>> GetMyAsync();

        Task<SurpriseBagReservationDto> CancelMineAsync(int id);

        // The customer, the restaurant's owner or an Admin; otherwise null.
        Task<SurpriseBagReservationDto?> GetByIdAsync(int id);

        // Restaurant owner or Admin.
        Task<IEnumerable<OwnerSurpriseBagReservationDto>> GetForRestaurantAsync(
            int restaurantId,
            DateOnly? pickupDate = null,
            SurpriseBagReservationStatus? status = null);

        Task<OwnerSurpriseBagReservationDto> CancelAsync(int id);

        Task<OwnerSurpriseBagReservationDto> CollectAsync(CollectSurpriseBagReservationDto dto);

        Task<OwnerSurpriseBagReservationDto> MarkNoShowAsync(int id);
    }
}
