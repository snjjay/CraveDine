import api from "../api/axios";

import type { UserFavorite } from "../types/UserFavorite";

class UserFavoriteService {

    async getMyFavorites(): Promise<UserFavorite[]> {

        const response =
            await api.get<UserFavorite[]>("/userfavorite");

        return response.data;

    }

    async add(restaurantId: number): Promise<void> {

        await api.post(
            "/userfavorite",
            {
                restaurantId
            }
        );

    }

    async remove(restaurantId: number): Promise<void> {

        await api.delete(
            "/userfavorite",
            {
                data: {
                    restaurantId
                }
            }
        );

    }

}

export default new UserFavoriteService();