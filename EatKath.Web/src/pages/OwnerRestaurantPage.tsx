import { useEffect, useRef, useState } from "react";
import { useSearchParams } from "react-router-dom";

import {
    Button,
    Card,
    CardMedia,
    CircularProgress,
    Container,
    Dialog,
    DialogActions,
    DialogContent,
    DialogContentText,
    DialogTitle,
    Grid,
    MenuItem,
    Paper,
    Stack,
    TextField,
    Typography
} from "@mui/material";

import OwnerRestaurantService from "../services/OwnerRestaurantService";
import RestaurantImageService from "../services/RestaurantImageService";
import AreaService from "../services/AreaService";
import CuisineService from "../services/CuisineService";
import CuisineMultiSelect from "../components/restaurants/CuisineMultiSelect";
import { getApiErrorMessage } from "../utils/apiError";
import { getImageUrl } from "../utils/imageUrl";
import { DEFAULT_CURRENCY_CODE, SUPPORTED_CURRENCIES } from "../utils/currency";
import { useNotification } from "../features/notifications/NotificationContext";

import type { Area } from "../types/Area";
import type { Cuisine } from "../types/Cuisine";
import type { Restaurant } from "../types/Restaurant";
import type { UpdateRestaurant } from "../types/UpdateRestaurant";
import type { RestaurantImage } from "../types/RestaurantImage";

function OwnerRestaurantPage() {

    const { notify } = useNotification();

    const [searchParams] = useSearchParams();

    const restaurantIdParam =
        Number(searchParams.get("restaurantId")) || null;

    const [restaurantId, setRestaurantId] = useState(0);

    const [selectedRestaurant, setSelectedRestaurant] =
        useState<Restaurant | null>(null);

    const [loading, setLoading] =
        useState(true);

    const [logoFile, setLogoFile] = useState<File | null>(null);
    const [coverFile, setCoverFile] = useState<File | null>(null);
    const [menuFile, setMenuFile] = useState<File | null>(null);

    const [galleryImages, setGalleryImages] = useState<RestaurantImage[]>([]);
    const [galleryLoading, setGalleryLoading] = useState(true);
    const [galleryFile, setGalleryFile] = useState<File | null>(null);
    const [uploading, setUploading] = useState(false);
    const [deleteTargetId, setDeleteTargetId] = useState<number | null>(null);

    // Area and Cuisine choices, and the save-in-progress flag.
    const [areas, setAreas] = useState<Area[]>([]);
    const [cuisines, setCuisines] = useState<Cuisine[]>([]);
    const [saving, setSaving] = useState(false);
    const savingRef = useRef(false);

    const [restaurant, setRestaurant] = useState<UpdateRestaurant>({
        name: "",
        description: "",
        address: "",
        phoneNumber: "",
        email: "",
        website: "",
        areaId: 0,
        currencyCode: DEFAULT_CURRENCY_CODE,
        isActive: true,
        cuisineIds: []
    });

    useEffect(() => {

        loadRestaurant();

    }, [restaurantIdParam]);

    // Area and Cuisine choices are the same for every restaurant: load once.
    useEffect(() => {

        let ignore = false;

        Promise.all([AreaService.getAll(), CuisineService.getAll()])
            .then(([areaData, cuisineData]) => {

                if (ignore)
                    return;

                setAreas(areaData);
                setCuisines(cuisineData);

            })
            .catch(error => {

                console.error(error);

                notify(
                    getApiErrorMessage(error, "Failed to load areas and cuisines. Please try again."),
                    "error"
                );

            });

        return () => {
            ignore = true;
        };

    }, [notify]);

    async function loadRestaurant() {

        if (!restaurantIdParam) {

            setSelectedRestaurant(null);
            setLoading(false);

            return;
        }

        try {

            const restaurants =
                await OwnerRestaurantService.getMyRestaurants();

            const data =
                restaurants.find(
                    restaurant =>
                        restaurant.id === restaurantIdParam
                );

            if (!data) {

                setSelectedRestaurant(null);

                return;
            }

            setSelectedRestaurant(data);

            setRestaurantId(data.id);

            setRestaurant({
                name: data.name,
                description: data.description,
                address: data.address,
                phoneNumber: data.phoneNumber,
                email: data.email,
                website: data.website,
                areaId: data.areaId,
                currencyCode: data.currencyCode,
                isActive: data.isActive,
                // Current cuisines, preselected (empty if none assigned yet).
                cuisineIds: data.cuisineIds ?? []
            });

            loadGallery(data.id);

        }
        catch (error) {

            console.error(error);

        }
        finally {

            setLoading(false);

        }

    }

    async function save() {

        // A ref (not just state) so two quick clicks can't both start a
        // save before React re-renders the disabled button.
        if (savingRef.current)
            return;

        if (!restaurant.areaId) {

            notify("Please select an area.", "warning");

            return;
        }

        if (!restaurant.cuisineIds || restaurant.cuisineIds.length === 0) {

            notify("Please select at least one cuisine.", "warning");

            return;
        }

        savingRef.current = true;
        setSaving(true);

        try {

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

            notify("Restaurant updated successfully.", "success");

            loadRestaurant();

        }
        catch (error) {

            console.error(error);

            // The API's validation message, e.g. "Select at least one cuisine."
            notify(
                getApiErrorMessage(error, "Failed to save the restaurant. Please try again."),
                "error"
            );

        }
        finally {

            savingRef.current = false;
            setSaving(false);

        }

    }

    async function loadGallery(id: number) {

        try {

            const data = await RestaurantImageService.getByRestaurant(id);

            setGalleryImages(data);

        }
        catch (error: any) {

            console.error(error);

            notify(
                error.response?.data?.Message ??
                error.message ??
                "Failed to load restaurant gallery. Please try again.",
                "error"
            );

        }
        finally {

            setGalleryLoading(false);

        }

    }

    async function handleGalleryUpload() {

        if (!galleryFile)
            return;

        setUploading(true);

        try {

            await RestaurantImageService.upload(restaurantId, galleryFile);

            notify("Image uploaded successfully.", "success");

            setGalleryFile(null);

            await loadGallery(restaurantId);

        }
        catch (error: any) {

            console.error(error);

            notify(
                error.response?.data?.Message ??
                error.message ??
                "Failed to upload image. Please try again.",
                "error"
            );

        }
        finally {

            setUploading(false);

        }

    }

    function handleDeleteImageClick(id: number) {

        setDeleteTargetId(id);

    }

    function handleCloseDeleteDialog() {

        setDeleteTargetId(null);

    }

    async function handleConfirmDeleteImage() {

        if (deleteTargetId === null)
            return;

        try {

            await RestaurantImageService.delete(deleteTargetId);

            notify("Image deleted successfully.", "success");

            await loadGallery(restaurantId);

        }
        catch (error: any) {

            console.error(error);

            notify(
                error.response?.data?.Message ??
                error.message ??
                "Failed to delete image. Please try again.",
                "error"
            );

        }
        finally {

            setDeleteTargetId(null);

        }

    }

    if (loading)
        return <CircularProgress />;

    if (!restaurantIdParam || !selectedRestaurant) {

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
                    Restaurant Profile
                </Typography>

                <Typography sx={{ mb: 3 }}>

                    Restaurant:
                    {" "}
                    {selectedRestaurant.name}

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
                        select
                        label="Area"
                        required
                        value={areas.length > 0 && restaurant.areaId ? restaurant.areaId : ""}
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

                    <CuisineMultiSelect
                        cuisines={cuisines}
                        value={restaurant.cuisineIds ?? []}
                        onChange={(cuisineIds) =>
                            setRestaurant({
                                ...restaurant,
                                cuisineIds
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

                    <TextField
                        select
                        label="Currency"
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

                    <Typography variant="h6">
                        Logo
                    </Typography>

                    {selectedRestaurant.logoUrl && (

                        <img
                            src={getImageUrl(selectedRestaurant.logoUrl)}
                            alt="Current logo"
                            style={{ maxWidth: 200, maxHeight: 200 }}
                        />

                    )}

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

                    {selectedRestaurant.coverImageUrl && (

                        <img
                            src={getImageUrl(selectedRestaurant.coverImageUrl)}
                            alt="Current cover image"
                            style={{ maxWidth: 400, maxHeight: 200 }}
                        />

                    )}

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

                    {selectedRestaurant.menuPdfUrl && (

                        <Typography>

                            <a
                                href={getImageUrl(selectedRestaurant.menuPdfUrl)}
                                target="_blank"
                                rel="noopener noreferrer"
                            >
                                View current menu PDF
                            </a>

                        </Typography>

                    )}

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
                        disabled={saving}
                        onClick={save}
                    >
                        {saving ? "Saving..." : "Save Restaurant"}
                    </Button>

                </Stack>

            </Paper>

            <Paper sx={{ p: 4, mt: 4 }}>

                <Typography
                    variant="h4"
                    sx={{ mb: 3 }}
                >
                    Restaurant Gallery
                </Typography>

                {galleryLoading ? (

                    <CircularProgress size={24} />

                ) : galleryImages.length === 0 ? (

                    <Typography
                        color="text.secondary"
                        sx={{ mb: 3 }}
                    >
                        No gallery images yet.
                    </Typography>

                ) : (

                    <Grid
                        container
                        spacing={2}
                        sx={{ mb: 3 }}
                    >

                        {galleryImages.map(image => (

                            <Grid
                                key={image.id}
                                size={{ xs: 12, sm: 6, md: 4 }}
                            >

                                <Card>

                                    <CardMedia
                                        component="img"
                                        height="150"
                                        image={getImageUrl(image.imageUrl)}
                                        alt={image.caption ?? selectedRestaurant.name}
                                    />

                                    <Button
                                        fullWidth
                                        color="error"
                                        onClick={() => handleDeleteImageClick(image.id)}
                                    >
                                        Delete
                                    </Button>

                                </Card>

                            </Grid>

                        ))}

                    </Grid>

                )}

                <Stack
                    direction="row"
                    spacing={2}
                    useFlexGap
                    alignItems="center"
                    sx={{ flexWrap: "wrap" }}
                >

                    <input
                        type="file"
                        accept="image/*"
                        onChange={(e) =>
                            setGalleryFile(
                                e.target.files?.[0] ?? null
                            )
                        }
                    />

                    <Button
                        variant="contained"
                        disabled={!galleryFile || uploading}
                        onClick={handleGalleryUpload}
                    >
                        {uploading ? "Uploading..." : "Upload Image"}
                    </Button>

                </Stack>

            </Paper>

            <Dialog
                open={deleteTargetId !== null}
                onClose={handleCloseDeleteDialog}
            >

                <DialogTitle>
                    Delete Image
                </DialogTitle>

                <DialogContent>

                    <DialogContentText>
                        Are you sure you want to delete this image? This cannot be undone.
                    </DialogContentText>

                </DialogContent>

                <DialogActions>

                    <Button onClick={handleCloseDeleteDialog}>
                        Keep Image
                    </Button>

                    <Button
                        color="error"
                        variant="contained"
                        onClick={handleConfirmDeleteImage}
                    >
                        Delete Image
                    </Button>

                </DialogActions>

            </Dialog>

        </Container>

    );

}

export default OwnerRestaurantPage;