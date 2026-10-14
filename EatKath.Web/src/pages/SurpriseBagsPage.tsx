import { Box, Typography } from "@mui/material";

import NoSurpriseBagsCard from "../components/surpriseBags/NoSurpriseBagsCard";
import PublicSurpriseBagList from "../components/surpriseBags/PublicSurpriseBagList";

// Bags shown on this page (the API's maximum).
const MAX_BAGS = 100;

function SurpriseBagsPage() {

    return (

        <>

            <Box sx={{ mb: 3 }}>
                <Typography variant="h4" component="h1">
                    Surprise Bags
                </Typography>
                <Typography color="text.secondary" sx={{ mt: 0.5, maxWidth: 720 }}>
                    Good food, less waste. Reserve a bag of a restaurant's surplus food at a lower price,
                    then pay at the restaurant and show your confirmation code when you collect it during
                    the pickup time.
                </Typography>
            </Box>

            <PublicSurpriseBagList
                take={MAX_BAGS}
                gridTemplateColumns="repeat(auto-fill, minmax(min(100%, 380px), 1fr))"
                emptyState={<NoSurpriseBagsCard />}
                skeletonCount={6}
            />

        </>

    );

}

export default SurpriseBagsPage;
