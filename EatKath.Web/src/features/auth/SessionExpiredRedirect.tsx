import { useContext, useEffect } from "react";
import { useLocation, useNavigate } from "react-router-dom";

import AuthContext from "./AuthContext";

// Sends the user to /login once after their session expired (AuthProvider
// signs them out and shows the message). Lives inside the router, which
// AuthProvider is outside of. Renders nothing.
function SessionExpiredRedirect() {

    const auth = useContext(AuthContext);

    const navigate = useNavigate();

    const { pathname } = useLocation();

    const sessionExpired = auth?.sessionExpired ?? false;

    const acknowledge = auth?.acknowledgeSessionExpired;

    useEffect(() => {

        if (!sessionExpired || !acknowledge)
            return;

        // Acknowledged straight away: one redirect per expiry, no loop.
        if (pathname !== "/login")
            navigate("/login", { replace: true });

        acknowledge();

    }, [sessionExpired, acknowledge, navigate, pathname]);

    return null;

}

export default SessionExpiredRedirect;
