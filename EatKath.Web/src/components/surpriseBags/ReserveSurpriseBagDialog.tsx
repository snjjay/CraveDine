// ReserveSurpriseBagDialog = "Reserve a Surprise Bag".
//
// The customer picks how many bags to reserve. Nothing is paid here:
// the restaurant takes payment when the bag is collected, and the
// customer shows the confirmation code at pickup.
//
// Availability (sold out, pickup window ended) is decided by the API in
// Kathmandu time - the dialog never works it out from the browser's
// clock, it shows what the API sends and reports the API's errors.
//
// The parent mounts this dialog only while it is open, so every
// opening starts with a fresh form.

import { useContext, useRef, useState } from "react";
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
    Link,
    MenuItem,
    Stack,
    TextField,
    Typography
} from "@mui/material";

import SurpriseBagReservationService from "../../services/SurpriseBagReservationService";
import AuthContext from "../../features/auth/AuthContext";
import type { SurpriseBag } from "../../types/SurpriseBag";
import type { SurpriseBagReservation } from "../../types/SurpriseBagReservation";
import { formatCurrency } from "../../utils/currency";
import { getSurpriseBagCategoryLabel } from "../../utils/surpriseBag";
import { formatDate, formatTime12Hour } from "../../utils/time";

// Must match the messages in the API (SurpriseBagReservationService.
// CreateAsync). The API stays the authority - these only decide when
// the dialog stops repeat attempts or refreshes the bag list.
const DUPLICATE_RESERVATION_MESSAGE =
    "You already have a reservation for this Surprise Bag.";

const SOLD_OUT_MESSAGE = "This Surprise Bag is sold out.";

// The bag can no longer be reserved at all (inactive, or its pickup
// window has ended).
const UNAVAILABLE_MESSAGES = [
    "This Surprise Bag is not available.",
    "The pickup window for this Surprise Bag has ended."
];

// "Only 2 left." - fewer bags left than the quantity asked for.
const ONLY_LEFT_PATTERN = /^Only \d+ left\.$/;

// No response (connection lost or timed out): the API may still have
// made the reservation. A repeat attempt can't reserve the bag twice -
// the API answers with the duplicate-reservation message instead.
const OUTCOME_UNKNOWN_MESSAGE =
    "We couldn't confirm your reservation because the connection was interrupted. " +
    "It may still have been made - please check My Surprise Bags before trying again. " +
    "Trying again won't reserve the bag twice.";

interface ReserveError {
    message: string;
    // The bag's availability changed (sold out, fewer left, no longer
    // offered), so the bag list should be refreshed.
    availabilityChanged: boolean;
    // The bag can no longer be reserved (inactive, pickup window ended,
    // not found).
    bagUnavailable: boolean;
    // No response - the reservation may or may not have been made.
    outcomeUnknown: boolean;
}

// Business-rule messages from the API are written for customers
// (e.g. "Only 2 left."), so they are shown as-is. A 404 may have no
// message, and anything else gets a safe generic message.
function getReserveError(error: unknown): ReserveError {

    if (axios.isAxiosError(error)) {

        if (!error.response)
            return {
                message: OUTCOME_UNKNOWN_MESSAGE,
                availabilityChanged: false,
                bagUnavailable: false,
                outcomeUnknown: true
            };

        const status = error.response.status;
        const apiMessage = (error.response.data as { Message?: string } | undefined)?.Message;

        if (status === 404)
            return {
                message: apiMessage || "This Surprise Bag could not be found.",
                availabilityChanged: true,
                bagUnavailable: true,
                outcomeUnknown: false
            };

        if (status < 500 && apiMessage) {

            const bagUnavailable = UNAVAILABLE_MESSAGES.includes(apiMessage);

            return {
                message: apiMessage,
                availabilityChanged:
                    bagUnavailable ||
                    apiMessage === SOLD_OUT_MESSAGE ||
                    ONLY_LEFT_PATTERN.test(apiMessage),
                bagUnavailable,
                outcomeUnknown: false
            };

        }

    }

    return {
        message: "Unable to reserve this Surprise Bag. Please try again.",
        availabilityChanged: false,
        bagUnavailable: false,
        outcomeUnknown: false
    };

}

interface Props {
    open: boolean;
    onClose: () => void;
    bag: SurpriseBag;
    // false when the bag is no longer in the restaurant's refreshed bag
    // list (e.g. deactivated or its pickup window ended).
    isListed?: boolean;
    // Called after a reservation, when the API reports that the bag's
    // availability changed (e.g. sold out), when the outcome is unknown
    // (no response) or the customer already has a reservation - to refresh
    // the bag list and which bags the customer has reserved.
    onAvailabilityChanged?: () => void;
    // Opens the customer's Surprise Bag reservations. The confirmation
    // only offers "View My Surprise Bags" when this is provided.
    onViewReservations?: () => void;
}

function ReserveSurpriseBagDialog({
    open,
    onClose,
    bag,
    isListed = true,
    onAvailabilityChanged,
    onViewReservations
}: Props) {

    const auth = useContext(AuthContext);
    const user = auth?.user ?? null;

    const [quantity, setQuantity] = useState(1);
    const [submitting, setSubmitting] = useState(false);
    const [error, setError] = useState<string | null>(null);
    // The last attempt got no response (see OUTCOME_UNKNOWN_MESSAGE).
    const [outcomeUnknown, setOutcomeUnknown] = useState(false);
    const [isDuplicate, setIsDuplicate] = useState(false);
    // The API reported the bag can no longer be reserved.
    const [reportedUnavailable, setReportedUnavailable] = useState(false);
    const [reservation, setReservation] = useState<SurpriseBagReservation | null>(null);

    // Blocks a second request before the disabled button re-renders.
    const submittingRef = useRef(false);

    // Most a customer can reserve now. The bag may be refreshed while the
    // dialog is open (e.g. after "Only 2 left."), so the selection is
    // kept within the current limit.
    const maxQuantity = bag.isSoldOut
        ? 0
        : Math.max(0, Math.min(bag.maxPerCustomer, bag.remainingQuantity));

    const selectedQuantity = Math.min(quantity, maxQuantity);

    const quantityOptions = Array.from({ length: maxQuantity }, (_, i) => i + 1);

    // Gone from the refreshed list, or the API said so (also when the
    // list couldn't be refreshed).
    const noLongerAvailable = !isListed || reportedUnavailable;

    const canSubmit =
        !submitting &&
        !isDuplicate &&
        !noLongerAvailable &&
        selectedQuantity >= 1;

    const remainingLabel = noLongerAvailable
        ? "No longer available"
        : bag.isSoldOut || bag.remainingQuantity <= 0
            ? "Sold out"
            : `${bag.remainingQuantity} ${bag.remainingQuantity === 1 ? "bag" : "bags"} left`;

    async function handleReserve() {

        if (!canSubmit || submittingRef.current)
            return;

        submittingRef.current = true;
        setSubmitting(true);
        setError(null);
        setOutcomeUnknown(false);

        try {

            const result = await SurpriseBagReservationService.reserve({
                surpriseBagId: bag.id,
                quantity: selectedQuantity
            });

            setReservation(result);

            onAvailabilityChanged?.();

        }
        catch (err) {

            const reserveError = getReserveError(err);
            const duplicate = reserveError.message === DUPLICATE_RESERVATION_MESSAGE;

            if (duplicate)
                setIsDuplicate(true);

            if (reserveError.bagUnavailable)
                setReportedUnavailable(true);

            setError(reserveError.message);
            setOutcomeUnknown(reserveError.outcomeUnknown);

            // A reservation may exist that the page doesn't know about yet
            // (unknown outcome, or the customer already holds one), so the
            // page reloads the bags and its "Already reserved" markers too.
            if (reserveError.availabilityChanged || reserveError.outcomeUnknown || duplicate)
                onAvailabilityChanged?.();

        }
        finally {

            submittingRef.current = false;
            setSubmitting(false);

        }

    }

    // Closing (Escape, backdrop) waits for a running request, so the
    // confirmation code is not lost.
    function handleClose() {

        if (!submitting)
            onClose();

    }

    // -----------------------------
    // Success: the confirmation to show at pickup
    // -----------------------------
    if (reservation) {

        return (

            <Dialog open={open} onClose={onClose} fullWidth maxWidth="sm">

                <DialogTitle>Surprise Bag reserved</DialogTitle>

                <DialogContent>

                    <Stack spacing={2} sx={{ mt: 1 }}>

                        <Alert severity="success">
                            Your Surprise Bag is reserved. Show this code at the restaurant and pay when you collect it.
                        </Alert>

                        <Box
                            sx={{
                                textAlign: "center",
                                py: 2,
                                px: 1,
                                borderRadius: "12px",
                                bgcolor: "primarySoft"
                            }}
                        >
                            <Typography variant="overline" color="text.secondary" component="p">
                                Confirmation code
                            </Typography>
                            <Typography
                                component="p"
                                sx={{
                                    fontFamily: "monospace",
                                    fontWeight: 700,
                                    fontSize: { xs: "1.75rem", sm: "2.25rem" },
                                    letterSpacing: "0.12em",
                                    color: "primary.main",
                                    overflowWrap: "anywhere"
                                }}
                            >
                                {reservation.confirmationCode}
                            </Typography>
                        </Box>

                        <Box>
                            <Typography variant="h6">{reservation.bagTitle}</Typography>
                            <Typography color="text.secondary">{reservation.restaurantName}</Typography>
                        </Box>

                        <Typography>Quantity: {reservation.quantity}</Typography>
                        <Typography>
                            Pay at pickup: <strong>{formatCurrency(reservation.totalPrice, reservation.currencyCode)}</strong>
                        </Typography>
                        <Typography>
                            Pick up: {formatDate(reservation.pickupDate)}, {formatTime12Hour(reservation.pickupStartTime)} – {formatTime12Hour(reservation.pickupEndTime)}
                        </Typography>

                        {(reservation.restaurantAddress || reservation.restaurantPhone) && (

                            <>

                                <Divider />

                                <Box>
                                    {reservation.restaurantAddress && (
                                        <Typography variant="body2">{reservation.restaurantAddress}</Typography>
                                    )}
                                    {reservation.restaurantPhone && (
                                        <Typography variant="body2">
                                            <Link href={`tel:${reservation.restaurantPhone}`}>
                                                {reservation.restaurantPhone}
                                            </Link>
                                        </Typography>
                                    )}
                                </Box>

                            </>

                        )}

                    </Stack>

                </DialogContent>

                <DialogActions>

                    <Button onClick={onClose}>Done</Button>

                    {onViewReservations && (
                        <Button variant="contained" onClick={onViewReservations}>
                            View My Surprise Bags
                        </Button>
                    )}

                </DialogActions>

            </Dialog>

        );

    }

    // -----------------------------
    // Reservation form
    // -----------------------------
    return (

        <Dialog open={open} onClose={handleClose} fullWidth maxWidth="sm">

            <DialogTitle>Reserve a Surprise Bag</DialogTitle>

            <DialogContent>

                <Stack spacing={2} sx={{ mt: 1 }}>

                    <Box>
                        <Typography variant="h6" sx={{ overflowWrap: "anywhere" }}>{bag.title}</Typography>
                        <Typography color="text.secondary">
                            {getSurpriseBagCategoryLabel(bag.category)} · {bag.restaurantName}
                        </Typography>
                    </Box>

                    <Stack direction="row" sx={{ alignItems: "baseline", flexWrap: "wrap", columnGap: 1 }}>
                        <Typography variant="h6" component="p" sx={{ fontWeight: 700 }}>
                            {formatCurrency(bag.price, bag.currencyCode)}
                        </Typography>
                        {bag.estimatedValue != null && (
                            <Typography variant="body2" color="text.secondary">
                                Worth {formatCurrency(bag.estimatedValue, bag.currencyCode)}
                            </Typography>
                        )}
                    </Stack>

                    {bag.description && (
                        <Typography variant="body2">{bag.description}</Typography>
                    )}

                    <Box>
                        <Typography variant="body2" sx={{ fontWeight: 500 }}>
                            Pick up {formatDate(bag.pickupDate)}, {formatTime12Hour(bag.pickupStartTime)} – {formatTime12Hour(bag.pickupEndTime)}
                        </Typography>
                        <Typography
                            variant="body2"
                            sx={{ fontWeight: 600, color: maxQuantity > 0 && !noLongerAvailable ? "text.secondary" : "error.main" }}
                        >
                            {remainingLabel}
                        </Typography>
                    </Box>

                    {bag.conditions && (
                        <Box>
                            <Typography variant="subtitle2">Good to know</Typography>
                            <Typography variant="body2" color="text.secondary">
                                {bag.conditions}
                            </Typography>
                        </Box>
                    )}

                    <Alert severity="info">
                        You pay at the restaurant when you collect your bag. Show your confirmation code at pickup.
                    </Alert>

                    {error && (
                        <Alert severity={outcomeUnknown ? "warning" : "error"} role="alert">
                            {error}
                        </Alert>
                    )}

                    {noLongerAvailable && (
                        <Alert severity="warning">
                            This Surprise Bag is no longer available, so it can't be reserved. Please choose another bag.
                        </Alert>
                    )}

                    {maxQuantity > 0 && !noLongerAvailable && (

                        <TextField
                            select
                            label="Quantity"
                            value={selectedQuantity}
                            onChange={(e) => {
                                setQuantity(Number(e.target.value));
                                // A duplicate reservation doesn't depend on the
                                // quantity, and a reservation that may have been
                                // made still needs checking.
                                if (!isDuplicate && !outcomeUnknown)
                                    setError(null);
                            }}
                            disabled={submitting || isDuplicate}
                            helperText={`You can reserve up to ${maxQuantity}.`}
                        >
                            {quantityOptions.map(n => (
                                <MenuItem key={n} value={n}>
                                    {n} {n === 1 ? "bag" : "bags"}
                                </MenuItem>
                            ))}
                        </TextField>

                    )}

                    {selectedQuantity >= 1 && !noLongerAvailable && (
                        <Typography>
                            Total to pay at pickup: <strong>{formatCurrency(bag.price * selectedQuantity, bag.currencyCode)}</strong>
                        </Typography>
                    )}

                    {user && (

                        <Box>
                            <Typography variant="subtitle2">Reserving as</Typography>
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
                    onClick={handleReserve}
                    disabled={!canSubmit}
                >
                    {submitting ? "Reserving..." : "Reserve"}
                </Button>

            </DialogActions>

        </Dialog>

    );

}

export default ReserveSurpriseBagDialog;
