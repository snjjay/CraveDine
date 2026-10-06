// AppBadge = one app download slot in the footer's "Get the CraveDine App"
// section (see APP_LISTINGS in config/siteConfig.ts):
// - listing published: the official store badge, linking to the listing;
// - not published yet: a badge-sized "Coming soon" tile that is plainly
//   not a link (no store artwork, which may only link to a live listing).

import { useState } from "react";

import { Box, Link, Typography } from "@mui/material";

import PhoneIphoneOutlinedIcon from "@mui/icons-material/PhoneIphoneOutlined";

import type { AppListing } from "../config/siteConfig";

// Official badges are shown at the same height so they line up.
const BADGE_HEIGHT = 44;

const FOCUS_SX = {
    "&:focus-visible": { outline: "2px solid", outlineColor: "primary.light", outlineOffset: 3, borderRadius: "8px" }
} as const;

function AppBadge({ app }: { app: AppListing }) {

    // If the badge image is missing, fall back to a plain text link.
    const [imageFailed, setImageFailed] = useState(false);

    if (!app.url) {

        return (

            <Box
                aria-label={`${app.storeName}: ${app.platformLabel} coming soon`}
                role="img"
                sx={{
                    height: BADGE_HEIGHT,
                    minWidth: 148,
                    px: 1.5,
                    display: "inline-flex",
                    alignItems: "center",
                    gap: 1,
                    borderRadius: "10px",
                    border: "1px dashed rgba(255, 255, 255, 0.32)",
                    color: "rgba(255, 255, 255, 0.72)",
                    cursor: "default",
                    userSelect: "none"
                }}
            >
                <PhoneIphoneOutlinedIcon aria-hidden sx={{ fontSize: 22, color: "rgba(255, 255, 255, 0.56)" }} />
                <Box aria-hidden sx={{ lineHeight: 1.15 }}>
                    <Typography component="span" sx={{ display: "block", fontSize: "0.6875rem", fontWeight: 700, letterSpacing: "0.06em", textTransform: "uppercase", color: "primary.light" }}>
                        Coming soon
                    </Typography>
                    <Typography component="span" sx={{ display: "block", fontSize: "0.875rem", fontWeight: 600, color: "#FFFFFF" }}>
                        {app.storeName}
                    </Typography>
                </Box>
            </Box>

        );

    }

    return (

        <Link
            href={app.url}
            target="_blank"
            rel="noopener noreferrer"
            aria-label={app.badgeAlt}
            sx={{ display: "inline-flex", ...FOCUS_SX }}
        >
            {imageFailed ? (
                <Box
                    component="span"
                    sx={{ height: BADGE_HEIGHT, px: 2, display: "inline-flex", alignItems: "center", borderRadius: "10px", border: "1px solid rgba(255, 255, 255, 0.5)", color: "#FFFFFF", fontWeight: 600 }}
                >
                    {app.badgeAlt}
                </Box>
            ) : (
                <Box
                    component="img"
                    src={app.badgeSrc}
                    alt={app.badgeAlt}
                    onError={() => setImageFailed(true)}
                    sx={{ height: BADGE_HEIGHT, width: "auto", display: "block" }}
                />
            )}
        </Link>

    );

}

export default AppBadge;
