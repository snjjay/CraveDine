import api from "../api/axios";

import type { Cuisine } from "../types/Cuisine";

class CuisineService {

    async getAll(): Promise<Cuisine[]> {

        const response =
            await api.get<Cuisine[]>("/cuisine");

        return response.data;

    }

}

export default new CuisineService();