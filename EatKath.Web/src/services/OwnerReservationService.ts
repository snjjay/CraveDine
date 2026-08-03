import api from "../api/axios";
import type { OwnerReservation } from "../types/OwnerReservation";

class OwnerReservationService {

    async getAll(): Promise<OwnerReservation[]> {

        const response =
            await api.get<OwnerReservation[]>(
                "/reservation/owner"
            );

        return response.data;
    }

    async confirm(id: number): Promise<void> {

        await api.put(
            `/reservation/${id}/confirm`
        );

    }

    async cancel(id: number): Promise<void> {

        await api.put(
            `/reservation/${id}/cancel`
        );

    }

}

export default new OwnerReservationService();