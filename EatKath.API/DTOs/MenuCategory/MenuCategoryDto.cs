namespace EatKath.API.DTOs.MenuCategory;

public class MenuCategoryDto
{
    public int Id { get; set; }

    public int RestaurantId { get; set; }

    public string Name { get; set; } = string.Empty;

    public int DisplayOrder { get; set; }
}