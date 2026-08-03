import {
    Button,
    Card,
    CardContent,
    Chip,
    Stack,
    Typography
} from "@mui/material";

import type { Deal } from "../../types/Deal";

interface Props {
    deal: Deal;
}

function DealCard({ deal }: Props) {

    return (
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

                <Button
                    variant="contained"
                    color="primary"
                >
                    Reserve
                </Button>

            </CardContent>

        </Card>
    );
}

export default DealCard;