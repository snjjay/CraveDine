// InfoSection = one titled section of an informational page. The id makes
// it linkable (e.g. /partner#list-your-restaurant).

import type { ReactNode } from "react";

import { Box, Typography } from "@mui/material";

interface Props {
    id: string;
    title: string;
    children: ReactNode;
}

function InfoSection({ id, title, children }: Props) {

    return (

        <Box
            component="section"
            id={id}
            aria-labelledby={`${id}-heading`}
            sx={{ scrollMarginTop: { xs: 76, md: 92 } }}
        >

            <Typography variant="h5" component="h2" id={`${id}-heading`} sx={{ mb: 1.25 }}>
                {title}
            </Typography>

            <Box
                sx={{
                    typography: "body1",
                    color: "text.secondary",
                    lineHeight: 1.7,
                    "& p": { m: 0, mb: 1.5 },
                    "& p:last-child": { mb: 0 },
                    "& ul, & ol": { m: 0, mb: 1.5, pl: 3 },
                    "& li": { mb: 0.75 },
                    "& strong": { color: "text.primary", fontWeight: 600 },
                    // Inline text links only: buttons rendered as links
                    // (MUI ButtonBase) keep their own colours, otherwise a
                    // contained button gets coral text on a coral background.
                    "& a:not(.MuiButtonBase-root)": { color: "primary.main", fontWeight: 600 }
                }}
            >
                {children}
            </Box>

        </Box>

    );

}

export default InfoSection;
