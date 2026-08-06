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