import { useEffect, useState } from "react";

import {
    CircularProgress,
    Grid,
    Typography
} from "@mui/material";

import RestaurantCard from "../components/restaurants/RestaurantCard";
import RestaurantService from "../services/RestaurantService";
import type { Restaurant } from "../types/Restaurant";

function RestaurantsPage() {

    const [restaurants, setRestaurants] = useState<Restaurant[]>([]);
    const [loading, setLoading] = useState(true);

    useEffect(() => {
        loadRestaurants();
    }, []);

    async function loadRestaurants() {

        try {

            const data = await RestaurantService.getAll();

            setRestaurants(data);

        } catch (error) {

            console.error(error);

        } finally {

            setLoading(false);

        }
    }

    if (loading) {

        return <CircularProgress />;

    }

    return (

        <>

            <Typography
                variant="h4"
                sx={{ mb: 3 }}
            >
                Restaurants
            </Typography>

            <Grid container spacing={3}>

                {restaurants.map((restaurant) => (

                    <Grid
                        size={{ xs: 12, sm: 6, md: 4, lg: 3 }}
                        key={restaurant.id}
                    >
                        <RestaurantCard restaurant={restaurant} />
                    </Grid>

                ))}

            </Grid>

        </>

    );
}

export default RestaurantsPage;