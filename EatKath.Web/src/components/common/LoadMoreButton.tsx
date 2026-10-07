// LoadMoreButton = the centred "View next 20 venues" button under a
// card list. Pair it with useLoadMore (hooks/useLoadMore.ts).

import { useRef, useState } from "react";

import { Box, Button } from "@mui/material";

import type { LoadMore } from "../../hooks/useLoadMore";

interface Props {
    loadMore: LoadMore<unknown>;
}

function LoadMoreButton({ loadMore }: Props) {

    const { nextCount, hasMore, total, visibleItems, showMore } = loadMore;

    const wrapperRef = useRef<HTMLDivElement>(null);

    // Screen-reader update after a click (the new cards appear above
    // the button, out of the reading position).
    const [announcement, setAnnouncement] = useState("");

    function handleClick() {

        const shownAfter = visibleItems.length + nextCount;

        setAnnouncement(`Showing ${shownAfter} of ${total} venues`);

        // The button disappears after the last batch: keep keyboard
        // focus here instead of losing it to the page.
        if (shownAfter >= total)
            wrapperRef.current?.focus();

        showMore();

    }

    return (

        // The wrapper always stays (it holds the screen-reader status and
        // takes focus after the last batch), but only takes up space
        // while the button is shown.
        <Box
            ref={wrapperRef}
            tabIndex={-1}
            sx={{ display: "flex", justifyContent: "center", mt: hasMore ? { xs: 4, md: 5 } : 0, outline: "none" }}
        >

            <Box
                role="status"
                aria-live="polite"
                sx={{ position: "absolute", width: "1px", height: "1px", overflow: "hidden", clip: "rect(0 0 0 0)", whiteSpace: "nowrap" }}
            >
                {announcement}
            </Box>

            {hasMore && (
                <Button
                    variant="contained"
                    color="secondary"
                    disableElevation
                    onClick={handleClick}
                    // Charcoal with white text by day; light with dark
                    // text at night (secondary palette).
                    sx={theme => ({
                        width: { xs: "100%", sm: "auto" },
                        minWidth: { sm: 260 },
                        minHeight: 48,
                        px: 4,
                        bgcolor: "secondary.main",
                        color: "secondary.contrastText",
                        fontWeight: 600,
                        fontSize: "0.9375rem",
                        "&:hover": { bgcolor: "#3A3A3A" },
                        ...theme.applyStyles("dark", {
                            "&:hover": { bgcolor: (theme.vars || theme).palette.secondary.dark }
                        })
                    })}
                >
                    View next {nextCount} {nextCount === 1 ? "venue" : "venues"}
                </Button>
            )}

        </Box>

    );

}

export default LoadMoreButton;
