import { useEffect, useState } from "react";
import { useParams } from "react-router-dom";
import MenuCategoryService from "../services/MenuCategoryService";
import MenuItemService from "../services/MenuItemService";

import type { MenuCategory } from "../types/MenuCategory";
import type { MenuItem } from "../types/MenuItem";
import { getImageUrl } from "../utils/imageUrl";
import {
    Avatar,
    Box,
    Button,
    Card,
    CardContent,
    CardMedia,
    CircularProgress,
    Divider,
    Grid,
    Link,
    Typography
} from "@mui/material";

import RestaurantService from "../services/RestaurantService";
import DealService from "../services/DealService";
import RestaurantImageService from "../services/RestaurantImageService";
import { useNotification } from "../features/notifications/NotificationContext";

import type { Restaurant } from "../types/Restaurant";
import type { Deal } from "../types/Deal";
import type { RestaurantImage } from "../types/RestaurantImage";

import DealCard from "../components/deals/DealCard";
import OpeningStatus from "../components/restaurants/OpeningStatus";
import RestaurantMenu from "../components/restaurants/RestaurantMenu";

// Target of the "View Menu" button.
const MENU_SECTION_ID = "restaurant-menu";

function RestaurantDetailsPage() {

    const { id } = useParams();

    const { notify } = useNotification();

    const [restaurant, setRestaurant] = useState<Restaurant | null>(null);
    const [deals, setDeals] = useState<Deal[]>([]);
    const [categories, setCategories] = useState<MenuCategory[]>([]);

    const [menuItems, setMenuItems] = useState<MenuItem[]>([]);

    const [galleryImages, setGalleryImages] = useState<RestaurantImage[]>([]);
    const [galleryLoading, setGalleryLoading] = useState(true);

    const [loading, setLoading] = useState(true);

    useEffect(() => {

        if (id) {

            loadRestaurant(Number(id));

            loadDeals(Number(id));
            loadMenu(Number(id));
            loadGallery(Number(id));

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


    async function loadGallery(id: number) {

        try {

            const data = await RestaurantImageService.getByRestaurant(id);

            setGalleryImages(data);

        }
        catch (error: any) {

            console.error(error);

            notify(
                error.response?.data?.Message ??
                error.message ??
                "Failed to load restaurant gallery. Please try again.",
                "error"
            );

        }
        finally {

            setGalleryLoading(false);

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

    const coverUrl = getImageUrl(restaurant.coverImageUrl);
    const logoUrl = getImageUrl(restaurant.logoUrl);

    return (

        <>

            <Card sx={{ mb: 4 }}>

                <Box sx={{ position: "relative" }}>

                    <CardMedia
                        component="img"
                        height="300"
                        image={coverUrl}
                        alt={restaurant.name}
                    />

                    {restaurant.logoUrl && (

                        <Avatar
                            src={logoUrl}
                            alt={`${restaurant.name} logo`}
                            sx={{
                                width: 80,
                                height: 80,
                                border: "3px solid white",
                                position: "absolute",
                                bottom: -40,
                                left: 16
                            }}
                        />

                    )}

                </Box>

                <CardContent sx={{ pt: 6 }}>

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

                    {/* Scrolls to the digital menu (which offers the PDF as a fallback). */}
                    <Button
                        variant="outlined"
                        onClick={() =>
                            document
                                .getElementById(MENU_SECTION_ID)
                                ?.scrollIntoView({ behavior: "smooth", block: "start" })
                        }
                        sx={{ mt: 2 }}
                    >
                        View Menu
                    </Button>

                </CardContent>

            </Card>

            <Typography
                variant="h5"
                sx={{ mb: 2 }}
            >
                Opening Hours
            </Typography>

            {(!restaurant.openingHours || restaurant.openingHours.length === 0) ? (

                <Typography
                    color="text.secondary"
                    sx={{ mb: 4 }}
                >
                    Opening hours not available.
                </Typography>

            ) : (

                <Card sx={{ mb: 4 }}>

                    <CardContent>

                        <OpeningStatus hours={restaurant.openingHours} />

                    </CardContent>

                </Card>

            )}

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
                        onRedeemed={() => loadDeals(deal.restaurantId)}
                    />

                ))

            )}

            <Divider sx={{ my: 4 }} />

            <Typography
                variant="h5"
                sx={{ mb: 2 }}
            >
                Gallery
            </Typography>

            {galleryLoading ? (

                <CircularProgress size={24} />

            ) : galleryImages.length === 0 ? (

                <Typography color="text.secondary">

                    No gallery images available.

                </Typography>

            ) : (

                <Grid container spacing={2}>

                    {galleryImages.map(image => (

                        <Grid
                            key={image.id}
                            size={{ xs: 12, sm: 6, md: 4 }}
                        >

                            <Card>

                                <CardMedia
                                    component="img"
                                    height="180"
                                    image={getImageUrl(image.imageUrl)}
                                    alt={image.caption ?? restaurant.name}
                                />

                            </Card>

                        </Grid>

                    ))}

                </Grid>

            )}

            <Divider sx={{ my: 4 }} />

            <Box
                id={MENU_SECTION_ID}
                sx={{ scrollMarginTop: 16 }}
            >

                <Typography
                    variant="h5"
                    sx={{ mb: 2 }}
                >
                    Menu
                </Typography>

                <RestaurantMenu
                    categories={categories}
                    items={menuItems}
                    deals={deals}
                    currencyCode={restaurant.currencyCode}
                    menuPdfUrl={restaurant.menuPdfUrl}
                />

            </Box>

        </>

    );

}

export default RestaurantDetailsPage;