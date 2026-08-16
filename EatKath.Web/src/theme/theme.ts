import { createTheme } from "@mui/material/styles";
import { colors } from "./colors";

export const theme = createTheme({
    palette: {
        primary: {
            main: colors.primary
        },
        secondary: {
            main: colors.secondary
        },
        background: {
            default: colors.background
        }
    },

    typography: {
        fontFamily: "Roboto, Arial, sans-serif",

        h4: {
            fontWeight: 700
        },

        h5: {
            fontWeight: 600
        },

        button: {
            textTransform: "none"
        }
    }
});

// ==========================================================
// THEME — theme.ts
// ==========================================================
//
// theme.ts = 🎨 Global visual/style rulebook.
//
// Uses Material UI (MUI).
//
// It controls the overall appearance of the application.
//
// ----------------------------------------------------------
//
// createTheme()
// → Creates the central MUI theme.
//
// ----------------------------------------------------------
//
// palette
// → 🎨 Defines application colours.
//
// primary
// → Main application colour.
//
// secondary
// → Secondary/supporting colour.
//
// background
// → Default page background colour.
//
// Colours come from:
//
// colors.primary
// colors.secondary
// colors.background
//
// ----------------------------------------------------------
//
// typography
// → ✍️ Defines text rules.
//
// fontFamily
// → Sets the default application font.
//
// h4 / h5
// → Controls heading styling.
//
// button
// → Controls button text styling.
//
// textTransform: "none"
// → Keeps button text as written instead of
//   automatically changing it to uppercase.
//
// ----------------------------------------------------------
//
// HOW IT FITS:
//
// main.tsx
//      ↓
// ThemeProvider
//      ↓
// theme.ts
//      ↓
// Global MUI styling
//      ↓
// MUI components throughout the application
//
// 🔑 REMEMBER:
//
// theme.ts = "How should my application look?"
//
// It does NOT:
// → call APIs
// → handle authentication
// → manage business logic
// → access the database
//
// ==========================================================

//Skip colors , next The next useful area is layouts, but we've already studied MainLayout.tsx. So we should not repeat it.

// ==========================================================
// NEXT FRONTEND STUDY ORDER
//
// Already covered:
//
// index.html              ✅
// main.tsx                ✅
// App.tsx                 ✅
// AppRoutes.tsx           ✅
// MainLayout.tsx          ✅
// MyFavoritesPage.tsx     ✅
// UserFavoriteService.ts  ✅
// axios.ts                ✅
// Components              ✅
// Auth                    ✅
// Types                   ⏭️ SKIP remaining
// Utils                   ✅
// theme.ts                ✅
// colors.ts               ⏭️ SKIP
//
// ----------------------------------------------------------
//
// NEXT:
//
// Restaurant-related Pages
//       ↓
// RestaurantDetailsPage.tsx
//       ↓
// Restaurant-related Services
//       ↓
// Deal-related Pages
//       ↓
// Deal Services
//       ↓
// Reservation Pages / Services
//
// ==========================================================
//I'd recommend this approach
//MyFavoritesPage          ✅ Study in detail
//RestaurantDetailsPage    ⏭️ Quick scan only
//RestaurantsPage          ⏭️ Quick scan only
//HomePage                 ⏭️ Already simple
//Other CRUD pages         ⏭️ Don't study every one


// ==========================================================
// FRONTEND STUDY — WHAT WE HAVE LEARNED
// ==========================================================
//
// We do NOT need to study every Page or Service line-by-line.
// Most of them repeat the same pattern.
//
// ----------------------------------------------------------
//
// PAGES
//
// MyFavoritesPage.tsx
// → Studied in detail.
//
// Important pattern:
//
// Page
//   ↓
// useState()
//   ↓
// useEffect()
//   ↓
// Service
//   ↓
// API
//   ↓
// Response
//   ↓
// setState()
//   ↓
// React re-renders
//   ↓
// UI updates
//
// Other Pages
// → Usually follow the same pattern.
// → Quick scan is enough unless they introduce something NEW.
//
// ----------------------------------------------------------
//
// SERVICES
//
// UserFavoriteService.ts
// → Studied in detail.
//
// AuthService.ts
// → Studied in detail.
//
// Most other services repeat:
//
// Service
//   ↓
// api.get()
// api.post()
// api.put()
// api.delete()
//   ↓
// response.data
//
// Therefore:
// → Do NOT study every Service line-by-line.
//
// Only study a Service in detail if it introduces something NEW.
//
// ----------------------------------------------------------
//
// FRONTEND ARCHITECTURE WE HAVE NOW UNDERSTOOD:
//
// index.html
// → Starting HTML page.
//
// main.tsx
// → Starts React application.
//
// App.tsx
// → Root React component.
//
// AppRoutes.tsx
// → Decides which Page to show for each URL.
//
// MainLayout.tsx
// → Common navigation/layout.
// → <Outlet /> displays the selected Page.
//
// Page
// → Screen shown to the user.
//
// Component
// → Reusable piece of UI.
//
// Service
// → Handles communication with the backend API.
//
// axios.ts
// → Actually sends HTTP requests.
//
// Types
// → Describe the shape of data.
//
// Utils
// → Small reusable helper functions.
//
// AuthContext
// → Defines shared authentication information.
//
// AuthProvider
// → Manages user/login/logout state.
//
// ProtectedRoute
// → Checks whether user can access a protected Page.
//
// LoginPage
// → Collects login details and starts authentication.
//
// AuthService
// → Sends login request to the API.
//
// theme.ts
// → Global Material UI styling.
//
// ----------------------------------------------------------
//
// IMPORTANT:
//
// We have already learned the individual building blocks.
//
// We should now stop reading every similar file.
//
// The better next step is:
//
// TRACE ONE COMPLETE REAL FEATURE END-TO-END.
//
// Recommended example:
//
// Create Deal
//      ↓
// CreateDealPage
//      ↓
// Deal Component / Form
//      ↓
// DealService
//      ↓
// axios
//      ↓
// .NET API
//      ↓
// response
//      ↓
// React state
//      ↓
// UI update
//
// This will show how all the pieces we learned
// work TOGETHER.
//
// ----------------------------------------------------------
//
// 🔑 FINAL REMINDER:
//
// Page       = screen
// Component  = reusable UI
// Service    = API communication
// axios      = HTTP connection
// Type       = data blueprint
// Context    = shared information
// Provider   = manages/provides shared state
// Utils      = helper
// Theme      = visual styling
//
// ==========================================================