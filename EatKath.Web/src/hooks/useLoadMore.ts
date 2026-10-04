// ==========================================================
// useLoadMore — show a list in batches ("View next 20 venues")
// ==========================================================
//
// Works on a list that is already loaded (e.g. the restaurant list
// from GET api/Restaurants, filtered in the browser): showing more
// only reveals more of it, it never calls the API again.
//
// resetKey: any string describing the current search/filters. When
// it changes, the list starts again from the first batch.
// ==========================================================

import { useState } from "react";

export const LOAD_MORE_BATCH_SIZE = 20;

export interface LoadMore<T> {
    // The items to render now (first N of the list).
    visibleItems: T[];
    // How many items the next click reveals (up to one batch).
    nextCount: number;
    hasMore: boolean;
    total: number;
    showMore: () => void;
}

export function useLoadMore<T>(
    items: T[],
    resetKey: string,
    batchSize: number = LOAD_MORE_BATCH_SIZE
): LoadMore<T> {

    const [shown, setShown] = useState({ key: resetKey, count: batchSize });

    // Search/filters changed: back to the first batch. Adjusting state
    // during render (React's "previous render" pattern) avoids showing
    // one frame with the old count.
    if (shown.key !== resetKey)
        setShown({ key: resetKey, count: batchSize });

    const count = shown.key === resetKey ? shown.count : batchSize;

    const visibleItems = items.slice(0, count);
    const remaining = items.length - visibleItems.length;

    function showMore() {
        setShown({ key: resetKey, count: count + batchSize });
    }

    return {
        visibleItems,
        nextCount: Math.min(batchSize, remaining),
        hasMore: remaining > 0,
        total: items.length,
        showMore
    };

}
