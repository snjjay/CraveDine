// RedeemOfferDialog = "Redeem your walk-in offer".
//
// The customer picks an arrival date, an arrival time inside the
// deal's arrival window and the number of guests. This is NOT a
// table reservation: it claims the discount, and the restaurant
// completes the redemption (records the bill) when they visit.
//
// Name, phone and email are never typed here - the API takes them
// from the logged-in account.
//
// The parent mounts this dialog only while it is open, so every
// opening starts with a fresh form.

import { useContext, useMemo, useState } from "react";
import { useNavigate } from "react-router-dom";
import axios from "axios";

import {
    Alert,
    Box,
    Button,
    Dialog,
    DialogActions,
    DialogContent,
    DialogTitle,
    Divider,
    MenuItem,
    Stack,
    TextField,
    Typography
} from "@mui/material";

import RedemptionService from "../../services/RedemptionService";
import AuthContext from "../../features/auth/AuthContext";
import type { Deal } from "../../types/Deal";
import type { Redemption } from "../../types/Redemption";
import { getDealHeadline, getDealSummary } from "../../utils/deal";
import {
    buildArrivalSlots,
    formatDate,
    formatTime12Hour,
    todayIsoDate
} from "../../utils/time";

// Must match the duplicate-claim message in the API
// (RedemptionService.RedeemAsync). The API stays the authority - this
// only lets the dialog stop repeat attempts for the same date.
const DUPLICATE_CLAIM_MESSAGE =
    "You have already redeemed this offer for the selected date.";

// Business-rule messages from the API are written for customers
// (e.g. "No offers are left for the selected date."), so they are
// shown as-is. Anything else gets a safe generic message.
function getRedeemErrorMessage(error: unknown): string {

    if (axios.isAxiosError(error)) {

        if (!error.response)
            return "Unable to connect to the server. Please try again later.";

        const apiMessage = (error.response.data as { Message?: string } | undefined)?.Message;

        if (error.response.status < 500 && apiMessage)
            return apiMessage;

    }

    return "Unable to redeem this offer. Please try again.";

}

interface Props {
    open: boolean;
    onClose: () => void;
    deal: Deal;
    // Called after a successful claim (e.g. to refresh "X left").
    onRedeemed?: () => void;
}

function RedeemOfferDialog({
    open,
    onClose,
    deal,
    onRedeemed
}: Props) {

    const navigate = useNavigate();

    const auth = useContext(AuthContext);
    const user = auth?.user ?? null;

    // Earliest selectable date: today, or the deal start if later.
    const today = todayIsoDate();
    const minDate = deal.startDate > today ? deal.startDate : today;
    const maxDate = deal.endDate;
    const dealHasEnded = minDate > maxDate;

    const [arrivalDate, setArrivalDate] = useState(minDate);
    const [arrivalTime, setArrivalTime] = useState("");
    const [guestCount, setGuestCount] = useState(Math.min(2, deal.maximumGuests));
    const [submitting, setSubmitting] = useState(false);
    const [error, setError] = useState<string | null>(null);
    const [claim, setClaim] = useState<Redemption | null>(null);

    // Arrival date the API reported as already redeemed by this
    // customer. Redeem stays disabled while that date is selected.
    const [duplicateDate, setDuplicateDate] = useState<string | null>(null);

    const isDuplicateDate = duplicateDate !== null && duplicateDate === arrivalDate;

    const dateIsValid =
        arrivalDate !== "" &&
        arrivalDate >= minDate &&
        arrivalDate <= maxDate;

    const slots = useMemo(
        () => dateIsValid
            ? buildArrivalSlots(deal.startTime, deal.endTime, arrivalDate)
            : [],
        [dateIsValid, deal.startTime, deal.endTime, arrivalDate]
    );

    // A selected time that is not offered for the chosen date
    // (e.g. already passed today) counts as no selection.
    const selectedTime = slots.includes(arrivalTime) ? arrivalTime : "";

    const guestOptions = Array.from(
        { length: Math.max(1, deal.maximumGuests) },
        (_, i) => i + 1
    );

    const canSubmit =
        !submitting &&
        !dealHasEnded &&
        !isDuplicateDate &&
        dateIsValid &&
        selectedTime !== "" &&
        guestCount >= 1 &&
        guestCount <= deal.maximumGuests;

    async function handleRedeem() {

        if (!canSubmit)
            return;

        setSubmitting(true);
        setError(null);

        try {

            const result = await RedemptionService.redeem({
                dealId: deal.id,
                arrivalDate,
                arrivalTime: selectedTime,
                guestCount
            });

            setClaim(result);

            onRedeemed?.();

        }
        catch (err) {

            const message = getRedeemErrorMessage(err);

            if (message === DUPLICATE_CLAIM_MESSAGE)
                setDuplicateDate(arrivalDate);

            setError(message);

        }
        finally {

            setSubmitting(false);

        }

    }

    // Changing the time or guests does not change eligibility for the
    // date, so a duplicate-claim error stays visible.
    function clearErrorUnlessDuplicate() {

        if (!isDuplicateDate)
            setError(null);

    }

    // -----------------------------
    // Success: show the claim to present at the restaurant
    // -----------------------------
    if (claim) {

        return (

            <Dialog open={open} onClose={onClose} fullWidth maxWidth="sm">

                <DialogTitle>Offer redeemed</DialogTitle>

                <DialogContent>

                    <Stack spacing={2} sx={{ mt: 1 }}>

                        <Alert severity="success">
                            Your walk-in offer is confirmed. Show this at the restaurant when you arrive.
                        </Alert>

                        <Box>
                            <Typography variant="h6">{getDealHeadline(deal)}</Typography>
                            <Typography color="text.secondary">{claim.restaurantName}</Typography>
                        </Box>

                        <Typography>Redemption #: <strong>{claim.id}</strong></Typography>
                        <Typography>Arrive: {formatDate(claim.arrivalDate)} at {formatTime12Hour(claim.arrivalTime)}</Typography>
                        <Typography>Guests: {claim.guestCount}</Typography>

                        <Divider />

                        <Typography variant="body2" color="text.secondary">
                            {claim.customerName} · {claim.customerPhone || "No phone on account"} · {claim.customerEmail}
                        </Typography>

                    </Stack>

                </DialogContent>

                <DialogActions>

                    <Button onClick={onClose}>Done</Button>

                    <Button
                        variant="contained"
                        onClick={() => navigate("/my-redemptions")}
                    >
                        View My Redemptions
                    </Button>

                </DialogActions>

            </Dialog>

        );

    }

    // -----------------------------
    // Claim form
    // -----------------------------
    return (

        <Dialog open={open} onClose={onClose} fullWidth maxWidth="sm">

            <DialogTitle>Redeem your walk-in offer</DialogTitle>

            <DialogContent>

                <Stack spacing={2} sx={{ mt: 1 }}>

                    <Box>
                        <Typography variant="h6">{getDealHeadline(deal)}</Typography>
                        <Typography>{getDealSummary(deal)}</Typography>
                        <Typography variant="body2" color="text.secondary">
                            Arrive between {formatTime12Hour(deal.startTime)} - {formatTime12Hour(deal.endTime)}
                        </Typography>
                    </Box>

                    <Alert severity="info">
                        This is a walk-in offer, not a table booking. Arrive in your chosen time and show your redemption to the restaurant.
                    </Alert>

                    {error && (
                        <Alert severity="error" role="alert">
                            {error}
                            {isDuplicateDate && " Choose a different date to redeem again."}
                        </Alert>
                    )}

                    {dealHasEnded ? (

                        <Alert severity="warning">This offer has ended.</Alert>

                    ) : (

                        <>

                            <TextField
                                label="Arrival date"
                                type="date"
                                value={arrivalDate}
                                onChange={(e) => {
                                    // A new date gets a fresh eligibility check.
                                    setArrivalDate(e.target.value);
                                    setDuplicateDate(null);
                                    setError(null);
                                }}
                                error={arrivalDate !== "" && !dateIsValid}
                                helperText={
                                    arrivalDate !== "" && !dateIsValid
                                        ? `Choose a date between ${formatDate(minDate)} and ${formatDate(maxDate)}.`
                                        : " "
                                }
                                slotProps={{
                                    inputLabel: { shrink: true },
                                    htmlInput: { min: minDate, max: maxDate }
                                }}
                            />

                            <TextField
                                select
                                label="Arrival time"
                                value={selectedTime}
                                onChange={(e) => {
                                    setArrivalTime(e.target.value);
                                    clearErrorUnlessDuplicate();
                                }}
                                disabled={slots.length === 0}
                                helperText={
                                    dateIsValid && slots.length === 0
                                        ? "No arrival times left on this date. Please choose another date."
                                        : " "
                                }
                            >
                                {slots.map(slot => (
                                    <MenuItem key={slot} value={slot}>
                                        {formatTime12Hour(slot)}
                                    </MenuItem>
                                ))}
                            </TextField>

                            <TextField
                                select
                                label="Guests"
                                value={guestCount}
                                onChange={(e) => {
                                    setGuestCount(Number(e.target.value));
                                    clearErrorUnlessDuplicate();
                                }}
                                helperText={`Up to ${deal.maximumGuests} guests for this offer.`}
                            >
                                {guestOptions.map(n => (
                                    <MenuItem key={n} value={n}>
                                        {n} {n === 1 ? "guest" : "guests"}
                                    </MenuItem>
                                ))}
                            </TextField>

                        </>

                    )}

                    {user && (

                        <Box>
                            <Typography variant="subtitle2">Redeeming as</Typography>
                            <Typography variant="body2">
                                {user.firstName} {user.lastName} · {user.email}
                            </Typography>
                            <Typography variant="caption" color="text.secondary">
                                Your account name, email and phone number will be shared with the restaurant.
                            </Typography>
                        </Box>

                    )}

                </Stack>

            </DialogContent>

            <DialogActions>

                <Button onClick={onClose} disabled={submitting}>
                    Cancel
                </Button>

                <Button
                    variant="contained"
                    onClick={handleRedeem}
                    disabled={!canSubmit}
                >
                    {submitting ? "Redeeming..." : "Redeem"}
                </Button>

            </DialogActions>

        </Dialog>

    );

}

export default RedeemOfferDialog;
