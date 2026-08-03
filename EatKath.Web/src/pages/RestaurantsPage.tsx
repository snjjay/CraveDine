import { useEffect, useState } from "react";

import {
    CircularProgress,
    Grid,
    Typography,
    TextField,
    FormControl,
    InputLabel,
    Select,
    MenuItem,
    type SelectChangeEvent
} from "@mui/material";

import RestaurantCard from "../components/restaurants/RestaurantCard";

import RestaurantService from "../services/RestaurantService";
import UserFavoriteService from "../services/UserFavoriteService";
import AreaService from "../services/AreaService";
import CuisineService from "../services/CuisineService";
import DiningTypeService from "../services/DiningTypeService";

import type { Restaurant } from "../types/Restaurant";
import type { UserFavorite } from "../types/UserFavorite";
import type { Area } from "../types/Area";
import type { Cuisine } from "../types/Cuisine";
import type { DiningType } from "../types/DiningType";

function RestaurantsPage() {

    const [restaurants, setRestaurants] = useState<Restaurant[]>([]);
    const [favorites, setFavorites] = useState<UserFavorite[]>([]);

    const [areas, setAreas] = useState<Area[]>([]);
    const [cuisines, setCuisines] = useState<Cuisine[]>([]);
    const [diningTypes, setDiningTypes] = useState<DiningType[]>([]);

    const [loading, setLoading] = useState(true);

    const [search, setSearch] = useState("");

    const [selectedArea, setSelectedArea] = useState("");
    const [selectedCuisine, setSelectedCuisine] = useState("");
    const [selectedDiningType, setSelectedDiningType] = useState("");

    useEffect(() => {

        loadData();

    }, []);

    async function loadData() {

        try {

            const restaurantsData =
                await RestaurantService.getAll();

            setRestaurants(restaurantsData);

            const areasData =
                await AreaService.getAll();

            setAreas(areasData);

            const cuisinesData =
                await CuisineService.getAll();

            setCuisines(cuisinesData);

            const diningTypesData =
                await DiningTypeService.getAll();

            setDiningTypes(diningTypesData);

            try {

                const favoritesData =
                    await UserFavoriteService.getMyFavorites();

                setFavorites(favoritesData);

            }
            catch {

                // Not logged in

            }

        }
        catch (error) {

            console.error(error);

        }
        finally {

            setLoading(false);

        }

    }

    if (loading)
        return <CircularProgress />;

    const filteredRestaurants = restaurants.filter(r => {

        const keyword = search.trim().toLowerCase();

        const matchesSearch =

            r.name.toLowerCase().includes(keyword) ||

            r.description.toLowerCase().includes(keyword) ||

            r.areaName.toLowerCase().includes(keyword) ||

            (r.bestDiscount?.toString() ?? "").includes(keyword);

        const matchesArea =

            selectedArea === "" ||

            r.areaId === Number(selectedArea);

        const matchesCuisine =

            selectedCuisine === "" ||

            r.cuisines.includes(selectedCuisine);

        const matchesDiningType =

            selectedDiningType === "" ||

            r.diningTypes.includes(selectedDiningType);

        return matchesSearch &&
            matchesArea &&
            matchesCuisine &&
            matchesDiningType;

    });

    return (

        <>

            <Typography
                variant="h4"
                sx={{ mb: 3 }}
            >
                Restaurants
            </Typography>

            <Grid
                container
                spacing={2}
                sx={{ mb: 3 }}
            >

                <Grid size={{ xs: 12, md: 4 }}>

                    <TextField
                        fullWidth
                        label="Search"
                        value={search}
                        onChange={(e) =>
                            setSearch(e.target.value)
                        }
                    />

                </Grid>

                <Grid size={{ xs: 12, md: 3 }}>

                    <FormControl fullWidth>

                        <InputLabel>Area</InputLabel>

                        <Select
                            label="Area"
                            value={selectedArea}
                            onChange={(e: SelectChangeEvent) =>
                                setSelectedArea(e.target.value)
                            }
                        >

                            <MenuItem value="">
                                All Areas
                            </MenuItem>

                            {areas.map(area => (

                                <MenuItem
                                    key={area.id}
                                    value={area.id.toString()}
                                >
                                    {area.name}
                                </MenuItem>

                            ))}

                        </Select>

                    </FormControl>

                </Grid>

                <Grid size={{ xs: 12, md: 2.5 }}>

                    <FormControl fullWidth>

                        <InputLabel>Cuisine</InputLabel>

                        <Select
                            label="Cuisine"
                            value={selectedCuisine}
                            onChange={(e: SelectChangeEvent) =>
                                setSelectedCuisine(e.target.value)
                            }
                        >

                            <MenuItem value="">
                                All Cuisines
                            </MenuItem>

                            {cuisines.map(cuisine => (

                                <MenuItem
                                    key={cuisine.id}
                                    value={cuisine.name}
                                >
                                    {cuisine.name}
                                </MenuItem>

                            ))}

                        </Select>

                    </FormControl>

                </Grid>

                <Grid size={{ xs: 12, md: 2.5 }}>

                    <FormControl fullWidth>

                        <InputLabel>Dining Type</InputLabel>

                        <Select
                            label="Dining Type"
                            value={selectedDiningType}
                            onChange={(e: SelectChangeEvent) =>
                                setSelectedDiningType(e.target.value)
                            }
                        >

                            <MenuItem value="">
                                All Dining Types
                            </MenuItem>

                            {diningTypes.map(type => (

                                <MenuItem
                                    key={type.id}
                                    value={type.name}
                                >
                                    {type.name}
                                </MenuItem>

                            ))}

                        </Select>

                    </FormControl>

                </Grid>

            </Grid>

            <Grid container spacing={3}>

                {filteredRestaurants.map(restaurant => (

                    <Grid
                        key={restaurant.id}
                        size={{
                            xs: 12,
                            sm: 6,
                            md: 4,
                            lg: 3
                        }}
                    >

                        <RestaurantCard
                            restaurant={restaurant}
                            isFavorite={
                                favorites.some(
                                    f => f.restaurantId === restaurant.id
                                )
                            }
                            onFavoriteChanged={loadData}
                        />

                    </Grid>

                ))}

            </Grid>

        </>

    );

}

export default RestaurantsPage;