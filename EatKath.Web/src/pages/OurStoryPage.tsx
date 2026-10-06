import { Link as RouterLink } from "react-router-dom";

import { Button, Stack } from "@mui/material";

import InfoPageLayout from "../components/info/InfoPageLayout";
import InfoSection from "../components/info/InfoSection";

function OurStoryPage() {

    return (

        <InfoPageLayout
            eyebrow="About CraveDine"
            title="Our story"
            intro="CraveDine helps people in Kathmandu discover great places to eat and enjoy them for less, while helping local restaurants welcome more guests."
            metaTitle="Our Story"
            metaDescription="CraveDine helps food lovers in Kathmandu discover restaurants and enjoy dine-in and takeaway offers at better value, starting in the Kathmandu Valley."
        >

            <InfoSection id="mission" title="Our mission">
                <p>
                    Eating out should feel exciting, not expensive. Our mission is simple: make it easier to
                    find good food nearby and to enjoy it at better value, whether you are planning a family
                    dinner, a quick lunch with friends or a takeaway on the way home.
                </p>
                <p>
                    At the same time, we want to give local restaurants, cafes and diners a straightforward way
                    to reach new customers and to fill the quieter hours of their day.
                </p>
            </InfoSection>

            <InfoSection id="why-kathmandu" title="Starting in the Kathmandu Valley">
                <p>
                    Kathmandu has an incredible food culture, from momo and thakali sets to bakeries, cafes and
                    international kitchens. Yet finding the right place, and knowing whether there is a good
                    offer on today, still often depends on word of mouth.
                </p>
                <p>
                    CraveDine starts here, with restaurants across Kathmandu, Lalitpur and Bhaktapur and the
                    neighbourhoods around them. Building locally lets us focus on what diners and restaurant
                    owners in the valley actually need.
                </p>
            </InfoSection>

            <InfoSection id="what-we-do" title="What CraveDine does">
                <ul>
                    <li>
                        <strong>Helps you discover restaurants.</strong> Browse local restaurants and filter by
                        area, cuisine and offer type.
                    </li>
                    <li>
                        <strong>Shows real, current offers.</strong> Restaurants publish dine-in and takeaway
                        discounts with clear dates, arrival times and availability, so you know what you are
                        getting before you go.
                    </li>
                    <li>
                        <strong>Keeps redeeming simple.</strong> Choose an offer and an arrival time, then show
                        your redemption at the restaurant. The discount is applied to your bill there.
                    </li>
                    <li>
                        <strong>Puts restaurants in control.</strong> Partner restaurants decide their own
                        discounts, times and limits, and manage their menus, photos and offers themselves.
                    </li>
                </ul>
            </InfoSection>

            <InfoSection id="what-we-value" title="What we value">
                <ul>
                    <li><strong>Honest value.</strong> Offers should be clear, with no hidden catches.</li>
                    <li><strong>Local first.</strong> We exist to support Kathmandu's restaurants and the people who love them.</li>
                    <li><strong>Simplicity.</strong> Finding and using an offer should take moments, not effort.</li>
                    <li><strong>Fairness for restaurants.</strong> Partners set terms that work for their business.</li>
                </ul>
            </InfoSection>

            <InfoSection id="get-started" title="Get started">
                <p>
                    Hungry? Explore what is on offer today. Running a restaurant? We would love to hear from you.
                </p>
                <Stack direction={{ xs: "column", sm: "row" }} spacing={1.5} sx={{ mt: 2 }}>
                    <Button component={RouterLink} to="/restaurants" variant="contained">
                        Explore restaurants
                    </Button>
                    <Button component={RouterLink} to="/partner" variant="outlined">
                        Partner with us
                    </Button>
                </Stack>
            </InfoSection>

        </InfoPageLayout>

    );

}

export default OurStoryPage;
