using AutoMapper;
using EatKath.API.Data;
using EatKath.API.DTOs.RestaurantImage;
using EatKath.API.Entities;
using EatKath.API.Interfaces;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace EatKath.API.Services
{
    public class RestaurantImageService : IRestaurantImageService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IValidator<CreateRestaurantImageDto> _createValidator;
        private readonly IValidator<UpdateRestaurantImageDto> _updateValidator;

        public RestaurantImageService(
            ApplicationDbContext context,
            IMapper mapper,
            IValidator<CreateRestaurantImageDto> createValidator,
            IValidator<UpdateRestaurantImageDto> updateValidator)
        {
            _context = context;
            _mapper = mapper;
            _createValidator = createValidator;
            _updateValidator = updateValidator;
        }

        public async Task<IEnumerable<RestaurantImageDto>> GetAllAsync()
        {
            var images = await _context.RestaurantImages
                .OrderBy(x => x.DisplayOrder)
                .ToListAsync();

            return _mapper.Map<IEnumerable<RestaurantImageDto>>(images);
        }

        public async Task<RestaurantImageDto?> GetByIdAsync(int id)
        {
            var image = await _context.RestaurantImages.FindAsync(id);

            if (image == null)
                return null;

            return _mapper.Map<RestaurantImageDto>(image);
        }

        public async Task<IEnumerable<RestaurantImageDto>> GetByRestaurantAsync(int restaurantId)
        {
            var images = await _context.RestaurantImages
                .Where(x => x.RestaurantId == restaurantId)
                .OrderBy(x => x.DisplayOrder)
                .ToListAsync();

            return _mapper.Map<IEnumerable<RestaurantImageDto>>(images);
        }

        public async Task<RestaurantImageDto> CreateAsync(CreateRestaurantImageDto dto)
        {
            var validation = await _createValidator.ValidateAsync(dto);

            if (!validation.IsValid)
                throw new ValidationException(validation.Errors);

            var restaurantExists = await _context.Restaurants
                .AnyAsync(x => x.Id == dto.RestaurantId);

            if (!restaurantExists)
                throw new Exception("Restaurant not found.");

            var entity = _mapper.Map<RestaurantImage>(dto);

            _context.RestaurantImages.Add(entity);

            await _context.SaveChangesAsync();

            return _mapper.Map<RestaurantImageDto>(entity);
        }

        public async Task<RestaurantImageDto?> UpdateAsync(int id, UpdateRestaurantImageDto dto)
        {
            var validation = await _updateValidator.ValidateAsync(dto);

            if (!validation.IsValid)
                throw new ValidationException(validation.Errors);

            var entity = await _context.RestaurantImages.FindAsync(id);

            if (entity == null)
                return null;

            _mapper.Map(dto, entity);

            await _context.SaveChangesAsync();

            return _mapper.Map<RestaurantImageDto>(entity);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var entity = await _context.RestaurantImages.FindAsync(id);

            if (entity == null)
                return false;

            _context.RestaurantImages.Remove(entity);

            await _context.SaveChangesAsync();

            return true;
        }
    }
}