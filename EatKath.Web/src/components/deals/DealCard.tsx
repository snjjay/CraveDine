import { useContext, useState } from "react";
import { useNavigate } from "react-router-dom";

import {
    Button,
    Card,
    CardContent,
    Chip,
    Stack,
    Typography
} from "@mui/material";

import RedeemOfferDialog from "../redemptions/RedeemOfferDialog";
import AuthContext from "../../features/auth/AuthContext";
import { useNotification } from "../../features/notifications/NotificationContext";
import type { Deal } from "../../types/Deal";
import { getDealHeadline, getDealSummary } from "../../utils/deal";
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

    return (
        <>
            <Card sx={{ mb: 2 }}>

                <CardContent>

                    <Typography variant="h6">
                        {headline}
                    </Typography>

                    <Typography sx={{ mt: 0.5 }}>
                        {summary}
                    </Typography>

                    <Typography sx={{ mt: 2, fontWeight: 500 }}>
                        Arrive between {formatTime12Hour(deal.startTime)} - {formatTime12Hour(deal.endTime)}
                    </Typography>

                    <Typography
                        variant="body2"
                        color="text.secondary"
                    >
                        Valid {formatCardDateRange(deal.startDate, deal.endDate)}
                    </Typography>

                    {deal.isActive ? (

                        <>

                            {availabilityLabel && (
                                <Typography
                                    variant="body2"
                                    sx={{ mt: 1, fontWeight: 500, color: availabilityColor }}
                                >
                                    {availabilityLabel}
                                </Typography>
                            )}

                            {!hasEnded && !deal.isSoldOut && (
                                <Button
                                    variant="contained"
                                    color="primary"
                                    onClick={handleRedeemClick}
                                    sx={{ mt: 2 }}
                                >
                                    Redeem
                                </Button>
                            )}

                        </>

                    ) : (

                        <Stack
                            direction="row"
                            spacing={2}
                            alignItems="center"
                        >

                            <Chip
                                label="Unavailable"
                                color="default"
                            />

                            <Typography
                                variant="body2"
                                color="text.secondary"
                            >
                                This offer is currently unavailable.
                            </Typography>

                        </Stack>

                    )}

                </CardContent>

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