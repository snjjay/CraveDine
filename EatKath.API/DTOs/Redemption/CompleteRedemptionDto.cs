using System.ComponentModel.DataAnnotations;

namespace EatKath.API.DTOs.Redemption
{
    public class CompleteRedemptionDto
    {
        [Required]
        [Range(0.01, 1000000)]
        public decimal BillAmount { get; set; }
    }
}