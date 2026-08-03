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

    async uploadLogo(
        id: number,
        file: File
    ): Promise<void> {

        const formData = new FormData();

        formData.append("file", file);

        await api.post(
            `/restaurant/${id}/logo`,
            formData,
            {
                headers: {
                    "Content-Type": "multipart/form-data"
                }
            }
        );

    }

    async uploadCover(
        id: number,
        file: File
    ): Promise<void> {

        const formData = new FormData();

        formData.append("file", file);

        await api.post(
            `/restaurant/${id}/cover`,
            formData,
            {
                headers: {
                    "Content-Type": "multipart/form-data"
                }
            }
        );

    }

    async uploadMenu(
        id: number,
        file: File
    ): Promise<void> {

        const formData = new FormData();

        formData.append("file", file);

        await api.post(
            `/restaurant/${id}/menu-pdf`,
            formData,
            {
                headers: {
                    "Content-Type": "multipart/form-data"
                }
            }
        );

    }

}

export default new OwnerRestaurantService();