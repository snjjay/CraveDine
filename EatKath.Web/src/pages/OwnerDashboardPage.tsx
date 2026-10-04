import { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";

import {
    Button,
    Chip,
    CircularProgress,
    Dialog,
    DialogActions,
    DialogContent,
    DialogContentText,
    DialogTitle,
    MenuItem,
    Paper,
    Stack,
    Table,
    TableBody,
    TableCell,
    TableContainer,
    TableHead,
    TableRow,
    TextField,
    Typography
} from "@mui/material";

import OwnerReservationService from "../services/OwnerReservationService";
import OwnerRestaurantService from "../services/OwnerRestaurantService";
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

import type { OwnerReservation } from "../types/OwnerReservation";
import type { Redemption } from "../types/Redemption";
import type { Restaurant } from "../types/Restaurant";

// The redemption whose bill the owner is recording.
interface BillTarget {
    redemptionId: number;
    description: string;
}

function OwnerDashboardPage() {

    const navigate = useNavigate();

    const { notify } = useNotification();

    const [restaurants, setRestaurants] =
        useState<Restaurant[]>([]);

    const [selectedRestaurantId, setSelectedRestaurantId] =
        useState<number | null>(null);

    const [reservations, setReservations] =
        useState<OwnerReservation[]>([]);

    // Walk-in offer redemptions for the selected restaurant
    const [redemptions, setRedemptions] =
        useState<Redemption[]>([]);

    const [loading, setLoading] =
        useState(true);

    const [billTarget, setBillTarget] =
        useState<BillTarget | null>(null);

    const [billAmount, setBillAmount] =
        useState(0);

    const [savingBill, setSavingBill] =
        useState(false);

    // Walk-in redemption the owner is about to cancel (confirmation dialog)
    const [cancelTarget, setCancelTarget] =
        useState<Redemption | null>(null);

    const [cancelling, setCancelling] =
        useState(false);

    useEffect(() => {
        loadData();
    }, []);

    useEffect(() => {

        if (selectedRestaurantId !== null)
            loadRedemptions(selectedRestaurantId);

    }, [selectedRestaurantId]); // eslint-disable-line react-hooks/exhaustive-deps

    async function loadRedemptions(restaurantId: number) {

        try {

            const data =
                await RedemptionService.getRestaurantRedemptions(restaurantId);

            setRedemptions(data);

        }
        catch (error) {

            console.error(error);

            setRedemptions([]);

            notify(
                getApiErrorMessage(error, "Failed to load redemptions. Please try again."),
                "error"
            );

        }

    }

    // Reload everything after an owner action.
    async function refresh() {

        await loadData();

        if (selectedRestaurantId !== null)
            await loadRedemptions(selectedRestaurantId);

    }

    async function loadData() {

        try {

            const restaurantData =
                await OwnerRestaurantService.getMyRestaurants();

            const reservationData =
                await OwnerReservationService.getAll();

            setRestaurants(restaurantData);

            // Keep the owner's current selection across reloads.
            setSelectedRestaurantId(current =>
                current !== null && restaurantData.some(r => r.id === current)
                    ? current
                    : restaurantData[0]?.id ?? null
            );

            setReservations(reservationData);

        }
        catch (error: any) {

            if (error.response?.status === 404) {

                setRestaurants([]);

                setSelectedRestaurantId(null);

                setReservations([]);

            }
            else {

                console.error(error);

            }

        }
        finally {

            setLoading(false);

        }

    }

    // Legacy reservation: complete it through its linked redemption.
    async function completedReservation(id: number) {

        const reservation = reservations.find(x => x.id === id);

        if (!reservation?.redemptionId) {

            notify("No redemption found for this reservation.", "warning");

            return;
        }

        openBillDialog(
            reservation.redemptionId,
            `${reservation.customerName} - ${reservation.dealTitle}`
        );

    }

    function openBillDialog(redemptionId: number, description: string) {

        setBillTarget({ redemptionId, description });

        setBillAmount(0);

    }

    // Records the bill; the API calculates the discount and final amount.
    async function completeRedemption() {

        if (billTarget === null)
            return;

        if (!(billAmount > 0)) {

            notify("Please enter the bill amount.", "warning");

            return;
        }

        setSavingBill(true);

        try {

            const result = await RedemptionService.complete(
                billTarget.redemptionId,
                {
                    billAmount
                }
            );

            notify(
                `Redemption completed. Discount ${formatCurrency(result.discountAmount ?? 0, result.currencyCode)}, ` +
                `customer pays ${formatCurrency(result.finalAmount ?? 0, result.currencyCode)}.`,
                "success"
            );

            setBillTarget(null);

            setBillAmount(0);

            await refresh();

        }
        catch (error: any) {

            console.error(error);

            notify(
                error.response?.data?.Message ??
                error.message ??
                "Something went wrong. Please try again.",
                "error"
            );

        }
        finally {

            setSavingBill(false);

        }

    }

    // Runs only after the owner confirms in the dialog. On failure the
    // dialog stays open so the owner can retry.
    async function confirmCancelRedemption() {

        if (cancelTarget === null)
            return;

        setCancelling(true);

        try {

            await RedemptionService.cancel(cancelTarget.id);

            notify("Redemption cancelled.", "success");

            setCancelTarget(null);

            await refresh();

        }
        catch (error) {

            console.error(error);

            notify(
                getApiErrorMessage(error, "Unable to cancel this redemption. Please try again."),
                "error"
            );

        }
        finally {

            setCancelling(false);

        }

    }

    async function noShowReservation(id: number) {

        await OwnerReservationService.noShow(id);

        refresh();

    }

    async function cancelReservation(id: number) {

        await OwnerReservationService.cancel(id);

        refresh();

    }

    function getChipColor(
        status: string
    ): "success" | "warning" | "error" | "info" | "default" {

        switch (status) {

            case "Pending":
                return "warning";

            case "Confirmed":
                return "success";

            case "Arrived":
                return "info";

            case "Completed":
                return "success";

            case "Rejected":
                return "error";

            case "Cancelled":
                return "default";

            case "NoShow":
                return "error";

            default:
                return "default";

        }

    }

    if (loading) {

        return <CircularProgress />;

    }

    return (

        <>

            <Stack
                direction="row"
                spacing={2}
                useFlexGap
                sx={{
                    justifyContent: "space-between",
                    alignItems: "center",
                    flexWrap: "wrap",
                    mb: 3
                }}
            >

                <Typography variant="h4">
                    Owner Dashboard
                </Typography>

                {restaurants.length > 0 && (

                    <Stack
                        direction="row"
                        spacing={1.5}
                        useFlexGap
                        sx={{ flexWrap: "wrap" }}
                    >

                        <Button
                            variant="contained"
                            onClick={() =>
                                navigate(
                                    `/owner/deals?restaurantId=${selectedRestaurantId}`
                                )
                            }
                        >
                            Manage Deals
                        </Button>

                        <Button
                            variant="contained"
                            onClick={() =>
                                navigate(
                                    `/owner/opening-hours?restaurantId=${selectedRestaurantId}`
                                )
                            }
                        >
                            Opening Hours
                        </Button>

                        <Button
                            variant="contained"
                            color="secondary"
                            onClick={() =>
                                navigate(
                                    `/owner/restaurant?restaurantId=${selectedRestaurantId}`
                                )
                            }
                        >
                            Edit Restaurant
                        </Button>

                        <Button
                            variant="contained"
                            onClick={() =>
                                navigate(
                                    `/owner/menu-categories?restaurantId=${selectedRestaurantId}`
                                )
                            }
                        >
                            Menu Categories
                        </Button>

                        <Button
                            variant="contained"
                            onClick={() =>
                                navigate(
                                    `/owner/menu-items?restaurantId=${selectedRestaurantId}`
                                )
                            }
                        >
                            Menu Items
                        </Button>

                    </Stack>

                )}

            </Stack>


            {restaurants.length === 0 && (

                <Paper sx={{ p: 4 }}>

                    <Typography
                        variant="h5"
                        gutterBottom
                    >
                        No Restaurant Assigned
                    </Typography>

                    <Typography sx={{ mb: 2 }}>
                        You don't have a restaurant yet. Create one to start
                        managing deals, reservations and redemptions.
                    </Typography>

                    <Button
                        variant="contained"
                        onClick={() => navigate("/owner/restaurant/new")}
                    >
                        Create Restaurant
                    </Button>

                </Paper>

            )}


            {restaurants.length > 0 && (

                <>

                    <Typography
                        variant="h5"
                        sx={{ mb: 2 }}
                    >
                        Your Restaurants
                    </Typography>

                    <TextField
                        select
                        label="Select Restaurant"
                        value={selectedRestaurantId ?? ""}
                        onChange={(e) =>
                            setSelectedRestaurantId(
                                Number(e.target.value)
                            )
                        }
                        sx={{
                            mb: 3,
                            minWidth: 300
                        }}
                    >

                        {restaurants.map((restaurant) => (

                            <MenuItem
                                key={restaurant.id}
                                value={restaurant.id}
                            >
                                {restaurant.name}
                            </MenuItem>

                        ))}

                    </TextField>


                    <Stack spacing={2} sx={{ mb: 4 }}>

                        {restaurants.map((restaurant) => (

                            <Paper
                                key={restaurant.id}
                                sx={{ p: 3 }}
                            >

                                <Typography variant="h5">
                                    {restaurant.name}
                                </Typography>

                                <Typography>
                                    {restaurant.address}
                                </Typography>

                                <Typography>
                                    {restaurant.phoneNumber}
                                </Typography>

                                <Typography>
                                    {restaurant.email}
                                </Typography>

                            </Paper>

                        ))}

                    </Stack>


                    <Typography
                        variant="h5"
                        sx={{ mb: 1 }}
                    >
                        Walk-in Offer Redemptions
                    </Typography>

                    <Typography
                        color="text.secondary"
                        sx={{ mb: 2 }}
                    >
                        Customers who claimed an offer at the selected restaurant. When they visit,
                        check the redemption number, then record the bill to apply the discount.
                    </Typography>

                    <TableContainer component={Paper} sx={{ mb: 4 }}>

                        <Table>

                            <TableHead>

                                <TableRow>

                                    <TableCell>#</TableCell>
                                    <TableCell>Customer</TableCell>
                                    <TableCell>Offer</TableCell>
                                    <TableCell>Arrival</TableCell>
                                    <TableCell>Guests</TableCell>
                                    <TableCell>Status</TableCell>
                                    <TableCell>Bill</TableCell>
                                    <TableCell>Actions</TableCell>

                                </TableRow>

                            </TableHead>

                            <TableBody>

                                {redemptions.length === 0 && (

                                    <TableRow>
                                        <TableCell colSpan={8}>
                                            No redemptions yet for this restaurant.
                                        </TableCell>
                                    </TableRow>

                                )}

                                {redemptions.map((redemption) => (

                                    <TableRow key={redemption.id}>

                                        <TableCell>
                                            {redemption.id}
                                        </TableCell>

                                        <TableCell>
                                            <Typography variant="body2">{redemption.customerName}</Typography>
                                            <Typography variant="caption" color="text.secondary" component="div">
                                                {redemption.customerPhone || "No phone"}
                                            </Typography>
                                            <Typography variant="caption" color="text.secondary" component="div">
                                                {redemption.customerEmail}
                                            </Typography>
                                        </TableCell>

                                        <TableCell>
                                            {redemption.dealTitle}
                                            {redemption.reservationId !== null && (
                                                <Typography variant="caption" color="text.secondary" component="div">
                                                    From reservation #{redemption.reservationId}
                                                </Typography>
                                            )}
                                        </TableCell>

                                        <TableCell>
                                            {formatDate(redemption.arrivalDate)}
                                            <Typography variant="caption" color="text.secondary" component="div">
                                                {formatTime(redemption.arrivalTime)}
                                            </Typography>
                                        </TableCell>

                                        <TableCell>
                                            {redemption.guestCount}
                                        </TableCell>

                                        <TableCell>
                                            <Chip
                                                label={getRedemptionStatusLabel(redemption.status)}
                                                color={getRedemptionStatusColor(redemption.status)}
                                            />
                                        </TableCell>

                                        <TableCell>
                                            {redemption.billAmount != null ? (
                                                <>
                                                    <Typography variant="body2">
                                                        {formatCurrency(redemption.billAmount, redemption.currencyCode)}
                                                    </Typography>
                                                    <Typography variant="caption" color="text.secondary" component="div">
                                                        Discount {formatCurrency(redemption.discountAmount ?? 0, redemption.currencyCode)}
                                                    </Typography>
                                                    <Typography variant="caption" color="text.secondary" component="div">
                                                        Paid {formatCurrency(redemption.finalAmount ?? 0, redemption.currencyCode)}
                                                    </Typography>
                                                </>
                                            ) : "-"}
                                        </TableCell>

                                        <TableCell>

                                            {redemption.status === RedemptionStatus.Redeemed && (

                                                <Stack
                                                    direction="row"
                                                    spacing={1}
                                                    flexWrap="wrap"
                                                >

                                                    <Button
                                                        size="small"
                                                        variant="contained"
                                                        color="success"
                                                        onClick={() =>
                                                            openBillDialog(
                                                                redemption.id,
                                                                `#${redemption.id} ${redemption.customerName} - ${redemption.dealTitle}`
                                                            )
                                                        }
                                                    >
                                                        Record Bill
                                                    </Button>

                                                    <Button
                                                        size="small"
                                                        variant="outlined"
                                                        color="error"
                                                        onClick={() =>
                                                            setCancelTarget(redemption)
                                                        }
                                                    >
                                                        Cancel
                                                    </Button>

                                                </Stack>

                                            )}

                                        </TableCell>

                                    </TableRow>

                                ))}

                            </TableBody>

                        </Table>

                    </TableContainer>


                    <Typography
                        variant="h5"
                        sx={{ mb: 1 }}
                    >
                        Legacy Reservations
                    </Typography>

                    <Typography
                        color="text.secondary"
                        sx={{ mb: 2 }}
                    >
                        Reservations made before walk-in offers, across all your restaurants.
                    </Typography>


                    <TableContainer component={Paper}>

                        <Table>

                            <TableHead>

                                <TableRow>

                                    <TableCell>Customer</TableCell>
                                    <TableCell>Deal</TableCell>
                                    <TableCell>Date</TableCell>
                                    <TableCell>Time</TableCell>
                                    <TableCell>Guests</TableCell>
                                    <TableCell>Status</TableCell>
                                    <TableCell>Actions</TableCell>

                                </TableRow>

                            </TableHead>


                            <TableBody>

                                {reservations.map((reservation) => (

                                    <TableRow key={reservation.id}>

                                        <TableCell>
                                            {reservation.customerName}
                                        </TableCell>

                                        <TableCell>
                                            {reservation.dealTitle}
                                        </TableCell>

                                        <TableCell>
                                            {reservation.reservationDate}
                                        </TableCell>

                                        <TableCell>
                                            {reservation.reservationTime}
                                        </TableCell>

                                        <TableCell>
                                            {reservation.guestCount}
                                        </TableCell>

                                        <TableCell>

                                            <Chip
                                                label={reservation.status}
                                                color={getChipColor(
                                                    reservation.status
                                                )}
                                            />

                                        </TableCell>


                                        <TableCell>

                                            <Stack
                                                direction="row"
                                                spacing={1}
                                                flexWrap="wrap"
                                            >

                                                {reservation.status === "Pending" && (

                                                    <>

                                                        <Button
                                                            size="small"
                                                            variant="contained"
                                                            color="success"
                                                            onClick={() =>
                                                                completedReservation(
                                                                    reservation.id
                                                                )
                                                            }
                                                        >
                                                            Redeem Offer
                                                        </Button>


                                                        <Button
                                                            size="small"
                                                            variant="contained"
                                                            color="warning"
                                                            onClick={() =>
                                                                noShowReservation(
                                                                    reservation.id
                                                                )
                                                            }
                                                        >
                                                            No Show
                                                        </Button>


                                                        <Button
                                                            size="small"
                                                            variant="outlined"
                                                            color="error"
                                                            onClick={() =>
                                                                cancelReservation(
                                                                    reservation.id
                                                                )
                                                            }
                                                        >
                                                            Cancel
                                                        </Button>

                                                    </>

                                                )}

                                            </Stack>

                                        </TableCell>

                                    </TableRow>

                                ))}

                            </TableBody>

                        </Table>

                    </TableContainer>

                </>

            )}


            <Dialog
                open={billTarget !== null}
                onClose={() => !savingBill && setBillTarget(null)}
            >

                <DialogTitle>
                    Record Bill
                </DialogTitle>


                <DialogContent>

                    <Typography sx={{ mt: 1 }}>
                        {billTarget?.description}
                    </Typography>

                    <TextField
                        fullWidth
                        label="Original Bill Amount"
                        type="number"
                        value={billAmount}
                        onChange={(e) =>
                            setBillAmount(Number(e.target.value))
                        }
                        helperText="The offer discount is calculated automatically."
                        slotProps={{ htmlInput: { min: 0, step: "0.01" } }}
                        sx={{ mt: 2 }}
                    />

                </DialogContent>


                <DialogActions>

                    <Button
                        onClick={() =>
                            setBillTarget(null)
                        }
                        disabled={savingBill}
                    >
                        Cancel
                    </Button>


                    <Button
                        variant="contained"
                        onClick={completeRedemption}
                        disabled={savingBill}
                    >
                        {savingBill ? "Saving..." : "Complete Redemption"}
                    </Button>

                </DialogActions>

            </Dialog>


            <Dialog
                open={cancelTarget !== null}
                onClose={() => !cancelling && setCancelTarget(null)}
            >

                <DialogTitle>
                    Cancel Redemption
                </DialogTitle>


                <DialogContent>

                    <DialogContentText>
                        Cancel redemption #{cancelTarget?.id} for "{cancelTarget?.dealTitle}"
                        {cancelTarget?.customerName ? ` (${cancelTarget.customerName})` : ""}?
                    </DialogContentText>

                    <DialogContentText sx={{ mt: 2 }}>
                        The offer will be released for other customers. This cannot be undone.
                    </DialogContentText>

                </DialogContent>


                <DialogActions>

                    <Button
                        onClick={() =>
                            setCancelTarget(null)
                        }
                        disabled={cancelling}
                    >
                        Keep Redemption
                    </Button>


                    <Button
                        variant="contained"
                        color="error"
                        onClick={confirmCancelRedemption}
                        disabled={cancelling}
                    >
                        {cancelling ? "Cancelling…" : "Cancel Redemption"}
                    </Button>

                </DialogActions>

            </Dialog>

        </>

    );

}

export default OwnerDashboardPage;