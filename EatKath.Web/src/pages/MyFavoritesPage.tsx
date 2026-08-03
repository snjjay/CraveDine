import { useEffect, useState } from "react";

import {
    CircularProgress,
    Grid,
    Typography
} from "@mui/material";

import UserFavoriteService from "../services/UserFavoriteService";

import type { UserFavorite } from "../types/UserFavorite";

function MyFavoritesPage() {

    const [favorites, setFavorites] = useState<UserFavorite[]>([]);
    const [loading, setLoading] = useState(true);

    useEffect(() => {

        loadFavorites();

    }, []);

    async function loadFavorites() {

        try {

            const data =
                await UserFavoriteService.getMyFavorites();

            setFavorites(data);

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

    return (

        <>

            <Typography
                variant="h4"
                sx={{ mb: 3 }}
            >
                My Favourite Restaurants
            </Typography>

            <Grid container spacing={3}>

                {favorites.map(f => (

                    <Grid
                        key={f.restaurantId}
                        size={{ xs: 12, sm: 6, md: 4 }}
                    >

                        <img
                            src={`https://localhost:7203${f.logoUrl}`}
                            alt={f.restaurantName}
                            style={{
                                width: "100%",
                                height: 180,
                                objectFit: "cover",
                                borderRadius: 8
                            }}
                        />

                        <Typography
                            variant="h6"
                            sx={{ mt: 1 }}
                        >
                            {f.restaurantName}
                        </Typography>

                    </Grid>

                ))}

            </Grid>

        </>

    );

}

export default MyFavoritesPage;