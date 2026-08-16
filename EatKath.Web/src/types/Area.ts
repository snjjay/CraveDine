export interface Area {

    id: number;

    name: string;

}

// ==========================================================
// TYPES FOLDER
// ==========================================================
//
// Types = 📋 data blueprints.
//
// They describe the SHAPE of data used by the frontend.
//
// Example:
//
// Restaurant type
// → describes what a Restaurant object contains.
//
// UserFavorite type
// → describes what a UserFavorite object contains.
//
// ----------------------------------------------------------
//
// WHERE TYPES FIT:
//
// API data
//     ↓
// Type describes the data
//     ↓
// Page / Component uses the data
//
// Types do NOT:
// → call the API
// → send requests
// → store data
//
// They only describe what the data should look like.
//
// ----------------------------------------------------------
//
// EXAMPLES:
//
// Area.ts
// → Describes Area data.
//
// Restaurant.ts
// → Describes Restaurant data.
//
// CreateDeal.ts
// → Describes data sent when creating a Deal.
//
// Deal.ts
// → Describes Deal data used by the frontend.
//
// ----------------------------------------------------------
//
// EXAMPLE FROM RestaurantCard:
//
// import type { Restaurant } from "../../types/Restaurant";
//
// restaurant: Restaurant;
//
// → "restaurant must follow the Restaurant blueprint."
//
// ----------------------------------------------------------
//
// EXAMPLE FROM MyFavoritesPage:
//
// import type { UserFavorite } from "../types/UserFavorite";
//
// useState<UserFavorite[]>([])
//
// → "favorites is a list of UserFavorite objects."
//
// ----------------------------------------------------------
//
// 🔑 REMEMBER:
//
// Type      = What does the data look like?
// Service   = How do I get/send the data?
// Component = How do I display/use the data?
//
// ==========================================================

//So next: Area.ts.

// ==========================================================
// Area.ts
// ==========================================================
//
// Area = 📋 Blueprint for Area data.
//
// It says every Area object should have:
//
// id
// → number
// → Identifies the area.
//
// name
// → string
// → Stores the area name.
//
// ----------------------------------------------------------
//
// Example:
//
// const area: Area = {
//     id: 1,
//     name: "Brisbane"
// };
//
// ----------------------------------------------------------
//
// WHY WE USE IT:
//
// If a component receives:
//
// restaurant.area
//
// TypeScript knows what an Area looks like.
//
// It helps catch mistakes such as:
//
// id: "1"        ❌ should be a number
// name: 123      ❌ should be a string
//
// ----------------------------------------------------------
//
// 🔑 REMEMBER:
//
// interface Area
// → Defines the shape of Area data.
//
// id: number
// → Area ID must be a number.
//
// name: string
// → Area name must be text.
//
// ==========================================================
//Nextg is Restaurant.ts