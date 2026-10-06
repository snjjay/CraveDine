// usePageMeta = sets the browser tab title and the meta description for
// a page, and restores the previous values when the page is left.

import { useEffect } from "react";

export function usePageMeta(title: string, description: string) {

    useEffect(() => {

        const meta = document.querySelector('meta[name="description"]');

        const previousTitle = document.title;
        const previousDescription = meta?.getAttribute("content") ?? "";

        document.title = `${title} | CraveDine`;
        meta?.setAttribute("content", description);

        return () => {
            document.title = previousTitle;
            meta?.setAttribute("content", previousDescription);
        };

    }, [title, description]);

}
