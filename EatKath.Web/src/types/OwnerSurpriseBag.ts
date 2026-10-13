import type { SurpriseBag } from "./SurpriseBag";

// The owner's view of a Surprise Bag: also inactive and past bags, with
// reservation counts.
// Matches: EatKath.API.DTOs.SurpriseBag.OwnerSurpriseBagDto
export interface OwnerSurpriseBag extends SurpriseBag {
    isActive: boolean;
    quantityTotal: number;
    // Bags held by Reserved / Collected reservations.
    reservedQuantity: number;
    collectedQuantity: number;
    // Pickup date and times can't change while Reserved or Collected
    // reservations exist.
    isPickupLocked: boolean;
    createdAt: string;
    updatedAt: string | null;
}

// Fields an owner enters when creating or editing a bag.
// Matches: EatKath.API.DTOs.SurpriseBag.SurpriseBagInputDto
//
// Amounts are in the restaurant's currency. Pickup date ("YYYY-MM-DD")
// and times ("HH:mm:ss") are Kathmandu time.
export interface SurpriseBagInput {
    title: string;
    // Numeric SurpriseBagCategory - see utils/surpriseBag.ts
    category: number;
    description: string;
    conditions: string;
    price: number;
    estimatedValue: number | null;
    quantityTotal: number;
    maxPerCustomer: number;
    pickupDate: string;
    pickupStartTime: string;
    pickupEndTime: string;
}

// Matches: EatKath.API.DTOs.SurpriseBag.CreateSurpriseBagDto
export interface CreateSurpriseBag extends SurpriseBagInput {
    restaurantId: number;
}

// Matches: EatKath.API.DTOs.SurpriseBag.UpdateSurpriseBagDto
// (no restaurantId - a bag can't move to another restaurant).
export interface UpdateSurpriseBag extends SurpriseBagInput {
    // true reactivates, false deactivates; left out, the bag keeps its
    // current state.
    isActive?: boolean;
}
