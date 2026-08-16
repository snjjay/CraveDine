export function getImageUrl(path?: string): string {

    if (!path) {
        return "https://placehold.co/1200x400?text=EatKath";
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
// → Use a default placeholder image.
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