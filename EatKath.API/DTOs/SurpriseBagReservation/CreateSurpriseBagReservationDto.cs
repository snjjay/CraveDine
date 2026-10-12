namespace EatKath.API.DTOs.SurpriseBagReservation
{
    // The customer comes from the signed-in user, never from the request.
    public class CreateSurpriseBagReservationDto
    {
        public int SurpriseBagId { get; set; }

        public int Quantity { get; set; } = 1;
    }
}
