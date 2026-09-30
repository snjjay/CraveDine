import { useEffect, useState } from "react";
import { useSearchParams } from "react-router-dom";

import {
    Button,
    Checkbox,
    CircularProgress,
    Container,
    FormControlLabel,
    MenuItem as MuiMenuItem,
    Paper,
    Stack,
    TextField,
    Typography
} from "@mui/material";

import OwnerRestaurantService from "../services/OwnerRestaurantService";
import MenuCategoryService from "../services/MenuCategoryService";
import MenuItemService from "../services/MenuItemService";
import { useNotification } from "../features/notifications/NotificationContext";

import type { Restaurant } from "../types/Restaurant";
import type { MenuCategory } from "../types/MenuCategory";
import type { MenuItem } from "../types/MenuItem";

function OwnerMenuItemsPage() {

    const { notify } = useNotification();

    const [searchParams] = useSearchParams();

    const restaurantId =
        Number(searchParams.get("restaurantId")) || null;

    const [editingId, setEditingId] =
        useState<number | null>(null);

    const [isFeatured, setIsFeatured] =
        useState(false);

    const [isAvailable, setIsAvailable] =
        useState(true);

    const [restaurant, setRestaurant] =
        useState<Restaurant | null>(null);

    const [categories, setCategories] =
        useState<MenuCategory[]>([]);

    const [items, setItems] =
        useState<MenuItem[]>([]);

    const [menuCategoryId, setMenuCategoryId] =
        useState(0);

    const [name, setName] = useState("");

    const [description, setDescription] =
        useState("");

    const [price, setPrice] =
        useState(0);

    const [loading, setLoading] =
        useState(true);

    useEffect(() => {

        loadData();

    }, [restaurantId]);

    async function loadData() {

        if (!restaurantId) {

            setRestaurant(null);
            setCategories([]);
            setItems([]);
            setLoading(false);

            return;
        }

        try {

            const restaurants =
                await OwnerRestaurantService.getMyRestaurants();

            const selectedRestaurant =
                restaurants.find(
                    restaurant =>
                        restaurant.id === restaurantId
                );

            if (!selectedRestaurant) {

                setRestaurant(null);
                setCategories([]);
                setItems([]);

                return;
            }

            setRestaurant(selectedRestaurant);

            const categoryData =
                await MenuCategoryService.getByRestaurant(
                    selectedRestaurant.id
                );

            setCategories(categoryData);

            const itemData =
                await MenuItemService.getByRestaurant(
                    selectedRestaurant.id
                );

            setItems(itemData);

        }
        catch (error) {

            console.error(error);

        }
        finally {

            setLoading(false);

        }

    }


    async function saveMenuItem() {

        if (!restaurant)
            return;

        try {

            if (editingId === null) {

                await MenuItemService.create({

                    restaurantId: restaurant.id,

                    menuCategoryId,

                    name,

                    description,

                    price,

                    isFeatured,

                    isAvailable

                });

            }
            else {

                await MenuItemService.update(

                    editingId,

                    {

                        menuCategoryId,

                        name,

                        description,

                        price,

                        isFeatured,

                        isAvailable

                    }

                );

            }

            setEditingId(null);

            setMenuCategoryId(0);

            setName("");

            setDescription("");

            setPrice(0);

            setIsFeatured(false);

            setIsAvailable(true);

            await loadData();

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

    }


    async function deleteMenuItem(id: number) {

        if (!confirm("Delete this menu item?"))
            return;

        try {

            await MenuItemService.delete(id);

            await loadData();

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

    }

    if (loading)
        return <CircularProgress />;

    if (!restaurantId || !restaurant) {

        return (

            <Typography>

                Restaurant not selected.

            </Typography>

        );
    }

    return (

        <Container maxWidth="md">

            <Paper sx={{ p: 4, mt: 4 }}>

                <Typography
                    variant="h4"
                    sx={{ mb: 3 }}
                >
                    Menu Items
                </Typography>

                <Typography sx={{ mb: 3 }}>

                    Restaurant:
                    {" "}
                    {restaurant.name}

                </Typography>

                <Stack spacing={2}>

                    <TextField
                        select
                        label="Category"
                        value={menuCategoryId}
                        onChange={(e) =>
                            setMenuCategoryId(
                                Number(e.target.value)
                            )
                        }
                    >

                        {categories.map(category => (

                            <MuiMenuItem
                                key={category.id}
                                value={category.id}
                            >
                                {category.name}
                            </MuiMenuItem>

                        ))}

                    </TextField>

                    <TextField
                        label="Item Name"
                        value={name}
                        onChange={(e) =>
                            setName(e.target.value)
                        }
                    />

                    <TextField
                        label="Description"
                        multiline
                        rows={3}
                        value={description}
                        onChange={(e) =>
                            setDescription(e.target.value)
                        }
                    />

                    <TextField
                        label="Price"
                        type="number"
                        value={price}
                        onChange={(e) =>
                            setPrice(
                                Number(e.target.value)
                            )
                        }
                    />


                    <FormControlLabel
                        control={
                            <Checkbox
                                checked={isFeatured}
                                onChange={(e) =>
                                    setIsFeatured(e.target.checked)
                                }
                            />
                        }
                        label="Featured"
                    />

                    <FormControlLabel
                        control={
                            <Checkbox
                                checked={isAvailable}
                                onChange={(e) =>
                                    setIsAvailable(e.target.checked)
                                }
                            />
                        }
                        label="Available"
                    />




                    <Button
                        variant="contained"
                        onClick={saveMenuItem}
                    >
                        {editingId === null
                            ? "Add Menu Item"
                            : "Update Menu Item"}
                    </Button>

                    {items.map(item => (

                        <Stack
                            key={item.id}
                            direction="row"
                            spacing={2}
                            alignItems="center"
                        >

                            <Typography sx={{ flex: 1 }}>

                                {item.name} - ${item.price}

                            </Typography>

                            <Button
                                size="small"
                                variant="outlined"
                                onClick={() => {

                                    setEditingId(item.id);

                                    setMenuCategoryId(item.menuCategoryId);

                                    setName(item.name);

                                    setDescription(item.description);

                                    setPrice(item.price);

                                    setIsFeatured(item.isFeatured);

                                    setIsAvailable(item.isAvailable);

                                }}
                            >
                                Edit
                            </Button>

                            <Button
                                size="small"
                                color="error"
                                variant="outlined"
                                onClick={() => deleteMenuItem(item.id)}
                            >
                                Delete
                            </Button>

                        </Stack>

                    ))}

                </Stack>

            </Paper>

        </Container>

    );

}

export default OwnerMenuItemsPage;