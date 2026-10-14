import { useContext, useEffect, useState } from "react";
import { Link as RouterLink, useLocation, useSearchParams } from "react-router-dom";

import {
    Alert,
    Box,
    Button,
    Card,
    CardContent,
    Chip,
    FormControl,
    Grid,
    InputAdornment,
    InputLabel,
    MenuItem,
    Select,
    Skeleton,
    Stack,
    TextField,
    Typography,
    type SelectChangeEvent
} from "@mui/material";

import ArrowForwardIcon from "@mui/icons-material/ArrowForward";
import EventAvailableOutlinedIcon from "@mui/icons-material/EventAvailableOutlined";
import LocalOfferOutlinedIcon from "@mui/icons-material/LocalOfferOutlined";
import SearchIcon from "@mui/icons-material/Search";
import SearchOffIcon from "@mui/icons-material/SearchOff";
import ShoppingBagOutlinedIcon from "@mui/icons-material/ShoppingBagOutlined";
import StorefrontOutlinedIcon from "@mui/icons-material/StorefrontOutlined";

import RestaurantCard from "../components/restaurants/RestaurantCard";
import { RESTAURANT_GRID_ITEM_SIZE, RESTAURANT_GRID_SPACING } from "../components/restaurants/restaurantGrid";
import LoadMoreButton from "../components/common/LoadMoreButton";
import SectionHeader from "../components/common/SectionHeader";
import NoSurpriseBagsCard from "../components/surpriseBags/NoSurpriseBagsCard";
import PublicSurpriseBagList from "../components/surpriseBags/PublicSurpriseBagList";
import AuthContext from "../features/auth/AuthContext";
import { useLoadMore } from "../hooks/useLoadMore";
import { hasEligibleOffer } from "../utils/cardOffers";
import { getImageUrl } from "../utils/imageUrl";
import HeroImageRotator from "../components/home/HeroImageRotator";
import { APPROVED_RESTAURANT_HERO_PHOTOS, type HeroImage } from "../config/heroImages";
import surpriseBagIllustration from "../assets/hero/surprise-bag.svg";
import { MenuTab, type MenuTabValue } from "../utils/menuPricing";

import RestaurantService from "../services/RestaurantService";
import UserFavoriteService from "../services/UserFavoriteService";
import AreaService from "../services/AreaService";
import CuisineService from "../services/CuisineService";

import type { Restaurant } from "../types/Restaurant";
import type { UserFavorite } from "../types/UserFavorite";
import type { Area } from "../types/Area";
import type { Cuisine } from "../types/Cuisine";

// Offer Type filter value for restaurants with a current deal of either
// type (Dine-in or Takeaway), e.g. the header's Deals link.
const ANY_DEAL_OFFER_VALUE = "deals";

// Offer Type filter: matches restaurants with at least one eligible
// deal of that type (same rules as the card overlays). Values are the
// API's OfferType numbers as strings, or ANY_DEAL_OFFER_VALUE;
// "" = Any Offer (no offer filter).
const OFFER_TYPE_OPTIONS = [
    { value: ANY_DEAL_OFFER_VALUE, label: "Dine-in or Takeaway" },
    { value: String(MenuTab.DineIn), label: "Dine-in" },
    { value: String(MenuTab.Takeaway), label: "Takeaway" }
];

// ?offer=deals / ?offer=dine-in / ?offer=takeaway (e.g. the header's
// Deals link and the footer's Dine-in Deals and Takeaway Deals links)
// preselects the Offer Type filter.
const OFFER_URL_VALUES: Record<string, string> = {
    "deals": ANY_DEAL_OFFER_VALUE,
    "dine-in": String(MenuTab.DineIn),
    "takeaway": String(MenuTab.Takeaway)
};

// True when the restaurant matches the selected Offer Type filter.
function matchesOfferFilter(restaurant: Restaurant, offerType: string, now: Date): boolean {

    if (offerType === "")
        return true;

    if (offerType === ANY_DEAL_OFFER_VALUE)
        return hasEligibleOffer(restaurant.dealSummaries, MenuTab.DineIn, now) ||
            hasEligibleOffer(restaurant.dealSummaries, MenuTab.Takeaway, now);

    return hasEligibleOffer(restaurant.dealSummaries, Number(offerType) as MenuTabValue, now);

}

// Home: Top Restaurant Deals and Featured Surprise Bags sit side by side
// on large screens (deals 7/12, bags 5/12) and stack below that.
const TOP_DEALS_COLUMN_SIZE = { xs: 12, lg: 7 };
const FEATURED_BAGS_COLUMN_SIZE = { xs: 12, lg: 5 };

// Top deal cards: 1 column on phones, 2 on tablets, 4 across the full
// width on laptops, 2 within the deals column on large screens.
// (The main listing uses the shared 3-across RESTAURANT_GRID_ITEM_SIZE.)
const TOP_DEALS_GRID_ITEM_SIZE = { xs: 12, sm: 6, md: 3, lg: 6 };

// Featured bags: matches the deal cards' rows (2 x 2 on tablets and
// laptops, one column beside the deals on large screens).
const FEATURED_BAGS_COLUMNS = {
    xs: "minmax(0, 1fr)",
    sm: "repeat(2, minmax(0, 1fr))",
    lg: "minmax(0, 1fr)"
};

const FEATURED_BAGS_COUNT = 4;

// Hero panels: text beside the image on tablets and large screens; on
// laptops (two narrow panels) the image sits above the text; phones show
// text only.
const HERO_PANEL_SX = {
    borderRadius: "16px",
    overflow: "hidden",
    display: "grid",
    gridTemplateAreas: { xs: '"text"', sm: '"text image"', md: '"image" "text"', lg: '"text image"' },
    gridTemplateColumns: {
        xs: "minmax(0, 1fr)",
        sm: "minmax(0, 1.3fr) minmax(0, 1fr)",
        md: "minmax(0, 1fr)",
        lg: "minmax(0, 1.3fr) minmax(0, 1fr)"
    },
    gridTemplateRows: { md: "180px auto", lg: "auto" }
};

const HERO_TEXT_SX = {
    gridArea: "text",
    p: { xs: 3, sm: 4, lg: 5 },
    display: "flex",
    flexDirection: "column",
    justifyContent: "center",
    alignItems: "flex-start"
};

const HERO_IMAGE_SX = {
    gridArea: "image",
    display: { xs: "none", sm: "block" },
    position: "relative",
    m: 1,
    mb: { md: 0 },
    minHeight: { sm: 280, md: 0, lg: 340 },
    borderRadius: "12px",
    overflow: "hidden"
};

const HERO_TITLE_SX = { mt: 2, color: "inherit", fontSize: { xs: "2rem", sm: "2.25rem", lg: "2.75rem" } };

const HERO_BADGE_SX = {
    display: "inline-flex",
    alignItems: "center",
    gap: 0.75,
    px: 1.25,
    py: 0.5,
    borderRadius: "8px",
    fontSize: "0.8125rem",
    fontWeight: 600
};

// Home: how deals and Surprise Bags work, side by side (each step names
// both, so bags are never described as deal redemptions).
const HOW_IT_WORKS_STEPS = [
    {
        icon: <SearchIcon />,
        title: "Find food near you",
        text: "Browse restaurants by area or cuisine, or search for a place or a discount."
    },
    {
        icon: <LocalOfferOutlinedIcon />,
        title: "Choose a deal or a Surprise Bag",
        text: "Pick a walk-in deal for a discount on your meal, or a Surprise Bag of a restaurant's surplus food at a lower price."
    },
    {
        icon: <EventAvailableOutlinedIcon />,
        title: "Redeem or reserve",
        text: "Redeem a deal for the time you plan to arrive, or reserve a Surprise Bag for its pickup time."
    },
    {
        icon: <StorefrontOutlinedIcon />,
        title: "Visit the restaurant",
        text: "Show your redemption to get the discount on your bill, or pay for your Surprise Bag and show its confirmation code when you collect it."
    }
];

// Filter dropdowns keep a usable width and scroll sideways on phones.
const FILTER_SX = { minWidth: { xs: 160, md: 180 }, flex: { md: 1 }, flexShrink: 0 };

// Read by screen readers only (the page's h1 above the two hero panels).
const VISUALLY_HIDDEN_SX = {
    position: "absolute",
    width: "1px",
    height: "1px",
    overflow: "hidden",
    clip: "rect(0 0 0 0)",
    whiteSpace: "nowrap"
};

// Scroll target for the Search button and the cuisine shortcuts.
const RESULTS_ID = "restaurant-results";

function scrollToResults() {

    document
        .getElementById(RESULTS_ID)
        ?.scrollIntoView({ behavior: "smooth", block: "start" });

}

function RestaurantsPage() {

    // "/" is Home: the listing gets a hero and discovery sections on top.
    const isHome = useLocation().pathname === "/";

    // Favourites are a customer feature (the heart only shows for customers).
    const auth = useContext(AuthContext);
    const isCustomer = auth?.user?.role === "Customer";

    const [restaurants, setRestaurants] = useState<Restaurant[]>([]);
    const [favorites, setFavorites] = useState<UserFavorite[]>([]);

    const [areas, setAreas] = useState<Area[]>([]);
    const [cuisines, setCuisines] = useState<Cuisine[]>([]);

    const [loading, setLoading] = useState(true);
    const [loadError, setLoadError] = useState(false);

    const [search, setSearch] = useState("");

    const [selectedArea, setSelectedArea] = useState("");
    const [selectedCuisine, setSelectedCuisine] = useState("");
    const [searchParams] = useSearchParams();
    const offerFromUrl = OFFER_URL_VALUES[searchParams.get("offer") ?? ""] ?? "";

    const [selectedOfferType, setSelectedOfferType] = useState(offerFromUrl);

    // A new ?offer= link (even while this page is open) re-applies the
    // filter; the dropdown stays freely changeable otherwise.
    const [appliedOfferFromUrl, setAppliedOfferFromUrl] = useState(offerFromUrl);

    if (offerFromUrl !== appliedOfferFromUrl) {
        setAppliedOfferFromUrl(offerFromUrl);
        setSelectedOfferType(offerFromUrl);
    }

    useEffect(() => {

        loadData();

    }, [isCustomer]); // eslint-disable-line react-hooks/exhaustive-deps

    async function loadData() {

        try {

            const restaurantsData =
                await RestaurantService.getAll();

            setRestaurants(restaurantsData);

            const areasData =
                await AreaService.getAll();

            setAreas(areasData);

            const cuisinesData =
                await CuisineService.getAll();

            setCuisines(cuisinesData);

            if (isCustomer) {

                try {

                    const favoritesData =
                        await UserFavoriteService.getMyFavorites();

                    setFavorites(favoritesData);

                }
                catch {

                    // Favourites are optional on this page. An expired
                    // session (401) is handled globally: the user is
                    // signed out, told once and sent to /login.

                }

            }
            else {

                // Signed out (e.g. the session expired): no favourite
                // state from the previous user's session.
                setFavorites([]);

            }

            setLoadError(false);

        }
        catch (error) {

            console.error(error);

            setLoadError(true);

        }
        finally {

            setLoading(false);

        }

    }

    // One "now" for the whole filter pass, so every restaurant is judged
    // at the same moment.
    const now = new Date();

    const filteredRestaurants = restaurants.filter(r => {

        const isActive = r.isActive;

        const keyword = search.trim().toLowerCase();

        const matchesSearch =

            r.name.toLowerCase().includes(keyword) ||

            r.description.toLowerCase().includes(keyword) ||

            r.areaName.toLowerCase().includes(keyword) ||

            (r.bestDiscount?.toString() ?? "").includes(keyword);

        const matchesArea =

            selectedArea === "" ||

            r.areaId === Number(selectedArea);

        const matchesCuisine =

            selectedCuisine === "" ||

            r.cuisines.includes(selectedCuisine);

        // Actual eligible deals (not sold out, usable today or later),
        // not the restaurant's dining-type tags.
        const matchesOfferType =

            matchesOfferFilter(r, selectedOfferType, now);

        return isActive &&
            matchesSearch &&
            matchesArea &&
            matchesCuisine &&
            matchesOfferType;

    });

    // e.g. "Dine-in" (for the empty-results message); undefined = Any Offer.
    const selectedOfferTypeLabel =
        OFFER_TYPE_OPTIONS.find(o => o.value === selectedOfferType)?.label;

    const hasFilters =
        search !== "" ||
        selectedArea !== "" ||
        selectedCuisine !== "" ||
        selectedOfferType !== "";

    // Show the results 20 at a time ("View next 20 venues"). A new
    // search, filter or page (Home vs Restaurants) starts again from
    // the first 20. The list is already loaded, so no extra API calls.
    const results = useLoadMore(
        filteredRestaurants,
        [isHome, search.trim().toLowerCase(), selectedArea, selectedCuisine, selectedOfferType].join("|")
    );

    function clearFilters() {

        setSearch("");
        setSelectedArea("");
        setSelectedCuisine("");
        setSelectedOfferType("");

    }

    // ---------- Home discovery data (derived from loaded restaurants) ----------

    // Active restaurants with offers, best discount first.
    const dealRestaurants = restaurants
        .filter(r => r.isActive && r.activeDeals > 0 && r.bestDiscount != null)
        .sort((a, b) => (b.bestDiscount ?? 0) - (a.bestDiscount ?? 0));

    const topDeals = dealRestaurants.slice(0, 4);

    const bestDiscount = dealRestaurants[0]?.bestDiscount ?? null;

    // Approved restaurant photos for the hero (config/heroImages.ts), used
    // only while they are an active restaurant's cover or, for a gallery
    // photo, while its restaurant is active. Uses the already-loaded list.
    const heroPhotos: HeroImage[] = APPROVED_RESTAURANT_HERO_PHOTOS
        .filter(path => {
            if (restaurants.some(r => r.isActive && r.coverImageUrl === path)) return true;
            const restaurantId = Number(path.match(/^\/uploads\/restaurants\/(\d+)\/gallery\//)?.[1]);
            return restaurants.some(r => r.id === restaurantId && r.isActive);
        })
        .map(path => ({ src: getImageUrl(path), alt: "" }));

    // Cuisines that at least one active restaurant serves.
    const cuisineShortcuts = cuisines
        .filter(c => restaurants.some(r => r.isActive && r.cuisines.includes(c.name)))
        .slice(0, 12);

    function selectCuisine(name: string) {

        setSelectedCuisine(current => current === name ? "" : name);
        scrollToResults();

    }

    const searchField = (
        <TextField
            value={search}
            onChange={(e) =>
                setSearch(e.target.value)
            }
            // On Home the results are further down: Enter jumps to them.
            onKeyDown={isHome ? (e) => { if (e.key === "Enter") scrollToResults(); } : undefined}
            placeholder="Search restaurants, areas or discounts"
            sx={{ flex: { md: 1.6 } }}
            slotProps={{
                htmlInput: { "aria-label": "Search restaurants" },
                input: {
                    startAdornment: (
                        <InputAdornment position="start">
                            <SearchIcon aria-hidden />
                        </InputAdornment>
                    )
                }
            }}
        />
    );

    const resultCount = !loading && !loadError && (
        <Typography variant="body2" color="text.secondary" aria-live="polite">
            {filteredRestaurants.length} {filteredRestaurants.length === 1 ? "restaurant" : "restaurants"}
        </Typography>
    );

    return (

        <>

            {isHome ? (

                <>

                    {/* ---------- Hero: restaurant deals | Surprise Bags ---------- */}
                    <Box
                        component="section"
                        aria-labelledby="home-hero-title"
                        sx={{
                            display: "grid",
                            gridTemplateColumns: { xs: "minmax(0, 1fr)", md: "repeat(2, minmax(0, 1fr))" },
                            gap: 2,
                            mb: 2
                        }}
                    >

                        <Typography id="home-hero-title" variant="h1" sx={VISUALLY_HIDDEN_SX}>
                            CraveDine: restaurant deals and Surprise Bags
                        </Typography>

                        {/* Restaurant deals */}
                        <Box sx={{ ...HERO_PANEL_SX, bgcolor: "inverse.main", color: "inverse.contrastText" }}>

                            <Box sx={HERO_TEXT_SX}>

                                <Box component="span" sx={{ ...HERO_BADGE_SX, bgcolor: "rgba(255, 255, 255, 0.10)" }}>
                                    <LocalOfferOutlinedIcon aria-hidden sx={{ fontSize: 16, color: "primary.light" }} />
                                    Walk-in restaurant deals
                                </Box>

                                <Typography variant="h1" component="h2" sx={HERO_TITLE_SX}>
                                    Great food.{" "}
                                    <Box component="span" sx={{ color: "primary.light", whiteSpace: "nowrap" }}>Better deals.</Box>
                                </Typography>

                                <Typography sx={{ mt: 1.5, color: "rgba(255, 255, 255, 0.82)", maxWidth: 480 }}>
                                    Discover walk-in offers at restaurants near you. Pick your arrival time,
                                    redeem in seconds and save on your bill.
                                </Typography>

                                <Button
                                    component={RouterLink}
                                    to="/restaurants?offer=deals"
                                    variant="contained"
                                    size="large"
                                    endIcon={<ArrowForwardIcon />}
                                    sx={{ mt: 3 }}
                                >
                                    Browse restaurant deals
                                </Button>

                                {!loading && !loadError && dealRestaurants.length > 0 && (
                                    <Typography variant="body2" sx={{ mt: 2, color: "rgba(255, 255, 255, 0.72)" }}>
                                        {dealRestaurants.length} {dealRestaurants.length === 1 ? "restaurant" : "restaurants"} with offers
                                        {bestDiscount != null && ` · up to ${bestDiscount}% off`}
                                    </Typography>
                                )}

                            </Box>

                            {/* Rotating food and restaurant imagery. Fixed size,
                                so rotating images never shift the layout. */}
                            <Box aria-hidden sx={HERO_IMAGE_SX}>
                                <HeroImageRotator restaurantPhotos={heroPhotos} />
                            </Box>

                        </Box>

                        {/* Surprise Bags */}
                        <Box sx={{ ...HERO_PANEL_SX, bgcolor: "successSoft", color: "text.primary" }}>

                            <Box sx={HERO_TEXT_SX}>

                                <Box component="span" sx={{ ...HERO_BADGE_SX, bgcolor: "background.paper", color: "success.main" }}>
                                    <ShoppingBagOutlinedIcon aria-hidden sx={{ fontSize: 16 }} />
                                    Surprise Bags
                                </Box>

                                <Typography variant="h1" component="h2" sx={HERO_TITLE_SX}>
                                    Good food.{" "}
                                    <Box component="span" sx={{ color: "success.main", whiteSpace: "nowrap" }}>Less waste.</Box>
                                </Typography>

                                <Typography color="text.secondary" sx={{ mt: 1.5, maxWidth: 480 }}>
                                    Reserve a bag of a restaurant's surplus food at a lower price,
                                    then pay and collect it at the pickup time.
                                </Typography>

                                <Button
                                    component={RouterLink}
                                    to="/surprise-bags"
                                    variant="contained"
                                    color="success"
                                    size="large"
                                    endIcon={<ArrowForwardIcon />}
                                    sx={{ mt: 3 }}
                                >
                                    Browse surprise bags
                                </Button>

                            </Box>

                            <Box aria-hidden sx={HERO_IMAGE_SX}>
                                <Box
                                    component="img"
                                    src={surpriseBagIllustration}
                                    alt=""
                                    sx={{ position: "absolute", inset: 0, width: "100%", height: "100%", objectFit: "cover", display: "block" }}
                                />
                            </Box>

                        </Box>

                    </Box>

                    {/* ---------- Search (filters "All restaurants" below) ---------- */}
                    <Card component="section" aria-label="Search restaurants" sx={{ mb: { xs: 4, md: 5 } }}>
                        <Stack direction={{ xs: "column", sm: "row" }} spacing={1.5} sx={{ p: { xs: 2, sm: 2.5 } }}>
                            <Box sx={{ flex: 1, display: "flex", "& > .MuiTextField-root": { flex: 1 } }}>
                                {searchField}
                            </Box>
                            <Button
                                variant="contained"
                                size="large"
                                startIcon={<SearchIcon />}
                                onClick={scrollToResults}
                                sx={{ flexShrink: 0, minHeight: 56, px: 3 }}
                            >
                                Search
                            </Button>
                        </Stack>
                    </Card>

                    {/* ---------- Explore by cuisine ---------- */}
                    {cuisineShortcuts.length > 0 && (
                        <Box component="section" aria-labelledby="cuisine-heading" sx={{ mb: { xs: 4, md: 5 } }}>
                            <SectionHeader id="cuisine-heading" title="Explore by cuisine" />
                            <Stack direction="row" useFlexGap spacing={1} sx={{ flexWrap: "wrap" }}>
                                {cuisineShortcuts.map(c => {
                                    const selected = selectedCuisine === c.name;
                                    return (
                                        <Chip
                                            key={c.id}
                                            label={c.name}
                                            clickable
                                            onClick={() => selectCuisine(c.name)}
                                            color={selected ? "primary" : "default"}
                                            variant={selected ? "filled" : "outlined"}
                                            aria-pressed={selected}
                                            sx={{ height: 38, px: 0.5, fontSize: "0.875rem", bgcolor: selected ? undefined : "background.paper" }}
                                        />
                                    );
                                })}
                            </Stack>
                        </Box>
                    )}

                    {/* ---------- Top Restaurant Deals | Featured Surprise Bags
                         (unfiltered view only, like the old Top deals strip) ---------- */}
                    {!hasFilters && (
                        <Grid container spacing={{ xs: 4, lg: 3 }} sx={{ mb: { xs: 4, md: 5 } }}>

                            <Grid component="section" aria-labelledby="top-deals-heading" size={TOP_DEALS_COLUMN_SIZE}>
                                <SectionHeader
                                    id="top-deals-heading"
                                    title="Top Restaurant Deals"
                                    subtitle="The biggest walk-in discounts available."
                                    action={
                                        <Button component={RouterLink} to="/restaurants?offer=deals" endIcon={<ArrowForwardIcon />}>
                                            View all deals
                                        </Button>
                                    }
                                />

                                {loading ? (

                                    <Grid container spacing={2} aria-busy="true" aria-label="Loading top deals">
                                        {Array.from({ length: 4 }, (_, i) => (
                                            <Grid key={i} size={TOP_DEALS_GRID_ITEM_SIZE}>
                                                <Card>
                                                    <Skeleton variant="rectangular" sx={{ aspectRatio: "16 / 10", height: "auto" }} />
                                                    <CardContent>
                                                        <Skeleton width="70%" height={28} />
                                                        <Skeleton width="50%" />
                                                    </CardContent>
                                                </Card>
                                            </Grid>
                                        ))}
                                    </Grid>

                                ) : loadError ? (

                                    <Alert severity="error">
                                        We couldn't load restaurant deals right now. Please try again later.
                                    </Alert>

                                ) : topDeals.length === 0 ? (

                                    <Card>
                                        <CardContent sx={{ textAlign: "center", py: 4 }}>
                                            <LocalOfferOutlinedIcon aria-hidden sx={{ fontSize: 40, color: "text.disabled" }} />
                                            <Typography variant="h6" component="p" sx={{ mt: 1 }}>
                                                No restaurant deals right now
                                            </Typography>
                                            <Typography color="text.secondary" sx={{ mt: 0.5 }}>
                                                Check back soon, or browse all restaurants below.
                                            </Typography>
                                        </CardContent>
                                    </Card>

                                ) : (

                                    <Grid container spacing={2}>
                                        {topDeals.map(restaurant => (
                                            <Grid key={restaurant.id} size={TOP_DEALS_GRID_ITEM_SIZE}>
                                                <RestaurantCard
                                                    restaurant={restaurant}
                                                    isFavorite={favorites.some(f => f.restaurantId === restaurant.id)}
                                                    onFavoriteChanged={loadData}
                                                />
                                            </Grid>
                                        ))}
                                    </Grid>

                                )}
                            </Grid>

                            <Grid component="section" aria-labelledby="featured-bags-heading" size={FEATURED_BAGS_COLUMN_SIZE}>
                                <SectionHeader
                                    id="featured-bags-heading"
                                    title="Featured Surprise Bags"
                                    subtitle="Surplus food from local restaurants, at a lower price."
                                    action={
                                        <Button component={RouterLink} to="/surprise-bags" color="success" endIcon={<ArrowForwardIcon />}>
                                            View all surprise bags
                                        </Button>
                                    }
                                />

                                <PublicSurpriseBagList
                                    take={FEATURED_BAGS_COUNT}
                                    gridTemplateColumns={FEATURED_BAGS_COLUMNS}
                                    emptyState={<NoSurpriseBagsCard compact />}
                                    skeletonCount={2}
                                />
                            </Grid>

                        </Grid>
                    )}

                    {/* ---------- All restaurants header ---------- */}
                    <Stack
                        id={RESULTS_ID}
                        direction="row"
                        sx={{ justifyContent: "space-between", alignItems: "flex-end", gap: 1, mb: 2, scrollMarginTop: { xs: 76, md: 88 } }}
                    >
                        <Typography variant="h5" component="h2">
                            All restaurants
                        </Typography>
                        {resultCount}
                    </Stack>

                </>

            ) : (

                /* ---------- Listing page header ---------- */
                <Stack
                    id={RESULTS_ID}
                    direction={{ xs: "column", sm: "row" }}
                    sx={{ justifyContent: "space-between", alignItems: { sm: "flex-end" }, gap: 0.5, mb: 2.5 }}
                >
                    <Box>
                        <Typography variant="h4" component="h1">
                            Restaurants
                        </Typography>
                        <Typography color="text.secondary" sx={{ mt: 0.5 }}>
                            Discover walk-in deals at restaurants near you.
                        </Typography>
                    </Box>

                    {resultCount}
                </Stack>

            )}

            {/* Search and filters (on Home, search has its own band below the hero) */}
            <Stack
                direction={{ xs: "column", md: "row" }}
                spacing={1.5}
                sx={{ mb: 3 }}
            >

                {!isHome && searchField}

                <Stack
                    direction="row"
                    spacing={1.5}
                    sx={{
                        flex: { md: 2.4 },
                        overflowX: { xs: "auto", md: "visible" },
                        pt: { xs: 0.75, md: 0 },
                        pb: { xs: 0.5, md: 0 },
                        alignItems: "center"
                    }}
                >

                    <FormControl sx={FILTER_SX}>

                        <InputLabel id="filter-area-label">Area</InputLabel>

                        <Select
                            labelId="filter-area-label"
                            label="Area"
                            value={selectedArea}
                            onChange={(e: SelectChangeEvent) =>
                                setSelectedArea(e.target.value)
                            }
                        >

                            <MenuItem value="">
                                All Areas
                            </MenuItem>

                            {areas.map(area => (

                                <MenuItem
                                    key={area.id}
                                    value={area.id.toString()}
                                >
                                    {area.name}
                                </MenuItem>

                            ))}

                        </Select>

                    </FormControl>

                    <FormControl sx={FILTER_SX}>

                        <InputLabel id="filter-cuisine-label">Cuisine</InputLabel>

                        <Select
                            labelId="filter-cuisine-label"
                            label="Cuisine"
                            value={selectedCuisine}
                            onChange={(e: SelectChangeEvent) =>
                                setSelectedCuisine(e.target.value)
                            }
                        >

                            <MenuItem value="">
                                All Cuisines
                            </MenuItem>

                            {cuisines.map(cuisine => (

                                <MenuItem
                                    key={cuisine.id}
                                    value={cuisine.name}
                                >
                                    {cuisine.name}
                                </MenuItem>

                            ))}

                        </Select>

                    </FormControl>

                    <FormControl sx={FILTER_SX}>

                        <InputLabel id="filter-offer-type-label">Offer Type</InputLabel>

                        <Select
                            labelId="filter-offer-type-label"
                            label="Offer Type"
                            value={selectedOfferType}
                            onChange={(e: SelectChangeEvent) =>
                                setSelectedOfferType(e.target.value)
                            }
                        >

                            <MenuItem value="">
                                Any Offer
                            </MenuItem>

                            {OFFER_TYPE_OPTIONS.map(option => (

                                <MenuItem
                                    key={option.value}
                                    value={option.value}
                                >
                                    {option.label}
                                </MenuItem>

                            ))}

                        </Select>

                    </FormControl>

                    {hasFilters && (
                        <Button onClick={clearFilters} sx={{ flexShrink: 0, whiteSpace: "nowrap" }}>
                            Clear filters
                        </Button>
                    )}

                </Stack>

            </Stack>

            {/* Results */}
            {loading ? (

                <Grid container spacing={RESTAURANT_GRID_SPACING} aria-busy="true" aria-label="Loading restaurants">
                    {Array.from({ length: 6 }, (_, i) => (
                        <Grid key={i} size={RESTAURANT_GRID_ITEM_SIZE}>
                            <Card>
                                <Skeleton variant="rectangular" sx={{ aspectRatio: "16 / 10", height: "auto" }} />
                                <CardContent>
                                    <Skeleton width="70%" height={28} />
                                    <Skeleton width="50%" />
                                    <Skeleton />
                                    <Skeleton width="85%" />
                                </CardContent>
                            </Card>
                        </Grid>
                    ))}
                </Grid>

            ) : loadError ? (

                <Alert severity="error">
                    We couldn't load restaurants right now. Please try again later.
                </Alert>

            ) : filteredRestaurants.length === 0 ? (

                <Card>
                    <CardContent sx={{ textAlign: "center", py: 6 }}>
                        <SearchOffIcon aria-hidden sx={{ fontSize: 40, color: "text.disabled" }} />
                        <Typography variant="h6" component="p" sx={{ mt: 1 }}>
                            No restaurants found
                        </Typography>
                        <Typography color="text.secondary" sx={{ mt: 0.5 }}>
                            {selectedOfferTypeLabel
                                ? `No current ${selectedOfferTypeLabel} offers match your filters. Try another offer type or clear your filters.`
                                : hasFilters
                                    ? "Try a different search or clear your filters."
                                    : "Check back soon for new restaurants."}
                        </Typography>
                        {hasFilters && (
                            <Button variant="outlined" onClick={clearFilters} sx={{ mt: 2 }}>
                                Clear filters
                            </Button>
                        )}
                    </CardContent>
                </Card>

            ) : (

                <>

                    <Grid container spacing={RESTAURANT_GRID_SPACING}>

                        {results.visibleItems.map(restaurant => (

                            <Grid
                                key={restaurant.id}
                                size={RESTAURANT_GRID_ITEM_SIZE}
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

                    <LoadMoreButton loadMore={results} />

                </>

            )}

            {/* ---------- How CraveDine works (home only) ---------- */}
            {isHome && (
                <Box component="section" aria-labelledby="how-it-works-heading" sx={{ mt: { xs: 5, md: 7 } }}>
                    <SectionHeader
                        id="how-it-works-heading"
                        title="How CraveDine works"
                        subtitle="Save with restaurant deals, or rescue surplus food with Surprise Bags."
                        action={
                            <Button component={RouterLink} to="/how-it-works" endIcon={<ArrowForwardIcon />}>
                                Learn more
                            </Button>
                        }
                    />
                    <Grid container spacing={2} component="ol" sx={{ listStyle: "none", p: 0, m: 0 }}>
                        {HOW_IT_WORKS_STEPS.map((step, index) => (
                            <Grid key={step.title} component="li" size={{ xs: 12, sm: 6, md: 3 }}>
                                <Card sx={{ height: "100%" }}>
                                    <CardContent sx={{ p: { xs: 2.5, md: 3 } }}>
                                        <Stack direction="row" spacing={1.5} sx={{ alignItems: "center" }}>
                                            <Box
                                                aria-hidden
                                                sx={{
                                                    width: 40,
                                                    height: 40,
                                                    borderRadius: "10px",
                                                    bgcolor: "primarySoft",
                                                    color: "primary.main",
                                                    display: "grid",
                                                    placeItems: "center",
                                                    flexShrink: 0
                                                }}
                                            >
                                                {step.icon}
                                            </Box>
                                            <Typography variant="overline" color="text.secondary" sx={{ lineHeight: 1.4 }}>
                                                Step {index + 1}
                                            </Typography>
                                        </Stack>
                                        <Typography variant="subtitle1" component="h3" sx={{ mt: 1.5, fontWeight: 700 }}>
                                            {step.title}
                                        </Typography>
                                        <Typography variant="body2" color="text.secondary" sx={{ mt: 0.5 }}>
                                            {step.text}
                                        </Typography>
                                    </CardContent>
                                </Card>
                            </Grid>
                        ))}
                    </Grid>
                </Box>
            )}

        </>

    );

}

export default RestaurantsPage;
