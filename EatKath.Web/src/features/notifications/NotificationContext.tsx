import { createContext, useContext } from "react";
import type { AlertColor } from "@mui/material";

// ==========================================================
// NotificationContext
// ==========================================================
//
// Shared place for showing in-app notifications, following
// the same shape as AuthContext/AuthProvider.
//
// notify(message, severity)
// → Shows a Snackbar/Alert with the given message.
//
// severity defaults to "info" when omitted.
//
// ==========================================================

export interface NotificationContextType {
    notify: (message: string, severity?: AlertColor) => void;
}

const NotificationContext =
    createContext<NotificationContextType | undefined>(undefined);

export default NotificationContext;

// ----------------------------------------------------------
// useNotification()
//
// Convenience hook so pages/components don't need to import
// useContext + NotificationContext + do the undefined-check
// themselves every time.
// ----------------------------------------------------------
export function useNotification(): NotificationContextType {

    const context = useContext(NotificationContext);

    if (!context) {
        throw new Error(
            "useNotification must be used within a NotificationProvider."
        );
    }

    return context;
}
