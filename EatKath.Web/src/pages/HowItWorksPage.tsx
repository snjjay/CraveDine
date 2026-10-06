import { Link as RouterLink } from "react-router-dom";

import { Box, Button, Stack, Typography } from "@mui/material";

import InfoPageLayout from "../components/info/InfoPageLayout";
import InfoSection from "../components/info/InfoSection";

function HowItWorksPage() {

    return (

        <InfoPageLayout
            eyebrow="Explore"
            title="How CraveDine works"
            intro="Find a restaurant, pick a dine-in or takeaway offer, redeem it for the time you plan to arrive, and get the discount on your bill at the restaurant."
            metaTitle="How It Works"
            metaDescription="How to discover restaurants in Kathmandu on CraveDine, choose dine-in or takeaway offers, redeem them and get your discount at the restaurant."
        >

            <InfoSection id="discover" title="1. Discover restaurants and offers">
                <p>
                    Browse restaurants on the <RouterLink to="/restaurants">Restaurants</RouterLink> page. You can
                    search by restaurant name, area or discount, and narrow the list by <strong>Area</strong>,{" "}
                    <strong>Cuisine</strong> and <strong>Offer Type</strong>.
                </p>
                <p>
                    Each restaurant card shows its current offers at a glance, for example
                    "25% Off · Dine In" with the arrival times. Signed in as a customer, you can also tap the
                    heart to save restaurants to your Favourites.
                </p>
            </InfoSection>

            <InfoSection id="dine-in-or-takeaway" title="2. Choose dine-in or takeaway">
                <p>Every offer is for one way of eating:</p>
                <ul>
                    <li><strong>Dine-in</strong> offers are for eating at the restaurant.</li>
                    <li><strong>Takeaway</strong> offers are for collecting your food to take away.</li>
                </ul>
                <p>
                    Use the Offer Type filter to see only restaurants with a current{" "}
                    <RouterLink to="/restaurants?offer=dine-in">dine-in</RouterLink> or{" "}
                    <RouterLink to="/restaurants?offer=takeaway">takeaway</RouterLink> offer.
                </p>
            </InfoSection>

            <InfoSection id="check-conditions" title="3. Check the offer conditions">
                <p>Open a restaurant to see the details of each offer before you redeem it:</p>
                <ul>
                    <li>the discount, for example 20% off, and whether it is for dine-in or takeaway;</li>
                    <li>the dates the offer is valid and the daily arrival window, for example 5:00 PM to 8:00 PM;</li>
                    <li>how many offers are left, as some offers are limited per day or in total;</li>
                    <li>the maximum number of guests, shown when you redeem.</li>
                </ul>
                <p>
                    The restaurant may also give you instructions when you arrive, so please follow them.
                </p>
                <p>
                    Restaurant menus also show the dine-in or takeaway price after the current offer, so you can
                    see what you might pay.
                </p>
            </InfoSection>

            <InfoSection id="redeem" title="4. Redeem the offer">
                <p>
                    Sign in to your customer account (or create one for free), then choose <strong>Redeem</strong>{" "}
                    on the offer you want and select:
                </p>
                <ul>
                    <li>your <strong>arrival date</strong>, within the offer's dates;</li>
                    <li>your <strong>arrival time</strong>, within the offer's arrival window;</li>
                    <li>the <strong>number of guests</strong>, up to the offer's limit.</li>
                </ul>
                <p>
                    You will receive a redemption number straight away, and your redemption appears under{" "}
                    <strong>My Redemptions</strong>. You can redeem each offer once per arrival date.
                </p>
                <p>
                    CraveDine offers are walk-in offers: redeeming an offer is not a table booking.
                </p>
            </InfoSection>

            <InfoSection id="at-the-restaurant" title="5. Enjoy it at the restaurant">
                <p>
                    Arrive at the time you chose and show your redemption from My Redemptions to the restaurant
                    team, then follow any instructions the restaurant gives you.
                </p>
                <p>
                    When the restaurant records your bill, the discount is applied to it, and you pay the
                    restaurant directly. CraveDine does not take payments.
                </p>
                <Box sx={{ mt: 2, p: 2.5, borderRadius: "12px", bgcolor: "primarySoft" }}>
                    <Typography sx={{ fontWeight: 700, color: "text.primary" }}>Example</Typography>
                    <Typography sx={{ mt: 0.5 }}>
                        With a 20% off offer, a bill of रु. 2,000 is reduced by रु. 400, so you pay रु. 1,600.
                    </Typography>
                </Box>
            </InfoSection>

            <InfoSection id="change-of-plans" title="Change of plans?">
                <p>
                    You can cancel a redemption from My Redemptions at any time before the restaurant has
                    completed it. This frees the offer for someone else.
                </p>
            </InfoSection>

            <Stack direction={{ xs: "column", sm: "row" }} spacing={1.5}>
                <Button component={RouterLink} to="/restaurants" variant="contained">
                    Find a restaurant
                </Button>
                <Button component={RouterLink} to="/faqs" variant="outlined">
                    Read the FAQs
                </Button>
            </Stack>

        </InfoPageLayout>

    );

}

export default HowItWorksPage;
