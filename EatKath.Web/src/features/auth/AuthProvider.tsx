import { useCallback, useEffect, useRef, useState } from "react";
import type { ReactNode } from "react";

import AuthContext from "./AuthContext";
import type { AuthResponse } from "./types";
import { onSessionExpired, SESSION_EXPIRED_MESSAGE } from "./sessionEvents";
import { useNotification } from "../notifications/NotificationContext";

interface Props {
    children: ReactNode;
}

const STORAGE_KEY = "user";

// Longest delay setTimeout supports (about 24.8 days).
const MAX_TIMER_DELAY = 2_147_483_647;

// Milliseconds left until the session's expiresAt (NaN if unknown).
// The API sends 7 fractional digits; trimmed to the 3 Date.parse expects.
function msUntilExpiry(user: AuthResponse): number {

    return Date.parse(String(user.expiresAt).replace(/(\.\d{3})\d+/, "$1")) - Date.now();

}

// The saved session. One already past its expiresAt is not restored
// (the API would refuse its token): expired = true. No side effects here
// (React may call state initialisers twice); the effect below removes it.
function readStoredSession(): { user: AuthResponse | null; expired: boolean } {

    const storedUser = localStorage.getItem(STORAGE_KEY);

    if (!storedUser)
        return { user: null, expired: false };

    const user: AuthResponse = JSON.parse(storedUser);

    if (msUntilExpiry(user) <= 0)
        return { user: null, expired: true };

    return { user, expired: false };

}

function AuthProvider({ children }: Props) {

    const { notify } = useNotification();

    const [initialSession] = useState(readStoredSession);

    const [user, setUser] = useState<AuthResponse | null>(initialSession.user);

    // Read by SessionExpiredRedirect (inside the router) to go to /login.
    const [sessionExpired, setSessionExpired] = useState(initialSession.expired);

    const startupExpiryHandled = useRef(false);

    // Shown after the current task, so it replaces (rather than is
    // replaced by) any error message a caller shows for the same failed
    // request.
    const showExpiredMessage = useCallback(() => {

        window.setTimeout(() => notify(SESSION_EXPIRED_MESSAGE, "warning"), 0);

    }, [notify]);

    // Signs out because the session is no longer valid (automatic logout
    // at expiresAt, or an API 401 - see api/axios.ts). Once the saved
    // session is gone, further calls (several 401s at once, timer + 401)
    // do nothing, so the message and redirect happen once.
    const expireSession = useCallback(() => {

        if (localStorage.getItem(STORAGE_KEY) === null)
            return;

        localStorage.removeItem(STORAGE_KEY);

        setUser(null);

        setSessionExpired(true);

        showExpiredMessage();

    }, [showExpiredMessage]);

    // A saved session that had already expired when the app started.
    useEffect(() => {

        if (!initialSession.expired || startupExpiryHandled.current)
            return;

        startupExpiryHandled.current = true;

        localStorage.removeItem(STORAGE_KEY);

        showExpiredMessage();

    }, [initialSession.expired, showExpiredMessage]);

    // Session expiry reported by the Axios layer.
    useEffect(() => onSessionExpired(expireSession), [expireSession]);

    // Automatic logout when the signed-in session reaches expiresAt. One
    // timer per signed-in user; cleared on login/logout/unmount.
    useEffect(() => {

        if (!user)
            return;

        const ms = msUntilExpiry(user);

        // Unknown expiry (or beyond what a timer supports): the API's 401
        // still signs the user out.
        if (Number.isNaN(ms) || ms > MAX_TIMER_DELAY)
            return;

        const timer = window.setTimeout(expireSession, Math.max(0, ms));

        return () => window.clearTimeout(timer);

    }, [user, expireSession]);

    function login(authUser: AuthResponse) {

        localStorage.setItem("user", JSON.stringify(authUser));

        setUser(authUser);

        setSessionExpired(false);
    }

    function logout() {

        localStorage.removeItem("user");

        setUser(null);
    }

    const acknowledgeSessionExpired = useCallback(() => setSessionExpired(false), []);

    return (
        <AuthContext.Provider
            value={{
                user,
                login,
                logout,
                sessionExpired,
                acknowledgeSessionExpired
            }}
        >
            {children}
        </AuthContext.Provider>
    );
}

export default AuthProvider;


// ==========================================================
// STEP 16 — AuthProvider.tsx
// ==========================================================
//
// AuthProvider = MANAGES the authentication information.
//
// Simple analogy:
//
// AuthContext     = 📋 Shared notice board
// AuthProvider    = 👤 Person managing the notice board
//
// ----------------------------------------------------------
//
// user
// → 📦 Stores the current logged-in user.
//
// user can be:
//
// 👤 User information
// OR
// null = nobody is logged in.
//
// ----------------------------------------------------------
//
// localStorage
// → 🗄️ Saves the user in the browser.
//
// When the application starts:
//
// Check localStorage
//      ↓
// User found?
//      ↓
// YES → restore user
// NO  → user = null
//
// This allows the login information to survive
// a browser refresh.
//
// ----------------------------------------------------------
//
// login(authUser)
//
// → 🔐 Logs the user in.
//
// Saves user to localStorage
//      ↓
// setUser(authUser)
//      ↓
// React knows the user is logged in.
//
// Two things are updated:
//
// localStorage → survives refresh
// React state  → updates the application immediately
//
// ----------------------------------------------------------
//
// logout()
//
// → 🚪 Logs the user out.
//
// Remove user from localStorage
//      ↓
// setUser(null)
//      ↓
// Application knows user is logged out.
//
// ----------------------------------------------------------
//
// AuthContext.Provider
//
// value={{
//     user,
//     login,
//     logout
// }}
//
// → Makes these available to everything inside
//   <AuthProvider>.
//
// In main.tsx:
//
// <AuthProvider>
//     <App />
// </AuthProvider>
//
// Therefore App and its components can access:
//
// user
// login()
// logout()
//
// ----------------------------------------------------------
//
// COMPLETE FLOW:
//
// main.tsx
//      ↓
// AuthProvider
//      ↓
// Manages user + login + logout
//      ↓
// AuthContext.Provider
//      ↓
// App
//      ↓
// MainLayout / ProtectedRoute / other components
//      ↓
// Can use authentication information
//
// 🔑 Remember:
//
// AuthContext  = defines/shared auth information
// AuthProvider = manages/provides the actual auth information
//
// ==========================================================Next is ProtectedRoute.tsx — Step 17.