namespace EatKath.API.DTOs.SurpriseBagReservation
{
    // The code the customer shows at pickup (any case; spaces trimmed).
    public class CollectSurpriseBagReservationDto
    {
        public string ConfirmationCode { get; set; } = string.Empty;
    }
}
