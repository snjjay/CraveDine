import { useEffect, useState } from "react";

import {
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
    Stack,
    Typography
} from "@mui/material";

import RedemptionService from "../services/RedemptionService";
import { useNotification } from "../features/notifications/NotificationContext";
import { getApiErrorMessage } from "../utils/apiError";
import { formatCurrency } from "../utils/currency";
import {
    RedemptionStatus,
    getRedemptionStatusColor,
    getRedemptionStatusLabel
} from "../utils/redemption";
import { formatDate, formatTime } from "../utils/time";

import type { Redemption } from "../types/Redemption";

function MyRedemptionsPage() {

    const { notify } = useNotification();

    const [redemptions, setRedemptions] =
        useState<Redemption[]>([]);

    const [loading, setLoading] =
        useState(true);

    // Claim the customer is about to cancel (confirmation dialog)
    const [cancelTarget, setCancelTarget] =
        useState<Redemption | null>(null);

    const [cancelling, setCancelling] =
        useState(false);

    useEffect(() => {

        loadRedemptions();

    }, []);

    async function loadRedemptions() {

        try {

            const data =
                await RedemptionService.getMyHistory();

            setRedemptions(data);

        }
        catch (error: any) {

            console.error(error);

            notify(
                error.response?.data?.Message ??
                error.message ??
                "Failed to load your redemption history. Please try again.",
                "error"
            );

        }
        finally {

            setLoading(false);

        }

    }

    async function handleConfirmCancel() {

        if (!cancelTarget)
            return;

        setCancelling(true);

        try {

            await RedemptionService.cancelMine(cancelTarget.id);

            notify("Your offer has been cancelled.", "success");

            setCancelTarget(null);

            await loadRedemptions();

        }
        catch (error) {

            console.error(error);

            notify(
                getApiErrorMessage(error, "Unable to cancel this offer. Please try again."),
                "error"
            );

        }
        finally {

            setCancelling(false);

        }

    }

    if (loading)
        return <CircularProgress />;

    return (

        <>

            <Typography
                variant="h4"
                sx={{ mb: 3 }}
            >
                My Redemptions
            </Typography>

            {redemptions.length === 0 ? (

                <Typography>
                    You have no redemptions yet.
                </Typography>

            ) : (

                <Stack spacing={2}>

                    {redemptions.map(r => (

                        <Card key={r.id}>

                            <CardContent>

                                <Typography variant="h6">

                                    {r.dealTitle}

                                </Typography>

                                <Typography
                                    color="text.secondary"
                                    sx={{ mb: 1 }}
                                >

                                    {r.restaurantName} · Redemption #{r.id}

                                </Typography>

                                <Typography>

                                    Arrive: {formatDate(r.arrivalDate)} at {formatTime(r.arrivalTime)}

                                </Typography>

                                <Typography>

                                    Guests: {r.guestCount}

                                </Typography>

                                {r.billAmount != null && (

                                    <Typography>

                                        Bill Amount: {formatCurrency(r.billAmount, r.currencyCode)}

                                    </Typography>

                                )}

                                {r.discountAmount != null && (

                                    <Typography>

                                        Discount Amount: {formatCurrency(r.discountAmount, r.currencyCode)}

                                    </Typography>

                                )}

                                {r.finalAmount != null && (

                                    <Typography>

                                        Final Amount: {formatCurrency(r.finalAmount, r.currencyCode)}

                                    </Typography>

                                )}

                                <Typography>

                                    Redeemed: {new Date(r.redeemedAt).toLocaleString()}

                                </Typography>

                                {r.completedAt && (

                                    <Typography>

                                        Completed: {new Date(r.completedAt).toLocaleString()}

                                    </Typography>

                                )}

                                <Stack
                                    direction="row"
                                    spacing={2}
                                    sx={{ mt: 2, alignItems: "center" }}
                                >

                                    <Chip
                                        label={getRedemptionStatusLabel(r.status)}
                                        color={getRedemptionStatusColor(r.status)}
                                    />

                                    {r.status === RedemptionStatus.Redeemed && (

                                        <Button
                                            color="error"
                                            variant="outlined"
                                            size="small"
                                            onClick={() => setCancelTarget(r)}
                                        >
                                            Cancel Offer
                                        </Button>

                                    )}

                                </Stack>

                            </CardContent>

                        </Card>

                    ))}

                </Stack>

            )}

            <Dialog
                open={cancelTarget !== null}
                onClose={() => !cancelling && setCancelTarget(null)}
            >

                <DialogTitle>
                    Cancel Offer
                </DialogTitle>

                <DialogContent>

                    <DialogContentText>
                        Are you sure you want to cancel this offer? The offer will be released for other customers.
                    </DialogContentText>

                </DialogContent>

                <DialogActions>

                    <Button
                        onClick={() => setCancelTarget(null)}
                        disabled={cancelling}
                    >
                        Keep Offer
                    </Button>

                    <Button
                        color="error"
                        variant="contained"
                        onClick={handleConfirmCancel}
                        disabled={cancelling}
                    >
                        {cancelling ? "Cancelling..." : "Cancel Offer"}
                    </Button>

                </DialogActions>

            </Dialog>

        </>

    );

}

export default MyRedemptionsPage;
