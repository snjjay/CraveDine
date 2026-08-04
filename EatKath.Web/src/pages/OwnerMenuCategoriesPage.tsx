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
import MenuCategoryService from "../services/MenuCategoryService";

import type { Restaurant } from "../types/Restaurant";
import type { MenuCategory } from "../types/MenuCategory";

function OwnerMenuCategoriesPage() {

    const [editingId, setEditingId] =
        useState<number | null>(null);

    const [restaurant, setRestaurant] =
        useState<Restaurant | null>(null);

    const [categories, setCategories] =
        useState<MenuCategory[]>([]);

    const [name, setName] = useState("");

    const [displayOrder, setDisplayOrder] = useState(1);

    useEffect(() => {

        loadData();

    }, []);

    async function loadData() {

        const restaurantData =
            await OwnerRestaurantService.getMyRestaurant();

        setRestaurant(restaurantData);

        const data =
            await MenuCategoryService.getByRestaurant(
                restaurantData.id
            );

        setCategories(data);

    }


    async function saveCategory() {

        if (!restaurant)
            return;

        try {

            if (editingId === null) {

                await MenuCategoryService.create({

                    restaurantId: restaurant.id,

                    name,

                    displayOrder

                });

            }
            else {

                await MenuCategoryService.update(

                    editingId,

                    {

                        name,

                        displayOrder

                    }

                );

            }

            setEditingId(null);

            setName("");

            setDisplayOrder(1);

            loadData();

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

    async function deleteCategory(id: number) {

        if (!confirm("Delete this category?"))
            return;

        try {

            await MenuCategoryService.delete(id);

            loadData();

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
                    Menu Categories
                </Typography>

                <Stack spacing={2}>

                    <TextField
                        label="Category Name"
                        value={name}
                        onChange={(e) =>
                            setName(e.target.value)
                        }
                    />

                    <TextField
                        label="Display Order"
                        type="number"
                        value={displayOrder}
                        onChange={(e) =>
                            setDisplayOrder(
                                Number(e.target.value)
                            )
                        }
                    />

                    <Button
                        variant="contained"
                        onClick={saveCategory}
                    >
                        {editingId === null
                            ? "Add Category"
                            : "Update Category"}
                    </Button>

                    {categories.map(category => (

                        <Stack
                            key={category.id}
                            direction="row"
                            spacing={2}
                            alignItems="center"
                        >

                            <Typography sx={{ flex: 1 }}>

                                {category.displayOrder}. {category.name}

                            </Typography>

                            <Button
                                size="small"
                                variant="outlined"
                                onClick={() => {

                                    setEditingId(category.id);

                                    setName(category.name);

                                    setDisplayOrder(category.displayOrder);

                                }}
                            >
                                Edit
                            </Button>

                            <Button
                                size="small"
                                color="error"
                                variant="outlined"
                                onClick={() => deleteCategory(category.id)}
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

export default OwnerMenuCategoriesPage;