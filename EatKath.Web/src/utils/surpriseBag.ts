// ==========================================================
// Surprise Bag category and reservation status
// ==========================================================
//
// The API sends both enums as their numeric values.
// Matches: EatKath.API.Enums.SurpriseBagCategory and
//          EatKath.API.Enums.SurpriseBagReservationStatus
// ==========================================================

export const SurpriseBagCategory = {
    Meals: 1,
    Bakery: 2,
    Groceries: 3,
    Mixed: 4
} as const;

export const SurpriseBagReservationStatus = {
    Reserved: 1,
    Collected: 2,
    Cancelled: 3,
    NoShow: 4
} as const;

type ChipColor = "success" | "warning" | "error" | "info" | "default";

export function getSurpriseBagCategoryLabel(category: number): string {

    switch (category) {

        case SurpriseBagCategory.Meals:
            return "Meals";

        case SurpriseBagCategory.Bakery:
            return "Bakery";

        case SurpriseBagCategory.Groceries:
            return "Groceries";

        case SurpriseBagCategory.Mixed:
            return "Mixed";

        default:
            return "Surprise Bag";

    }

}

export function getSurpriseBagReservationStatusLabel(status: number): string {

    switch (status) {

        case SurpriseBagReservationStatus.Reserved:
            return "Reserved";

        case SurpriseBagReservationStatus.Collected:
            return "Collected";

        case SurpriseBagReservationStatus.Cancelled:
            return "Cancelled";

        case SurpriseBagReservationStatus.NoShow:
            return "No-show";

        default:
            return "Unknown";

    }

}

export function getSurpriseBagReservationStatusColor(status: number): ChipColor {

    switch (status) {

        case SurpriseBagReservationStatus.Reserved:
            return "warning";

        case SurpriseBagReservationStatus.Collected:
            return "success";

        case SurpriseBagReservationStatus.NoShow:
            return "error";

        default:
            return "default";

    }

}

// Pickup dates and times are Kathmandu time (the API's business time),
// so reservation timestamps are shown in Kathmandu time too.
const BUSINESS_TIME_ZONE = "Asia/Kathmandu";

// Reservation timestamps (createdAt, collectedAt, cancelledAt) are UTC
// (set from DateTime.UtcNow, stored as datetime2). The API sends them in
// two forms, confirmed over HTTP: values read back from the database have
// no "Z" (e.g. GET .../my), which the browser would take as its own local
// time, while values set in the same request have one (e.g. the 201 from
// a new reservation, the cancel-mine response). A value without a time
// zone is therefore read as UTC; one with "Z" or an offset is used as
// sent. Fractional seconds beyond the 3 Date.parse expects are trimmed.
// "2026-10-13T05:12:34.1234567" -> "Tue, 13 Oct 2026, 10:57 am"
export function formatReservationTimestamp(value: string): string {

    const trimmed = value.replace(/(\.\d{3})\d+/, "$1");
    const hasTimeZone = /(Z|[+-]\d{2}:\d{2})$/i.test(trimmed);
    const date = new Date(hasTimeZone ? trimmed : `${trimmed}Z`);

    if (Number.isNaN(date.getTime()))
        return value;

    const options: Intl.DateTimeFormatOptions = {
        weekday: "short",
        day: "numeric",
        month: "short",
        year: "numeric",
        hour: "numeric",
        minute: "2-digit"
    };

    try {

        return date.toLocaleString(undefined, { ...options, timeZone: BUSINESS_TIME_ZONE });

    }
    catch {

        // A browser without the time zone: its own local time.
        return date.toLocaleString(undefined, options);

    }

}
