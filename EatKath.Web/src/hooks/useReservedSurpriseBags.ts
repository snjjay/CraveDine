// ==========================================================
// useReservedSurpriseBags — which listed bags the customer holds
// ==========================================================
//
// Bags the logged-in customer already holds a Reserved reservation
// for, so their cards show "Already reserved" instead of Reserve.
// Loaded only for a customer, and only while `enabled` (e.g. the page
// has bags to show). The API still rejects a duplicate reservation by
// itself; if this fails, cards simply offer Reserve.
//
// reload(): call after a reservation attempt (a new reservation
// changes which bags are already reserved).
// ==========================================================

import { useCallback, useContext, useEffect, useState } from "react";

import AuthContext from "../features/auth/AuthContext";
import SurpriseBagReservationService from "../services/SurpriseBagReservationService";
import { SurpriseBagReservationStatus } from "../utils/surpriseBag";

export function useReservedSurpriseBags(enabled: boolean) {

    // Only customers can reserve.
    const auth = useContext(AuthContext);
    const customerId = auth?.user?.role === "Customer" ? auth.user.userId : null;

    const [reservedBags, setReservedBags] =
        useState<{ customerId: number; bagIds: Set<number> } | null>(null);

    // Incremented to reload them.
    const [reloadKey, setReloadKey] = useState(0);

    useEffect(() => {

        if (customerId === null || !enabled)
            return;

        let active = true;

        SurpriseBagReservationService.getMine()
            .then(reservations => {

                if (active)
                    setReservedBags({
                        customerId,
                        bagIds: new Set(reservations
                            .filter(r => r.status === SurpriseBagReservationStatus.Reserved)
                            .map(r => r.surpriseBagId))
                    });

            })
            .catch(error => console.error(error));

        return () => {
            active = false;
        };

    }, [customerId, enabled, reloadKey]);

    // Never another user's markers (e.g. after signing out and in again).
    function isAlreadyReserved(bagId: number): boolean {

        return reservedBags !== null &&
            reservedBags.customerId === customerId &&
            reservedBags.bagIds.has(bagId);

    }

    const reload = useCallback(() => setReloadKey(key => key + 1), []);

    return { isAlreadyReserved, reload };

}
