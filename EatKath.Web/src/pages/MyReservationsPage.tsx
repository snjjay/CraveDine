import { useEffect, useState } from "react";

import {
    Card,
    CardContent,
    Chip,
    CircularProgress,
    Stack,
    Typography
} from "@mui/material";

import ReservationService from "../services/ReservationService";

import type { Reservation } from "../types/Reservation";

function MyReservationsPage() {

    const [reservations, setReservations] =
        useState<Reservation[]>([]);

    const [loading, setLoading] =
        useState(true);

    useEffect(() => {

        loadReservations();

    }, []);

    async function loadReservations() {

        try {

            const data =
                await ReservationService.getMyReservations();

            setReservations(data);

        }
        finally {

            setLoading(false);

        }

    }

    function getChipColor(status?: string) {

        switch (status) {

            case "Pending":
                return "warning";

            case "Confirmed":
                return "success";

            case "Arrived":
                return "info";

            case "Completed":
                return "success";

            case "Rejected":
                return "error";

            case "Cancelled":
                return "default";

            case "NoShow":
                return "error";

            default:
                return "default";

        }

    }

    if (loading)
        return <CircularProgress />;

    return (

        <>

            <Typography
                variant="h4"
                sx={{ mb: 3 }}
            >
                My Reservations
            </Typography>

            <Stack spacing={2}>

                {reservations.map(r => (

                    <Card key={r.id}>

                        <CardContent>

                            <Typography variant="h6">

                                Reservation #{r.id}

                            </Typography>

                            <Typography>

                                Date: {r.reservationDate}

                            </Typography>

                            <Typography>

                                Time: {r.reservationTime}

                            </Typography>

                            <Typography>

                                Guests: {r.guestCount}

                            </Typography>

                            <Chip
                                sx={{ mt: 2 }}
                                label={r.status}
                                color={getChipColor(r.status)}
                            />

                        </CardContent>

                    </Card>

                ))}

            </Stack>

        </>

    );

}

export default MyReservationsPage;