namespace EatKath.API.DTOs.Redemption
{
    public class RedemptionDto
    {
        public int Id { get; set; }

        public int DealId { get; set; }

        public string DealTitle { get; set; } = string.Empty;

        public int UserId { get; set; }

        public string CustomerName { get; set; } = string.Empty;

        public decimal RedemptionAmount { get; set; }

        public DateTime RedeemedAt { get; set; }
    }
}