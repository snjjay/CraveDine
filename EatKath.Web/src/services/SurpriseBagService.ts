import api from "../api/axios";

import type { SurpriseBag } from "../types/SurpriseBag";

class SurpriseBagService {

    // Open and upcoming bags across all active restaurants: bags that can
    // still be reserved first (soonest pickup first), then sold-out ones.
    // The API returns at most `take` bags (1-100). Public.
    async getAll(take: number): Promise<SurpriseBag[]> {

        const response =
            await api.get<SurpriseBag[]>(
                "/SurpriseBag",
                { params: { take } }
            );

        return response.data;

    }

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
