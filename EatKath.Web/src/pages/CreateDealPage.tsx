import { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";

import {
    Button,
    Container,
    MenuItem,
    Paper,
    Stack,
    TextField,
    Typography
} from "@mui/material";

import OwnerRestaurantService from "../services/OwnerRestaurantService";
import OwnerDealService from "../services/OwnerDealService";

import type { DealForm } from "../types/DealForm";

function CreateDealPage() {

    const navigate = useNavigate();

    const [deal, setDeal] = useState<DealForm>({
        restaurantId: 0,
        title: "",
        description: "",
        discountPercentage: 20,
        offerType: 1,
        promoImageUrl: "",
        termsAndConditions: "",
        startDate: "",
        endDate: "",
        startTime: "18:00",
        endTime: "21:00",
        maximumGuests: 20,

        reservationLimit: 1,

        dailyRedemptionLimit: 100,
        isActive: true
    });

    useEffect(() => {

        loadRestaurant();

    }, []);

    async function loadRestaurant() {

        const restaurant = await OwnerRestaurantService.getMyRestaurant();

        setDeal(d => ({
            ...d,
            restaurantId: restaurant.id
        }));

    }

    async function saveDeal() {

        try {

            const request = {

                ...deal,

                startTime:
                    deal.startTime.length === 5
                        ? `${deal.startTime}:00`
                        : deal.startTime,

                endTime:
                    deal.endTime.length === 5
                        ? `${deal.endTime}:00`
                        : deal.endTime

            };

            console.log(request);

            await OwnerDealService.create(request);

            alert("Deal created successfully.");

            navigate("/owner/deals");

        }
        catch (error: any) {

            console.error(error);

            if (error.response) {

                console.log(error.response.data);

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

    return (

        <Container maxWidth="md">

            <Paper sx={{ p: 4, mt: 4 }}>

                <Typography
                    variant="h4"
                    sx={{ mb: 3 }}
                >
                    Create Deal
                </Typography>

                <Stack spacing={2}>

                    <TextField
                        label="Title"
                        value={deal.title}
                        onChange={(e) =>
                            setDeal({
                                ...deal,
                                title: e.target.value
                            })
                        }
                    />

                    <TextField
                        label="Description"
                        multiline
                        rows={4}
                        value={deal.description}
                        onChange={(e) =>
                            setDeal({
                                ...deal,
                                description: e.target.value
                            })
                        }
                    />

                    <TextField
                        label="Discount %"
                        type="number"
                        value={deal.discountPercentage}
                        onChange={(e) =>
                            setDeal({
                                ...deal,
                                discountPercentage: Number(e.target.value)
                            })
                        }
                    />

                    <TextField
                        select
                        label="Offer Type"
                        value={deal.offerType}
                        onChange={(e) =>
                            setDeal({
                                ...deal,
                                offerType: Number(e.target.value)
                            })
                        }
                    >
                        <MenuItem value={1}>Dine In</MenuItem>
                        <MenuItem value={2}>Takeaway</MenuItem>
                    </TextField>

                    <TextField
                        label="Start Date"
                        type="date"
                        slotProps={{
                            inputLabel: {
                                shrink: true
                            }
                        }}
                        value={deal.startDate}
                        onChange={(e) =>
                            setDeal({
                                ...deal,
                                startDate: e.target.value
                            })
                        }
                    />

                    <TextField
                        label="End Date"
                        type="date"
                        slotProps={{
                            inputLabel: {
                                shrink: true
                            }
                        }}
                        value={deal.endDate}
                        onChange={(e) =>
                            setDeal({
                                ...deal,
                                endDate: e.target.value
                            })
                        }
                    />

                    <TextField
                        label="Start Time"
                        type="time"
                        slotProps={{
                            inputLabel: {
                                shrink: true
                            }
                        }}
                        value={deal.startTime}
                        onChange={(e) =>
                            setDeal({
                                ...deal,
                                startTime: e.target.value
                            })
                        }
                    />

                    <TextField
                        label="End Time"
                        type="time"
                        slotProps={{
                            inputLabel: {
                                shrink: true
                            }
                        }}
                        value={deal.endTime}
                        onChange={(e) =>
                            setDeal({
                                ...deal,
                                endTime: e.target.value
                            })
                        }
                    />

                    <TextField
                        label="Maximum Guests"

                        type="number"
                        value={deal.maximumGuests}
                        onChange={(e) =>
                            setDeal({
                                ...deal,
                                maximumGuests: Number(e.target.value)
                            })
                        }
                    />

                    <TextField
                        label="Reservation Limit"
                        type="number"
                        value={deal.reservationLimit}
                        onChange={(e) =>
                            setDeal({
                                ...deal,
                                reservationLimit: Number(e.target.value)
                            })
                        }
                    />

                    <TextField
                        label="Daily Redemption Limit"
                        type="number"
                        value={deal.dailyRedemptionLimit}
                        onChange={(e) =>
                            setDeal({
                                ...deal,
                                dailyRedemptionLimit: Number(e.target.value)
                            })
                        }
                    />

                    <Stack
                        direction="row"
                        spacing={2}
                    >

                        <Button
                            variant="contained"
                            onClick={saveDeal}
                        >
                            Save
                        </Button>

                        <Button
                            onClick={() => navigate("/owner/deals")}
                        >
                            Cancel
                        </Button>

                    </Stack>

                </Stack>

            </Paper>

        </Container>

    );

}

export default CreateDealPage;