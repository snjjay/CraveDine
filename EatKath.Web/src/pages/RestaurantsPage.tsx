import { useContext, useEffect, useState } from "react";
import { useLocation } from "react-router-dom";

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

import ArrowDownwardIcon from "@mui/icons-material/ArrowDownward";
import LocalOfferOutlinedIcon from "@mui/icons-material/LocalOfferOutlined";
import SearchIcon from "@mui/icons-material/Search";
import SearchOffIcon from "@mui/icons-material/SearchOff";

import RestaurantCard from "../components/restaurants/RestaurantCard";
import { RESTAURANT_GRID_ITEM_SIZE, RESTAURANT_GRID_SPACING } from "../components/restaurants/restaurantGrid";
import LoadMoreButton from "../components/common/LoadMoreButton";
import SectionHeader from "../components/common/SectionHeader";
import AuthContext from "../features/auth/AuthContext";
import { useLoadMore } from "../hooks/useLoadMore";
import { hasEligibleOffer } from "../utils/cardOffers";
import { getImageUrl } from "../utils/imageUrl";
import HeroImageRotator from "../components/home/HeroImageRotator";
import { APPROVED_RESTAURANT_HERO_PHOTOS, type HeroImage } from "../config/heroImages";
import { MenuTab, type MenuTabValue } from "../utils/menuPricing";

import RestaurantService from "../services/RestaurantService";
import UserFavoriteService from "../services/UserFavoriteService";
import AreaService from "../services/AreaService";
import CuisineService from "../services/CuisineService";

import type { Restaurant } from "../types/Restaurant";
import type { UserFavorite } from "../types/UserFavorite";
import type { Area } from "../types/Area";
import type { Cuisine } from "../types/Cuisine";

// Offer Type filter: matches restaurants with at least one eligible
// deal of that type (same rules as the card overlays). Values are the
// API's OfferType numbers as strings; "" = Any Offer.
const OFFER_TYPE_OPTIONS = [
    { value: String(MenuTab.DineIn), label: "Dine-in" },
    { value: String(MenuTab.Takeaway), label: "Takeaway" }
];

// Top deals strip: 1 column on phones, 2 on tablets, 3 on laptops, 4 on large screens.
// (The main listing uses the shared 3-across RESTAURANT_GRID_ITEM_SIZE.)
const TOP_DEALS_GRID_ITEM_SIZE = { xs: 12, sm: 6, md: 4, lg: 3 };

// Filter dropdowns keep a usable width and scroll sideways on phones.
const FILTER_SX = { minWidth: { xs: 160, md: 180 }, flex: { md: 1 }, flexShrink: 0 };

// Scroll target for "Browse restaurants" and the cuisine shortcuts.
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
    const [selectedOfferType, setSelectedOfferType] = useState("");

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

                    // Favourites are optional on this page.

                }

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

            selectedOfferType === "" ||

            hasEligibleOffer(r.dealSummaries, Number(selectedOfferType) as MenuTabValue, now);

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
    // only while their restaurant is active and, for a cover, while it is
    // still that restaurant's cover. Uses the already-loaded list.
    const heroPhotos: HeroImage[] = APPROVED_RESTAURANT_HERO_PHOTOS
        .filter(path => {
            const restaurantId = Number(path.match(/^\/uploads\/restaurants\/(\d+)\//)?.[1]);
            const restaurant = restaurants.find(r => r.id === restaurantId && r.isActive);
            return !!restaurant && (path.includes("/gallery/") || restaurant.coverImageUrl === path);
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

                    {/* ---------- Home hero ---------- */}
                    <Box
                        component="section"
                        aria-labelledby="home-hero-title"
                        sx={{
                            borderRadius: "16px",
                            overflow: "hidden",
                            bgcolor: "text.primary",
                            color: "#FFFFFF",
                            display: "grid",
                            gridTemplateColumns: { xs: "minmax(0, 1fr)", md: "minmax(0, 1.15fr) minmax(0, 1fr)" },
                            mb: { xs: 4, md: 5 }
                        }}
                    >

                        <Box sx={{ p: { xs: 3, sm: 4, md: 6 }, display: "flex", flexDirection: "column", justifyContent: "center" }}>

                            <Box
                                component="span"
                                sx={{
                                    alignSelf: "flex-start",
                                    display: "inline-flex",
                                    alignItems: "center",
                                    gap: 0.75,
                                    px: 1.25,
                                    py: 0.5,
                                    borderRadius: "8px",
                                    bgcolor: "rgba(255, 255, 255, 0.10)",
                                    fontSize: "0.8125rem",
                                    fontWeight: 600
                                }}
                            >
                                <LocalOfferOutlinedIcon aria-hidden sx={{ fontSize: 16, color: "primary.light" }} />
                                Walk-in restaurant deals
                            </Box>

                            <Typography
                                id="home-hero-title"
                                variant="h1"
                                sx={{ mt: 2, color: "inherit", fontSize: { xs: "2.125rem", sm: "2.5rem", md: "3rem" } }}
                            >
                                Great food.{" "}
                                <Box component="span" sx={{ color: "primary.light", whiteSpace: "nowrap" }}>Better deals.</Box>
                            </Typography>

                            <Typography sx={{ mt: 1.5, color: "rgba(255, 255, 255, 0.82)", maxWidth: 520, fontSize: { md: "1.0625rem" } }}>
                                Discover walk-in offers at restaurants near you. Pick your arrival time,
                                redeem in seconds and save on your bill.
                            </Typography>

                            <Stack direction={{ xs: "column", sm: "row", md: "column", lg: "row" }} spacing={1.5} sx={{ mt: 3, maxWidth: 600 }}>
                                <Box sx={{ flex: 1, display: "flex", "& > .MuiTextField-root": { flex: 1 } }}>
                                    {searchField}
                                </Box>
                                <Button
                                    variant="contained"
                                    size="large"
                                    endIcon={<ArrowDownwardIcon />}
                                    onClick={scrollToResults}
                                    sx={{ flexShrink: 0, minHeight: 56 }}
                                >
                                    Browse restaurants
                                </Button>
                            </Stack>

                            {!loading && !loadError && dealRestaurants.length > 0 && (
                                <Typography variant="body2" sx={{ mt: 2, color: "rgba(255, 255, 255, 0.72)" }}>
                                    {dealRestaurants.length} {dealRestaurants.length === 1 ? "restaurant" : "restaurants"} with offers
                                    {bestDiscount != null && ` · up to ${bestDiscount}% off`}
                                </Typography>
                            )}

                        </Box>

                        {/* Rotating food and restaurant imagery (desktop only).
                            Fixed size, so rotating images never shift the layout. */}
                        <Box
                            aria-hidden
                            sx={{
                                display: { xs: "none", md: "block" },
                                position: "relative",
                                m: 1,
                                minHeight: 380
                            }}
                        >
                            <HeroImageRotator restaurantPhotos={heroPhotos} />
                        </Box>

                    </Box>

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

                    {/* ---------- Top deals (unfiltered view only) ---------- */}
                    {!loading && !loadError && !hasFilters && topDeals.length > 0 && (
                        <Box component="section" aria-labelledby="top-deals-heading" sx={{ mb: { xs: 4, md: 5 } }}>
                            <SectionHeader
                                id="top-deals-heading"
                                title="Top deals right now"
                                subtitle="The biggest walk-in discounts available."
                            />
                            <Grid container spacing={3}>
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
                        </Box>
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

            {/* Search and filters (on Home, search lives in the hero) */}
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

        </>

    );

}

export default RestaurantsPage;
