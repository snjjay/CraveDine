// InfoRow = an icon followed by a label and its content
// (address, phone, website ... in the restaurant information card).

import type { ReactNode } from "react";

import { Box, Stack, Typography } from "@mui/material";

interface Props {
    icon: ReactNode;
    // Short label, e.g. "Address" (shown small above the content).
    label: string;
    children: ReactNode;
}

function InfoRow({ icon, label, children }: Props) {

    return (

        <Stack direction="row" spacing={1.5} sx={{ alignItems: "flex-start" }}>

            <Box
                aria-hidden
                sx={{
                    width: 36,
                    height: 36,
                    flexShrink: 0,
                    borderRadius: "10px",
                    bgcolor: "primarySoft",
                    color: "primary.main",
                    display: "grid",
                    placeItems: "center",
                    "& svg": { fontSize: 20 }
                }}
            >
                {icon}
            </Box>

            <Box sx={{ minWidth: 0, flex: 1 }}>

                <Typography variant="caption" color="text.secondary" component="div">
                    {label}
                </Typography>

                <Box sx={{ typography: "body2", overflowWrap: "anywhere" }}>
                    {children}
                </Box>

            </Box>

        </Stack>

    );

}

export default InfoRow;
