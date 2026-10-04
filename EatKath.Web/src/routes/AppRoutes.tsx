import { BrowserRouter, Route, Routes } from "react-router-dom";

import MainLayout from "../layouts/MainLayout";

// HomePage (pages/HomePage.tsx) is kept but not routed: "/" shows the
// restaurant list. To restore it, import it and use it for "/".
import RestaurantsPage from "../pages/RestaurantsPage";
import RestaurantDetailsPage from "../pages/RestaurantDetailsPage";
import MyFavoritesPage from "../pages/MyFavoritesPage";

import OwnerDashboardPage from "../pages/OwnerDashboardPage";
import OwnerDealsPage from "../pages/OwnerDealsPage";
import CreateDealPage from "../pages/CreateDealPage";
import EditDealPage from "../pages/EditDealPage";
import OwnerRestaurantPage from "../pages/OwnerRestaurantPage";
import OwnerCreateRestaurantPage from "../pages/OwnerCreateRestaurantPage";

import AdminDashboardPage from "../pages/AdminDashboardPage";
import AdminUsersPage from "../pages/AdminUsersPage";

import LoginPage from "../features/auth/LoginPage";
import RegisterPage from "../features/auth/RegisterPage";
import ProtectedRoute from "../features/auth/ProtectedRoute";
import MyReservationsPage from "../pages/MyReservationsPage";
import MyRedemptionsPage from "../pages/MyRedemptionsPage";
import ProfilePage from "../pages/ProfilePage";
import OwnerOpeningHoursPage from "../pages/OwnerOpeningHoursPage";
import OwnerMenuCategoriesPage from "../pages/OwnerMenuCategoriesPage";
import OwnerMenuItemsPage from "../pages/OwnerMenuItemsPage";

function AppRoutes() {

    return (

        <BrowserRouter>

            <Routes>

                <Route element={<MainLayout />}>

                    <Route
                        path="/"
                        element={<RestaurantsPage />}
                    />

                    <Route
                        path="/restaurants"
                        element={<RestaurantsPage />}
                    />

                    <Route
                        path="/restaurants/:id"
                        element={<RestaurantDetailsPage />}
                    />

                    <Route
                        path="/favorites"
                        element={
                            <ProtectedRoute role="Customer">
                                <MyFavoritesPage />
                            </ProtectedRoute>
                        }
                    />

                    <Route
                        path="/my-reservations"
                        element={
                            <ProtectedRoute role="Customer">
                                <MyReservationsPage />
                            </ProtectedRoute>
                        }
                    />

                    <Route
                        path="/my-redemptions"
                        element={
                            <ProtectedRoute role="Customer">
                                <MyRedemptionsPage />
                            </ProtectedRoute>
                        }
                    />

                    <Route
                        path="/profile"
                        element={
                            <ProtectedRoute role="Customer">
                                <ProfilePage />
                            </ProtectedRoute>
                        }
                    />

                    <Route
                        path="/login"
                        element={<LoginPage />}
                    />

                    <Route
                        path="/register"
                        element={<RegisterPage />}
                    />

                    <Route
                        path="/owner"
                        element={
                            <ProtectedRoute role="Owner">
                                <OwnerDashboardPage />
                            </ProtectedRoute>
                        }
                    />

                    <Route
                        path="/owner/deals"
                        element={
                            <ProtectedRoute role="Owner">
                                <OwnerDealsPage />
                            </ProtectedRoute>
                        }
                    />

                    <Route
                        path="/owner/deals/new"
                        element={
                            <ProtectedRoute role="Owner">
                                <CreateDealPage />
                            </ProtectedRoute>
                        }
                    />

                    <Route
                        path="/owner/deals/edit/:id"
                        element={
                            <ProtectedRoute role="Owner">
                                <EditDealPage />
                            </ProtectedRoute>
                        }
                    />

                    <Route
                        path="/owner/restaurant"
                        element={
                            <ProtectedRoute role="Owner">
                                <OwnerRestaurantPage />
                            </ProtectedRoute>
                        }
                    />

                    <Route
                        path="/owner/restaurant/new"
                        element={
                            <ProtectedRoute role="Owner">
                                <OwnerCreateRestaurantPage />
                            </ProtectedRoute>
                        }
                    />

                    <Route
                        path="/owner/opening-hours"
                        element={
                            <ProtectedRoute role="Owner">
                                <OwnerOpeningHoursPage />
                            </ProtectedRoute>
                        }
                    />

                    <Route
                        path="/admin"
                        element={
                            <ProtectedRoute role="Admin">
                                <AdminDashboardPage />
                            </ProtectedRoute>
                        }
                    />

                    <Route
                        path="/admin/users"
                        element={
                            <ProtectedRoute role="Admin">
                                <AdminUsersPage />
                            </ProtectedRoute>
                        }
                    />

                </Route>

                <Route
                    path="/owner/menu-items"
                    element={
                        <ProtectedRoute role="Owner">
                            <OwnerMenuItemsPage />
                        </ProtectedRoute>
                    }
                />


                <Route
                    path="/owner/menu-categories"
                    element={
                        <ProtectedRoute role="Owner">
                            <OwnerMenuCategoriesPage />
                        </ProtectedRoute>
                    }
                />

                

            </Routes>

        </BrowserRouter>

    );

}

export default AppRoutes;


// ==========================================================
// FRONTEND FLOW — STEP 4
// ==========================================================
//
// AppRoutes.tsx = ROUTE / PAGE DIRECTORY.
//
// Main job:
// "When the user goes to a URL,
//  which page should React show?"
//
// ----------------------------------------------------------
//
// BrowserRouter
// → Turns on browser-based routing.
//
// Routes
// → Contains the list of application routes.
//
// Route
// → Connects a URL to a React page.
//
// Example:
//
// /restaurants
//      ↓
// RestaurantsPage
//
// ----------------------------------------------------------
//
// :id means the value can change.
//
// /restaurants/:id
//
// Examples:
// /restaurants/10
// /restaurants/25
// /restaurants/100
//
// All can show RestaurantDetailsPage.
//
// ----------------------------------------------------------
//
// ProtectedRoute
// → Checks whether the user has the required role.
//
// Example:
//
// /owner
//    ↓
// ProtectedRoute role="Owner"
//    ↓
// Is user an Owner?
//    ↓
// Yes → OwnerDashboardPage
// No  → Block / redirect
//
// ----------------------------------------------------------
//
// MainLayout
// → Common layout around pages
//   such as Header, Navigation and Footer.
//
// ----------------------------------------------------------
//
// SIMPLE FLOW:
//
// User enters URL
//      ↓
// AppRoutes
//      ↓
// Find matching Route
//      ↓
// Check ProtectedRoute if required
//      ↓
// Show the Page
//
// ----------------------------------------------------------
//
// EXAMPLES:
//
// "/"                    → HomePage
// "/restaurants"         → RestaurantsPage
// "/restaurants/:id"     → RestaurantDetailsPage
// "/login"               → LoginPage
// "/favorites"           → Customer only
// "/owner"               → Owner only
// "/admin"               → Admin only
//
// 🔑 Remember:
//
// AppRoutes = "Which page should I show for this URL?"
// ProtectedRoute = "Is this user allowed to see it?"
//
// ==========================================================