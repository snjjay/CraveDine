import { useContext } from "react";
import { Link, Outlet } from "react-router-dom";

import {
    AppBar,
    Box,
    Button,
    Container,
    Toolbar,
    Typography
} from "@mui/material";

import AuthContext from "../features/auth/AuthContext";

function MainLayout() {

    const auth = useContext(AuthContext);

    if (!auth)
        throw new Error("AuthContext not found.");

    const { user, logout } = auth;

    return (

        <Box>

            <AppBar position="static">

                <Toolbar>

                    <Typography
                        variant="h6"
                        sx={{ flexGrow: 1 }}
                    >
                        EatKath
                    </Typography>

                    <Button
                        color="inherit"
                        component={Link}
                        to="/"
                    >
                        Home
                    </Button>

                    <Button
                        color="inherit"
                        component={Link}
                        to="/restaurants"
                    >
                        Restaurants
                    </Button>

                    {user && (

                        <Button
                            color="inherit"
                            component={Link}
                            to="/favorites"
                        >
                            ❤️ Favourites
                        </Button>

                    )}

                    {user ? (

                        <Button
                            color="inherit"
                            onClick={logout}
                        >
                            Logout
                        </Button>

                    ) : (

                        <Button
                            color="inherit"
                            component={Link}
                            to="/login"
                        >
                            Login
                        </Button>

                    )}

                </Toolbar>

            </AppBar>

            <Container sx={{ mt: 4 }}>

                <Outlet />

            </Container>

        </Box>

    );

}

export default MainLayout;