using EatKath.API.DTOs.Restaurant;
using EatKath.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EatKath.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RestaurantController : ControllerBase
    {
        private readonly IRestaurantService _restaurantService;

        public RestaurantController(IRestaurantService restaurantService)
        {
            _restaurantService = restaurantService;
        }

        // Public - Anyone can browse restaurants
        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var restaurants = await _restaurantService.GetAllAsync();
            return Ok(restaurants);
        }

        // Public - Anyone can view a restaurant
        [AllowAnonymous]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var restaurant = await _restaurantService.GetByIdAsync(id);

            if (restaurant == null)
                return NotFound();

            return Ok(restaurant);
        }

        // Admin or Owner can create a restaurant
        [Authorize(Roles = "Admin,Owner")]
        [HttpPost]
        public async Task<IActionResult> Create(CreateRestaurantDto dto)
        {
            var restaurant = await _restaurantService.CreateAsync(dto);

            return CreatedAtAction(
                nameof(GetById),
                new { id = restaurant.Id },
                restaurant);
        }

        // Admin or Owner can update
        [Authorize(Roles = "Admin,Owner")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, UpdateRestaurantDto dto)
        {
            var restaurant = await _restaurantService.UpdateAsync(id, dto);

            if (restaurant == null)
                return NotFound();

            return Ok(restaurant);
        }

        // Only Admin can delete restaurants
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _restaurantService.DeleteAsync(id);

            if (!deleted)
                return NotFound();

            return NoContent();
        }
    }
}