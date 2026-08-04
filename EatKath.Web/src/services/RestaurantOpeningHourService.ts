import api from "../api/axios";

import type { RestaurantOpeningHour } from "../types/RestaurantOpeningHour";

class RestaurantOpeningHourService {

    async getByRestaurant(
        restaurantId: number
    ): Promise<RestaurantOpeningHour[]> {

        const response =
            await api.get<RestaurantOpeningHour[]>(
                `/RestaurantOpeningHour/restaurant/${restaurantId}`
            );

        return response.data;

    }

    async update(
        id: number,
        openingHour: RestaurantOpeningHour
    ): Promise<RestaurantOpeningHour> {

        const response =
            await api.put<RestaurantOpeningHour>(
                `/RestaurantOpeningHour/${id}`,
                openingHour
            );

        return response.data;

    }

    async create(
        openingHour: RestaurantOpeningHour
    ): Promise<RestaurantOpeningHour> {

        const response =
            await api.post<RestaurantOpeningHour>(
                "/RestaurantOpeningHour",
                openingHour
            );

        return response.data;

    }

}

export default new RestaurantOpeningHourService();