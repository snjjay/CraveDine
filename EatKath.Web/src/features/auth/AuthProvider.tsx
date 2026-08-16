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


// ==========================================================
// STEP 16 — AuthProvider.tsx
// ==========================================================
//
// AuthProvider = MANAGES the authentication information.
//
// Simple analogy:
//
// AuthContext     = 📋 Shared notice board
// AuthProvider    = 👤 Person managing the notice board
//
// ----------------------------------------------------------
//
// user
// → 📦 Stores the current logged-in user.
//
// user can be:
//
// 👤 User information
// OR
// null = nobody is logged in.
//
// ----------------------------------------------------------
//
// localStorage
// → 🗄️ Saves the user in the browser.
//
// When the application starts:
//
// Check localStorage
//      ↓
// User found?
//      ↓
// YES → restore user
// NO  → user = null
//
// This allows the login information to survive
// a browser refresh.
//
// ----------------------------------------------------------
//
// login(authUser)
//
// → 🔐 Logs the user in.
//
// Saves user to localStorage
//      ↓
// setUser(authUser)
//      ↓
// React knows the user is logged in.
//
// Two things are updated:
//
// localStorage → survives refresh
// React state  → updates the application immediately
//
// ----------------------------------------------------------
//
// logout()
//
// → 🚪 Logs the user out.
//
// Remove user from localStorage
//      ↓
// setUser(null)
//      ↓
// Application knows user is logged out.
//
// ----------------------------------------------------------
//
// AuthContext.Provider
//
// value={{
//     user,
//     login,
//     logout
// }}
//
// → Makes these available to everything inside
//   <AuthProvider>.
//
// In main.tsx:
//
// <AuthProvider>
//     <App />
// </AuthProvider>
//
// Therefore App and its components can access:
//
// user
// login()
// logout()
//
// ----------------------------------------------------------
//
// COMPLETE FLOW:
//
// main.tsx
//      ↓
// AuthProvider
//      ↓
// Manages user + login + logout
//      ↓
// AuthContext.Provider
//      ↓
// App
//      ↓
// MainLayout / ProtectedRoute / other components
//      ↓
// Can use authentication information
//
// 🔑 Remember:
//
// AuthContext  = defines/shared auth information
// AuthProvider = manages/provides the actual auth information
//
// ==========================================================Next is ProtectedRoute.tsx — Step 17.