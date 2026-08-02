import axios from "axios";

// ==========================================================
// AXIOS HTTP CLIENT
// ==========================================================
//
// Axios is used to communicate with the ASP.NET Core Web API.
//
// Instead of writing:
//
//     axios.get(...)
//     axios.post(...)
//     axios.put(...)
//
// throughout the application, we create ONE shared Axios
// instance and use it everywhere.
//
// Benefits:
//
// • Centralised configuration
// • Easier maintenance
// • Authentication can be added in one place
// • Error handling can be added in one place
// • Base URL only needs to be configured once
//

const api = axios.create({

    // ------------------------------------------------------
    // Base URL of the backend API.
    //
    // import.meta.env reads values from Vite environment
    // files (.env.development, .env.production, etc.)
    //
    // Development:
    //     https://localhost:7203/api
    //
    // Production (Azure):
    //     https://api.eatkath.com/api
    //
    // Docker (future):
    //     http://localhost:8080/api
    //
    // This means we never hard-code URLs throughout
    // the application.
    // ------------------------------------------------------
    baseURL: import.meta.env.VITE_API_URL,

    // ------------------------------------------------------
    // Default headers sent with every HTTP request.
    // ------------------------------------------------------
    headers: {
        "Content-Type": "application/json"
    }
});

// Export the configured Axios client.
// Every API call in EatKath will use this instance.
export default api;