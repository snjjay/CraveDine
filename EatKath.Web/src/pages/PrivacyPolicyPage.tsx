import { Link as RouterLink } from "react-router-dom";

import InfoPageLayout from "../components/info/InfoPageLayout";
import InfoSection from "../components/info/InfoSection";
import ToConfirm from "../components/info/ToConfirm";

function PrivacyPolicyPage() {

    return (

        <InfoPageLayout
            eyebrow="Legal"
            title="Privacy Policy"
            intro={<>This policy explains what information CraveDine collects, how it is used and the choices you have. Last updated: <ToConfirm>publication date</ToConfirm>.</>}
            metaTitle="Privacy Policy"
            metaDescription="How CraveDine collects, uses and protects personal information when you discover restaurants and redeem offers in Kathmandu."
            draft
        >

            <InfoSection id="who-we-are" title="1. Who we are">
                <p>
                    CraveDine is a restaurant discovery and food-offer platform for Kathmandu, Nepal. In this policy,
                    "CraveDine", "we" and "us" refer to <ToConfirm>registered legal entity name, registration number
                    and registered address</ToConfirm>.
                </p>
            </InfoSection>

            <InfoSection id="information-we-collect" title="2. Information we collect">
                <p><strong>When you create a customer account:</strong></p>
                <ul>
                    <li>your first and last name;</li>
                    <li>your email address;</li>
                    <li>your phone number, if you choose to provide it;</li>
                    <li>your password, which we store only in a protected (hashed) form, never as plain text.</li>
                </ul>
                <p><strong>When you use CraveDine:</strong></p>
                <ul>
                    <li>
                        offers you redeem, including the restaurant, arrival date and time, number of guests and the
                        redemption's status;
                    </li>
                    <li>
                        when a restaurant completes your redemption: the bill amount it records, and the resulting
                        discount and final amount;
                    </li>
                    <li>restaurants you save as favourites.</li>
                </ul>
                <p><strong>From restaurant partners:</strong></p>
                <ul>
                    <li>
                        account details of the restaurant owner (name, email, phone) and restaurant information such
                        as name, address, area, cuisines, contact details, opening hours, menus, photos and offers.
                    </li>
                </ul>
                <p><strong>Technical information:</strong></p>
                <ul>
                    <li>
                        when you sign in, your browser stores a sign-in token and basic account details (name, email
                        and account type) in its local storage, so you stay signed in;
                    </li>
                    <li>our servers may record error logs when something goes wrong, to help us fix problems.</li>
                </ul>
            </InfoSection>

            <InfoSection id="how-we-use" title="3. How we use your information">
                <ul>
                    <li>to create and manage your account and keep you signed in;</li>
                    <li>to let you redeem offers and show your redemption history;</li>
                    <li>to share the details a restaurant needs to honour your redemption (see section 4);</li>
                    <li>to show your favourite restaurants;</li>
                    <li>to keep CraveDine secure and to investigate and fix problems;</li>
                    <li>to respond when you contact us.</li>
                </ul>
                <p>
                    We do not currently use advertising cookies or third-party analytics. <ToConfirm>whether any
                    marketing communications or analytics will be introduced before launch</ToConfirm>.
                </p>
            </InfoSection>

            <InfoSection id="sharing" title="4. When we share information">
                <ul>
                    <li>
                        <strong>With the restaurant you redeem an offer at.</strong> The restaurant can see your
                        name, email address, phone number (if provided), arrival date and time and number of guests,
                        so it can recognise you and apply your discount.
                    </li>
                    <li>
                        <strong>With service providers</strong> that help us run CraveDine, such as cloud hosting.{" "}
                        <ToConfirm>names of hosting and other service providers</ToConfirm>.
                    </li>
                    <li>
                        <strong>Fonts.</strong> Our website loads fonts from Google Fonts, which means your browser
                        connects to Google's servers and shares information such as your IP address with Google.
                    </li>
                    <li>
                        <strong>Where required by law</strong>, or to protect the rights, safety and security of our
                        users, partners or CraveDine.
                    </li>
                </ul>
                <p>We do not sell your personal information.</p>
            </InfoSection>

            <InfoSection id="storage-and-security" title="5. Where information is stored and how it is protected">
                <p>
                    CraveDine's data is stored with a cloud hosting provider. <ToConfirm>the country or region
                    where data is stored, and any safeguards for storage outside Nepal</ToConfirm>.
                </p>
                <p>
                    We use security measures such as hashed passwords and encrypted (HTTPS) connections to our
                    service. No system is completely secure, so please keep your password private.
                </p>
            </InfoSection>

            <InfoSection id="retention" title="6. How long we keep information">
                <p>
                    We keep your information for as long as your account is active and as needed to provide
                    CraveDine and meet legal obligations. <ToConfirm>specific retention periods for accounts,
                    redemption records and logs</ToConfirm>.
                </p>
            </InfoSection>

            <InfoSection id="your-choices" title="7. Your choices and requests">
                <p>
                    You can view your account details on your Profile page and your redemption history in My
                    Redemptions. Editing your details or deleting your account is not yet available in the app;
                    please <RouterLink to="/contact">contact us</RouterLink> to ask us to correct or delete your
                    information. <ToConfirm>the request process, response times and any rights that apply under
                    the law of Nepal</ToConfirm>.
                </p>
            </InfoSection>

            <InfoSection id="children" title="8. Children">
                <p>
                    CraveDine is not intended for children. <ToConfirm>minimum age for creating an account</ToConfirm>.
                </p>
            </InfoSection>

            <InfoSection id="changes" title="9. Changes to this policy">
                <p>
                    We may update this policy as CraveDine develops. We will post the updated version on this page
                    with a new "last updated" date. <ToConfirm>how users will be told about significant
                    changes</ToConfirm>.
                </p>
            </InfoSection>

            <InfoSection id="contact" title="10. Contact">
                <p>
                    Questions about this policy or your information? Please use our{" "}
                    <RouterLink to="/contact">Contact Us</RouterLink> page. <ToConfirm>privacy contact email and
                    postal address</ToConfirm>.
                </p>
            </InfoSection>

        </InfoPageLayout>

    );

}

export default PrivacyPolicyPage;
