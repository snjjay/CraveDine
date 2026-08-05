import api from "../api/axios";

import type { Redemption } from "../types/Redemption";
import type { CompleteRedemption } from "../types/CompleteRedemption";

class RedemptionService {

    async getRestaurantRedemptions(
        restaurantId: number
    ): Promise<Redemption[]> {

        const response =
            await api.get<Redemption[]>(
                `/Redemption/restaurant/${restaurantId}`
            );

        return response.data;

    }

    async complete(
        id: number,
        dto: CompleteRedemption
    ): Promise<Redemption> {

        const response =
            await api.post<Redemption>(
                `/Redemption/${id}/complete`,
                dto
            );

        return response.data;

    }

}

export default new RedemptionService();