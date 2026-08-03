import { BrowserRouter, Route, Routes } from "react-router-dom";

import MainLayout from "../layouts/MainLayout";

import HomePage from "../pages/HomePage";
import LoginPage from "../features/auth/LoginPage";
import RestaurantsPage from "../pages/RestaurantsPage";
import RestaurantDetailsPage from "../pages/RestaurantDetailsPage";
import OwnerDashboardPage from "../pages/OwnerDashboardPage";
import OwnerDealsPage from "../pages/OwnerDealsPage";
import AdminDashboardPage from "../pages/AdminDashboardPage";

import ProtectedRoute from "../features/auth/ProtectedRoute";

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
                        path="/owner"
                        element={
                            <ProtectedRoute>
                                <OwnerDashboardPage />
                            </ProtectedRoute>
                        }
                    />

                    <Route
                        path="/owner/deals"
                        element={
                            <ProtectedRoute>
                                <OwnerDealsPage />
                            </ProtectedRoute>
                        }
                    />

                    <Route
                        path="/admin"
                        element={
                            <ProtectedRoute>
                                <AdminDashboardPage />
                            </ProtectedRoute>
                        }
                    />

                    <Route
                        path="/login"
                        element={<LoginPage />}
                    />

                </Route>

            </Routes>

        </BrowserRouter>

    );

}

export default AppRoutes;