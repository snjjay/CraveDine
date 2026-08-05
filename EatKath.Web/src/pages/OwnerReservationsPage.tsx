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

import type { Restaurant } from "../types/Restaurant";
import type { Redemption } from "../types/Redemption";

function OwnerReservationsPage() {

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

        if (error.response) {

            alert(
                JSON.stringify(
                    error.response.data,
                    null,
                    2
                )
            );

        }
        else {

            alert(error.message);

        }

    }

}


    function getChipColor(status: string) {

        switch (status) {

            case "Redeemed":
                return "warning";

            case "Completed":
                return "success";

            default:
                return "default";

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

                                Reservation #{r.id}

                            </Typography>

                            <Typography>

                                Customer: {r.userName}

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
                                label={r.status}
                                color={getChipColor(r.status)}
                            />

                            {r.status === "Redeemed" && (

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