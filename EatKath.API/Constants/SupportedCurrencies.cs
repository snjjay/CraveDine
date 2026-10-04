namespace EatKath.API.Constants
{
    // Single source of truth for which currency codes a Restaurant
    // may be set to. Kept deliberately small/static rather than a
    // database table, since the task only requires validating
    // against a fixed initial list - not a fully dynamic currency
    // management feature.
    public static class SupportedCurrencies
    {
        public const string Default = "NPR";

        public static readonly HashSet<string> Codes = new(StringComparer.OrdinalIgnoreCase)
        {
            "NPR",
            "AUD",
            "USD",
            "INR",
            "GBP",
            "EUR"
        };
    }
}
