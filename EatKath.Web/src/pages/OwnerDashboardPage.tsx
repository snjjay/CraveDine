
import { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";

import {
    Button,
    Chip,
    CircularProgress,
    Dialog,
    DialogActions,
    DialogContent,
    DialogTitle,
    Paper,
    Stack,
    Table,
    TableBody,
    TableCell,
    TableContainer,
    TableHead,
    TableRow,
    TextField,
    Typography
} from "@mui/material";

import OwnerReservationService from "../services/OwnerReservationService";
import OwnerRestaurantService from "../services/OwnerRestaurantService";
import RedemptionService from "../services/RedemptionService";

import type { OwnerReservation } from "../types/OwnerReservation";
import type { Restaurant } from "../types/Restaurant";

function OwnerDashboardPage() {

    const navigate = useNavigate();

    const [restaurant, setRestaurant] =
        useState<Restaurant | null>(null);

    const [reservations, setReservations] =
        useState<OwnerReservation[]>([]);

    const [loading, setLoading] =
        useState(true);

    const [selectedReservationId, setSelectedReservationId] =
        useState<number | null>(null);

    const [billAmount, setBillAmount] =
        useState(0);

    useEffect(() => {

        loadData();

    }, []);

    async function loadData() {

        try {

            const restaurantData =
                await OwnerRestaurantService.getMyRestaurant();

            const reservationData =
                await OwnerReservationService.getAll();

            setRestaurant(restaurantData);

            setReservations(reservationData);

        }
        catch (error: any) {

            if (error.response?.status === 404) {

                setRestaurant(null);

                setReservations([]);

            }
            else {

                console.error(error);

            }

        }
        finally {

            setLoading(false);

        }

    }

   

    async function completedReservation(id: number) {

        setSelectedReservationId(id);

        setBillAmount(0);

    }

    async function completeRedemption() {

        if (selectedReservationId === null)
            return;

        try {

           

            // Complete redemption
            const reservation = reservations.find(
                x => x.id === selectedReservationId
            );

            if (!reservation?.redemptionId) {
                alert("No redemption found for this reservation.");
                return;
            }

            await RedemptionService.complete(
                reservation.redemptionId,
                {
                    billAmount
                }
            );

            setSelectedReservationId(null);

            setBillAmount(0);

            await loadData();

        }
        catch (error: any) {

            console.error(error);

            alert(
                error.response?.data?.message ??
                error.message
            );

        }

    }

    async function noShowReservation(id: number) {

        await OwnerReservationService.noShow(id);

        loadData();

    }

    async function cancelReservation(id: number) {

        await OwnerReservationService.cancel(id);

        loadData();

    }

    function getChipColor(
        status: string
    ): "success" | "warning" | "error" | "info" | "default" {

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

    if (loading) {

        return <CircularProgress />;

    }

    return (

        <>

            <Stack
                direction="row"
                spacing={2}
                sx={{
                    justifyContent: "space-between",
                    alignItems: "center",
                    mb: 3
                }}
            >

                <Typography variant="h4">

                    Owner Dashboard

                </Typography>

                {restaurant && (

                    <Stack
                        direction="row"
                        spacing={2}
                    >

                        <Button
                            variant="contained"
                            onClick={() => navigate("/owner/deals")}
                        >
                            Manage Deals
                        </Button>

                        <Button
                            variant="contained"
                            onClick={() => navigate("/owner/opening-hours")}
                        >
                            Opening Hours
                        </Button>

                        <Button
                            variant="contained"
                            color="secondary"
                            onClick={() => navigate("/owner/restaurant")}
                        >
                            Edit Restaurant
                        </Button>

                        <Button
                            variant="contained"
                            onClick={() => navigate("/owner/menu-categories")}
                        >
                            Menu Categories
                        </Button>

                        <Button
                            variant="contained"
                            onClick={() => navigate("/owner/menu-items")}
                        >
                            Menu Items
                        </Button>



                    </Stack>

                )}

            </Stack>

            {!restaurant && (

                <Paper sx={{ p: 4 }}>

                    <Typography
                        variant="h5"
                        gutterBottom
                    >
                        No Restaurant Assigned
                    </Typography>

                    <Typography>

                        No restaurant has been assigned to your account yet.
                        Please contact an administrator.

                    </Typography>

                </Paper>

            )}

            {restaurant && (

                <>

                    <Paper sx={{ p: 3, mb: 4 }}>

                        <Typography variant="h5">

                            {restaurant.name}

                        </Typography>

                        <Typography>

                            {restaurant.address}

                        </Typography>

                        <Typography>

                            {restaurant.phoneNumber}

                        </Typography>

                        <Typography>

                            {restaurant.email}

                        </Typography>

                    </Paper>

                    <Typography
                        variant="h5"
                        sx={{ mb: 2 }}
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

                                        <TableCell>
                                            {reservation.customerName}
                                        </TableCell>

                                        <TableCell>
                                            {reservation.dealTitle}
                                        </TableCell>

                                        <TableCell>
                                            {reservation.reservationDate}
                                        </TableCell>

                                        <TableCell>
                                            {reservation.reservationTime}
                                        </TableCell>

                                        <TableCell>
                                            {reservation.guestCount}
                                        </TableCell>

                                        <TableCell>

                                            <Chip
                                                label={reservation.status}
                                                color={getChipColor(reservation.status)}
                                            />

                                        </TableCell>

                                        <TableCell>

                                            <Stack
                                                direction="row"
                                                spacing={1}
                                                flexWrap="wrap"
                                            >

                                                {reservation.status === "Pending" && (

                                                    <>
    <Button
        size="small"
        variant="contained"
        color="success"
        onClick={() => completedReservation(reservation.id)}
    >
        Redeem Offer
    </Button>

    <Button
        size="small"
        variant="contained"
        color="warning"
        onClick={() => noShowReservation(reservation.id)}
    >
        No Show
    </Button>

    <Button
        size="small"
        variant="outlined"
        color="error"
        onClick={() => cancelReservation(reservation.id)}
    >
        Cancel
    </Button>
</>

                                                   

                                                )}

                                                



                                            </Stack>

                                        </TableCell>

                                    </TableRow>

                                ))}

                            </TableBody>

                        </Table>

                    </TableContainer>

                </>

            )}

            <Dialog
                open={selectedReservationId !== null}
                onClose={() => setSelectedReservationId(null)}
            >
                <DialogTitle>
                    Redeem Offer
                </DialogTitle>

                <DialogContent>

                    <TextField
                        fullWidth
                        label="Original Bill Amount"
                        type="number"
                        value={billAmount}
                        onChange={(e) =>
                            setBillAmount(Number(e.target.value))
                        }
                        sx={{ mt: 2 }}
                    />

                </DialogContent>

                <DialogActions>

                    <Button
                        onClick={() =>
                            setSelectedReservationId(null)
                        }
                    >
                        Cancel
                    </Button>

                    <Button
                        variant="contained"
                        onClick={completeRedemption}
                    >
                        Redeem Offer
                    </Button>

                </DialogActions>

            </Dialog>

        </>

    );

}

export default OwnerDashboardPage;