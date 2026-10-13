import api from "../api/axios";

import type {
    CreateSurpriseBagReservation,
    SurpriseBagReservation
} from "../types/SurpriseBagReservation";

class SurpriseBagReservationService {

    // Customer reserves a Surprise Bag (paid at pickup).
    async reserve(dto: CreateSurpriseBagReservation): Promise<SurpriseBagReservation> {

        const response =
            await api.post<SurpriseBagReservation>(
                "/SurpriseBagReservation",
                dto
            );

        return response.data;

    }

    // The logged-in customer's reservations, newest first.
    async getMine(): Promise<SurpriseBagReservation[]> {

        const response =
            await api.get<SurpriseBagReservation[]>(
                "/SurpriseBagReservation/my"
            );

        return response.data;

    }

    // Customer cancels their own reservation (only while canCancel).
    async cancelMine(id: number): Promise<SurpriseBagReservation> {

        const response =
            await api.put<SurpriseBagReservation>(
                `/SurpriseBagReservation/${id}/cancel-mine`
            );

        return response.data;

    }

    // One of the logged-in customer's reservations. 404 when it isn't theirs.
    async getById(id: number): Promise<SurpriseBagReservation> {

        const response =
            await api.get<SurpriseBagReservation>(
                `/SurpriseBagReservation/${id}`
            );

        return response.data;

    }

}

export default new SurpriseBagReservationService();
