//RegisterPage collects a new customer's details → sends them to the API → receives
//the user/token → gives them to AuthProvider → user is now logged in.


import { useContext } from "react";

import {
    Button,
    Link as MuiLink,
    Stack,
    TextField
} from "@mui/material";

import { useForm } from "react-hook-form";

import AuthService from "../../services/AuthService";
import AuthContext from "./AuthContext";
import AuthCard from "../../components/common/AuthCard";
import type { RegisterRequest } from "./types";
import { Link as RouterLink, useNavigate } from "react-router-dom";
import { useNotification } from "../notifications/NotificationContext";

function RegisterPage() {

    const navigate = useNavigate();

    const { notify } = useNotification();

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
        formState: { errors }
    } = useForm<RegisterRequest>();

    // Called when the user clicks Create Account
    async function onSubmit(data: RegisterRequest) {

        try {

            // Send registration request to ASP.NET Core API
            const response = await AuthService.register(data);

            // A successful registration already returns a JWT,
            // so the new user is logged in immediately - no
            // separate login step is required.
            login(response);

            navigate("/");

        }
        catch (error: any) {

            console.error(error);

            const apiMessage = error.response?.data?.Message;

            if (apiMessage === "Email already exists.") {

                notify("An account with this email already exists.", "error");

            }
            else if (apiMessage) {

                notify(apiMessage, "error");

            }
            else {

                notify("Registration failed. Please try again.", "error");

            }

        }
    }

    return (

        <AuthCard
            title="Create your account"
            subtitle="Join CraveDine to redeem walk-in offers at restaurants near you."
            footer={
                <>
                    Already have an account?{" "}
                    <MuiLink component={RouterLink} to="/login">
                        Log in
                    </MuiLink>
                </>
            }
        >

                <form onSubmit={handleSubmit(onSubmit)}>

                    <Stack spacing={2}>

                        <TextField
                            label="First Name"
                            autoComplete="given-name"
                            error={!!errors.firstName}
                            helperText={errors.firstName?.message}
                            {...register("firstName", {
                                required: "First name is required"
                            })}
                        />

                        <TextField
                            label="Last Name"
                            autoComplete="family-name"
                            error={!!errors.lastName}
                            helperText={errors.lastName?.message}
                            {...register("lastName", {
                                required: "Last name is required"
                            })}
                        />

                        <TextField
                            label="Email"
                            autoComplete="email"
                            error={!!errors.email}
                            helperText={errors.email?.message}
                            {...register("email", {
                                required: "Email is required",
                                pattern: {
                                    value: /^[^\s@]+@[^\s@]+\.[^\s@]+$/,
                                    message: "Enter a valid email address"
                                }
                            })}
                        />

                        <TextField
                            label="Password"
                            autoComplete="new-password"
                            type="password"
                            error={!!errors.password}
                            helperText={errors.password?.message}
                            {...register("password", {
                                required: "Password is required",
                                minLength: {
                                    value: 8,
                                    message: "Password must be at least 8 characters"
                                }
                            })}
                        />

                        <TextField
                            label="Phone Number"
                            autoComplete="tel"
                            {...register("phoneNumber")}
                        />

                        <Button
                            variant="contained"
                            type="submit"
                            size="large"
                            fullWidth
                        >
                            Create Account
                        </Button>

                    </Stack>

                </form>

        </AuthCard>
    );
}

export default RegisterPage;
