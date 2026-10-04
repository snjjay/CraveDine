export interface Deal {
    id: number;
    restaurantId: number;
    restaurantName: string;
    title: string;
    description: string;
    discountPercentage: number;
    offerType: number;
    promoImageUrl: string;
    termsAndConditions: string;
    startDate: string;
    endDate: string;
    startTime: string;
    endTime: string;
    maximumGuests: number;
    reservationLimit: number;
    dailyRedemptionLimit: number;
    isActive: boolean;
    // Walk-in availability (only on the restaurant deal list).
    // remainingOffers: null = unlimited.
    // availabilityDate: date remainingOffers refers to; null = offer ended.
    remainingOffers?: number | null;
    availabilityDate?: string | null;
    // Total offer cap used up - no date can be claimed.
    isSoldOut?: boolean;
}