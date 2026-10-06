import { useContext, useState } from "react";
import { useNavigate } from "react-router-dom";

import {
    Box,
    Button,
    Card,
    Chip,
    Stack,
    Typography
} from "@mui/material";

import EventOutlinedIcon from "@mui/icons-material/EventOutlined";
import ScheduleIcon from "@mui/icons-material/Schedule";
import ConfirmationNumberOutlinedIcon from "@mui/icons-material/ConfirmationNumberOutlined";

import RedeemOfferDialog from "../redemptions/RedeemOfferDialog";
import AuthContext from "../../features/auth/AuthContext";
import { useNotification } from "../../features/notifications/NotificationContext";
import type { Deal } from "../../types/Deal";
import { getDealHeadline, getDealSummary } from "../../utils/deal";
import { getOfferTypeLabel } from "../../utils/offerType";
import { formatDate, formatTime12Hour, todayIsoDate } from "../../utils/time";

interface Props {
    deal: Deal;
    // Called after a successful claim (e.g. to refresh "X left").
    onRedeemed?: () => void;
}

function toLocalDate(date: string): Date {

    const [year, month, day] = date.split("-").map(Number);

    return new Date(year, month - 1, day);

}

// "2026-10-04", "2026-10-10" -> "Oct 4 - Oct 10, 2026"
// Different years -> "Dec 28, 2026 - Jan 3, 2027"
function formatCardDateRange(startDate: string, endDate: string): string {

    const start = toLocalDate(startDate);
    const end = toLocalDate(endDate);

    const monthDay: Intl.DateTimeFormatOptions = { month: "short", day: "numeric" };
    const monthDayYear: Intl.DateTimeFormatOptions = { ...monthDay, year: "numeric" };

    const startText = start.toLocaleDateString(
        "en-US",
        start.getFullYear() === end.getFullYear() ? monthDay : monthDayYear
    );

    return `${startText} - ${end.toLocaleDateString("en-US", monthDayYear)}`;

}

function DealCard({ deal, onRedeemed }: Props) {

    const navigate = useNavigate();
    const { notify } = useNotification();

    const auth = useContext(AuthContext);
    const user = auth?.user ?? null;

    const [open, setOpen] = useState(false);

    // e.g. "25% Off - Dine In" / "Enjoy 25% off your dine-in bill"
    const headline = getDealHeadline(deal);
    const summary = getDealSummary(deal);

    // Availability comes from the restaurant deal list:
    // availabilityDate null + remainingOffers 0 = offer ended.
    const hasEnded =
        deal.availabilityDate === null && deal.remainingOffers === 0;

    const remaining = deal.remainingOffers;

    const forDate =
        deal.availabilityDate && deal.availabilityDate !== todayIsoDate()
            ? `on ${formatDate(deal.availabilityDate)}`
            : "today";

    let availabilityLabel: string | null = null;
    let availabilityColor = "text.secondary";

    if (hasEnded) {
        availabilityLabel = "Offer ended";
    }
    else if (deal.isSoldOut) {
        availabilityLabel = "Sold out";
        availabilityColor = "error.main";
    }
    else if (remaining === 0) {
        // Daily limit reached - other dates can still be chosen.
        availabilityLabel = `No offers left ${forDate}`;
        availabilityColor = "warning.main";
    }
    else if (remaining != null) {
        availabilityLabel = `${remaining} ${remaining === 1 ? "offer" : "offers"} left ${forDate}`;
        availabilityColor = remaining <= 3 ? "warning.main" : "success.main";
    }

    // Only logged-in customers can redeem. Visitors are sent to log in.
    function handleRedeemClick() {

        if (!user) {
            navigate("/login");
            return;
        }

        if (user.role !== "Customer") {
            notify("Please log in with a customer account to redeem offers.", "info");
            return;
        }

        setOpen(true);

    }

    const canRedeem = deal.isActive && !hasEnded && !deal.isSoldOut;

    return (
        <>
            {/* Coupon-style offer card: discount panel | details */}
            <Card
                component="article"
                aria-label={headline}
                sx={{ height: "100%", display: "flex", alignItems: "stretch" }}
            >

                <Box
                    sx={{
                        width: { xs: 92, sm: 112 },
                        flexShrink: 0,
                        bgcolor: "deal.soft",
                        borderRight: "2px dashed",
                        borderColor: "rgba(209, 43, 56, 0.28)",
                        display: "flex",
                        flexDirection: "column",
                        alignItems: "center",
                        justifyContent: "center",
                        textAlign: "center",
                        px: 1,
                        py: 2
                    }}
                >
                    <Typography
                        component="p"
                        sx={theme => ({
                            fontFamily: "h1.fontFamily",
                            fontWeight: 800,
                            fontSize: { xs: "1.75rem", sm: "2rem" },
                            lineHeight: 1,
                            letterSpacing: "-0.03em",
                            color: "deal.dark",
                            ...theme.applyStyles("dark", { color: (theme.vars || theme).palette.deal.text })
                        })}
                    >
                        {deal.discountPercentage}%
                    </Typography>
                    <Typography variant="overline" sx={{ color: "deal.text", lineHeight: 1.8 }}>
                        Off
                    </Typography>
                    <Typography variant="caption" sx={{ color: "deal.text", fontWeight: 600 }}>
                        {getOfferTypeLabel(deal.offerType)}
                    </Typography>
                </Box>

                <Stack spacing={1} sx={{ p: { xs: 2, sm: 2.5 }, flex: 1, minWidth: 0 }}>

                    <Typography variant="subtitle1" component="h3" sx={{ fontWeight: 700 }}>
                        {summary}
                    </Typography>

                    <Stack direction="row" spacing={1} sx={{ alignItems: "center", color: "text.primary" }}>
                        <ScheduleIcon aria-hidden sx={{ fontSize: 18, color: "text.secondary" }} />
                        <Typography variant="body2" sx={{ fontWeight: 500 }}>
                            Arrive {formatTime12Hour(deal.startTime)} – {formatTime12Hour(deal.endTime)}
                        </Typography>
                    </Stack>

                    <Stack direction="row" spacing={1} sx={{ alignItems: "center" }}>
                        <EventOutlinedIcon aria-hidden sx={{ fontSize: 18, color: "text.secondary" }} />
                        <Typography variant="body2" color="text.secondary">
                            Valid {formatCardDateRange(deal.startDate, deal.endDate)}
                        </Typography>
                    </Stack>

                    {deal.isActive ? (

                        <>

                            {availabilityLabel && (
                                <Stack direction="row" spacing={1} sx={{ alignItems: "center" }}>
                                    <ConfirmationNumberOutlinedIcon aria-hidden sx={{ fontSize: 18, color: availabilityColor }} />
                                    <Typography
                                        variant="body2"
                                        sx={{ fontWeight: 600, color: availabilityColor }}
                                    >
                                        {availabilityLabel}
                                    </Typography>
                                </Stack>
                            )}

                            {canRedeem && (
                                <Box sx={{ pt: 0.5 }}>
                                    <Button
                                        variant="contained"
                                        color="primary"
                                        onClick={handleRedeemClick}
                                        aria-label={`Redeem ${headline}`}
                                        sx={{ minWidth: 120, width: { xs: "100%", sm: "auto" } }}
                                    >
                                        Redeem
                                    </Button>
                                </Box>
                            )}

                        </>

                    ) : (

                        <Stack
                            direction="row"
                            spacing={1.5}
                            sx={{ alignItems: "center", flexWrap: "wrap", rowGap: 1 }}
                        >

                            <Chip
                                label="Unavailable"
                                color="default"
                                size="small"
                            />

                            <Typography
                                variant="body2"
                                color="text.secondary"
                            >
                                This offer is currently unavailable.
                            </Typography>

                        </Stack>

                    )}

                </Stack>

            </Card>

            {open && (
                <RedeemOfferDialog
                    open={open}
                    onClose={() => setOpen(false)}
                    deal={deal}
                    onRedeemed={onRedeemed}
                />
            )}
        </>
    );
}

export default DealCard;