// RestaurantMenu = the customer's native digital menu.
//
// - Dine In / Takeaway tabs, scrollable category navigation, items.
// - When a deal for the selected tab applies today, prices show the
//   original price struck through and the discounted price, with a
//   banner naming the deal's time window. This is a preview only -
//   the real discount is applied to the bill when the offer is
//   redeemed (see utils/menuPricing.ts).
// - With no digital menu items, shows "Menu coming soon" and the
//   uploaded PDF menu (if any) as a fallback.

import { useEffect, useMemo, useState } from "react";

import {
    Box,
    Button,
    Card,
    CardContent,
    Chip,
    Stack,
    Tab,
    Tabs,
    Typography
} from "@mui/material";

import LocalOfferOutlinedIcon from "@mui/icons-material/LocalOfferOutlined";
import PictureAsPdfOutlinedIcon from "@mui/icons-material/PictureAsPdfOutlined";
import RestaurantMenuIcon from "@mui/icons-material/RestaurantMenu";

// Heights of the sticky site header (see MainLayout) and of this
// menu's sticky tabs + category bar, used for sticky offsets and for
// scrolling a category into view below them.
const SITE_HEADER_HEIGHT = { xs: 60, md: 68 };
// (Desktop allows for the category chips wrapping onto a second row.)
const CATEGORY_SCROLL_MARGIN = { xs: 60 + 112, md: 68 + 152 };

// Text read by screen readers only (e.g. "Original price"). Sizes are
// strings: in sx a bare 1 means 100%, which made the span overflow.
const visuallyHidden = {
    position: "absolute",
    width: "1px",
    height: "1px",
    overflow: "hidden",
    clip: "rect(0 0 0 0)",
    whiteSpace: "nowrap"
} as const;

import type { Deal } from "../../types/Deal";
import type { MenuCategory } from "../../types/MenuCategory";
import type { MenuItem } from "../../types/MenuItem";
import { formatCurrency } from "../../utils/currency";
import { getImageUrl } from "../../utils/imageUrl";
import {
    MenuTab,
    calculateDiscountedPrice,
    selectApplicableDeal,
    type MenuTabValue
} from "../../utils/menuPricing";
import { formatTime12Hour } from "../../utils/time";

interface Props {
    categories: MenuCategory[];
    items: MenuItem[];
    deals: Deal[];
    currencyCode: string;
    menuPdfUrl?: string | null;
}

const TAB_WORDING: Record<MenuTabValue, string> = {
    [MenuTab.DineIn]: "dine-in",
    [MenuTab.Takeaway]: "takeaway"
};

function categorySectionId(categoryId: number) {

    return `menu-category-${categoryId}`;

}

function RestaurantMenu({
    categories,
    items,
    deals,
    currencyCode,
    menuPdfUrl
}: Props) {

    const [tab, setTab] = useState<MenuTabValue>(MenuTab.DineIn);
    const [selectedCategoryId, setSelectedCategoryId] = useState<number | null>(null);
    const [now, setNow] = useState(() => new Date());

    // Re-check every minute so a deal switches on/off at its window.
    useEffect(() => {

        const timer = setInterval(() => setNow(new Date()), 60_000);

        return () => clearInterval(timer);

    }, []);

    // Categories in display order, with their available items;
    // empty categories are left out.
    const sections = useMemo(
        () => [...categories]
            .sort((a, b) => a.displayOrder - b.displayOrder)
            .map(category => ({
                category,
                items: items.filter(item =>
                    item.menuCategoryId === category.id &&
                    item.isAvailable
                )
            }))
            .filter(section => section.items.length > 0),
        [categories, items]
    );

    const applicable = selectApplicableDeal(deals, tab, now);

    // Shown on Takeaway when only a dine-in offer exists (and vice
    // versa), so customers know why prices differ between tabs.
    const otherTab = tab === MenuTab.DineIn ? MenuTab.Takeaway : MenuTab.DineIn;
    const otherTabDeal = applicable ? null : selectApplicableDeal(deals, otherTab, now);

    function scrollToCategory(categoryId: number) {

        setSelectedCategoryId(categoryId);

        document
            .getElementById(categorySectionId(categoryId))
            ?.scrollIntoView({ behavior: "smooth", block: "start" });

    }

    // -----------------------------
    // No digital menu yet
    // -----------------------------
    if (sections.length === 0) {

        return (

            <Card>

                <CardContent sx={{ textAlign: "center", py: 5 }}>

                    <RestaurantMenuIcon aria-hidden sx={{ fontSize: 36, color: "text.disabled" }} />

                    <Typography variant="h6" component="p" sx={{ mt: 1 }}>
                        Menu coming soon
                    </Typography>

                    <Typography color="text.secondary" sx={{ mt: 0.5 }}>
                        This restaurant hasn't added its digital menu yet.
                    </Typography>

                    {menuPdfUrl && (

                        <Button
                            variant="outlined"
                            href={getImageUrl(menuPdfUrl)}
                            target="_blank"
                            rel="noopener noreferrer"
                            startIcon={<PictureAsPdfOutlinedIcon />}
                            sx={{ mt: 2 }}
                        >
                            View PDF Menu
                        </Button>

                    )}

                </CardContent>

            </Card>

        );

    }

    // -----------------------------
    // Digital menu
    // -----------------------------
    return (
        // overflow-x: clip (not hidden) keeps the tab/category bar pinned
        // while scrolling and stops wide content causing page overflow.
        <Card sx={{ overflowX: "clip", overflowY: "visible" }}>

            {/* Sticky tabs + category navigation */}
            <Box
                sx={{
                    position: "sticky",
                    top: SITE_HEADER_HEIGHT,
                    zIndex: 2,
                    bgcolor: "background.paper",
                    borderBottom: "1px solid",
                    borderColor: "divider",
                    borderTopLeftRadius: "12px",
                    borderTopRightRadius: "12px"
                }}
            >

                <Tabs
                    value={tab}
                    onChange={(_, value: MenuTabValue) => setTab(value)}
                    variant="fullWidth"
                    aria-label="Menu prices for"
                    sx={{ borderBottom: "1px solid", borderColor: "divider" }}
                >
                    <Tab label="Dine In" value={MenuTab.DineIn} />
                    <Tab label="Takeaway" value={MenuTab.Takeaway} />
                </Tabs>

                {/* Horizontally scrollable category navigation */}
                <Box
                    component="nav"
                    aria-label="Menu categories"
                    sx={{
                        display: "flex",
                        gap: 1,
                        // Phones: swipe sideways (no visible scrollbar).
                        // Desktop: chips wrap, so no inner scrollbar at all.
                        flexWrap: { xs: "nowrap", md: "wrap" },
                        overflowX: { xs: "auto", md: "visible" },
                        px: { xs: 2, sm: 2.5 },
                        py: 1.5,
                        scrollbarWidth: "none",
                        "&::-webkit-scrollbar": { display: "none" }
                    }}
                >
                    {sections.map(({ category }) => {

                        const selected = selectedCategoryId === category.id;

                        return (
                            <Chip
                                key={category.id}
                                label={category.name}
                                clickable
                                color={selected ? "primary" : "default"}
                                variant={selected ? "filled" : "outlined"}
                                onClick={() => scrollToCategory(category.id)}
                                aria-current={selected ? "true" : undefined}
                                sx={{ flexShrink: 0, height: 34 }}
                            />
                        );

                    })}
                </Box>

            </Box>

            <CardContent sx={{ px: { xs: 2, sm: 2.5 } }}>

                {otherTabDeal && (
                    <Typography variant="body2" color="text.secondary" sx={{ mb: 2 }}>
                        The {TAB_WORDING[otherTab]} offer doesn't apply to {TAB_WORDING[tab]} orders,
                        so {TAB_WORDING[tab]} prices are shown without a discount.
                    </Typography>
                )}

                <Stack spacing={4}>

                    {sections.map(({ category, items: categoryItems }) => (

                        <Box
                            key={category.id}
                            id={categorySectionId(category.id)}
                            component="section"
                            aria-label={category.name}
                            sx={{ scrollMarginTop: CATEGORY_SCROLL_MARGIN }}
                        >

                            <Stack direction="row" spacing={1} sx={{ alignItems: "baseline", mb: 0.5 }}>
                                <Typography variant="h6" component="h3">
                                    {category.name}
                                </Typography>
                                <Typography variant="caption" color="text.secondary">
                                    {categoryItems.length} {categoryItems.length === 1 ? "item" : "items"}
                                </Typography>
                            </Stack>

                            {/* One column on phones/tablets, two on large screens */}
                            <Box
                                sx={{
                                    display: "grid",
                                    gridTemplateColumns: { xs: "minmax(0, 1fr)", lg: "repeat(2, minmax(0, 1fr))" },
                                    columnGap: 4
                                }}
                            >

                                {categoryItems.map(item => {

                                    const discountedPrice = applicable
                                        ? calculateDiscountedPrice(item.price, applicable.deal.discountPercentage)
                                        : null;

                                    return (

                                        <Stack
                                            key={item.id}
                                            direction="row"
                                            spacing={2}
                                            sx={{
                                                py: 2,
                                                alignItems: "flex-start",
                                                borderBottom: "1px solid",
                                                borderColor: "divider"
                                            }}
                                        >

                                            <Box sx={{ flex: 1, minWidth: 0 }}>

                                                <Typography sx={{ fontWeight: 600, lineHeight: 1.4, overflowWrap: "anywhere" }}>
                                                    {item.name}
                                                </Typography>

                                                {item.isFeatured && (
                                                    <Box
                                                        component="span"
                                                        sx={{
                                                            display: "inline-block",
                                                            mt: 0.5,
                                                            px: 0.75,
                                                            borderRadius: "6px",
                                                            bgcolor: "primarySoft",
                                                            // deal.text: the readable red for small text on this tint
                                                            color: "deal.text",
                                                            fontSize: "0.6875rem",
                                                            fontWeight: 700,
                                                            lineHeight: 1.7
                                                        }}
                                                    >
                                                        Featured
                                                    </Box>
                                                )}

                                                {item.description && (
                                                    <Typography
                                                        variant="body2"
                                                        color="text.secondary"
                                                        sx={{
                                                            mt: 0.25,
                                                            display: "-webkit-box",
                                                            WebkitLineClamp: 3,
                                                            overflowWrap: "anywhere",
                                                            WebkitBoxOrient: "vertical",
                                                            overflow: "hidden"
                                                        }}
                                                    >
                                                        {item.description}
                                                    </Typography>
                                                )}

                                                <Stack direction="row" spacing={1} sx={{ mt: 0.75, alignItems: "baseline", flexWrap: "wrap" }}>

                                                    {discountedPrice !== null ? (
                                                        <>
                                                            <Typography
                                                                component="span"
                                                                sx={theme => ({
                                                                    fontWeight: 700,
                                                                    color: "deal.dark",
                                                                    ...theme.applyStyles("dark", { color: (theme.vars || theme).palette.deal.text })
                                                                })}
                                                            >
                                                                {formatCurrency(discountedPrice, currencyCode)}
                                                            </Typography>
                                                            <Typography
                                                                component="span"
                                                                variant="body2"
                                                                color="text.secondary"
                                                                sx={{ textDecoration: "line-through" }}
                                                            >
                                                                <Box component="span" sx={visuallyHidden}>Original price </Box>
                                                                {formatCurrency(item.price, currencyCode)}
                                                            </Typography>
                                                        </>
                                                    ) : (
                                                        <Typography component="span" sx={{ fontWeight: 700 }}>
                                                            {formatCurrency(item.price, currencyCode)}
                                                        </Typography>
                                                    )}

                                                </Stack>

                                            </Box>

                                            {item.imageUrl && (
                                                <Box
                                                    component="img"
                                                    src={getImageUrl(item.imageUrl)}
                                                    alt={item.name}
                                                    loading="lazy"
                                                    sx={{
                                                        width: { xs: 80, sm: 96 },
                                                        height: { xs: 80, sm: 96 },
                                                        objectFit: "cover",
                                                        borderRadius: "10px",
                                                        border: "1px solid",
                                                        borderColor: "divider",
                                                        flexShrink: 0
                                                    }}
                                                />
                                            )}

                                        </Stack>

                                    );

                                })}

                            </Box>

                        </Box>

                    ))}

                </Stack>

            </CardContent>

            {/* Deal banner - stays visible at the bottom while scrolling the menu */}
            {applicable && (

                <Box sx={{ position: "sticky", bottom: 0, p: { xs: 1.5, sm: 2 }, pt: 0, zIndex: 1 }}>

                    <Stack
                        direction="row"
                        spacing={1.5}
                        role="note"
                        sx={{
                            alignItems: "flex-start",
                            bgcolor: "deal.dark",
                            color: "deal.contrastText",
                            borderRadius: "12px",
                            px: 2,
                            py: 1.5,
                            boxShadow: 3
                        }}
                    >

                        <LocalOfferOutlinedIcon aria-hidden sx={{ mt: 0.25 }} />

                        <Box sx={{ minWidth: 0, overflowWrap: "anywhere" }}>

                            <Typography sx={{ fontWeight: 700, color: "inherit" }}>
                                {applicable.deal.discountPercentage}% off {TAB_WORDING[tab]} · {formatTime12Hour(applicable.deal.startTime)} – {formatTime12Hour(applicable.deal.endTime)}
                            </Typography>

                            <Typography variant="body2" sx={{ color: "inherit" }}>
                                {applicable.timing === "running"
                                    ? "Available now."
                                    : `Starts today at ${formatTime12Hour(applicable.deal.startTime)}.`}
                                {" "}Prices shown are a preview: the discount applies to your bill when you redeem this offer and arrive within its time window.
                            </Typography>

                        </Box>

                    </Stack>

                </Box>

            )}

        </Card>

    );

}

export default RestaurantMenu;
