// ==========================================================
// CraveDine public site details (footer, Contact Us page)
// ==========================================================
//
// Fill these in with VERIFIED details before publishing. Anything left
// null is never linked: social icons show as "coming soon", the app
// badges are replaced by a "coming soon" note, and the Contact page
// says the detail will be published soon.
//
// Do not add placeholder or guessed URLs here.
// ==========================================================

export interface SocialProfile {
    id: "instagram" | "facebook" | "tiktok" | "linkedin" | "x" | "youtube";
    label: string;
    url: string | null;
}

// App download badges ("Get the CraveDine App" in the footer).
//
// To activate a badge once the app is published:
//   1. set `url` to the verified store listing URL;
//   2. add the OFFICIAL badge artwork, unaltered, at `badgeSrc` in /public:
//      - App Store: "Download on the App Store" badge from Apple's
//        marketing resources (developer.apple.com/app-store/marketing/guidelines)
//      - Google Play: "Get it on Google Play" badge from
//        play.google.com/intl/en_us/badges
// Without a url, the footer shows a "Coming soon" tile instead (store
// guidelines only allow the official badges to link to a live listing).
export interface AppListing {
    storeName: string;
    // Shown in the "Coming soon" tile, e.g. "Android app".
    platformLabel: string;
    // Verified store listing URL, or null while the app isn't published.
    url: string | null;
    // Official badge artwork in /public.
    badgeSrc: string;
    badgeAlt: string;
}

export const SOCIAL_PROFILES: SocialProfile[] = [
    { id: "instagram", label: "Instagram", url: null },
    { id: "facebook", label: "Facebook", url: null },
    { id: "tiktok", label: "TikTok", url: null },
    { id: "linkedin", label: "LinkedIn", url: null },
    { id: "x", label: "X", url: null },
    { id: "youtube", label: "YouTube", url: null }
];

export const APP_LISTINGS: { googlePlay: AppListing; appStore: AppListing } = {
    googlePlay: {
        storeName: "Google Play",
        platformLabel: "Android app",
        url: null,
        badgeSrc: "/badges/google-play-badge.png",
        badgeAlt: "Get CraveDine on Google Play"
    },
    appStore: {
        storeName: "App Store",
        platformLabel: "iPhone app",
        url: null,
        badgeSrc: "/badges/app-store-badge.svg",
        badgeAlt: "Download CraveDine on the App Store"
    }
};

export const CONTACT_DETAILS: {
    supportEmail: string | null;
    partnershipsEmail: string | null;
    phone: string | null;
    // Office or postal address, if one will be published.
    address: string | null;
    // e.g. "Sunday to Friday, 10:00 AM - 6:00 PM (Nepal time)"
    supportHours: string | null;
} = {
    supportEmail: null,
    partnershipsEmail: null,
    phone: null,
    address: null,
    supportHours: null
};
