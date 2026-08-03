import { BrowserRouter, Route, Routes } from "react-router-dom";

import MainLayout from "../layouts/MainLayout";

import HomePage from "../pages/HomePage";
import RestaurantsPage from "../pages/RestaurantsPage";
import RestaurantDetailsPage from "../pages/RestaurantDetailsPage";

import OwnerDashboardPage from "../pages/OwnerDashboardPage";
import OwnerDealsPage from "../pages/OwnerDealsPage";
import CreateDealPage from "../pages/CreateDealPage";

import AdminDashboardPage from "../pages/AdminDashboardPage";

import LoginPage from "../features/auth/LoginPage";
import ProtectedRoute from "../features/auth/ProtectedRoute";
import EditDealPage from "../pages/EditDealPage";

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
                        path="/login"
                        element={<LoginPage />}
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
                        path="/owner/deals/new"
                        element={
                            <ProtectedRoute>
                                <CreateDealPage />
                            </ProtectedRoute>
                        }
                    />


                    <Route
                        path="/owner/deals/edit/:id"
                        element={
                            <ProtectedRoute>
                                <EditDealPage />
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

                </Route>

            </Routes>

        </BrowserRouter>

    );

}

export default AppRoutes;