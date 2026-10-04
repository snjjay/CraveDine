// OpeningStatus = the restaurant's current open/closed status for
// customers, e.g. "Open · Closes at 9:00 PM". The full weekly
// schedule is only shown to owners (Opening Hours management).

import { useEffect, useState } from "react";

import { Box, Typography } from "@mui/material";

import type { RestaurantOpeningHour } from "../../types/RestaurantOpeningHour";
import { getOpeningStatus } from "../../utils/openingHours";

interface Props {
    hours: RestaurantOpeningHour[];
    // "text": inline status line (default).
    // "pill": compact white pill with a status dot (e.g. on the hero photo).
    variant?: "text" | "pill";
}

function OpeningStatus({ hours, variant = "text" }: Props) {

    const [now, setNow] = useState(() => new Date());

    // Re-check every minute so the status flips at opening/closing time.
    useEffect(() => {

        const timer = setInterval(() => setNow(new Date()), 60_000);

        return () => clearInterval(timer);

    }, []);

    const status = getOpeningStatus(hours, now);

    if (variant === "pill") {

        return (

            <Box
                component="span"
                sx={{
                    display: "inline-flex",
                    alignItems: "center",
                    gap: 0.75,
                    px: 1.25,
                    py: 0.5,
                    borderRadius: "8px",
                    bgcolor: "background.paper",
                    color: "text.primary",
                    fontSize: "0.8125rem",
                    lineHeight: 1.4,
                    whiteSpace: "nowrap"
                }}
            >
                <Box
                    component="span"
                    aria-hidden
                    sx={{
                        width: 8,
                        height: 8,
                        borderRadius: "50%",
                        bgcolor: status.isOpen ? "success.main" : "error.main"
                    }}
                />
                <Box component="span" sx={{ fontWeight: 700, color: status.isOpen ? "success.main" : "error.main" }}>
                    {status.isOpen ? "Open" : "Closed"}
                </Box>
                <Box component="span" sx={{ color: "text.secondary" }}>
                    · {status.detail}
                </Box>
            </Box>

        );

    }

    return (

        <Typography>

            <Box
                component="span"
                sx={{
                    fontWeight: 600,
                    color: status.isOpen ? "success.main" : "error.main"
                }}
            >
                {status.isOpen ? "Open" : "Closed"}
            </Box>

            {" · "}{status.detail}

        </Typography>

    );

}

export default OpeningStatus;
