import { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";

import {
    Button,
    Card,
    CardContent,
    Chip,
    CircularProgress,
    Stack,
    Typography
} from "@mui/material";

import OwnerDealService from "../services/OwnerDealService";
import type { Deal } from "../types/Deal";

function OwnerDealsPage() {

    const navigate = useNavigate();

    const [deals, setDeals] = useState<Deal[]>([]);
    const [loading, setLoading] = useState(true);

    useEffect(() => {

        loadDeals();

    }, []);

    async function loadDeals() {

        try {

            const data = await OwnerDealService.getMyDeals();

            setDeals(data);

        }
        catch (error) {

            console.error(error);

        }
        finally {

            setLoading(false);

        }

    }

    async function deleteDeal(id: number) {

        if (!confirm("Delete this deal?"))
            return;

        await OwnerDealService.delete(id);

        loadDeals();

    }

    if (loading)
        return <CircularProgress />;

    return (

        <>

            <Stack
                direction="row"
                spacing={2}
                sx={{
                    justifyContent: "space-between",
                    alignItems: "center",
                    mb: 3
                }}
            >

                <Typography variant="h4">

                    My Deals

                </Typography>

                <Button
                    variant="contained"
                    onClick={() => navigate("/owner/deals/new")}
                >
                    New Deal
                </Button>

            </Stack>

            {deals.map(deal => (

                <Card
                    key={deal.id}
                    sx={{ mb: 2 }}
                >

                    <CardContent>

                        <Stack
                            direction="row"
                            spacing={2}
                            sx={{
                                justifyContent: "space-between",
                                alignItems: "center"
                            }}
                        >

                            <Typography variant="h6">

                                {deal.title}

                            </Typography>

                            <Chip
                                color="success"
                                label={`${deal.discountPercentage}% OFF`}
                            />

                        </Stack>

                        <Typography sx={{ mt: 2 }}>

                            {deal.description}

                        </Typography>

                        <Typography
                            variant="body2"
                            sx={{ mt: 2 }}
                        >

                            {deal.startDate} - {deal.endDate}

                        </Typography>

                        <Typography variant="body2">

                            {deal.startTime} - {deal.endTime}

                        </Typography>

                        <Stack
                            direction="row"
                            spacing={2}
                            sx={{ mt: 3 }}
                        >

                            <Button
                                variant="outlined"
                                onClick={() =>
                                    navigate(`/owner/deals/edit/${deal.id}`)
                                }
                            >
                                Edit
                            </Button>

                            <Button
                                variant="outlined"
                                color="error"
                                onClick={() => deleteDeal(deal.id)}
                            >
                                Delete
                            </Button>

                        </Stack>

                    </CardContent>

                </Card>

            ))}

        </>

    );

}

export default OwnerDealsPage;