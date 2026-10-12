namespace EatKath.API.Enums
{
    public enum SurpriseBagReservationStatus
    {
        // Holds bags until collected or cancelled.
        Reserved = 1,

        // Paid at the restaurant and handed over.
        Collected = 2,

        // Released back to the bag's available quantity.
        Cancelled = 3,

        // Not collected; marked by the owner after the pickup window.
        NoShow = 4
    }
}
