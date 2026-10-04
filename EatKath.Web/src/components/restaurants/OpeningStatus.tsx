// OpeningStatus = the restaurant's current open/closed status for
// customers, e.g. "Open · Closes at 9:00 PM". The full weekly
// schedule is only shown to owners (Opening Hours management).

import { useEffect, useState } from "react";

import { Box, Typography } from "@mui/material";

import type { RestaurantOpeningHour } from "../../types/RestaurantOpeningHour";
import { getOpeningStatus } from "../../utils/openingHours";

interface Props {
    hours: RestaurantOpeningHour[];
}

function OpeningStatus({ hours }: Props) {

    const [now, setNow] = useState(() => new Date());

    // Re-check every minute so the status flips at opening/closing time.
    useEffect(() => {

        const timer = setInterval(() => setNow(new Date()), 60_000);

        return () => clearInterval(timer);

    }, []);

    const status = getOpeningStatus(hours, now);

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
