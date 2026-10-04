// ==========================================================
// Customer-facing opening status
// ==========================================================
//
// Works out "Open · Closes at 9:00 PM" / "Closed · Opens ... at ..."
// from a restaurant's configured weekly opening hours.
//
// - dayOfWeek uses .NET DayOfWeek numbering (Sunday = 0), which is
//   the same as JavaScript's Date.getDay().
// - "Now" is the browser's local time, the same convention the rest
//   of the frontend uses (restaurants have no time zone field).
// - Opening hours never cross midnight: the API requires
//   CloseTime > OpenTime. A row that breaks this (legacy data) is
//   ignored rather than risk showing Open at the wrong time.
// ==========================================================

import type { RestaurantOpeningHour } from "../types/RestaurantOpeningHour";
import { formatTime12Hour } from "./time";

const DAY_NAMES = [
    "Sunday",
    "Monday",
    "Tuesday",
    "Wednesday",
    "Thursday",
    "Friday",
    "Saturday"
];

export interface OpeningStatus {
    isOpen: boolean;
    // e.g. "Closes at 9:00 PM", "Opens Monday at 10:00 AM"
    detail: string;
}

function toMinutes(time: string): number {

    const [hours, minutes] = time.split(":").map(Number);

    return hours * 60 + minutes;

}

// Day with a usable opening window.
function isOpeningDay(
    hour: RestaurantOpeningHour | undefined
): hour is RestaurantOpeningHour {

    return hour !== undefined &&
        !hour.isClosed &&
        toMinutes(hour.closeTime) > toMinutes(hour.openTime);

}

export function getOpeningStatus(
    hours: RestaurantOpeningHour[],
    now: Date = new Date()
): OpeningStatus {

    const today = now.getDay();
    const minutesNow = now.getHours() * 60 + now.getMinutes();

    const hoursFor = (day: number) =>
        hours.find(h => h.dayOfWeek === day);

    const todayHours = hoursFor(today);

    if (isOpeningDay(todayHours)) {

        const open = toMinutes(todayHours.openTime);
        const close = toMinutes(todayHours.closeTime);

        // Open from opening time up to (not including) closing time.
        if (minutesNow >= open && minutesNow < close) {
            return {
                isOpen: true,
                detail: `Closes at ${formatTime12Hour(todayHours.closeTime)}`
            };
        }

        if (minutesNow < open) {
            return {
                isOpen: false,
                detail: `Opens today at ${formatTime12Hour(todayHours.openTime)}`
            };
        }

    }

    // Today is closed or already over - find the next opening day,
    // up to the same weekday next week.
    for (let offset = 1; offset <= 7; offset++) {

        const day = (today + offset) % 7;
        const dayHours = hoursFor(day);

        if (!isOpeningDay(dayHours))
            continue;

        const when =
            offset === 1 ? "tomorrow" :
            offset === 7 ? `next ${DAY_NAMES[day]}` :
            DAY_NAMES[day];

        return {
            isOpen: false,
            detail: `Opens ${when} at ${formatTime12Hour(dayHours.openTime)}`
        };

    }

    return {
        isOpen: false,
        detail: "No upcoming opening time"
    };

}
