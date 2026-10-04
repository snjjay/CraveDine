// SectionHeader = a page section title with an optional subtitle and
// an optional action on the right (used on the restaurant details page).

import type { ReactNode } from "react";

import { Box, Stack, Typography } from "@mui/material";

interface Props {
    title: string;
    subtitle?: string;
    action?: ReactNode;
    // id for aria-labelledby on the surrounding <section>.
    id?: string;
}

function SectionHeader({ title, subtitle, action, id }: Props) {

    return (

        <Stack
            direction="row"
            sx={{ alignItems: "flex-end", justifyContent: "space-between", gap: 2, mb: 2 }}
        >

            <Box sx={{ minWidth: 0 }}>

                <Typography variant="h5" component="h2" id={id}>
                    {title}
                </Typography>

                {subtitle && (
                    <Typography variant="body2" color="text.secondary" sx={{ mt: 0.25 }}>
                        {subtitle}
                    </Typography>
                )}

            </Box>

            {action && (
                <Box sx={{ flexShrink: 0 }}>
                    {action}
                </Box>
            )}

        </Stack>

    );

}

export default SectionHeader;
