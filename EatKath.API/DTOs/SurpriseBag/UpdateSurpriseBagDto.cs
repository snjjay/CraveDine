namespace EatKath.API.DTOs.SurpriseBag
{
    // No RestaurantId: a bag can't be moved to another restaurant.
    public class UpdateSurpriseBagDto : SurpriseBagInputDto
    {
        // true reactivates a deactivated bag, false deactivates it; left out
        // (null), the bag keeps its current state.
        public bool? IsActive { get; set; }
    }
}
