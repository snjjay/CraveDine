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

    async function cancelReservation(id: number) {

        await OwnerReservationService.cancel(id);

        loadData();

    }

    function getChipColor(status: string): "success" | "warning" | "error" {

        switch (status) {

            case "Confirmed":
                return "success";

            case "Cancelled":
                return "error";

            default:
                return "warning";
        }

    }

    if (loading) {

        return <CircularProgress />;

    }

    return (

        <>

            <Stack
                direction="row"
                justifyContent="space-between"
                alignItems="center"
                sx={{ mb: 3 }}
            >

                <Typography variant="h4">

                    Owner Dashboard

                </Typography>

                <Button
                    variant="contained"
                    onClick={() => navigate("/owner/deals")}
                >
                    Manage Deals
                </Button>

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
                            <TableCell align="center">
                                Actions
                            </TableCell>

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

                                    {reservation.status === "Pending" && (

                                        <Stack
                                            direction="row"
                                            spacing={1}
                                        >

                                            <Button
                                                variant="contained"
                                                size="small"
                                                color="success"
                                                onClick={() =>
                                                    confirmReservation(reservation.id)
                                                }
                                            >
                                                Confirm
                                            </Button>

                                            <Button
                                                variant="contained"
                                                size="small"
                                                color="error"
                                                onClick={() =>
                                                    cancelReservation(reservation.id)
                                                }
                                            >
                                                Cancel
                                            </Button>

                                        </Stack>

                                    )}

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