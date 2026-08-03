import api from "../api/axios";

import type { DiningType } from "../types/DiningType";

class DiningTypeService {

    async getAll(): Promise<DiningType[]> {

        const response =
            await api.get<DiningType[]>("/diningtype");

        return response.data;

    }

}

export default new DiningTypeService();