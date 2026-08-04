using System.ComponentModel.DataAnnotations;

namespace EatKath.API.Entities
{
    public class Reservation
    {
        public int Id { get; set; }

        [Required]
        public int DealId { get; set; }

        public Deal Deal { get; set; } = null!;

        public int UserId { get; set; }

        public User User { get; set; } = null!;

        [Required]
        [MaxLength(100)]
        public string CustomerName { get; set; } = string.Empty;

        [Required]
        [MaxLength(20)]
        public string PhoneNumber { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? Email { get; set; }

        [Required]
        public DateOnly ReservationDate { get; set; }

        [Required]
        public TimeOnly ReservationTime { get; set; }

        [Range(1, 20)]
        public int GuestCount { get; set; }

        [Required]
        [MaxLength(20)]
        public string Status { get; set; } = ReservationStatus.Pending;

        [MaxLength(20)]
        public string ConfirmationCode { get; set; } = Guid.NewGuid().ToString("N")[..8].ToUpper();

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;



    }
}