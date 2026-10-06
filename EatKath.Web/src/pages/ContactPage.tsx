import type { ReactNode } from "react";
import { Link as RouterLink } from "react-router-dom";

import { Alert, Card, CardContent, Grid, Link, Stack, Typography } from "@mui/material";

import AccessTimeOutlinedIcon from "@mui/icons-material/AccessTimeOutlined";
import EmailOutlinedIcon from "@mui/icons-material/EmailOutlined";
import HandshakeOutlinedIcon from "@mui/icons-material/HandshakeOutlined";
import PhoneOutlinedIcon from "@mui/icons-material/PhoneOutlined";
import PlaceOutlinedIcon from "@mui/icons-material/PlaceOutlined";

import InfoPageLayout from "../components/info/InfoPageLayout";
import InfoSection from "../components/info/InfoSection";
import InfoRow from "../components/common/InfoRow";
import { CONTACT_DETAILS } from "../config/siteConfig";

// A configured detail, or a clear "to be published" note when it isn't set
// yet (details come from config/siteConfig.ts).
function Detail({ value, href }: { value: string | null; href?: string }): ReactNode {

    if (!value)
        return <Typography component="span" color="text.secondary">Will be published soon</Typography>;

    return href ? <Link href={href} sx={{ fontWeight: 600 }}>{value}</Link> : value;

}

function ContactPage() {

    const { supportEmail, partnershipsEmail, phone, address, supportHours } = CONTACT_DETAILS;

    const missingDetails = !supportEmail || !phone;

    return (

        <InfoPageLayout
            eyebrow="About CraveDine"
            title="Contact us"
            intro="Questions about an offer, your account or partnering with CraveDine? We are here to help."
            metaTitle="Contact Us"
            metaDescription="Contact the CraveDine team in Kathmandu for help with offers, redemptions, your account or restaurant partnerships."
        >

            {missingDetails && (
                <Alert severity="info" sx={{ borderRadius: "12px" }}>
                    Our support contact details will be published here before launch. In the meantime, the{" "}
                    <Link component={RouterLink} to="/faqs" sx={{ fontWeight: 600 }}>FAQs</Link> answer the most
                    common questions.
                </Alert>
            )}

            <Card>
                <CardContent sx={{ p: { xs: 2.5, md: 3 } }}>
                    <Grid container spacing={3}>
                        <Grid size={{ xs: 12, sm: 6 }}>
                            <InfoRow icon={<EmailOutlinedIcon />} label="Customer support">
                                <Detail value={supportEmail} href={supportEmail ? `mailto:${supportEmail}` : undefined} />
                            </InfoRow>
                        </Grid>
                        <Grid size={{ xs: 12, sm: 6 }}>
                            <InfoRow icon={<HandshakeOutlinedIcon />} label="Restaurant partnerships">
                                <Detail value={partnershipsEmail} href={partnershipsEmail ? `mailto:${partnershipsEmail}` : undefined} />
                            </InfoRow>
                        </Grid>
                        <Grid size={{ xs: 12, sm: 6 }}>
                            <InfoRow icon={<PhoneOutlinedIcon />} label="Phone">
                                <Detail value={phone} href={phone ? `tel:${phone.replace(/\s+/g, "")}` : undefined} />
                            </InfoRow>
                        </Grid>
                        <Grid size={{ xs: 12, sm: 6 }}>
                            <InfoRow icon={<AccessTimeOutlinedIcon />} label="Support hours">
                                <Detail value={supportHours} />
                            </InfoRow>
                        </Grid>
                        {address && (
                            <Grid size={12}>
                                <InfoRow icon={<PlaceOutlinedIcon />} label="Address">
                                    {address}
                                </InfoRow>
                            </Grid>
                        )}
                    </Grid>
                </CardContent>
            </Card>

            <InfoSection id="customers" title="Help with an offer or redemption">
                <p>To help us respond quickly, please include:</p>
                <ul>
                    <li>your name and the email address on your CraveDine account;</li>
                    <li>the restaurant name and your <strong>redemption number</strong> (shown in My Redemptions);</li>
                    <li>the date and time of your visit, and what happened.</li>
                </ul>
                <p>
                    Questions about your bill or a refund are best raised with the restaurant first, as you pay
                    the restaurant directly.
                </p>
            </InfoSection>

            <InfoSection id="restaurants" title="Restaurant partners">
                <p>
                    Interested in listing your restaurant? Tell us your restaurant's name, area and the best way to
                    reach you. You can read more on our <RouterLink to="/partner">Partner with Us</RouterLink> page.
                    Existing partners can include their restaurant name and the redemption number for anything
                    related to a specific visit.
                </p>
            </InfoSection>

            <Stack>
                <Typography variant="body2" color="text.secondary">
                    Looking for answers right away? Visit our{" "}
                    <Link component={RouterLink} to="/faqs" sx={{ fontWeight: 600 }}>FAQs</Link> or{" "}
                    <Link component={RouterLink} to="/how-it-works" sx={{ fontWeight: 600 }}>How It Works</Link>.
                </Typography>
            </Stack>

        </InfoPageLayout>

    );

}

export default ContactPage;
