import { useContext } from "react";
import { Link, Outlet } from "react-router-dom";

import {
    AppBar,
    Box,
    Button,
    Container,
    Toolbar,
    Typography
} from "@mui/material";

import AuthContext from "../features/auth/AuthContext";

function MainLayout() {

    const auth = useContext(AuthContext); //What is the current user's login information?

    if (!auth)
        throw new Error("AuthContext not found.");

    const { user, logout } = auth; //user → current logged-in user || logout → function to log the user out

    const isCustomer = user?.role === "Customer";  // Is the user a customer?
    const isOwner = user?.role === "Owner";        // Is the user an owner?
    const isAdmin = user?.role === "Admin";        // Is the user an admin?

    return (

        <Box>

            <AppBar position="static">

                <Toolbar>  {/* creates the top navigation area using Material UI. */}

                    <Typography
                        variant="h6"
                        sx={{ flexGrow: 1 }}
                    >
                        EatKath
                    </Typography>

                    <Button
                        color="inherit"
                        component={Link}
                        to="/"
                    >
                        Home
                    </Button>

                    <Button
                        color="inherit"
                        component={Link}
                        to="/restaurants"  //Clicking the button goes to /restaurants.
                    >
                        Restaurants
                    </Button>

                    {isCustomer && (

                        <>
                            <Button
                                color="inherit"
                                component={Link}
                                to="/favorites"
                            >
                                ❤️ Favourites
                            </Button>

                            <Button
                                color="inherit"
                                component={Link}
                                to="/my-reservations"
                            >
                                📅 My Reservations
                            </Button>
                        </>

                    )}

                    {isOwner && (

                        <Button
                            color="inherit"
                            component={Link}
                            to="/owner"
                        >
                            🏪 Owner Dashboard
                        </Button>

                    )}

                    {isAdmin && (

                        <Button
                            color="inherit"
                            component={Link}
                            to="/admin"
                        >
                            ⚙️ Admin Dashboard
                        </Button>

                    )}

                    {user ? (

                        <Button
                            color="inherit"
                            onClick={logout}
                        >
                            Logout
                        </Button>

                    ) : (

                        <Button
                            color="inherit"
                            component={Link}
                            to="/login"
                        >
                            Login
                        </Button>

                    )}

                </Toolbar>

            </AppBar>

            <Container sx={{ mt: 4 }}>

                <Outlet /> {/*is basically the placeholder where the selected page gets inserted. MainLayout provides the common page structure. <Outlet /> is where the current route's page appears.*/}

            </Container>

        </Box>

    );

}

export default MainLayout;

// ==========================================================
// FRONTEND FLOW — STEP 5
// ==========================================================
//
// MainLayout.tsx = COMMON PAGE STRUCTURE.
//
// Provides things that appear around many pages,
// such as the navigation bar.
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
// 5. MainLayout.tsx  ← HERE
//      ↓
// 6. Page
//
// ----------------------------------------------------------
//
// AuthContext
// → Gets the current user and logout function.
//
// user
// → Current logged-in user.
//
// Role checks:
//
// isCustomer → show Customer navigation
// isOwner    → show Owner navigation
// isAdmin    → show Admin navigation
//
// ----------------------------------------------------------
//
// Navigation:
//
// Link + to="/restaurants"
// → Clicking the button goes to /restaurants.
//
// AppRoutes then chooses RestaurantsPage.
//
// ----------------------------------------------------------
//
// <Outlet />
// → PLACEHOLDER where the current route's page appears.
//
// Example:
//
// /restaurants
//      ↓
// MainLayout
//      ↓
// <Outlet />
//      ↓
// RestaurantsPage
//
// /
//      ↓
// MainLayout
//      ↓
// <Outlet />
//      ↓
// HomePage
//
// 🔑 Remember:
//
// MainLayout = common structure/navigation
// Outlet     = place where the selected page appears
//
// ==========================================================