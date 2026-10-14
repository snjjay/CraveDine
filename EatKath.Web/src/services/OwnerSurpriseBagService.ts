import api from "../api/axios";

import type {
    CreateSurpriseBag,
    OwnerSurpriseBag,
    UpdateSurpriseBag
} from "../types/OwnerSurpriseBag";

class OwnerSurpriseBagService {

    // The owner's bags (all restaurants, or one), including inactive and
    // past bags, newest pickup first.
    async getMyBags(
        restaurantId?: number | null
    ): Promise<OwnerSurpriseBag[]> {

        const response =
            await api.get<OwnerSurpriseBag[]>("/SurpriseBag/my", {
                params:
                    restaurantId
                        ? { restaurantId }
                        : undefined
            });

        return response.data;

    }

    // One of the owner's bags, whatever its state.
    async getById(id: number): Promise<OwnerSurpriseBag> {

        const response =
            await api.get<OwnerSurpriseBag>(`/SurpriseBag/my/${id}`);

        return response.data;

    }

    async create(bag: CreateSurpriseBag): Promise<OwnerSurpriseBag> {

        const response =
            await api.post<OwnerSurpriseBag>("/SurpriseBag", bag);

        return response.data;

    }

    // Full update. isActive: true reactivates a deactivated bag; left
    // out, the bag keeps its current state.
    async update(
        id: number,
        bag: UpdateSurpriseBag
    ): Promise<OwnerSurpriseBag> {

        const response =
            await api.put<OwnerSurpriseBag>(`/SurpriseBag/${id}`, bag);

        return response.data;

    }

    // Soft delete: hides the bag and stops new reservations; existing
    // reservations are kept.
    async deactivate(id: number): Promise<void> {

        await api.delete(`/SurpriseBag/${id}`);

    }

    // Uploads (or replaces) the bag's image and returns its new stored
    // path. Allowed for inactive and pickup-locked bags. onProgress
    // receives 0-100 while the file is sent.
    async uploadImage(
        id: number,
        file: File,
        onProgress?: (percent: number) => void
    ): Promise<string> {

        const formData = new FormData();

        formData.append("file", file);

        const response =
            await api.post<{ imageUrl: string }>(
                `/SurpriseBag/${id}/image`,
                formData,
                {
                    headers: {
                        "Content-Type": "multipart/form-data"
                    },
                    // Longer than the default 30s: a 5 MB photo on a slow
                    // connection can take a while to send.
                    timeout: 120_000,
                    onUploadProgress: event => {
                        if (onProgress && event.total)
                            onProgress(Math.round((event.loaded / event.total) * 100));
                    }
                }
            );

        return response.data.imageUrl;

    }

    async deleteImage(id: number): Promise<void> {

        await api.delete(`/SurpriseBag/${id}/image`);

    }

}

export default new OwnerSurpriseBagService();
