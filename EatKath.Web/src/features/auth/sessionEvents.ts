// ==========================================================
// Session expiry signal
// ==========================================================
//
// Lets code outside React (the Axios layer) tell the auth state that
// the signed-in session is no longer valid. AuthProvider listens, signs
// the user out, shows SESSION_EXPIRED_MESSAGE once and sends them to
// /login (SessionExpiredRedirect).
// ==========================================================

export const SESSION_EXPIRED_MESSAGE = "Your session has expired, please sign in again";

const EVENT_NAME = "session-expired";

const sessionEvents = new EventTarget();

export function signalSessionExpired(): void {

    sessionEvents.dispatchEvent(new Event(EVENT_NAME));

}

// Returns a function that removes the listener.
export function onSessionExpired(listener: () => void): () => void {

    sessionEvents.addEventListener(EVENT_NAME, listener);

    return () => sessionEvents.removeEventListener(EVENT_NAME, listener);

}

// Errors from requests rejected because the session expired are marked,
// so callers can skip their own error message (the global one is shown).
const SESSION_EXPIRED_FLAG = "__sessionExpired";

export function markSessionExpiredError(error: unknown): void {

    if (error && typeof error === "object")
        (error as Record<string, unknown>)[SESSION_EXPIRED_FLAG] = true;

}

export function isSessionExpiredError(error: unknown): boolean {

    return !!error && typeof error === "object" &&
        (error as Record<string, unknown>)[SESSION_EXPIRED_FLAG] === true;

}
