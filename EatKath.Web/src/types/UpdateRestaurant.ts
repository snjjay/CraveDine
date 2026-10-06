export interface UpdateRestaurant {

    name: string;

    description: string;

    address: string;

    phoneNumber: string;

    email: string;

    website: string;

    areaId: number;

    currencyCode: string;

    isActive: boolean;
    // Optional: omitted = keep the current cuisines; supplied = replace
    // them with this set (at least one).
    cuisineIds?: number[];
}