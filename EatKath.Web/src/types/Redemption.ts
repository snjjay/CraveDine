export interface Redemption {
    id: number;
    dealId: number;
    dealTitle: string;
    restaurantId: number;
    restaurantName: string;
    userId: number;
    customerName: string;
    customerPhone: string;
    customerEmail: string;
    reservationId: number | null;
    arrivalDate: string;
    arrivalTime: string;
    guestCount: number;
    // Numeric RedemptionStatus - see utils/redemption.ts
    status: number;
    billAmount: number | null;
    discountAmount: number | null;
    finalAmount: number | null;
    currencyCode: string;
    redeemedAt: string;
    completedAt?: string;
}
