// SurpriseBagReservationActionDialog = confirm a restaurant's status change
// to a Surprise Bag reservation: collect, no-show or cancel.
//
// Every change is final, so the dialog shows exactly which reservation
// (customer, bag, code, amount) is about to change. Whether a change is
// allowed right now (e.g. no-show only after the pickup window ends -
// Kathmandu time) is decided by the API; its message is shown as-is.
//
// The parent mounts this dialog only while it is open.

import { useRef, useState } from "react";
import axios from "axios";

import {
    Alert,
    Box,
    Button,
    Chip,
    Dialog,
    DialogActions,
    DialogContent,
    DialogTitle,
    Stack,
    Typography
} from "@mui/material";

import OwnerSurpriseBagReservationService from "../../services/OwnerSurpriseBagReservationService";
import type { OwnerSurpriseBagReservation } from "../../types/OwnerSurpriseBagReservation";
import { formatCurrency } from "../../utils/currency";
import {
    SurpriseBagReservationStatus,
    getSurpriseBagReservationStatusColor,
    getSurpriseBagReservationStatusLabel
} from "../../utils/surpriseBag";
import { formatDate, formatTime12Hour } from "../../utils/time";

export type ReservationAction = "collect" | "noShow" | "cancel";

const ACTION_TITLES: Record<ReservationAction, string> = {
    collect: "Collect Surprise Bag",
    noShow: "Mark as No-show",
    cancel: "Cancel Reservation"
};

const ACTION_BUTTONS: Record<ReservationAction, { label: string; busy: string }> = {
    collect: { label: "Mark as Collected", busy: "Saving..." },
    noShow: { label: "Mark as No-show", busy: "Saving..." },
    cancel: { label: "Cancel Reservation", busy: "Cancelling..." }
};

// Same wording as the API's messages for a reservation that can't change.
function getFinalStatusMessage(status: number): string {

    switch (status) {

        case SurpriseBagReservationStatus.Collected:
            return "This reservation has already been collected and can't be changed.";

        case SurpriseBagReservationStatus.Cancelled:
            return "This reservation has already been cancelled and can't be changed.";

        case SurpriseBagReservationStatus.NoShow:
            return "This reservation has already been marked as a no-show and can't be changed.";

        default:
            return "This reservation can no longer be changed.";

    }

}

interface ActionError {
    message: string;
    // The API answered: the change isn't allowed (now), so trying again
    // won't help. Without a response the change may or may not have
    // happened.
    rejected: boolean;
}

function getActionError(error: unknown): ActionError {

    if (axios.isAxiosError(error)) {

        if (!error.response)
            return {
                message: "We couldn't confirm the change because the connection was interrupted. The list is being refreshed - check the reservation's status before trying again.",
                rejected: false
            };

        const apiMessage = (error.response.data as { Message?: string } | undefined)?.Message;

        if (error.response.status === 404)
            return { message: apiMessage || "This reservation could not be found.", rejected: true };

        if (error.response.status < 500 && apiMessage)
            return { message: apiMessage, rejected: true };

    }

    return { message: "Unable to update this reservation. Please try again.", rejected: false };

}

interface Props {
    open: boolean;
    action: ReservationAction;
    reservation: OwnerSurpriseBagReservation;
    onClose: () => void;
    // Called with the updated reservation after a successful change.
    onDone: (updated: OwnerSurpriseBagReservation) => void;
    // Called after a failed attempt: the reservation may have changed, so
    // the list should be refreshed.
    onFailed: () => void;
}

function SurpriseBagReservationActionDialog({
    open,
    action,
    reservation,
    onClose,
    onDone,
    onFailed
}: Props) {

    const [submitting, setSubmitting] = useState(false);
    const [error, setError] = useState<ActionError | null>(null);

    // Blocks a second request before the disabled button re-renders.
    const submittingRef = useRef(false);

    const r = reservation;

    // Collected, Cancelled and NoShow are final.
    const isFinal = r.status !== SurpriseBagReservationStatus.Reserved;

    const canConfirm = !isFinal && !submitting && !error?.rejected;

    const total = formatCurrency(r.totalPrice, r.currencyCode);

    async function handleConfirm() {

        if (!canConfirm || submittingRef.current)
            return;

        submittingRef.current = true;
        setSubmitting(true);
        setError(null);

        try {

            const updated =
                action === "collect"
                    ? await OwnerSurpriseBagReservationService.collect(r.confirmationCode)
                    : action === "noShow"
                        ? await OwnerSurpriseBagReservationService.markNoShow(r.id)
                        : await OwnerSurpriseBagReservationService.cancel(r.id);

            onDone(updated);

        }
        catch (err) {

            console.error(err);

            setError(getActionError(err));

            onFailed();

        }
        finally {

            submittingRef.current = false;
            setSubmitting(false);

        }

    }

    // Closing (Escape, backdrop) waits for a running request.
    function handleClose() {

        if (!submitting)
            onClose();

    }

    return (

        <Dialog
            open={open}
            onClose={handleClose}
            fullWidth
            maxWidth="sm"
            aria-labelledby="reservation-action-title"
        >

            <DialogTitle id="reservation-action-title">
                {ACTION_TITLES[action]}
            </DialogTitle>

            <DialogContent>

                <Stack spacing={2} sx={{ mt: 1 }}>

                    {/* Exactly which reservation will change */}
                    <Box
                        sx={{
                            p: 2,
                            borderRadius: "12px",
                            border: "1px solid",
                            borderColor: "divider"
                        }}
                    >

                        <Stack
                            direction="row"
                            sx={{ justifyContent: "space-between", alignItems: "flex-start", gap: 2 }}
                        >

                            <Box sx={{ minWidth: 0 }}>
                                <Typography variant="subtitle1" sx={{ fontWeight: 700 }}>
                                    {r.customerName}
                                </Typography>
                                {r.customerPhone && (
                                    <Typography variant="body2" color="text.secondary">
                                        {r.customerPhone}
                                    </Typography>
                                )}
                            </Box>

                            <Chip
                                size="small"
                                label={getSurpriseBagReservationStatusLabel(r.status)}
                                color={getSurpriseBagReservationStatusColor(r.status)}
                            />

                        </Stack>

                        <Typography sx={{ mt: 1.5, overflowWrap: "anywhere" }}>
                            {r.quantity} × {r.bagTitle}
                        </Typography>

                        <Typography>
                            Code:{" "}
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
                            {action === "collect" ? "Take payment" : "Total"}: <strong>{total}</strong>
                        </Typography>

                    </Box>

                    {isFinal ? (

                        <Alert severity="info">
                            {getFinalStatusMessage(r.status)}
                        </Alert>

                    ) : (

                        <Typography>
                            {action === "collect" &&
                                `Hand over ${r.quantity === 1 ? "the bag" : `${r.quantity} bags`} and take payment of ${total}. Collection is only possible on the pickup date.`}
                            {action === "noShow" &&
                                "Mark this reservation as a no-show? This is only possible after the pickup window has ended."}
                            {action === "cancel" &&
                                "Cancel this reservation? The bag is released for other customers. The customer isn't notified automatically. If the customer didn't come, use No-show instead."}
                            {" "}This can't be undone.
                        </Typography>

                    )}

                    {error && (
                        <Alert severity={error.rejected ? "error" : "warning"} role="alert">
                            {error.message}
                        </Alert>
                    )}

                </Stack>

            </DialogContent>

            <DialogActions>

                <Button onClick={onClose} disabled={submitting}>
                    {isFinal || error?.rejected ? "Close" : "Back"}
                </Button>

                {!isFinal && (
                    <Button
                        variant="contained"
                        color={action === "collect" ? "primary" : "error"}
                        onClick={handleConfirm}
                        disabled={!canConfirm}
                    >
                        {submitting ? ACTION_BUTTONS[action].busy : ACTION_BUTTONS[action].label}
                    </Button>
                )}

            </DialogActions>

        </Dialog>

    );

}

export default SurpriseBagReservationActionDialog;
