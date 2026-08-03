import {
    Card,
    CardContent,
    CardMedia,
    Typography,
    Button,
    Stack,
    Chip,
    IconButton
} from "@mui/material";

import FavoriteIcon from "@mui/icons-material/Favorite";
import FavoriteBorderIcon from "@mui/icons-material/FavoriteBorder";

import { Link } from "react-router-dom";

import UserFavoriteService from "../../services/UserFavoriteService";

import type { Restaurant } from "../../types/Restaurant";

interface Props {
    restaurant: Restaurant;
    isFavorite: boolean;
    onFavoriteChanged: () => void;
}

function RestaurantCard({
    restaurant,
    isFavorite,
    onFavoriteChanged
}: Props) {

    const imageUrl = restaurant.logoUrl
        ? `https://localhost:7203${restaurant.logoUrl}`
        : "https://placehold.co/600x300?text=EatKath";

    async function toggleFavorite() {

        try {

            if (isFavorite) {

                await UserFavoriteService.remove(restaurant.id);

            }
            else {

                await UserFavoriteService.add(restaurant.id);

            }

            onFavoriteChanged();

        }
        catch (error) {

            console.error(error);

        }

    }

    return (

        <Card
            sx={{
                height: "100%",
                display: "flex",
                flexDirection: "column"
            }}
        >

            <CardMedia
                component="img"
                height="180"
                image={imageUrl}
                alt={restaurant.name}
            />

            <CardContent sx={{ flexGrow: 1 }}>

                <Stack
                    direction="row"
                    justifyContent="space-between"
                    alignItems="center"
                >

                    <Typography variant="h6">

                        {restaurant.name}

                    </Typography>

                    <IconButton
                        color="error"
                        onClick={toggleFavorite}
                    >
                        {isFavorite
                            ? <FavoriteIcon />
                            : <FavoriteBorderIcon />}
                    </IconButton>

                </Stack>

                <Typography
                    variant="body2"
                    color="text.secondary"
                >
                    📍 {restaurant.areaName}
                </Typography>

                <Typography sx={{ mt: 1, mb: 2 }}>
                    {restaurant.description}
                </Typography>

                {restaurant.activeDeals > 0 ? (

                    <Stack
                        direction="row"
                        spacing={1}
                        sx={{ mb: 2 }}
                    >

                        <Chip
                            color="success"
                            label={`${restaurant.bestDiscount}% OFF`}
                        />

                        <Chip
                            color="primary"
                            label={`${restaurant.activeDeals} Deals`}
                        />

                    </Stack>

                ) : (

                    <Chip
                        label="No Active Deals"
                        sx={{ mb: 2 }}
                    />

                )}

                <Button
                    component={Link}
                    to={`/restaurants/${restaurant.id}`}
                    fullWidth
                    variant="contained"
                >
                    View Details
                </Button>

            </CardContent>

        </Card>

    );

}

export default RestaurantCard;