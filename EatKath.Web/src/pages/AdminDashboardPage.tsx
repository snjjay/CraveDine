import { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";

import {
    Button,
    Card,
    CardContent,
    CircularProgress,
    Grid,
    Stack,
    Typography
} from "@mui/material";

import UserService from "../services/UserService";
import RestaurantService from "../services/RestaurantService";
import DealService from "../services/DealService";
import ReservationService from "../services/ReservationService";
import { useNotification } from "../features/notifications/NotificationContext";

import type { User } from "../types/User";
import type { Restaurant } from "../types/Restaurant";
import type { Deal } from "../types/Deal";
import type { Reservation } from "../types/Reservation";

interface StatCardProps {
    label: string;
    value: number;
}

function StatCard({ label, value }: StatCardProps) {

    return (

        <Card sx={{ height: "100%" }}>

            <CardContent>

                <Typography variant="body2" color="text.secondary" sx={{ fontWeight: 500 }}>

                    {label}

                </Typography>

                <Typography
                    component="p"
                    sx={{ fontFamily: "h4.fontFamily", fontWeight: 700, fontSize: "2rem", lineHeight: 1.2, mt: 0.5 }}
                >

                    {value}

                </Typography>

            </CardContent>

        </Card>

    );

}

function AdminDashboardPage() {

    const navigate = useNavigate();

    const { notify } = useNotification();

    const [users, setUsers] = useState<User[]>([]);
    const [restaurants, setRestaurants] = useState<Restaurant[]>([]);
    const [deals, setDeals] = useState<Deal[]>([]);
    const [reservations, setReservations] = useState<Reservation[]>([]);

    const [loading, setLoading] = useState(true);

    useEffect(() => {

        loadDashboard();

    }, []);

    async function loadDashboard() {

        try {

            const [
                userData,
                restaurantData,
                dealData,
                reservationData
            ] = await Promise.all([
                UserService.getAll(),
                RestaurantService.getAll(),
                DealService.getAll(),
                ReservationService.getAll()
            ]);

            setUsers(userData);
            setRestaurants(restaurantData);
            setDeals(dealData);
            setReservations(reservationData);

        }
        catch (error: any) {

            console.error(error);

            notify(
                error.response?.data?.Message ??
                error.message ??
                "Failed to load admin dashboard data. Please try again.",
                "error"
            );

        }
        finally {

            setLoading(false);

        }

    }

    if (loading)
        return <CircularProgress />;

    const activeUsers = users.filter(u => u.isActive).length;
    const inactiveUsers = users.length - activeUsers;

    const activeRestaurants = restaurants.filter(r => r.isActive).length;
    const inactiveRestaurants = restaurants.length - activeRestaurants;

    const activeDeals = deals.filter(d => d.isActive).length;
    const inactiveDeals = deals.length - activeDeals;

    const reservationStatusCounts = reservations.reduce<Record<string, number>>(
        (counts, reservation) => {

            const status = reservation.status ?? "Unknown";

            counts[status] = (counts[status] ?? 0) + 1;

            return counts;

        },
        {}
    );

    return (

        <>

            <Stack
                direction="row"
                spacing={2}
                useFlexGap
                sx={{
                    justifyContent: "space-between",
                    alignItems: "center",
                    flexWrap: "wrap",
                    mb: 3
                }}
            >

                <Typography variant="h4">
                    Admin Dashboard
                </Typography>

                <Button
                    variant="contained"
                    onClick={() => navigate("/admin/users")}
                >
                    Manage Users
                </Button>

            </Stack>

            <Grid
                container
                spacing={2}
                sx={{ mb: 3 }}
            >

                <Grid size={{ xs: 6, sm: 3 }}>
                    <StatCard label="Total Users" value={users.length} />
                </Grid>

                <Grid size={{ xs: 6, sm: 3 }}>
                    <StatCard label="Total Restaurants" value={restaurants.length} />
                </Grid>

                <Grid size={{ xs: 6, sm: 3 }}>
                    <StatCard label="Total Deals" value={deals.length} />
                </Grid>

                <Grid size={{ xs: 6, sm: 3 }}>
                    <StatCard label="Total Reservations" value={reservations.length} />
                </Grid>

            </Grid>

            <Grid
                container
                spacing={2}
                sx={{ mb: 4 }}
            >

                <Grid size={{ xs: 6, md: 2 }}>
                    <StatCard label="Active Users" value={activeUsers} />
                </Grid>

                <Grid size={{ xs: 6, md: 2 }}>
                    <StatCard label="Inactive Users" value={inactiveUsers} />
                </Grid>

                <Grid size={{ xs: 6, md: 2 }}>
                    <StatCard label="Active Restaurants" value={activeRestaurants} />
                </Grid>

                <Grid size={{ xs: 6, md: 2 }}>
                    <StatCard label="Inactive Restaurants" value={inactiveRestaurants} />
                </Grid>

                <Grid size={{ xs: 6, md: 2 }}>
                    <StatCard label="Active Deals" value={activeDeals} />
                </Grid>

                <Grid size={{ xs: 6, md: 2 }}>
                    <StatCard label="Inactive Deals" value={inactiveDeals} />
                </Grid>

            </Grid>

            <Typography
                variant="h5"
                sx={{ mb: 2 }}
            >
                Reservation Status Breakdown
            </Typography>

            {Object.keys(reservationStatusCounts).length === 0 ? (

                <Typography color="text.secondary">
                    No reservations found.
                </Typography>

            ) : (

                <Grid container spacing={2}>

                    {Object.entries(reservationStatusCounts).map(([status, count]) => (

                        <Grid
                            key={status}
                            size={{ xs: 6, md: 2 }}
                        >
                            <StatCard label={status} value={count} />
                        </Grid>

                    ))}

                </Grid>

            )}

        </>

    );

}

export default AdminDashboardPage;
