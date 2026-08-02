import { createTheme } from "@mui/material/styles";
import { colors } from "./colors";

export const theme = createTheme({
    palette: {
        primary: {
            main: colors.primary
        },
        secondary: {
            main: colors.secondary
        },
        background: {
            default: colors.background
        }
    },

    typography: {
        fontFamily: "Roboto, Arial, sans-serif",

        h4: {
            fontWeight: 700
        },

        h5: {
            fontWeight: 600
        },

        button: {
            textTransform: "none"
        }
    }
});