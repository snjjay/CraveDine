import api from "../api/axios";

import type { MenuItem } from "../types/MenuItem";
import type { CreateMenuItem } from "../types/CreateMenuItem";
import type { UpdateMenuItem } from "../types/UpdateMenuItem";

class MenuItemService {

    async getByRestaurant(
        restaurantId: number
    ): Promise<MenuItem[]> {

        const response =
            await api.get<MenuItem[]>(
                `/MenuItem/restaurant/${restaurantId}`
            );

        return response.data;

    }

    async getByCategory(
        categoryId: number
    ): Promise<MenuItem[]> {

        const response =
            await api.get<MenuItem[]>(
                `/MenuItem/category/${categoryId}`
            );

        return response.data;

    }

    async create(
        item: CreateMenuItem
    ): Promise<MenuItem> {

        const response =
            await api.post<MenuItem>(
                "/MenuItem",
                item
            );

        return response.data;

    }

    async update(
        id: number,
        item: UpdateMenuItem
    ): Promise<MenuItem> {

        const response =
            await api.put<MenuItem>(
                `/MenuItem/${id}`,
                item
            );

        return response.data;

    }

    async delete(id: number): Promise<void> {

        await api.delete(`/MenuItem/${id}`);

    }

    // Uploads (or replaces) the item's image and returns its new stored
    // path. onProgress receives 0-100 while the file is sent.
    async uploadImage(
        id: number,
        file: File,
        onProgress?: (percent: number) => void
    ): Promise<string> {

        const formData = new FormData();

        formData.append("file", file);

        const response =
            await api.post<{ imageUrl: string }>(
                `/MenuItem/${id}/image`,
                formData,
                {
                    headers: {
                        "Content-Type": "multipart/form-data"
                    },
                    // Longer than the default 30s: a 5 MB photo on a slow
                    // connection can take a while to send.
                    timeout: 120_000,
                    onUploadProgress: event => {
                        if (onProgress && event.total)
                            onProgress(Math.round((event.loaded / event.total) * 100));
                    }
                }
            );

        return response.data.imageUrl;

    }

    async deleteImage(id: number): Promise<void> {

        await api.delete(`/MenuItem/${id}/image`);

    }

}

export default new MenuItemService();