import { useEffect, useState } from "react";
import { useParams } from "react-router-dom";

import {
   
    Card,
    CardContent,
    CardMedia,
    CircularProgress,
    Divider,
    Link,
    Typography
} from "@mui/material";

import RestaurantService from "../services/RestaurantService";
import DealService from "../services/DealService";

import type { Restaurant } from "../types/Restaurant";
import type { Deal } from "../types/Deal";

import DealCard from "../components/deals/DealCard";

function RestaurantDetailsPage() {

    const { id } = useParams();

    const [restaurant, setRestaurant] = useState<Restaurant | null>(null);
    const [deals, setDeals] = useState<Deal[]>([]);
    const [loading, setLoading] = useState(true);

    useEffect(() => {

        if (id) {
            loadRestaurant(Number(id));
            loadDeals(Number(id));
        }

    }, [id]);

    async function loadRestaurant(id: number) {

        const data = await RestaurantService.getById(id);

        setRestaurant(data);
    }

    async function loadDeals(id: number) {

        try {

            const data = await DealService.getByRestaurant(id);

            setDeals(data);

        } catch (error) {

            console.error(error);

        } finally {

            setLoading(false);

        }
    }

    if (loading) {

        return <CircularProgress />;

    }

    if (!restaurant) {

        return (
            <Typography variant="h5">
                Restaurant not found.
            </Typography>
        );

    }

    return (

        <>

            <Card sx={{ mb: 4 }}>

                <CardMedia
                    component="img"
                    height="300"
                    image={
                        restaurant.logoUrl ||
                        "https://placehold.co/1200x400?text=EatKath"
                    }
                    alt={restaurant.name}
                />

                <CardContent>

                    <Typography variant="h4">
                        {restaurant.name}
                    </Typography>

                    <Typography color="text.secondary">
                        📍 {restaurant.areaName}
                    </Typography>

                    <Typography sx={{ mt: 2 }}>
                        {restaurant.description}
                    </Typography>

                    <Divider sx={{ my: 3 }} />

                    <Typography>
                        <strong>Address:</strong> {restaurant.address}
                    </Typography>

                    <Typography>
                        <strong>Phone:</strong> {restaurant.phoneNumber}
                    </Typography>

                    <Typography>
                        <strong>Email:</strong> {restaurant.email}
                    </Typography>

                    <Typography sx={{ mt: 1 }}>
                        <strong>Website:</strong>{" "}
                        <Link
                            href={restaurant.website}
                            target="_blank"
                        >
                            {restaurant.website}
                        </Link>
                    </Typography>

                </CardContent>

            </Card>

            <Typography
                variant="h5"
                sx={{ mb: 2 }}
            >
                Available Deals
            </Typography>

            {deals.length === 0 ? (

                <Typography color="text.secondary">
                    No deals available.
                </Typography>

            ) : (

                deals.map((deal) => (

                    <DealCard
                        key={deal.id}
                        deal={deal}
                    />

                ))

            )}

        </>

    );
}

export default RestaurantDetailsPage;