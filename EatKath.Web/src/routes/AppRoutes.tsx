import { BrowserRouter, Route, Routes } from "react-router-dom";

import MainLayout from "../layouts/MainLayout";

import HomePage from "../pages/HomePage";
import RestaurantsPage from "../pages/RestaurantsPage";
import RestaurantDetailsPage from "../pages/RestaurantDetailsPage";
import MyFavoritesPage from "../pages/MyFavoritesPage";

import OwnerDashboardPage from "../pages/OwnerDashboardPage";
import OwnerDealsPage from "../pages/OwnerDealsPage";
import CreateDealPage from "../pages/CreateDealPage";
import EditDealPage from "../pages/EditDealPage";
import OwnerRestaurantPage from "../pages/OwnerRestaurantPage";

import AdminDashboardPage from "../pages/AdminDashboardPage";

import LoginPage from "../features/auth/LoginPage";
import ProtectedRoute from "../features/auth/ProtectedRoute";
import MyReservationsPage from "../pages/MyReservationsPage";
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
                        element={<HomePage />}
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
                        path="/login"
                        element={<LoginPage />}
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