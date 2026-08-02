// ==========================================================
// AUTHENTICATION TYPES
// ==========================================================
//
// These interfaces match the ASP.NET Core DTOs.
//
// Backend:
// LoginDto          -> LoginRequest
// AuthResponseDto   -> AuthResponse
//
// ==========================================================

// Request sent to the API
export interface LoginRequest {
    email: string;
    password: string;
}

// Response returned by the API
export interface AuthResponse {

    userId: number;

    firstName: string;

    lastName: string;

    email: string;

    role: string;

    token: string;

    expiresAt: string;
}