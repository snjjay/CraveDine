export interface Redemption {

    id: number;

    dealTitle: string;

    userName: string;

    arrivalDate: string;

    arrivalTime: string;

    guestCount: number;

    status: string;

    billAmount?: number;

    discountAmount?: number;

    finalAmount?: number;

}