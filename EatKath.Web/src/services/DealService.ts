import api from "../api/axios";
import type { Deal } from "../types/Deal";

class DealService {

    async getAll(): Promise<Deal[]> {

        const response = await api.get<Deal[]>("/deal");

        return response.data;
    }

    async getByRestaurant(restaurantId: number): Promise<Deal[]> {

        const response = await api.get<Deal[]>(
            `/deal/restaurant/${restaurantId}`
        );

        return response.data;
    }
}

export default new DealService();