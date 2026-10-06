// ==========================================================
// EatKath colour tokens
// ==========================================================
//
// Single source of truth for the EatKath palette (used by theme.ts).
// Warm white surfaces, charcoal text and a coral-red brand accent used
// purposefully for brand, deals and key actions.
//
// Contrast notes (WCAG):
// - primary (#D92F3C): white text 4.75:1, on background 4.5:1
// - brand (#E23744): 4.3:1 -> logo tile, large display text only
// - dealDark (#D12B38): white text 5.1:1 -> deal badges
// - dealText (#B71F2C) on dealSoft: 5.7:1 -> small text on the tint
// - textSecondary (#686868) on background: 5.1:1
// ==========================================================

export const colors = {

    // Brand coral-red
    brand: "#E23744",
    primary: "#D92F3C",
    primaryDark: "#B71F2C",
    primaryLight: "#F0646F",
    primarySoft: "#FDECEE",

    // Deal / discount accent (same family as the brand)
    deal: "#E23744",
    dealDark: "#D12B38",
    dealText: "#B71F2C",
    dealSoft: "#FDECEE",

    // Surfaces
    background: "#FBF9F7",
    surface: "#FFFFFF",
    surfaceMuted: "#F5F2EF",
    border: "#EEEAE6",
    borderStrong: "#DED8D2",

    // Text
    textPrimary: "#1C1C1C",
    textSecondary: "#686868",
    textDisabled: "#A9A4A0",

    // Status
    success: "#1E7B4F",
    successSoft: "#E9F5EE",
    warning: "#B45309",
    error: "#C62828",
    info: "#2B5F8A",

    // Menus/dialogs, and the dark bands (footer, home hero)
    raised: "#FFFFFF",
    inverse: "#1C1C1C"

};

// ==========================================================
// Night theme tokens
// ==========================================================
//
// Warm charcoal surfaces, light text, the same coral. Contrast (WCAG)
// on surface #1F1C1A unless noted:
// - textPrimary 14.8:1 (16.2:1 on background), textSecondary 7.5:1
// - primary #F0646F 5.45:1 -> coral text, links, focus ring
// - filled coral buttons keep #D92F3C with white text (4.75:1)
// - dealText on dealSoft 6.45:1
// - borderStrong 3.8:1 -> form field outlines
// - success/warning/error/info 6.3:1 or more
// ==========================================================

export const darkColors = {

    brand: "#E23744",
    primary: "#F0646F",
    primaryFill: "#D92F3C",
    primaryFillHover: "#B71F2C",
    primarySoft: "#3B2225",

    deal: "#E23744",
    dealDark: "#D12B38",
    dealText: "#FF8A92",
    dealSoft: "#3B2225",

    background: "#151311",
    surface: "#1F1C1A",
    surfaceMuted: "#26221F",
    raised: "#2A2623",
    border: "#34302C",
    borderStrong: "#7D766F",

    textPrimary: "#F3EFEA",
    textSecondary: "#B3ABA4",
    textDisabled: "#7D766F",

    success: "#5BC891",
    successSoft: "#1C3328",
    warning: "#F0A35E",
    error: "#F27A73",
    info: "#86B8E3",

    selected: "#2E2A27",
    inverse: "#0D0B0A"

};
