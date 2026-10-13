import type { SurpriseBagReservation } from "./SurpriseBagReservation";

// The restaurant's view of a Surprise Bag reservation: also who reserved
// it, from the customer's account.
// Matches: EatKath.API.DTOs.SurpriseBagReservation.OwnerSurpriseBagReservationDto
//
// Note: canCancel (inherited) is the CUSTOMER's rule (Reserved and before
// the pickup window starts). The restaurant can cancel any Reserved
// reservation, so owner screens don't use it.
export interface OwnerSurpriseBagReservation extends SurpriseBagReservation {
    userId: number;
    customerName: string;
    customerPhone: string;
    customerEmail: string;
}

// Optional filters for a restaurant's reservations (applied by the API).
export interface SurpriseBagReservationFilters {
    // "YYYY-MM-DD" (the bag's pickup date)
    pickupDate?: string;
    // Numeric SurpriseBagReservationStatus - see utils/surpriseBag.ts
    status?: number;
}
