// InfoPageLayout = the shared layout for CraveDine's informational pages
// (Our Story, How It Works, FAQs, Partner with Us, Contact, legal pages):
// page title + intro, readable content width, page title/description
// metadata, and scrolling to the top (or to a #section link).

import { useEffect, type ReactNode } from "react";
import { useLocation } from "react-router-dom";

import { Alert, Box, Stack, Typography } from "@mui/material";

import { usePageMeta } from "../../hooks/usePageMeta";

interface Props {
    // Small label above the title, e.g. "About CraveDine".
    eyebrow?: string;
    title: string;
    intro?: ReactNode;
    // Browser tab title and search description.
    metaTitle: string;
    metaDescription: string;
    // Legal drafts show a "draft for legal review" notice.
    draft?: boolean;
    children: ReactNode;
}

function InfoPageLayout({ eyebrow, title, intro, metaTitle, metaDescription, draft, children }: Props) {

    usePageMeta(metaTitle, metaDescription);

    const { pathname, hash } = useLocation();

    // Footer links can open a page part-way down (e.g. /faqs#restaurants);
    // otherwise start each page at the top.
    useEffect(() => {

        if (hash) {

            const target = document.getElementById(decodeURIComponent(hash.slice(1)));

            if (target) {
                target.scrollIntoView({ block: "start" });
                return;
            }

        }

        window.scrollTo({ top: 0 });

    }, [pathname, hash]);

    return (

        <Box sx={{ maxWidth: 860, mx: "auto", pb: { xs: 2, md: 4 } }}>

            <Box component="header" sx={{ mb: { xs: 3, md: 4 } }}>

                {eyebrow && (
                    <Typography
                        sx={{ color: "primary.main", fontWeight: 700, fontSize: "0.8125rem", letterSpacing: "0.08em", textTransform: "uppercase", mb: 1 }}
                    >
                        {eyebrow}
                    </Typography>
                )}

                <Typography variant="h3" component="h1" sx={{ fontSize: { xs: "2rem", md: "2.5rem" } }}>
                    {title}
                </Typography>

                {intro && (
                    <Typography color="text.secondary" sx={{ mt: 1.5, fontSize: { md: "1.0625rem" }, lineHeight: 1.7 }}>
                        {intro}
                    </Typography>
                )}

                {draft && (
                    <Alert severity="warning" variant="outlined" sx={{ mt: 3, borderRadius: "12px" }}>
                        <strong>Draft for legal review.</strong> This page is an initial draft based on how
                        CraveDine currently works. It is not legal advice. It must be reviewed by a qualified
                        legal adviser, and every highlighted "To be confirmed" detail completed, before it is
                        published.
                    </Alert>
                )}

            </Box>

            <Stack spacing={{ xs: 3.5, md: 4.5 }}>
                {children}
            </Stack>

        </Box>

    );

}

export default InfoPageLayout;
