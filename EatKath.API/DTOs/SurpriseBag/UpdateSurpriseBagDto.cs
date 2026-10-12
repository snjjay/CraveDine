namespace EatKath.API.DTOs.SurpriseBag
{
    // No RestaurantId: a bag can't be moved to another restaurant.
    public class UpdateSurpriseBagDto : SurpriseBagInputDto
    {
        // Setting this back to true reactivates a deactivated bag.
        public bool IsActive { get; set; } = true;
    }
}
