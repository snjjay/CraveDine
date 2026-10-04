import axios from "axios";

// Returns the API's error message ({ "Message": "..." } from the
// ExceptionMiddleware) or the given fallback when there is none.
export function getApiErrorMessage(error: unknown, fallback: string): string {

    if (axios.isAxiosError(error)) {

        const message = (error.response?.data as { Message?: string } | undefined)?.Message;

        if (message)
            return message;

    }

    return fallback;

}
