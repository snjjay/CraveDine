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
    Grid,
    Link,
    Skeleton,
    Stack,
    Typography
} from "@mui/material";

import EmailOutlinedIcon from "@mui/icons-material/EmailOutlined";
import LanguageIcon from "@mui/icons-material/Language";
import LocalOfferOutlinedIcon from "@mui/icons-material/LocalOfferOutlined";
import PhoneOutlinedIcon from "@mui/icons-material/PhoneOutlined";
import PhotoLibraryOutlinedIcon from "@mui/icons-material/PhotoLibraryOutlined";
import PlaceOutlinedIcon from "@mui/icons-material/PlaceOutlined";
import RestaurantMenuIcon from "@mui/icons-material/RestaurantMenu";
import ScheduleIcon from "@mui/icons-material/Schedule";
import StorefrontOutlinedIcon from "@mui/icons-material/StorefrontOutlined";

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
import DiscountBadge from "../components/common/DiscountBadge";
import InfoRow from "../components/common/InfoRow";
import SectionHeader from "../components/common/SectionHeader";

// Target of the "View Menu" button.
const MENU_SECTION_ID = "restaurant-menu";

// Space for the sticky header when scrolling to a section.
const SECTION_SCROLL_MARGIN = { xs: 76, md: 88 };

// "https://www.example.com/path" -> "example.com/path"
function displayWebsite(url: string): string {

    return url.replace(/^https?:\/\//i, "").replace(/^www\./i, "").replace(/\/$/, "");

}

// Website links need a protocol to leave the app.
function websiteHref(url: string): string {

    return /^https?:\/\//i.test(url) ? url : `https://${url}`;

}

function scrollToMenu() {

    document
        .getElementById(MENU_SECTION_ID)
        ?.scrollIntoView({ behavior: "smooth", block: "start" });

}

// Placeholder while the restaurant loads.
function DetailsSkeleton() {

    return (

        <Box aria-busy="true" aria-label="Loading restaurant">
            <Skeleton variant="rounded" sx={{ height: { xs: 240, sm: 300, md: 360 } }} />
            <Grid container spacing={{ xs: 3, md: 4 }} sx={{ mt: 1 }}>
                <Grid size={{ xs: 12, md: 7, lg: 8 }}>
                    <Skeleton width={160} height={36} />
                    <Skeleton variant="rounded" height={150} sx={{ mt: 1 }} />
                    <Skeleton width={120} height={36} sx={{ mt: 3 }} />
                    <Skeleton variant="rounded" height={320} sx={{ mt: 1 }} />
                </Grid>
                <Grid size={{ xs: 12, md: 5, lg: 4 }}>
                    <Skeleton variant="rounded" height={320} />
                </Grid>
            </Grid>
        </Box>

    );

}

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

        return <DetailsSkeleton />;

    }

    if (!restaurant) {

        return (

            <Card>
                <CardContent sx={{ textAlign: "center", py: 6 }}>
                    <StorefrontOutlinedIcon aria-hidden sx={{ fontSize: 40, color: "text.disabled" }} />
                    <Typography variant="h5" component="h1" sx={{ mt: 1 }}>
                        Restaurant not found.
                    </Typography>
                    <Typography color="text.secondary" sx={{ mt: 0.5 }}>
                        It may have been removed or is no longer available.
                    </Typography>
                </CardContent>
            </Card>

        );

    }

    const coverUrl = getImageUrl(restaurant.coverImageUrl);
    const logoUrl = getImageUrl(restaurant.logoUrl);

    // e.g. "Kathmandu · Nepali, Tibetan"
    const meta = [
        restaurant.areaName,
        restaurant.cuisines.join(", ")
    ].filter(Boolean).join(" · ");

    const hasOpeningHours =
        !!restaurant.openingHours && restaurant.openingHours.length > 0;

    return (

        <>

            {/* ---------- Hero ---------- */}
            <Box
                component="header"
                sx={{
                    position: "relative",
                    borderRadius: "12px",
                    overflow: "hidden",
                    height: { xs: 260, sm: 320, md: 380 },
                    bgcolor: "text.primary"
                }}
            >

                <Box
                    component="img"
                    src={coverUrl}
                    alt=""
                    sx={{ width: "100%", height: "100%", objectFit: "cover", display: "block" }}
                />

                {/* Gradient keeps the white text readable on any photo */}
                <Box
                    aria-hidden
                    sx={{
                        position: "absolute",
                        inset: 0,
                        background: "linear-gradient(180deg, rgba(20,18,16,0.05) 25%, rgba(20,18,16,0.82) 100%)"
                    }}
                />

                <Stack
                    direction="row"
                    spacing={{ xs: 1.5, md: 2 }}
                    sx={{
                        position: "absolute",
                        left: { xs: 16, md: 32 },
                        right: { xs: 16, md: 32 },
                        bottom: { xs: 16, md: 28 },
                        alignItems: "flex-end",
                        color: "#FFFFFF"
                    }}
                >

                    {restaurant.logoUrl && (
                        <Avatar
                            src={logoUrl}
                            alt={`${restaurant.name} logo`}
                            variant="rounded"
                            sx={{
                                width: { xs: 64, md: 88 },
                                height: { xs: 64, md: 88 },
                                borderRadius: "12px",
                                border: "3px solid #FFFFFF",
                                bgcolor: "background.paper",
                                flexShrink: 0
                            }}
                        />
                    )}

                    <Box sx={{ minWidth: 0 }}>

                        <Typography
                            variant="h3"
                            component="h1"
                            sx={{ color: "inherit", textShadow: "0 1px 2px rgba(0,0,0,0.25)", overflowWrap: "anywhere" }}
                        >
                            {restaurant.name}
                        </Typography>

                        {meta && (
                            <Stack direction="row" spacing={0.5} sx={{ alignItems: "center", mt: 0.5, opacity: 0.92 }}>
                                <PlaceOutlinedIcon aria-hidden sx={{ fontSize: 18 }} />
                                <Typography sx={{ color: "inherit" }}>
                                    {meta}
                                </Typography>
                            </Stack>
                        )}

                        <Stack direction="row" sx={{ mt: 1.5, flexWrap: "wrap", gap: 1 }}>
                            {hasOpeningHours && (
                                <OpeningStatus hours={restaurant.openingHours} variant="pill" />
                            )}
                            {restaurant.activeDeals > 0 && restaurant.bestDiscount != null && (
                                <DiscountBadge label={`Up to ${restaurant.bestDiscount}% off`} size="medium" />
                            )}
                        </Stack>

                    </Box>

                </Stack>

            </Box>

            {/* ---------- Content: main + information sidebar ---------- */}
            <Grid container spacing={{ xs: 3, md: 4 }} sx={{ mt: { xs: 3, md: 4 } }}>

                {/* Sidebar - first on mobile, right column on desktop */}
                <Grid size={{ xs: 12, md: 5, lg: 4 }} sx={{ order: { xs: 1, md: 2 } }}>

                    <Card
                        component="aside"
                        aria-label="Restaurant information"
                        sx={{ position: { md: "sticky" }, top: { md: 88 } }}
                    >
                        <CardContent>

                            {restaurant.description && (
                                <>
                                    <Typography variant="h6" component="h2">
                                        About
                                    </Typography>
                                    <Typography variant="body2" color="text.secondary" sx={{ mt: 0.5, mb: 2.5 }}>
                                        {restaurant.description}
                                    </Typography>
                                </>
                            )}

                            <Stack spacing={2}>

                                <InfoRow icon={<ScheduleIcon />} label="Opening hours">
                                    {hasOpeningHours
                                        ? <OpeningStatus hours={restaurant.openingHours} />
                                        : <Typography variant="body2" color="text.secondary">Opening hours not available.</Typography>}
                                </InfoRow>

                                {restaurant.address && (
                                    <InfoRow icon={<PlaceOutlinedIcon />} label="Address">
                                        {restaurant.address}
                                    </InfoRow>
                                )}

                                {restaurant.phoneNumber && (
                                    <InfoRow icon={<PhoneOutlinedIcon />} label="Phone">
                                        <Link href={`tel:${restaurant.phoneNumber}`}>
                                            {restaurant.phoneNumber}
                                        </Link>
                                    </InfoRow>
                                )}

                                {restaurant.email && (
                                    <InfoRow icon={<EmailOutlinedIcon />} label="Email">
                                        <Link href={`mailto:${restaurant.email}`}>
                                            {restaurant.email}
                                        </Link>
                                    </InfoRow>
                                )}

                                {restaurant.website && (
                                    <InfoRow icon={<LanguageIcon />} label="Website">
                                        <Link
                                            href={websiteHref(restaurant.website)}
                                            target="_blank"
                                            rel="noopener noreferrer"
                                        >
                                            {displayWebsite(restaurant.website)}
                                        </Link>
                                    </InfoRow>
                                )}

                            </Stack>

                            {/* Scrolls to the digital menu (which offers the PDF as a fallback). */}
                            <Button
                                variant="outlined"
                                color="primary"
                                startIcon={<RestaurantMenuIcon />}
                                onClick={scrollToMenu}
                                fullWidth
                                sx={{ mt: 3 }}
                            >
                                View Menu
                            </Button>

                        </CardContent>
                    </Card>

                </Grid>

                {/* Main content */}
                <Grid size={{ xs: 12, md: 7, lg: 8 }} sx={{ order: { xs: 2, md: 1 } }}>

                    <Stack spacing={{ xs: 4, md: 5 }}>

                        {/* Deals */}
                        <Box component="section" aria-labelledby="deals-heading">

                            <SectionHeader
                                id="deals-heading"
                                title="Deals"
                                subtitle="Redeem a walk-in offer, then show it when you arrive."
                            />

                            {deals.length === 0 ? (

                                <Card>
                                    <CardContent sx={{ textAlign: "center", py: 4 }}>
                                        <LocalOfferOutlinedIcon aria-hidden sx={{ fontSize: 32, color: "text.disabled" }} />
                                        <Typography sx={{ mt: 0.5 }}>
                                            No deals available.
                                        </Typography>
                                        <Typography variant="body2" color="text.secondary">
                                            Check back soon for new offers.
                                        </Typography>
                                    </CardContent>
                                </Card>

                            ) : (

                                <Box
                                    sx={{
                                        display: "grid",
                                        gap: 2,
                                        gridTemplateColumns: "repeat(auto-fill, minmax(min(100%, 380px), 1fr))"
                                    }}
                                >
                                    {deals.map((deal) => (

                                        <DealCard
                                            key={deal.id}
                                            deal={deal}
                                            onRedeemed={() => loadDeals(deal.restaurantId)}
                                        />

                                    ))}
                                </Box>

                            )}

                        </Box>

                        {/* Digital menu */}
                        <Box
                            component="section"
                            id={MENU_SECTION_ID}
                            aria-labelledby="menu-heading"
                            sx={{ scrollMarginTop: SECTION_SCROLL_MARGIN }}
                        >

                            <SectionHeader id="menu-heading" title="Menu" />

                            <RestaurantMenu
                                categories={categories}
                                items={menuItems}
                                deals={deals}
                                currencyCode={restaurant.currencyCode}
                                menuPdfUrl={restaurant.menuPdfUrl}
                            />

                        </Box>

                        {/* Gallery */}
                        <Box component="section" aria-labelledby="gallery-heading">

                            <SectionHeader id="gallery-heading" title="Gallery" />

                            {galleryLoading ? (

                                <Grid container spacing={1.5}>
                                    {Array.from({ length: 3 }, (_, i) => (
                                        <Grid key={i} size={{ xs: 6, sm: 4 }}>
                                            <Skeleton variant="rounded" sx={{ aspectRatio: "4 / 3", height: "auto" }} />
                                        </Grid>
                                    ))}
                                </Grid>

                            ) : galleryImages.length === 0 ? (

                                <Stack direction="row" spacing={1} sx={{ alignItems: "center", color: "text.secondary" }}>
                                    <PhotoLibraryOutlinedIcon aria-hidden sx={{ fontSize: 20 }} />
                                    <Typography color="text.secondary">
                                        No gallery images available.
                                    </Typography>
                                </Stack>

                            ) : (

                                <Grid container spacing={1.5}>

                                    {galleryImages.map(image => (

                                        <Grid
                                            key={image.id}
                                            size={{ xs: 6, sm: 4 }}
                                        >

                                            <Box
                                                component="img"
                                                src={getImageUrl(image.imageUrl)}
                                                alt={image.caption ?? restaurant.name}
                                                loading="lazy"
                                                sx={{
                                                    width: "100%",
                                                    aspectRatio: "4 / 3",
                                                    objectFit: "cover",
                                                    display: "block",
                                                    borderRadius: "12px",
                                                    border: "1px solid",
                                                    borderColor: "divider"
                                                }}
                                            />

                                        </Grid>

                                    ))}

                                </Grid>

                            )}

                        </Box>

                    </Stack>

                </Grid>

            </Grid>

        </>

    );

}

export default RestaurantDetailsPage;
