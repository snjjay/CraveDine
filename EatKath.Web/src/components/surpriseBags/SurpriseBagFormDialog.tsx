// SurpriseBagFormDialog = create or edit one of the owner's Surprise Bags.
//
// The checks here mirror the API's validator (SurpriseBagInputValidator)
// so most mistakes are caught before saving; the API stays the authority
// and its messages are shown as-is (e.g. a pickup window that has already
// ended - Kathmandu time - or a max per customer below what one customer
// already holds, which the list doesn't show).
//
// The parent mounts this dialog only while it is open, so every opening
// starts with a fresh form.

import { useEffect, useMemo, useRef, useState, type ChangeEvent } from "react";
import axios from "axios";

import {
    Alert,
    Box,
    Button,
    Dialog,
    DialogActions,
    DialogContent,
    DialogTitle,
    LinearProgress,
    MenuItem,
    Stack,
    TextField,
    Typography
} from "@mui/material";

import ShoppingBagOutlinedIcon from "@mui/icons-material/ShoppingBagOutlined";

import OwnerSurpriseBagService from "../../services/OwnerSurpriseBagService";
import { getApiErrorMessage } from "../../utils/apiError";
import { IMAGE_ACCEPT, validateImage } from "../../utils/imageUpload";
import { getImageUrl } from "../../utils/imageUrl";
import type {
    OwnerSurpriseBag,
    SurpriseBagInput
} from "../../types/OwnerSurpriseBag";
import {
    SurpriseBagCategory,
    getSurpriseBagCategoryLabel
} from "../../utils/surpriseBag";

// Same limits as the API (SurpriseBagInputValidator).
const MAX_TITLE_LENGTH = 150;
const MAX_TEXT_LENGTH = 1000;
const MAX_QUANTITY_TOTAL = 1000;

// decimal(18,2): up to 16 whole digits and 2 decimals, not negative.
const MONEY_PATTERN = /^\d{1,16}(\.\d{1,2})?$/;

const WHOLE_NUMBER_PATTERN = /^\d+$/;

const CATEGORY_OPTIONS = Object.values(SurpriseBagCategory);

// Form values as typed (numbers stay strings until saved).
interface FormValues {
    title: string;
    category: number;
    description: string;
    conditions: string;
    price: string;
    estimatedValue: string;
    quantityTotal: string;
    maxPerCustomer: string;
    pickupDate: string;
    // "HH:mm" for the time inputs.
    pickupStartTime: string;
    pickupEndTime: string;
}

type FormErrors = Partial<Record<keyof FormValues, string>>;

function toFormValues(bag: OwnerSurpriseBag | null): FormValues {

    if (!bag) {

        return {
            title: "",
            category: SurpriseBagCategory.Meals,
            description: "",
            conditions: "",
            price: "",
            estimatedValue: "",
            // No preset: the owner enters how many bags they offer.
            quantityTotal: "",
            maxPerCustomer: "1",
            pickupDate: "",
            pickupStartTime: "18:00",
            pickupEndTime: "20:00"
        };

    }

    return {
        title: bag.title,
        category: bag.category,
        description: bag.description,
        conditions: bag.conditions,
        price: String(bag.price),
        estimatedValue: bag.estimatedValue != null ? String(bag.estimatedValue) : "",
        quantityTotal: String(bag.quantityTotal),
        maxPerCustomer: String(bag.maxPerCustomer),
        pickupDate: bag.pickupDate,
        pickupStartTime: bag.pickupStartTime.slice(0, 5),
        pickupEndTime: bag.pickupEndTime.slice(0, 5)
    };

}

// "18:00" -> "18:00:00" (the API's TimeOnly format, as for deals).
function toApiTime(time: string): string {

    return time.length === 5 ? `${time}:00` : time;

}

function validate(values: FormValues, usedQuantity: number): FormErrors {

    const errors: FormErrors = {};

    const title = values.title.trim();

    if (!title)
        errors.title = "Enter a title.";
    else if (title.length > MAX_TITLE_LENGTH)
        errors.title = `Use ${MAX_TITLE_LENGTH} characters or fewer.`;

    if (values.description.trim().length > MAX_TEXT_LENGTH)
        errors.description = `Use ${MAX_TEXT_LENGTH} characters or fewer.`;

    if (values.conditions.trim().length > MAX_TEXT_LENGTH)
        errors.conditions = `Use ${MAX_TEXT_LENGTH} characters or fewer.`;

    const price = values.price.trim();

    if (!MONEY_PATTERN.test(price))
        errors.price = "Enter a price of 0 or more, with up to 2 decimal places.";

    const estimatedValue = values.estimatedValue.trim();

    if (estimatedValue) {

        if (!MONEY_PATTERN.test(estimatedValue))
            errors.estimatedValue = "Enter an amount with up to 2 decimal places, or leave it empty.";
        else if (!errors.price && Number(estimatedValue) <= Number(price))
            errors.estimatedValue = "Estimated value must be greater than the price.";

    }

    const quantityTotal = Number(values.quantityTotal);
    const quantityIsValid =
        WHOLE_NUMBER_PATTERN.test(values.quantityTotal.trim()) &&
        quantityTotal >= 1 &&
        quantityTotal <= MAX_QUANTITY_TOTAL;

    if (!quantityIsValid)
        errors.quantityTotal = `Enter a whole number from 1 to ${MAX_QUANTITY_TOTAL}.`;
    else if (quantityTotal < usedQuantity)
        errors.quantityTotal = `Can't be less than the ${usedQuantity} bags already reserved or collected.`;

    const maxPerCustomer = Number(values.maxPerCustomer);

    if (!WHOLE_NUMBER_PATTERN.test(values.maxPerCustomer.trim()) || maxPerCustomer < 1)
        errors.maxPerCustomer = "Enter a whole number of 1 or more.";
    else if (quantityIsValid && maxPerCustomer > quantityTotal)
        errors.maxPerCustomer = "Can't be more than the total quantity.";

    if (!values.pickupDate)
        errors.pickupDate = "Choose a pickup date.";

    if (!values.pickupStartTime)
        errors.pickupStartTime = "Choose a start time.";

    if (!values.pickupEndTime)
        errors.pickupEndTime = "Choose an end time.";
    else if (values.pickupStartTime && values.pickupEndTime <= values.pickupStartTime)
        errors.pickupEndTime = "Must be later than the start time (same day).";

    return errors;

}

// A time left as it was is sent exactly as the API sent it (with its
// seconds), so an unchanged pickup window never counts as a change.
function toApiTimeKeeping(time: string, original: string | undefined): string {

    return original && original.slice(0, 5) === time ? original : toApiTime(time);

}

function toInput(values: FormValues, bag: OwnerSurpriseBag | null): SurpriseBagInput {

    const estimatedValue = values.estimatedValue.trim();

    return {
        title: values.title.trim(),
        category: values.category,
        description: values.description.trim(),
        conditions: values.conditions.trim(),
        price: Number(values.price.trim()),
        estimatedValue: estimatedValue ? Number(estimatedValue) : null,
        quantityTotal: Number(values.quantityTotal.trim()),
        maxPerCustomer: Number(values.maxPerCustomer.trim()),
        pickupDate: values.pickupDate,
        pickupStartTime: toApiTimeKeeping(values.pickupStartTime, bag?.pickupStartTime),
        pickupEndTime: toApiTimeKeeping(values.pickupEndTime, bag?.pickupEndTime)
    };

}

// API validation errors arrive joined with " | " (ExceptionMiddleware);
// shown one per line. No response: the save may still have happened.
function getSaveErrorMessages(error: unknown): string[] {

    if (axios.isAxiosError(error)) {

        if (!error.response)
            return ["We couldn't confirm the save because the connection was interrupted. Check the list before trying again."];

        const apiMessage = (error.response.data as { Message?: string } | undefined)?.Message;

        if (error.response.status === 404)
            return [apiMessage || "This Surprise Bag could not be found."];

        if (error.response.status < 500 && apiMessage)
            return apiMessage.split(" | ");

    }

    return ["Unable to save this Surprise Bag. Please try again."];

}

// Image upload/removal errors. No response: the change may still have
// happened (the list is reloaded when the form closes).
function getImageErrorMessage(error: unknown, fallback: string): string {

    if (axios.isAxiosError(error) && !error.response)
        return "We couldn't confirm the image change because the connection was interrupted. Check the bag before trying again.";

    return getApiErrorMessage(error, fallback);

}

interface Props {
    open: boolean;
    onClose: () => void;
    // Called with the saved bag after a successful create or update;
    // warning is set when a new bag was created but its image wasn't
    // uploaded.
    onSaved: (bag: OwnerSurpriseBag, warning?: string) => void;
    // Restaurant a new bag is created for.
    restaurantId: number;
    // Bag to edit; null creates a new bag.
    bag: OwnerSurpriseBag | null;
    // Restaurant currency, shown on the amount fields.
    currencyCode: string;
}

function SurpriseBagFormDialog({
    open,
    onClose,
    onSaved,
    restaurantId,
    bag,
    currencyCode
}: Props) {

    const isEdit = bag !== null;

    // Bags already reserved or collected: the total can't go below this.
    const usedQuantity = bag ? bag.reservedQuantity + bag.collectedQuantity : 0;

    const pickupLocked = bag?.isPickupLocked ?? false;

    const [values, setValues] = useState<FormValues>(() => toFormValues(bag));
    const [showErrors, setShowErrors] = useState(false);
    const [saving, setSaving] = useState(false);
    const [apiErrors, setApiErrors] = useState<string[]>([]);

    // Blocks a second request before the disabled button re-renders.
    const savingRef = useRef(false);

    // ---------- Optional image (cosmetic: also for inactive and
    // pickup-locked bags) ----------

    // Saved image of the bag being edited; changed straight away.
    const [imageUrl, setImageUrl] = useState<string | null>(bag?.imageUrl ?? null);

    // Image chosen for a new bag, uploaded once the bag is created.
    const [pendingImage, setPendingImage] = useState<File | null>(null);

    const [imageAction, setImageAction] = useState<"upload" | "remove" | null>(null);
    const [uploadProgress, setUploadProgress] = useState(0);
    const [imageError, setImageError] = useState<string | null>(null);
    const [confirmRemove, setConfirmRemove] = useState(false);

    // Saved image that failed to load (shows the fallback instead).
    const [failedImageSrc, setFailedImageSrc] = useState<string | null>(null);

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

    const savedImageSrc = imageUrl ? getImageUrl(imageUrl) : null;

    const previewSrc = isEdit
        ? (savedImageSrc && savedImageSrc !== failedImageSrc ? savedImageSrc : null)
        : pendingPreview;

    const errors = validate(values, usedQuantity);
    const hasErrors = Object.keys(errors).length > 0;

    function fieldError(field: keyof FormValues): string | undefined {

        return showErrors ? errors[field] : undefined;

    }

    function setValue<K extends keyof FormValues>(field: K, value: FormValues[K]) {

        setValues(current => ({ ...current, [field]: value }));

    }

    async function handleSave() {

        if (savingRef.current)
            return;

        setShowErrors(true);

        if (hasErrors)
            return;

        savingRef.current = true;
        setSaving(true);
        setApiErrors([]);

        try {

            const input = toInput(values, bag);

            // Edits leave isActive out, so the bag keeps its active state.
            if (bag) {
                onSaved(await OwnerSurpriseBagService.update(bag.id, input));
                return;
            }

            const created = await OwnerSurpriseBagService.create({ ...input, restaurantId });

            // The upload needs the new bag's id, so it happens after the
            // bag is created. A failed upload doesn't undo the bag.
            if (pendingImage) {

                setImageAction("upload");
                setUploadProgress(0);

                try {

                    const uploaded = await OwnerSurpriseBagService.uploadImage(created.id, pendingImage, setUploadProgress);

                    onSaved({ ...created, imageUrl: uploaded });

                }
                catch (error) {

                    console.error(error);

                    onSaved(
                        created,
                        `"${created.title}" was created, but its image wasn't uploaded: ${getImageErrorMessage(error, "please try again.")} Open Edit to add it.`
                    );

                }
                finally {

                    setImageAction(null);

                }

                return;

            }

            onSaved(created);

        }
        catch (error) {

            console.error(error);

            setApiErrors(getSaveErrorMessages(error));

        }
        finally {

            savingRef.current = false;
            setSaving(false);

        }

    }

    // Closing (Escape, backdrop) waits for a running save or image change.
    function handleClose() {

        if (!saving && !imageBusy)
            onClose();

    }

    async function handleImageSelected(e: ChangeEvent<HTMLInputElement>) {

        const file = e.target.files?.[0];

        // Allows choosing the same file again after an error.
        e.target.value = "";

        if (!file || imageBusyRef.current)
            return;

        setImageError(null);
        setConfirmRemove(false);

        const problem = validateImage(file);

        if (problem) {
            setImageError(problem);
            return;
        }

        if (!bag) {
            setPendingImage(file);
            return;
        }

        imageBusyRef.current = true;
        setImageAction("upload");
        setUploadProgress(0);

        try {

            setImageUrl(await OwnerSurpriseBagService.uploadImage(bag.id, file, setUploadProgress));

        }
        catch (error) {

            console.error(error);

            setImageError(getImageErrorMessage(error, "Failed to upload the image. Please try again."));

        }
        finally {

            imageBusyRef.current = false;
            setImageAction(null);

        }

    }

    async function handleRemoveImage() {

        if (!bag || imageBusyRef.current)
            return;

        imageBusyRef.current = true;
        setImageAction("remove");
        setImageError(null);

        try {

            await OwnerSurpriseBagService.deleteImage(bag.id);

            setImageUrl(null);
            setConfirmRemove(false);

        }
        catch (error) {

            console.error(error);

            setImageError(getImageErrorMessage(error, "Failed to remove the image. Please try again."));

        }
        finally {

            imageBusyRef.current = false;
            setImageAction(null);

        }

    }

    const chooseImageLabel = isEdit
        ? (imageUrl ? "Replace Image" : "Upload Image")
        : (pendingImage ? "Change Image" : "Choose Image");

    const amountAdornment = currencyCode ? ` (${currencyCode})` : "";

    return (

        <Dialog open={open} onClose={handleClose} fullWidth maxWidth="sm">

            <DialogTitle>
                {isEdit ? "Edit Surprise Bag" : "New Surprise Bag"}
            </DialogTitle>

            <DialogContent>

                <Stack spacing={2} sx={{ mt: 1 }}>

                    {apiErrors.length > 0 && (
                        <Alert severity="error" role="alert">
                            {apiErrors.map((message, index) => (
                                <div key={index}>{message}</div>
                            ))}
                        </Alert>
                    )}

                    {showErrors && hasErrors && (
                        <Alert severity="warning">
                            Please fix the highlighted fields.
                        </Alert>
                    )}

                    <TextField
                        label="Title"
                        value={values.title}
                        onChange={(e) => setValue("title", e.target.value)}
                        error={!!fieldError("title")}
                        helperText={fieldError("title") ?? `${values.title.trim().length}/${MAX_TITLE_LENGTH}`}
                        required
                        disabled={saving}
                    />

                    <TextField
                        select
                        label="Category"
                        value={values.category}
                        onChange={(e) => setValue("category", Number(e.target.value))}
                        disabled={saving}
                    >
                        {CATEGORY_OPTIONS.map(category => (
                            <MenuItem key={category} value={category}>
                                {getSurpriseBagCategoryLabel(category)}
                            </MenuItem>
                        ))}
                    </TextField>

                    <TextField
                        label="Description"
                        multiline
                        minRows={3}
                        value={values.description}
                        onChange={(e) => setValue("description", e.target.value)}
                        error={!!fieldError("description")}
                        helperText={fieldError("description") ?? "What customers might find in the bag."}
                        disabled={saving}
                    />

                    <TextField
                        label="Conditions"
                        multiline
                        minRows={2}
                        value={values.conditions}
                        onChange={(e) => setValue("conditions", e.target.value)}
                        error={!!fieldError("conditions")}
                        helperText={fieldError("conditions") ?? "E.g. allergens, or bring your own bag."}
                        disabled={saving}
                    />

                    {/* Optional image */}
                    <Stack direction={{ xs: "column", sm: "row" }} spacing={2} sx={{ alignItems: { sm: "center" } }}>

                        <Box
                            sx={{
                                width: 128,
                                height: 96,
                                flexShrink: 0,
                                borderRadius: "10px",
                                border: "1px solid",
                                borderColor: "divider",
                                bgcolor: "primarySoft",
                                color: "primary.main",
                                overflow: "hidden",
                                display: "grid",
                                placeItems: "center"
                            }}
                        >
                            {previewSrc ? (
                                <Box
                                    component="img"
                                    src={previewSrc}
                                    alt={values.title.trim() ? `Image of ${values.title.trim()}` : "Surprise Bag image"}
                                    onError={() => setFailedImageSrc(previewSrc)}
                                    sx={{ width: "100%", height: "100%", objectFit: "cover", display: "block" }}
                                />
                            ) : (
                                <ShoppingBagOutlinedIcon aria-hidden sx={{ fontSize: 36 }} />
                            )}
                        </Box>

                        <Stack spacing={1} sx={{ minWidth: 0 }}>

                            <Stack direction="row" spacing={1} useFlexGap sx={{ flexWrap: "wrap" }}>

                                <Button variant="outlined" component="label" disabled={saving || imageBusy}>
                                    {chooseImageLabel}
                                    <input hidden type="file" accept={IMAGE_ACCEPT} onChange={handleImageSelected} />
                                </Button>

                                {isEdit && imageUrl && !confirmRemove && (
                                    <Button color="error" disabled={saving || imageBusy} onClick={() => setConfirmRemove(true)}>
                                        Remove Image
                                    </Button>
                                )}

                                {!isEdit && pendingImage && (
                                    <Button disabled={saving} onClick={() => setPendingImage(null)}>
                                        Clear
                                    </Button>
                                )}

                            </Stack>

                            {confirmRemove && (
                                <Alert
                                    severity="warning"
                                    action={
                                        <Stack direction="row" spacing={1}>
                                            <Button color="inherit" size="small" disabled={imageBusy} onClick={() => setConfirmRemove(false)}>
                                                Keep
                                            </Button>
                                            <Button color="error" size="small" disabled={imageBusy} onClick={handleRemoveImage}>
                                                {imageAction === "remove" ? "Removing..." : "Remove"}
                                            </Button>
                                        </Stack>
                                    }
                                >
                                    Remove this bag's image? The category icon is shown instead.
                                </Alert>
                            )}

                            <Typography variant="caption" color="text.secondary">
                                {isEdit
                                    ? "Optional. JPG, PNG or WEBP, up to 5 MB. Image changes are saved straight away."
                                    : "Optional. JPG, PNG or WEBP, up to 5 MB. Uploaded when you create the bag."}
                            </Typography>

                            {imageAction === "upload" && (
                                <Box sx={{ width: 220, maxWidth: "100%" }} role="status">
                                    <LinearProgress variant="determinate" value={uploadProgress} aria-label="Image upload progress" />
                                    <Typography variant="caption" color="text.secondary">
                                        Uploading image... {uploadProgress}%
                                    </Typography>
                                </Box>
                            )}

                            {imageError && (
                                <Alert severity="error" role="alert" onClose={() => setImageError(null)}>
                                    {imageError}
                                </Alert>
                            )}

                        </Stack>

                    </Stack>

                    <Stack direction={{ xs: "column", sm: "row" }} spacing={2}>

                        <TextField
                            label={`Price${amountAdornment}`}
                            type="number"
                            value={values.price}
                            onChange={(e) => setValue("price", e.target.value)}
                            error={!!fieldError("price")}
                            helperText={fieldError("price") ?? "Paid at pickup."}
                            required
                            disabled={saving}
                            slotProps={{ htmlInput: { min: 0, step: "0.01" } }}
                            sx={{ flex: 1 }}
                        />

                        <TextField
                            label={`Estimated value${amountAdornment}`}
                            type="number"
                            value={values.estimatedValue}
                            onChange={(e) => setValue("estimatedValue", e.target.value)}
                            error={!!fieldError("estimatedValue")}
                            helperText={fieldError("estimatedValue") ?? "Optional. More than the price."}
                            disabled={saving}
                            slotProps={{ htmlInput: { min: 0, step: "0.01" } }}
                            sx={{ flex: 1 }}
                        />

                    </Stack>

                    <Stack direction={{ xs: "column", sm: "row" }} spacing={2}>

                        <TextField
                            label="Total quantity"
                            type="number"
                            value={values.quantityTotal}
                            onChange={(e) => setValue("quantityTotal", e.target.value)}
                            error={!!fieldError("quantityTotal")}
                            helperText={
                                fieldError("quantityTotal") ??
                                (usedQuantity > 0
                                    ? `At least ${usedQuantity} (already reserved or collected).`
                                    : `1 to ${MAX_QUANTITY_TOTAL}.`)
                            }
                            required
                            disabled={saving}
                            slotProps={{ htmlInput: { min: Math.max(1, usedQuantity), max: MAX_QUANTITY_TOTAL, step: 1 } }}
                            sx={{ flex: 1 }}
                        />

                        <TextField
                            label="Max per customer"
                            type="number"
                            value={values.maxPerCustomer}
                            onChange={(e) => setValue("maxPerCustomer", e.target.value)}
                            error={!!fieldError("maxPerCustomer")}
                            helperText={
                                fieldError("maxPerCustomer") ??
                                (usedQuantity > 0
                                    ? "Can't be lower than what one customer already holds."
                                    : "Up to the total quantity.")
                            }
                            required
                            disabled={saving}
                            slotProps={{ htmlInput: { min: 1, step: 1 } }}
                            sx={{ flex: 1 }}
                        />

                    </Stack>

                    {pickupLocked && (
                        <Alert severity="info">
                            The pickup date and times can't be changed because bags have already been reserved or collected.
                        </Alert>
                    )}

                    <TextField
                        label="Pickup date"
                        type="date"
                        value={values.pickupDate}
                        onChange={(e) => setValue("pickupDate", e.target.value)}
                        error={!!fieldError("pickupDate")}
                        helperText={fieldError("pickupDate") ?? "Kathmandu time."}
                        required
                        disabled={saving || pickupLocked}
                        slotProps={{ inputLabel: { shrink: true } }}
                    />

                    <Stack direction={{ xs: "column", sm: "row" }} spacing={2}>

                        <TextField
                            label="Pickup from"
                            type="time"
                            value={values.pickupStartTime}
                            onChange={(e) => setValue("pickupStartTime", e.target.value)}
                            error={!!fieldError("pickupStartTime")}
                            helperText={fieldError("pickupStartTime") ?? " "}
                            required
                            disabled={saving || pickupLocked}
                            slotProps={{ inputLabel: { shrink: true } }}
                            sx={{ flex: 1 }}
                        />

                        <TextField
                            label="Pickup until"
                            type="time"
                            value={values.pickupEndTime}
                            onChange={(e) => setValue("pickupEndTime", e.target.value)}
                            error={!!fieldError("pickupEndTime")}
                            helperText={fieldError("pickupEndTime") ?? " "}
                            required
                            disabled={saving || pickupLocked}
                            slotProps={{ inputLabel: { shrink: true } }}
                            sx={{ flex: 1 }}
                        />

                    </Stack>

                </Stack>

            </DialogContent>

            <DialogActions>

                <Button onClick={onClose} disabled={saving || imageBusy}>
                    Cancel
                </Button>

                <Button
                    variant="contained"
                    onClick={handleSave}
                    disabled={saving || imageBusy}
                >
                    {saving ? "Saving..." : isEdit ? "Save Changes" : "Create Bag"}
                </Button>

            </DialogActions>

        </Dialog>

    );

}

export default SurpriseBagFormDialog;
