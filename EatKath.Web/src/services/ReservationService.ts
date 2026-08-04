import api from "../api/axios";
import type { Reservation } from "../types/Reservation";

class ReservationService {

    // ----------------------------------------
    // Create Reservation
    // POST /api/reservation
    // ----------------------------------------

async getMyReservations(): Promise<Reservation[]> {

    const response = await api.get<Reservation[]>(
        "/reservation/my"
    );

    return response.data;

}


    async create(reservation: Reservation): Promise<Reservation> {

        const response = await api.post<Reservation>(
            "/reservation",
            reservation
        );

        return response.data;
    }

    // ----------------------------------------
    // Get All Reservations
    // GET /api/reservation
    // ----------------------------------------

    async getAll(): Promise<Reservation[]> {

        const response = await api.get<Reservation[]>(
            "/reservation"
        );

        return response.data;
    }

    // ----------------------------------------
    // Get Reservation
    // GET /api/reservation/{id}
    // ----------------------------------------

    async getById(id: number): Promise<Reservation> {

        const response = await api.get<Reservation>(
            `/reservation/${id}`
        );

        return response.data;
    }

    // ----------------------------------------
    // Delete Reservation
    // DELETE /api/reservation/{id}
    // ----------------------------------------

    async delete(id: number): Promise<void> {

        await api.delete(`/reservation/${id}`);
    }
}

export default new ReservationService();