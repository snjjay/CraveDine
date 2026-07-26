namespace EatKath.API.DTOs.Owner
{
    public class OwnerDashboardDto
    {
        public int ActiveDeals { get; set; }

        public int PendingRedemptions { get; set; }

        public int CompletedToday { get; set; }

        public decimal TodayRevenue { get; set; }

        public int CustomersServedToday { get; set; }
    }
}