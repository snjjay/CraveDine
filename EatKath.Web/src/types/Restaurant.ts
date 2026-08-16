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


// ==========================================================
// Restaurant.ts
// ==========================================================
//
// Restaurant = 📋 Blueprint for Restaurant data.
//
// It describes what information a Restaurant object contains.
//
// ----------------------------------------------------------
//
// IMPORTANT:
//
// id: number
// → Restaurant ID.
//
// name: string
// → Restaurant name.
//
// logoUrl: string
// → Restaurant image/path.
//
// areaName: string
// → Restaurant's area name.
//
// bestDiscount: number | null
// → Discount percentage OR null if there is no discount.
//
// activeDeals: number
// → Number of active deals.
//
// isFavorite: boolean
// → Whether this restaurant is currently a favourite.
//
// cuisines: string[]
// → List of cuisine names.
//
// diningTypes: string[]
// → List of dining type names.
//
// ----------------------------------------------------------
//
// string[]
// → A LIST/ARRAY of strings.
//
// Example:
//
// cuisines: ["Indian", "Nepali", "Thai"]
//
// ----------------------------------------------------------
//
// number | null
// → Can contain a number OR no value.
//
// Example:
//
// bestDiscount: 25
// OR
// bestDiscount: null
//
// ----------------------------------------------------------
//
// HOW IT IS USED:
//
// .NET API
//      ↓
// Restaurant data
//      ↓
// Restaurant.ts
//      ↓
// Describes/checks the data shape
//      ↓
// RestaurantCard / Pages
//
// Example:
//
// restaurant.name
// restaurant.logoUrl
// restaurant.activeDeals
//
// TypeScript knows these fields because Restaurant.ts
// defines them.
//
// 🔑 Remember:
//
// Type = "What does the data look like?"
// ==========================================================

//Next is CreateDeal.ts, because it introduces an important distinction: types used for sending data to the API versus types used for receiving/displaying data.