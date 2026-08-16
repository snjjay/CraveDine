import { createContext } from "react";
import type { AuthResponse } from "./types";

export interface AuthContextType {
    user: AuthResponse | null;  //Current logged-in user, AuthResponse>Shape of the user's login information, null //No user is logged in, so the current user is null
    login: (user: AuthResponse) => void; //Function that logs a user in
    logout: () => void;  //Function that logs a user out
}

const AuthContext = createContext<AuthContextType | undefined>(undefined); //Create a shared place where authentication information can be provided to the application
//Initially AuthContext undefined, because nobody has provided the authentication information yet., Later, AuthProvider.tsx will provide the actual values.
export default AuthContext;


//Don't think of Context as complicated. 
//In your project, it is basically a shared place for login information and login/logout functions.
//Think of AuthContext as a shared notice board in the EatKath application:
//📋 AUTH NOTICE BOARD

//Current user: Sanjay / Customer

//Available actions:
//→ Login
//→ Logout


// ==========================================================
// STEP 15 — AuthContext.tsx
// ==========================================================
//
// AuthContext = shared place for authentication information.
//
// Simple analogy:
//
// 📋 Shared authentication notice board
//
// It can contain:
//
// user
// → 👤 Who is currently logged in?
//
// login()
// → 🔐 Log a user in.
//
// logout()
// → 🚪 Log a user out.
//
// ----------------------------------------------------------
//
// AuthContextType:
//
// user: AuthResponse | null
// → Stores the current user.
// → null = nobody is logged in.
//
// login: (user) => void
// → Function for logging in a user.
//
// logout: () => void
// → Function for logging out.
//
// ----------------------------------------------------------
//
// createContext(...)
// → Creates the shared AuthContext.
//
// IMPORTANT:
//
// AuthContext does NOT actually perform the login.
//
// It defines what authentication information/functions
// are available.
//
// AuthProvider will provide the actual values.
//
// ----------------------------------------------------------
//
// FLOW:
//
// AuthContext.tsx
// → Defines shared authentication information
//        ↓
// AuthProvider.tsx
// → Provides/manages the actual information
//        ↓
// MainLayout / ProtectedRoute / Components
// → Use the authentication information
//
// 🔑 Remember:
//
// AuthContext = defines the shared auth information.
// AuthProvider = provides/manages the actual auth information.
//
// ==========================================================