namespace EatKath.API.DTOs.Reservation
{
    public class OwnerReservationDto
    {
        public int Id { get; set; }

        public int? RedemptionId { get; set; }

        public int DealId { get; set; }

        public string DealTitle { get; set; } = string.Empty;

        public string CustomerName { get; set; } = string.Empty;

        public string PhoneNumber { get; set; } = string.Empty;

        public string? Email { get; set; }

        public DateOnly ReservationDate { get; set; }

        public TimeOnly ReservationTime { get; set; }

        public int GuestCount { get; set; }

        public string Status { get; set; } = string.Empty;
    }
}