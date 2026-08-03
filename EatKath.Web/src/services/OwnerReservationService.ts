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

    async reject(id: number): Promise<void> {

        await api.put(
            `/reservation/${id}/reject`
        );

    }

    async arrived(id: number): Promise<void> {

        await api.put(
            `/reservation/${id}/arrived`
        );

    }

    async completed(id: number): Promise<void> {

        await api.put(
            `/reservation/${id}/completed`
        );

    }

    async noShow(id: number): Promise<void> {

        await api.put(
            `/reservation/${id}/no-show`
        );

    }

    async cancel(id: number): Promise<void> {

        await api.put(
            `/reservation/${id}/cancel`
        );

    }

}

export default new OwnerReservationService();