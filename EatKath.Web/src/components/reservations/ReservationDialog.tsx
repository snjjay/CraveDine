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
import type { Reservation } from "../../types/Reservation";

interface Props {
    open: boolean;
    onClose: () => void;
    dealId: number;
}

function ReservationDialog({
    open,
    onClose,
    dealId
}: Props) {

    const [reservation, setReservation] = useState<Reservation>({
        dealId,
        customerName: "",
        phoneNumber: "",
        email: "",
        reservationDate: "",
        reservationTime: "",
        guestCount: 2
    });

    useEffect(() => {

        setReservation({
            dealId,
            customerName: "",
            phoneNumber: "",
            email: "",
            reservationDate: "",
            reservationTime: "",
            guestCount: 2
        });

    }, [dealId, open]);

    async function handleSubmit() {

        try {

            const request: Reservation = {

                ...reservation,

                reservationTime:
                    reservation.reservationTime.length === 5
                        ? `${reservation.reservationTime}:00`
                        : reservation.reservationTime
            };

            console.log(request);

            const result = await ReservationService.create(request);

            console.log(result);

            alert("Reservation created successfully!");

            onClose();

        }
        catch (error: any) {

    console.error(error);

    if (error.response?.status === 400) {

        alert(error.response.data);

    }
    else {

        alert(
            "Unable to create reservation. Please try again."
        );

    }

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
                            }
                        }}
                        value={reservation.reservationDate}
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
                            }
                        }}
                        value={reservation.reservationTime}
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
                        value={reservation.guestCount}
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

                <Button onClick={onClose}>
                    Cancel
                </Button>

                <Button
                    variant="contained"
                    onClick={handleSubmit}
                >
                    Confirm Reservation
                </Button>

            </DialogActions>

        </Dialog>

    );

}

export default ReservationDialog;