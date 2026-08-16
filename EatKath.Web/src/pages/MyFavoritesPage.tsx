import { useEffect, useState } from "react"; //useState → stores data that can change. useEffect → runs code when the page loads.

import {
    CircularProgress,
    Grid,
    Typography
} from "@mui/material";

import UserFavoriteService from "../services/UserFavoriteService";//Bring the UserFavoriteService code from the services folder so I can use it in this page.

import type { UserFavorite } from "../types/UserFavorite"; //Bring me the description/shape of what a UserFavorite looks like..

function MyFavoritesPage() {

    //Create a box called favorites where I will store the restaurants returned by the API.
    const [favorites, setFavorites] = useState<UserFavorite[]>([]);

        // Initially:
        //
        // favorites = []
        //
        // → Empty box.
        //
        // setFavorites()
        // → Function used to put data into the favorites box.
        //
        // Example:
        //
        // API returns 3 restaurants
        //        ↓
        // setFavorites(data)
        //        ↓
        // favorites now contains 3 restaurants

    const [loading, setLoading] = useState(true); //This is another little boxI'm currently waiting for the API

    useEffect(() => { //load when page opens

        loadFavorites();

    }, []); //The empty means Run this when the page loads.

    async function loadFavorites() { //Call the Service

        try {

            const data =
                await UserFavoriteService.getMyFavorites(); //Service, please get my favourite restaurants

            setFavorites(data);// Put the restaurants returned by the API into the favorites box. Put API data into React state

        }
        catch (error) {

            console.error(error);

        }
        finally {

            setLoading(false); //Whether the API succeeds or fails, loading is finished.

        }

    }

    if (loading)
        return <CircularProgress />; //If the API is still loading, show a spinning circle to indicate that the page is loading.

    return (

        <>

            <Typography
                variant="h4"
                sx={{ mb: 3 }}
            >
                My Favourite Restaurants
            </Typography>

            <Grid container spacing={3}>

                {favorites.map(f => (   //For every favourite restaurant in my box, create some UI.

                    <Grid
                        key={f.restaurantId}
                        size={{ xs: 12, sm: 6, md: 4 }}
                    >

                        <img
                            src={`https://localhost:7203${f.logoUrl}`}
                            alt={f.restaurantName}
                            style={{
                                width: "100%",
                                height: 180,
                                objectFit: "cover",
                                borderRadius: 8
                            }}
                        />

                        <Typography
                            variant="h6"
                            sx={{ mt: 1 }}
                        >
                            {f.restaurantName}
                        </Typography>

                    </Grid>

                ))}

            </Grid>

        </>

    );

}

export default MyFavoritesPage;


// ================================================================================================================
// MyFavoritesPage.tsx — SIMPLE UNDERSTANDING
// ================================================================================================================
//
// | Code                                      | 🏠 Easy analogy                         | What it does                                      |
// |-------------------------------------------|------------------------------------------|---------------------------------------------------|
// | useState<UserFavorite[]>([])              | 📦 Empty box for favourite restaurants   | Creates a box to store a LIST of UserFavorite data |
// | favorites                                 | 📦 What's currently inside the box       | Holds the current favourite restaurants           |
// | setFavorites(data)                        | 📥 Put data into the box                 | Updates favorites with API results                 |
// | useState(true)                            | ⏳ Waiting switch = ON                   | Creates loading state; starts as true              |
// | loading                                   | 🔘 Current position of waiting switch    | Tells us whether data is still loading             |
// | setLoading(false)                         | 🔘 Turn waiting switch OFF               | Says loading has finished                          |
// | useEffect()                               | 🔔 "When page opens, do this"            | Starts loading the favourites                      |
// | UserFavoriteService                      | 📞 Person who gets the information       | Gets favourite restaurants from the API            |
// | API                                       | 🏢 Place that has the information        | Finds the user's favourite restaurants             |
// | favorites.map(...)                        | 🔄 Go through each restaurant             | Creates UI for every restaurant                    |
// | <img> + <Typography>                      | 🖼️ Show the information                  | Shows restaurant image and name                     |
//
// ---------------------------------------------------------------------------------------------------------------
//
// IMPORTANT:
//
// useState<UserFavorite[]>([])
//
// → Creates a box called "favorites".
// → The box starts empty: []
// → It is expected to contain a LIST of UserFavorite objects.
//
// Example:
//
// favorites = []
//        ↓
// API returns restaurants
//        ↓
// setFavorites(data)
//        ↓
// favorites = [Restaurant A, Restaurant B, Restaurant C]
//
//
//
// useState(true)
//
// → Creates a "loading" value.
// → It starts as true because we are waiting for the API.
//
// loading = true
//        ↓
// ⏳ Show loading spinner
//        ↓
// API finishes
//        ↓
// setLoading(false)
//        ↓
// loading = false
//        ↓
// Show the actual restaurants
//
// ---------------------------------------------------------------------------------------------------------------
//
// COMPLETE FLOW:
//
// 👤 User opens My Favourites
//          ↓
// 📦 favorites = []                         → Empty box
//          ↓
// 🔘 loading = true                        → Still waiting
//          ↓
// 🔔 useEffect()                           → Start getting favourites
//          ↓
// 📞 UserFavoriteService                   → Ask for the data
//          ↓
// 🌐 API
//          ↓
// 🗄️ Database
//          ↓
// 🍕 Restaurant A
// 🍔 Restaurant B
// 🍜 Restaurant C
//          ↓
// 📥 setFavorites(data)                     → Put them into the box
//          ↓
// 🔘 setLoading(false)                      → Stop waiting
//          ↓
// 🔄 favorites.map(...)                     → Go through each restaurant
//          ↓
// 🖼️ Restaurant A   🖼️ Restaurant B   🖼️ Restaurant C
//          ↓
// 👤 User sees the favourites
//
// ---------------------------------------------------------------------------------------------------------------
//
// 🔑 REMEMBER:
//
// useState()   = "Keep information in a box."
// useEffect()  = "When the page opens, get the data."
// Service      = "Get the data."
// map()        = "Display each item."
//
// ================================================================================================================

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
// 7. Service                 ← NEXT
//       ↓
// 8. axios.ts
//       ↓
// 9. .NET API
//
// ==========================================================


// ==========================================================
// NEXT — STUDY THE RESPONSE COMING BACK FROM THE API
// ==========================================================
//
// We do NOT need to study MyFavoritesPage again.
//
// We only need to understand how the response travels back:
//
// .NET API
//     ↓
// axios.ts
//     ↓
// UserFavoriteService
//     ↓
// MyFavoritesPage
//     ↓
// setFavorites(data)
//     ↓
// React updates the UI
//
// ----------------------------------------------------------
//
// In UserFavoriteService.ts, focus on these lines:
//
// const response =
//     await api.get<UserFavorite[]>("/userfavorite");
//
// → axios receives the response from the API.
//
//
//
// return response.data;
//
// → Take the data from the response
//   and return it to MyFavoritesPage.
//
// ----------------------------------------------------------
//
// Then go back to MyFavoritesPage:
//
// const data =
//     await UserFavoriteService.getMyFavorites();
//
// → Page receives the data returned by the Service.
//
//
//
// setFavorites(data);
//
// → Put the returned data into React state.
//
//
//
// React sees the state changed
//     ↓
// Component re-renders
//     ↓
// favorites.map(...)
//     ↓
// Favourite restaurants appear on screen.
//
// ==========================================================
//
// NEXT FILE TO STUDY:
//
// UserFavoriteService.ts
//
// Focus ONLY on:
//     api.get()
//     response
//     response.data
//     return response.data
//
// ==========================================================


// ==========================================================
// NEXT — STUDY THE RESPONSE COMING BACK FROM THE API
// ==========================================================
//
// We do NOT need to study MyFavoritesPage again.
//
// We only need to understand how the response travels back:
//
// .NET API
//     ↓
// axios.ts
//     ↓
// UserFavoriteService
//     ↓
// MyFavoritesPage
//     ↓
// setFavorites(data)
//     ↓
// React updates the UI
//
// ----------------------------------------------------------
//
// In UserFavoriteService.ts, focus on these lines:
//
// const response =
//     await api.get<UserFavorite[]>("/userfavorite");
//
// → axios receives the response from the API.
//
//
//
// return response.data;
//
// → Take the data from the response
//   and return it to MyFavoritesPage.
//
// ----------------------------------------------------------
//
// Then go back to MyFavoritesPage:
//
// const data =
//     await UserFavoriteService.getMyFavorites();
//
// → Page receives the data returned by the Service.
//
//
//
// setFavorites(data);
//
// → Put the returned data into React state.
//
//
//
// React sees the state changed
//     ↓
// Component re-renders
//     ↓
// favorites.map(...)
//     ↓
// Favourite restaurants appear on screen.
//
// ==========================================================
//
// NEXT FILE TO STUDY:
//
// UserFavoriteService.ts
//
// Focus ONLY on:
//     api.get()
//     response
//     response.data
//     return response.data
//
// ==========================================================

// ==========================================================
// RESPONSE SIDE — WHAT HAPPENS AFTER THE API RESPONDS
// ==========================================================
//
// .NET API
//     ↓
// axios.ts
//     ↓
// UserFavoriteService
//     ↓
// MyFavoritesPage
//
// ----------------------------------------------------------
//
// const data =
//     await UserFavoriteService.getMyFavorites();
//
// → Wait for the Service to return the API data.
//
//
// setFavorites(data);
//
// → Put the returned favourite restaurants into
//   the favorites state box.
//
// Example:
//
// API returns:
// [Restaurant A, Restaurant B, Restaurant C]
//          ↓
// data
//          ↓
// setFavorites(data)
//          ↓
// favorites = [Restaurant A, Restaurant B, Restaurant C]
//
// ----------------------------------------------------------
//
// After setFavorites(data):
//
// React sees that the state changed
//          ↓
// React re-renders the page
//          ↓
// favorites.map(...)
//          ↓
// Restaurant A, B and C are displayed
//
// 🔑 Remember:
//
// Service     → returns the data
// setFavorites → stores the data
// React        → updates the screen
//
// ==========================================================


// ==========================================================
// STEP 13 — REACT STATE UPDATE / RE-RENDER
// ==========================================================
//
// setFavorites(data)
//       ↓
// React updates the "favorites" state
//       ↓
// React re-renders MyFavoritesPage
//       ↓
// favorites now contains the API data
//       ↓
// favorites.map(...)
//       ↓
// React creates the restaurant UI
//       ↓
// 👤 User sees the favourite restaurants
//
// ----------------------------------------------------------
//
// Simple analogy:
//
// 📦 favorites = box containing restaurant data
//
// setFavorites(data)
// → Put the new restaurants into the box.
//
// React notices the box changed
// → Rebuilds the part of the screen that uses the box.
//
// ----------------------------------------------------------
//
// 🔑 Remember:
//
// setFavorites() → changes the state
// Re-render      → React updates the screen
//
// ==========================================================

// ==========================================================
// STEP 14 — UI UPDATES
// ==========================================================
//
// favorites.map(...)
// → React goes through the favourite restaurants
//   and creates the UI for each one.
//
// Example:
//
// favorites
//     ↓
// Restaurant A
// Restaurant B
// Restaurant C
//     ↓
// favorites.map(...)
//     ↓
// 🖼️ Restaurant A
// 🖼️ Restaurant B
// 🖼️ Restaurant C
//
// ----------------------------------------------------------
//
// 🔑 Remember:
//
// API data → state → React re-render → UI
//
// ==========================================================


// ==========================================================
// EATKATH FRONTEND — CURRENT LEARNING PROGRESS
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
// 9. .NET API                ⏭️ SKIP — already studied
//       ↓
// 10. Response               ✅
//       ↓
// 11. State update           ✅ setFavorites(data)
//       ↓
// 12. React re-render        ✅
//       ↓
// 13. UI update              ✅
//       ↓
// 14. Components             ← NEXT
//
// ----------------------------------------------------------
//
// We have now completed one complete example:
//
// REQUEST:
//
// Page → Service → axios → .NET API
//
// RESPONSE:
//
// .NET API → axios → Service → Page
//                          ↓
//                    setFavorites()
//                          ↓
//                    React re-renders
//                          ↓
//                       UI updates
//
// ----------------------------------------------------------
//
// NEXT: 
//
// Study the Components folder. 
//
// Components = reusable pieces of the UI.  Next RestaurantCard.tsx — Component
//
// ==========================================================