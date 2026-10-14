// Public view of a Surprise Bag.
// Matches: EatKath.API.DTOs.SurpriseBag.SurpriseBagDto
//
// Amounts are in currencyCode (the restaurant's currency). Pickup date
// ("YYYY-MM-DD") and times ("HH:mm:ss") are Kathmandu time; the API
// decides which bags are still visible, so the browser's clock is not
// used to work out expiry.
export interface SurpriseBag {
    id: number;
    restaurantId: number;
    restaurantName: string;
    currencyCode: string;
    title: string;
    // Numeric SurpriseBagCategory - see utils/surpriseBag.ts
    category: number;
    description: string;
    conditions: string;
    price: number;
    estimatedValue: number | null;
    maxPerCustomer: number;
    pickupDate: string;
    pickupStartTime: string;
    pickupEndTime: string;
    // Stored image path (use getImageUrl), or null: show the category icon.
    imageUrl: string | null;
    // Calculated by the API (Reserved and Collected reservations hold stock).
    remainingQuantity: number;
    isSoldOut: boolean;
}
