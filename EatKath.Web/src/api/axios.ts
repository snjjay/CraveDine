import axios from "axios"; //Get a phone >Axios = the phone/connection that actually sends the request to your .NET API

import { markSessionExpiredError, signalSessionExpired } from "../features/auth/sessionEvents";

const api = axios.create({ //Set up your EatKath phone, Create your own configured Axios instance

    baseURL: import.meta.env.VITE_API_URL, //Tells Axios where the .NET API lives

    timeout: 30_000, //Give up after 30s (rejects with code ECONNABORTED) so pages show their error state instead of loading forever

    headers: {
        "Content-Type": "application/json" //Tell API what's inside the package, Says we're sending JSON
    }

});

api.interceptors.request.use( //Security check before sending, Runs before every API request

    (config) => {

        const storedUser = localStorage.getItem("user"); //Look for saved login details, Gets the logged-in user's information

        if (storedUser) {

            const user = JSON.parse(storedUser); //User's access ticket , JWT token

            if (user.token) {

                config.headers.Authorization = `Bearer ${user.token}`; //Attach ticket to request, Sends JWT to the .NET API

            }

        }

        return config;  //"Okay, send it" Allows the request to continue

    },

    (error) => Promise.reject(error)

);

// Token of the signed-in user saved in localStorage (or null).
function getStoredToken(): string | null {

    try {

        const storedUser = localStorage.getItem("user");

        return storedUser ? (JSON.parse(storedUser).token ?? null) : null;

    }
    catch {

        return null;

    }

}

// Expired/invalid session: an authenticated request answered with 401.
// Only when the request carried the token that is still saved (so a late
// 401 for an old token, or several 401s at once, signal only once - the
// first one clears the saved session), and not for the login/register
// calls, whose 401 means wrong credentials (handled by LoginPage).
// AuthProvider reacts to the signal: signs out, shows the message once
// and redirects to /login. The error is still rejected (marked) so
// callers can finish; they skip their own message for it.
api.interceptors.response.use(

    (response) => response,

    (error) => {

        const status = error.response?.status;
        const url = String(error.config?.url ?? "");
        const sentAuthorization = String(error.config?.headers?.Authorization ?? "");
        const storedToken = getStoredToken();

        if (
            status === 401 &&
            !/\/auth\/(login|register)$/i.test(url) &&
            storedToken !== null &&
            sentAuthorization === `Bearer ${storedToken}`
        ) {

            markSessionExpiredError(error);

            signalSessionExpired();

        }

        return Promise.reject(error);

    }

);

export default api; //Give the configured phone to other files, Services can use api.get(), api.post(), etc.


// 1. index.html              ✅
// 2. main.tsx                ✅
// 3. App.tsx                 ✅
// 4. AppRoutes.tsx           ✅
// 5. MainLayout.tsx          ✅
// 6. Page                    ✅ MyFavoritesPage
//      ↓
//7. Service                 ✅ UserFavoriteService
//     ↓
//8. axios.ts                ✅
//     ↓
//9. .NET API                ← NEXT



// ==========================================================
// EATKATH FRONTEND FLOW — RESPONSE SIDE
// ==========================================================
//
// We already studied the REQUEST going to the API:
//
// MyFavoritesPage
//      ↓
// UserFavoriteService
//      ↓
// axios.ts
//      ↓
// .NET API
//      ↓
// Database
//
// We will SKIP studying the .NET API again because
// it has already been covered in the backend project.
//
// ----------------------------------------------------------
//
// Now we follow what happens AFTER the API sends
// the response back:
//
// Database
//      ↓
// .NET API
//      ↓
// 10. axios.ts
//      ↓
// 11. UserFavoriteService
//      ↓
// 12. MyFavoritesPage
//      ↓
// 13. setState
//      ↓
// 14. React re-renders
//      ↓
// 👤 User sees updated UI
//
// ----------------------------------------------------------
//
// OUR FRONTEND LEARNING SEQUENCE:
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
// ───────── RESPONSE COMES BACK ─────────
//       ↓
// 10. axios.ts               ← receives response
//       ↓
// 11. Service                ← returns response data
//       ↓
// 12. Page                   ← receives the data
//       ↓
// 13. setState               ← updates React state
//       ↓
// 14. Component re-renders   ← React updates the UI
//
// ----------------------------------------------------------
//
// EXAMPLE FROM MyFavoritesPage:
//
// const data =
//     await UserFavoriteService.getMyFavorites();
//
// → Wait for the API response.
// → Service gives the returned data to the page.
//
// setFavorites(data);
//
// → Put the returned data into React state.
// → React notices the state changed.
// → React re-renders the page.
// → Favourite restaurants appear on screen.
//
// ==========================================================
