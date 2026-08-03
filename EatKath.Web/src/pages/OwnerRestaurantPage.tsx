import { useEffect, useState } from "react";

import {
    Button,
    Container,
    Paper,
    Stack,
    TextField,
    Typography
} from "@mui/material";

import OwnerRestaurantService from "../services/OwnerRestaurantService";

import type { Restaurant } from "../types/Restaurant";
import type { UpdateRestaurant } from "../types/UpdateRestaurant";

function OwnerRestaurantPage() {

    const [restaurantId, setRestaurantId] = useState(0);

    const [logoFile, setLogoFile] = useState<File | null>(null);
    const [coverFile, setCoverFile] = useState<File | null>(null);
    const [menuFile, setMenuFile] = useState<File | null>(null);

    const [restaurant, setRestaurant] = useState<UpdateRestaurant>({
        name: "",
        description: "",
        address: "",
        phoneNumber: "",
        email: "",
        website: "",
        areaId: 0,
        isActive: true
    });

    useEffect(() => {

        loadRestaurant();

    }, []);

    async function loadRestaurant() {

        const data: Restaurant =
            await OwnerRestaurantService.getMyRestaurant();

        setRestaurantId(data.id);

        setRestaurant({
            name: data.name,
            description: data.description,
            address: data.address,
            phoneNumber: data.phoneNumber,
            email: data.email,
            website: data.website,
            areaId: data.areaId,
            isActive: data.isActive
        });

    }

    async function save() {

        await OwnerRestaurantService.update(
            restaurantId,
            restaurant
        );

        if (logoFile)
            await OwnerRestaurantService.uploadLogo(
                restaurantId,
                logoFile
            );

        if (coverFile)
            await OwnerRestaurantService.uploadCover(
                restaurantId,
                coverFile
            );

        if (menuFile)
            await OwnerRestaurantService.uploadMenu(
                restaurantId,
                menuFile
            );

        alert("Restaurant updated successfully.");

        loadRestaurant();

    }

    return (

        <Container maxWidth="md">

            <Paper sx={{ p: 4, mt: 4 }}>

                <Typography
                    variant="h4"
                    sx={{ mb: 3 }}
                >
                    Restaurant Profile
                </Typography>

                <Stack spacing={2}>

                    <TextField
                        label="Restaurant Name"
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
                        value={restaurant.address}
                        onChange={(e) =>
                            setRestaurant({
                                ...restaurant,
                                address: e.target.value
                            })
                        }
                    />

                    <TextField
                        label="Phone"
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

                    <Typography variant="h6">
                        Logo
                    </Typography>

                    <input
                        type="file"
                        accept="image/*"
                        onChange={(e) =>
                            setLogoFile(
                                e.target.files?.[0] ?? null
                            )
                        }
                    />

                    <Typography variant="h6">
                        Cover Image
                    </Typography>

                    <input
                        type="file"
                        accept="image/*"
                        onChange={(e) =>
                            setCoverFile(
                                e.target.files?.[0] ?? null
                            )
                        }
                    />

                    <Typography variant="h6">
                        Menu PDF
                    </Typography>

                    <input
                        type="file"
                        accept=".pdf"
                        onChange={(e) =>
                            setMenuFile(
                                e.target.files?.[0] ?? null
                            )
                        }
                    />

                    <Button
                        variant="contained"
                        onClick={save}
                    >
                        Save Restaurant
                    </Button>

                </Stack>

            </Paper>

        </Container>

    );

}

export default OwnerRestaurantPage;