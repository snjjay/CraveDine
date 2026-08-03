import api from "../api/axios";

import type { Area } from "../types/Area";

class AreaService {

    async getAll(): Promise<Area[]> {

        const response =
           await api.get<Area[]>("/areas");

        return response.data;

    }

}

export default new AreaService();