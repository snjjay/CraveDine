using EatKath.API.Enums;

namespace EatKath.API.Entities
{
    // A customer's reservation of one Surprise Bag listing. CreatedAt
    // (BaseEntity) is when it was reserved; all timestamps are UTC.
    public class SurpriseBagReservation : BaseEntity
    {
        public int SurpriseBagId { get; set; }

        // Customer
        public int UserId { get; set; }

        public int Quantity { get; set; } = 1;

        // The bag's price when reserved: the customer pays
        // Quantity x UnitPrice at pickup even if the price changes later.
        public decimal UnitPrice { get; set; }

        // Shown to the customer and checked by the restaurant at pickup.
        // Generated securely by the reservation service - never here.
        public string ConfirmationCode { get; set; } = string.Empty;

        public SurpriseBagReservationStatus Status { get; set; } = SurpriseBagReservationStatus.Reserved;

        public DateTime? CollectedAt { get; set; }

        public DateTime? CancelledAt { get; set; }

        // Navigation Properties
        public SurpriseBag SurpriseBag { get; set; } = null!;

        public User User { get; set; } = null!;
    }
}
