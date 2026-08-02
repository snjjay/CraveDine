import api from "../api/axios";
import type { LoginRequest, AuthResponse } from "../features/auth/types";

// ==========================================================
// AUTHENTICATION SERVICE
// ==========================================================
//
// Handles all authentication API calls.
//
// Future methods:
//
// • login()
// • logout()
// • register()
// • refreshToken()
// • forgotPassword()
// • changePassword()
//
// ==========================================================

class AuthService {

    // ------------------------------------------------------
    // Login
    //
    // POST:
    //      /api/auth/login
    //
    // Returns:
    //      JWT Token
    //      User Information
    // ------------------------------------------------------
    async login(request: LoginRequest): Promise<AuthResponse> {

        const response = await api.post<AuthResponse>(
            "/auth/login",
            request
        );

        return response.data;
    }
}

export default new AuthService();