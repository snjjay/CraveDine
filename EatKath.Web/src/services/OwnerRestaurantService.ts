import api from "../api/axios";

import type { Restaurant } from "../types/Restaurant";
import type { UpdateRestaurant } from "../types/UpdateRestaurant";

class OwnerRestaurantService {

    async getMyRestaurant(): Promise<Restaurant> {

        const response =
            await api.get<Restaurant>("/restaurant/my");

        return response.data;

    }

    async update(
        id: number,
        restaurant: UpdateRestaurant
    ): Promise<Restaurant> {

        const response =
            await api.put<Restaurant>(
                `/restaurant/${id}`,
                restaurant
            );

        return response.data;

    }

}

export default new OwnerRestaurantService();