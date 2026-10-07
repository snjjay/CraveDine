// Think of it as RestaurantCard = a reusable restaurant display box.
import { useContext, useState } from "react";

import {
    Box,
    Card,
    CardActionArea,
    CardContent,
    IconButton,
    Stack,
    Typography
} from "@mui/material";

import ArrowForwardIcon from "@mui/icons-material/ArrowForward";
import BoltIcon from "@mui/icons-material/Bolt";
import FavoriteIcon from "@mui/icons-material/Favorite";
import FavoriteBorderIcon from "@mui/icons-material/FavoriteBorder";

import { Link } from "react-router-dom";

import UserFavoriteService from "../../services/UserFavoriteService";
import AuthContext from "../../features/auth/AuthContext";
import { isSessionExpiredError } from "../../features/auth/sessionEvents";
import { useNotification } from "../../features/notifications/NotificationContext";
import { getApiErrorMessage } from "../../utils/apiError";

import type { Restaurant } from "../../types/Restaurant";
import { selectCardOffers } from "../../utils/cardOffers";
import { getImageUrl } from "../../utils/imageUrl";
import RestaurantImageFallback from "./RestaurantImageFallback";

// Compact white offer card on top of the photo.
const OVERLAY_SX = {
    bgcolor: "rgba(255, 255, 255, 0.95)",
    borderRadius: "10px",
    boxShadow: "0 2px 8px rgba(28, 28, 28, 0.18)",
    px: 1.25,
    py: 0.625,
    minWidth: 0
} as const;

interface Props {
    restaurant: Restaurant; //Restaurant information,Give me the restaurant to display
    isFavorite: boolean; //Yes/No switch,Tell me whether this restaurant is a favourite
    onFavoriteChanged: () => void; //Notification button, Tell the parent that the favourite changed
}

function RestaurantCard({ //Give me these 3 things and I'll build the restaurant card.
    restaurant,
    isFavorite,
    onFavoriteChanged
}: Props) {

    // Favourites are a customer feature - the heart is only shown to
    // logged-in customers.
    const auth = useContext(AuthContext);
    const isCustomer = auth?.user?.role === "Customer";

    const { notify } = useNotification();

    // Prefer the cover photo; fall back to the logo, then to the
    // CraveDine fallback tile (also used if the image fails to load).
    const imagePath = restaurant.coverImageUrl || restaurant.logoUrl;
    const [imageFailed, setImageFailed] = useState(false);

    // Up to two offer summaries for the photo (+N more).
    const { offers, moreCount } = selectCardOffers(restaurant.dealSummaries);

    // Eligible = current Dine In/Takeaway offers (ended, sold-out and
    // Delivery deals are already excluded by selectCardOffers).
    const eligibleOfferCount = offers.length + moreCount;
    const hasMoreOffers = eligibleOfferCount > 2;

    // One compact metadata line, e.g. "Nepali, Tibetan · Kathmandu".
    // (No distance: restaurants have no location coordinates.)
    const metaText = [
        restaurant.cuisines.slice(0, 2).join(", "),
        restaurant.areaName
    ].filter(Boolean).join(" · ");

    async function toggleFavorite() { //When the user clicks toggleFavorite, is it already a fav yes, no? If yes, remove it from favs. If no, add it to favs. Then tell the parent that the fav changed.

        try {

            if (isFavorite) {

                await UserFavoriteService.remove(restaurant.id);

            }
            else {

                await UserFavoriteService.add(restaurant.id);

            }

            onFavoriteChanged(); //Hey, parent component — the favourite has changed

        }
        catch (error) {

            console.error(error);

            // An expired session is reported once globally (the user is
            // signed out and sent to /login), so no second message here.
            if (!isSessionExpiredError(error)) {

                notify(
                    getApiErrorMessage(
                        error,
                        isFavorite
                            ? "Couldn't remove this restaurant from your favourites. Please try again."
                            : "Couldn't add this restaurant to your favourites. Please try again."
                    ),
                    "error"
                );

            }

        }

    }

    return (

        <Card
            sx={{
                height: "100%",
                position: "relative",
                transition: "box-shadow 160ms ease, transform 160ms ease",
                "&:hover": { boxShadow: 3, transform: "translateY(-2px)" },
                "&:hover img": { transform: "scale(1.03)" },
                "@media (prefers-reduced-motion: reduce)": {
                    transition: "none",
                    "&:hover": { transform: "none" },
                    "&:hover img": { transform: "none" }
                }
            }}
        >

            {/* The whole card is one link to the restaurant. The heart
                button sits outside it (no nested interactive elements). */}
            <CardActionArea
                component={Link}
                to={`/restaurants/${restaurant.id}`}
                sx={{
                    height: "100%",
                    display: "flex",
                    flexDirection: "column",
                    alignItems: "stretch",
                    justifyContent: "flex-start",
                    "&.Mui-focusVisible": { outlineOffset: -3 }
                }}
            >

                <Box sx={{ position: "relative", aspectRatio: "16 / 10", overflow: "hidden", bgcolor: "action.hover" }}>

                    {imagePath && !imageFailed ? (
                        <Box
                            component="img"
                            src={getImageUrl(imagePath)}
                            alt={restaurant.name}
                            loading="lazy"
                            onError={() => setImageFailed(true)}
                            sx={{
                                width: "100%",
                                height: "100%",
                                objectFit: "cover",
                                display: "block",
                                transition: "transform 300ms ease"
                            }}
                        />
                    ) : (
                        <RestaurantImageFallback />
                    )}

                    {/* Bottom of the photo: offer overlays on the left, "View all
                        offers" bottom-right. One wrapping flex row, so they
                        never overlap: on narrow cards the button drops to its
                        own line below the overlays instead of squeezing them.
                        The heart sits top-right. */}
                    {/* data-light: the white overlays on the photo keep
                        Day colours in Night mode too (theme CSS variables). */}
                    {offers.length > 0 && (
                        <Box
                            data-light=""
                            sx={{
                                position: "absolute",
                                left: 10,
                                right: 10,
                                bottom: 10,
                                display: "flex",
                                flexWrap: "wrap",
                                alignItems: "flex-end",
                                gap: 0.75
                            }}
                        >
                            <Stack
                                spacing={0.75}
                                sx={{
                                    flex: "0 1 auto",
                                    minWidth: 0,
                                    alignItems: "flex-start"
                                }}
                            >
                                {offers.map((offer, index) => {

                                    const isLast = index === offers.length - 1;

                                    return (
                                        <Stack
                                            key={offer.id}
                                            direction="row"
                                            spacing={0.75}
                                            sx={{ alignItems: "flex-end", maxWidth: "100%" }}
                                        >
                                            <Box sx={OVERLAY_SX}>
                                                <Stack direction="row" spacing={0.375} sx={{ alignItems: "center" }}>
                                                    {offer.live && (
                                                        <BoltIcon aria-hidden sx={{ fontSize: 15, color: "deal.dark", flexShrink: 0 }} />
                                                    )}
                                                    <Typography
                                                        component="span"
                                                        sx={{ fontSize: "0.8125rem", fontWeight: 700, lineHeight: 1.3, color: "text.primary", overflowWrap: "anywhere" }}
                                                    >
                                                        {offer.headline}
                                                    </Typography>
                                                </Stack>
                                                <Typography
                                                    component="span"
                                                    sx={{ display: "block", fontSize: "0.75rem", lineHeight: 1.35, color: "text.secondary", overflowWrap: "anywhere" }}
                                                >
                                                    {offer.detail}
                                                </Typography>
                                            </Box>

                                            {isLast && moreCount > 0 && (
                                                <Box
                                                    sx={{
                                                        ...OVERLAY_SX,
                                                        flexShrink: 0,
                                                        py: 0.375,
                                                        fontSize: "0.75rem",
                                                        fontWeight: 700,
                                                        lineHeight: 1.4,
                                                        color: "text.primary",
                                                        whiteSpace: "nowrap"
                                                    }}
                                                >
                                                    +{moreCount} more
                                                </Box>
                                            )}
                                        </Stack>
                                    );

                                })}
                            </Stack>

                            {/* Only when there are more eligible offers than the
                                two shown. Visual only: the whole card is already
                                the link to /restaurants/{id} (no nested button). */}
                            {hasMoreOffers && (
                                <Box
                                    component="span"
                                    sx={{
                                        ml: "auto",
                                        flexShrink: 0,
                                        display: "inline-flex",
                                        alignItems: "center",
                                        gap: 0.25,
                                        bgcolor: "primary.main",
                                        color: "primary.contrastText",
                                        borderRadius: "8px",
                                        boxShadow: "0 2px 8px rgba(28, 28, 28, 0.18)",
                                        px: 1,
                                        py: 0.5,
                                        fontSize: "0.75rem",
                                        fontWeight: 700,
                                        lineHeight: 1.3,
                                        whiteSpace: "nowrap"
                                    }}
                                >
                                    View all offers
                                    <ArrowForwardIcon aria-hidden sx={{ fontSize: 14 }} />
                                </Box>
                            )}
                        </Box>
                    )}

                </Box>

                <CardContent sx={{ flexGrow: 1, display: "flex", flexDirection: "column", gap: 0.5 }}>

                    <Stack direction="row" spacing={1} sx={{ alignItems: "center", minWidth: 0 }}>

                        <Typography variant="h6" component="h3" noWrap title={restaurant.name} sx={{ minWidth: 0 }}>
                            {restaurant.name}
                        </Typography>

                        {/* Subtle marker for synthetic demo listings */}
                        {restaurant.isDemo && (
                            <Box
                                component="span"
                                title="Demo listing: sample data for testing"
                                sx={{
                                    flexShrink: 0,
                                    px: 0.75,
                                    py: 0.125,
                                    borderRadius: "6px",
                                    border: "1px solid",
                                    borderColor: "divider",
                                    color: "text.secondary",
                                    fontSize: "0.6875rem",
                                    fontWeight: 600,
                                    lineHeight: 1.4,
                                    letterSpacing: "0.02em"
                                }}
                            >
                                Demo
                            </Box>
                        )}

                    </Stack>

                    {metaText && (
                        <Typography variant="body2" color="text.secondary" noWrap title={metaText}>
                            {metaText}
                        </Typography>
                    )}

                </CardContent>

            </CardActionArea>

            {isCustomer && (
                <IconButton
                    data-light=""
                    onClick={toggleFavorite}
                    aria-label={isFavorite
                        ? `Remove ${restaurant.name} from favourites`
                        : `Add ${restaurant.name} to favourites`}
                    aria-pressed={isFavorite}
                    sx={{
                        position: "absolute",
                        top: 10,
                        right: 10,
                        width: 40,
                        height: 40,
                        borderRadius: "50%",
                        bgcolor: "rgba(255, 255, 255, 0.94)",
                        color: isFavorite ? "deal.dark" : "text.primary",
                        boxShadow: 1,
                        "&:hover": { bgcolor: "#FFFFFF" }
                    }}
                >
                    {isFavorite
                        ? <FavoriteIcon fontSize="small" />
                        : <FavoriteBorderIcon fontSize="small" />}
                </IconButton>
            )}

        </Card>

    );

}

export default RestaurantCard;


// ==========================================================
// COMPONENT — RestaurantCard.tsx
// ==========================================================
//
// RestaurantCard = reusable restaurant display box.
//
// A parent gives the component:
//
// restaurant
// → 📋 Restaurant information to display.
//
// isFavorite
// → ❤️ Yes/No value telling whether it is a favourite.
//
// onFavoriteChanged
// → 🔔 Tells the parent that the favourite changed.
//
// ----------------------------------------------------------
//
// DATA COMES INTO THE COMPONENT:
//
// Parent Page
//      ↓
// Props
//      ↓
// RestaurantCard
//
// ----------------------------------------------------------
//
// RestaurantCard displays:
//
// 🖼️ Restaurant image
// 🏷️ Deal overlays on the image (up to 2, then "+N more")
// 🔘 "View all offers →" on the image (only with more than 2 offers)
// ❤️ Favourite button (customers only)
// 🏪 Restaurant name
// 📍 Cuisine · Area (one line)
//
// ----------------------------------------------------------
//
// FAVOURITE FLOW:
//
// User clicks ❤️
//      ↓
// toggleFavorite()
//      ↓
// Is it already a favourite?
//      ↓
// YES → UserFavoriteService.remove()
// NO  → UserFavoriteService.add()
//      ↓
// axios
//      ↓
// .NET API
//
// After it finishes:
//
// onFavoriteChanged()
//      ↓
// Tell the parent:
// "The favourite changed."
//
// ----------------------------------------------------------
//
// OPENING THE RESTAURANT:
//
// Click anywhere on the card (or "View all offers →")
//      ↓
// /restaurants/{id}
//      ↓
// AppRoutes
//      ↓
// RestaurantDetailsPage
//
// ----------------------------------------------------------
//
// 🔑 REMEMBER:
//
// Component = reusable piece of UI.
//
// Props = data/functions given to the component.
//
// Component can:
// → display the data
// → respond to user actions
// → call a Service
// → notify its parent
//
// ==========================================================


// ==========================================================
// EATKATH FRONTEND FLOW
// ==========================================================
//
// 1. index.html              ✅
// 2. main.tsx                ✅
// 3. App.tsx                 ✅
// 4. AppRoutes.tsx           ✅
// 5. MainLayout.tsx          ✅
// 6. Page                    ✅ MyFavoritesPage
//       ↓
// 7. Service                 ✅ UserFavoriteService
//       ↓
// 8. axios.ts                ✅
//       ↓
// 9. .NET API                ⏭️ SKIP
//       ↓
// 10. Response               ✅
//       ↓
// 11. State update           ✅
//       ↓
// 12. React re-render        ✅
//       ↓
// 13. UI update              ✅
//       ↓
// 14. Components             ✅ RestaurantCard
//       ↓
// 15. Hooks / Context        ← NEXT  No files in Hooks
//
// ==========================================================
//Yes. Skip hooks/ — there are no files there, so there's nothing to study.

//The next useful area is features/auth/, because these files are actually used in your application: Next is SuthContext.tsx, which is the authentication context that provides the current user and logout function to the rest of the app.
