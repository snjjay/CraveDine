import { useEffect, useState } from "react";

import {
    Button,
    Checkbox,
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

import type { Restaurant } from "../types/Restaurant";
import type { MenuCategory } from "../types/MenuCategory";
import type { MenuItem } from "../types/MenuItem";

function OwnerMenuItemsPage() {

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

    useEffect(() => {

        loadData();

    }, []);

    async function loadData() {

        const restaurantData =
            await OwnerRestaurantService.getMyRestaurant();

        setRestaurant(restaurantData);

        const categoryData =
            await MenuCategoryService.getByRestaurant(
                restaurantData.id
            );

        setCategories(categoryData);

        const itemData =
            await MenuItemService.getByRestaurant(
                restaurantData.id
            );

        setItems(itemData);

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


    async function deleteMenuItem(id: number) {

        if (!confirm("Delete this menu item?"))
            return;

        try {

            await MenuItemService.delete(id);

            await loadData();

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



    return (

        <Container maxWidth="md">

            <Paper sx={{ p: 4, mt: 4 }}>

                <Typography
                    variant="h4"
                    sx={{ mb: 3 }}
                >
                    Menu Items
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