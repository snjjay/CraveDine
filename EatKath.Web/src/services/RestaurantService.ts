import api from "../api/axios";
import type { Restaurant } from "../types/Restaurant";

// ==========================================================
// RESTAURANT SERVICE
// ==========================================================
//
// Handles all Restaurant API requests.
//
// ==========================================================

class RestaurantService {

    // ------------------------------------------------------
    // GET: /api/restaurant
    // Get all restaurants
    // ------------------------------------------------------
    async getAll(): Promise<Restaurant[]> {

        const response = await api.get<Restaurant[]>("/restaurant");

        return response.data;
    }

    // ------------------------------------------------------
    // GET: /api/restaurant/{id}
    // Get restaurant by id
    // ------------------------------------------------------
    async getById(id: number): Promise<Restaurant> {

        const response = await api.get<Restaurant>(`/restaurant/${id}`);

        return response.data;
    }
}

export default new RestaurantService();