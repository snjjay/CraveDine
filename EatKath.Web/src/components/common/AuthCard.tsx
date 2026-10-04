// AuthCard = the centred, branded card used by the Login and
// Register pages (layout only - the forms keep their own logic).

import type { ReactNode } from "react";

import { Box, Paper, Typography } from "@mui/material";
import LocalDiningIcon from "@mui/icons-material/LocalDining";

interface Props {
    title: string;
    subtitle: string;
    children: ReactNode;
    // Shown under the form, e.g. a link to the other auth page.
    footer?: ReactNode;
}

function AuthCard({ title, subtitle, children, footer }: Props) {

    return (

        <Box sx={{ display: "flex", justifyContent: "center", py: { xs: 1, md: 4 } }}>

            <Paper sx={{ width: "100%", maxWidth: 460, p: { xs: 3, sm: 4 } }}>

                <Box
                    aria-hidden
                    sx={{
                        width: 44,
                        height: 44,
                        borderRadius: "12px",
                        bgcolor: "brand",
                        color: "#FFFFFF",
                        display: "grid",
                        placeItems: "center",
                        mb: 2
                    }}
                >
                    <LocalDiningIcon />
                </Box>

                <Typography variant="h4" component="h1">
                    {title}
                </Typography>

                <Typography color="text.secondary" sx={{ mt: 0.5, mb: 3 }}>
                    {subtitle}
                </Typography>

                {children}

                {footer && (
                    <Box sx={{ mt: 3, textAlign: "center", typography: "body2", color: "text.secondary" }}>
                        {footer}
                    </Box>
                )}

            </Paper>

        </Box>

    );

}

export default AuthCard;
