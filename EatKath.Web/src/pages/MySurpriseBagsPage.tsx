import { useEffect, useRef, useState } from "react";
import { Link as RouterLink } from "react-router-dom";
import axios from "axios";

import {
    Alert,
    Box,
    Button,
    Card,
    CardContent,
    Chip,
    CircularProgress,
    Dialog,
    DialogActions,
    DialogContent,
    DialogContentText,
    DialogTitle,
    Link,
    Stack,
    Typography
} from "@mui/material";

import SurpriseBagReservationService from "../services/SurpriseBagReservationService";
import { useNotification } from "../features/notifications/NotificationContext";
import { getApiErrorMessage } from "../utils/apiError";
import { formatCurrency } from "../utils/currency";
import {
    SurpriseBagReservationStatus,
    formatReservationTimestamp,
    getSurpriseBagCategoryLabel,
    getSurpriseBagReservationStatusColor,
    getSurpriseBagReservationStatusLabel
} from "../utils/surpriseBag";
import { formatDate, formatTime12Hour } from "../utils/time";

import type { SurpriseBagReservation } from "../types/SurpriseBagReservation";

// "To pay at pickup" while Reserved; what was paid once Collected. A
// cancelled or no-show reservation was never paid.
function getAmountLabel(status: number): string {

    switch (status) {

        case SurpriseBagReservationStatus.Reserved:
            return "To pay at pickup";

        case SurpriseBagReservationStatus.Collected:
            return "Paid at pickup";

        default:
            return "Total (not paid)";

    }

}

function MySurpriseBagsPage() {

    const { notify } = useNotification();

    // null until the history has loaded once (or if that failed).
    const [reservations, setReservations] =
        useState<SurpriseBagReservation[] | null>(null);

    const [loading, setLoading] =
        useState(true);

    // Reservation the customer is about to cancel (confirmation dialog)
    const [cancelTarget, setCancelTarget] =
        useState<SurpriseBagReservation | null>(null);

    const [cancelling, setCancelling] =
        useState(false);

    // Incremented to load the history again (retry, after a cancellation).
    const [reloadKey, setReloadKey] =
        useState(0);

    // Why the history couldn't be loaded the first time (shown on the
    // page with Try Again).
    const [loadError, setLoadError] =
        useState<string | null>(null);

    // The history has loaded at least once.
    const hasLoaded = useRef(false);

    // A cancellation whose outcome is unknown (no response). Shown on the
    // page until dismissed, so a notification from the history reload
    // (e.g. it fails too) can't hide it.
    const [unconfirmedNotice, setUnconfirmedNotice] =
        useState<string | null>(null);

    // The first failed load is shown on the page only. A failed reload
    // keeps the history already shown; the error is a notification. A
    // response that arrives after leaving the page is ignored.
    useEffect(() => {

        let active = true;

        SurpriseBagReservationService.getMine()
            .then(data => {

                if (!active)
                    return;

                hasLoaded.current = true;

                setReservations(data);
                setLoadError(null);

            })
            .catch(error => {

                console.error(error);

                if (!active)
                    return;

                const message =
                    getApiErrorMessage(error, "We couldn't load your Surprise Bags right now. Please try again.");

                if (hasLoaded.current)
                    notify(message, "error");
                else
                    setLoadError(message);

            })
            .finally(() => {

                if (active)
                    setLoading(false);

            });

        return () => {
            active = false;
        };

    }, [reloadKey, notify]);

    function reloadReservations() {

        setReloadKey(key => key + 1);

    }

    function handleRetry() {

        setLoading(true);
        setLoadError(null);

        reloadReservations();

    }

    async function handleConfirmCancel() {

        if (!cancelTarget)
            return;

        setCancelling(true);

        try {

            await SurpriseBagReservationService.cancelMine(cancelTarget.id);

            notify("Your Surprise Bag reservation has been cancelled.", "success");

            setCancelTarget(null);

            reloadReservations();

        }
        catch (error) {

            console.error(error);

            // No response (connection lost or timed out): the API may still
            // have cancelled it, so it isn't reported as failed and isn't
            // retried; the history is reloaded to show its actual state.
            if (axios.isAxiosError(error) && !error.response) {

                setUnconfirmedNotice(
                    `We couldn't confirm that your reservation for "${cancelTarget.bagTitle}" was cancelled because the connection was interrupted. It may still have gone through - check its status below before trying again.`
                );

                setCancelTarget(null);

                reloadReservations();

                return;

            }

            notify(
                getApiErrorMessage(error, "Unable to cancel this reservation. Please try again."),
                "error"
            );

            // The API answered (e.g. the pickup window has started, or it
            // was already collected): the reservation changed, so show
            // its current state.
            if (axios.isAxiosError(error) && error.response) {

                setCancelTarget(null);

                reloadReservations();

            }

        }
        finally {

            setCancelling(false);

        }

    }

    if (loading)
        return <CircularProgress aria-label="Loading your Surprise Bags" />;

    return (

        <>

            <Typography
                variant="h4"
                component="h1"
                sx={{ mb: 3 }}
            >
                My Surprise Bags
            </Typography>

            {unconfirmedNotice && (
                <Alert
                    severity="warning"
                    onClose={() => setUnconfirmedNotice(null)}
                    sx={{ mb: 3 }}
                >
                    {unconfirmedNotice}
                </Alert>
            )}

            {reservations === null ? (

                <Stack spacing={2} sx={{ alignItems: "flex-start" }}>

                    <Alert severity="error">
                        {loadError ?? "We couldn't load your Surprise Bags right now. Please try again."}
                    </Alert>

                    <Button variant="contained" onClick={handleRetry}>
                        Try Again
                    </Button>

                </Stack>

            ) : reservations.length === 0 ? (

                <Stack spacing={2} sx={{ alignItems: "flex-start" }}>

                    <Typography>
                        You have no Surprise Bag reservations yet.
                    </Typography>

                    <Button
                        variant="contained"
                        component={RouterLink}
                        to="/restaurants"
                    >
                        Browse Restaurants
                    </Button>

                </Stack>

            ) : (

                <Stack spacing={2}>

                    {reservations.map(r => (

                        <Card key={r.id} component="article" aria-label={`${r.bagTitle}, ${getSurpriseBagReservationStatusLabel(r.status)}`}>

                            <CardContent>

                                <Stack
                                    direction="row"
                                    sx={{ justifyContent: "space-between", alignItems: "flex-start", gap: 2 }}
                                >

                                    <Box sx={{ minWidth: 0 }}>

                                        <Typography variant="h6" component="h2" sx={{ overflowWrap: "anywhere" }}>
                                            {r.bagTitle}
                                        </Typography>

                                        <Typography color="text.secondary">
                                            {getSurpriseBagCategoryLabel(r.category)} · {r.restaurantName}
                                        </Typography>

                                    </Box>

                                    <Chip
                                        label={getSurpriseBagReservationStatusLabel(r.status)}
                                        color={getSurpriseBagReservationStatusColor(r.status)}
                                        sx={{ flexShrink: 0 }}
                                    />

                                </Stack>

                                <Typography sx={{ mt: 1.5 }}>
                                    Confirmation code:{" "}
                                    <Box
                                        component="strong"
                                        sx={{ fontFamily: "monospace", fontSize: "1.125rem", letterSpacing: "0.08em" }}
                                    >
                                        {r.confirmationCode}
                                    </Box>
                                </Typography>

                                <Typography>
                                    Pick up: {formatDate(r.pickupDate)}, {formatTime12Hour(r.pickupStartTime)} – {formatTime12Hour(r.pickupEndTime)}
                                </Typography>

                                <Typography>
                                    Quantity: {r.quantity}
                                </Typography>

                                <Typography>
                                    {getAmountLabel(r.status)}: <strong>{formatCurrency(r.totalPrice, r.currencyCode)}</strong>
                                </Typography>

                                {(r.restaurantAddress || r.restaurantPhone) && (

                                    <Typography variant="body2" color="text.secondary" sx={{ mt: 1 }}>
                                        {r.restaurantAddress}
                                        {r.restaurantAddress && r.restaurantPhone && " · "}
                                        {r.restaurantPhone && (
                                            <Link href={`tel:${r.restaurantPhone}`}>
                                                {r.restaurantPhone}
                                            </Link>
                                        )}
                                    </Typography>

                                )}

                                <Typography variant="body2" color="text.secondary" sx={{ mt: 1 }}>
                                    Reserved: {formatReservationTimestamp(r.createdAt)}
                                </Typography>

                                {r.collectedAt && (
                                    <Typography variant="body2" color="text.secondary">
                                        Collected: {formatReservationTimestamp(r.collectedAt)}
                                    </Typography>
                                )}

                                {r.cancelledAt && (
                                    <Typography variant="body2" color="text.secondary">
                                        Cancelled: {formatReservationTimestamp(r.cancelledAt)}
                                    </Typography>
                                )}

                                {r.canCancel && (

                                    <Box sx={{ mt: 2 }}>
                                        <Button
                                            color="error"
                                            variant="outlined"
                                            size="small"
                                            onClick={() => setCancelTarget(r)}
                                            aria-label={`Cancel reservation for ${r.bagTitle}`}
                                        >
                                            Cancel Reservation
                                        </Button>
                                    </Box>

                                )}

                            </CardContent>

                        </Card>

                    ))}

                </Stack>

            )}

            <Dialog
                open={cancelTarget !== null}
                onClose={() => !cancelling && setCancelTarget(null)}
                aria-labelledby="cancel-surprise-bag-title"
                aria-describedby="cancel-surprise-bag-description"
            >

                <DialogTitle id="cancel-surprise-bag-title">
                    Cancel Reservation
                </DialogTitle>

                <DialogContent>

                    <DialogContentText id="cancel-surprise-bag-description">
                        Are you sure you want to cancel your reservation{cancelTarget ? ` for "${cancelTarget.bagTitle}"` : ""}? The bag will be released for other customers.
                    </DialogContentText>

                </DialogContent>

                <DialogActions>

                    <Button
                        onClick={() => setCancelTarget(null)}
                        disabled={cancelling}
                    >
                        Keep Reservation
                    </Button>

                    <Button
                        color="error"
                        variant="contained"
                        onClick={handleConfirmCancel}
                        disabled={cancelling}
                    >
                        {cancelling ? "Cancelling..." : "Cancel Reservation"}
                    </Button>

                </DialogActions>

            </Dialog>

        </>

    );

}

export default MySurpriseBagsPage;
