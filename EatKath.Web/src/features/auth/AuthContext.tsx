import { createContext } from "react";
import type { AuthResponse } from "./types";

export interface AuthContextType {
    user: AuthResponse | null;
    login: (user: AuthResponse) => void;
    logout: () => void;
}

const AuthContext = createContext<AuthContextType | undefined>(undefined);

export default AuthContext;