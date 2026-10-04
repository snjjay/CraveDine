// ==========================================================
// Currency
// ==========================================================
//
// Single shared source for:
// - the list of currencies an Owner can choose for their restaurant
// - formatting a monetary amount using a restaurant's currency code
//
// Matches: EatKath.API.Constants.SupportedCurrencies
// ==========================================================

export interface CurrencyOption {
    code: string;
    label: string;
}

export const SUPPORTED_CURRENCIES: CurrencyOption[] = [
    { code: "NPR", label: "NPR — Nepalese Rupee" },
    { code: "AUD", label: "AUD — Australian Dollar" },
    { code: "USD", label: "USD — US Dollar" },
    { code: "INR", label: "INR — Indian Rupee" },
    { code: "GBP", label: "GBP — British Pound" },
    { code: "EUR", label: "EUR — Euro" }
];

export const DEFAULT_CURRENCY_CODE = "NPR";

// Currencies displayed without decimal places (common practical
// convention for whole-unit amounts), everything else uses the
// standard 2-decimal display.
const ZERO_DECIMAL_CURRENCIES = new Set(["NPR"]);

// Formats an amount using a restaurant's currency code, e.g.
// formatCurrency(250, "NPR")  -> "NPR 250"
// formatCurrency(25, "USD")   -> "USD 25.00"
// formatCurrency(25, "AUD")   -> "AUD 25.00"
//
// Gracefully handles a missing/invalid amount or currency code
// instead of throwing - callers display monetary values across
// several pages, and a formatting edge case must never crash them.
export function formatCurrency(
    amount: number | null | undefined,
    currencyCode: string | null | undefined
): string {

    if (amount === null || amount === undefined)
        return "";

    const code = currencyCode || DEFAULT_CURRENCY_CODE;

    const fractionDigits = ZERO_DECIMAL_CURRENCIES.has(code) ? 0 : 2;

    try {

        return new Intl.NumberFormat("en-US", {
            style: "currency",
            currency: code,
            currencyDisplay: "code",
            minimumFractionDigits: fractionDigits,
            maximumFractionDigits: fractionDigits
        }).format(amount);

    }
    catch {

        // Intl throws for a code it doesn't recognise - fall back to
        // a plain "<code> <amount>" rendering rather than crashing.
        return `${code} ${amount}`;

    }

}
