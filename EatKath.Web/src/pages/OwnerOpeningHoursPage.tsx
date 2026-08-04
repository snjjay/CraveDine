import { useEffect, useState } from "react";

import {
    Button,
    Checkbox,
    CircularProgress,
    Paper,
    Table,
    TableBody,
    TableCell,
    TableContainer,
    TableHead,
    TableRow,
    TextField,
    Typography
} from "@mui/material";

import OwnerRestaurantService from "../services/OwnerRestaurantService";
import RestaurantOpeningHourService from "../services/RestaurantOpeningHourService";

import type { Restaurant } from "../types/Restaurant";
import type { RestaurantOpeningHour } from "../types/RestaurantOpeningHour";

function OwnerOpeningHoursPage() {

    const [restaurant, setRestaurant] =
        useState<Restaurant | null>(null);

    const [hours, setHours] =
        useState<RestaurantOpeningHour[]>([]);

    const [loading, setLoading] =
        useState(true);

    useEffect(() => {

        loadData();

    }, []);

    async function loadData() {

        try {

            const restaurantData =
                await OwnerRestaurantService.getMyRestaurant();

            setRestaurant(restaurantData);

            const openingHours =
                await RestaurantOpeningHourService.getByRestaurant(
                    restaurantData.id
                );

            setHours(openingHours);

        }
        finally {

            setLoading(false);

        }

    }

    async function saveHours() {

        try {

            for (const hour of hours) {

                await RestaurantOpeningHourService.update(
                    hour.id,
                    {
                        dayOfWeek: hour.dayOfWeek,
                        openTime: hour.openTime,
                        closeTime: hour.closeTime,
                        isClosed: hour.isClosed
                    }
                );

            }

            alert("Opening hours updated successfully.");

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




    if (loading) {

        return <CircularProgress />;

    }

    return (

        <>

            <Typography
                variant="h4"
                sx={{ mb: 3 }}
            >
                Opening Hours
            </Typography>

            <Typography>

                Restaurant:
                {" "}
                {restaurant?.name}

            </Typography>

            <TableContainer
                component={Paper}
                sx={{ mt: 3 }}
            >

                <Table>

                    <TableHead>

                        <TableRow>

                            <TableCell>Day</TableCell>
                            <TableCell>Open</TableCell>
                            <TableCell>Close</TableCell>
                            <TableCell>Closed</TableCell>

                        </TableRow>

                    </TableHead>

                    <TableBody>

                        {hours.map((hour) => (

                            <TableRow key={hour.id}>

                                <TableCell>
                                    {hour.dayOfWeek}
                                </TableCell>

                                <TableCell>

                                    <TextField
                                        type="time"
                                        size="small"
                                        value={hour.openTime.substring(0, 5)}
                                        onChange={(e) =>

                                            setHours(hours.map(h =>

                                                h.id === hour.id
                                                    ? {
                                                        ...h,
                                                        openTime: `${e.target.value}:00`
                                                    }
                                                    : h

                                            ))

                                        }
                                    />

                                </TableCell>

                                <TableCell>

                                    <TextField
                                        type="time"
                                        size="small"
                                        value={hour.closeTime.substring(0, 5)}
                                        onChange={(e) =>

                                            setHours(hours.map(h =>

                                                h.id === hour.id
                                                    ? {
                                                        ...h,
                                                        closeTime: `${e.target.value}:00`
                                                    }
                                                    : h

                                            ))

                                        }
                                    />

                                </TableCell>

                                <TableCell>

                                    <Checkbox
                                        checked={hour.isClosed}
                                        onChange={(e) =>

                                            setHours(hours.map(h =>

                                                h.id === hour.id
                                                    ? {
                                                        ...h,
                                                        isClosed: e.target.checked
                                                    }
                                                    : h

                                            ))

                                        }
                                    />

                                </TableCell>

                            </TableRow>

                        ))}

                    </TableBody>

                </Table>
                <Button
                    variant="contained"
                    sx={{ mt: 3 }}
                    onClick={saveHours}
                >
                    Save Opening Hours
                </Button>

            </TableContainer>

        </>

    );

}

export default OwnerOpeningHoursPage;