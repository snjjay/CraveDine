import { useEffect, useRef, useState, type FormEvent } from "react";
import { useNavigate, useSearchParams } from "react-router-dom";

import {
    Alert,
    Box,
    Button,
    Card,
    CardContent,
    Chip,
    CircularProgress,
    LinearProgress,
    Link,
    MenuItem,
    Stack,
    TextField,
    Typography
} from "@mui/material";

import SurpriseBagReservationActionDialog, {
    type ReservationAction
} from "../components/surpriseBags/SurpriseBagReservationActionDialog";
import OwnerRestaurantService from "../services/OwnerRestaurantService";
import OwnerSurpriseBagReservationService from "../services/OwnerSurpriseBagReservationService";
import OwnerSurpriseBagService from "../services/OwnerSurpriseBagService";
import { useNotification } from "../features/notifications/NotificationContext";
import { getApiErrorMessage } from "../utils/apiError";
import { formatCurrency } from "../utils/currency";
import {
    SurpriseBagReservationStatus,
    formatReservationTimestamp,
    getSurpriseBagReservationStatusColor,
    getSurpriseBagReservationStatusLabel
} from "../utils/surpriseBag";
import { formatDate, formatTime12Hour } from "../utils/time";

import type { OwnerSurpriseBag } from "../types/OwnerSurpriseBag";
import type { OwnerSurpriseBagReservation } from "../types/OwnerSurpriseBagReservation";
import type { Restaurant } from "../types/Restaurant";

// Same limit as the API (CollectSurpriseBagReservationValidator).
const MAX_CODE_LENGTH = 12;

const STATUS_OPTIONS = Object.values(SurpriseBagReservationStatus);

interface ActionTarget {
    action: ReservationAction;
    reservation: OwnerSurpriseBagReservation;
}

// What the amount means for the restaurant at each status.
function getAmountLabel(status: number): string {

    switch (status) {

        case SurpriseBagReservationStatus.Reserved:
            return "Due at pickup";

        case SurpriseBagReservationStatus.Collected:
            return "Paid at pickup";

        default:
            return "Total (not paid)";

    }

}

function getDoneMessage(action: ReservationAction, updated: OwnerSurpriseBagReservation): string {

    switch (action) {

        case "collect":
            return `Collected: ${updated.quantity} × ${updated.bagTitle} for ${updated.customerName}.`;

        case "noShow":
            return `${updated.customerName}'s reservation was marked as a no-show.`;

        case "cancel":
            return `${updated.customerName}'s reservation was cancelled and the bag released.`;

    }

}

function OwnerSurpriseBagReservationsPage() {

    const navigate = useNavigate();

    const { notify } = useNotification();

    const [searchParams] = useSearchParams();

    // Chosen with the restaurant picker on the Owner Dashboard.
    const restaurantId =
        Number(searchParams.get("restaurantId")) || null;

    // Optional: start with one bag selected (from the bag list).
    const bagIdFromUrl =
        Number(searchParams.get("bagId")) || null;

    const [restaurants, setRestaurants] =
        useState<Restaurant[]>([]);

    // The restaurant's bags (options for the bag filter).
    const [bags, setBags] =
        useState<OwnerSurpriseBag[]>([]);

    // null until the reservations have loaded once (or if that failed).
    const [reservations, setReservations] =
        useState<OwnerSurpriseBagReservation[] | null>(null);

    const [loading, setLoading] =
        useState(restaurantId !== null);

    // A reload after a filter change or an action (the list stays shown).
    const [fetching, setFetching] =
        useState(false);

    // Why the page couldn't be loaded the first time (shown with Try Again).
    const [loadError, setLoadError] =
        useState<string | null>(null);

    // Incremented to load the reservations again.
    const [reloadKey, setReloadKey] =
        useState(0);

    // Filters: pickup date and status are applied by the API; the bag
    // filter is applied to the loaded reservations (surpriseBagId).
    const [pickupDate, setPickupDate] =
        useState("");

    const [status, setStatus] =
        useState<number | "">("");

    const [bagId, setBagId] =
        useState<number | "">(bagIdFromUrl ?? "");

    // Collect by confirmation code
    const [code, setCode] =
        useState("");

    const [lookingUp, setLookingUp] =
        useState(false);

    const [lookupError, setLookupError] =
        useState<string | null>(null);

    // Status change being confirmed (dialog)
    const [actionTarget, setActionTarget] =
        useState<ActionTarget | null>(null);

    // Another restaurant chosen while the page is open: its reservations
    // (and any open dialog) must not carry over.
    const [loadedFor, setLoadedFor] =
        useState(restaurantId);

    if (restaurantId !== loadedFor) {
        setLoadedFor(restaurantId);
        setReservations(null);
        setBags([]);
        setBagId(bagIdFromUrl ?? "");
        setLoading(restaurantId !== null);
        setLoadError(null);
        setActionTarget(null);
        setLookupError(null);
    }

    // Restaurant whose reservations have loaded at least once.
    const hasLoaded = useRef<number | null>(null);

    // The first failed load is shown on the page; a failed reload keeps
    // the reservations already shown and is a notification. A response
    // for an older request (e.g. a filter changed again) is ignored.
    useEffect(() => {

        if (restaurantId === null)
            return;

        let active = true;

        Promise.all([
            OwnerRestaurantService.getMyRestaurants(),
            OwnerSurpriseBagService.getMyBags(restaurantId),
            OwnerSurpriseBagReservationService.getForRestaurant(restaurantId, {
                pickupDate: pickupDate || undefined,
                status: status || undefined
            })
        ])
            .then(([restaurantData, bagData, reservationData]) => {

                if (!active)
                    return;

                hasLoaded.current = restaurantId;

                setRestaurants(restaurantData);
                setBags(bagData);
                setReservations(reservationData);
                setLoadError(null);

            })
            .catch(error => {

                console.error(error);

                if (!active)
                    return;

                const message =
                    getApiErrorMessage(error, "We couldn't load the reservations right now. Please try again.");

                if (hasLoaded.current === restaurantId)
                    notify(message, "error");
                else
                    setLoadError(message);

            })
            .finally(() => {

                if (active) {
                    setLoading(false);
                    setFetching(false);
                }

            });

        return () => {
            active = false;
        };

    }, [restaurantId, pickupDate, status, reloadKey, notify]);

    function reloadReservations() {

        setFetching(true);
        setReloadKey(key => key + 1);

    }

    function handleRetry() {

        setLoading(true);
        setLoadError(null);

        reloadReservations();

    }

    function handlePickupDateChange(value: string) {

        setFetching(true);
        setPickupDate(value);

    }

    function handleStatusChange(value: number | "") {

        setFetching(true);
        setStatus(value);

    }

    function handleClearFilters() {

        if (pickupDate || status)
            setFetching(true);

        setPickupDate("");
        setStatus("");
        setBagId("");

    }

    // Finds the code among this restaurant's reservations (freshly loaded,
    // ignoring the filters) so the owner can check who it is before it's
    // marked as collected. Codes are matched like the API: trimmed, any
    // case.
    async function handleFindCode(event: FormEvent) {

        event.preventDefault();

        const trimmed = code.trim();

        if (!trimmed || lookingUp || restaurantId === null)
            return;

        setLookingUp(true);
        setLookupError(null);

        try {

            const all =
                await OwnerSurpriseBagReservationService.getForRestaurant(restaurantId);

            const match = all.find(r =>
                r.confirmationCode.toUpperCase() === trimmed.toUpperCase());

            if (match)
                setActionTarget({ action: "collect", reservation: match });
            else
                setLookupError(`No reservation with code "${trimmed}" was found for this restaurant. Check the code and the restaurant selected.`);

        }
        catch (error) {

            console.error(error);

            setLookupError(getApiErrorMessage(error, "Unable to look up this code. Please try again."));

        }
        finally {

            setLookingUp(false);

        }

    }

    function handleActionDone(updated: OwnerSurpriseBagReservation) {

        if (actionTarget) {

            notify(getDoneMessage(actionTarget.action, updated), "success");

            if (actionTarget.action === "collect")
                setCode("");

        }

        setActionTarget(null);

        reloadReservations();

    }

    // No restaurant chosen: the restaurant picker is on the dashboard.
    if (restaurantId === null) {

        return (

            <Stack spacing={2} sx={{ alignItems: "flex-start" }}>

                <Typography variant="h4" component="h1">
                    Surprise Bag Reservations
                </Typography>

                <Alert severity="info">
                    Choose a restaurant on the Owner Dashboard to see its Surprise Bag reservations.
                </Alert>

                <Button variant="contained" onClick={() => navigate("/owner")}>
                    Go to Owner Dashboard
                </Button>

            </Stack>

        );

    }

    if (loading)
        return <CircularProgress aria-label="Loading reservations" />;

    const restaurant =
        restaurants.find(r => r.id === restaurantId) ?? null;

    const loaded = reservations !== null && loadError === null;

    // Bag filter: only a bag of this restaurant counts.
    const selectedBagId =
        bagId !== "" && bags.some(b => b.id === bagId) ? bagId : "";

    const visibleReservations = (reservations ?? []).filter(r =>
        selectedBagId === "" || r.surpriseBagId === selectedBagId);

    const hasFilters = pickupDate !== "" || status !== "" || selectedBagId !== "";

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

                <Box>

                    <Typography variant="h4" component="h1">
                        Surprise Bag Reservations
                    </Typography>

                    {restaurant && (
                        <Typography color="text.secondary">
                            {restaurant.name}
                        </Typography>
                    )}

                </Box>

                <Button
                    variant="outlined"
                    onClick={() => navigate(`/owner/surprise-bags?restaurantId=${restaurantId}`)}
                >
                    Manage Surprise Bags
                </Button>

            </Stack>

            {!loaded ? (

                <Stack spacing={2} sx={{ alignItems: "flex-start" }}>

                    <Alert severity="error">
                        {loadError ?? "We couldn't load the reservations right now. Please try again."}
                    </Alert>

                    <Button variant="contained" onClick={handleRetry}>
                        Try Again
                    </Button>

                </Stack>

            ) : (

                <Stack spacing={3}>

                    {/* Collect by confirmation code */}
                    <Card component="section" aria-labelledby="collect-heading">

                        <CardContent>

                            <Typography variant="h6" component="h2" id="collect-heading">
                                Collect a bag
                            </Typography>

                            <Typography variant="body2" color="text.secondary" sx={{ mb: 2 }}>
                                Enter the code the customer shows. You'll see their reservation and confirm before it's marked as collected.
                            </Typography>

                            <Stack
                                component="form"
                                onSubmit={handleFindCode}
                                direction={{ xs: "column", sm: "row" }}
                                spacing={1.5}
                                sx={{ alignItems: { sm: "flex-start" } }}
                            >

                                <TextField
                                    label="Confirmation code"
                                    value={code}
                                    onChange={(e) => {
                                        setCode(e.target.value);
                                        setLookupError(null);
                                    }}
                                    disabled={lookingUp}
                                    slotProps={{
                                        htmlInput: {
                                            maxLength: MAX_CODE_LENGTH + 4,
                                            autoCapitalize: "characters",
                                            autoComplete: "off",
                                            spellCheck: false,
                                            style: { fontFamily: "monospace", letterSpacing: "0.08em" }
                                        }
                                    }}
                                    sx={{ minWidth: { sm: 260 } }}
                                />

                                <Button
                                    type="submit"
                                    variant="contained"
                                    disabled={lookingUp || code.trim() === "" || code.trim().length > MAX_CODE_LENGTH}
                                    sx={{ minHeight: 56 }}
                                >
                                    {lookingUp ? "Finding..." : "Find Reservation"}
                                </Button>

                            </Stack>

                            {lookupError && (
                                <Alert severity="warning" role="alert" sx={{ mt: 2 }}>
                                    {lookupError}
                                </Alert>
                            )}

                        </CardContent>

                    </Card>

                    {/* Filters */}
                    <Stack
                        component="section"
                        aria-label="Filter reservations"
                        direction={{ xs: "column", md: "row" }}
                        spacing={2}
                        sx={{ alignItems: { md: "center" } }}
                    >

                        <TextField
                            label="Pickup date"
                            type="date"
                            value={pickupDate}
                            onChange={(e) => handlePickupDateChange(e.target.value)}
                            slotProps={{ inputLabel: { shrink: true } }}
                            sx={{ minWidth: 180 }}
                        />

                        <TextField
                            select
                            label="Status"
                            value={status}
                            onChange={(e) => handleStatusChange(e.target.value === "" ? "" : Number(e.target.value))}
                            // Show "All statuses" (the empty value) instead of a blank field.
                            slotProps={{ select: { displayEmpty: true }, inputLabel: { shrink: true } }}
                            sx={{ minWidth: 180 }}
                        >
                            <MenuItem value="">All statuses</MenuItem>
                            {STATUS_OPTIONS.map(s => (
                                <MenuItem key={s} value={s}>
                                    {getSurpriseBagReservationStatusLabel(s)}
                                </MenuItem>
                            ))}
                        </TextField>

                        <TextField
                            select
                            label="Bag"
                            value={selectedBagId}
                            onChange={(e) => setBagId(e.target.value === "" ? "" : Number(e.target.value))}
                            // Show "All bags" (the empty value) instead of a blank field.
                            slotProps={{ select: { displayEmpty: true }, inputLabel: { shrink: true } }}
                            sx={{ minWidth: 220, flex: { md: 1 } }}
                        >
                            <MenuItem value="">All bags</MenuItem>
                            {bags.map(b => (
                                <MenuItem key={b.id} value={b.id}>
                                    {b.title} · {formatDate(b.pickupDate)}
                                </MenuItem>
                            ))}
                        </TextField>

                        <Button
                            onClick={handleClearFilters}
                            disabled={!hasFilters}
                            sx={{ alignSelf: { xs: "flex-start", md: "center" } }}
                        >
                            Clear Filters
                        </Button>

                    </Stack>

                    <Box>

                        {fetching && <LinearProgress aria-label="Updating reservations" sx={{ mb: 2 }} />}

                        {visibleReservations.length === 0 ? (

                            <Typography>
                                {hasFilters
                                    ? "No reservations match these filters."
                                    : "No Surprise Bag reservations yet."}
                            </Typography>

                        ) : (

                            <Stack spacing={2}>

                                <Typography variant="body2" color="text.secondary">
                                    {visibleReservations.length} {visibleReservations.length === 1 ? "reservation" : "reservations"}
                                </Typography>

                                {visibleReservations.map(r => {

                                    const isReserved = r.status === SurpriseBagReservationStatus.Reserved;

                                    return (

                                        <Card
                                            key={r.id}
                                            component="article"
                                            aria-label={`${r.customerName}, ${r.bagTitle}, ${getSurpriseBagReservationStatusLabel(r.status)}`}
                                        >

                                            <CardContent>

                                                <Stack
                                                    direction="row"
                                                    sx={{ justifyContent: "space-between", alignItems: "flex-start", gap: 2 }}
                                                >

                                                    <Box sx={{ minWidth: 0 }}>

                                                        <Typography variant="h6" component="h3" sx={{ overflowWrap: "anywhere" }}>
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

                                                    </Box>

                                                    <Chip
                                                        label={getSurpriseBagReservationStatusLabel(r.status)}
                                                        color={getSurpriseBagReservationStatusColor(r.status)}
                                                        sx={{ flexShrink: 0 }}
                                                    />

                                                </Stack>

                                                <Typography sx={{ mt: 1.5, fontWeight: 600 }}>
                                                    {r.customerName}
                                                </Typography>

                                                <Typography variant="body2" sx={{ overflowWrap: "anywhere" }}>
                                                    {r.customerPhone && (
                                                        <Link href={`tel:${r.customerPhone}`}>{r.customerPhone}</Link>
                                                    )}
                                                    {r.customerPhone && r.customerEmail && " · "}
                                                    {r.customerEmail && (
                                                        <Link href={`mailto:${r.customerEmail}`}>{r.customerEmail}</Link>
                                                    )}
                                                </Typography>

                                                <Typography sx={{ mt: 1 }}>
                                                    Pick up: {formatDate(r.pickupDate)}, {formatTime12Hour(r.pickupStartTime)} – {formatTime12Hour(r.pickupEndTime)}
                                                </Typography>

                                                <Typography>
                                                    {getAmountLabel(r.status)}: <strong>{formatCurrency(r.totalPrice, r.currencyCode)}</strong>
                                                </Typography>

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

                                                {isReserved ? (

                                                    <Stack
                                                        direction="row"
                                                        spacing={1.5}
                                                        useFlexGap
                                                        sx={{ mt: 2, flexWrap: "wrap" }}
                                                    >

                                                        <Button
                                                            variant="contained"
                                                            onClick={() => setActionTarget({ action: "collect", reservation: r })}
                                                            aria-label={`Collect ${r.customerName}'s reservation`}
                                                        >
                                                            Collect
                                                        </Button>

                                                        <Button
                                                            variant="outlined"
                                                            onClick={() => setActionTarget({ action: "noShow", reservation: r })}
                                                            aria-label={`Mark ${r.customerName}'s reservation as a no-show`}
                                                        >
                                                            No-show
                                                        </Button>

                                                        <Button
                                                            variant="outlined"
                                                            color="error"
                                                            onClick={() => setActionTarget({ action: "cancel", reservation: r })}
                                                            aria-label={`Cancel ${r.customerName}'s reservation`}
                                                        >
                                                            Cancel
                                                        </Button>

                                                    </Stack>

                                                ) : (

                                                    <Typography variant="caption" color="text.secondary" component="p" sx={{ mt: 1.5 }}>
                                                        Final status - can't be changed.
                                                    </Typography>

                                                )}

                                            </CardContent>

                                        </Card>

                                    );

                                })}

                            </Stack>

                        )}

                    </Box>

                </Stack>

            )}

            {/* Mounted only while open, so each opening starts fresh. */}
            {actionTarget && (
                <SurpriseBagReservationActionDialog
                    open
                    action={actionTarget.action}
                    reservation={actionTarget.reservation}
                    onClose={() => setActionTarget(null)}
                    onDone={handleActionDone}
                    onFailed={reloadReservations}
                />
            )}

        </>

    );

}

export default OwnerSurpriseBagReservationsPage;
