export interface RestaurantImage {

    id: number;

    restaurantId: number;

    imageUrl: string;

    caption?: string;

    displayOrder: number;

    isPrimary: boolean;

}
