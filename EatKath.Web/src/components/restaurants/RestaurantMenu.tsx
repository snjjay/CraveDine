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
    Alert,
    Box,
    Button,
    Card,
    CardContent,
    Chip,
    Divider,
    Stack,
    Tab,
    Tabs,
    Typography
} from "@mui/material";

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

                <CardContent>

                    <Typography variant="h6">
                        Menu coming soon
                    </Typography>

                    <Typography color="text.secondary" sx={{ mt: 1 }}>
                        This restaurant hasn't added its digital menu yet.
                    </Typography>

                    {menuPdfUrl && (

                        <Button
                            variant="outlined"
                            href={getImageUrl(menuPdfUrl)}
                            target="_blank"
                            rel="noopener noreferrer"
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

        <Card>

            <Tabs
                value={tab}
                onChange={(_, value: MenuTabValue) => setTab(value)}
                variant="fullWidth"
            >
                <Tab label="Dine In" value={MenuTab.DineIn} />
                <Tab label="Takeaway" value={MenuTab.Takeaway} />
            </Tabs>

            <Divider />

            {/* Horizontally scrollable category navigation */}
            <Box
                sx={{
                    display: "flex",
                    gap: 1,
                    overflowX: "auto",
                    px: 2,
                    py: 1.5,
                    "&::-webkit-scrollbar": { height: 6 }
                }}
            >
                {sections.map(({ category }) => (
                    <Chip
                        key={category.id}
                        label={category.name}
                        clickable
                        color={selectedCategoryId === category.id ? "primary" : "default"}
                        variant={selectedCategoryId === category.id ? "filled" : "outlined"}
                        onClick={() => scrollToCategory(category.id)}
                        sx={{ flexShrink: 0 }}
                    />
                ))}
            </Box>

            <Divider />

            <CardContent>

                {otherTabDeal && (
                    <Typography variant="body2" color="text.secondary" sx={{ mb: 2 }}>
                        The {TAB_WORDING[otherTab]} offer doesn't apply to {TAB_WORDING[tab]} orders,
                        so {TAB_WORDING[tab]} prices are shown without a discount.
                    </Typography>
                )}

                <Stack spacing={3}>

                    {sections.map(({ category, items: categoryItems }) => (

                        <Box
                            key={category.id}
                            id={categorySectionId(category.id)}
                            sx={{ scrollMarginTop: 16 }}
                        >

                            <Typography variant="h6" sx={{ mb: 1 }}>
                                {category.name}
                            </Typography>

                            <Stack divider={<Divider flexItem />}>

                                {categoryItems.map(item => {

                                    const discountedPrice = applicable
                                        ? calculateDiscountedPrice(item.price, applicable.deal.discountPercentage)
                                        : null;

                                    return (

                                        <Stack
                                            key={item.id}
                                            direction="row"
                                            spacing={2}
                                            sx={{ py: 1.5, alignItems: "flex-start" }}
                                        >

                                            <Box sx={{ flex: 1, minWidth: 0 }}>

                                                <Typography sx={{ fontWeight: 600 }}>
                                                    {item.isFeatured && "⭐ "}
                                                    {item.name}
                                                </Typography>

                                                {item.description && (
                                                    <Typography variant="body2" color="text.secondary">
                                                        {item.description}
                                                    </Typography>
                                                )}

                                                <Box sx={{ mt: 0.5 }}>

                                                    {discountedPrice !== null ? (
                                                        <>
                                                            <Typography
                                                                component="span"
                                                                color="text.secondary"
                                                                sx={{ textDecoration: "line-through", mr: 1 }}
                                                            >
                                                                {formatCurrency(item.price, currencyCode)}
                                                            </Typography>
                                                            <Typography
                                                                component="span"
                                                                sx={{ fontWeight: 600, color: "success.main" }}
                                                            >
                                                                {formatCurrency(discountedPrice, currencyCode)}
                                                            </Typography>
                                                        </>
                                                    ) : (
                                                        <Typography component="span" sx={{ fontWeight: 600 }}>
                                                            {formatCurrency(item.price, currencyCode)}
                                                        </Typography>
                                                    )}

                                                </Box>

                                            </Box>

                                            {item.imageUrl && (
                                                <Box
                                                    component="img"
                                                    src={getImageUrl(item.imageUrl)}
                                                    alt={item.name}
                                                    loading="lazy"
                                                    sx={{
                                                        width: { xs: 72, sm: 96 },
                                                        height: { xs: 72, sm: 96 },
                                                        objectFit: "cover",
                                                        borderRadius: 1,
                                                        flexShrink: 0
                                                    }}
                                                />
                                            )}

                                        </Stack>

                                    );

                                })}

                            </Stack>

                        </Box>

                    ))}

                </Stack>

            </CardContent>

            {/* Deal banner - stays visible at the bottom while scrolling the menu */}
            {applicable && (

                <Box sx={{ position: "sticky", bottom: 0, p: 2, pt: 0, zIndex: 1 }}>

                    <Alert severity="success" variant="filled">

                        <Typography sx={{ fontWeight: 600 }}>
                            {applicable.deal.discountPercentage}% off {TAB_WORDING[tab]} · {formatTime12Hour(applicable.deal.startTime)} – {formatTime12Hour(applicable.deal.endTime)}
                        </Typography>

                        <Typography variant="body2">
                            {applicable.timing === "running"
                                ? "Available now."
                                : `Starts today at ${formatTime12Hour(applicable.deal.startTime)}.`}
                            {" "}Prices shown are a preview: the discount applies to your bill when you redeem this offer and arrive within its time window.
                        </Typography>

                    </Alert>

                </Box>

            )}

        </Card>

    );

}

export default RestaurantMenu;
