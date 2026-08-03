import api from "../api/axios";

import type { Deal } from "../types/Deal";
import type { CreateDeal } from "../types/CreateDeal";
import type { UpdateDeal } from "../types/UpdateDeal";

class OwnerDealService {

    async getMyDeals(): Promise<Deal[]> {

        const response =
            await api.get<Deal[]>("/deal/my");

        return response.data;
    }

    async create(deal: CreateDeal): Promise<Deal> {

        const response =
            await api.post<Deal>("/deal", deal);

        return response.data;
    }

    async update(
        id: number,
        deal: UpdateDeal
    ): Promise<Deal> {

        const response =
            await api.put<Deal>(
                `/deal/${id}`,
                deal
            );

        return response.data;
    }

    async delete(id: number): Promise<void> {

        await api.delete(`/deal/${id}`);

    }

    async getById(id: number): Promise<Deal> {

        const response =
            await api.get<Deal>(`/deal/${id}`);

        return response.data;

    }

    
}

export default new OwnerDealService();