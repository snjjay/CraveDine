export interface RestaurantOpeningHour {

    id: number;

    restaurantId: number;

    dayOfWeek: number;

    openTime: string;

    closeTime: string;

    isClosed: boolean;

}