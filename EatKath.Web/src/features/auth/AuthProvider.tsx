import { useState } from "react";
import type { ReactNode } from "react";

import AuthContext from "./AuthContext";
import type { AuthResponse } from "./types";

interface Props {
    children: ReactNode;
}

function AuthProvider({ children }: Props) {

   const [user, setUser] = useState<AuthResponse | null>(() => {

    const storedUser = localStorage.getItem("user");

    if (storedUser) {
        return JSON.parse(storedUser);
    }

    return null;
});

    function login(authUser: AuthResponse) {

        localStorage.setItem("user", JSON.stringify(authUser));

        setUser(authUser);
    }

    function logout() {

        localStorage.removeItem("user");

        setUser(null);
    }

    return (
        <AuthContext.Provider
            value={{
                user,
                login,
                logout
            }}
        >
            {children}
        </AuthContext.Provider>
    );
}

export default AuthProvider;