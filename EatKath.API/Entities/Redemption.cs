using EatKath.API.Enums;

namespace EatKath.API.Entities
{
    public class Redemption : BaseEntity
    {
        // Offer redeemed
        public int DealId { get; set; }

        // Customer
        public int UserId { get; set; }

        // Customer selected during redemption
        public DateOnly ArrivalDate { get; set; }

        public TimeOnly ArrivalTime { get; set; }

        public int GuestCount { get; set; }

        // Restaurant enters these values when completing redemption
        public decimal? BillAmount { get; set; }

        public decimal? DiscountAmount { get; set; }

        public decimal? FinalAmount { get; set; }

        // Workflow Status
        public RedemptionStatus Status { get; set; } = RedemptionStatus.Redeemed;

        public DateTime RedeemedAt { get; set; } = DateTime.UtcNow;

        public DateTime? CompletedAt { get; set; }

        // Navigation Properties
        public Deal Deal { get; set; } = null!;

        public User User { get; set; } = null!;
    }
}