import { useEffect, useState } from "react";

import {
    CircularProgress,
    Grid,
    Typography,
    TextField
} from "@mui/material";

import RestaurantCard from "../components/restaurants/RestaurantCard";

import RestaurantService from "../services/RestaurantService";
import UserFavoriteService from "../services/UserFavoriteService";

import type { Restaurant } from "../types/Restaurant";
import type { UserFavorite } from "../types/UserFavorite";

function RestaurantsPage() {

    const [restaurants, setRestaurants] = useState<Restaurant[]>([]);
    const [favorites, setFavorites] = useState<UserFavorite[]>([]);
    const [loading, setLoading] = useState(true);

    const [search, setSearch] = useState("");

    useEffect(() => {

        loadData();

    }, []);

    async function loadData() {

        try {

            const restaurantsData =
                await RestaurantService.getAll();

            setRestaurants(restaurantsData);

            try {

                const favoritesData =
                    await UserFavoriteService.getMyFavorites();

                setFavorites(favoritesData);

            }
            catch {

                // User not logged in

            }

        }
        catch (error) {

            console.error(error);

        }
        finally {

            setLoading(false);

        }

    }

    if (loading)
        return <CircularProgress />;

    const filteredRestaurants = restaurants.filter(r =>
        r.name.toLowerCase().includes(search.toLowerCase())
    );

    return (

        <>

            <Typography
                variant="h4"
                sx={{ mb: 3 }}
            >
                Restaurants
            </Typography>

            <TextField
                fullWidth
                label="Search Restaurants"
                placeholder="Type restaurant name..."
                value={search}
                onChange={(e) => setSearch(e.target.value)}
                sx={{ mb: 3 }}
            />

            <Grid container spacing={3}>

                {filteredRestaurants.map((restaurant) => (

                    <Grid
                        key={restaurant.id}
                        size={{ xs: 12, sm: 6, md: 4, lg: 3 }}
                    >

                        <RestaurantCard
                            restaurant={restaurant}
                            isFavorite={
                                favorites.some(
                                    f => f.restaurantId === restaurant.id
                                )
                            }
                            onFavoriteChanged={loadData}
                        />

                    </Grid>

                ))}

            </Grid>

        </>

    );

}

export default RestaurantsPage;