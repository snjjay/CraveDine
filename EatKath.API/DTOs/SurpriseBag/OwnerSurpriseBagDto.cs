namespace EatKath.API.DTOs.SurpriseBag
{
    // The owner's view: also inactive and past bags, with reservation counts.
    public class OwnerSurpriseBagDto : SurpriseBagDto
    {
        public bool IsActive { get; set; }

        public int QuantityTotal { get; set; }

        // Calculated by SurpriseBagService, not mapped.
        public int ReservedQuantity { get; set; }

        public int CollectedQuantity { get; set; }

        // Pickup date and times can't change while Reserved or Collected
        // reservations exist.
        public bool IsPickupLocked { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }
    }
}
