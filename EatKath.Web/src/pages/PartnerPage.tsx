import { Link as RouterLink } from "react-router-dom";

import { Button, Stack } from "@mui/material";

import InfoPageLayout from "../components/info/InfoPageLayout";
import InfoSection from "../components/info/InfoSection";

function PartnerPage() {

    return (

        <InfoPageLayout
            eyebrow="For Restaurants"
            title="Partner with CraveDine"
            intro="Reach food lovers across Kathmandu who are looking for their next meal, with dine-in and takeaway offers that you design and control."
            metaTitle="Partner with Us"
            metaDescription="List your restaurant on CraveDine and reach diners in Kathmandu with dine-in and takeaway offers you control. Learn how partnering works."
        >

            <InfoSection id="why-partner" title="Why partner with CraveDine">
                <ul>
                    <li>
                        <strong>Be discovered.</strong> Your restaurant appears alongside others in the Kathmandu
                        Valley, searchable by area, cuisine and offer type.
                    </li>
                    <li>
                        <strong>Fill quieter hours.</strong> Offers have a daily arrival window, so you can target
                        the times you most want more guests, for example a mid-afternoon lull.
                    </li>
                    <li>
                        <strong>Stay in control.</strong> You choose the discount, dine-in or takeaway, dates, arrival
                        times, maximum guests and how many offers are available per day or in total.
                    </li>
                    <li>
                        <strong>Show off your food.</strong> Add your logo, cover photo, gallery, opening hours and a
                        digital menu that shows customers the price after your current offer.
                    </li>
                    <li>
                        <strong>Simple at the counter.</strong> See who is coming and when. When a guest visits, you
                        enter the bill amount and CraveDine works out the discount and final amount. Guests pay you
                        directly.
                    </li>
                </ul>
                <p>
                    Results depend on many factors, including your offers, location and demand, so CraveDine cannot
                    guarantee a particular number of customers or level of sales.
                </p>
            </InfoSection>

            <InfoSection id="how-it-works-for-restaurants" title="How it works for restaurants">
                <ol>
                    <li><strong>Get in touch</strong> with our team and tell us about your restaurant.</li>
                    <li><strong>We set up your owner account</strong> and agree the partnership terms with you.</li>
                    <li>
                        <strong>Create your restaurant profile:</strong> area, cuisines, address, contact details,
                        photos, opening hours and menu.
                    </li>
                    <li><strong>Publish your offers</strong> for dine-in, takeaway or both.</li>
                    <li>
                        <strong>Welcome guests.</strong> Customers redeem your offer for an arrival time and show it
                        when they arrive; you complete the redemption with their bill amount.
                    </li>
                </ol>
            </InfoSection>

            <InfoSection id="list-your-restaurant" title="List your restaurant">
                <p>To get started, please have the following ready:</p>
                <ul>
                    <li>your restaurant's name, address and area (for example Thamel, Jawalakhel or Bhaktapur);</li>
                    <li>the cuisines you serve;</li>
                    <li>a phone number and email address for the restaurant;</li>
                    <li>your logo and a few good photos;</li>
                    <li>your menu and opening hours;</li>
                    <li>ideas for the offers you would like to run.</li>
                </ul>
                <p>
                    Then contact our team and we will guide you through the next steps. Partnership terms, including
                    any fees, are agreed with you before you go live.
                </p>
                <Stack direction={{ xs: "column", sm: "row" }} spacing={1.5} sx={{ mt: 2 }}>
                    <Button component={RouterLink} to="/contact" variant="contained">
                        Contact our team
                    </Button>
                    <Button component={RouterLink} to="/faqs#restaurants" variant="outlined">
                        Restaurant FAQs
                    </Button>
                </Stack>
            </InfoSection>

        </InfoPageLayout>

    );

}

export default PartnerPage;
