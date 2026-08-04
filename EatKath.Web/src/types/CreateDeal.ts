export interface CreateDeal {

    restaurantId: number;

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
}