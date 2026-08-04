export interface MenuItem {

    id: number;

    restaurantId: number;

    menuCategoryId: number;

    name: string;

    description: string;

    price: number;

    imageUrl?: string;

    isFeatured: boolean;

    isAvailable: boolean;

}