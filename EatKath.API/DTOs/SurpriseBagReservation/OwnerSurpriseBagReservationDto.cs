namespace EatKath.API.DTOs.SurpriseBagReservation
{
    // The restaurant's (and Admin's) view: also who reserved it, from the
    // customer's account.
    public class OwnerSurpriseBagReservationDto : SurpriseBagReservationDto
    {
        public int UserId { get; set; }

        public string CustomerName { get; set; } = string.Empty;

        public string CustomerPhone { get; set; } = string.Empty;

        public string CustomerEmail { get; set; } = string.Empty;
    }
}
