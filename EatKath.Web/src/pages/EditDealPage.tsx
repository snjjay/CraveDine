import { useEffect, useState } from "react";
import { useNavigate, useParams } from "react-router-dom";

import {
    Button,
    CircularProgress,
    Container,
    MenuItem,
    Paper,
    Stack,
    TextField,
    Typography
} from "@mui/material";

import OwnerDealService from "../services/OwnerDealService";
import type { UpdateDeal } from "../types/UpdateDeal";

function EditDealPage() {

    const { id } = useParams();

    const navigate = useNavigate();

    const [loading, setLoading] = useState(true);

    const [deal, setDeal] = useState<UpdateDeal>({
        title: "",
        description: "",
        discountPercentage: 0,
        offerType: 1,
        promoImageUrl: "",
        termsAndConditions: "",
        startDate: "",
        endDate: "",
        startTime: "",
        endTime: "",
        maximumGuests: 20,
        reservationLimit: 1,
        dailyRedemptionLimit: 100,
        isActive: true
    });

    useEffect(() => {

        loadDeal();

    }, []);

    async function loadDeal() {

        if (!id)
            return;

        const data = await OwnerDealService.getById(Number(id));

        setDeal({
            title: data.title,
            description: data.description,
            discountPercentage: data.discountPercentage,
            offerType: data.offerType,
            promoImageUrl: data.promoImageUrl,
            termsAndConditions: data.termsAndConditions,
            startDate: data.startDate,
            endDate: data.endDate,
            startTime: data.startTime.substring(0, 5),
            endTime: data.endTime.substring(0, 5),
            maximumGuests: data.maximumGuests,
            reservationLimit: data.reservationLimit,
            dailyRedemptionLimit: data.dailyRedemptionLimit,
            isActive: data.isActive
        });

        setLoading(false);

    }

    async function save() {

        if (!id)
            return;

        await OwnerDealService.update(Number(id), {

            ...deal,

            startTime:
                deal.startTime.length === 5
                    ? `${deal.startTime}:00`
                    : deal.startTime,

            endTime:
                deal.endTime.length === 5
                    ? `${deal.endTime}:00`
                    : deal.endTime

        });

        alert("Deal updated.");

        navigate("/owner/deals");

    }

    if (loading)
        return <CircularProgress />;

    return (

        <Container maxWidth="md">

            <Paper sx={{ p: 4, mt: 4 }}>

                <Typography
                    variant="h4"
                    sx={{ mb: 3 }}
                >
                    Edit Deal
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
                        slotProps={{ inputLabel: { shrink: true } }}
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
                        slotProps={{ inputLabel: { shrink: true } }}
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
                        slotProps={{ inputLabel: { shrink: true } }}
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
                        slotProps={{ inputLabel: { shrink: true } }}
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
                            onClick={save}
                        >
                            Save Changes
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

export default EditDealPage;