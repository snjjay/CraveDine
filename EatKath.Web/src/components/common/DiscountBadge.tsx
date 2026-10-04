// DiscountBadge = the coral offer badge used on restaurant cards,
// the restaurant hero and deal cards.
//
// - "solid": white text on the darker coral (4.6:1) - for photos.
// - "soft":  dark coral text on the soft coral tint (5.6:1).

import { Box } from "@mui/material";
import LocalOfferOutlinedIcon from "@mui/icons-material/LocalOfferOutlined";

interface Props {
    // Text to show, e.g. "Up to 25% off" or "25% off".
    label: string;
    variant?: "solid" | "soft";
    size?: "small" | "medium";
}

function DiscountBadge({ label, variant = "solid", size = "small" }: Props) {

    const solid = variant === "solid";

    return (

        <Box
            component="span"
            sx={{
                display: "inline-flex",
                alignItems: "center",
                gap: 0.5,
                px: size === "small" ? 1 : 1.25,
                py: size === "small" ? 0.375 : 0.5,
                borderRadius: "8px",
                bgcolor: solid ? "deal.dark" : "deal.soft",
                color: solid ? "deal.contrastText" : "deal.text",
                fontWeight: 700,
                fontSize: size === "small" ? "0.75rem" : "0.8125rem",
                lineHeight: 1.4,
                whiteSpace: "nowrap"
            }}
        >
            <LocalOfferOutlinedIcon aria-hidden sx={{ fontSize: size === "small" ? 14 : 16 }} />
            {label}
        </Box>

    );

}

export default DiscountBadge;
