// A customer's Surprise Bag reservation.
// Matches: EatKath.API.DTOs.SurpriseBagReservation.SurpriseBagReservationDto
//
// Bag and restaurant details come from the reserved bag (also when it is
// now inactive or past). unitPrice is the price when reserved and
// totalPrice (quantity x unitPrice) is paid at pickup, in currencyCode.
// Pickup date and times are Kathmandu time.
export interface SurpriseBagReservation {
    id: number;
    surpriseBagId: number;
    bagTitle: string;
    // Numeric SurpriseBagCategory - see utils/surpriseBag.ts
    category: number;
    restaurantId: number;
    restaurantName: string;
    restaurantAddress: string;
    restaurantPhone: string;
    currencyCode: string;
    pickupDate: string;
    pickupStartTime: string;
    pickupEndTime: string;
    quantity: number;
    unitPrice: number;
    totalPrice: number;
    confirmationCode: string;
    // Numeric SurpriseBagReservationStatus - see utils/surpriseBag.ts
    status: number;
    createdAt: string;
    collectedAt: string | null;
    cancelledAt: string | null;
    // Calculated by the API: Reserved and the pickup window hasn't started.
    canCancel: boolean;
}

// Customer reserves a Surprise Bag. The customer is not sent - the API
// takes it from the logged-in account.
// Matches: EatKath.API.DTOs.SurpriseBagReservation.CreateSurpriseBagReservationDto
export interface CreateSurpriseBagReservation {
    surpriseBagId: number;
    quantity: number;
}
