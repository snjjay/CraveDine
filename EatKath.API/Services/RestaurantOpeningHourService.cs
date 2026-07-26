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

        public RestaurantOpeningHourService(
            ApplicationDbContext context,
            IMapper mapper,
            IValidator<CreateRestaurantOpeningHourDto> createValidator,
            IValidator<UpdateRestaurantOpeningHourDto> updateValidator)
        {
            _context = context;
            _mapper = mapper;
            _createValidator = createValidator;
            _updateValidator = updateValidator;
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

            var restaurantExists = await _context.Restaurants
                .AnyAsync(x => x.Id == dto.RestaurantId);

            if (!restaurantExists)
                throw new Exception("Restaurant not found.");

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

            var entity = await _context.RestaurantOpeningHours.FindAsync(id);

            if (entity == null)
                return null;

            _mapper.Map(dto, entity);

            await _context.SaveChangesAsync();

            return _mapper.Map<RestaurantOpeningHourDto>(entity);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var entity = await _context.RestaurantOpeningHours.FindAsync(id);

            if (entity == null)
                return false;

            _context.RestaurantOpeningHours.Remove(entity);

            await _context.SaveChangesAsync();

            return true;
        }
    }
}