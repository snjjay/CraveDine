// HeroImageRotator = the homepage hero photo mosaic (one tall image, two
// small ones) that crossfades to a new set every 10 seconds.
//
// - Uses approved restaurant photos first, then the curated collection
//   (config/heroImages.ts); no API calls of its own.
// - Each rotation cycle shows every image once before repeating.
// - The next set is preloaded before it fades in; images that fail to
//   load are dropped from the pool, and tiles have a dark background so
//   nothing flashes white.
// - With reduced motion, the first set stays still (no rotation).
// - Pauses while the browser tab is hidden.

import { useEffect, useMemo, useRef, useState } from "react";

import { Box, useMediaQuery } from "@mui/material";
import { keyframes } from "@mui/system";

import { CURATED_HERO_IMAGES, type HeroImage } from "../../config/heroImages";

const ROTATE_MS = 10_000;
const FADE_MS = 1_200;
const PER_SET = 3;

// Restaurant photos alone are used once there are enough for two sets.
const ENOUGH_RESTAURANT_PHOTOS = PER_SET * 2;

const fadeIn = keyframes`
    from { opacity: 0; }
    to { opacity: 1; }
`;

// Split the pool into sets of three; a short last set is topped up from
// the start so every set is full.
function buildSets(pool: HeroImage[]): HeroImage[][] {

    if (pool.length === 0)
        return [];

    const sets: HeroImage[][] = [];

    for (let i = 0; i < pool.length; i += PER_SET) {

        const set = pool.slice(i, i + PER_SET);

        for (let j = 0; set.length < Math.min(PER_SET, pool.length); j++) {
            if (!set.includes(pool[j]))
                set.push(pool[j]);
        }

        sets.push(set);
    }

    return sets;

}

// Resolves once every image in the set has loaded or failed; returns the
// sources that failed.
function preload(set: HeroImage[]): Promise<string[]> {

    return Promise.all(set.map(image => new Promise<string | null>(resolve => {

        const img = new Image();
        img.onload = () => resolve(null);
        img.onerror = () => resolve(image.src);
        img.src = image.src;

    }))).then(results => results.filter((src): src is string => src !== null));

}

interface Props {
    // Approved restaurant photos, already checked against loaded data.
    restaurantPhotos: HeroImage[];
}

function HeroImageRotator({ restaurantPhotos }: Props) {

    const reducedMotion = useMediaQuery("(prefers-reduced-motion: reduce)");

    const [failed, setFailed] = useState<string[]>([]);

    const pool = useMemo(() => {

        const candidates = restaurantPhotos.length >= ENOUGH_RESTAURANT_PHOTOS
            ? restaurantPhotos
            : [...restaurantPhotos, ...CURATED_HERO_IMAGES];

        const seen = new Set<string>();

        return candidates.filter(image => {
            if (seen.has(image.src) || failed.includes(image.src))
                return false;
            seen.add(image.src);
            return true;
        });

    }, [restaurantPhotos, failed]);

    const sets = useMemo(() => buildSets(pool), [pool]);

    const [current, setCurrent] = useState(0);
    const [previous, setPrevious] = useState<number | null>(null);

    // Latest values for the interval callback.
    const latest = useRef({ current, sets });

    useEffect(() => {
        latest.current = { current, sets };
    });

    useEffect(() => {

        if (reducedMotion || sets.length < 2)
            return;

        let cancelled = false;
        let clearPrevious: number | undefined;

        const timer = window.setInterval(async () => {

            if (document.hidden)
                return;

            const { current: shown, sets: allSets } = latest.current;
            const next = (shown + 1) % allSets.length;

            const failedSources = await preload(allSets[next]);

            if (cancelled)
                return;

            if (failedSources.length > 0) {
                // Drop broken images; the sets rebuild without them.
                setFailed(list => [...list, ...failedSources]);
                return;
            }

            setPrevious(shown % allSets.length);
            setCurrent(next);

            window.clearTimeout(clearPrevious);
            clearPrevious = window.setTimeout(() => setPrevious(null), FADE_MS);

        }, ROTATE_MS);

        return () => {
            cancelled = true;
            window.clearInterval(timer);
            window.clearTimeout(clearPrevious);
        };

    }, [reducedMotion, sets.length]);

    if (sets.length === 0)
        return null;

    const shownIndex = current % sets.length;

    const renderSet = (index: number, animate: boolean) => (

        <Box
            key={`set-${index}-${sets[index].map(i => i.src).join("|")}`}
            data-hero-set={index}
            sx={{
                position: "absolute",
                inset: 0,
                display: "grid",
                gridTemplateColumns: "1fr 1fr",
                gridTemplateRows: "1fr 1fr",
                gap: 1,
                animation: animate ? `${fadeIn} ${FADE_MS}ms ease` : "none"
            }}
        >
            {sets[index].map((image, i) => (

                <Box
                    key={image.src}
                    sx={{
                        gridRow: i === 0 ? "span 2" : undefined,
                        minHeight: 0,
                        borderRadius: "12px",
                        overflow: "hidden",
                        // Dark tile behind the image: no white flash and a
                        // tidy fallback if an image fails.
                        background: "linear-gradient(135deg, #3A2B29 0%, #262120 100%)"
                    }}
                >
                    <Box
                        component="img"
                        src={image.src}
                        alt={image.alt}
                        onError={() => setFailed(list => list.includes(image.src) ? list : [...list, image.src])}
                        sx={{ width: "100%", height: "100%", objectFit: "cover", display: "block" }}
                    />
                </Box>

            ))}
        </Box>

    );

    return (

        <Box sx={{ position: "absolute", inset: 0 }}>
            {previous !== null && previous !== shownIndex && previous < sets.length && renderSet(previous, false)}
            {renderSet(shownIndex, previous !== null && !reducedMotion)}
        </Box>

    );

}

export default HeroImageRotator;
