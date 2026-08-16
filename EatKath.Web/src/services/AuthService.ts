
//AuthService = the worker that sends login requests to the backend.
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
    async login(request: LoginRequest): Promise<AuthResponse> { //Give me the user's login details, send them to the API, and I'll return an AuthResponse

        const response = await api.post<AuthResponse>( //Send a POST request to /auth/login and send the login details with it.
            "/auth/login",
            request
        );

        return response.data; //Take the data returned by the API and give it back to LoginPage.
    }
}

export default new AuthService();

// ==========================================================
// STEP 19 — AuthService.ts
// ==========================================================
//
// AuthService = 🧑‍💼 LOGIN API WORKER.
//
// Its job:
// → Send authentication requests to the .NET API.
//
// ----------------------------------------------------------
//
// Login flow:
//
// LoginPage
//      ↓
// AuthService.login(data)
//      ↓
// api.post()
//      ↓
// /auth/login
//      ↓
// .NET API
//      ↓
// AuthResponse
//      ↓
// response.data
//      ↓
// LoginPage
//
// ----------------------------------------------------------
//
// login(request)
//
// → Receives the user's login details.
//
// Example:
//
// {
//     email: "user@email.com",
//     password: "******"
// }
//
// ----------------------------------------------------------
//
// api.post<AuthResponse>(
//     "/auth/login",
//     request
// )
//
// → Sends a POST request to the backend login endpoint.
//
// Think:
//
// 📞 "Send these login details to the login office."
//
// ----------------------------------------------------------
//
// return response.data;
//
// → Take the data returned by the API
//   and give it back to LoginPage.
//
// ----------------------------------------------------------
//
// 🔑 Remember:
//
// LoginPage  = collects login details
// AuthService = sends login request
// axios      = actually sends HTTP request
// .NET API   = checks login and returns response
// AuthProvider = stores the successful login
//
// ==========================================================

//So the authentication chain is now complete:
//15. AuthContext.tsx       ✅
//16. AuthProvider.tsx      ✅
//17. ProtectedRoute.tsx    ✅
//18. LoginPage.tsx         ✅
//19. AuthService.ts        ✅

//Next, we should move out of authentication and continue with the 
//remaining frontend services / types that are actually used by your pages.