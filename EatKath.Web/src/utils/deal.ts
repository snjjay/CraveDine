// ==========================================================
// Customer-facing deal wording
// ==========================================================
//
// Built from the discount and offer type instead of the owner-entered
// title/description, which often repeat the discount and the
// restaurant name. The underlying deal data is not changed.
// ==========================================================

import type { Deal } from "../types/Deal";
import { getOfferTypeLabel } from "./offerType";

type DealOffer = Pick<Deal, "discountPercentage" | "offerType">;

// e.g. "25% Off - Dine In"
export function getDealHeadline(deal: DealOffer): string {

    return `${deal.discountPercentage}% Off - ${getOfferTypeLabel(deal.offerType)}`;

}

// e.g. "Enjoy 25% off your dine-in bill"
export function getDealSummary(deal: DealOffer): string {

    const offerKind =
        deal.offerType >= 1 && deal.offerType <= 3
            ? `${getOfferTypeLabel(deal.offerType).toLowerCase().replace(" ", "-")} `
            : "";

    return `Enjoy ${deal.discountPercentage}% off your ${offerKind}bill`;

}
