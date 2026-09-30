import { useEffect, useState } from "react";
import { Link as RouterLink } from "react-router-dom";

import {
    Button,
    Card,
    CardContent,
    Chip,
    CircularProgress,
    Dialog,
    DialogActions,
    DialogContent,
    DialogContentText,
    DialogTitle,
    Stack,
    Typography
} from "@mui/material";

import ReservationService from "../services/ReservationService";
import { useNotification } from "../features/notifications/NotificationContext";

import type { Reservation } from "../types/Reservation";

const CANCELLABLE_STATUSES = ["Pending", "Confirmed"];

function MyReservationsPage() {

    const { notify } = useNotification();

    const [reservations, setReservations] =
        useState<Reservation[]>([]);

    const [loading, setLoading] =
        useState(true);

    const [cancelTargetId, setCancelTargetId] =
        useState<number | null>(null);

    useEffect(() => {

        loadReservations();

    }, []);

    async function loadReservations() {

        try {

            const data =
                await ReservationService.getMyReservations();

            setReservations(data);

        }
        catch (error: any) {

            console.error(error);

            notify(
                error.response?.data?.Message ??
                error.message ??
                "Failed to load your reservations. Please try again.",
                "error"
            );

        }
        finally {

            setLoading(false);

        }

    }

    function handleCancelClick(id?: number) {

        if (id === undefined)
            return;

        setCancelTargetId(id);

    }

    function handleCloseCancelDialog() {

        setCancelTargetId(null);

    }

    async function handleConfirmCancel() {

        if (cancelTargetId === null)
            return;

        try {

            await ReservationService.cancelMine(cancelTargetId);

            notify("Reservation cancelled.", "success");

            await loadReservations();

        }
        catch (error: any) {

            console.error(error);

            notify(
                error.response?.data?.Message ??
                error.message ??
                "Failed to cancel reservation. Please try again.",
                "error"
            );

        }
        finally {

            setCancelTargetId(null);

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

            {reservations.length === 0 ? (

                <Stack spacing={2} alignItems="flex-start">

                    <Typography>
                        You have no reservations yet.
                    </Typography>

                    <Button
                        variant="contained"
                        component={RouterLink}
                        to="/restaurants"
                    >
                        Browse Restaurants
                    </Button>

                </Stack>

            ) : (

            <Stack spacing={2}>

                {reservations.map(r => (

                    <Card key={r.id}>

                        <CardContent>

                            {r.restaurantName && (

                                <Typography
                                    variant="subtitle1"
                                    color="text.secondary"
                                >

                                    {r.restaurantName}

                                </Typography>

                            )}

                            {r.dealTitle && (

                                <Typography variant="h6">

                                    {r.dealTitle}

                                </Typography>

                            )}

                            <Typography
                                variant="body2"
                                color="text.secondary"
                                sx={{ mb: 1 }}
                            >

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

                            <Typography>

                                Confirmation Code: {r.confirmationCode}

                            </Typography>

                            <Chip
                                sx={{ mt: 2 }}
                                label={r.status}
                                color={getChipColor(r.status)}
                            />

                            {CANCELLABLE_STATUSES.includes(r.status ?? "") && (

                                <Stack sx={{ mt: 2 }}>

                                    <Button
                                        variant="outlined"
                                        color="error"
                                        onClick={() => handleCancelClick(r.id)}
                                    >
                                        Cancel Reservation
                                    </Button>

                                </Stack>

                            )}

                        </CardContent>

                    </Card>

                ))}

            </Stack>

            )}

            <Dialog
                open={cancelTargetId !== null}
                onClose={handleCloseCancelDialog}
            >

                <DialogTitle>
                    Cancel Reservation
                </DialogTitle>

                <DialogContent>

                    <DialogContentText>
                        Are you sure you want to cancel this reservation? This cannot be undone.
                    </DialogContentText>

                </DialogContent>

                <DialogActions>

                    <Button onClick={handleCloseCancelDialog}>
                        Keep Reservation
                    </Button>

                    <Button
                        color="error"
                        variant="contained"
                        onClick={handleConfirmCancel}
                    >
                        Cancel Reservation
                    </Button>

                </DialogActions>

            </Dialog>

        </>

    );

}

export default MyReservationsPage;