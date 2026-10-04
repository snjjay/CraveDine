//LoginPage collects the user's login details → sends them to the API → receives 
//the user/token → gives them to AuthProvider → user is now logged in.


import { useContext, useState } from "react";

import axios from "axios";

import {
    Alert,
    Button,
    Container,
    Paper,
    Stack,
    TextField,
    Typography
} from "@mui/material";

import { useForm } from "react-hook-form";

import AuthService from "../../services/AuthService";
import AuthContext from "./AuthContext";
import type { LoginRequest } from "./types";
import { useNavigate } from "react-router-dom";

// Turns a failed login request into a safe, user-friendly message.
// Never shows raw server text, stack traces or request details.
function getLoginErrorMessage(error: unknown): string {

    if (axios.isAxiosError(error)) {

        // No response at all = API unreachable (offline, server down, CORS, timeout)
        if (!error.response) {
            return "Unable to connect to the server. Please try again later.";
        }

        const status = error.response.status;

        if (status === 401) {
            return "Invalid email or password. Please check your credentials and try again.";
        }

        if (status === 400) {
            return "Please enter a valid email address and password.";
        }

        if (status >= 500) {
            return "Something went wrong on our end. Please try again later.";
        }
    }

    return "Login failed. Please try again.";
}

function LoginPage() {

    const navigate = useNavigate();

    // Access the Authentication Context
    const auth = useContext(AuthContext);//Give me the authentication functions.

    if (!auth) {
        throw new Error("AuthContext not found.");
    }

    const { login } = auth;


    // React Hook Form
    const {
        register,
        handleSubmit,
        formState: { isSubmitting }
    } = useForm<LoginRequest>();

    // Message shown in the Alert when login fails
    const [loginError, setLoginError] = useState<string | null>(null);

    // Hide the old error as soon as the user starts correcting their details
    function clearLoginError() {
        if (loginError) {
            setLoginError(null);
        }
    }

    // Called when the user clicks Login
    async function onSubmit(data: LoginRequest) {

        setLoginError(null);

        try {

            // Send login request to ASP.NET Core API
            const response = await AuthService.login(data);

            // Save authenticated user in Auth Context
            login(response);

            navigate("/");

        }
        catch (error) {

            // Stay on the login page and tell the user what went wrong.
            // The full error object is not logged because it contains
            // the submitted request body (including the password).
            setLoginError(getLoginErrorMessage(error));

        }
    }

    return (

        <Container maxWidth="sm">

            <Paper sx={{ p: 4, mt: 6 }}>

                <Typography
                    variant="h4"
                    sx={{ mb: 3 }}
                >
                    Login
                </Typography>

                <form onSubmit={handleSubmit(onSubmit)}>

                    <Stack spacing={2}>

                        {loginError && (
                            <Alert severity="error" role="alert">
                                {loginError}
                            </Alert>
                        )}

                        <TextField
                            label="Email"
                            {...register("email", { onChange: clearLoginError })}
                        />

                        <TextField
                            label="Password"
                            type="password"
                            {...register("password", { onChange: clearLoginError })}
                        />

                        <Button
                            variant="contained"
                            type="submit"
                            disabled={isSubmitting}
                        >
                            {isSubmitting ? "Logging in..." : "Login"}
                        </Button>

                    </Stack>

                </form>

            </Paper>

        </Container >
    );
}

export default LoginPage;


// ==========================================================
// STEP 18 — LoginPage.tsx
// ==========================================================
//
// LoginPage = collects login details and starts authentication.
//
// MAIN FLOW:
//
// 👤 User enters email + password
//          ↓
// 📝 LoginPage
//          ↓
// AuthService.login()
//          ↓
// axios
//          ↓
// .NET API
//          ↓
// ✅ API returns AuthResponse
//          ↓
// login(response)
//          ↓
// AuthProvider
//          ↓
// 💾 Save user + token
//          ↓
// navigate("/")
//          ↓
// 🏠 HomePage
//
// ----------------------------------------------------------
//
// useForm()
// → 📝 Tools for managing the login form.
//
// register("email")
// → Connects the Email textbox to the form.
//
// register("password")
// → Connects the Password textbox to the form.
//
// handleSubmit()
// → Collects the form data when Login is clicked.
//
// ----------------------------------------------------------
//
// AuthService.login(data)
// → Sends the email/password to the backend.
//
//
//
// const response = await AuthService.login(data);
//
// → Wait for the API response.
//
// → response contains the authenticated user's information
//   and token.
//
// ----------------------------------------------------------
//
// login(response)
//
// → Give the successful login result to AuthProvider.
//
// AuthProvider then:
//
// → Saves the user/token in localStorage.
// → Updates the React user state.
//
// ----------------------------------------------------------
//
// navigate("/")
//
// → After successful login, go to the Home page.
//
// ----------------------------------------------------------
//
// 🔑 REMEMBER:
//
// LoginPage
// → Collects credentials.
//
// AuthService
// → Sends login request.
//
// AuthProvider
// → Stores/manages logged-in user.
//
// ProtectedRoute
// → Checks whether user is allowed to access a page.
//
// ==========================================================
// ==========================================================
// AUTH FLOW
//
// 15. AuthContext.tsx       ✅
// 16. AuthProvider.tsx      ✅
// 17. ProtectedRoute.tsx    ✅
// 18. LoginPage.tsx         ✅
//       ↓
// 19. AuthService.ts        ← NEXT
//       ↓
// 20. axios.ts              ✅ already studied
//       ↓
// 21. .NET API              ⏭️ skip
//
// ==========================================================