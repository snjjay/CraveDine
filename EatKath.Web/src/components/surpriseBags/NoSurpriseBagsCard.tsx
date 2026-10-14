// NoSurpriseBagsCard = the empty state while no Surprise Bags are
// available (the /surprise-bags page and the home page's featured bags).

import { Link as RouterLink } from "react-router-dom";

import { Button, Card, CardContent, Typography } from "@mui/material";

import ShoppingBagOutlinedIcon from "@mui/icons-material/ShoppingBagOutlined";

interface Props {
    // Less padding, and fills its grid cell (home page).
    compact?: boolean;
}

function NoSurpriseBagsCard({ compact = false }: Props) {

    return (

        <Card sx={{ height: compact ? "100%" : undefined }}>
            <CardContent sx={{ textAlign: "center", py: compact ? 4 : 6, px: 3 }}>
                <ShoppingBagOutlinedIcon aria-hidden sx={{ fontSize: 40, color: "success.main" }} />
                <Typography variant="h6" component="p" sx={{ mt: 1 }}>
                    No Surprise Bags available right now
                </Typography>
                <Typography color="text.secondary" sx={{ mt: 0.5, maxWidth: 460, mx: "auto" }}>
                    Restaurants add bags of surplus food for pickup later in the day. Check back soon,
                    or browse today's restaurant deals in the meantime.
                </Typography>
                <Button
                    component={RouterLink}
                    to="/restaurants?offer=deals"
                    variant="outlined"
                    sx={{ mt: 2 }}
                >
                    Browse restaurant deals
                </Button>
            </CardContent>
        </Card>

    );

}

export default NoSurpriseBagsCard;
