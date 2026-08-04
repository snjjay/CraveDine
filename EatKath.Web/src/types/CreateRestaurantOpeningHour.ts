export interface CreateRestaurantOpeningHour {

    restaurantId: number;

    dayOfWeek: number;

    openTime: string;

    closeTime: string;

    isClosed: boolean;

}