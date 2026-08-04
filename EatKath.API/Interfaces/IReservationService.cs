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

        Task<IEnumerable<ReservationDto>> GetMyReservationsAsync(int userId);

        Task<bool> ConfirmReservationAsync(int id);

        Task<bool> CancelReservationAsync(int id);

        Task<bool> RejectReservationAsync(int id);

        Task<bool> ArriveReservationAsync(int id);

        Task<bool> CompleteReservationAsync(int id);

        Task<bool> NoShowReservationAsync(int id);
    }
}