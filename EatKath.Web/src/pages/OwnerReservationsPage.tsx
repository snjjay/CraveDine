import { useEffect, useState } from "react";

import {
    Button,
    Card,
    CardContent,
    Chip,
    CircularProgress,
    Dialog,
    DialogActions,
    DialogContent,
    DialogTitle,
    Stack,
    TextField,
    Typography
} from "@mui/material";

import OwnerRestaurantService from "../services/OwnerRestaurantService";
import RedemptionService from "../services/RedemptionService";
import { useNotification } from "../features/notifications/NotificationContext";
import {
    RedemptionStatus,
    getRedemptionStatusColor,
    getRedemptionStatusLabel
} from "../utils/redemption";

import type { Restaurant } from "../types/Restaurant";
import type { Redemption } from "../types/Redemption";

function OwnerReservationsPage() {

   const { notify } = useNotification();

   const [, setRestaurant] =
    useState<Restaurant | null>(null);

    const [redemptions, setRedemptions] =
        useState<Redemption[]>([]);

    const [loading, setLoading] =
        useState(true);

        const [selectedId, setSelectedId] =
    useState<number | null>(null);

const [billAmount, setBillAmount] =
    useState(0);

    useEffect(() => {

        loadData();

    }, []);

    async function loadData() {

        try {

            const restaurantData =
                await OwnerRestaurantService.getMyRestaurant();

            setRestaurant(restaurantData);

            const data =
                await RedemptionService.getRestaurantRedemptions(
                    restaurantData.id
                );

            setRedemptions(data);

        }
        finally {

            setLoading(false);

        }

    }

async function completeRedemption() {

    if (selectedId === null)
        return;

    try {

        await RedemptionService.complete(

            selectedId,

            {
                billAmount
            }

        );

        setSelectedId(null);

        setBillAmount(0);

        await loadData();

    }
    catch (error: any) {

        console.error(error);

        notify(
            error.response?.data?.Message ??
            error.message ??
            "Something went wrong. Please try again.",
            "error"
        );

    }

}


    if (loading)
        return <CircularProgress />;

    return (

        <>

            <Typography
                variant="h4"
                sx={{ mb: 3 }}
            >
                Owner Reservations
            </Typography>

            <Stack spacing={2}>

                {redemptions.map(r => (

                    <Card key={r.id}>

                        <CardContent>

                            <Typography variant="h6">

                                Redemption #{r.id}

                            </Typography>

                            <Typography>

                                Customer: {r.customerName}

                            </Typography>

                            <Typography>

                                Deal: {r.dealTitle}

                            </Typography>

                            <Typography>

                                Date: {r.arrivalDate}

                            </Typography>

                            <Typography>

                                Time: {r.arrivalTime}

                            </Typography>

                            <Typography>

                                Guests: {r.guestCount}

                            </Typography>

                            <Chip
                                sx={{ mt: 2 }}
                                label={getRedemptionStatusLabel(r.status)}
                                color={getRedemptionStatusColor(r.status)}
                            />

                            {r.status === RedemptionStatus.Redeemed && (

                                <Button
    sx={{ ml: 2 }}
    variant="contained"
    onClick={() => {

        setSelectedId(r.id);

        setBillAmount(0);

    }}
>
    Complete
</Button>

                            )}

                        </CardContent>

                    </Card>

                ))}

            </Stack>

            <Dialog
    open={selectedId !== null}
    onClose={() => setSelectedId(null)}
>

    <DialogTitle>

        Complete Redemption

    </DialogTitle>

    <DialogContent>

        <TextField
            fullWidth
            label="Bill Amount"
            type="number"
            value={billAmount}
            onChange={(e) =>
                setBillAmount(
                    Number(e.target.value)
                )
            }
            sx={{ mt: 2 }}
        />

    </DialogContent>

    <DialogActions>

        <Button
            onClick={() =>
                setSelectedId(null)
            }
        >
            Cancel
        </Button>

        <Button
    variant="contained"
    onClick={completeRedemption}
>
    Save
</Button>

    </DialogActions>

</Dialog>

        </>

    );

}

export default OwnerReservationsPage;