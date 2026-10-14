import { useEffect, useRef, useState } from "react";
import { useNavigate, useSearchParams } from "react-router-dom";
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
    Stack,
    Typography
} from "@mui/material";

import SurpriseBagFormDialog from "../components/surpriseBags/SurpriseBagFormDialog";
import OwnerRestaurantService from "../services/OwnerRestaurantService";
import OwnerSurpriseBagService from "../services/OwnerSurpriseBagService";
import { useNotification } from "../features/notifications/NotificationContext";
import { getApiErrorMessage } from "../utils/apiError";
import { formatCurrency } from "../utils/currency";
import { getSurpriseBagCategoryLabel } from "../utils/surpriseBag";
import { formatDate, formatTime12Hour } from "../utils/time";

import type { OwnerSurpriseBag } from "../types/OwnerSurpriseBag";
import type { Restaurant } from "../types/Restaurant";

// No response (connection lost or timed out): the API may still have made
// the change, so it can't be reported as failed.
function isUnconfirmed(error: unknown): boolean {

    return axios.isAxiosError(error) && !error.response;

}

// The bag being created (bag: null) or edited in the form dialog.
interface FormTarget {
    bag: OwnerSurpriseBag | null;
}

function OwnerSurpriseBagsPage() {

    const navigate = useNavigate();

    const { notify } = useNotification();

    const [searchParams] = useSearchParams();

    // Chosen with the restaurant picker on the Owner Dashboard.
    const restaurantId =
        Number(searchParams.get("restaurantId")) || null;

    const [restaurants, setRestaurants] =
        useState<Restaurant[]>([]);

    // null until the bags have loaded once (or if that failed).
    const [bags, setBags] =
        useState<OwnerSurpriseBag[] | null>(null);

    const [loading, setLoading] =
        useState(restaurantId !== null);

    // Why the page couldn't be loaded the first time (shown with Try Again).
    const [loadError, setLoadError] =
        useState<string | null>(null);

    // Incremented to load the bags again (retry, after a change).
    const [reloadKey, setReloadKey] =
        useState(0);

    const [formTarget, setFormTarget] =
        useState<FormTarget | null>(null);

    // Bag the owner is about to deactivate (confirmation dialog)
    const [deactivateTarget, setDeactivateTarget] =
        useState<OwnerSurpriseBag | null>(null);

    const [deactivating, setDeactivating] =
        useState(false);

    // Bag being reactivated (its button shows progress).
    const [reactivatingId, setReactivatingId] =
        useState<number | null>(null);

    // Bag whose latest details are being loaded before Edit opens.
    const [openingEditId, setOpeningEditId] =
        useState<number | null>(null);

    // A deactivation/reactivation whose outcome is unknown (no response).
    // Shown on the page until dismissed, so a notification from the list
    // reload (e.g. it fails too) can't hide it.
    const [unconfirmedNotice, setUnconfirmedNotice] =
        useState<string | null>(null);

    // Another restaurant chosen while the page is open: its bags (and any
    // open dialog) must not carry over.
    const [bagsFor, setBagsFor] =
        useState(restaurantId);

    if (restaurantId !== bagsFor) {
        setBagsFor(restaurantId);
        setBags(null);
        setLoading(restaurantId !== null);
        setLoadError(null);
        setFormTarget(null);
        setDeactivateTarget(null);
        setUnconfirmedNotice(null);
    }

    // Restaurant whose bags have loaded at least once.
    const hasLoaded = useRef<number | null>(null);

    // The first failed load is shown on the page; a failed reload keeps
    // the bags already shown and is a notification. A response that
    // arrives after leaving the page is ignored.
    useEffect(() => {

        if (restaurantId === null)
            return;

        let active = true;

        Promise.all([
            OwnerRestaurantService.getMyRestaurants(),
            OwnerSurpriseBagService.getMyBags(restaurantId)
        ])
            .then(([restaurantData, bagData]) => {

                if (!active)
                    return;

                hasLoaded.current = restaurantId;

                setRestaurants(restaurantData);
                setBags(bagData);
                setLoadError(null);

            })
            .catch(error => {

                console.error(error);

                if (!active)
                    return;

                const message =
                    getApiErrorMessage(error, "We couldn't load your Surprise Bags right now. Please try again.");

                if (hasLoaded.current === restaurantId)
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

    }, [restaurantId, reloadKey, notify]);

    function reloadBags() {

        setReloadKey(key => key + 1);

    }

    function handleRetry() {

        setLoading(true);
        setLoadError(null);

        reloadBags();

    }

    // warning: a new bag was created but its image wasn't uploaded.
    function handleSaved(bag: OwnerSurpriseBag, warning?: string) {

        if (warning)
            notify(warning, "warning");
        else
            notify(
                formTarget?.bag ? "Surprise Bag updated." : `"${bag.title}" created.`,
                "success"
            );

        setFormTarget(null);

        reloadBags();

    }

    // A save that lost its connection may still have happened, so the
    // list is reloaded whenever the form closes.
    function handleFormClose() {

        setFormTarget(null);

        reloadBags();

    }

    // The form starts from the bag's current details, not what this page
    // last loaded, so saving can't overwrite newer changes with stale data.
    // If they can't be loaded, the form isn't opened.
    async function handleEdit(bag: OwnerSurpriseBag) {

        setOpeningEditId(bag.id);

        try {

            const current = await OwnerSurpriseBagService.getById(bag.id);

            setFormTarget({ bag: current });

        }
        catch (error) {

            console.error(error);

            notify(
                getApiErrorMessage(error, "Unable to load the latest details of this Surprise Bag. Please try again."),
                "error"
            );

        }
        finally {

            setOpeningEditId(null);

        }

    }

    async function handleConfirmDeactivate() {

        if (!deactivateTarget)
            return;

        setDeactivating(true);

        try {

            await OwnerSurpriseBagService.deactivate(deactivateTarget.id);

            notify("Surprise Bag deactivated. Existing reservations are kept.", "success");

        }
        catch (error) {

            console.error(error);

            if (isUnconfirmed(error))
                setUnconfirmedNotice(
                    `We couldn't confirm that "${deactivateTarget.title}" was deactivated because the connection was interrupted. It may still have gone through - check its status in the list below before trying again.`
                );
            else
                notify(
                    getApiErrorMessage(error, "Unable to deactivate this Surprise Bag. Please try again."),
                    "error"
                );

        }
        finally {

            setDeactivating(false);
            setDeactivateTarget(null);

            reloadBags();

        }

    }

    // Reactivation is a full update with isActive: true. The bag is read
    // again first so the update resends its current details, not what
    // this page last loaded.
    async function handleReactivate(bag: OwnerSurpriseBag) {

        setReactivatingId(bag.id);

        // The update was sent: from here a lost connection leaves the
        // outcome unknown (before it, nothing has changed).
        let updateSent = false;

        try {

            const current = await OwnerSurpriseBagService.getById(bag.id);

            updateSent = true;

            await OwnerSurpriseBagService.update(current.id, {
                title: current.title,
                category: current.category,
                description: current.description,
                conditions: current.conditions,
                price: current.price,
                estimatedValue: current.estimatedValue,
                quantityTotal: current.quantityTotal,
                maxPerCustomer: current.maxPerCustomer,
                pickupDate: current.pickupDate,
                pickupStartTime: current.pickupStartTime,
                pickupEndTime: current.pickupEndTime,
                isActive: true
            });

            notify("Surprise Bag reactivated.", "success");

        }
        catch (error) {

            console.error(error);

            if (updateSent && isUnconfirmed(error))
                setUnconfirmedNotice(
                    `We couldn't confirm that "${bag.title}" was reactivated because the connection was interrupted. It may still have gone through - check its status in the list below before trying again.`
                );
            else
                notify(
                    getApiErrorMessage(error, "Unable to reactivate this Surprise Bag. Please try again."),
                    "error"
                );

        }
        finally {

            setReactivatingId(null);

            reloadBags();

        }

    }

    // No restaurant chosen: the restaurant picker is on the dashboard.
    if (restaurantId === null) {

        return (

            <Stack spacing={2} sx={{ alignItems: "flex-start" }}>

                <Typography variant="h4" component="h1">
                    Surprise Bags
                </Typography>

                <Alert severity="info">
                    Choose a restaurant on the Owner Dashboard to manage its Surprise Bags.
                </Alert>

                <Button variant="contained" onClick={() => navigate("/owner")}>
                    Go to Owner Dashboard
                </Button>

            </Stack>

        );

    }

    if (loading)
        return <CircularProgress aria-label="Loading Surprise Bags" />;

    // Only the owner's own restaurants (the API checks this too).
    const restaurant =
        restaurants.find(r => r.id === restaurantId) ?? null;

    const loaded = bags !== null && loadError === null;

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
                        Surprise Bags
                    </Typography>

                    {restaurant && (
                        <Typography color="text.secondary">
                            {restaurant.name}
                        </Typography>
                    )}

                </Box>

                {loaded && restaurant && (
                    <Stack direction="row" spacing={1.5} useFlexGap sx={{ flexWrap: "wrap" }}>

                        <Button
                            variant="outlined"
                            onClick={() => navigate(`/owner/surprise-bag-reservations?restaurantId=${restaurant.id}`)}
                        >
                            Reservations
                        </Button>

                        <Button
                            variant="contained"
                            onClick={() => setFormTarget({ bag: null })}
                        >
                            New Surprise Bag
                        </Button>

                    </Stack>
                )}

            </Stack>

            {unconfirmedNotice && (
                <Alert
                    severity="warning"
                    onClose={() => setUnconfirmedNotice(null)}
                    sx={{ mb: 3 }}
                >
                    {unconfirmedNotice}
                </Alert>
            )}

            {!loaded ? (

                <Stack spacing={2} sx={{ alignItems: "flex-start" }}>

                    <Alert severity="error">
                        {loadError ?? "We couldn't load your Surprise Bags right now. Please try again."}
                    </Alert>

                    <Button variant="contained" onClick={handleRetry}>
                        Try Again
                    </Button>

                </Stack>

            ) : !restaurant ? (

                <Stack spacing={2} sx={{ alignItems: "flex-start" }}>

                    <Alert severity="warning">
                        This restaurant isn't one of yours. Choose a restaurant on the Owner Dashboard.
                    </Alert>

                    <Button variant="contained" onClick={() => navigate("/owner")}>
                        Go to Owner Dashboard
                    </Button>

                </Stack>

            ) : bags.length === 0 ? (

                <Typography>
                    No Surprise Bags yet. Create one to offer surplus food at a lower price.
                </Typography>

            ) : (

                <Stack spacing={2}>

                    {bags.map(bag => (

                        <Card key={bag.id} component="article" aria-label={bag.title}>

                            <CardContent>

                                <Stack
                                    direction="row"
                                    sx={{ justifyContent: "space-between", alignItems: "flex-start", gap: 2, flexWrap: "wrap" }}
                                >

                                    <Box sx={{ minWidth: 0 }}>

                                        <Typography variant="h6" component="h2" sx={{ overflowWrap: "anywhere" }}>
                                            {bag.title}
                                        </Typography>

                                        <Typography color="text.secondary">
                                            {getSurpriseBagCategoryLabel(bag.category)} · {formatCurrency(bag.price, bag.currencyCode)}
                                            {bag.estimatedValue != null && ` (worth ${formatCurrency(bag.estimatedValue, bag.currencyCode)})`}
                                        </Typography>

                                    </Box>

                                    <Stack direction="row" spacing={1} useFlexGap sx={{ flexWrap: "wrap" }}>

                                        <Chip
                                            size="small"
                                            label={bag.isActive ? "Active" : "Inactive"}
                                            color={bag.isActive ? "success" : "default"}
                                        />

                                        {bag.isSoldOut && (
                                            <Chip size="small" label="Sold out" color="error" />
                                        )}

                                        {bag.isPickupLocked && (
                                            <Chip size="small" label="Pickup locked" variant="outlined" />
                                        )}

                                    </Stack>

                                </Stack>

                                <Typography sx={{ mt: 1.5 }}>
                                    Pick up: {formatDate(bag.pickupDate)}, {formatTime12Hour(bag.pickupStartTime)} – {formatTime12Hour(bag.pickupEndTime)}
                                </Typography>

                                <Typography>
                                    Total {bag.quantityTotal} · Reserved {bag.reservedQuantity} · Collected {bag.collectedQuantity} · Remaining {bag.remainingQuantity}
                                </Typography>

                                <Typography variant="body2" color="text.secondary">
                                    Up to {bag.maxPerCustomer} per customer
                                </Typography>

                                {bag.isPickupLocked && (
                                    <Typography variant="body2" color="text.secondary">
                                        Pickup date and times can't change: bags have been reserved or collected.
                                    </Typography>
                                )}

                                <Stack
                                    direction="row"
                                    spacing={1.5}
                                    useFlexGap
                                    sx={{ mt: 2, flexWrap: "wrap" }}
                                >

                                    <Button
                                        variant="outlined"
                                        onClick={() => handleEdit(bag)}
                                        disabled={openingEditId !== null}
                                        aria-label={`Edit ${bag.title}`}
                                    >
                                        {openingEditId === bag.id ? "Opening..." : "Edit"}
                                    </Button>

                                    <Button
                                        variant="outlined"
                                        onClick={() => navigate(`/owner/surprise-bag-reservations?restaurantId=${bag.restaurantId}&bagId=${bag.id}`)}
                                        aria-label={`Reservations for ${bag.title}`}
                                    >
                                        Reservations
                                    </Button>

                                    {bag.isActive ? (

                                        <Button
                                            variant="outlined"
                                            color="error"
                                            onClick={() => setDeactivateTarget(bag)}
                                            aria-label={`Deactivate ${bag.title}`}
                                        >
                                            Deactivate
                                        </Button>

                                    ) : (

                                        <Button
                                            variant="outlined"
                                            color="success"
                                            onClick={() => handleReactivate(bag)}
                                            disabled={reactivatingId !== null}
                                            aria-label={`Reactivate ${bag.title}`}
                                        >
                                            {reactivatingId === bag.id ? "Reactivating..." : "Reactivate"}
                                        </Button>

                                    )}

                                </Stack>

                            </CardContent>

                        </Card>

                    ))}

                </Stack>

            )}

            {/* Mounted only while open, so each opening starts fresh. */}
            {formTarget && restaurant && (
                <SurpriseBagFormDialog
                    open
                    onClose={handleFormClose}
                    onSaved={handleSaved}
                    restaurantId={restaurant.id}
                    bag={formTarget.bag}
                    currencyCode={formTarget.bag?.currencyCode ?? restaurant.currencyCode}
                />
            )}

            <Dialog
                open={deactivateTarget !== null}
                onClose={() => !deactivating && setDeactivateTarget(null)}
                aria-labelledby="deactivate-surprise-bag-title"
                aria-describedby="deactivate-surprise-bag-description"
            >

                <DialogTitle id="deactivate-surprise-bag-title">
                    Deactivate Surprise Bag
                </DialogTitle>

                <DialogContent>

                    <DialogContentText id="deactivate-surprise-bag-description">
                        Deactivate{deactivateTarget ? ` "${deactivateTarget.title}"` : ""}? Customers will no longer see it or be able to reserve it. Existing reservations are kept, and you can reactivate it later.
                    </DialogContentText>

                </DialogContent>

                <DialogActions>

                    <Button
                        onClick={() => setDeactivateTarget(null)}
                        disabled={deactivating}
                    >
                        Keep Active
                    </Button>

                    <Button
                        color="error"
                        variant="contained"
                        onClick={handleConfirmDeactivate}
                        disabled={deactivating}
                    >
                        {deactivating ? "Deactivating..." : "Deactivate"}
                    </Button>

                </DialogActions>

            </Dialog>

        </>

    );

}

export default OwnerSurpriseBagsPage;
