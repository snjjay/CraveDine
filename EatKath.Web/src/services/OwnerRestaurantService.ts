import api from "../api/axios";

import type { Restaurant } from "../types/Restaurant";
import type { UpdateRestaurant } from "../types/UpdateRestaurant";
import type { CreateRestaurant } from "../types/CreateRestaurant";

class OwnerRestaurantService {

    async getMyRestaurant(): Promise<Restaurant> {

        const response =
            await api.get<Restaurant>("/restaurants/my");

        return response.data;

    }

    async create(
        restaurant: CreateRestaurant
    ): Promise<Restaurant> {

        const response =
            await api.post<Restaurant>(
                "/restaurants",
                restaurant
            );

        return response.data;

    }

    async update(
        id: number,
        restaurant: UpdateRestaurant
    ): Promise<Restaurant> {

        const response =
            await api.put<Restaurant>(
                `/restaurants/${id}`,
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
            `/restaurants/${id}/logo`,
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
            `/restaurants/${id}/cover`,
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
            `/restaurants/${id}/menu-pdf`,
            formData,
            {
                headers: {
                    "Content-Type": "multipart/form-data"
                }
            }
        );

    }

    async getMyRestaurants(): Promise<Restaurant[]> {

    const response =
        await api.get<Restaurant[]>("/restaurants/my");

    return response.data;

    }

}

export default new OwnerRestaurantService();