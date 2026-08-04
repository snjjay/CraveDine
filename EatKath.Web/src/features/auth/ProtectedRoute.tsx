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