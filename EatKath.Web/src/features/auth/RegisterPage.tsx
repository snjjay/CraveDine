//RegisterPage collects a new customer's details → sends them to the API → receives
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
import type { RegisterRequest } from "./types";
import { useNavigate } from "react-router-dom";
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

        <Container maxWidth="sm">

            <Paper sx={{ p: 4, mt: 6 }}>

                <Typography
                    variant="h4"
                    sx={{ mb: 3 }}
                >
                    Create Account
                </Typography>

                <form onSubmit={handleSubmit(onSubmit)}>

                    <Stack spacing={2}>

                        <TextField
                            label="First Name"
                            error={!!errors.firstName}
                            helperText={errors.firstName?.message}
                            {...register("firstName", {
                                required: "First name is required"
                            })}
                        />

                        <TextField
                            label="Last Name"
                            error={!!errors.lastName}
                            helperText={errors.lastName?.message}
                            {...register("lastName", {
                                required: "Last name is required"
                            })}
                        />

                        <TextField
                            label="Email"
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
                            {...register("phoneNumber")}
                        />

                        <Button
                            variant="contained"
                            type="submit"
                        >
                            Create Account
                        </Button>

                    </Stack>

                </form>

            </Paper>

        </Container>
    );
}

export default RegisterPage;
