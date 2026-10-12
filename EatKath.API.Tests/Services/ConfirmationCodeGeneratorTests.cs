using EatKath.API.Services;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EatKath.API.Tests.Services;

[TestClass]
public class ConfirmationCodeGeneratorTests
{
    private readonly ConfirmationCodeGenerator _generator = new();

    [TestMethod]
    public void Generate_ShouldReturnEightCharacters_FromTheUnambiguousAlphabet()
    {
        for (var i = 0; i < 1000; i++)
        {
            var code = _generator.Generate();

            code.Should().HaveLength(ConfirmationCodeGenerator.Length);
            code.Should().MatchRegex("^[ABCDEFGHJKMNPQRSTUVWXYZ23456789]{8}$");
        }
    }

    [TestMethod]
    public void Alphabet_ShouldExcludeEasilyConfusedCharacters_AndFitTheDatabaseColumn()
    {
        ConfirmationCodeGenerator.Alphabet.Should().NotContainAny("0", "O", "1", "I", "L");
        ConfirmationCodeGenerator.Alphabet.Distinct().Should().HaveCount(31);
        ConfirmationCodeGenerator.Length.Should().BeLessThanOrEqualTo(12);
    }

    [TestMethod]
    public void Generate_ShouldNotRepeat_AcrossManyCodes()
    {
        var codes = Enumerable.Range(0, 20_000).Select(_ => _generator.Generate()).ToList();

        codes.Should().OnlyHaveUniqueItems();
    }

    [TestMethod]
    [DataRow(" k7m2q9xa ", "K7M2Q9XA")]
    [DataRow("K7M2Q9XA", "K7M2Q9XA")]
    [DataRow(null, "")]
    public void Normalize_ShouldTrimAndUpperCase(string? input, string expected)
    {
        ConfirmationCodeGenerator.Normalize(input).Should().Be(expected);
    }
}
