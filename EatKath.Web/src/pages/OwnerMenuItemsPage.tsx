import { useEffect, useMemo, useRef, useState } from "react";
import type { ChangeEvent } from "react";
import { useSearchParams } from "react-router-dom";

import {
    Box,
    Button,
    Checkbox,
    CircularProgress,
    Container,
    Dialog,
    DialogActions,
    DialogContent,
    DialogContentText,
    DialogTitle,
    FormControlLabel,
    LinearProgress,
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
import { formatCurrency } from "../utils/currency";
import { getApiErrorMessage } from "../utils/apiError";
import { getImageUrl } from "../utils/imageUrl";

import type { Restaurant } from "../types/Restaurant";
import type { MenuCategory } from "../types/MenuCategory";
import type { MenuItem } from "../types/MenuItem";

// Same limit and types as the API (MenuItemService.MaxImageBytes).
const MAX_IMAGE_BYTES = 5 * 1024 * 1024;
const IMAGE_ACCEPT = ".jpg,.jpeg,.png,.webp,image/jpeg,image/png,image/webp";
const IMAGE_TYPES = ["image/jpeg", "image/png", "image/webp"];
const IMAGE_EXTENSION = /\.(jpe?g|png|webp)$/i;

// A message describing why the file can't be used, or null when it's fine.
function validateImage(file: File): string | null {

    if (!IMAGE_EXTENSION.test(file.name) || (file.type && !IMAGE_TYPES.includes(file.type)))
        return "Please choose a JPG, PNG or WEBP image.";

    if (file.size === 0)
        return "The selected image is empty.";

    if (file.size > MAX_IMAGE_BYTES)
        return "Images must be 5 MB or smaller.";

    return null;

}

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

    const [saving, setSaving] =
        useState(false);

    // Image of the item being edited (saved immediately on change).
    const [editingImageUrl, setEditingImageUrl] =
        useState<string | null>(null);

    // Image chosen for a new item, uploaded once the item is added.
    const [pendingImage, setPendingImage] =
        useState<File | null>(null);

    const [imageAction, setImageAction] =
        useState<"upload" | "remove" | null>(null);

    const [uploadProgress, setUploadProgress] =
        useState(0);

    const [removeDialogOpen, setRemoveDialogOpen] =
        useState(false);

    // Synchronous guards against double clicks (state updates are async).
    const savingRef = useRef(false);
    const imageBusyRef = useRef(false);

    const imageBusy = imageAction !== null;

    const pendingPreview = useMemo(
        () => pendingImage ? URL.createObjectURL(pendingImage) : null,
        [pendingImage]
    );

    useEffect(() => {

        return () => {
            if (pendingPreview)
                URL.revokeObjectURL(pendingPreview);
        };

    }, [pendingPreview]);

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

    function resetForm() {

        setEditingId(null);

        setMenuCategoryId(0);

        setName("");

        setDescription("");

        setPrice(0);

        setIsFeatured(false);

        setIsAvailable(true);

        setEditingImageUrl(null);

        setPendingImage(null);

    }

    function startEditing(item: MenuItem) {

        setEditingId(item.id);

        setMenuCategoryId(item.menuCategoryId);

        setName(item.name);

        setDescription(item.description);

        setPrice(item.price);

        setIsFeatured(item.isFeatured);

        setIsAvailable(item.isAvailable);

        setEditingImageUrl(item.imageUrl || null);

        setPendingImage(null);

    }

    // Uploads an image for a saved item. Returns the new path, or null
    // after showing the error.
    async function uploadImage(itemId: number, file: File): Promise<string | null> {

        imageBusyRef.current = true;

        setImageAction("upload");

        setUploadProgress(0);

        try {

            const imageUrl =
                await MenuItemService.uploadImage(itemId, file, setUploadProgress);

            setItems(current =>
                current.map(item =>
                    item.id === itemId ? { ...item, imageUrl } : item
                )
            );

            return imageUrl;

        }
        catch (error) {

            console.error(error);

            notify(
                getApiErrorMessage(error, "Failed to upload the image. Please try again."),
                "error"
            );

            return null;

        }
        finally {

            imageBusyRef.current = false;

            setImageAction(null);

        }

    }


    async function saveMenuItem() {

        if (!restaurant || savingRef.current || imageBusyRef.current)
            return;

        savingRef.current = true;

        setSaving(true);

        try {

            if (editingId === null) {

                const created = await MenuItemService.create({

                    restaurantId: restaurant.id,

                    menuCategoryId,

                    name,

                    description,

                    price,

                    isFeatured,

                    isAvailable

                });

                // The upload needs the new item's id, so it happens after
                // the item is saved.
                if (pendingImage) {

                    const imageUrl = await uploadImage(created.id, pendingImage);

                    if (!imageUrl) {

                        // The item exists; keep it open so the owner can
                        // try the image again.
                        notify("Menu item added, but its image was not uploaded. You can try again below.", "warning");

                        setEditingId(created.id);

                        setEditingImageUrl(null);

                        setPendingImage(null);

                        await loadData();

                        return;
                    }

                    notify("Menu item added with its image.", "success");

                }
                else {

                    notify("Menu item added.", "success");

                }

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

                notify("Menu item updated.", "success");

            }

            resetForm();

            await loadData();

        }
        catch (error) {

            console.error(error);

            notify(
                getApiErrorMessage(error, "Something went wrong. Please try again."),
                "error"
            );

        }
        finally {

            savingRef.current = false;

            setSaving(false);

        }

    }


    async function handleImageSelected(e: ChangeEvent<HTMLInputElement>) {

        const file = e.target.files?.[0];

        // Allows choosing the same file again after an error.
        e.target.value = "";

        if (!file || imageBusyRef.current)
            return;

        const problem = validateImage(file);

        if (problem) {

            notify(problem, "error");

            return;
        }

        if (editingId === null) {

            setPendingImage(file);

            return;
        }

        const replacing = !!editingImageUrl;

        const imageUrl = await uploadImage(editingId, file);

        if (imageUrl) {

            setEditingImageUrl(imageUrl);

            notify(replacing ? "Image replaced." : "Image uploaded.", "success");

        }

    }


    async function removeImage() {

        if (editingId === null || imageBusyRef.current)
            return;

        const itemId = editingId;

        imageBusyRef.current = true;

        setImageAction("remove");

        try {

            await MenuItemService.deleteImage(itemId);

            setEditingImageUrl(null);

            setItems(current =>
                current.map(item =>
                    item.id === itemId ? { ...item, imageUrl: undefined } : item
                )
            );

            setRemoveDialogOpen(false);

            notify("Image removed.", "success");

        }
        catch (error) {

            console.error(error);

            notify(
                getApiErrorMessage(error, "Failed to remove the image. Please try again."),
                "error"
            );

        }
        finally {

            imageBusyRef.current = false;

            setImageAction(null);

        }

    }


    async function deleteMenuItem(id: number) {

        if (!confirm("Delete this menu item?"))
            return;

        try {

            await MenuItemService.delete(id);

            if (editingId === id)
                resetForm();

            await loadData();

        }
        catch (error) {

            console.error(error);

            notify(
                getApiErrorMessage(error, "Something went wrong. Please try again."),
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

    const previewSrc =
        editingId === null
            ? pendingPreview
            : editingImageUrl ? getImageUrl(editingImageUrl) : null;

    const chooseLabel =
        editingId === null
            ? (pendingImage ? "Change Image" : "Choose Image")
            : (editingImageUrl ? "Replace Image" : "Upload Image");

    return (

        <Container maxWidth="md">

            <Paper sx={{ p: { xs: 2, sm: 4 }, mt: 4 }}>

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

                    <Box component="section" aria-labelledby="menu-item-image-heading">

                        <Typography
                            id="menu-item-image-heading"
                            variant="h6"
                            sx={{ mb: 1 }}
                        >
                            Image
                        </Typography>

                        <Stack
                            direction={{ xs: "column", sm: "row" }}
                            spacing={2}
                            alignItems={{ xs: "flex-start", sm: "center" }}
                        >

                            <Box
                                sx={{
                                    width: 120,
                                    height: 120,
                                    flexShrink: 0,
                                    borderRadius: 2,
                                    border: "1px solid",
                                    borderColor: "divider",
                                    bgcolor: "action.hover",
                                    overflow: "hidden",
                                    display: "flex",
                                    alignItems: "center",
                                    justifyContent: "center"
                                }}
                            >

                                {previewSrc ? (

                                    <Box
                                        component="img"
                                        src={previewSrc}
                                        alt={name ? `Image of ${name}` : "Menu item image"}
                                        sx={{ width: "100%", height: "100%", objectFit: "cover" }}
                                    />

                                ) : (

                                    <Typography variant="caption" color="text.secondary">
                                        No image
                                    </Typography>

                                )}

                            </Box>

                            <Stack spacing={1} sx={{ minWidth: 0 }}>

                                <Stack direction="row" spacing={1} useFlexGap sx={{ flexWrap: "wrap" }}>

                                    <Button
                                        variant="outlined"
                                        component="label"
                                        disabled={imageBusy || saving}
                                    >
                                        {chooseLabel}
                                        <input
                                            hidden
                                            type="file"
                                            accept={IMAGE_ACCEPT}
                                            onChange={handleImageSelected}
                                        />
                                    </Button>

                                    {editingId !== null && editingImageUrl && (

                                        <Button
                                            variant="outlined"
                                            color="error"
                                            disabled={imageBusy || saving}
                                            onClick={() => setRemoveDialogOpen(true)}
                                        >
                                            Remove Image
                                        </Button>

                                    )}

                                    {editingId === null && pendingImage && (

                                        <Button
                                            disabled={saving}
                                            onClick={() => setPendingImage(null)}
                                        >
                                            Clear
                                        </Button>

                                    )}

                                </Stack>

                                <Typography variant="caption" color="text.secondary">

                                    {editingId === null
                                        ? "JPG, PNG or WEBP, up to 5 MB. Uploaded when you add the item."
                                        : "JPG, PNG or WEBP, up to 5 MB. Image changes are saved straight away."}

                                </Typography>

                                {imageAction === "upload" && (

                                    <Box sx={{ width: 220, maxWidth: "100%" }} role="status">

                                        <LinearProgress
                                            variant="determinate"
                                            value={uploadProgress}
                                            aria-label="Image upload progress"
                                        />

                                        <Typography variant="caption" color="text.secondary">
                                            Uploading image... {uploadProgress}%
                                        </Typography>

                                    </Box>

                                )}

                                {imageAction === "remove" && (

                                    <Typography variant="caption" color="text.secondary" role="status">
                                        Removing image...
                                    </Typography>

                                )}

                            </Stack>

                        </Stack>

                    </Box>


                    <Stack direction="row" spacing={1}>

                        <Button
                            variant="contained"
                            disabled={saving || imageBusy}
                            onClick={saveMenuItem}
                            sx={{ flex: 1 }}
                        >
                            {saving
                                ? (imageAction === "upload" ? "Uploading image..." : "Saving...")
                                : editingId === null
                                    ? "Add Menu Item"
                                    : "Update Menu Item"}
                        </Button>

                        {editingId !== null && (

                            <Button
                                disabled={saving || imageBusy}
                                onClick={resetForm}
                            >
                                Cancel
                            </Button>

                        )}

                    </Stack>

                    {items.map(item => (

                        <Stack
                            key={item.id}
                            direction="row"
                            spacing={2}
                            alignItems="center"
                        >

                            {item.imageUrl ? (

                                <Box
                                    component="img"
                                    src={getImageUrl(item.imageUrl)}
                                    alt=""
                                    sx={{ width: 40, height: 40, objectFit: "cover", borderRadius: 1, flexShrink: 0 }}
                                />

                            ) : (

                                <Box
                                    aria-hidden
                                    sx={{ width: 40, height: 40, borderRadius: 1, flexShrink: 0, bgcolor: "action.hover" }}
                                />

                            )}

                            <Typography sx={{ flex: 1, minWidth: 0 }}>

                                {item.name} - {formatCurrency(item.price, restaurant?.currencyCode)}

                            </Typography>

                            <Button
                                size="small"
                                variant="outlined"
                                disabled={saving || imageBusy}
                                onClick={() => startEditing(item)}
                            >
                                Edit
                            </Button>

                            <Button
                                size="small"
                                color="error"
                                variant="outlined"
                                disabled={saving || imageBusy}
                                onClick={() => deleteMenuItem(item.id)}
                            >
                                Delete
                            </Button>

                        </Stack>

                    ))}

                </Stack>

            </Paper>

            <Dialog
                open={removeDialogOpen}
                onClose={() => !imageBusy && setRemoveDialogOpen(false)}
            >

                <DialogTitle>
                    Remove Image
                </DialogTitle>

                <DialogContent>

                    <DialogContentText>
                        Remove this menu item's image? Customers will see the item without a photo.
                    </DialogContentText>

                </DialogContent>

                <DialogActions>

                    <Button
                        disabled={imageBusy}
                        onClick={() => setRemoveDialogOpen(false)}
                    >
                        Cancel
                    </Button>

                    <Button
                        color="error"
                        disabled={imageBusy}
                        onClick={removeImage}
                    >
                        {imageAction === "remove" ? "Removing..." : "Remove"}
                    </Button>

                </DialogActions>

            </Dialog>

        </Container>

    );

}

export default OwnerMenuItemsPage;
