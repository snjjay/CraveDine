namespace EatKath.API.DTOs.MenuCategory;

public class CreateMenuCategoryDto
{
    public int RestaurantId { get; set; }

    public string Name { get; set; } = string.Empty;

    public int DisplayOrder { get; set; }
}