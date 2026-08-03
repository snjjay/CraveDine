using AutoMapper;
using EatKath.API.Data;
using EatKath.API.DTOs.RestaurantImage;
using EatKath.API.Entities;
using EatKath.API.Interfaces;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace EatKath.API.Services
{
    public class RestaurantImageService : IRestaurantImageService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IValidator<CreateRestaurantImageDto> _createValidator;
        private readonly IValidator<UpdateRestaurantImageDto> _updateValidator;
        private readonly FileStorageService _fileStorage;

        public RestaurantImageService(
            ApplicationDbContext context,
            IMapper mapper,
            IValidator<CreateRestaurantImageDto> createValidator,
            IValidator<UpdateRestaurantImageDto> updateValidator,
            FileStorageService fileStorage)
        {
            _context = context;
            _mapper = mapper;
            _createValidator = createValidator;
            _updateValidator = updateValidator;
            _fileStorage = fileStorage;
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

            await _fileStorage.DeleteFileAsync(entity.ImageUrl);

            _context.RestaurantImages.Remove(entity);

            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<RestaurantImageDto> UploadAsync(int restaurantId, IFormFile file)
        {
            var restaurant = await _context.Restaurants.FindAsync(restaurantId);

            if (restaurant == null)
                throw new Exception("Restaurant not found.");

            var imagePath = await _fileStorage.SaveImageAsync(
                file,
                $"uploads/restaurants/{restaurantId}/gallery",
                Guid.NewGuid().ToString());

            var image = new RestaurantImage
            {
                RestaurantId = restaurantId,
                ImageUrl = imagePath,
                Caption = string.Empty,
                DisplayOrder = await _context.RestaurantImages
                    .CountAsync(x => x.RestaurantId == restaurantId) + 1,
                IsPrimary = false
            };

            _context.RestaurantImages.Add(image);

            await _context.SaveChangesAsync();

            return _mapper.Map<RestaurantImageDto>(image);
        }
    }
}