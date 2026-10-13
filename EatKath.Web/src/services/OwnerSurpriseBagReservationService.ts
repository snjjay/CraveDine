import api from "../api/axios";

import type {
    OwnerSurpriseBagReservation,
    SurpriseBagReservationFilters
} from "../types/OwnerSurpriseBagReservation";

class OwnerSurpriseBagReservationService {

    // A restaurant's reservations (with customer details), latest pickup
    // date first. 404 if the restaurant isn't the owner's.
    async getForRestaurant(
        restaurantId: number,
        filters: SurpriseBagReservationFilters = {}
    ): Promise<OwnerSurpriseBagReservation[]> {

        const params: Record<string, string | number> = {};

        if (filters.pickupDate)
            params.pickupDate = filters.pickupDate;

        if (filters.status)
            params.status = filters.status;

        const response =
            await api.get<OwnerSurpriseBagReservation[]>(
                `/SurpriseBagReservation/restaurant/${restaurantId}`,
                { params }
            );

        return response.data;

    }

    // Reserved -> Collected, by the code the customer shows (trimmed; the
    // API matches it case-insensitively). Only on the pickup date.
    async collect(confirmationCode: string): Promise<OwnerSurpriseBagReservation> {

        const response =
            await api.post<OwnerSurpriseBagReservation>(
                "/SurpriseBagReservation/collect",
                { confirmationCode: confirmationCode.trim() }
            );

        return response.data;

    }

    // Reserved -> NoShow. Only after the pickup window has ended.
    async markNoShow(id: number): Promise<OwnerSurpriseBagReservation> {

        const response =
            await api.put<OwnerSurpriseBagReservation>(
                `/SurpriseBagReservation/${id}/no-show`
            );

        return response.data;

    }

    // Reserved -> Cancelled (the bag is released).
    async cancel(id: number): Promise<OwnerSurpriseBagReservation> {

        const response =
            await api.put<OwnerSurpriseBagReservation>(
                `/SurpriseBagReservation/${id}/cancel`
            );

        return response.data;

    }

}

export default new OwnerSurpriseBagReservationService();
