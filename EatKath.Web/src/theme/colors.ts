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
    info: "#2B5F8A"

};
