import api from "../api/axios";

import type { User } from "../types/User";

class UserService {

    // ----------------------------------------
    // Get All Users (Admin only)
    // GET /api/user
    // ----------------------------------------

    async getAll(): Promise<User[]> {

        const response =
            await api.get<User[]>("/user");

        return response.data;

    }

}

export default new UserService();
