namespace EatKath.API.Entities
{
    public class RestaurantImage : BaseEntity
    {
        public int RestaurantId { get; set; }

        public string ImageUrl { get; set; } = string.Empty;

        public string? Caption { get; set; }

        public int DisplayOrder { get; set; }

        public bool IsPrimary { get; set; }

        public Restaurant Restaurant { get; set; } = null!;
    }
}