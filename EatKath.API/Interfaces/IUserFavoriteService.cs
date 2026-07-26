using EatKath.API.DTOs.UserFavorite;

namespace EatKath.API.Interfaces
{
    public interface IUserFavoriteService
    {
        Task<IEnumerable<UserFavoriteDto>> GetMyFavoritesAsync();

        Task AddAsync(CreateUserFavoriteDto dto);

        Task RemoveAsync(RemoveUserFavoriteDto dto);
    }
}