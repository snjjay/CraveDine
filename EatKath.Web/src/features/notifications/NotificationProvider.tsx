import { useCallback, useState } from "react";
import type { ReactNode, SyntheticEvent } from "react";

import { Alert, Snackbar } from "@mui/material";
import type { AlertColor } from "@mui/material";

import NotificationContext from "./NotificationContext";

interface Props {
    children: ReactNode;
}

function NotificationProvider({ children }: Props) {

    const [open, setOpen] = useState(false);

    const [message, setMessage] =
        useState("");

    const [severity, setSeverity] =
        useState<AlertColor>("info");

    const notify = useCallback(
        (message: string, severity: AlertColor = "info") => {

            setMessage(message);
            setSeverity(severity);
            setOpen(true);

        },
        []
    );

    function handleClose(
        _event?: SyntheticEvent | Event,
        reason?: string
    ) {

        if (reason === "clickaway")
            return;

        setOpen(false);

    }

    return (
        <NotificationContext.Provider value={{ notify }}>

            {children}

            <Snackbar
                open={open}
                autoHideDuration={5000}
                onClose={handleClose}
                anchorOrigin={{
                    vertical: "bottom",
                    horizontal: "center"
                }}
            >

                <Alert
                    onClose={handleClose}
                    severity={severity}
                    variant="filled"
                    sx={{ width: "100%" }}
                >
                    {message}
                </Alert>

            </Snackbar>

        </NotificationContext.Provider>
    );
}

export default NotificationProvider;
