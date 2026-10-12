using EatKath.API.Enums;

namespace EatKath.API.DTOs.SurpriseBag
{
    // Fields an owner enters when creating or editing a Surprise Bag
    // (validated by SurpriseBagInputValidator). Amounts are in the
    // restaurant's currency; pickup date and times are Kathmandu time.
    public abstract class SurpriseBagInputDto
    {
        public string Title { get; set; } = string.Empty;

        public SurpriseBagCategory Category { get; set; }

        public string Description { get; set; } = string.Empty;

        public string Conditions { get; set; } = string.Empty;

        public decimal Price { get; set; }

        public decimal? EstimatedValue { get; set; }

        public int QuantityTotal { get; set; }

        public int MaxPerCustomer { get; set; } = 1;

        public DateOnly PickupDate { get; set; }

        public TimeOnly PickupStartTime { get; set; }

        public TimeOnly PickupEndTime { get; set; }
    }
}
