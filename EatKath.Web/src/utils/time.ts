// ==========================================================
// Date / time helpers for walk-in offers
// ==========================================================
//
// The API sends DateOnly as "YYYY-MM-DD" and TimeOnly as
// "HH:mm:ss". All helpers work in the browser's local time.
// ==========================================================

// Minutes between selectable arrival times.
export const ARRIVAL_SLOT_MINUTES = 15;

function toMinutes(time: string): number {

    const [hours, minutes] = time.split(":").map(Number);

    return hours * 60 + minutes;

}

function fromMinutes(totalMinutes: number): string {

    const hours = Math.floor(totalMinutes / 60);
    const minutes = totalMinutes % 60;

    return `${String(hours).padStart(2, "0")}:${String(minutes).padStart(2, "0")}:00`;

}

// "14:30:00" -> "2:30pm"
export function formatTime(time: string): string {

    const total = toMinutes(time);
    const hours = Math.floor(total / 60);
    const minutes = total % 60;
    const suffix = hours >= 12 ? "pm" : "am";
    const displayHours = hours % 12 === 0 ? 12 : hours % 12;

    return `${displayHours}:${String(minutes).padStart(2, "0")}${suffix}`;

}

// "18:00:00" -> "6:00 PM"
export function formatTime12Hour(time: string): string {

    return formatTime(time).replace(/(am|pm)$/, suffix => ` ${suffix.toUpperCase()}`);

}

// "2026-11-10" -> "Tue, 10 Nov 2026"
export function formatDate(date: string): string {

    const [year, month, day] = date.split("-").map(Number);

    return new Date(year, month - 1, day).toLocaleDateString(undefined, {
        weekday: "short",
        day: "numeric",
        month: "short",
        year: "numeric"
    });

}

// Today's local date as "YYYY-MM-DD" (or the date of `now`, if given).
export function todayIsoDate(now: Date = new Date()): string {

    return [
        now.getFullYear(),
        String(now.getMonth() + 1).padStart(2, "0"),
        String(now.getDate()).padStart(2, "0")
    ].join("-");

}

// Arrival times ("HH:mm:ss") from start to end inclusive.
// When the date is today, times that have already passed are left out.
export function buildArrivalSlots(
    startTime: string,
    endTime: string,
    date: string
): string[] {

    const start = toMinutes(startTime);
    const end = toMinutes(endTime);

    let earliest = start;

    if (date === todayIsoDate()) {

        const now = new Date();

        earliest = Math.max(start, now.getHours() * 60 + now.getMinutes());

    }

    const slots: string[] = [];

    for (let minutes = start; minutes <= end; minutes += ARRIVAL_SLOT_MINUTES) {

        if (minutes >= earliest)
            slots.push(fromMinutes(minutes));

    }

    return slots;

}
