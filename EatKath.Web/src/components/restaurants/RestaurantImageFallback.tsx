import { Box } from "@mui/material";
import LocalDiningIcon from "@mui/icons-material/LocalDining";

interface Props {
    // "hero" = the large header image on the restaurant page.
    size?: "card" | "hero";
}

// Shown where a restaurant has no photo (or its photo fails to load):
// a soft CraveDine tile instead of an unrelated stock image. Fills its
// container; decorative (the restaurant name is always shown nearby).
function RestaurantImageFallback({ size = "card" }: Props) {

    const badge = size === "hero" ? 88 : 56;

    return (

        <Box
            aria-hidden
            data-image-fallback
            sx={theme => ({
                width: "100%",
                height: "100%",
                display: "flex",
                alignItems: "center",
                justifyContent: "center",
                // On the hero, sit above the name/details at the bottom.
                pb: size === "hero" ? { xs: 12, md: 10 } : 0,
                background: `linear-gradient(135deg, ${(theme.vars || theme).palette.primarySoft} 0%, ${(theme.vars || theme).palette.surfaceMuted} 100%)`
            })}
        >

            <Box
                sx={{
                    width: badge,
                    height: badge,
                    borderRadius: "50%",
                    bgcolor: "brand",
                    color: "#FFFFFF",
                    display: "grid",
                    placeItems: "center",
                    boxShadow: 2
                }}
            >
                <LocalDiningIcon sx={{ fontSize: badge / 2 }} />
            </Box>

        </Box>

    );

}

export default RestaurantImageFallback;
