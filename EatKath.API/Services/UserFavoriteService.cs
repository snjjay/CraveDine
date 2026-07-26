using AutoMapper;
using EatKath.API.Data;
using EatKath.API.DTOs.UserFavorite;
using EatKath.API.Entities;
using EatKath.API.Interfaces;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace EatKath.API.Services
{
    public class UserFavoriteService : IUserFavoriteService
    {
        private readonly ApplicationDbContext _context;
        private readonly ICurrentUserService _currentUser;
        private readonly IMapper _mapper;
        private readonly IValidator<CreateUserFavoriteDto> _createValidator;
        private readonly IValidator<RemoveUserFavoriteDto> _removeValidator;

        public UserFavoriteService(
            ApplicationDbContext context,
            ICurrentUserService currentUser,
            IMapper mapper,
            IValidator<CreateUserFavoriteDto> createValidator,
            IValidator<RemoveUserFavoriteDto> removeValidator)
        {
            _context = context;
            _currentUser = currentUser;
            _mapper = mapper;
            _createValidator = createValidator;
            _removeValidator = removeValidator;
        }

        public async Task<IEnumerable<UserFavoriteDto>> GetMyFavoritesAsync()
        {
            var userId = _currentUser.UserId;

            var favorites = await _context.UserFavorites
                .Include(x => x.Restaurant)
                .Where(x => x.UserId == userId)
                .ToListAsync();

            return _mapper.Map<IEnumerable<UserFavoriteDto>>(favorites);
        }

        public async Task AddAsync(CreateUserFavoriteDto dto)
        {
            var validation = await _createValidator.ValidateAsync(dto);

            if (!validation.IsValid)
                throw new ValidationException(validation.Errors);

            var userId = _currentUser.UserId;

            var exists = await _context.UserFavorites.AnyAsync(x =>
                x.UserId == userId &&
                x.RestaurantId == dto.RestaurantId);

            if (exists)
                throw new Exception("Restaurant already added to favourites.");

            _context.UserFavorites.Add(new UserFavorite
            {
                UserId = userId,
                RestaurantId = dto.RestaurantId,
                CreatedAt = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();
        }

        public async Task RemoveAsync(RemoveUserFavoriteDto dto)
        {
            var validation = await _removeValidator.ValidateAsync(dto);

            if (!validation.IsValid)
                throw new ValidationException(validation.Errors);

            var userId = _currentUser.UserId;

            var favourite = await _context.UserFavorites.FirstOrDefaultAsync(x =>
                x.UserId == userId &&
                x.RestaurantId == dto.RestaurantId);

            if (favourite == null)
                return;

            _context.UserFavorites.Remove(favourite);

            await _context.SaveChangesAsync();
        }
    }
}