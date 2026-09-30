using AutoMapper;
using EatKath.API.Data;
using EatKath.API.DTOs.RestaurantOpeningHour;
using EatKath.API.Entities;
using EatKath.API.Interfaces;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace EatKath.API.Services
{
    public class RestaurantOpeningHourService : IRestaurantOpeningHourService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IValidator<CreateRestaurantOpeningHourDto> _createValidator;
        private readonly IValidator<UpdateRestaurantOpeningHourDto> _updateValidator;
        private readonly ICurrentUserService _currentUser;

        public RestaurantOpeningHourService(
            ApplicationDbContext context,
            IMapper mapper,
            IValidator<CreateRestaurantOpeningHourDto> createValidator,
            IValidator<UpdateRestaurantOpeningHourDto> updateValidator,
            ICurrentUserService currentUser)
        {
            _context = context;
            _mapper = mapper;
            _createValidator = createValidator;
            _updateValidator = updateValidator;
            _currentUser = currentUser;
        }

        public async Task<IEnumerable<RestaurantOpeningHourDto>> GetAllAsync()
        {
            var hours = await _context.RestaurantOpeningHours
                .OrderBy(x => x.DayOfWeek)
                .ToListAsync();

            return _mapper.Map<IEnumerable<RestaurantOpeningHourDto>>(hours);
        }

        public async Task<RestaurantOpeningHourDto?> GetByIdAsync(int id)
        {
            var entity = await _context.RestaurantOpeningHours.FindAsync(id);

            if (entity == null)
                return null;

            return _mapper.Map<RestaurantOpeningHourDto>(entity);
        }

        public async Task<IEnumerable<RestaurantOpeningHourDto>> GetByRestaurantAsync(int restaurantId)
        {
            var hours = await _context.RestaurantOpeningHours
                .Where(x => x.RestaurantId == restaurantId)
                .OrderBy(x => x.DayOfWeek)
                .ToListAsync();

            return _mapper.Map<IEnumerable<RestaurantOpeningHourDto>>(hours);
        }

        public async Task<RestaurantOpeningHourDto> CreateAsync(CreateRestaurantOpeningHourDto dto)
        {
            var validation = await _createValidator.ValidateAsync(dto);

            if (!validation.IsValid)
                throw new ValidationException(validation.Errors);

            var restaurant = await _context.Restaurants
                .FirstOrDefaultAsync(x => x.Id == dto.RestaurantId);

            if (restaurant == null)
                throw new NotFoundException("Restaurant not found.");

            if (!_currentUser.IsAdmin &&
                restaurant.OwnerId != _currentUser.UserId)
            {
                throw new BusinessRuleException("You are not authorized to add opening hours for this restaurant.");
            }

            var entity = _mapper.Map<RestaurantOpeningHour>(dto);

            _context.RestaurantOpeningHours.Add(entity);

            await _context.SaveChangesAsync();

            return _mapper.Map<RestaurantOpeningHourDto>(entity);
        }

        public async Task<RestaurantOpeningHourDto?> UpdateAsync(int id, UpdateRestaurantOpeningHourDto dto)
        {
            var validation = await _updateValidator.ValidateAsync(dto);

            if (!validation.IsValid)
                throw new ValidationException(validation.Errors);

            var entity = await _context.RestaurantOpeningHours
                .Include(x => x.Restaurant)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (entity == null)
                return null;

            if (!_currentUser.IsAdmin &&
                entity.Restaurant.OwnerId != _currentUser.UserId)
            {
                throw new BusinessRuleException("You are not authorized to modify this restaurant's opening hours.");
            }

            _mapper.Map(dto, entity);

            await _context.SaveChangesAsync();

            return _mapper.Map<RestaurantOpeningHourDto>(entity);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var entity = await _context.RestaurantOpeningHours
                .Include(x => x.Restaurant)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (entity == null)
                return false;

            if (!_currentUser.IsAdmin &&
                entity.Restaurant.OwnerId != _currentUser.UserId)
            {
                throw new BusinessRuleException("You are not authorized to delete this restaurant's opening hours.");
            }

            _context.RestaurantOpeningHours.Remove(entity);

            await _context.SaveChangesAsync();

            return true;
        }
    }
}