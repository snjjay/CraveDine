export interface Reservation {
    id?: number;

    dealId: number;

    customerName: string;

    phoneNumber: string;

    email?: string;

    reservationDate: string;

    reservationTime: string;

    guestCount: number;

    status?: string;

    confirmationCode?: string;
}