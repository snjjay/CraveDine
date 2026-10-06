import { createTheme, type Shadows } from "@mui/material/styles";
import { colors, darkColors } from "./colors";

// Custom palette entries, usable in sx:
// - deal:         coral-red offer/discount accent, e.g. color: "deal.dark"
// - primarySoft:  light brand tint for highlights, e.g. bgcolor: "primarySoft"
// - brand:        vivid brand red for the logo tile and large display text
// - successSoft:  light green tint behind "Open" style statuses
// - surfaceMuted: subtle neutral surface (table headers, fallback tiles)
// - borderStrong: form field and outlined button borders
// - raised:       menus, popovers and dialogs
// - inverse:      the dark bands (footer, home hero), always light text
declare module "@mui/material/styles" {
    interface Palette {
        deal: { main: string; dark: string; text: string; soft: string; contrastText: string };
        primarySoft: string;
        brand: string;
        successSoft: string;
        surfaceMuted: string;
        borderStrong: string;
        raised: string;
        inverse: { main: string; contrastText: string };
    }
    interface PaletteOptions {
        deal?: { main: string; dark: string; text: string; soft: string; contrastText: string };
        primarySoft?: string;
        brand?: string;
        successSoft?: string;
        surfaceMuted?: string;
        borderStrong?: string;
        raised?: string;
        inverse?: { main: string; contrastText: string };
    }
}

// Outfit (rounded geometric, strong numerals for discounts) for
// headings; Inter for body and UI text.
const headingFont = '"Outfit", "Inter", "Roboto", "Helvetica", "Arial", sans-serif';
const bodyFont = '"Inter", "Roboto", "Helvetica", "Arial", sans-serif';

// Breakpoints/shadows of a default theme, used to build ours.
const base = createTheme();
const md = base.breakpoints.up("md");

// Very light charcoal shadows (levels 1-4 for surfaces/hover,
// 8 for menus/popovers, 16/24 for drawers/dialogs).
const shadows = [...base.shadows] as Shadows;
shadows[1] = "0 1px 2px rgba(28, 28, 28, 0.04)";
shadows[2] = "0 2px 8px rgba(28, 28, 28, 0.06)";
shadows[3] = "0 6px 18px rgba(28, 28, 28, 0.08)";
shadows[4] = "0 10px 28px rgba(28, 28, 28, 0.10)";
shadows[8] = "0 12px 32px rgba(28, 28, 28, 0.12)";
shadows[16] = "0 16px 40px rgba(28, 28, 28, 0.14)";
shadows[24] = "0 20px 48px rgba(28, 28, 28, 0.16)";

// Day (the original CraveDine theme) and Night colour schemes. Switched
// with useColorScheme() (ThemeToggle); the choice is stored by MUI in
// localStorage ("mui-mode") and follows the OS until the user picks one.
// index.html applies the stored/OS scheme before React loads (no flash).
// Components use palette tokens, which resolve to CSS variables, so the
// same styles work in both schemes.
export const theme = createTheme({

    cssVariables: {
        colorSchemeSelector: "data"
    },

    colorSchemes: {

        light: {
            palette: {
                primary: {
                    main: colors.primary,
                    dark: colors.primaryDark,
                    light: colors.primaryLight,
                    contrastText: "#FFFFFF"
                },
                // Charcoal for neutral emphasis (e.g. secondary buttons).
                secondary: {
                    main: colors.textPrimary,
                    dark: "#000000",
                    light: "#4A4A4A",
                    contrastText: "#FFFFFF"
                },
                deal: {
                    main: colors.deal,
                    dark: colors.dealDark,
                    text: colors.dealText,
                    soft: colors.dealSoft,
                    contrastText: "#FFFFFF"
                },
                success: { main: colors.success },
                warning: { main: colors.warning },
                error: { main: colors.error },
                info: { main: colors.info },
                background: {
                    default: colors.background,
                    paper: colors.surface
                },
                text: {
                    primary: colors.textPrimary,
                    secondary: colors.textSecondary,
                    disabled: colors.textDisabled
                },
                divider: colors.border,
                primarySoft: colors.primarySoft,
                brand: colors.brand,
                successSoft: colors.successSoft,
                surfaceMuted: colors.surfaceMuted,
                borderStrong: colors.borderStrong,
                raised: colors.raised,
                inverse: { main: colors.inverse, contrastText: "#FFFFFF" },
                action: {
                    hover: "rgba(28, 28, 28, 0.05)",
                    // Neutral: also the background of default (grey) chips such
                    // as "Cancelled", so it must not look like a positive status.
                    selected: "#EFEBE6"
                }
            }
        },

        dark: {
            palette: {
                // Lighter coral so coral text, links and focus rings stay
                // readable on dark surfaces; filled buttons keep the
                // original coral (see MuiButton).
                primary: {
                    main: darkColors.primary,
                    dark: darkColors.primaryFill,
                    light: darkColors.primary,
                    contrastText: "#1C1C1C"
                },
                secondary: {
                    main: darkColors.textPrimary,
                    dark: "#FFFFFF",
                    light: "#D9D3CD",
                    contrastText: "#1C1C1C"
                },
                deal: {
                    main: darkColors.deal,
                    dark: darkColors.dealDark,
                    text: darkColors.dealText,
                    soft: darkColors.dealSoft,
                    contrastText: "#FFFFFF"
                },
                success: { main: darkColors.success },
                warning: { main: darkColors.warning },
                error: { main: darkColors.error },
                info: { main: darkColors.info },
                background: {
                    default: darkColors.background,
                    paper: darkColors.surface
                },
                text: {
                    primary: darkColors.textPrimary,
                    secondary: darkColors.textSecondary,
                    disabled: darkColors.textDisabled
                },
                divider: darkColors.border,
                primarySoft: darkColors.primarySoft,
                brand: darkColors.brand,
                successSoft: darkColors.successSoft,
                surfaceMuted: darkColors.surfaceMuted,
                borderStrong: darkColors.borderStrong,
                raised: darkColors.raised,
                inverse: { main: darkColors.inverse, contrastText: "#FFFFFF" },
                action: {
                    hover: "rgba(255, 255, 255, 0.06)",
                    selected: darkColors.selected
                }
            }
        }

    },

    shape: {
        // Buttons and inputs; cards and chips set their own radius below.
        borderRadius: 10
    },

    shadows,

    typography: {
        fontFamily: bodyFont,
        h1: { fontFamily: headingFont, fontWeight: 700, fontSize: "2.25rem", lineHeight: 1.15, letterSpacing: "-0.02em", [md]: { fontSize: "2.75rem" } },
        h2: { fontFamily: headingFont, fontWeight: 700, fontSize: "1.875rem", lineHeight: 1.2, letterSpacing: "-0.02em", [md]: { fontSize: "2.25rem" } },
        h3: { fontFamily: headingFont, fontWeight: 700, fontSize: "1.625rem", lineHeight: 1.2, letterSpacing: "-0.015em", [md]: { fontSize: "1.875rem" } },
        // Page titles
        h4: { fontFamily: headingFont, fontWeight: 700, fontSize: "1.5rem", lineHeight: 1.25, letterSpacing: "-0.01em", [md]: { fontSize: "1.75rem" } },
        // Section titles
        h5: { fontFamily: headingFont, fontWeight: 700, fontSize: "1.1875rem", lineHeight: 1.3, letterSpacing: "-0.005em", [md]: { fontSize: "1.3125rem" } },
        // Card titles
        h6: { fontFamily: headingFont, fontWeight: 600, fontSize: "1.125rem", lineHeight: 1.3 },
        subtitle1: { fontWeight: 600, fontSize: "1rem", lineHeight: 1.45 },
        subtitle2: { fontWeight: 600, fontSize: "0.875rem", lineHeight: 1.45 },
        body1: { fontSize: "0.9375rem", lineHeight: 1.6 },
        body2: { fontSize: "0.875rem", lineHeight: 1.55 },
        caption: { fontSize: "0.75rem", lineHeight: 1.5 },
        overline: { fontWeight: 600, fontSize: "0.6875rem", letterSpacing: "0.08em", lineHeight: 1.6 },
        button: { fontWeight: 600, fontSize: "0.9375rem", textTransform: "none", letterSpacing: 0 }
    },

    components: {

        MuiCssBaseline: {
            styleOverrides: theme => ({
                body: {
                    backgroundColor: theme.vars.palette.background.default,
                    WebkitFontSmoothing: "antialiased",
                    MozOsxFontSmoothing: "grayscale"
                },
                "::selection": {
                    backgroundColor: theme.vars.palette.primarySoft
                }
            })
        },

        // Visible keyboard focus for every button, icon button, tab,
        // chip, list item and card action area.
        MuiButtonBase: {
            styleOverrides: {
                root: ({ theme }) => ({
                    "&.Mui-focusVisible": {
                        outline: `2px solid ${theme.vars.palette.primary.main}`,
                        outlineOffset: 2
                    }
                })
            }
        },

        MuiButton: {
            defaultProps: {
                disableElevation: true
            },
            styleOverrides: {
                root: {
                    borderRadius: 10,
                    paddingInline: 16,
                    minHeight: 40
                },
                sizeSmall: {
                    minHeight: 32,
                    paddingInline: 12,
                    fontSize: "0.8125rem"
                },
                sizeLarge: {
                    minHeight: 48,
                    paddingInline: 22,
                    fontSize: "1rem"
                },
                containedPrimary: ({ theme }) => ({
                    "&:hover": { backgroundColor: theme.vars.palette.primary.dark },
                    // Night: the same filled coral and white text as Day.
                    // :where() adds no specificity, so disabled styles and
                    // component sx still win over this.
                    ":where([data-dark]) &": {
                        backgroundColor: darkColors.primaryFill,
                        color: "#FFFFFF",
                        "&:hover": { backgroundColor: darkColors.primaryFillHover }
                    }
                }),
                outlined: ({ theme }) => ({
                    borderColor: theme.vars.palette.borderStrong,
                    backgroundColor: theme.vars.palette.background.paper
                }),
                outlinedPrimary: ({ theme }) => ({
                    "&:hover": {
                        borderColor: theme.vars.palette.primary.main,
                        backgroundColor: theme.vars.palette.primarySoft
                    }
                })
            }
        },

        MuiIconButton: {
            styleOverrides: {
                root: {
                    borderRadius: 10
                }
            }
        },

        MuiPaper: {
            styleOverrides: {
                root: {
                    backgroundImage: "none"
                },
                rounded: {
                    borderRadius: 14
                },
                // Default surface (Card, Paper, TableContainer): flat
                // white with a subtle border and a very light shadow.
                elevation1: ({ theme }) => ({
                    border: `1px solid ${theme.vars.palette.divider}`,
                    boxShadow: shadows[1]
                })
            }
        },

        MuiCard: {
            styleOverrides: {
                root: {
                    borderRadius: 14,
                    overflow: "hidden"
                }
            }
        },

        MuiCardContent: {
            styleOverrides: {
                root: {
                    padding: 20,
                    "&:last-child": { paddingBottom: 20 }
                }
            }
        },

        MuiChip: {
            styleOverrides: {
                root: {
                    borderRadius: 8,
                    fontWeight: 600
                },
                sizeSmall: {
                    height: 26,
                    fontSize: "0.75rem"
                },
                outlined: ({ theme }) => ({
                    borderColor: theme.vars.palette.borderStrong
                })
            }
        },

        MuiOutlinedInput: {
            styleOverrides: {
                root: ({ theme }) => ({
                    borderRadius: 10,
                    backgroundColor: theme.vars.palette.background.paper,
                    "&:hover:not(.Mui-focused):not(.Mui-error) .MuiOutlinedInput-notchedOutline": {
                        borderColor: theme.vars.palette.text.disabled
                    }
                }),
                notchedOutline: ({ theme }) => ({
                    borderColor: theme.vars.palette.borderStrong
                })
            }
        },

        MuiPopover: {
            styleOverrides: {
                paper: ({ theme }) => ({
                    backgroundColor: theme.vars.palette.raised
                })
            }
        },

        MuiMenu: {
            styleOverrides: {
                paper: ({ theme }) => ({
                    borderRadius: 12,
                    border: `1px solid ${theme.vars.palette.divider}`,
                    backgroundColor: theme.vars.palette.raised
                })
            }
        },

        MuiMenuItem: {
            styleOverrides: {
                root: ({ theme }) => ({
                    "&.Mui-selected": { backgroundColor: theme.vars.palette.primarySoft },
                    "&.Mui-selected:hover": { backgroundColor: theme.vars.palette.primarySoft }
                })
            }
        },

        MuiDialog: {
            styleOverrides: {
                paper: ({ theme }) => ({
                    borderRadius: 14,
                    backgroundColor: theme.vars.palette.raised
                })
            }
        },

        MuiDialogTitle: {
            styleOverrides: {
                root: {
                    fontFamily: headingFont,
                    fontWeight: 700,
                    fontSize: "1.125rem"
                }
            }
        },

        MuiDialogActions: {
            styleOverrides: {
                root: {
                    padding: "12px 24px 20px",
                    gap: 8
                }
            }
        },

        MuiTabs: {
            styleOverrides: {
                indicator: {
                    height: 3,
                    borderRadius: "3px 3px 0 0"
                }
            }
        },

        MuiTab: {
            styleOverrides: {
                root: {
                    textTransform: "none",
                    fontWeight: 600,
                    fontSize: "0.9375rem",
                    minHeight: 48
                }
            }
        },

        MuiAlert: {
            styleOverrides: {
                root: {
                    borderRadius: 10,
                    // Links take the alert's own (high-contrast) text colour,
                    // underlined so they still read as links.
                    "& .MuiLink-root": {
                        color: "inherit",
                        textDecoration: "underline"
                    }
                }
            }
        },

        MuiAppBar: {
            defaultProps: {
                elevation: 0,
                color: "inherit"
            },
            styleOverrides: {
                root: ({ theme }) => ({
                    backgroundColor: theme.vars.palette.background.paper,
                    color: theme.vars.palette.text.primary,
                    borderBottom: `1px solid ${theme.vars.palette.divider}`
                })
            }
        },

        MuiLink: {
            defaultProps: {
                underline: "hover"
            },
            styleOverrides: {
                root: ({ theme }) => ({
                    fontWeight: 500,
                    "&.Mui-focusVisible, &:focus-visible": {
                        outline: `2px solid ${theme.vars.palette.primary.main}`,
                        outlineOffset: 2
                    }
                })
            }
        },

        MuiTableCell: {
            styleOverrides: {
                root: ({ theme }) => ({
                    borderColor: theme.vars.palette.divider
                }),
                head: ({ theme }) => ({
                    backgroundColor: theme.vars.palette.surfaceMuted,
                    color: theme.vars.palette.text.secondary,
                    fontWeight: 600,
                    fontSize: "0.8125rem"
                })
            }
        },

        MuiSkeleton: {
            styleOverrides: {
                rounded: {
                    borderRadius: 14
                }
            }
        },

        // Charcoal with white text by day; light with dark text at night.
        MuiTooltip: {
            styleOverrides: {
                tooltip: ({ theme }) => ({
                    backgroundColor: theme.vars.palette.secondary.main,
                    color: theme.vars.palette.secondary.contrastText,
                    fontSize: "0.75rem",
                    borderRadius: 8
                })
            }
        }

    }

});

// ==========================================================
// THEME — theme.ts
// ==========================================================
//
// theme.ts = 🎨 Global visual/style rulebook.
//
// Uses Material UI (MUI).
//
// It controls the overall appearance of the application.
//
// ----------------------------------------------------------
//
// createTheme()
// → Creates the central MUI theme.
//
// ----------------------------------------------------------
//
// palette
// → 🎨 Defines application colours.
//
// primary
// → Main application colour.
//
// secondary
// → Secondary/supporting colour.
//
// background
// → Default page background colour.
//
// Colours come from:
//
// colors.primary
// colors.secondary
// colors.background
//
// ----------------------------------------------------------
//
// typography
// → ✍️ Defines text rules.
//
// fontFamily
// → Sets the default application font.
//
// h4 / h5
// → Controls heading styling.
//
// button
// → Controls button text styling.
//
// textTransform: "none"
// → Keeps button text as written instead of
//   automatically changing it to uppercase.
//
// ----------------------------------------------------------
//
// HOW IT FITS:
//
// main.tsx
//      ↓
// ThemeProvider
//      ↓
// theme.ts
//      ↓
// Global MUI styling
//      ↓
// MUI components throughout the application
//
// 🔑 REMEMBER:
//
// theme.ts = "How should my application look?"
//
// It does NOT:
// → call APIs
// → handle authentication
// → manage business logic
// → access the database
//
// ==========================================================

//Skip colors , next The next useful area is layouts, but we've already studied MainLayout.tsx. So we should not repeat it.

// ==========================================================
// NEXT FRONTEND STUDY ORDER
//
// Already covered:
//
// index.html              ✅
// main.tsx                ✅
// App.tsx                 ✅
// AppRoutes.tsx           ✅
// MainLayout.tsx          ✅
// MyFavoritesPage.tsx     ✅
// UserFavoriteService.ts  ✅
// axios.ts                ✅
// Components              ✅
// Auth                    ✅
// Types                   ⏭️ SKIP remaining
// Utils                   ✅
// theme.ts                ✅
// colors.ts               ⏭️ SKIP
//
// ----------------------------------------------------------
//
// NEXT:
//
// Restaurant-related Pages
//       ↓
// RestaurantDetailsPage.tsx
//       ↓
// Restaurant-related Services
//       ↓
// Deal-related Pages
//       ↓
// Deal Services
//       ↓
// Reservation Pages / Services
//
// ==========================================================
//I'd recommend this approach
//MyFavoritesPage          ✅ Study in detail
//RestaurantDetailsPage    ⏭️ Quick scan only
//RestaurantsPage          ⏭️ Quick scan only
//HomePage                 ⏭️ Already simple
//Other CRUD pages         ⏭️ Don't study every one


// ==========================================================
// FRONTEND STUDY — WHAT WE HAVE LEARNED
// ==========================================================
//
// We do NOT need to study every Page or Service line-by-line.
// Most of them repeat the same pattern.
//
// ----------------------------------------------------------
//
// PAGES
//
// MyFavoritesPage.tsx
// → Studied in detail.
//
// Important pattern:
//
// Page
//   ↓
// useState()
//   ↓
// useEffect()
//   ↓
// Service
//   ↓
// API
//   ↓
// Response
//   ↓
// setState()
//   ↓
// React re-renders
//   ↓
// UI updates
//
// Other Pages
// → Usually follow the same pattern.
// → Quick scan is enough unless they introduce something NEW.
//
// ----------------------------------------------------------
//
// SERVICES
//
// UserFavoriteService.ts
// → Studied in detail.
//
// AuthService.ts
// → Studied in detail.
//
// Most other services repeat:
//
// Service
//   ↓
// api.get()
// api.post()
// api.put()
// api.delete()
//   ↓
// response.data
//
// Therefore:
// → Do NOT study every Service line-by-line.
//
// Only study a Service in detail if it introduces something NEW.
//
// ----------------------------------------------------------
//
// FRONTEND ARCHITECTURE WE HAVE NOW UNDERSTOOD:
//
// index.html
// → Starting HTML page.
//
// main.tsx
// → Starts React application.
//
// App.tsx
// → Root React component.
//
// AppRoutes.tsx
// → Decides which Page to show for each URL.
//
// MainLayout.tsx
// → Common navigation/layout.
// → <Outlet /> displays the selected Page.
//
// Page
// → Screen shown to the user.
//
// Component
// → Reusable piece of UI.
//
// Service
// → Handles communication with the backend API.
//
// axios.ts
// → Actually sends HTTP requests.
//
// Types
// → Describe the shape of data.
//
// Utils
// → Small reusable helper functions.
//
// AuthContext
// → Defines shared authentication information.
//
// AuthProvider
// → Manages user/login/logout state.
//
// ProtectedRoute
// → Checks whether user can access a protected Page.
//
// LoginPage
// → Collects login details and starts authentication.
//
// AuthService
// → Sends login request to the API.
//
// theme.ts
// → Global Material UI styling.
//
// ----------------------------------------------------------
//
// IMPORTANT:
//
// We have already learned the individual building blocks.
//
// We should now stop reading every similar file.
//
// The better next step is:
//
// TRACE ONE COMPLETE REAL FEATURE END-TO-END.
//
// Recommended example:
//
// Create Deal
//      ↓
// CreateDealPage
//      ↓
// Deal Component / Form
//      ↓
// DealService
//      ↓
// axios
//      ↓
// .NET API
//      ↓
// response
//      ↓
// React state
//      ↓
// UI update
//
// This will show how all the pieces we learned
// work TOGETHER.
//
// ----------------------------------------------------------
//
// 🔑 FINAL REMINDER:
//
// Page       = screen
// Component  = reusable UI
// Service    = API communication
// axios      = HTTP connection
// Type       = data blueprint
// Context    = shared information
// Provider   = manages/provides shared state
// Utils      = helper
// Theme      = visual styling
//
// ==========================================================