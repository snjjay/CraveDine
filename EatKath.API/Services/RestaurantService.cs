using AutoMapper;
using AutoMapper.QueryableExtensions;
using EatKath.API.Data;
using EatKath.API.DTOs.Restaurant;
using EatKath.API.Entities;
using EatKath.API.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace EatKath.API.Services
{
    public class RestaurantService : IRestaurantService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly FileStorageService _fileStorage;

        public RestaurantService(
            ApplicationDbContext context,
            IMapper mapper,
            FileStorageService fileStorage)
        {
            _context = context;
            _mapper = mapper;
            _fileStorage = fileStorage;
        }

        public async Task<IEnumerable<RestaurantDto>> GetAllAsync()
        {
            return await _context.Restaurants
                .Include(r => r.Area)
                .ProjectTo<RestaurantDto>(_mapper.ConfigurationProvider)
                .ToListAsync();
        }

        public async Task<RestaurantDto?> GetByIdAsync(int id)
        {
            var restaurant = await _context.Restaurants
                .Include(r => r.Area)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (restaurant == null)
                return null;

            return _mapper.Map<RestaurantDto>(restaurant);
        }

        public async Task<RestaurantDto> CreateAsync(CreateRestaurantDto dto)
        {
            var restaurant = _mapper.Map<Restaurant>(dto);

            _context.Restaurants.Add(restaurant);

            await _context.SaveChangesAsync();

            await _context.Entry(restaurant)
                .Reference(r => r.Area)
                .LoadAsync();

            return _mapper.Map<RestaurantDto>(restaurant);
        }

        public async Task<RestaurantDto?> UpdateAsync(int id, UpdateRestaurantDto dto)
        {
            var restaurant = await _context.Restaurants
                .Include(r => r.Area)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (restaurant == null)
                return null;

            _mapper.Map(dto, restaurant);

            await _context.SaveChangesAsync();

            await _context.Entry(restaurant)
                .Reference(r => r.Area)
                .LoadAsync();

            return _mapper.Map<RestaurantDto>(restaurant);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var restaurant = await _context.Restaurants.FindAsync(id);

            if (restaurant == null)
                return false;

            _context.Restaurants.Remove(restaurant);

            await _context.SaveChangesAsync();

            return true;
        }

        // ============================
        // Upload Logo
        // ============================

        public async Task<string> UploadLogoAsync(int restaurantId, IFormFile file)
        {
            var restaurant = await _context.Restaurants.FindAsync(restaurantId);

            if (restaurant == null)
                throw new Exception("Restaurant not found.");

            var path = await _fileStorage.SaveImageAsync(
                file,
                $"uploads/restaurants/{restaurantId}",
                "logo");

            restaurant.LogoUrl = path;

            await _context.SaveChangesAsync();

            return path;
        }

        // ============================
        // Upload Cover
        // ============================

        public async Task<string> UploadCoverAsync(int restaurantId, IFormFile file)
        {
            var restaurant = await _context.Restaurants.FindAsync(restaurantId);

            if (restaurant == null)
                throw new Exception("Restaurant not found.");

            var path = await _fileStorage.SaveImageAsync(
                file,
                $"uploads/restaurants/{restaurantId}",
                "cover");

            restaurant.CoverImageUrl = path;

            await _context.SaveChangesAsync();

            return path;
        }

        // ============================
        // Upload Menu PDF
        // ============================

        public async Task<string> UploadMenuPdfAsync(int restaurantId, IFormFile file)
        {
            var restaurant = await _context.Restaurants.FindAsync(restaurantId);

            if (restaurant == null)
                throw new Exception("Restaurant not found.");

            var path = await _fileStorage.SavePdfAsync(
                file,
                $"uploads/restaurants/{restaurantId}",
                "menu");

            restaurant.MenuPdfUrl = path;

            await _context.SaveChangesAsync();

            return path;
        }


        public async Task DeleteLogoAsync(int restaurantId)
        {
            var restaurant = await _context.Restaurants.FindAsync(restaurantId);

            if (restaurant == null)
                throw new Exception("Restaurant not found.");

            await _fileStorage.DeleteFileAsync(restaurant.LogoUrl);

            restaurant.LogoUrl = string.Empty;

            await _context.SaveChangesAsync();
        }



        public async Task DeleteCoverAsync(int restaurantId)
        {
            var restaurant = await _context.Restaurants.FindAsync(restaurantId);

            if (restaurant == null)
                throw new Exception("Restaurant not found.");

            await _fileStorage.DeleteFileAsync(restaurant.CoverImageUrl);

            restaurant.CoverImageUrl = string.Empty;

            await _context.SaveChangesAsync();
        }


        public async Task DeleteMenuPdfAsync(int restaurantId)
        {
            var restaurant = await _context.Restaurants.FindAsync(restaurantId);

            if (restaurant == null)
                throw new Exception("Restaurant not found.");

            await _fileStorage.DeleteFileAsync(restaurant.MenuPdfUrl);

            restaurant.MenuPdfUrl = string.Empty;

            await _context.SaveChangesAsync();
        }

    }
}