namespace EatKath.API.Interfaces
{
    // Creates Surprise Bag reservation confirmation codes (replaceable in
    // tests to force collisions).
    public interface IConfirmationCodeGenerator
    {
        string Generate();
    }
}
