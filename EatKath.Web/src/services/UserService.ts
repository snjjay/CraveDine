import api from "../api/axios";

import type { User } from "../types/User";
import type { CreateUserRequest } from "../types/CreateUser";
import type { UpdateUserRequest } from "../types/UpdateUser";

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

    // ----------------------------------------
    // Get User By Id (Admin only)
    // GET /api/user/{id}
    // ----------------------------------------

    async getById(id: number): Promise<User> {

        const response =
            await api.get<User>(`/user/${id}`);

        return response.data;

    }

    // ----------------------------------------
    // Create User (Admin only)
    // POST /api/user
    // ----------------------------------------

    async create(dto: CreateUserRequest): Promise<User> {

        const response =
            await api.post<User>("/user", dto);

        return response.data;

    }

    // ----------------------------------------
    // Update User (Admin only)
    // PUT /api/user/{id}
    // ----------------------------------------

    async update(
        id: number,
        dto: UpdateUserRequest
    ): Promise<User> {

        const response =
            await api.put<User>(`/user/${id}`, dto);

        return response.data;

    }

    // ----------------------------------------
    // Delete User (Admin only)
    // DELETE /api/user/{id}
    // ----------------------------------------

    async delete(id: number): Promise<void> {

        await api.delete(`/user/${id}`);

    }

}

export default new UserService();
