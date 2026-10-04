// ==========================================================
// Restaurant card offer overlays
// ==========================================================
//
// Picks up to two offers to summarise on a restaurant card image,
// from the deal summaries in the restaurant list. Display only - it
// never changes deal availability, limits or redemption.
//
// - Only Dine In and Takeaway (cards don't offer Delivery).
// - Sold-out offers (total cap used up) are left out.
// - Timing reuses getDealTimingToday (same rules as the menu):
//   running now > later today > a later date; then highest discount.
// - When both exist, one Dine In and one Takeaway offer are shown.
// - Remaining relevant offers are counted for "+N more".
// ==========================================================

import type { RestaurantDealSummary } from "../types/Restaurant";
import { getDealTimingToday, MenuTab } from "./menuPricing";
import { getOfferTypeLabel } from "./offerType";
import { formatTime12Hour, todayIsoDate } from "./time";

export interface CardOffer {
    id: number;
    // e.g. "30% Off · Dine In"
    headline: string;
    // e.g. "Arrive 5:00 PM – 6:00 PM", "Tomorrow · 5:00 PM – 6:00 PM"
    detail: string;
    // Inside today's arrival window right now.
    live: boolean;
}

export interface CardOffers {
    offers: CardOffer[];
    moreCount: number;
}

const MAX_CARD_OFFERS = 2;

interface Candidate {
    summary: RestaurantDealSummary;
    // 0 = running now, 1 = later today, 2 = a later date
    rank: number;
    detail: string;
}

function addDays(isoDate: string, days: number): string {

    const [year, month, day] = isoDate.split("-").map(Number);
    const date = new Date(year, month - 1, day + days);

    return todayIsoDate(date);

}

// "2026-11-20" -> "Nov 20" (same style as the deal card dates)
function formatShortDate(isoDate: string): string {

    const [year, month, day] = isoDate.split("-").map(Number);

    return new Date(year, month - 1, day).toLocaleDateString("en-US", { month: "short", day: "numeric" });

}

function toCandidate(summary: RestaurantDealSummary, now: Date): Candidate | null {

    const window = `${formatTime12Hour(summary.startTime)} – ${formatTime12Hour(summary.endTime)}`;

    const timing = getDealTimingToday({ ...summary, isActive: true }, now);

    if (timing === "running")
        return { summary, rank: 0, detail: `Arrive ${window}` };

    if (timing === "upcoming")
        return { summary, rank: 1, detail: `Arrive ${window}` };

    // Not usable today: show the next date it can be used, if any.
    const today = todayIsoDate(now);

    if (summary.availabilityDate > today)
        return { summary, rank: 2, detail: `From ${formatShortDate(summary.availabilityDate)} · ${window}` };

    const tomorrow = addDays(today, 1);

    if (summary.endDate >= tomorrow)
        return { summary, rank: 2, detail: `Tomorrow · ${window}` };

    // Ends today and today's window is over (or full).
    return null;

}

function byRelevance(a: Candidate, b: Candidate): number {

    if (a.rank !== b.rank)
        return a.rank - b.rank;

    return b.summary.discountPercentage - a.summary.discountPercentage;

}

export function selectCardOffers(
    summaries: RestaurantDealSummary[] | undefined,
    now: Date = new Date()
): CardOffers {

    const candidates = (summaries ?? [])
        .filter(s =>
            (s.offerType === MenuTab.DineIn || s.offerType === MenuTab.Takeaway) &&
            !s.isSoldOut)
        .map(s => toCandidate(s, now))
        .filter((c): c is Candidate => c !== null)
        .sort(byRelevance);

    // One of each dining type first, then fill by relevance.
    const chosen: Candidate[] = [];

    for (const type of [MenuTab.DineIn, MenuTab.Takeaway]) {

        const best = candidates.find(c => c.summary.offerType === type);

        if (best)
            chosen.push(best);

    }

    for (const candidate of candidates) {

        if (chosen.length >= MAX_CARD_OFFERS)
            break;

        if (!chosen.includes(candidate))
            chosen.push(candidate);

    }

    const shown = chosen.sort(byRelevance).slice(0, MAX_CARD_OFFERS);

    return {
        offers: shown.map(c => ({
            id: c.summary.id,
            headline: `${c.summary.discountPercentage}% Off · ${getOfferTypeLabel(c.summary.offerType)}`,
            detail: c.detail,
            live: c.rank === 0
        })),
        moreCount: candidates.length - shown.length
    };

}
