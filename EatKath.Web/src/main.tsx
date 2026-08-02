import { StrictMode } from "react";
import { createRoot } from "react-dom/client";

import { CssBaseline, ThemeProvider } from "@mui/material";

import App from "./App";
import { theme } from "./theme/theme";

import AuthProvider from "./features/auth/AuthProvider";

createRoot(document.getElementById("root")!).render(
    <StrictMode>
        <ThemeProvider theme={theme}>
            <CssBaseline />

            <AuthProvider>
                <App />
            </AuthProvider>

        </ThemeProvider>
    </StrictMode>
);