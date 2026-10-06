import { Link as RouterLink } from "react-router-dom";

import InfoPageLayout from "../components/info/InfoPageLayout";
import InfoSection from "../components/info/InfoSection";
import ToConfirm from "../components/info/ToConfirm";

function TermsPage() {

    return (

        <InfoPageLayout
            eyebrow="Legal"
            title="Terms & Conditions"
            intro={<>These terms apply when you use CraveDine to discover restaurants and redeem offers. Last updated: <ToConfirm>publication date</ToConfirm>.</>}
            metaTitle="Terms & Conditions"
            metaDescription="The terms for using CraveDine to discover restaurants in Kathmandu and redeem dine-in and takeaway offers."
            draft
        >

            <InfoSection id="about" title="1. About CraveDine">
                <p>
                    CraveDine is operated by <ToConfirm>registered legal entity name, registration number and
                    address</ToConfirm>. CraveDine lets you discover restaurants in Kathmandu, Nepal and redeem
                    dine-in and takeaway offers published by partner restaurants.
                </p>
                <p>
                    CraveDine is not a restaurant. Meals, service and pricing are provided by the restaurants
                    themselves.
                </p>
            </InfoSection>

            <InfoSection id="accounts" title="2. Your account">
                <ul>
                    <li>You need a customer account to redeem offers or save favourites.</li>
                    <li>Please provide accurate details and keep your password private.</li>
                    <li>You are responsible for activity on your account.</li>
                    <li>
                        We may suspend or close accounts that misuse CraveDine. <ToConfirm>account suspension and
                        termination process</ToConfirm>.
                    </li>
                    <li><ToConfirm>minimum age to create an account</ToConfirm>.</li>
                </ul>
            </InfoSection>

            <InfoSection id="offers" title="3. Offers">
                <ul>
                    <li>
                        Offers are created by restaurants, which set the discount, whether it is for dine-in or
                        takeaway, the valid dates, the daily arrival window, the maximum number of guests and any
                        limits on how many offers are available per day or in total.
                    </li>
                    <li>Offers are subject to availability and may sell out.</li>
                    <li>
                        A restaurant may change or withdraw an offer. Redemptions you have already made will{" "}
                        <ToConfirm>be honoured or handled in a specified way if an offer changes</ToConfirm>.
                    </li>
                    <li>
                        The discount applies to the bill amount recorded by the restaurant when it completes your
                        redemption. <ToConfirm>any exclusions, e.g. drinks, service charge or taxes, and whether
                        offers can be combined with other promotions</ToConfirm>.
                    </li>
                </ul>
            </InfoSection>

            <InfoSection id="redemption" title="4. Redeeming an offer">
                <ul>
                    <li>
                        To redeem, choose an arrival date and time within the offer's limits and the number of guests,
                        up to the offer's maximum.
                    </li>
                    <li>You can redeem each offer once per arrival date.</li>
                    <li>
                        Redeeming an offer is not a table reservation. Please arrive at your chosen time, show your
                        redemption to the restaurant and follow its instructions.
                    </li>
                    <li>
                        <ToConfirm>what happens if you arrive late or do not attend</ToConfirm>.
                    </li>
                </ul>
            </InfoSection>

            <InfoSection id="cancellation" title="5. Cancellations">
                <p>
                    You can cancel a redemption in My Redemptions before the restaurant completes it. A restaurant
                    can also cancel a redemption. <ToConfirm>the circumstances in which a restaurant may cancel a
                    redemption</ToConfirm>.
                </p>
            </InfoSection>

            <InfoSection id="payments" title="6. Payments and refunds">
                <p>
                    CraveDine does not process payments. You pay the restaurant directly, in Nepalese rupees (NPR,
                    रु.), for the bill after your discount. Questions about your bill, payments or refunds should be
                    raised with the restaurant. If you believe an offer was not honoured,{" "}
                    <RouterLink to="/contact">contact us</RouterLink> with your redemption number.
                </p>
            </InfoSection>

            <InfoSection id="restaurant-partners" title="7. Restaurant partners">
                <p>
                    Restaurants that list on CraveDine are responsible for the accuracy of their information and
                    offers and for honouring valid redemptions. Partnership terms are set out in a separate
                    agreement. <ToConfirm>partner agreement and any fees</ToConfirm>.
                </p>
            </InfoSection>

            <InfoSection id="acceptable-use" title="8. Acceptable use">
                <p>When using CraveDine, you agree not to:</p>
                <ul>
                    <li>provide false information or use someone else's account;</li>
                    <li>misuse offers, for example by redeeming offers you do not intend to use;</li>
                    <li>interfere with, disrupt or try to gain unauthorised access to CraveDine;</li>
                    <li>use CraveDine for anything unlawful.</li>
                </ul>
            </InfoSection>

            <InfoSection id="content" title="9. Content and intellectual property">
                <p>
                    Restaurant names, photos, menus and offers are provided by restaurants. The CraveDine name, logo
                    and website belong to CraveDine. <ToConfirm>ownership and licensing of user and restaurant
                    content</ToConfirm>.
                </p>
            </InfoSection>

            <InfoSection id="liability" title="10. Liability">
                <p>
                    <ToConfirm>limitation of liability, warranties and disclaimers, to be drafted by a legal
                    adviser in line with the law of Nepal</ToConfirm>.
                </p>
            </InfoSection>

            <InfoSection id="changes" title="11. Changes to these terms">
                <p>
                    We may update these terms as CraveDine develops. The latest version will always be on this page.{" "}
                    <ToConfirm>how users will be told about significant changes</ToConfirm>.
                </p>
            </InfoSection>

            <InfoSection id="governing-law" title="12. Governing law and contact">
                <p>
                    <ToConfirm>governing law and jurisdiction</ToConfirm>. Questions about these terms? Please use
                    our <RouterLink to="/contact">Contact Us</RouterLink> page.
                </p>
            </InfoSection>

        </InfoPageLayout>

    );

}

export default TermsPage;
