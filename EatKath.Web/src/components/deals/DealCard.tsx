import { useState } from "react";

import {
    Button,
    Card,
    CardContent,
    Chip,
    Stack,
    Typography
} from "@mui/material";

import ReservationDialog from "../reservations/ReservationDialog";
import type { Deal } from "../../types/Deal";

interface Props {
    deal: Deal;
}

function DealCard({ deal }: Props) {

    const [open, setOpen] = useState(false);

    return (
        <>
            <Card sx={{ mb: 2 }}>

                <CardContent>

                    <Stack
                        direction="row"
                        sx={{
                            justifyContent: "space-between",
                            alignItems: "center",
                            mb: 2
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

                    <Typography sx={{ mb: 2 }}>
                        {deal.description}
                    </Typography>

                    <Typography
                        variant="body2"
                        color="text.secondary"
                    >
                        Time: {deal.startTime} - {deal.endTime}
                    </Typography>

                    <Typography
                        variant="body2"
                        color="text.secondary"
                        sx={{ mb: 2 }}
                    >
                        Valid: {deal.startDate} - {deal.endDate}
                    </Typography>

                    {deal.isActive ? (

                        <Button
                            variant="contained"
                            color="primary"
                            onClick={() => setOpen(true)}
                        >
                            Reserve
                        </Button>

                    ) : (

                        <Stack
                            direction="row"
                            spacing={2}
                            alignItems="center"
                        >

                            <Chip
                                label="Unavailable"
                                color="default"
                            />

                            <Typography
                                variant="body2"
                                color="text.secondary"
                            >
                                This deal is currently unavailable for reservations.
                            </Typography>

                        </Stack>

                    )}

                </CardContent>

            </Card>

            <ReservationDialog
                open={open}
                onClose={() => setOpen(false)}
                deal={deal}
            />
        </>
    );
}

export default DealCard;