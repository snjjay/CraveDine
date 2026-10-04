import api from "../api/axios";

import type { Redemption } from "../types/Redemption";
import type { CompleteRedemption } from "../types/CompleteRedemption";
import type { CreateRedemption } from "../types/CreateRedemption";

class RedemptionService {

    // Customer claims a walk-in offer (no reservation is created).
    async redeem(dto: CreateRedemption): Promise<Redemption> {

        const response =
            await api.post<Redemption>(
                "/Redemption",
                dto
            );

        return response.data;

    }

    // Customer cancels their own claim.
    async cancelMine(id: number): Promise<Redemption> {

        const response =
            await api.put<Redemption>(
                `/Redemption/${id}/cancel-mine`
            );

        return response.data;

    }

    // Owner cancels a claim for their restaurant.
    async cancel(id: number): Promise<Redemption> {

        const response =
            await api.put<Redemption>(
                `/Redemption/${id}/cancel`
            );

        return response.data;

    }

    async getMyHistory(): Promise<Redemption[]> {

        const response =
            await api.get<Redemption[]>(
                "/Redemption/my-history"
            );

        return response.data;

    }

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