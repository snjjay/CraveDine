// Walk-in offer claim. Customer name, phone and email are not sent -
// the API takes them from the logged-in account.
export interface CreateRedemption {
    dealId: number;
    arrivalDate: string;
    arrivalTime: string;
    guestCount: number;
}
