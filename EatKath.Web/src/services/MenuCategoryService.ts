import api from "../api/axios";

import type { MenuCategory } from "../types/MenuCategory";
import type { CreateMenuCategory } from "../types/CreateMenuCategory";
import type { UpdateMenuCategory } from "../types/UpdateMenuCategory";

class MenuCategoryService {

    async getByRestaurant(
        restaurantId: number
    ): Promise<MenuCategory[]> {

        const response =
            await api.get<MenuCategory[]>(
                `/MenuCategory/restaurant/${restaurantId}`
            );

        return response.data;

    }

    async create(
        category: CreateMenuCategory
    ): Promise<MenuCategory> {

        const response =
            await api.post<MenuCategory>(
                "/MenuCategory",
                category
            );

        return response.data;

    }

    async update(
        id: number,
        category: UpdateMenuCategory
    ): Promise<MenuCategory> {

        const response =
            await api.put<MenuCategory>(
                `/MenuCategory/${id}`,
                category
            );

        return response.data;

    }

    async delete(id: number): Promise<void> {

        await api.delete(`/MenuCategory/${id}`);

    }

}

export default new MenuCategoryService();