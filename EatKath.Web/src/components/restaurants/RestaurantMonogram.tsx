import { Box } from "@mui/material";
import type { SxProps, Theme } from "@mui/material";

interface Props {
    name: string;
    sx?: SxProps<Theme>;
}

// First letter of the name (e.g. "1Chef's Table" -> "C"), or its first
// character when it has no letters.
function initialOf(name: string): string {

    const letter = name.match(/\p{L}/u)?.[0];

    return (letter ?? name.trim().charAt(0) ?? "").toUpperCase() || "?";

}

// CraveDine coral initial tile, used instead of a logo when a restaurant
// has none. Decorative: the restaurant name is always shown next to it.
// Size it through sx (width/height); the letter scales with it.
function RestaurantMonogram({ name, sx }: Props) {

    return (

        <Box
            aria-hidden
            data-monogram
            sx={[
                {
                    width: 64,
                    height: 64,
                    flexShrink: 0,
                    borderRadius: "12px",
                    bgcolor: "brand",
                    color: "#FFFFFF",
                    display: "grid",
                    placeItems: "center",
                    fontFamily: '"Outfit", "Inter", sans-serif',
                    fontWeight: 700,
                    fontSize: "2rem",
                    lineHeight: 1,
                    userSelect: "none"
                },
                ...(Array.isArray(sx) ? sx : [sx])
            ]}
        >
            {initialOf(name)}
        </Box>

    );

}

export default RestaurantMonogram;
