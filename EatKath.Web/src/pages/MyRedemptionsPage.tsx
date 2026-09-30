import { useEffect, useState } from "react";

import {
    Card,
    CardContent,
    Chip,
    CircularProgress,
    Stack,
    Typography
} from "@mui/material";

import RedemptionService from "../services/RedemptionService";
import { useNotification } from "../features/notifications/NotificationContext";

import type { Redemption } from "../types/Redemption";

function MyRedemptionsPage() {

    const { notify } = useNotification();

    const [redemptions, setRedemptions] =
        useState<Redemption[]>([]);

    const [loading, setLoading] =
        useState(true);

    useEffect(() => {

        loadRedemptions();

    }, []);

    async function loadRedemptions() {

        try {

            const data =
                await RedemptionService.getMyHistory();

            setRedemptions(data);

        }
        catch (error: any) {

            console.error(error);

            notify(
                error.response?.data?.Message ??
                error.message ??
                "Failed to load your redemption history. Please try again.",
                "error"
            );

        }
        finally {

            setLoading(false);

        }

    }

    function getChipColor(status?: string) {

        switch (status) {

            case "Redeemed":
                return "warning";

            case "Completed":
                return "success";

            case "Cancelled":
                return "default";

            case "Expired":
                return "error";

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
                My Redemptions
            </Typography>

            {redemptions.length === 0 ? (

                <Typography>
                    You have no redemptions yet.
                </Typography>

            ) : (

                <Stack spacing={2}>

                    {redemptions.map(r => (

                        <Card key={r.id}>

                            <CardContent>

                                <Typography variant="h6">

                                    {r.dealTitle}

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

                                {r.billAmount != null && (

                                    <Typography>

                                        Bill Amount: NPR {r.billAmount}

                                    </Typography>

                                )}

                                {r.discountAmount != null && (

                                    <Typography>

                                        Discount Amount: NPR {r.discountAmount}

                                    </Typography>

                                )}

                                {r.finalAmount != null && (

                                    <Typography>

                                        Final Amount: NPR {r.finalAmount}

                                    </Typography>

                                )}

                                <Typography>

                                    Redeemed: {new Date(r.redeemedAt).toLocaleString()}

                                </Typography>

                                {r.completedAt && (

                                    <Typography>

                                        Completed: {new Date(r.completedAt).toLocaleString()}

                                    </Typography>

                                )}

                                <Chip
                                    sx={{ mt: 2 }}
                                    label={r.status}
                                    color={getChipColor(r.status)}
                                />

                            </CardContent>

                        </Card>

                    ))}

                </Stack>

            )}

        </>

    );

}

export default MyRedemptionsPage;
