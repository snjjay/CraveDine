// PublicSurpriseBagList = open and upcoming Surprise Bags from all active
// restaurants (GET api/SurpriseBag?take=N), with the reservation dialog.
// Used by the home page's Featured Surprise Bags and the /surprise-bags
// page. Shows only real bags from the API: while there are none, the
// caller's empty state is shown instead.

import { useEffect, useState, type ReactNode } from "react";
import { useNavigate } from "react-router-dom";
import axios from "axios";

import { Alert, Box, Button, Card, Skeleton, Stack } from "@mui/material";

import SurpriseBagCard from "./SurpriseBagCard";
import ReserveSurpriseBagDialog from "./ReserveSurpriseBagDialog";
import { useReservedSurpriseBags } from "../../hooks/useReservedSurpriseBags";
import SurpriseBagService from "../../services/SurpriseBagService";
import type { SurpriseBag } from "../../types/SurpriseBag";

interface Props {
    // Most bags to show (the API allows 1-100).
    take: number;
    // CSS grid columns for the cards.
    gridTemplateColumns: string | Record<string, string>;
    // Shown when no bags are available right now.
    emptyState: ReactNode;
    // Number of loading placeholders.
    skeletonCount?: number;
}

function PublicSurpriseBagList({ take, gridTemplateColumns, emptyState, skeletonCount = 3 }: Props) {

    const navigate = useNavigate();

    const [bags, setBags] = useState<SurpriseBag[]>([]);

    // "ready" stays "ready" when a later refresh fails: the last bags
    // are kept rather than replaced by an error.
    const [status, setStatus] = useState<"loading" | "error" | "ready">("loading");

    // Incremented to reload the bags (Try again, or after a reservation).
    const [reloadKey, setReloadKey] = useState(0);

    // Bag the customer is reserving (the reservation dialog is open).
    const [reserveTarget, setReserveTarget] = useState<SurpriseBag | null>(null);

    // A bag the API no longer lists (e.g. its pickup window ended): its
    // open dialog shows it as no longer available.
    const [unlistedBagId, setUnlistedBagId] = useState<number | null>(null);

    const reservedBags = useReservedSurpriseBags(bags.length > 0);

    useEffect(() => {

        let active = true;

        SurpriseBagService.getAll(take)
            .then(data => {

                if (!active)
                    return;

                setBags(data);
                setStatus("ready");

            })
            .catch(error => {

                console.error(error);

                if (active)
                    setStatus(current => current === "ready" ? "ready" : "error");

            });

        return () => {
            active = false;
        };

    }, [take, reloadKey]);

    function retry() {

        setStatus("loading");
        setReloadKey(key => key + 1);

    }

    function openReservation(bag: SurpriseBag) {

        setReserveTarget(bag);
        setUnlistedBagId(null);

    }

    // After a reservation or an availability error: reload the list and
    // the "Already reserved" markers, and refresh the open dialog's bag by
    // id (it may have moved out of the first `take` bags, e.g. sold out).
    function handleAvailabilityChanged(bag: SurpriseBag) {

        setReloadKey(key => key + 1);
        reservedBags.reload();

        SurpriseBagService.getById(bag.id)
            .then(fresh => {

                setReserveTarget(target => target?.id === fresh.id ? fresh : target);
                setUnlistedBagId(id => id === fresh.id ? null : id);

            })
            .catch(error => {

                // 404: no longer publicly visible. Anything else (e.g. no
                // connection) keeps the last details.
                if (axios.isAxiosError(error) && error.response?.status === 404)
                    setUnlistedBagId(bag.id);
                else
                    console.error(error);

            });

    }

    if (status === "loading") {

        return (
            <Box aria-busy="true" aria-label="Loading Surprise Bags" sx={{ display: "grid", gap: 2, gridTemplateColumns }}>
                {Array.from({ length: skeletonCount }, (_, i) => (
                    <Card key={i} sx={{ display: "flex" }}>
                        <Skeleton variant="rectangular" sx={{ width: { xs: 92, sm: 112 }, height: "auto", minHeight: 180 }} />
                        <Stack spacing={0.5} sx={{ p: 2.5, flex: 1 }}>
                            <Skeleton width="70%" height={28} />
                            <Skeleton width="40%" />
                            <Skeleton width="60%" />
                            <Skeleton width="50%" />
                        </Stack>
                    </Card>
                ))}
            </Box>
        );

    }

    if (status === "error") {

        return (
            <Alert
                severity="error"
                action={<Button color="inherit" size="small" onClick={retry}>Try again</Button>}
            >
                We couldn't load Surprise Bags right now.
            </Alert>
        );

    }

    if (bags.length === 0)
        return <>{emptyState}</>;

    return (

        <>

            <Box sx={{ display: "grid", gap: 2, gridTemplateColumns }}>
                {bags.map(bag => (
                    <SurpriseBagCard
                        key={bag.id}
                        bag={bag}
                        onReserve={openReservation}
                        alreadyReserved={reservedBags.isAlreadyReserved(bag.id)}
                        showRestaurant
                    />
                ))}
            </Box>

            {/* Mounted only while open, so each opening starts fresh. */}
            {reserveTarget && (
                <ReserveSurpriseBagDialog
                    open
                    onClose={() => setReserveTarget(null)}
                    bag={reserveTarget}
                    isListed={reserveTarget.id !== unlistedBagId}
                    onAvailabilityChanged={() => handleAvailabilityChanged(reserveTarget)}
                    onViewReservations={() => navigate("/my-surprise-bags")}
                />
            )}

        </>

    );

}

export default PublicSurpriseBagList;
