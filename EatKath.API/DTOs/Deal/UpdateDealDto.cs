using EatKath.API.Enums;
using System.ComponentModel.DataAnnotations;

namespace EatKath.API.DTOs.Deal
{
    public class UpdateDealDto
    {
        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string Description { get; set; } = string.Empty;

        [Range(1, 100)]
        public decimal DiscountPercentage { get; set; }

        [Required]
        public OfferType OfferType { get; set; }

        public string PromoImageUrl { get; set; } = string.Empty;

        public string TermsAndConditions { get; set; } = string.Empty;

        [Required]
        public DateOnly StartDate { get; set; }

        [Required]
        public DateOnly EndDate { get; set; }

        [Required]
        public TimeOnly StartTime { get; set; }

        [Required]
        public TimeOnly EndTime { get; set; }

        [Range(1, 50)]
        public int MaximumGuests { get; set; }

        [Range(0, 180)]
        public int AdvanceRedeemMinutes { get; set; }

        [Range(0, 10000)]
        public int DailyRedemptionLimit { get; set; }

        public bool IsActive { get; set; }
    }
}