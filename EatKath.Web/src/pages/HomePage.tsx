import { useContext, useEffect, useState } from "react";
import { Link as RouterLink } from "react-router-dom";

import {
    Alert,
    Box,
    Button,
    CircularProgress,
    Grid,
    Paper,
    Typography
} from "@mui/material";

import RestaurantCard from "../components/restaurants/RestaurantCard";
import RestaurantService from "../services/RestaurantService";
import UserFavoriteService from "../services/UserFavoriteService";
import AuthContext from "../features/auth/AuthContext";

import type { Restaurant } from "../types/Restaurant";
import type { UserFavorite } from "../types/UserFavorite";

// Maximum number of restaurants shown on the Home page.
const MAX_HOME_RESTAURANTS = 8;

function HomePage() {

    const auth = useContext(AuthContext);

    const isCustomer = auth?.user?.role === "Customer";

    const [restaurants, setRestaurants] = useState<Restaurant[]>([]);
    const [favorites, setFavorites] = useState<UserFavorite[]>([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);

    useEffect(() => {

        loadRestaurants();

    }, []);

    // Favorites belong to customers only - no favorite API calls for
    // visitors, Owners or Admins.
    useEffect(() => {

        if (isCustomer)
            loadFavorites();

    }, [isCustomer]);

    async function loadRestaurants() {

        try {

            const data = await RestaurantService.getAll();

            // Active restaurants that currently have deals, most deals first.
            const withDeals = data
                .filter(r => r.isActive && r.activeDeals > 0)
                .sort((a, b) => b.activeDeals - a.activeDeals)
                .slice(0, MAX_HOME_RESTAURANTS);

            setRestaurants(withDeals);

        }
        catch (err) {

            console.error(err);

            setError("We couldn't load restaurants right now. Please try again later.");

        }
        finally {

            setLoading(false);

        }

    }

    async function loadFavorites() {

        try {

            const data = await UserFavoriteService.getMyFavorites();

            setFavorites(data);

        }
        catch (err) {

            // Favorites are optional on the Home page.
            console.error(err);

        }

    }

    return (

        <>

            <Paper sx={{ p: 4, mb: 4 }}>

                <Typography
                    variant="h4"
                    sx={{ mb: 2 }}
                >
                    Walk-in deals at restaurants near you
                </Typography>

                <Typography
                    color="text.secondary"
                    sx={{ mb: 3 }}
                >
                    Discover restaurant deals, choose your arrival time and redeem the offer.
                    Then simply show your redemption at the restaurant when you arrive.
                </Typography>

                <Button
                    variant="contained"
                    component={RouterLink}
                    to="/restaurants"
                >
                    Browse Restaurants
                </Button>

            </Paper>

            <Typography
                variant="h5"
                sx={{ mb: 2 }}
            >
                Restaurants with deals today
            </Typography>

            {loading ? (

                <CircularProgress />

            ) : error ? (

                <Alert severity="error">
                    {error}
                </Alert>

            ) : restaurants.length === 0 ? (

                <Typography color="text.secondary">
                    No deals available right now
                </Typography>

            ) : (

                <Grid container spacing={3}>

                    {restaurants.map(restaurant => (

                        <Grid
                            key={restaurant.id}
                            size={{
                                xs: 12,
                                sm: 6,
                                md: 4,
                                lg: 3
                            }}
                        >

                            <RestaurantCard
                                restaurant={restaurant}
                                isFavorite={
                                    favorites.some(
                                        f => f.restaurantId === restaurant.id
                                    )
                                }
                                onFavoriteChanged={() => {
                                    if (isCustomer)
                                        loadFavorites();
                                }}
                            />

                        </Grid>

                    ))}

                </Grid>

            )}

            <Box sx={{ mt: 4 }}>

                <Button
                    component={RouterLink}
                    to="/restaurants"
                >
                    View all restaurants
                </Button>

            </Box>

        </>

    );

}

export default HomePage;
