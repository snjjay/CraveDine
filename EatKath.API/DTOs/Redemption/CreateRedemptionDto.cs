using System.ComponentModel.DataAnnotations;

namespace EatKath.API.DTOs.Redemption
{
    public class CreateRedemptionDto
    {
        [Required]
        public int DealId { get; set; }

        [Required]
        public DateOnly ArrivalDate { get; set; }

        [Required]
        public TimeOnly ArrivalTime { get; set; }

        [Range(1, 20)]
        public int GuestCount { get; set; }
    }
}