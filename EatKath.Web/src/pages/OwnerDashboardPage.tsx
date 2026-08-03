import { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";

import {
    Button,
    Card,
    CardContent,
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
import OwnerRestaurantService from "../services/OwnerRestaurantService";

import type { OwnerReservation } from "../types/OwnerReservation";
import type { Restaurant } from "../types/Restaurant";

function OwnerDashboardPage() {

    const navigate = useNavigate();

    const [restaurant, setRestaurant] = useState<Restaurant | null>(null);

    const [reservations, setReservations] = useState<OwnerReservation[]>([]);

    const [loading, setLoading] = useState(true);

    useEffect(() => {

        loadData();

    }, []);

    async function loadData() {

        try {

            const [restaurantData, reservationData] =
                await Promise.all([

                    OwnerRestaurantService.getMyRestaurant(),

                    OwnerReservationService.getAll()

                ]);

            setRestaurant(restaurantData);

            setReservations(reservationData);

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

        loadData();

    }

    async function rejectReservation(id: number) {

        await OwnerReservationService.reject(id);

        loadData();

    }

    async function arrivedReservation(id: number) {

        await OwnerReservationService.arrived(id);

        loadData();

    }

    async function completedReservation(id: number) {

        await OwnerReservationService.completed(id);

        loadData();

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
                    color="secondary"
                    onClick={() => navigate("/owner/restaurant")}
                >
                    Edit Restaurant
                </Button>

            </Stack>

        </Stack>

        {restaurant && (

            <Card sx={{ mb: 4 }}>

                <CardContent>

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

                    <Typography>

                        {restaurant.website}

                    </Typography>

                    <Typography sx={{ mt: 2 }}>

                        Active Deals: {restaurant.activeDeals}

                    </Typography>

                    <Typography>

                        Best Discount: {restaurant.bestDiscount ?? 0}%

                    </Typography>

                </CardContent>

            </Card>

        )}

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
                                                color="success"
                                                variant="contained"
                                                onClick={() =>
                                                    confirmReservation(reservation.id)
                                                }
                                            >
                                                Confirm
                                            </Button>

                                            <Button
                                                size="small"
                                                color="error"
                                                variant="contained"
                                                onClick={() =>
                                                    rejectReservation(reservation.id)
                                                }
                                            >
                                                Reject
                                            </Button>
                                        </>
                                    )}

                                    {reservation.status === "Confirmed" && (
                                        <>
                                            <Button
                                                size="small"
                                                color="info"
                                                variant="contained"
                                                onClick={() =>
                                                    arrivedReservation(reservation.id)
                                                }
                                            >
                                                Arrived
                                            </Button>

                                            <Button
                                                size="small"
                                                color="warning"
                                                variant="contained"
                                                onClick={() =>
                                                    noShowReservation(reservation.id)
                                                }
                                            >
                                                No Show
                                            </Button>

                                            <Button
                                                size="small"
                                                color="error"
                                                variant="outlined"
                                                onClick={() =>
                                                    cancelReservation(reservation.id)
                                                }
                                            >
                                                Cancel
                                            </Button>
                                        </>
                                    )}

                                    {reservation.status === "Arrived" && (
                                        <Button
                                            size="small"
                                            color="success"
                                            variant="contained"
                                            onClick={() =>
                                                completedReservation(reservation.id)
                                            }
                                        >
                                            Completed
                                        </Button>
                                    )}

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