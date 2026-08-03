using EatKath.API.DTOs.Reservation;

namespace EatKath.API.Interfaces
{
    public interface IReservationService
    {
        Task<ReservationDto> CreateAsync(CreateReservationDto dto);

        Task<IEnumerable<ReservationDto>> GetAllAsync();

        Task<ReservationDto?> GetByIdAsync(int id);

        Task<bool> DeleteAsync(int id);

        Task<IEnumerable<ReservationDto>> GetOwnerReservationsAsync(int ownerId);

        Task<bool> ConfirmReservationAsync(int id);

        Task<bool> CancelReservationAsync(int id);


    }
}