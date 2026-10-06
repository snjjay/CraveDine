// ==========================================================
// Homepage hero images
// ==========================================================
//
// The hero rotates through approved restaurant photos first, then the
// curated CraveDine collection below (see HeroImageRotator).
// ==========================================================

import cafe from "../assets/hero/cafe.svg";
import chiyaSelRoti from "../assets/hero/chiya-sel-roti.svg";
import dalBhat from "../assets/hero/dal-bhat.svg";
import momo from "../assets/hero/momo.svg";
import pizza from "../assets/hero/pizza.svg";
import ramen from "../assets/hero/ramen.svg";

export interface HeroImage {
    src: string;
    alt: string;
}

// Restaurant photos approved for the homepage hero, as stored paths, e.g.
// "/uploads/restaurants/51/cover.jpeg" or ".../gallery/<file>.jpeg".
//
// Only add food or restaurant photos CraveDine has the rights to feature
// (no watermarks, no unrelated images). A photo is used only while it is
// the cover of an active restaurant (or, for "/uploads/restaurants/{id}/
// gallery/..." paths, while that restaurant is active).
//
// Current entries: covers of the local development demo restaurants
// (licensed Wikimedia Commons photos, see
// EatKath.API/Data/Demo/demo-image-attribution.json). Where the demo data
// isn't present (e.g. production), they match no restaurant and the hero
// falls back to the curated illustrations.
export const APPROVED_RESTAURANT_HERO_PHOTOS: string[] = [
    "/uploads/demo/momo-01.jpg",
    "/uploads/demo/dalbhat-01.jpg",
    "/uploads/demo/tibetan-03.jpg",
    "/uploads/demo/pizza-01.jpg",
    "/uploads/demo/korean-01.jpg",
    "/uploads/demo/japanese-01.jpg",
    "/uploads/demo/southindian-01.jpg",
    "/uploads/demo/thai-01.jpg",
    "/uploads/demo/bakery-01.jpg"
];

// Curated fallback: original illustrations created for CraveDine (not
// photographs of any venue). Replace or extend with approved food
// photography placed in src/assets/hero/.
export const CURATED_HERO_IMAGES: HeroImage[] = [
    { src: momo, alt: "Illustration of momo in a bamboo steamer with achar" },
    { src: dalBhat, alt: "Illustration of a dal bhat thali" },
    { src: chiyaSelRoti, alt: "Illustration of chiya and sel roti" },
    { src: pizza, alt: "Illustration of a pizza" },
    { src: ramen, alt: "Illustration of a bowl of ramen" },
    { src: cafe, alt: "Illustration of a coffee and croissant" }
];
