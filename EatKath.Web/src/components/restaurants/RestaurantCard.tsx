// Think of it as RestaurantCard = a reusable restaurant display box.
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
import { getImageUrl } from "../../utils/imageUrl";

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

    const imageUrl = getImageUrl(restaurant.logoUrl);

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
// 🏪 Restaurant name
// 📍 Area
// 📝 Description
// 🏷️ Deals
// ❤️ Favourite button
// 🔘 View Details
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
// VIEW DETAILS:
//
// View Details
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