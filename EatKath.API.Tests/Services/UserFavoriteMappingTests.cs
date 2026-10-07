using EatKath.API.DTOs.UserFavorite;
using EatKath.API.Entities;
using EatKath.API.Tests.Helpers;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EatKath.API.Tests.Services;

[TestClass]
public class UserFavoriteMappingTests
{
    [TestMethod]
    public void Map_ShouldIncludeRestaurantNameLogoAndCover()
    {
        var mapper = MapperFactory.Create();

        var favorite = new UserFavorite
        {
            UserId = 7,
            RestaurantId = 3,
            Restaurant = new Restaurant
            {
                Id = 3,
                Name = "Himal Momo Kitchen",
                LogoUrl = string.Empty,
                CoverImageUrl = "/uploads/demo/momo-01.jpg"
            }
        };

        var dto = mapper.Map<UserFavoriteDto>(favorite);

        dto.RestaurantName.Should().Be("Himal Momo Kitchen");
        dto.LogoUrl.Should().BeEmpty();
        dto.CoverImageUrl.Should().Be("/uploads/demo/momo-01.jpg");
    }

    [TestMethod]
    public void Map_ShouldLeaveCoverNull_WhenRestaurantHasNoCover()
    {
        var mapper = MapperFactory.Create();

        var favorite = new UserFavorite
        {
            Restaurant = new Restaurant { Name = "Bread Basket", CoverImageUrl = null }
        };

        mapper.Map<UserFavoriteDto>(favorite).CoverImageUrl.Should().BeNull();
    }
}
