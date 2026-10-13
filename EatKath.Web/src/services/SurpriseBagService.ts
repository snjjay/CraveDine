import api from "../api/axios";

import type { SurpriseBag } from "../types/SurpriseBag";

class SurpriseBagService {

    // A restaurant's bags for today (pickup window not yet ended) and
    // upcoming days, sold-out bags included (isSoldOut). Public.
    async getByRestaurant(restaurantId: number): Promise<SurpriseBag[]> {

        const response =
            await api.get<SurpriseBag[]>(
                `/SurpriseBag/restaurant/${restaurantId}`
            );

        return response.data;

    }

    // A publicly visible bag. The API returns 404 (with no message)
    // for an unknown, inactive or past bag. Public.
    async getById(id: number): Promise<SurpriseBag> {

        const response =
            await api.get<SurpriseBag>(
                `/SurpriseBag/${id}`
            );

        return response.data;

    }

}

export default new SurpriseBagService();
