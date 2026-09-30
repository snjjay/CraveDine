export interface Redemption {

    id: number;

    dealTitle: string;

    customerName: string;

    arrivalDate: string;

    arrivalTime: string;

    guestCount: number;

    status: string;

    billAmount: number | null;

    discountAmount: number | null;

    finalAmount: number | null;

    redeemedAt: string;

    completedAt?: string;

}