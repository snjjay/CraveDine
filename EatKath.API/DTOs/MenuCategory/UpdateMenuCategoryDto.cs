namespace EatKath.API.DTOs.MenuCategory;

public class UpdateMenuCategoryDto
{
    public string Name { get; set; } = string.Empty;

    public int DisplayOrder { get; set; }
}