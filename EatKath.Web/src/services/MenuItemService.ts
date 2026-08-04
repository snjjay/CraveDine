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

}

export default new MenuItemService();