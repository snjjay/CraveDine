// ==========================================================
// Digital menu pricing (display preview only)
// ==========================================================
//
// Picks the one deal that applies to a menu tab today and works out
// the discounted price to show next to each item.
//
// - The real discount is still calculated by the API when the owner
//   records the bill (RedemptionService.CompleteRedemptionAsync);
//   nothing here changes redemption or billing.
// - Discounts never stack: exactly one deal (or none) is chosen.
// - "Now" is the browser's local time, the same convention as the
//   rest of the frontend (restaurants have no time zone field).
// - Deals are expected from GET api/Deal/restaurant/{id}, which only
//   returns active deals and includes availability for today.
// ==========================================================

import type { Deal } from "../types/Deal";
import { todayIsoDate } from "./time";

// Menu tabs use the API's OfferType values (Delivery has no tab).
export const MenuTab = {
    DineIn: 1,
    Takeaway: 2
} as const;

export type MenuTabValue = typeof MenuTab[keyof typeof MenuTab];

// "running"  = inside today's arrival window now
// "upcoming" = valid today, window starts later today
export type DealTiming = "running" | "upcoming";

export interface ApplicableDeal {
    deal: Deal;
    timing: DealTiming;
}

function toMinutes(time: string): number {

    const [hours, minutes] = time.split(":").map(Number);

    return hours * 60 + minutes;

}

// How a deal stands today, or null when it cannot be used today
// (inactive, outside its dates, sold out, or today's window is over).
export function getDealTimingToday(
    deal: Deal,
    now: Date = new Date()
): DealTiming | null {

    if (!deal.isActive)
        return null;

    const today = todayIsoDate(now);

    if (today < deal.startDate || today > deal.endDate)
        return null;

    // Total offer cap used up.
    if (deal.isSoldOut)
        return null;

    // Daily limit used up for today.
    if (deal.availabilityDate === today && deal.remainingOffers === 0)
        return null;

    const minutesNow = now.getHours() * 60 + now.getMinutes();

    // The API accepts arrival times up to and including the end time.
    if (minutesNow > toMinutes(deal.endTime))
        return null;

    return minutesNow >= toMinutes(deal.startTime) ? "running" : "upcoming";

}

// The single deal to show for a tab: a running deal first, otherwise
// the next one starting today (earliest start); ties go to the
// highest discount. Dine-in deals never apply to Takeaway or vice versa.
export function selectApplicableDeal(
    deals: Deal[],
    tab: MenuTabValue,
    now: Date = new Date()
): ApplicableDeal | null {

    const candidates: ApplicableDeal[] = [];

    for (const deal of deals) {

        if (deal.offerType !== tab)
            continue;

        const timing = getDealTimingToday(deal, now);

        if (timing)
            candidates.push({ deal, timing });

    }

    candidates.sort((a, b) => {

        if (a.timing !== b.timing)
            return a.timing === "running" ? -1 : 1;

        if (a.timing === "upcoming") {

            const startDifference =
                toMinutes(a.deal.startTime) - toMinutes(b.deal.startTime);

            if (startDifference !== 0)
                return startDifference;

        }

        if (a.deal.discountPercentage !== b.deal.discountPercentage)
            return b.deal.discountPercentage - a.deal.discountPercentage;

        return a.deal.id - b.deal.id;

    });

    return candidates[0] ?? null;

}

// Price after the discount, rounded to 2 decimals the same way as the
// API's bill calculation: discount = round(price × % / 100, 2) using
// round-half-to-even (.NET Math.Round default), price - discount.
export function calculateDiscountedPrice(
    price: number,
    discountPercentage: number
): number {

    // Discount in cents; trim floating-point noise before rounding.
    const rawCents = Math.round(price * discountPercentage * 1e6) / 1e6;

    const floorCents = Math.floor(rawCents);
    const fraction = rawCents - floorCents;

    const discountCents =
        fraction > 0.5 ? floorCents + 1 :
        fraction < 0.5 ? floorCents :
        floorCents % 2 === 0 ? floorCents : floorCents + 1;

    return (Math.round(price * 100) - discountCents) / 100;

}
