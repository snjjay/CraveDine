import { Typography } from "@mui/material";

function HomePage() {
    return (
        <Typography variant="h4">
            Home Page
        </Typography>
    );
}

export default HomePage;

// ==========================================================
// FRONTEND FLOW — STEP 6
// ==========================================================
//
// HomePage.tsx = A PAGE / SCREEN.
//
// It is the page shown when the user goes to:
//
// /
// ↓
// HomePage
//
// ----------------------------------------------------------
//
// <Typography>
// → Material UI component used to display styled text.
//
// variant="h4"
// → Controls the heading style/size.
//
// ----------------------------------------------------------
//
// CURRENT HOME PAGE IS VERY SIMPLE:
//
// Page
//  ↓
// Display "Home Page"
//
// It currently does NOT call:
// → API
// → Service
// → Hook
//
// ----------------------------------------------------------
//
// FLOW:
//
// 1. index.html
//      ↓
// 2. main.tsx
//      ↓
// 3. App.tsx
//      ↓
// 4. AppRoutes.tsx
//      ↓
// 5. MainLayout.tsx
//      ↓
// 6. HomePage.tsx
//
// 🔑 Remember:
//
// Page = an actual screen the user sees.
//
// ==========================================================

//Look at next 
//MyFavoritesPage.tsx       ← introduces protected user data
//CreateDealPage.tsx         ← forms + API