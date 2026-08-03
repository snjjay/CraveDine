// ==========================================================
// Restaurant
// ==========================================================
//
// Matches:
// EatKath.API.DTOs.Restaurant.RestaurantDto
//
// ==========================================================

// ==========================================================
// Restaurant
// ==========================================================

export interface Restaurant {

    id: number;

    name: string;

    description: string;

    address: string;

    phoneNumber: string;

    email: string;

    website: string;

    logoUrl: string;

    isActive: boolean;

    areaId: number;

    areaName: string;

    bestDiscount: number | null;

    activeDeals: number;

    isFavorite: boolean;

    cuisines: string[];

    diningTypes: string[];

}