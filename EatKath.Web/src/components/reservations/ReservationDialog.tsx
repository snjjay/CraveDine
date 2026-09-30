import { useEffect, useState } from "react";

import {
    Button,
    Dialog,
    DialogActions,
    DialogContent,
    DialogTitle,
    Stack,
    TextField
} from "@mui/material";

import ReservationService from "../../services/ReservationService";
import { useNotification } from "../../features/notifications/NotificationContext";
import type { Reservation } from "../../types/Reservation";
import type { Deal } from "../../types/Deal";

interface Props {
    open: boolean;
    onClose: () => void;
    deal: Deal;
}

// TimeOnly values come back from the API as "HH:mm:ss";
// the native time input only works in "HH:mm".
function toInputTime(value: string) {
    return value.length >= 5 ? value.slice(0, 5) : value;
}

function ReservationDialog({
    open,
    onClose,
    deal
}: Props) {

    const { notify } = useNotification();

    const [reservation, setReservation] = useState<Reservation>({
        dealId: deal.id,
        customerName: "",
        phoneNumber: "",
        email: "",
        reservationDate: "",
        reservationTime: "",
        guestCount: 2
    });

    const [submitting, setSubmitting] = useState(false);

    useEffect(() => {

        setReservation({
            dealId: deal.id,
            customerName: "",
            phoneNumber: "",
            email: "",
            reservationDate: "",
            reservationTime: "",
            guestCount: 2
        });

        setSubmitting(false);

    }, [deal.id, open]);

    const minTime = toInputTime(deal.startTime);
    const maxTime = toInputTime(deal.endTime);

    const guestCountError =
        reservation.guestCount < 1 ||
        reservation.guestCount > deal.maximumGuests;

    const dateError =
        reservation.reservationDate < deal.startDate ||
        reservation.reservationDate > deal.endDate;

    const timeError =
        reservation.reservationTime < minTime ||
        reservation.reservationTime > maxTime;

    const canSubmit =
        !submitting &&
        !guestCountError &&
        !dateError &&
        !timeError;

    async function handleSubmit() {

        if (submitting)
            return;

        if (guestCountError || dateError || timeError) {

            notify(
                "Please fix the highlighted fields before submitting.",
                "error"
            );

            return;

        }

        setSubmitting(true);

        try {

            const request: Reservation = {

                ...reservation,

                reservationTime:
                    reservation.reservationTime.length === 5
                        ? `${reservation.reservationTime}:00`
                        : reservation.reservationTime
            };

            const result = await ReservationService.create(request);

            notify(
                `Reservation created successfully. Confirmation code: ${result.confirmationCode}`,
                "success"
            );

            onClose();

        }
        catch (error: any) {

            console.error(error);

            notify(
                error.response?.data?.Message ??
                error.message ??
                "Unable to create reservation. Please try again.",
                "error"
            );

            setSubmitting(false);

        }

    }

    return (

        <Dialog
            open={open}
            onClose={onClose}
            fullWidth
            maxWidth="sm"
        >

            <DialogTitle>
                Reserve Table
            </DialogTitle>

            <DialogContent>

                <Stack
                    spacing={2}
                    sx={{ mt: 2 }}
                >

                    <TextField
                        label="Customer Name"
                        fullWidth
                        value={reservation.customerName}
                        onChange={(e) =>
                            setReservation({
                                ...reservation,
                                customerName: e.target.value
                            })
                        }
                    />

                    <TextField
                        label="Phone Number"
                        fullWidth
                        value={reservation.phoneNumber}
                        onChange={(e) =>
                            setReservation({
                                ...reservation,
                                phoneNumber: e.target.value
                            })
                        }
                    />

                    <TextField
                        label="Email"
                        fullWidth
                        value={reservation.email}
                        onChange={(e) =>
                            setReservation({
                                ...reservation,
                                email: e.target.value
                            })
                        }
                    />

                    <TextField
                        label="Reservation Date"
                        type="date"
                        slotProps={{
                            inputLabel: {
                                shrink: true
                            },
                            htmlInput: {
                                min: deal.startDate,
                                max: deal.endDate
                            }
                        }}
                        value={reservation.reservationDate}
                        error={reservation.reservationDate !== "" && dateError}
                        helperText={
                            reservation.reservationDate !== "" && dateError
                                ? `Must be between ${deal.startDate} and ${deal.endDate}.`
                                : " "
                        }
                        onChange={(e) =>
                            setReservation({
                                ...reservation,
                                reservationDate: e.target.value
                            })
                        }
                    />

                    <TextField
                        label="Reservation Time"
                        type="time"
                        slotProps={{
                            inputLabel: {
                                shrink: true
                            },
                            htmlInput: {
                                min: minTime,
                                max: maxTime
                            }
                        }}
                        value={reservation.reservationTime}
                        error={reservation.reservationTime !== "" && timeError}
                        helperText={
                            reservation.reservationTime !== "" && timeError
                                ? `Must be between ${minTime} and ${maxTime}.`
                                : " "
                        }
                        onChange={(e) =>
                            setReservation({
                                ...reservation,
                                reservationTime: e.target.value
                            })
                        }
                    />

                    <TextField
                        label="Guests"
                        type="number"
                        fullWidth
                        slotProps={{
                            htmlInput: {
                                min: 1,
                                max: deal.maximumGuests
                            }
                        }}
                        value={reservation.guestCount}
                        error={guestCountError}
                        helperText={
                            guestCountError
                                ? `Must be between 1 and ${deal.maximumGuests} guests.`
                                : " "
                        }
                        onChange={(e) =>
                            setReservation({
                                ...reservation,
                                guestCount: Number(e.target.value)
                            })
                        }
                    />

                </Stack>

            </DialogContent>

            <DialogActions>

                <Button
                    onClick={onClose}
                    disabled={submitting}
                >
                    Cancel
                </Button>

                <Button
                    variant="contained"
                    onClick={handleSubmit}
                    disabled={!canSubmit}
                >
                    {submitting ? "Reserving..." : "Confirm Reservation"}
                </Button>

            </DialogActions>

        </Dialog>

    );

}

export default ReservationDialog;
