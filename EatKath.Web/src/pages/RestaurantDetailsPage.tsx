import { useEffect, useState } from "react";
import { useParams } from "react-router-dom";
import MenuCategoryService from "../services/MenuCategoryService";
import MenuItemService from "../services/MenuItemService";

import type { MenuCategory } from "../types/MenuCategory";
import type { MenuItem } from "../types/MenuItem";
import { getImageUrl } from "../utils/imageUrl";
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
    const [categories, setCategories] = useState<MenuCategory[]>([]);

    const [menuItems, setMenuItems] = useState<MenuItem[]>([]);

    const [loading, setLoading] = useState(true);

    useEffect(() => {

        if (id) {

            loadRestaurant(Number(id));

            loadDeals(Number(id));
            loadMenu(Number(id));

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

        }
        catch (error) {

            console.error(error);

        }
        finally {

            setLoading(false);

        }

    }


    async function loadMenu(id: number) {

        try {

            const categoryData =
                await MenuCategoryService.getByRestaurant(id);

            setCategories(categoryData);

            const itemData =
                await MenuItemService.getByRestaurant(id);

            setMenuItems(itemData);

        }
        catch (error) {

            console.error(error);

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

    const imageUrl = getImageUrl(restaurant.logoUrl);

    return (

        <>

            <Card sx={{ mb: 4 }}>

                <CardMedia
                    component="img"
                    height="300"
                    image={imageUrl}
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

            <Divider sx={{ my: 4 }} />

            <Typography
                variant="h5"
                sx={{ mb: 2 }}
            >
                Menu
            </Typography>

            {categories.map(category => (

                <div key={category.id}>

                    <Typography
                        variant="h6"
                        sx={{ mt: 3 }}
                    >
                        {category.name}
                    </Typography>

                    {menuItems
                        .filter(item =>
                            item.menuCategoryId === category.id &&
                            item.isAvailable
                        )
                        .map(item => (

                            <Card
                                key={item.id}
                                sx={{ mt: 1, mb: 1 }}
                            >

                                <CardContent>

                                    <Typography variant="subtitle1">

                                        {item.isFeatured && "⭐ "}
                                        {item.name}

                                    </Typography>

                                    <Typography
                                        color="text.secondary"
                                    >
                                        {item.description}
                                    </Typography>

                                    <Typography
                                        sx={{ mt: 1 }}
                                        fontWeight="bold"
                                    >
                                        NPR {item.price}
                                    </Typography>

                                </CardContent>

                            </Card>

                        ))}

                </div>

            ))}

        </>

    );

}

export default RestaurantDetailsPage;