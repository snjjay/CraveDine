using EatKath.API.Enums;

namespace EatKath.API.DTOs.Redemption
{
    public class RedemptionDto
    {
        public int Id { get; set; }

        public int DealId { get; set; }

        public string DealTitle { get; set; } = string.Empty;

        public int UserId { get; set; }

        public string CustomerName { get; set; } = string.Empty;

        public DateOnly ArrivalDate { get; set; }

        public TimeOnly ArrivalTime { get; set; }

        public int GuestCount { get; set; }

        public decimal? BillAmount { get; set; }

        public decimal? DiscountAmount { get; set; }

        public decimal? FinalAmount { get; set; }

        public RedemptionStatus Status { get; set; }

        public DateTime RedeemedAt { get; set; }

        public DateTime? CompletedAt { get; set; }
    }
}