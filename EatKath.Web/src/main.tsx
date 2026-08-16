import { StrictMode } from "react";
import { createRoot } from "react-dom/client";

import { CssBaseline, ThemeProvider } from "@mui/material";

import App from "./App";
import { theme } from "./theme/theme";

import AuthProvider from "./features/auth/AuthProvider";

createRoot(document.getElementById("root")!).render( //Find <div id='root'> from index.html and put my React application inside it
    <StrictMode>
        <ThemeProvider theme={theme}>
            <CssBaseline />

            <AuthProvider>
                <App /> {/* Start App, but give it access to authentication information */}
            </AuthProvider>

        </ThemeProvider>
    </StrictMode>
);


// ==========================================================
// FRONTEND FLOW — STEP 2
// ==========================================================
//
// main.tsx = STARTS THE REACT APPLICATION.
//
// index.html gave us:
//
// <div id="root"></div>
//
// main.tsx finds that empty box and puts React inside it.
//
// ----------------------------------------------------------
//
// IMPORTANT:
//
// createRoot(document.getElementById("root")!).render(...)
//
// Think:
// "Find the root box from index.html
//  and put my React application inside it."
//
// ----------------------------------------------------------
//
// PROVIDERS / WRAPPERS:
//
// <ThemeProvider>
//     → Gives the app the EatKath/MUI theme.
//
// <CssBaseline />
//     → Provides basic consistent CSS.
//
// <AuthProvider>
//     → Makes authentication/user information available
//       to the application.
//
// <App />
//     → Starts the main EatKath application.
//
// ----------------------------------------------------------
//
// FLOW:
//
// 🌐 Browser
//      ↓
// 1. index.html
//      ↓
// 2. main.tsx
//      ↓
// StrictMode
//      ↓
// ThemeProvider
//      ↓
// AuthProvider
//      ↓
// 3. App.tsx
//
// 🔑 Remember:
//
// index.html = Entry door
// main.tsx   = Starts React + sets up providers
// App.tsx    = Main application
//
// ==========================================================