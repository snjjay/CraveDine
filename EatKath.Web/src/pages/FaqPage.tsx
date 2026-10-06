import type { ReactNode } from "react";
import { Link as RouterLink } from "react-router-dom";

import {
    Accordion,
    AccordionDetails,
    AccordionSummary,
    Box,
    Typography
} from "@mui/material";

import ExpandMoreIcon from "@mui/icons-material/ExpandMore";

import InfoPageLayout from "../components/info/InfoPageLayout";
import InfoSection from "../components/info/InfoSection";

interface Faq {
    question: string;
    answer: ReactNode;
}

const FAQ_GROUPS: { id: string; title: string; items: Faq[] }[] = [
    {
        id: "finding-restaurants",
        title: "Finding restaurants and offers",
        items: [
            {
                question: "How do I find restaurants in Kathmandu?",
                answer: (
                    <p>
                        Go to <RouterLink to="/restaurants">Restaurants</RouterLink>. You can search by name, area or
                        discount, and filter by Area (including Kathmandu, Lalitpur, Bhaktapur and their
                        neighbourhoods), Cuisine and Offer Type.
                    </p>
                )
            },
            {
                question: "What is the difference between dine-in and takeaway offers?",
                answer: (
                    <p>
                        A dine-in offer is for eating at the restaurant. A takeaway offer is for collecting your food
                        to take away. Each offer applies to one or the other, as shown on the offer.
                    </p>
                )
            },
            {
                question: "When can I use an offer?",
                answer: (
                    <p>
                        Each offer is valid between set dates and has a daily arrival window, for example 12:00 PM
                        to 3:00 PM, both shown on the offer. When you redeem, you choose an arrival date and time
                        inside those limits.
                    </p>
                )
            },
            {
                question: "Why does an offer say it is sold out, or that no offers are left?",
                answer: (
                    <p>
                        Restaurants can limit how many times an offer is redeemed, in total or per day. "Sold out"
                        means the total limit has been reached. "No offers left" for a date means that day's limit
                        has been reached; you may still be able to choose another date.
                    </p>
                )
            },
            {
                question: "Can I save restaurants I like?",
                answer: (
                    <p>
                        Yes. When you are signed in as a customer, tap the heart on a restaurant to add it to your
                        Favourites.
                    </p>
                )
            }
        ]
    },
    {
        id: "redeeming",
        title: "Redeeming offers",
        items: [
            {
                question: "How do I redeem an offer?",
                answer: (
                    <p>
                        Sign in, open the restaurant, choose Redeem on the offer and select your arrival date, arrival
                        time and number of guests. You will get a redemption number, and the redemption is listed
                        under My Redemptions. See <RouterLink to="/how-it-works">How It Works</RouterLink> for the
                        full steps.
                    </p>
                )
            },
            {
                question: "Do I need to book a table?",
                answer: (
                    <p>
                        No. CraveDine offers are walk-in offers. Redeeming an offer tells the restaurant when you plan
                        to arrive, but it is not a table reservation.
                    </p>
                )
            },
            {
                question: "What do I do at the restaurant?",
                answer: (
                    <p>
                        Arrive at the time you chose, show your redemption from My Redemptions to the restaurant team
                        and follow their instructions. The restaurant applies the discount when it records your bill.
                    </p>
                )
            },
            {
                question: "Can I redeem the same offer more than once?",
                answer: (
                    <p>
                        You can redeem each offer once per arrival date. If the offer runs on other days, you can
                        redeem it again for a different date, as long as offers are still available.
                    </p>
                )
            },
            {
                question: "Can I cancel a redemption?",
                answer: (
                    <p>
                        Yes. Open My Redemptions and cancel it at any time before the restaurant has completed it.
                        Restaurants can also cancel a redemption.
                    </p>
                )
            }
        ]
    },
    {
        id: "payments",
        title: "Payments and refunds",
        items: [
            {
                question: "Do I pay through CraveDine?",
                answer: (
                    <p>
                        No. CraveDine does not take payments. You pay the restaurant directly for your meal, with the
                        offer's discount applied to your bill. Prices are in Nepalese rupees (NPR, रु.).
                    </p>
                )
            },
            {
                question: "How do refunds work?",
                answer: (
                    <p>
                        Because payments are made directly to the restaurant, CraveDine does not process refunds.
                        Please raise any question about your bill or a refund with the restaurant. If you think an
                        offer was not honoured, <RouterLink to="/contact">contact us</RouterLink> with your
                        redemption number.
                    </p>
                )
            }
        ]
    },
    {
        id: "account",
        title: "Your account",
        items: [
            {
                question: "Do I need an account?",
                answer: (
                    <p>
                        You can browse restaurants and offers without an account. To redeem offers or save
                        favourites, create a free customer account with your name, email address and a password.
                        Adding a phone number is optional.
                    </p>
                )
            },
            {
                question: "Why was I signed out?",
                answer: (
                    <p>
                        For your security, you are signed out automatically after about an hour. Simply sign in
                        again to continue.
                    </p>
                )
            },
            {
                question: "I forgot my password. What should I do?",
                answer: (
                    <p>
                        Password reset is not available in the app yet. Please{" "}
                        <RouterLink to="/contact">contact us</RouterLink> for help with your account.
                    </p>
                )
            }
        ]
    },
    {
        id: "restaurants",
        title: "For restaurants",
        items: [
            {
                question: "How can my restaurant join CraveDine?",
                answer: (
                    <p>
                        Get in touch through our <RouterLink to="/partner">Partner with Us</RouterLink> page. Our team
                        will talk through how CraveDine works and set up a restaurant owner account for you.
                    </p>
                )
            },
            {
                question: "What can I manage as a restaurant partner?",
                answer: (
                    <p>
                        From the owner dashboard you can manage your restaurant profile (including area, cuisines,
                        logo, cover photo, gallery and menu PDF), opening hours and your digital menu. You can create
                        and edit dine-in and takeaway offers, and view, complete and cancel customer redemptions.
                    </p>
                )
            },
            {
                question: "Who decides the discount and the offer conditions?",
                answer: (
                    <p>
                        You do. For each offer you set the title and description, the discount, whether it is
                        dine-in or takeaway, the dates, the daily arrival window, the maximum number of guests, and
                        optional limits on offers per day and in total.
                    </p>
                )
            },
            {
                question: "How is the discount applied to a customer's bill?",
                answer: (
                    <p>
                        When the customer visits, you complete their redemption by entering the bill amount.
                        CraveDine calculates the discount and the final amount for that visit. The customer pays you
                        directly.
                    </p>
                )
            },
            {
                question: "Does it cost anything to join?",
                answer: (
                    <p>
                        Partnership terms, including any fees, are agreed directly with our team. Please{" "}
                        <RouterLink to="/contact">contact us</RouterLink> to discuss them.
                    </p>
                )
            }
        ]
    },
    {
        id: "support",
        title: "Support",
        items: [
            {
                question: "How do I contact CraveDine?",
                answer: (
                    <p>
                        Visit our <RouterLink to="/contact">Contact Us</RouterLink> page. If your question is about a
                        redemption, please include your redemption number so we can help faster.
                    </p>
                )
            }
        ]
    }
];

function FaqPage() {

    return (

        <InfoPageLayout
            eyebrow="About CraveDine"
            title="Frequently asked questions"
            intro="Answers to common questions about finding restaurants, redeeming offers, payments and partnering with CraveDine."
            metaTitle="FAQs"
            metaDescription="Answers to common questions about CraveDine: finding restaurants in Kathmandu, dine-in and takeaway offers, redemption, payments, accounts and restaurant partnerships."
        >

            {FAQ_GROUPS.map(group => (

                <InfoSection key={group.id} id={group.id} title={group.title}>

                    <Box>
                        {group.items.map((faq, index) => (

                            <Accordion
                                key={faq.question}
                                disableGutters
                                elevation={0}
                                sx={{
                                    bgcolor: "background.paper",
                                    border: "1px solid",
                                    borderColor: "divider",
                                    borderRadius: "0 !important",
                                    "&:first-of-type": { borderTopLeftRadius: "12px !important", borderTopRightRadius: "12px !important" },
                                    "&:last-of-type": { borderBottomLeftRadius: "12px !important", borderBottomRightRadius: "12px !important" },
                                    "&:not(:first-of-type)": { borderTop: 0 },
                                    "&::before": { display: "none" }
                                }}
                            >
                                <AccordionSummary
                                    expandIcon={<ExpandMoreIcon />}
                                    id={`${group.id}-q${index}`}
                                    aria-controls={`${group.id}-a${index}`}
                                >
                                    <Typography component="h3" sx={{ fontWeight: 600, color: "text.primary" }}>
                                        {faq.question}
                                    </Typography>
                                </AccordionSummary>
                                <AccordionDetails id={`${group.id}-a${index}`} sx={{ pt: 0 }}>
                                    {faq.answer}
                                </AccordionDetails>
                            </Accordion>

                        ))}
                    </Box>

                </InfoSection>

            ))}

        </InfoPageLayout>

    );

}

export default FaqPage;
