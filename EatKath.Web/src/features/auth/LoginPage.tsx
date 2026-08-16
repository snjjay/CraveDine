//LoginPage collects the user's login details → sends them to the API → receives 
//the user/token → gives them to AuthProvider → user is now logged in.


import { useContext } from "react";

import {
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
        handleSubmit
    } = useForm<LoginRequest>();

    // Called when the user clicks Login
    async function onSubmit(data: LoginRequest) {

        try {

            // Send login request to ASP.NET Core API
            const response = await AuthService.login(data);

            // Save authenticated user in Auth Context
            login(response);

            navigate("/");

        }
        catch (error) {

            console.error(error);

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

                        <TextField
                            label="Email"
                            {...register("email")}
                        />

                        <TextField
                            label="Password"
                            type="password"
                            {...register("password")}
                        />

                        <Button
                            variant="contained"
                            type="submit"
                        >
                            Login
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