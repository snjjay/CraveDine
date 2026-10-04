// ==========================================================
// ProfilePage — the logged-in customer's details (read-only)
// ==========================================================
//
// Shows the name and email from the signed-in user's session
// (AuthContext, filled from the login response). No API call and no
// editing yet. Route: /profile (customers only, see AppRoutes).
// ==========================================================

import { useContext } from "react";

import { Card, CardContent, Stack, Typography } from "@mui/material";

import BadgeOutlinedIcon from "@mui/icons-material/BadgeOutlined";
import EmailOutlinedIcon from "@mui/icons-material/EmailOutlined";
import PersonOutlineIcon from "@mui/icons-material/PersonOutline";

import AuthContext from "../features/auth/AuthContext";
import InfoRow from "../components/common/InfoRow";

function ProfilePage() {

    const auth = useContext(AuthContext);

    if (!auth)
        throw new Error("AuthContext not found.");

    // ProtectedRoute only renders this page for a logged-in customer.
    const { user } = auth;

    if (!user)
        return null;

    return (

        <>

            <Typography variant="h4" sx={{ mb: 0.5 }}>
                Profile
            </Typography>

            <Typography variant="body2" color="text.secondary" sx={{ mb: 3 }}>
                Your account details.
            </Typography>

            <Card sx={{ maxWidth: 560 }}>

                <CardContent>

                    <Stack spacing={2.5}>

                        <InfoRow icon={<PersonOutlineIcon />} label="First name">
                            {user.firstName}
                        </InfoRow>

                        <InfoRow icon={<BadgeOutlinedIcon />} label="Surname">
                            {user.lastName}
                        </InfoRow>

                        <InfoRow icon={<EmailOutlinedIcon />} label="Email">
                            {user.email}
                        </InfoRow>

                    </Stack>

                </CardContent>

            </Card>

        </>

    );

}

export default ProfilePage;
