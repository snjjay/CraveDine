import { useEffect, useState } from "react";

import {
    Button,
    Chip,
    CircularProgress,
    Paper,
    Stack,
    Table,
    TableBody,
    TableCell,
    TableContainer,
    TableHead,
    TableRow,
    Typography
} from "@mui/material";

import OwnerReservationService from "../services/OwnerReservationService";
import type { OwnerReservation } from "../types/OwnerReservation";

function OwnerDashboardPage() {

    const [reservations, setReservations] = useState<OwnerReservation[]>([]);
    const [loading, setLoading] = useState(true);

    useEffect(() => {

        loadReservations();

    }, []);

    async function loadReservations() {

        try {

            const data =
                await OwnerReservationService.getAll();

            setReservations(data);

        }
        catch (error) {

            console.error(error);

        }
        finally {

            setLoading(false);

        }

    }

    async function confirmReservation(id: number) {

        await OwnerReservationService.confirm(id);

        loadReservations();

    }

    async function cancelReservation(id: number) {

        await OwnerReservationService.cancel(id);

        loadReservations();

    }

    function getStatusColor(status: string) {

        switch (status) {

            case "Confirmed":
                return "success";

            case "Cancelled":
                return "error";

            default:
                return "warning";

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
                Reservations
            </Typography>

            <TableContainer component={Paper}>

                <Table>

                    <TableHead>

                        <TableRow>

                            <TableCell>Customer</TableCell>
                            <TableCell>Deal</TableCell>
                            <TableCell>Date</TableCell>
                            <TableCell>Time</TableCell>
                            <TableCell>Guests</TableCell>
                            <TableCell>Status</TableCell>
                            <TableCell>Actions</TableCell>

                        </TableRow>

                    </TableHead>

                    <TableBody>

                        {reservations.map((reservation) => (

                            <TableRow key={reservation.id}>

                                <TableCell>{reservation.customerName}</TableCell>

                                <TableCell>{reservation.dealTitle}</TableCell>

                                <TableCell>{reservation.reservationDate}</TableCell>

                                <TableCell>{reservation.reservationTime}</TableCell>

                                <TableCell>{reservation.guestCount}</TableCell>

                                <TableCell>

                                    <Chip
                                        label={reservation.status}
                                        color={getStatusColor(reservation.status)}
                                    />

                                </TableCell>

                                <TableCell>

                                    <Stack direction="row" spacing={1}>

                                        <Button
                                            size="small"
                                            variant="contained"
                                            color="success"
                                            onClick={() => confirmReservation(reservation.id)}
                                        >
                                            Confirm
                                        </Button>

                                        <Button
                                            size="small"
                                            variant="contained"
                                            color="error"
                                            onClick={() => cancelReservation(reservation.id)}
                                        >
                                            Cancel
                                        </Button>

                                    </Stack>

                                </TableCell>

                            </TableRow>

                        ))}

                    </TableBody>

                </Table>

            </TableContainer>

        </>

    );

}

export default OwnerDashboardPage;