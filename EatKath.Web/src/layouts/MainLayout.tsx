import { useContext, useState, type ReactNode } from "react"; //enables to read shared data from Context.
import { Link, Outlet, useLocation } from "react-router-dom";

import {
    AppBar,
    Avatar,
    Box,
    Button,
    Container,
    Divider,
    Drawer,
    IconButton,
    List,
    ListItemButton,
    ListItemIcon,
    ListItemText,
    ListSubheader,
    Menu,
    MenuItem,
    Stack,
    Toolbar,
    Typography,
    useMediaQuery,
    useTheme
} from "@mui/material";

import AdminPanelSettingsOutlinedIcon from "@mui/icons-material/AdminPanelSettingsOutlined";
import CloseIcon from "@mui/icons-material/Close";
import ConfirmationNumberOutlinedIcon from "@mui/icons-material/ConfirmationNumberOutlined";
import FavoriteBorderIcon from "@mui/icons-material/FavoriteBorder";
import HomeOutlinedIcon from "@mui/icons-material/HomeOutlined";
import KeyboardArrowDownIcon from "@mui/icons-material/KeyboardArrowDown";
import LocalDiningIcon from "@mui/icons-material/LocalDining";
import LoginIcon from "@mui/icons-material/Login";
import LogoutIcon from "@mui/icons-material/Logout";
import MenuIcon from "@mui/icons-material/Menu";
import PersonAddAlt1OutlinedIcon from "@mui/icons-material/PersonAddAlt1Outlined";
import PersonOutlineIcon from "@mui/icons-material/PersonOutline";
import StorefrontOutlinedIcon from "@mui/icons-material/StorefrontOutlined";

import AuthContext from "../features/auth/AuthContext";

// Shared page width: ~1440px with responsive side padding.
const PAGE_CONTAINER_SX = {
    maxWidth: 1440,
    px: { xs: 2, sm: 3, md: 4 }
};

interface NavItem {
    label: string;
    to: string;
    icon: ReactNode;
    // Paths (besides `to`) that should highlight this item.
    matches: (pathname: string) => boolean;
}

function MainLayout() {

    const auth = useContext(AuthContext); //What is the current user's login information?

    if (!auth)
        throw new Error("AuthContext not found.");

    const { user, logout } = auth; //user → current logged-in user || logout → function to log the user out

    const isCustomer = user?.role === "Customer";  // Is the user a customer?
    const isOwner = user?.role === "Owner";        // Is the user an owner?
    const isAdmin = user?.role === "Admin";        // Is the user an admin?

    const { pathname } = useLocation();

    const [drawerOpen, setDrawerOpen] = useState(false);

    // Customer account dropdown (desktop header only). It is also shut
    // when the screen narrows to the mobile layout, where its anchor
    // button is hidden and the drawer takes over.
    const [accountAnchor, setAccountAnchor] = useState<HTMLElement | null>(null);
    const isDesktop = useMediaQuery(useTheme().breakpoints.up("md"));
    const accountMenuOpen = Boolean(accountAnchor) && isDesktop;

    // Account identity: the full name (account menu and the button's
    // aria-label) and avatar initials from the signed-in user's first
    // name and surname.
    const fullName = [user?.firstName, user?.lastName].filter(Boolean).join(" ");
    const initials = [user?.firstName, user?.lastName]
        .map(name => name?.trim().charAt(0) ?? "")
        .join("")
        .toUpperCase();

    // Navigation for the current role. Home (the restaurant list) is
    // shared; restaurant detail pages also count as Home.
    const navItems: NavItem[] = [
        {
            label: "Home",
            to: "/",
            icon: <HomeOutlinedIcon fontSize="small" />,
            matches: p => p === "/" || p.startsWith("/restaurants")
        }
    ];

    if (isCustomer) {
        navItems.push(
            {
                label: "Favourites",
                to: "/favorites",
                icon: <FavoriteBorderIcon fontSize="small" />,
                matches: p => p.startsWith("/favorites")
            },
            {
                label: "My Redemptions",
                to: "/my-redemptions",
                icon: <ConfirmationNumberOutlinedIcon fontSize="small" />,
                matches: p => p.startsWith("/my-redemptions")
            }
        );
    }

    if (isOwner) {
        navItems.push({
            label: "Owner Dashboard",
            to: "/owner",
            icon: <StorefrontOutlinedIcon fontSize="small" />,
            matches: p => p.startsWith("/owner")
        });
    }

    if (isAdmin) {
        navItems.push({
            label: "Admin Dashboard",
            to: "/admin",
            icon: <AdminPanelSettingsOutlinedIcon fontSize="small" />,
            matches: p => p.startsWith("/admin")
        });
    }

    function handleLogout() {
        setDrawerOpen(false);
        logout();
    }

    function closeAccountMenu() {
        setAccountAnchor(null);
    }

    function handleAccountSignOut() {
        closeAccountMenu();
        logout();
    }

    return (

        <Box sx={{ minHeight: "100vh", bgcolor: "background.default" }}>

            <AppBar position="sticky">

                <Container maxWidth={false} sx={PAGE_CONTAINER_SX}>

                    <Toolbar disableGutters sx={{ minHeight: { xs: 60, md: 68 }, gap: 2 }}>

                        {/* Wordmark */}
                        <Box
                            component={Link}
                            to="/"
                            aria-label="CraveDine home"
                            sx={{
                                display: "flex",
                                alignItems: "center",
                                gap: 1,
                                color: "text.primary",
                                textDecoration: "none",
                                borderRadius: "10px",
                                mr: { md: 2 },
                                "&:focus-visible": { outline: "2px solid", outlineColor: "primary.main", outlineOffset: 2 }
                            }}
                        >
                            <Box
                                aria-hidden
                                sx={{
                                    width: 38,
                                    height: 38,
                                    borderRadius: "10px",
                                    bgcolor: "brand",
                                    color: "#FFFFFF",
                                    display: "grid",
                                    placeItems: "center"
                                }}
                            >
                                <LocalDiningIcon sx={{ fontSize: 22 }} />
                            </Box>
                            <Typography
                                component="span"
                                sx={{ fontFamily: "h6.fontFamily", fontWeight: 700, fontSize: "1.375rem", lineHeight: 1.2, letterSpacing: "-0.02em" }}
                            >
                                Crave<Box component="span" sx={{ color: "primary.main" }}>Dine</Box>
                            </Typography>
                        </Box>

                        {/* Desktop navigation */}
                        <Stack
                            component="nav"
                            aria-label="Main navigation"
                            direction="row"
                            spacing={0.5}
                            sx={{ display: { xs: "none", md: "flex" }, flexGrow: 1 }}
                        >
                            {navItems.map(item => {

                                const active = item.matches(pathname);

                                return (
                                    <Button
                                        key={item.to}
                                        component={Link}
                                        to={item.to}
                                        startIcon={item.icon}
                                        aria-current={active ? "page" : undefined}
                                        sx={{
                                            position: "relative",
                                            color: active ? "primary.main" : "text.secondary",
                                            whiteSpace: "nowrap",
                                            "&:hover": {
                                                backgroundColor: "action.hover",
                                                color: active ? "primary.main" : "text.primary"
                                            },
                                            // Active indicator on the header's bottom edge
                                            "&::after": {
                                                content: '""',
                                                position: "absolute",
                                                left: 12,
                                                right: 12,
                                                bottom: -14,
                                                height: 3,
                                                borderRadius: "3px 3px 0 0",
                                                bgcolor: active ? "primary.main" : "transparent"
                                            }
                                        }}
                                    >
                                        {item.label}
                                    </Button>
                                );

                            })}
                        </Stack>

                        {/* Desktop account actions */}
                        <Stack
                            direction="row"
                            spacing={1}
                            sx={{ display: { xs: "none", md: "flex" }, alignItems: "center" }}
                        >
                            {isCustomer ? (
                                <Button
                                    id="account-menu-button"
                                    color="inherit"
                                    aria-label={`Account menu for ${fullName}`}
                                    aria-haspopup="menu"
                                    aria-controls={accountMenuOpen ? "account-menu" : undefined}
                                    aria-expanded={accountMenuOpen}
                                    onClick={event => setAccountAnchor(event.currentTarget)}
                                    endIcon={<KeyboardArrowDownIcon fontSize="small" />}
                                    sx={{
                                        pl: 0.75,
                                        gap: 0.25,
                                        color: "text.primary",
                                        "& .MuiButton-endIcon": {
                                            ml: 0.25,
                                            transition: "transform 160ms ease",
                                            transform: accountMenuOpen ? "rotate(180deg)" : "none"
                                        }
                                    }}
                                >
                                    {/* Avatar + arrow only; the name and email are in
                                        the menu (and in the button's aria-label). */}
                                    <Avatar
                                        aria-hidden
                                        sx={{ width: 30, height: 30, bgcolor: "primary.main", fontSize: "0.8125rem", fontWeight: 700 }}
                                    >
                                        {initials || <PersonOutlineIcon fontSize="small" />}
                                    </Avatar>
                                </Button>
                            ) : user ? (
                                <>
                                    <Typography variant="body2" color="text.secondary" sx={{ whiteSpace: "nowrap" }}>
                                        Hi, {user.firstName}
                                    </Typography>
                                    <Button
                                        variant="outlined"
                                        color="inherit"
                                        startIcon={<LogoutIcon fontSize="small" />}
                                        onClick={logout}
                                    >
                                        Logout
                                    </Button>
                                </>
                            ) : (
                                <>
                                    <Button component={Link} to="/login" color="inherit">
                                        Login
                                    </Button>
                                    <Button component={Link} to="/register" variant="contained">
                                        Sign Up
                                    </Button>
                                </>
                            )}
                        </Stack>

                        {/* Customer account dropdown (desktop) */}
                        {isCustomer && (
                            <Menu
                                id="account-menu"
                                anchorEl={accountAnchor}
                                open={accountMenuOpen}
                                onClose={closeAccountMenu}
                                anchorOrigin={{ vertical: "bottom", horizontal: "right" }}
                                transformOrigin={{ vertical: "top", horizontal: "right" }}
                                slotProps={{
                                    paper: { sx: { mt: 1, minWidth: 240, maxWidth: 320 } },
                                    list: { "aria-labelledby": "account-menu-button", sx: { py: 0.5 } }
                                }}
                            >
                                {/* Account header. ListSubheader (not a Box) so the
                                    menu's keyboard focus skips it. */}
                                <ListSubheader
                                    component="div"
                                    disableSticky
                                    sx={{ px: 2, pt: 1, pb: 1.25, lineHeight: "normal", bgcolor: "transparent", color: "text.primary" }}
                                >
                                    <Typography variant="subtitle1" sx={{ fontWeight: 700, lineHeight: 1.35, overflowWrap: "anywhere" }}>
                                        {fullName}
                                    </Typography>
                                    <Typography variant="body2" color="text.secondary" sx={{ overflowWrap: "anywhere" }}>
                                        {user?.email}
                                    </Typography>
                                </ListSubheader>

                                <Divider sx={{ my: 0.5 }} />

                                {/* Favourites and My Redemptions are in the main
                                    navigation, so the menu only holds account items. */}
                                <MenuItem
                                    component={Link}
                                    to="/profile"
                                    selected={pathname.startsWith("/profile")}
                                    onClick={closeAccountMenu}
                                >
                                    <ListItemIcon><PersonOutlineIcon fontSize="small" /></ListItemIcon>
                                    Profile
                                </MenuItem>

                                <Divider sx={{ my: 0.5 }} />

                                <MenuItem onClick={handleAccountSignOut}>
                                    <ListItemIcon><LogoutIcon fontSize="small" /></ListItemIcon>
                                    Sign out
                                </MenuItem>
                            </Menu>
                        )}

                        {/* Mobile menu button */}
                        <Box sx={{ display: { xs: "flex", md: "none" }, ml: "auto" }}>
                            <IconButton
                                aria-label="Open navigation menu"
                                aria-controls={drawerOpen ? "mobile-navigation" : undefined}
                                aria-expanded={drawerOpen}
                                onClick={() => setDrawerOpen(true)}
                                sx={{ width: 44, height: 44 }}
                            >
                                <MenuIcon />
                            </IconButton>
                        </Box>

                    </Toolbar>

                </Container>

            </AppBar>

            {/* Mobile navigation drawer */}
            <Drawer
                id="mobile-navigation"
                anchor="right"
                open={drawerOpen}
                onClose={() => setDrawerOpen(false)}
                slotProps={{ paper: { sx: { width: 300, maxWidth: "85vw" } } }}
            >
                <Stack direction="row" sx={{ alignItems: "center", justifyContent: "space-between", px: 2, py: 1.5 }}>
                    <Typography sx={{ fontFamily: "h6.fontFamily", fontWeight: 800, fontSize: "1.125rem" }}>
                        Menu
                    </Typography>
                    <IconButton aria-label="Close navigation menu" onClick={() => setDrawerOpen(false)} sx={{ width: 44, height: 44 }}>
                        <CloseIcon />
                    </IconButton>
                </Stack>

                <Divider />

                {user && (
                    <Typography variant="body2" color="text.secondary" sx={{ px: 2.5, pt: 2 }}>
                        Signed in as {user.firstName}
                    </Typography>
                )}

                <List component="nav" aria-label="Main navigation" sx={{ px: 1, py: 1 }}>
                    {navItems.map(item => {

                        const active = item.matches(pathname);

                        return (
                            <ListItemButton
                                key={item.to}
                                component={Link}
                                to={item.to}
                                selected={active}
                                aria-current={active ? "page" : undefined}
                                onClick={() => setDrawerOpen(false)}
                                sx={{ borderRadius: "10px", minHeight: 48, color: active ? "primary.main" : "text.primary" }}
                            >
                                <ListItemIcon sx={{ minWidth: 36, color: "inherit" }}>
                                    {item.icon}
                                </ListItemIcon>
                                <ListItemText primary={item.label} slotProps={{ primary: { sx: { fontWeight: 600 } } }} />
                            </ListItemButton>
                        );

                    })}
                </List>

                <Divider />

                <Stack spacing={1} sx={{ p: 2 }}>
                    {user ? (
                        <Button
                            variant="outlined"
                            color="inherit"
                            startIcon={<LogoutIcon fontSize="small" />}
                            onClick={handleLogout}
                            fullWidth
                        >
                            Logout
                        </Button>
                    ) : (
                        <>
                            <Button
                                component={Link}
                                to="/login"
                                variant="outlined"
                                color="inherit"
                                startIcon={<LoginIcon fontSize="small" />}
                                onClick={() => setDrawerOpen(false)}
                                fullWidth
                            >
                                Login
                            </Button>
                            <Button
                                component={Link}
                                to="/register"
                                variant="contained"
                                startIcon={<PersonAddAlt1OutlinedIcon fontSize="small" />}
                                onClick={() => setDrawerOpen(false)}
                                fullWidth
                            >
                                Sign Up
                            </Button>
                        </>
                    )}
                </Stack>
            </Drawer>

            <Container
                component="main"
                maxWidth={false}
                sx={{ ...PAGE_CONTAINER_SX, py: { xs: 3, md: 4 } }}
            >

                <Outlet /> {/*is basically the placeholder where the selected page gets inserted. MainLayout provides the common page structure. <Outlet /> is where the current route's page appears.*/}

            </Container>

        </Box>

    );

}

export default MainLayout;

// ==========================================================
// FRONTEND FLOW — STEP 5
// ==========================================================
//
// MainLayout.tsx = COMMON PAGE STRUCTURE.
//
// Provides things that appear around many pages,
// such as the navigation bar.
//
// FLOW:
//
// 1. index.html
//      ↓
// 2. main.tsx
//      ↓
// 3. App.tsx
//      ↓
// 4. AppRoutes.tsx
//      ↓
// 5. MainLayout.tsx  ← HERE
//      ↓
// 6. Page
//
// ----------------------------------------------------------
//
// AuthContext
// → Gets the current user and logout function.
//
// user
// → Current logged-in user.
//
// Role checks:
//
// isCustomer → show Customer navigation
// isOwner    → show Owner navigation
// isAdmin    → show Admin navigation
//
// ----------------------------------------------------------
//
// Navigation:
//
// Link + to="/restaurants"
// → Clicking the button goes to /restaurants.
//
// AppRoutes then chooses RestaurantsPage.
//
// ----------------------------------------------------------
//
// <Outlet />
// → PLACEHOLDER where the current route's page appears.
//
// Example:
//
// /restaurants
//      ↓
// MainLayout
//      ↓
// <Outlet />
//      ↓
// RestaurantsPage
//
// /
//      ↓
// MainLayout
//      ↓
// <Outlet />
//      ↓
// HomePage
//
// 🔑 Remember:
//
// MainLayout = common structure/navigation
// Outlet     = place where the selected page appears
//
// ==========================================================