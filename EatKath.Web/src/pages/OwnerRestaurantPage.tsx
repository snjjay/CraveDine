import { useEffect, useState } from "react";
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
    Paper,
    Stack,
    TextField,
    Typography
} from "@mui/material";

import OwnerRestaurantService from "../services/OwnerRestaurantService";
import RestaurantImageService from "../services/RestaurantImageService";
import { getImageUrl } from "../utils/imageUrl";
import { useNotification } from "../features/notifications/NotificationContext";

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

    }, [restaurantIdParam]);

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
                isActive: data.isActive
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
                        onClick={save}
                    >
                        Save Restaurant
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
                    alignItems="center"
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