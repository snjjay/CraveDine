import api from "../api/axios";

import type { RestaurantImage } from "../types/RestaurantImage";

class RestaurantImageService {

    // ----------------------------------------
    // Get Gallery Images For Restaurant
    // GET /api/restaurantimage/restaurant/{restaurantId}
    // ----------------------------------------

    async getByRestaurant(restaurantId: number): Promise<RestaurantImage[]> {

        const response =
            await api.get<RestaurantImage[]>(
                `/restaurantimage/restaurant/${restaurantId}`
            );

        return response.data;

    }

    // ----------------------------------------
    // Upload Gallery Image
    // POST /api/restaurantimage/upload
    // ----------------------------------------

    async upload(
        restaurantId: number,
        file: File
    ): Promise<RestaurantImage> {

        const formData = new FormData();

        formData.append("RestaurantId", restaurantId.toString());
        formData.append("File", file);

        const response =
            await api.post<RestaurantImage>(
                "/restaurantimage/upload",
                formData,
                {
                    headers: {
                        "Content-Type": "multipart/form-data"
                    }
                }
            );

        return response.data;

    }

    // ----------------------------------------
    // Delete Gallery Image
    // DELETE /api/restaurantimage/{id}
    // ----------------------------------------

    async delete(id: number): Promise<void> {

        await api.delete(`/restaurantimage/${id}`);

    }

}

export default new RestaurantImageService();
