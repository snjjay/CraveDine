import api from "../api/axios";
import type { Restaurant } from "../types/Restaurant";

class RestaurantService {

    async getAll(): Promise<Restaurant[]> {
        const response = await api.get<Restaurant[]>("/Restaurant");
        return response.data;
    }

    async getById(id: number): Promise<Restaurant> {
        const response = await api.get<Restaurant>(`/Restaurant/${id}`);
        return response.data;
    }
}

export default new RestaurantService();