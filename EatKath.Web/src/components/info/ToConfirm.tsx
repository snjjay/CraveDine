// ToConfirm = a highlighted marker for a business or legal detail that must
// be supplied before a page is published, e.g.
// <ToConfirm>registered company name</ToConfirm>

import type { ReactNode } from "react";

import { Box } from "@mui/material";

function ToConfirm({ children }: { children: ReactNode }) {

    return (

        <Box
            component="mark"
            sx={theme => ({
                bgcolor: "rgba(237, 108, 2, 0.12)",
                color: "warning.dark",
                fontWeight: 600,
                px: 0.5,
                borderRadius: "4px",
                // Night: the lighter warning colour reads on the tint.
                ...theme.applyStyles("dark", { color: (theme.vars || theme).palette.warning.main })
            })}
        >
            [To be confirmed: {children}]
        </Box>

    );

}

export default ToConfirm;
