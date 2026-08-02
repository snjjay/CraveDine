import { BrowserRouter, Routes, Route } from "react-router-dom";

import HomePage from "../pages/HomePage";
import LoginPage from "../pages/LoginPage";
import RestaurantsPage from "../pages/RestaurantsPage";
import RestaurantDetailsPage from "../pages/RestaurantDetailsPage";
import OwnerDashboardPage from "../pages/OwnerDashboardPage";
import AdminDashboardPage from "../pages/AdminDashboardPage";

function AppRoutes() {
    return (
        <BrowserRouter>
            <Routes>
                <Route path="/" element={<HomePage />} />
                <Route path="/login" element={<LoginPage />} />
                <Route path="/restaurants" element={<RestaurantsPage />} />
                <Route path="/restaurants/:id" element={<RestaurantDetailsPage />} />
                <Route path="/owner" element={<OwnerDashboardPage />} />
                <Route path="/admin" element={<AdminDashboardPage />} />
            </Routes>
        </BrowserRouter>
    );
}

export default AppRoutes;