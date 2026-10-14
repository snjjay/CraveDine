// ==========================================================
// Image uploads (menu items, Surprise Bags)
// ==========================================================
//
// Checked in the browser before uploading, with the same limit and types
// as the API (FileStorageService.SaveValidatedImageAsync; the services'
// MaxImageBytes). The API still checks the file's content itself.
// ==========================================================

export const MAX_IMAGE_BYTES = 5 * 1024 * 1024;

export const IMAGE_ACCEPT = ".jpg,.jpeg,.png,.webp,image/jpeg,image/png,image/webp";

const IMAGE_TYPES = ["image/jpeg", "image/png", "image/webp"];
const IMAGE_EXTENSION = /\.(jpe?g|png|webp)$/i;

// A message describing why the file can't be used, or null when it's fine.
export function validateImage(file: File): string | null {

    if (!IMAGE_EXTENSION.test(file.name) || (file.type && !IMAGE_TYPES.includes(file.type)))
        return "Please choose a JPG, PNG or WEBP image.";

    if (file.size === 0)
        return "The selected image is empty.";

    if (file.size > MAX_IMAGE_BYTES)
        return "Images must be 5 MB or smaller.";

    return null;

}
