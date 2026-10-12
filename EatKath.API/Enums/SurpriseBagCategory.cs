namespace EatKath.API.Enums
{
    // What kind of surplus food a Surprise Bag contains. Validated in the
    // API/service (not the database), so adding a category needs no migration.
    public enum SurpriseBagCategory
    {
        Meals = 1,
        Bakery = 2,
        Groceries = 3,
        Mixed = 4
    }
}
