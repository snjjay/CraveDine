// Footer = the CraveDine site footer: brand, app downloads, social links,
// link columns and copyright. Social, app-store and contact details come
// from config/siteConfig.ts; anything not configured is never linked.

import { Link as RouterLink } from "react-router-dom";

import {
    Box,
    Container,
    Divider,
    Grid,
    IconButton,
    Link,
    Stack,
    SvgIcon,
    Tooltip,
    Typography,
    type SvgIconProps
} from "@mui/material";

import FacebookIcon from "@mui/icons-material/Facebook";
import InstagramIcon from "@mui/icons-material/Instagram";
import LinkedInIcon from "@mui/icons-material/LinkedIn";
import LocalDiningIcon from "@mui/icons-material/LocalDining";
import XIcon from "@mui/icons-material/X";
import YouTubeIcon from "@mui/icons-material/YouTube";

import { APP_LISTINGS, SOCIAL_PROFILES, type SocialProfile } from "../config/siteConfig";
import AppBadge from "./AppBadge";
import { PAGE_CONTAINER_SX } from "./layoutConstants";

// MUI has no TikTok icon; this is the TikTok glyph (Simple Icons, CC0).
function TikTokIcon(props: SvgIconProps) {
    return (
        <SvgIcon {...props}>
            <path d="M12.53.02C13.84 0 15.14.01 16.44 0c.08 1.53.63 3.09 1.75 4.17 1.12 1.11 2.7 1.62 4.24 1.79v4.03c-1.44-.05-2.89-.35-4.2-.97-.57-.26-1.1-.59-1.62-.93-.01 2.92.01 5.84-.02 8.75-.08 1.4-.54 2.79-1.35 3.94-1.31 1.92-3.58 3.17-5.91 3.21-1.43.08-2.86-.31-4.08-1.03-2.02-1.19-3.44-3.37-3.65-5.71-.02-.5-.03-1-.01-1.49.18-1.9 1.12-3.72 2.58-4.96 1.66-1.44 3.98-2.13 6.15-1.72.02 1.48-.04 2.96-.04 4.44-.99-.32-2.15-.23-3.02.37-.63.41-1.11 1.04-1.36 1.75-.21.51-.15 1.07-.14 1.61.24 1.64 1.82 3.02 3.5 2.87 1.12-.01 2.19-.66 2.77-1.61.19-.33.4-.67.41-1.06.1-1.79.06-3.57.07-5.36.01-4.03-.01-8.05.02-12.07z" />
        </SvgIcon>
    );
}

const SOCIAL_ICONS: Record<SocialProfile["id"], typeof InstagramIcon | typeof TikTokIcon> = {
    instagram: InstagramIcon,
    facebook: FacebookIcon,
    tiktok: TikTokIcon,
    linkedin: LinkedInIcon,
    x: XIcon,
    youtube: YouTubeIcon
};

interface FooterLink {
    label: string;
    to: string;
}

const FOOTER_COLUMNS: { heading: string; links: FooterLink[] }[] = [
    {
        heading: "Explore",
        links: [
            { label: "Restaurants", to: "/restaurants" },
            { label: "Dine-in Deals", to: "/restaurants?offer=dine-in" },
            { label: "Takeaway Deals", to: "/restaurants?offer=takeaway" },
            { label: "How It Works", to: "/how-it-works" }
        ]
    },
    {
        heading: "For Restaurants",
        links: [
            { label: "Partner with Us", to: "/partner" },
            { label: "List Your Restaurant", to: "/partner#list-your-restaurant" },
            { label: "Restaurant FAQs", to: "/faqs#restaurants" },
            { label: "Contact Support", to: "/contact" }
        ]
    },
    {
        heading: "About CraveDine",
        links: [
            { label: "Our Story", to: "/our-story" },
            { label: "FAQs", to: "/faqs" },
            { label: "Contact Us", to: "/contact" }
        ]
    },
    {
        heading: "Legal",
        links: [
            { label: "Privacy Policy", to: "/privacy" },
            { label: "Terms & Conditions", to: "/terms" }
        ]
    }
];

const MUTED = "rgba(255, 255, 255, 0.72)";

// Small uppercase headings (link columns and the app section).
const COLUMN_HEADING_SX = {
    fontSize: "0.8125rem",
    fontWeight: 700,
    letterSpacing: "0.06em",
    textTransform: "uppercase",
    color: "#FFFFFF"
} as const;

// Google Play first, then the App Store.
const APP_BADGES = [APP_LISTINGS.googlePlay, APP_LISTINGS.appStore];

const FOCUS_SX = {
    "&:focus-visible": { outline: "2px solid", outlineColor: "primary.light", outlineOffset: 2, borderRadius: "4px" }
} as const;

function Footer() {

    // Pages without a #section start at the top (the destination page
    // may be the one already open, e.g. Restaurants with a filter).
    function handleLinkClick(to: string) {
        if (!to.includes("#"))
            window.scrollTo({ top: 0 });
    }

    return (

        <Box component="footer" sx={{ bgcolor: "text.primary", color: "#FFFFFF", mt: { xs: 5, md: 8 } }}>

            <Container maxWidth={false} sx={{ ...PAGE_CONTAINER_SX, py: { xs: 5, md: 7 } }}>

                <Grid container spacing={{ xs: 4, md: 6 }}>

                    {/* Brand, apps and social. Beside the link columns from
                        1200px; above them on smaller screens, so the two app
                        badges always have room to sit side by side. */}
                    <Grid size={{ xs: 12, lg: 4 }}>

                        <Box
                            component={RouterLink}
                            to="/"
                            onClick={() => handleLinkClick("/")}
                            aria-label="CraveDine home"
                            sx={{ display: "inline-flex", alignItems: "center", gap: 1, color: "#FFFFFF", textDecoration: "none", ...FOCUS_SX }}
                        >
                            <Box
                                aria-hidden
                                sx={{ width: 36, height: 36, borderRadius: "10px", bgcolor: "brand", display: "grid", placeItems: "center" }}
                            >
                                <LocalDiningIcon sx={{ fontSize: 21 }} />
                            </Box>
                            <Typography component="span" sx={{ fontFamily: "h6.fontFamily", fontWeight: 700, fontSize: "1.3125rem", letterSpacing: "-0.02em" }}>
                                Crave<Box component="span" sx={{ color: "primary.light" }}>Dine</Box>
                            </Typography>
                        </Box>

                        <Typography sx={{ mt: 2, color: MUTED, maxWidth: 360, lineHeight: 1.7 }}>
                            Discover restaurants and walk-in dine-in and takeaway offers across Kathmandu.
                        </Typography>

                        {/* App downloads: official badges for published listings,
                            "Coming soon" tiles otherwise (see APP_LISTINGS) */}
                        <Box component="section" aria-labelledby="footer-app-heading" sx={{ mt: 3.5 }}>
                            <Typography component="h2" id="footer-app-heading" sx={{ ...COLUMN_HEADING_SX, mb: 1.5 }}>
                                Get the CraveDine App
                            </Typography>
                            <Stack direction="row" spacing={1.5} useFlexGap sx={{ flexWrap: "wrap" }}>
                                {APP_BADGES.map(app => (
                                    <AppBadge key={app.storeName} app={app} />
                                ))}
                            </Stack>
                        </Box>

                        {/* Social profiles: only configured ones are links */}
                        <Stack direction="row" spacing={0.5} sx={{ mt: 2.5, ml: -1 }} aria-label="CraveDine on social media">
                            {SOCIAL_PROFILES.map(profile => {

                                const Icon = SOCIAL_ICONS[profile.id];

                                return profile.url ? (
                                    <IconButton
                                        key={profile.id}
                                        component="a"
                                        href={profile.url}
                                        target="_blank"
                                        rel="noopener noreferrer"
                                        aria-label={`CraveDine on ${profile.label}`}
                                        sx={{ color: "#FFFFFF", "&:hover": { color: "primary.light", bgcolor: "rgba(255, 255, 255, 0.08)" }, ...FOCUS_SX }}
                                    >
                                        <Icon fontSize="small" />
                                    </IconButton>
                                ) : (
                                    <Tooltip key={profile.id} title={`${profile.label} - coming soon`}>
                                        <Box
                                            component="span"
                                            role="img"
                                            aria-label={`${profile.label} (coming soon)`}
                                            sx={{ width: 40, height: 40, display: "inline-grid", placeItems: "center", color: "rgba(255, 255, 255, 0.38)" }}
                                        >
                                            <Icon fontSize="small" />
                                        </Box>
                                    </Tooltip>
                                );

                            })}
                        </Stack>

                    </Grid>

                    {/* Link columns */}
                    <Grid size={{ xs: 12, lg: 8 }}>
                        <Box component="nav" aria-label="Footer">
                            <Grid container spacing={{ xs: 3.5, sm: 3 }}>
                                {FOOTER_COLUMNS.map(column => (
                                    <Grid key={column.heading} size={{ xs: 6, sm: 3 }}>
                                        <Typography
                                            component="h2"
                                            sx={{ ...COLUMN_HEADING_SX, mb: 1.5 }}
                                        >
                                            {column.heading}
                                        </Typography>
                                        <Stack component="ul" spacing={1.25} sx={{ listStyle: "none", m: 0, p: 0 }}>
                                            {column.links.map(link => (
                                                <li key={link.to}>
                                                    <Link
                                                        component={RouterLink}
                                                        to={link.to}
                                                        onClick={() => handleLinkClick(link.to)}
                                                        underline="hover"
                                                        sx={{ color: MUTED, fontSize: "0.9375rem", "&:hover": { color: "#FFFFFF" }, ...FOCUS_SX }}
                                                    >
                                                        {link.label}
                                                    </Link>
                                                </li>
                                            ))}
                                        </Stack>
                                    </Grid>
                                ))}
                            </Grid>
                        </Box>
                    </Grid>

                </Grid>

                <Divider sx={{ borderColor: "rgba(255, 255, 255, 0.12)", mt: { xs: 4, md: 6 }, mb: 3 }} />

                <Stack direction={{ xs: "column", sm: "row" }} spacing={1} sx={{ justifyContent: "space-between", color: MUTED }}>
                    <Typography variant="body2">
                        © 2026 CraveDine. All rights reserved.
                    </Typography>
                    <Typography variant="body2">
                        Made for food lovers in Kathmandu.
                    </Typography>
                </Stack>

            </Container>

        </Box>

    );

}

export default Footer;
