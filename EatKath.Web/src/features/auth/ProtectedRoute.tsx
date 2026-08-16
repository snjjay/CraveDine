import { useContext } from "react";
import { Navigate } from "react-router-dom";
import type { ReactNode } from "react";

import AuthContext from "./AuthContext";

interface Props {
    children: ReactNode;
    role?: string;
}

function ProtectedRoute({
    children,
    role
}: Props) {

    const auth = useContext(AuthContext);

    if (!auth) {
        throw new Error("AuthContext not found.");
    }

    // Not logged in
    if (!auth.user) {
        return <Navigate to="/login" replace />;
    }

    // Logged in but wrong role
    if (role && auth.user.role !== role) {
        return <Navigate to="/" replace />;
    }

    return <>{children}</>;
}

export default ProtectedRoute;


// ==========================================================
// STEP 17 — ProtectedRoute.tsx
// ==========================================================
//
// ProtectedRoute = 🛂 SECURITY GUARD for pages.
//
// Before showing a protected page, it checks:
//
// 1. Is the user logged in?
// 2. Does the user have the required role?
//
// ----------------------------------------------------------
//
// children
// → 🚪 The page behind the security guard.
//
// role
// → 🎫 The role required to enter.
//
// Example:
//
// <ProtectedRoute role="Owner">
//     <OwnerDashboardPage />
// </ProtectedRoute>
//
// → Only an Owner can enter OwnerDashboardPage.
//
// ----------------------------------------------------------
//
// CHECK 1 — LOGGED IN?
//
// if (!auth.user)
//
// → No user = not logged in.
//
// Send user to:
//
// /login
//
// ----------------------------------------------------------
//
// CHECK 2 — CORRECT ROLE?
//
// if (role && auth.user.role !== role)
//
// → User is logged in but has the wrong role.
//
// Example:
//
// Required = Owner
// User     = Customer
//
// Customer ≠ Owner
//      ↓
// Send user to /
//
// ----------------------------------------------------------
//
// CHECK 3 — ALLOWED
//
// return <>{children}</>;
//
// → User is logged in
// → User has the correct role
// → Show the requested page.
//
// ----------------------------------------------------------
//
// COMPLETE FLOW:
//
// User tries to open protected page
//          ↓
// 🛂 ProtectedRoute
//          ↓
// Is user logged in?
//     ↓              ↓
//    NO              YES
//     ↓              ↓
// /login        Is role correct?
//                  ↓       ↓
//                 NO       YES
//                  ↓        ↓
//                  /     Show children
//                              ↓
//                          Actual Page
//
// ----------------------------------------------------------
//
// IMPORTANT:
//
// ProtectedRoute
// → Protects the FRONTEND PAGE.
//
// [Authorize] in .NET API
// → Protects the BACKEND/API.
//
// Both are important.
//
// 🔑 Remember:
//
// ProtectedRoute = "Are you allowed to see this page?"
//
// ==========================================================

// ==========================================================
// AUTH FLOW
//
// 15. AuthContext.tsx       ✅
// 16. AuthProvider.tsx      ✅
// 17. ProtectedRoute.tsx    ✅
// 18. LoginPage.tsx         ← NEXT
//
// ==========================================================