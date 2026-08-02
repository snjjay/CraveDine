import { useContext } from "react";
import { Navigate } from "react-router-dom";
import type { ReactNode } from "react";

import AuthContext from "./AuthContext";

interface Props {
    children: ReactNode;
}

function ProtectedRoute({ children }: Props) {

    const auth = useContext(AuthContext);

    if (!auth) {
        throw new Error("AuthContext not found.");
    }

    if (!auth.user) {
        return <Navigate to="/login" replace />;
    }

    return <>{children}</>;
}

export default ProtectedRoute;