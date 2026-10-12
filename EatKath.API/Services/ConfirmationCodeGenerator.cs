using EatKath.API.Interfaces;
using System.Security.Cryptography;

namespace EatKath.API.Services
{
    // Eight cryptographically random characters from an alphabet without
    // easily confused characters (no 0/O, 1/I/L): about 8.5 x 10^11 codes,
    // within the database's 12-character limit. Uniqueness is checked by
    // SurpriseBagReservationService and enforced by a unique index.
    public sealed class ConfirmationCodeGenerator : IConfirmationCodeGenerator
    {
        public const string Alphabet = "ABCDEFGHJKMNPQRSTUVWXYZ23456789";

        public const int Length = 8;

        public string Generate() => RandomNumberGenerator.GetString(Alphabet, Length);

        // Codes are stored upper-case; staff may type them in any case.
        public static string Normalize(string? code) => (code ?? string.Empty).Trim().ToUpperInvariant();
    }
}
