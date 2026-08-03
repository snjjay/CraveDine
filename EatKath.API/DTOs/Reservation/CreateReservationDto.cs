using System.ComponentModel.DataAnnotations;

namespace EatKath.API.DTOs.Reservation
{
    public class CreateReservationDto
    {
        [Required]
        public int DealId { get; set; }

        [Required]
        [MaxLength(100)]
        public string CustomerName { get; set; } = string.Empty;

        [Required]
        [MaxLength(20)]
        public string PhoneNumber { get; set; } = string.Empty;

        [EmailAddress]
        public string? Email { get; set; }

        [Required]
        public DateOnly ReservationDate { get; set; }

        [Required]
        public TimeOnly ReservationTime { get; set; }

        [Range(1, 20)]
        public int GuestCount { get; set; }
    }
}