using System.ComponentModel.DataAnnotations;

namespace EatKath.API.DTOs.Redemption
{
    // Customer request to claim a walk-in offer.
    // Customer name, phone and email are NOT part of the request -
    // they always come from the authenticated user's account.
    public class CreateRedemptionDto
    {
        [Required]
        public int DealId { get; set; }

        [Required]
        public DateOnly ArrivalDate { get; set; }

        [Required]
        public TimeOnly ArrivalTime { get; set; }

        [Range(1, 50)]
        public int GuestCount { get; set; }
    }
}
