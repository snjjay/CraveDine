import {
    Card,
    CardContent,
    CardMedia,
    Typography,
    Button,
    Stack,
    Chip
} from "@mui/material";

import { Link } from "react-router-dom";

import type { Restaurant } from "../../types/Restaurant";

interface Props {
    restaurant: Restaurant;
}

function RestaurantCard({ restaurant }: Props) {

    return (

        <Card sx={{ height: "100%", display: "flex", flexDirection: "column" }}>

            <CardMedia
                component="img"
                height="180"
                image={
                    restaurant.logoUrl ||
                    "https://placehold.co/600x300?text=EatKath"
                }
                alt={restaurant.name}
            />

            <CardContent sx={{ flexGrow: 1 }}>

                <Typography variant="h6">
                    {restaurant.name}
                </Typography>

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