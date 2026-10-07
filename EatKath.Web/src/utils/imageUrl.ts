export function getImageUrl(path?: string | null): string {

    // No external placeholder: callers show their own local fallback
    // (e.g. RestaurantImageFallback) when there is no image.
    if (!path) {
        return "";
    }

    if (path.startsWith("http://") || path.startsWith("https://")) {
        return path;
    }

    const apiBase = import.meta.env.VITE_API_URL.replace(/\/api$/, "");

    return `${apiBase}${path}`;
}

// ==========================================================
// STEP 21 — utils/imageUrl.ts
// ==========================================================
//
// getImageUrl() = 🖼️ IMAGE URL HELPER.
//
// Its job:
// → Take an image path
// → Turn it into a complete URL the browser can use.
//
// ----------------------------------------------------------
//
// 1. NO IMAGE:
//
// if (!path)
//
// → No image path was provided.
//
// → Return "" - the component shows its local fallback instead.
//
//
// 2. ALREADY A COMPLETE URL:
//
// http://... or https://...
//
// → The image already has a complete address.
// → Return it as-is.
//
//
// 3. ONLY A PATH:
//
// Example:
//
// /uploads/restaurants/photo.jpg
//
// → Add the API/server address to the path.
//
// API address
//      +
// /uploads/restaurants/photo.jpg
//      ↓
// Complete image URL
//
// ----------------------------------------------------------
//
// EXAMPLE FROM RestaurantCard:
//
// const imageUrl = getImageUrl(restaurant.logoUrl);
//
// → Convert the restaurant's logo path
//   into a URL that the browser can use.
//
// ----------------------------------------------------------
//
// 🔑 REMEMBER:
//
// getImageUrl()
// = "Give me an image path and I'll give you
//    a complete URL to display the image."
//
// ==========================================================

//the next thing I'd check is theme/theme.ts