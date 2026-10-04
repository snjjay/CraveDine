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
import AreaService from "../services/AreaService";
import { useNotification } from "../features/notifications/NotificationContext";
import { DEFAULT_CURRENCY_CODE, SUPPORTED_CURRENCIES } from "../utils/currency";

import type { Area } from "../types/Area";
import type { CreateRestaurant } from "../types/CreateRestaurant";

function OwnerCreateRestaurantPage() {

    const { notify } = useNotification();

    const navigate = useNavigate();

    const [areas, setAreas] = useState<Area[]>([]);

    const [saving, setSaving] = useState(false);

    const [restaurant, setRestaurant] = useState<CreateRestaurant>({
        ownerId: 0,
        name: "",
        description: "",
        address: "",
        areaId: 0,
        phoneNumber: "",
        email: "",
        website: "",
        logoUrl: "",
        currencyCode: DEFAULT_CURRENCY_CODE,
        isActive: true
    });

    useEffect(() => {

        loadAreas();

    }, []);

    async function loadAreas() {

        try {

            const data = await AreaService.getAll();

            setAreas(data);

        }
        catch (error: any) {

            console.error(error);

            notify(
                error.response?.data?.Message ??
                error.message ??
                "Failed to load areas. Please try again.",
                "error"
            );

        }

    }

    async function saveRestaurant() {

        if (!restaurant.name.trim()) {

            notify("Restaurant name is required.", "warning");

            return;
        }

        if (!restaurant.address.trim()) {

            notify("Address is required.", "warning");

            return;
        }

        if (!restaurant.areaId) {

            notify("Please select an area.", "warning");

            return;
        }

        if (!restaurant.phoneNumber.trim()) {

            notify("Phone number is required.", "warning");

            return;
        }

        if (!restaurant.email.trim()) {

            notify("Email is required.", "warning");

            return;
        }

        setSaving(true);

        try {

            const created =
                await OwnerRestaurantService.create(restaurant);

            notify("Restaurant created successfully.", "success");

            navigate(
                `/owner/restaurant?restaurantId=${created.id}`
            );

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
        finally {

            setSaving(false);

        }

    }

    return (

        <Container maxWidth="md">

            <Paper sx={{ p: 4, mt: 4 }}>

                <Typography
                    variant="h4"
                    sx={{ mb: 3 }}
                >
                    Create Restaurant
                </Typography>

                <Stack spacing={2}>

                    <TextField
                        label="Restaurant Name"
                        required
                        value={restaurant.name}
                        onChange={(e) =>
                            setRestaurant({
                                ...restaurant,
                                name: e.target.value
                            })
                        }
                    />

                    <TextField
                        label="Description"
                        multiline
                        rows={4}
                        value={restaurant.description}
                        onChange={(e) =>
                            setRestaurant({
                                ...restaurant,
                                description: e.target.value
                            })
                        }
                    />

                    <TextField
                        label="Address"
                        required
                        value={restaurant.address}
                        onChange={(e) =>
                            setRestaurant({
                                ...restaurant,
                                address: e.target.value
                            })
                        }
                    />

                    <TextField
                        select
                        label="Area"
                        required
                        value={restaurant.areaId || ""}
                        onChange={(e) =>
                            setRestaurant({
                                ...restaurant,
                                areaId: Number(e.target.value)
                            })
                        }
                    >

                        {areas.map(area => (

                            <MenuItem
                                key={area.id}
                                value={area.id}
                            >
                                {area.name}
                            </MenuItem>

                        ))}

                    </TextField>

                    <TextField
                        select
                        label="Currency"
                        required
                        value={restaurant.currencyCode}
                        onChange={(e) =>
                            setRestaurant({
                                ...restaurant,
                                currencyCode: e.target.value
                            })
                        }
                    >

                        {SUPPORTED_CURRENCIES.map(currency => (

                            <MenuItem
                                key={currency.code}
                                value={currency.code}
                            >
                                {currency.label}
                            </MenuItem>

                        ))}

                    </TextField>

                    <TextField
                        label="Phone"
                        required
                        value={restaurant.phoneNumber}
                        onChange={(e) =>
                            setRestaurant({
                                ...restaurant,
                                phoneNumber: e.target.value
                            })
                        }
                    />

                    <TextField
                        label="Email"
                        required
                        value={restaurant.email}
                        onChange={(e) =>
                            setRestaurant({
                                ...restaurant,
                                email: e.target.value
                            })
                        }
                    />

                    <TextField
                        label="Website"
                        value={restaurant.website}
                        onChange={(e) =>
                            setRestaurant({
                                ...restaurant,
                                website: e.target.value
                            })
                        }
                    />

                    <Stack
                        direction="row"
                        spacing={2}
                    >

                        <Button
                            variant="contained"
                            disabled={saving}
                            onClick={saveRestaurant}
                        >
                            {saving ? "Creating..." : "Create Restaurant"}
                        </Button>

                        <Button
                            disabled={saving}
                            onClick={() => navigate("/owner")}
                        >
                            Cancel
                        </Button>

                    </Stack>

                </Stack>

            </Paper>

        </Container>

    );

}

export default OwnerCreateRestaurantPage;
