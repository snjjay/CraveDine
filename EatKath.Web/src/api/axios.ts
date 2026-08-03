import axios from "axios";

const api = axios.create({

    baseURL: import.meta.env.VITE_API_URL,

    headers: {
        "Content-Type": "application/json"
    }

});

api.interceptors.request.use(

    (config) => {

        const storedUser = localStorage.getItem("user");

        if (storedUser) {

            const user = JSON.parse(storedUser);

            if (user.token) {

                config.headers.Authorization = `Bearer ${user.token}`;

            }

        }

        return config;

    },

    (error) => Promise.reject(error)

);

export default api;