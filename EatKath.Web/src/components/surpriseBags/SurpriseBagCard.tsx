import { useContext } from "react";
import { Link as RouterLink, useNavigate } from "react-router-dom";

import {
    Box,
    Button,
    Card,
    Link,
    Stack,
    Typography
} from "@mui/material";

import CheckCircleOutlineIcon from "@mui/icons-material/CheckCircleOutline";
import EventOutlinedIcon from "@mui/icons-material/EventOutlined";
import ScheduleIcon from "@mui/icons-material/Schedule";
import ShoppingBagOutlinedIcon from "@mui/icons-material/ShoppingBagOutlined";
import StorefrontOutlinedIcon from "@mui/icons-material/StorefrontOutlined";

import AuthContext from "../../features/auth/AuthContext";
import { useNotification } from "../../features/notifications/NotificationContext";
import type { SurpriseBag } from "../../types/SurpriseBag";
import { formatCurrency } from "../../utils/currency";
import { getSurpriseBagCategoryLabel } from "../../utils/surpriseBag";
import { formatDate, formatTime12Hour } from "../../utils/time";

interface Props {
    bag: SurpriseBag;
    // Called when a logged-in customer chooses to reserve this bag.
    onReserve: (bag: SurpriseBag) => void;
    // The logged-in customer already holds a Reserved reservation for this
    // bag (the API would reject another), so Reserve isn't offered.
    alreadyReserved?: boolean;
    // Lists with bags from many restaurants (home page, /surprise-bags)
    // name the restaurant and link to it.
    showRestaurant?: boolean;
}

function SurpriseBagCard({ bag, onReserve, alreadyReserved = false, showRestaurant = false }: Props) {

    const navigate = useNavigate();
    const { notify } = useNotification();

    const auth = useContext(AuthContext);
    const user = auth?.user ?? null;

    const categoryLabel = getSurpriseBagCategoryLabel(bag.category);

    // The API only lists bags whose pickup window hasn't ended, so the
    // card shows the pickup date as sent (Kathmandu time) and never
    // works out "today" or expiry from the browser's clock.
    const remaining = bag.remainingQuantity;

    const availabilityLabel = bag.isSoldOut
        ? "Sold out"
        : `${remaining} ${remaining === 1 ? "bag" : "bags"} left`;

    const availabilityColor = bag.isSoldOut
        ? "error.main"
        : remaining <= 3 ? "warning.main" : "success.main";

    // Only logged-in customers can reserve. Visitors are sent to log in.
    function handleReserveClick() {

        if (!user) {
            navigate("/login");
            return;
        }

        if (user.role !== "Customer") {
            notify("Please log in with a customer account to reserve Surprise Bags.", "info");
            return;
        }

        onReserve(bag);

    }

    return (

        // Bag panel | details (same layout as DealCard)
        <Card
            component="article"
            aria-label={bag.title}
            sx={{ height: "100%", display: "flex", alignItems: "stretch" }}
        >

            <Box
                sx={{
                    width: { xs: 92, sm: 112 },
                    flexShrink: 0,
                    bgcolor: "primarySoft",
                    color: "primary.main",
                    display: "flex",
                    flexDirection: "column",
                    alignItems: "center",
                    justifyContent: "center",
                    gap: 0.5,
                    textAlign: "center",
                    px: 1,
                    py: 2
                }}
            >
                <ShoppingBagOutlinedIcon aria-hidden sx={{ fontSize: { xs: 32, sm: 36 } }} />
                <Typography variant="caption" sx={{ color: "inherit", fontWeight: 600 }}>
                    {categoryLabel}
                </Typography>
            </Box>

            <Stack spacing={1} sx={{ p: { xs: 2, sm: 2.5 }, flex: 1, minWidth: 0 }}>

                <Typography variant="subtitle1" component="h3" sx={{ fontWeight: 700, overflowWrap: "anywhere" }}>
                    {bag.title}
                </Typography>

                {showRestaurant && (
                    <Stack direction="row" spacing={1} sx={{ alignItems: "center", minWidth: 0 }}>
                        <StorefrontOutlinedIcon aria-hidden sx={{ fontSize: 18, color: "text.secondary" }} />
                        <Link
                            component={RouterLink}
                            to={`/restaurants/${bag.restaurantId}`}
                            variant="body2"
                            color="text.secondary"
                            underline="hover"
                            sx={{ fontWeight: 600, overflowWrap: "anywhere" }}
                        >
                            {bag.restaurantName}
                        </Link>
                    </Stack>
                )}

                <Stack direction="row" sx={{ alignItems: "baseline", flexWrap: "wrap", columnGap: 1 }}>
                    <Typography variant="h6" component="p" sx={{ fontWeight: 700 }}>
                        {formatCurrency(bag.price, bag.currencyCode)}
                    </Typography>
                    {bag.estimatedValue != null && (
                        <Typography variant="body2" color="text.secondary">
                            Worth {formatCurrency(bag.estimatedValue, bag.currencyCode)}
                        </Typography>
                    )}
                </Stack>

                <Stack direction="row" spacing={1} sx={{ alignItems: "center" }}>
                    <EventOutlinedIcon aria-hidden sx={{ fontSize: 18, color: "text.secondary" }} />
                    <Typography variant="body2" sx={{ fontWeight: 500 }}>
                        Pick up {formatDate(bag.pickupDate)}
                    </Typography>
                </Stack>

                <Stack direction="row" spacing={1} sx={{ alignItems: "center" }}>
                    <ScheduleIcon aria-hidden sx={{ fontSize: 18, color: "text.secondary" }} />
                    <Typography variant="body2" color="text.secondary">
                        {formatTime12Hour(bag.pickupStartTime)} – {formatTime12Hour(bag.pickupEndTime)}
                    </Typography>
                </Stack>

                <Typography variant="body2" sx={{ fontWeight: 600, color: availabilityColor }}>
                    {availabilityLabel}
                </Typography>

                {alreadyReserved ? (

                    <Stack spacing={1} sx={{ pt: 0.5 }}>

                        <Stack direction="row" spacing={1} sx={{ alignItems: "center", color: "success.main" }}>
                            <CheckCircleOutlineIcon aria-hidden sx={{ fontSize: 18 }} />
                            <Typography variant="body2" sx={{ fontWeight: 600, color: "inherit" }}>
                                Already reserved
                            </Typography>
                        </Stack>

                        <Box>
                            <Button
                                variant="outlined"
                                component={RouterLink}
                                to="/my-surprise-bags"
                                aria-label={`Already reserved - view your reservation for ${bag.title}`}
                                sx={{ minWidth: 120, width: { xs: "100%", sm: "auto" } }}
                            >
                                View My Reservation
                            </Button>
                        </Box>

                    </Stack>

                ) : (

                    <Box sx={{ pt: 0.5 }}>
                        <Button
                            variant="contained"
                            color="primary"
                            onClick={handleReserveClick}
                            disabled={bag.isSoldOut}
                            aria-label={bag.isSoldOut ? `${bag.title} is sold out` : `Reserve ${bag.title}`}
                            sx={{ minWidth: 120, width: { xs: "100%", sm: "auto" } }}
                        >
                            {bag.isSoldOut ? "Sold out" : "Reserve"}
                        </Button>
                    </Box>

                )}

            </Stack>

        </Card>

    );

}

export default SurpriseBagCard;
