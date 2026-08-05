export interface OwnerReservation {

    id: number;

    redemptionId?: number;

    dealId: number;

    dealTitle: string;

    customerName: string;

    phoneNumber: string;

    email: string;

    reservationDate: string;

    reservationTime: string;

    guestCount: number;

    status: string;
}