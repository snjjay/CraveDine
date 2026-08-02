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
    const auth = useContext(AuthContext);

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